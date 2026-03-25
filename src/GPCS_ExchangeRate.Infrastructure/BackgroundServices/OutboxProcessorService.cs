using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GPCS_ExchangeRate.Infrastructure.BackgroundServices
{
    public class OutboxProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorService> logger)
        : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Outbox Processor Service started.");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Outbox Processor unexpected error.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            logger.LogInformation("Outbox Processor Service is stopping.");
        }

        private async Task ProcessOutboxAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            var externalDbService = scope.ServiceProvider
                .GetRequiredService<IExternalDbService>();

            var pendingEvents = await unitOfWork.ExchangeRateOutBoxEvents
                .GetUnprocessedAsync(cancellationToken);

            if (pendingEvents.Count == 0)
            {
                logger.LogDebug("No pending outbox events found.");
                return;
            }

            logger.LogInformation(
                "Found {Count} pending outbox events.",
                pendingEvents.Count);

            foreach (var outboxEvent in pendingEvents)
            {
                await ProcessEventAsync(
                    outboxEvent, 
                    unitOfWork, 
                    externalDbService,
                    cancellationToken);
            }
        }

        private async Task ProcessEventAsync(
            ExchangeRateOutBoxEvents outboxEvent,
            IUnitOfWork unitOfWork,
            IExternalDbService externalDbService,
            CancellationToken cancellationToken)
        {
            outboxEvent.Status = OutBoxStatus.Processing;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                logger.LogInformation(
                    "Processing outbox event {Id}, EventType: {EventType}",
                    outboxEvent.Id, outboxEvent.EventType);

                // Deserialize Payload
                var payloads = JsonSerializer.Deserialize<List<ExchangeRatePayloadDto>>(
                    outboxEvent.Payload)
                    ?? throw new InvalidOperationException(
                        $"Payload is null for outbox event {outboxEvent.Id}.");

                await externalDbService.SyncExchangeRateAsync(payloads, cancellationToken);

                outboxEvent.Status = OutBoxStatus.Completed;
                outboxEvent.ErrorMessage = null;

                logger.LogInformation(
                    "Outbox event {Id} completed successfully.",
                    outboxEvent.Id);
            }
            catch (Exception ex)
            {
                outboxEvent.RetryCount++;
                outboxEvent.ErrorMessage = ex.Message;

                outboxEvent.Status = outboxEvent.RetryCount >= outboxEvent.MaxRetryAttempts
                    ? OutBoxStatus.Failed
                    : OutBoxStatus.Pending; 

                logger.LogError(ex,
                    "Outbox event {Id} failed. RetryCount: {RetryCount}/{MaxRetry}",
                    outboxEvent.Id, outboxEvent.RetryCount, outboxEvent.MaxRetryAttempts);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
