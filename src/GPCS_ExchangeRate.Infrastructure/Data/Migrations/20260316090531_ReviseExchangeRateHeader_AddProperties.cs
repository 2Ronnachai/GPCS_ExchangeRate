using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPCS_ExchangeRate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviseExchangeRateHeader_AddProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ExchangeRateHeaders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentStatus",
                table: "ExchangeRateHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ExchangeRateHeaders");

            migrationBuilder.DropColumn(
                name: "DocumentStatus",
                table: "ExchangeRateHeaders");
        }
    }
}
