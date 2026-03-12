using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPCS_ExchangeRate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeRateHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Period = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateHeaders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRateDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExchangeRateHeaderId = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Rate2Digit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate4Digit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRateDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRateDetails_ExchangeRateHeaders_ExchangeRateHeaderId",
                        column: x => x.ExchangeRateHeaderId,
                        principalTable: "ExchangeRateHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UIX_ExchangeRateDetail_HeaderId_CurrencyCode",
                table: "ExchangeRateDetails",
                columns: new[] { "ExchangeRateHeaderId", "CurrencyCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHeader_DocumentNumber",
                table: "ExchangeRateHeaders",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRateHeader_Period",
                table: "ExchangeRateHeaders",
                column: "Period");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRateDetails");

            migrationBuilder.DropTable(
                name: "ExchangeRateHeaders");
        }
    }
}
