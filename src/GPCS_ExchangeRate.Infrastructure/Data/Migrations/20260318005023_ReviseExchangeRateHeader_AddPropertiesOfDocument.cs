using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPCS_ExchangeRate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviseExchangeRateHeader_AddPropertiesOfDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "ExchangeRateHeaders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUrgent",
                table: "ExchangeRateHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "ExchangeRateHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "ExchangeRateHeaders");

            migrationBuilder.DropColumn(
                name: "IsUrgent",
                table: "ExchangeRateHeaders");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "ExchangeRateHeaders");
        }
    }
}
