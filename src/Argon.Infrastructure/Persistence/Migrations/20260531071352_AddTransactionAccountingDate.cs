using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionAccountingDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AccountingDate",
                table: "Transactions",
                type: "date",
                nullable: true);

            // Backfill the accounting date for parsed transactions from the jsonb raw import
            // data (the value is already stored there). Manual entries have no RawImportData
            // and keep a null AccountingDate, falling back to Date at query time.
            migrationBuilder.Sql(
                """
                UPDATE "Transactions"
                SET "AccountingDate" = ("RawImportData" ->> 'AccountingDate')::date
                WHERE "AccountingDate" IS NULL
                  AND "RawImportData" IS NOT NULL
                  AND jsonb_typeof("RawImportData") = 'object'
                  AND jsonb_exists("RawImportData", 'AccountingDate');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountingDate",
                table: "Transactions");
        }
    }
}
