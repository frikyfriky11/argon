using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // De-duplicate any pre-existing case-insensitive duplicates before creating the
            // unique indexes, so the migration applies cleanly on databases that predate the
            // rule. Within each group of rows sharing a lower-cased value, the earliest row
            // (by Created, then Id) keeps its value; the others get a numeric suffix. The
            // value is truncated when needed so the suffix still fits the column length.
            migrationBuilder.Sql(
                """
                WITH ranked AS (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY LOWER("Name") ORDER BY "Created", "Id") AS rn
                    FROM "Accounts"
                )
                UPDATE "Accounts" a
                SET "Name" = LEFT(a."Name", 50 - LENGTH(' (' || r.rn || ')')) || ' (' || r.rn || ')'
                FROM ranked r
                WHERE a."Id" = r."Id" AND r.rn > 1;
                """);

            migrationBuilder.Sql(
                """
                WITH ranked AS (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY LOWER("Name") ORDER BY "Created", "Id") AS rn
                    FROM "Counterparties"
                )
                UPDATE "Counterparties" c
                SET "Name" = LEFT(c."Name", 100 - LENGTH(' (' || r.rn || ')')) || ' (' || r.rn || ')'
                FROM ranked r
                WHERE c."Id" = r."Id" AND r.rn > 1;
                """);

            migrationBuilder.Sql(
                """
                WITH ranked AS (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY LOWER("IdentifierText") ORDER BY "Created", "Id") AS rn
                    FROM "CounterpartyIdentifiers"
                )
                UPDATE "CounterpartyIdentifiers" ci
                SET "IdentifierText" = LEFT(ci."IdentifierText", 250 - LENGTH(' (' || r.rn || ')')) || ' (' || r.rn || ')'
                FROM ranked r
                WHERE ci."Id" = r."Id" AND r.rn > 1;
                """);

            // Case-insensitive unique indexes. These are functional indexes on LOWER(column),
            // which EF Core's fluent API cannot model, so they live as raw SQL and are not
            // reflected in the model snapshot. They back the case-insensitive uniqueness rules
            // enforced at the application layer by the Create/Update validators.
            migrationBuilder.Sql("""CREATE UNIQUE INDEX "IX_Accounts_Name_LowerUnique" ON "Accounts" (LOWER("Name"));""");
            migrationBuilder.Sql("""CREATE UNIQUE INDEX "IX_Counterparties_Name_LowerUnique" ON "Counterparties" (LOWER("Name"));""");
            migrationBuilder.Sql("""CREATE UNIQUE INDEX "IX_CounterpartyIdentifiers_IdentifierText_LowerUnique" ON "CounterpartyIdentifiers" (LOWER("IdentifierText"));""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The de-duplication renames are intentionally not reverted (the original values
            // are not retained); Down only removes the indexes.
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_CounterpartyIdentifiers_IdentifierText_LowerUnique";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Counterparties_Name_LowerUnique";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Accounts_Name_LowerUnique";""");
        }
    }
}
