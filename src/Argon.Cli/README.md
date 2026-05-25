# Argon CLI

`argon` is a thin command-line client over the Argon WebApi. It authenticates against the same Authentik OIDC provider as the browser front-end and exposes CRUD for **Accounts, Counterparties, Counterparty Identifiers, and Transactions**.

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
dotnet run --project src/Argon.Cli -- tx list --account <id>
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

### Output formats

Every command accepts `-o`/`--output` with `table` (default), `json`, or `csv`. `table` prints scalar columns of the response, `json` pretty-prints, and `csv` emits one row per record. Nested children (like transaction rows) only show up under `json` for the list endpoints — `argon tx get <id>` always shows the rows.

## Quick reference

```bash
# Auth
argon login
argon whoami
argon logout

# Accounts
argon accounts list
argon accounts list --from 2026-01-01 --to 2026-05-20
argon accounts create --name "Checking" --type Cash
argon accounts update <id> --name "Renamed" --type Cash
argon accounts favourite <id>            # toggles favourite on
argon accounts favourite <id> --is-favourite false
argon accounts delete <id>

# Counterparties
argon counterparties list --name acme
argon counterparties create --name "ACME Corp"
argon cp update <id> --name "New name"
argon cp delete <id>

# Counterparty identifiers
argon counterparty-identifiers create --counterparty <cp> --text "IT60X0542811101000000123456"
argon cpi list --counterparty <cp>

# Transactions
argon tx list --account <a> --from 2026-01-01 --page-size 50
argon tx get <id>
# Rows: <accountId>:<debit>:<credit>[:<description>]. Use 0 for the empty side.
argon tx create --date 2026-05-20 --counterparty <cp> \
    -r <acc1>:100:0:"groceries" \
    -r <acc2>:0:100:"cash out"
argon tx delete <id>
```

## Troubleshooting

- **`Not signed in. Run argon login.`** — your token file is missing, expired without a refresh token, or the refresh failed. Run `argon login` again.
- **Headless / SSH session** — the CLI detects `SSH_CONNECTION` / `SSH_TTY` and will **not** try to launch a browser. Copy the printed URL into a browser on another machine.
- **Reset everything** — delete the credentials file:
  - Linux/macOS: `rm ~/.config/argon-cli/credentials.json`
  - Windows: `del %LOCALAPPDATA%\argon-cli\credentials.json`
- **Token storage on Linux/macOS** — tokens are stored in a `0600` file in plain JSON. On Windows they are wrapped with DPAPI (`ProtectedData`). If you want stronger protection on Linux/macOS, file an issue — Keychain / libsecret integration is the natural next step.
- **400 with a list of field errors** — these come straight from the API's FluentValidation. Re-read the message; it names the offending property.

## Adding a new command

The shape to copy is `Commands/AccountsCommand.cs`: a static class with a single `Build(CliContextFactory)` entry point that returns a configured `Command`. Each subcommand is a small private method that defines its options, then `SetHandler` calls the matching method on the generated `*Client` from `Generated/BackendClient.cs`. Register the new top-level command in `Program.cs` next to the others.
