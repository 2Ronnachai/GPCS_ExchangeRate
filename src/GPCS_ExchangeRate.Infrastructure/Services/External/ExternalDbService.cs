using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GPCS_ExchangeRate.Infrastructure.Services.External
{
    public class ExternalDbService(
    IConfiguration config,
    ILogger<ExternalDbService> logger)
    : IExternalDbService
    {
        private readonly string _connString =
            config.GetConnectionString("ExternalDb")!;

        public async Task SyncExchangeRateAsync(
            List<ExchangeRatePayloadDto> payloads,
            CancellationToken cancellationToken)
        {
            using var conn = new SqlConnection(_connString);
            await conn.OpenAsync(cancellationToken);

            using var transaction = conn.BeginTransaction();

            try
            {
                foreach (var payload in payloads)
                {
                    using var cmd = new SqlCommand(
                        "sp_SyncExchangeRate", conn, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@CurrencyCode", payload.CurrencyCode);
                    cmd.Parameters.AddWithValue("@Period", payload.Period);
                    cmd.Parameters.AddWithValue("@Rate", (double)payload.Rate);
                    cmd.Parameters.AddWithValue("@Rate2", (double)payload.Rate2);
                    cmd.Parameters.AddWithValue("@AppUserID", payload.AppUserID);
                    cmd.Parameters.AddWithValue("@AppDate", payload.AppDate);
                    cmd.Parameters.AddWithValue("@UpdUserID", (object?)payload.UpdUserID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdDate", (object?)payload.UpdDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdPGM", (object?)payload.UpdPGM ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync(cancellationToken);

                    logger.LogInformation(
                        "Synced CurrencyCode: {CurrencyCode}, Period: {Period}",
                        payload.CurrencyCode, payload.Period);
                }

                await transaction.CommitAsync(cancellationToken);

                logger.LogInformation(
                    "Sync completed. Total {Count} currencies synced.",
                    payloads.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogError(ex,
                    "Failed to sync ExchangeRate to external DB. Rolling back.");
                throw;
            }
        }
    }
}
