using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Argon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCounterparties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the new Counterparties table
            migrationBuilder.CreateTable(
                name: "Counterparties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<Instant>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counterparties", x => x.Id);
                });
            
            // Insert unique descriptions into Counterparties and give them new IDs
            migrationBuilder.Sql(
                """
                INSERT INTO "Counterparties" ("Id", "Name", "Created")
                SELECT DISTINCT ON ("Description") gen_random_uuid(), "Description", CURRENT_TIMESTAMP FROM "Transactions" WHERE "Description" IS NOT NULL;
                """);

            // Add new CounterpartyId column to Transactions
            migrationBuilder.AddColumn<Guid>(
                name: "CounterpartyId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            
            // Update Transactions to reference Counterparties
            migrationBuilder.Sql(
                """
                UPDATE "Transactions"
                SET "CounterpartyId" = (SELECT "Id" FROM "Counterparties" WHERE "Counterparties"."Name" = "Transactions"."Description")
                WHERE "Description" IS NOT NULL;
                """);
            
            // Make CounterpartyId non-nullable (after migration)
            migrationBuilder.AlterColumn<Guid>(
                name: "CounterpartyId",
                table: "Transactions",
                nullable: false);
            
            // Drop the Description column
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CounterpartyId",
                table: "Transactions",
                column: "CounterpartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Counterparties_CounterpartyId",
                table: "Transactions",
                column: "CounterpartyId",
                principalTable: "Counterparties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add the Description column
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
            
            // Restore old data from Counterparties
            migrationBuilder.Sql(
                """
                UPDATE "Transactions"
                SET "Description" = (SELECT "Name" FROM "Counterparties" WHERE "Counterparties"."Id" = "Transactions"."CounterpartyId")
                WHERE "CounterpartyId" IS NOT NULL;
                """);
            
            // Remove foreign key and CounterpartyId column
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Counterparties_CounterpartyId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CounterpartyId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                table: "Transactions");

            // Drop the Counterparties table
            migrationBuilder.DropTable(
                name: "Counterparties");
        }
    }
}
