# Argon CLI

`argon` is a thin command-line client over the Argon WebApi. It authenticates against the same Authentik OIDC provider as the browser front-end and exposes the surface needed to keep the ledger reconciled day to day: **Accounts, Counterparties, Counterparty Identifiers, and Transactions**.

> Out of scope for v1: Bank Statements, Budget Items. Both live on the API and can be added later.

## Prerequisites

- .NET 8 SDK
- A running Argon WebApi instance, e.g.:
  ```bash
  docker compose -f compose/docker-compose.dev.yml up -d   # Postgres
  dotnet run --project src/Argon.WebApi/Argon.WebApi.csproj
  ```
- An OAuth 2.0 application registered in Authentik for the CLI:
  - Name: `argon-cli`
  - Type: **Public** (no client secret)
  - Grant: **Device code** (`urn:ietf:params:oauth:grant-type:device_code`)
  - Scopes: `openid profile offline_access`

  Put the resulting client id in `src/Argon.Cli/appsettings.json` under `Auth:ClientId`, or override at runtime via `--client-id` / `ARGON_CLIENT_ID`.

## Local development — no install/pack loop

Run directly from source with `dotnet run`. Everything after `--` is forwarded to the CLI:

```bash
dotnet run --project src/Argon.Cli -- login
dotnet run --project src/Argon.Cli -- whoami
dotnet run --project src/Argon.Cli -- accounts list
dotnet run --project src/Argon.Cli -- tx list --account Sparkasse
```

Make it less verbose during a hacking session:

```bash
alias argon-dev='dotnet run --project /absolute/path/to/argon/src/Argon.Cli --'
argon-dev accounts list
```

Notes worth knowing:

- The `--` separator is required so the `dotnet` driver does not swallow CLI flags.
- Tokens written by `dotnet run` and by the installed global tool both land in the **same** `~/.config/argon-cli/credentials.json` (or `%LOCALAPPDATA%\argon-cli\credentials.json` on Windows). A `login` done one way is visible from the other.
- Point at a non-default API: either `ARGON_BASE_URL=https://argon.staging.example.com dotnet run --project src/Argon.Cli -- accounts list` or `--base-url https://...`.
- To regenerate `Generated/BackendClient.cs` after editing the API, just `dotnet build src/Argon.WebApi` — the existing NSwag MSBuild target writes both the TypeScript and C# clients.

## Install as a global `argon` command

Only needed when you want a system-wide command:

```bash
dotnet pack src/Argon.Cli/Argon.Cli.csproj -c Release
dotnet tool install -g --add-source ./src/Argon.Cli/nupkg argon
# Updating:
dotnet tool update -g --add-source ./src/Argon.Cli/nupkg argon
# Uninstalling:
dotnet tool uninstall -g argon
```

## Configuration

Resolution order, highest priority first:

1. CLI flags — `--base-url`, `--authority`, `--client-id`, `-o/--output`
2. Environment variables — `ARGON_BASE_URL`, `ARGON_AUTHORITY`, `ARGON_CLIENT_ID`
3. User config file — `~/.config/argon-cli/config.json` (Linux/macOS) or `%LOCALAPPDATA%\argon-cli\config.json` (Windows)
4. .NET user-secrets (dev — see below)
5. Bundled `appsettings.json` (ships with `http://localhost:5000` and stubs for the Authentik authority and client id)

User config file format mirrors `appsettings.json`:

```json
{
  "BaseUrl": "https://argon.example.com",
  "Auth": {
    "Authority": "https://auth.example.com/application/o/argon/",
    "ClientId": "your-cli-client-id"
  }
}
```

### Dev secrets via `dotnet user-secrets`

The bundled `appsettings.json` ships with stub values for `Auth:Authority` and `Auth:ClientId` so the file can stay in version control without leaking real endpoints. For local dev, put the real values in the .NET user-secrets store — it lives outside the repo and the CLI loads it automatically (the project's `UserSecretsId` is `argon-cli`):

```bash
dotnet user-secrets --project src/Argon.Cli set "Auth:Authority" "https://auth.example.com/application/o/argon/"
dotnet user-secrets --project src/Argon.Cli set "Auth:ClientId" "<your-client-id>"

# optional — only if you're not pointing at the default http://localhost:5000
dotnet user-secrets --project src/Argon.Cli set "BaseUrl" "https://argon.staging.example.com"

dotnet user-secrets --project src/Argon.Cli list
dotnet user-secrets --project src/Argon.Cli remove "Auth:ClientId"
```

Secrets land at `~/.microsoft/usersecrets/argon-cli/secrets.json` (Linux/macOS) or `%APPDATA%\Microsoft\UserSecrets\argon-cli\secrets.json` (Windows).

## Output formats

Every command accepts `-o`/`--output` with `table` (default), `json`, or `csv`. `table` prints scalar columns of the response, `json` pretty-prints, and `csv` emits one row per record. Nested children (like transaction rows) only show up under `json` for the list endpoints — `argon tx get <id>` always shows the rows.

`tx get` returns the same enriched shape as `tx list`: alongside the rows it carries `counterpartyName`, `rawImportData`, `status`, and `potentialDuplicateOfTransactionId`, so an audit can be driven entirely from `tx get` without joining back to `tx list`. In table mode the status (and any duplicate-of pointer) print in the header block.

`-o`/`--output` (and `--base-url`, `--authority`, `--client-id`) are **global options**: they work both before and after the subcommand, so `argon -o json tx list` and `argon tx list -o json` are equivalent.

JSON output has two ergonomic touches that make life easier when piping into `jq`:

- **`rawImportData` is flattened**. The original string is still there for fidelity, but every field inside it (`amount`, `accountingDate`, `currencyDate`, `rawDescription`, `counterpartyName`, parser-specific extras) is also lifted onto the surrounding transaction. So instead of `(.rawImportData | fromjson | .Amount)`, you can just write `.amount`. Pre-existing top-level fields take precedence on conflict.
- **`accountType` carries its name**. Wherever an `accountType` integer appears, a sibling `accountTypeName` carries the enum string (`Cash`, `Expense`, `Revenue`, `Setup`, `Debit`, `Credit`). The integer is preserved.

```bash
argon tx list -o json | jq '.[0] | {amount, counterpartyName, status}'
argon accounts list -o json | jq '.[] | select(.accountTypeName == "Expense") | .name'
```

## Common conventions

A few things hold across every command — worth reading once.

### Names are accepted wherever an account or counterparty id is expected

`--account`, `--counterparty`, the `tx categorize --account`, the `tx set-counterparty <counterparty>` positional, the account portion inside `--row`, and `tx history --counterparty` all accept either a GUID **or** the entity's name. The CLI fetches the list once per invocation and resolves locally:

Name resolution is **exact (case-insensitive) first, then substring**: an exact name match always wins; if there is none, a unique case-insensitive substring match is used (so `Athesia` resolves `Athesia Buch`). When a substring matches more than one entity the CLI lists the candidates and asks you to disambiguate rather than guessing. `tx history --counterparty` takes `--exact` to opt out of the substring fallback.

```bash
argon tx list --counterparty "Eurospar"
argon tx categorize <tx-id> --account "Alimentari"
argon tx create --counterparty "Amazon" \
    -r "Sparkasse famiglia":0:12.34 \
    -r "Store digitali":12.34:0
```

If a name matches more than one record the CLI errors out and prints the candidate GUIDs so you can disambiguate. `id` arguments on `get` / `update` / `delete` stay strict GUIDs because they target one specific record.

### `--status` accepts short aliases

`tx list --status` takes any of:

- `pending` (or `pending-import-review` / `PendingImportReview`)
- `confirmed`
- `duplicate` (or `potential-duplicate` / `PotentialDuplicate`)

### Pagination

`list` commands that paginate (`tx list`, `cp list`, `cpi list`) accept `--page` and `--page-size`. Use `--page-size -1` to fetch everything in one call — handy when piping to JSON.

When results are truncated (more rows exist beyond the page shown), the CLI says so: in table mode a `… N more not shown — use --page-size -1 for all` line follows the footer; in `-o json`/`-o csv` mode the same hint is written to **stderr**, so stdout stays a clean JSON/CSV payload for `| jq` while you still see that you're not looking at the full set. This is the cure for "I concluded it didn't exist, but it was just on page 2."

### Amount syntax in `--row`

`-r` / `--row` takes a colon-separated tuple. The empty side uses `0`:

```
<account>:<debit>:<credit>[:<description>]                # tx create
[rowId]:<account>:<debit>:<credit>[:<description>]        # tx update (empty rowId = new row)
```

`<account>` is a name or GUID; amounts use `.` as the decimal separator. The two-row default for a single bank movement is "cash side credit, expense side debit" (or vice versa for income).

## Auth

```bash
argon login        # device-code flow; on a desktop the URL opens automatically
argon whoami       # prints user info from the cached id_token claims
argon logout       # removes the local credentials file
```

In headless / SSH sessions the CLI detects `SSH_CONNECTION` / `SSH_TTY` and refuses to launch a browser — copy the printed URL to a desktop browser instead.

## Accounts

```bash
argon accounts list
argon accounts list --from 2026-01-01 --to 2026-05-20      # restrict total-amount window
argon accounts get <id>
argon accounts create --name "Checking" --type Cash
argon accounts update <id> --name "Renamed" --type Cash
argon accounts favourite <id>                              # toggles favourite on
argon accounts favourite <id> --is-favourite false
argon accounts delete <id>
```

`--type` takes the enum name — `Cash`, `Expense`, `Revenue`, `Setup`, `Debit`, `Credit`. `--from`/`--to` only affect the running-total window in the response, not which accounts are returned.

## Counterparties

```bash
argon counterparties list                                  # paginated
argon counterparties list --name "amazon"                  # case-insensitive substring filter
argon counterparties list --page-size -1                   # fetch all
argon cp get <id>
argon cp create --name "ACME Corp"
argon cp update <id> --name "New name"
argon cp delete <id>
```

`cp` is an alias for `counterparties`.

## Counterparty identifiers

Identifiers are the strings (IBANs, card descriptors, raw substrings) the importer uses to auto-match a counterparty against a bank-statement line.

```bash
argon cpi list                                              # paginated, all identifiers
argon cpi list --counterparty "Stadtwerke Bruneck"
argon cpi list --text "STADTWERKE"
argon cpi get <id>
argon cpi create --counterparty "Stadtwerke Bruneck" --text "STADTWERKE"
argon cpi update <id> --counterparty "Stadtwerke Bruneck" --text "STADTWERKE BRUNECK"
argon cpi delete <id>
```

### `cpi resolve <raw-text>` — preview what the importer would match

Given an arbitrary string (typically a snippet of a bank line), `resolve` returns the counterparties the importer would auto-match for it, with flags for whether each match came from a counterparty identifier, the counterparty name, or both:

```bash
argon cpi resolve "BONIFICO STADTWERKE BRUNECK 0001"
argon cpi resolve "AMAZON EU SARL" -o json
```

Two counterparties = ambiguous match = importer leaves the row uncategorised. Zero matches = no auto-match. One match = importer will assign it.

## Transactions

`tx` is an alias for `transactions`.

### Listing

```bash
argon tx list                                              # most recent first, paginated
argon tx list --status pending                             # rows still awaiting an account
argon tx list --status duplicate                           # potential duplicates flagged by the importer
argon tx list --account "Sparkasse famiglia" --from 2026-01-01
argon tx list --counterparty "Eurospar" --counterparty "Iperpoli"     # repeatable
argon tx list --month 2025-10                              # whole month, end-of-month aware
argon tx list --month current                              # this calendar month
argon tx list --month last                                 # previous calendar month
argon tx list --unlinked                                   # transactions with no linked counterparty
argon tx list --linked --month 2025-10                     # composes with the other filters
argon tx list --page-size 50
argon tx list --page-size -1 -o json | jq '.[] | .amount'
```

`--account` and `--counterparty` are repeatable for "any of these". `--from` and `--to` are inclusive. `--month` (`yyyy-MM`, `current`, or `last`) is shorthand that expands to an inclusive `--from`/`--to` range and cannot be combined with them. `--status` is the most useful filter during a reconciliation pass — `pending` is everything the importer left for a human to categorise. `--unlinked`/`--linked` filter on whether a counterparty is attached (server-side) — `--unlinked` is the counterparty-hygiene scan in one flag.

### Reading

```bash
argon tx get <id>                                          # shows date, counterparty, then rows
argon tx get <id> -o json                                  # full payload including rawImportData
```

### Creating a manual transaction

Both rows are filled by hand. Amounts use `0` for the empty side; the two sides must sum to zero (it's a double-entry ledger).

```bash
argon tx create --date 2026-05-20 --counterparty "Mein Beck" \
    -r "Sparkasse famiglia":0:8.50:"breakfast" \
    -r "Alimentari":8.50:0
```

### Updating a transaction (full payload)

`update` rewrites the transaction in one shot. Pass the row id in front of an existing row to keep it; leave the row id empty (`:`) to insert a new row. `--date` and `--counterparty` are optional — omit them to keep the transaction's existing values (the CLI fetches them for you).

```bash
argon tx update <tx-id> --date 2026-05-20 --counterparty "Mein Beck" \
    -r <row1-id>:"Sparkasse famiglia":0:8.50 \
    -r <row2-id>:"Ristoranti":8.50:0
```

For the much more common case of "the importer left one row blank and I just need to fill it in", reach for `tx categorize` instead. To change/add a *few* rows of a multi-row transaction without re-emitting the whole payload, use `tx patch`.

### `tx patch` — change only the rows you name

`patch` is the safe way to edit a multi-row transaction: you name only the rows you're changing (or adding); every other row passes through **verbatim** from the live transaction, so the immutable bank leg can't be corrupted by re-typing.

```bash
# fill the blank offsetting row and label it — the cash leg is never touched
argon tx patch <tx-id> -r <blank-row-id>:"Prodotti per la pulizia della casa":1.16:0:"Sale lavastoviglie"

# split a Mooney payment: set the real cost on the blank row, add a fee row
argon tx patch <tx-id> \
    -r <blank-row-id>:"Imposte e tasse":33.50:0 \
    -r :"Commissioni bancarie":1.50:0:"Commissione Mooney"
```

- A row with a `rowId` updates that row; an empty `rowId` (`:`) appends a new one.
- `--date`/`--counterparty` are optional and default to the transaction's existing values, so a patch on an as-yet-unlinked transaction works without inventing a counterparty.
- A parsed **Cash** (bank) row is treated as **read-only**: patch refuses to change its account/debit/credit and tells you so. Pass `--force` only if you genuinely mean to alter the parsed bank amount. Combined with the server-side balance check, this makes the silent "cash leg drifted by €X" class of error impossible via patch.

### `tx find` — locate a transaction by amount

Search for transactions having a row whose debit or credit matches an amount, with an optional tolerance — "is there a transaction anywhere matching this receipt?". The match runs server-side against the row debit/credit columns, so it returns just the hits (defaults to every match, not a single page).

```bash
argon tx find --amount 93.78                       # exact
argon tx find --amount 93.78 --tolerance 0.50      # near match
argon tx find --amount 50 --month 2025-11          # scope to a month
```

### `tx duplicate` — clone an existing transaction

For movements the parser can't ingest (cash withdrawals, recurring manual entries), clone a previous one instead of typing both rows from scratch. The rows, accounts, descriptions and counterparty are copied; `--date` sets the new date and the optional `--amount` rescales every row so the transaction total becomes that amount (proportionally — ideal for a fixed-shape cash-cash transfer). `--counterparty` overrides the copied counterparty.

```bash
argon tx duplicate <last-prelievo-id> --date 2026-06-08 --amount 50
```

### `tx categorize` — fill in one row of a pending transaction

```bash
argon tx categorize <tx-id> --account "Alimentari"
argon tx categorize <tx-id> --row 2 --account "Skoda Fabia - Benzina"
argon tx categorize <tx-id> --account "Prodotti per la pulizia della casa" -d "Sale lavastoviglie"
```

The CLI fetches the transaction, picks the unique row without an assigned account, and patches just that row. The transaction auto-confirms once every row has an account. Use `--row <counter>` (the 1-based `rowCounter` shown in `tx get`) when there are multiple blank rows or you want to overwrite a specific one. `--description`/`-d` sets the label on the row being categorized in the same call — no need to fall back to `tx update` just to add a description. Omitting it leaves any existing description untouched; pass `-d ""` to clear it.

### `tx set-counterparty` — fix the counterparty on a transaction

```bash
argon tx set-counterparty <tx-id> "Stadtwerke Bruneck"
argon tx set-counterparty <tx-id> 6b20ecc8-1234-...
```

Useful when the importer's auto-match landed on the wrong counterparty (or none).

### `tx history --counterparty` — frequency table by account

Answers "for this counterparty, which accounts has it historically been posted to, and how often?". Ordered by descending count.

```bash
argon tx history --counterparty "Eurospar"
argon tx history --counterparty "Amazon" -o json
```

A typical Amazon row will show several expense accounts (`Libri`, `Giochi`, `Elettronica`, `Cosmetica`, ...) plus the cash side. During reconciliation it's the fastest answer to "what does this usually map to?".

### Deleting

```bash
argon tx delete <tx-id>
```

## Reconciliation walkthrough

The day-to-day loop after running the bank-statement importer:

```bash
# 1. See how much is left to do
argon tx list --status pending --page-size -1

# 2. Pick one and inspect it (rows will show which one is missing an account)
argon tx get <tx-id>

# 3. Check what this counterparty usually maps to
argon tx history --counterparty "Eurospar"

# 4. Categorise the blank row (auto-confirms the transaction)
argon tx categorize <tx-id> --account "Alimentari"

# 5. Sometimes the auto-matched counterparty is wrong — fix it before categorising
argon tx set-counterparty <tx-id> "Stadtwerke Bruneck"
```

Wrapping the whole pending list in a script is straightforward — every operation accepts names so you never have to copy GUIDs around:

```bash
argon tx list --status pending --page-size -1 -o json |
  jq -r '.[] | "\(.id)\t\(.counterpartyName)\t\(.amount)\t\(.rawDescription)"'
```

## Troubleshooting

- **`Not signed in. Run argon login.`** — your token file is missing, expired without a refresh token, or the refresh failed. Run `argon login` again.
- **`No <thing> matching '<name>'.`** — a name didn't resolve. Either it's misspelled or it doesn't exist yet; `argon accounts list` / `cp list` will show the canonical spellings.
- **`Multiple <thing> entries match '<name>' (ids: ...). Disambiguate with the GUID.`** — two records share the name. Pass one of the printed GUIDs instead.
- **Headless / SSH session** — the CLI detects `SSH_CONNECTION` / `SSH_TTY` and will **not** try to launch a browser. Copy the printed URL into a browser on another machine.
- **Reset everything** — delete the credentials file:
  - Linux/macOS: `rm ~/.config/argon-cli/credentials.json`
  - Windows: `del %LOCALAPPDATA%\argon-cli\credentials.json`
- **Token storage on Linux/macOS** — tokens are stored in a `0600` file in plain JSON. On Windows they are wrapped with DPAPI (`ProtectedData`). If you want stronger protection on Linux/macOS, file an issue — Keychain / libsecret integration is the natural next step.
- **400 with a list of field errors** — these come straight from the API's FluentValidation. Re-read the message; it names the offending property.

## Adding a new command

The shape to copy is `Commands/AccountsCommand.cs`: a static class with a single `Build(CliContextFactory)` entry point that returns a configured `Command`. Each subcommand is a small private method that defines its options, then `SetHandler` calls the matching method on the generated `*Client` from `Generated/BackendClient.cs`. When the new command takes an account or counterparty reference, use `app.Resolver.ResolveAccountAsync` / `ResolveCounterpartyAsync` to accept names as well as GUIDs. Register the new top-level command in `Program.cs` next to the others.
