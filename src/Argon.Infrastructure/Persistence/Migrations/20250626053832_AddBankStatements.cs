using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Argon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBankStatements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Counterparties_CounterpartyId",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CounterpartyId",
                table: "Transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BankStatementId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PotentialDuplicateOfTransactionId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawImportData",
                table: "Transactions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "TransactionRows",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "BankStatements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FileContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    ImportedToAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankStatements_Accounts_ImportedToAccountId",
                        column: x => x.ImportedToAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterpartyIdentifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CounterpartyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentifierText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Created = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterpartyIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterpartyIdentifiers_Counterparties_CounterpartyId",
                        column: x => x.CounterpartyId,
                        principalTable: "Counterparties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BankStatementId",
                table: "Transactions",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PotentialDuplicateOfTransactionId",
                table: "Transactions",
                column: "PotentialDuplicateOfTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_ImportedToAccountId",
                table: "BankStatements",
                column: "ImportedToAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterpartyIdentifiers_CounterpartyId",
                table: "CounterpartyIdentifiers",
                column: "CounterpartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BankStatements_BankStatementId",
                table: "Transactions",
                column: "BankStatementId",
                principalTable: "BankStatements",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Counterparties_CounterpartyId",
                table: "Transactions",
                column: "CounterpartyId",
                principalTable: "Counterparties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Transactions_PotentialDuplicateOfTransactionId",
                table: "Transactions",
                column: "PotentialDuplicateOfTransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BankStatements_BankStatementId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Counterparties_CounterpartyId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Transactions_PotentialDuplicateOfTransactionId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "BankStatements");

            migrationBuilder.DropTable(
                name: "CounterpartyIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BankStatementId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PotentialDuplicateOfTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BankStatementId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PotentialDuplicateOfTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RawImportData",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CounterpartyId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountId",
                table: "TransactionRows",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Counterparties_CounterpartyId",
                table: "Transactions",
                column: "CounterpartyId",
                principalTable: "Counterparties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
