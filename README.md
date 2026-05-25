# Argon

Personal finance management app. See [`CLAUDE.md`](CLAUDE.md) for architecture and [`src/Argon.Cli/README.md`](src/Argon.Cli/README.md) for the CLI.

## Dev database

```bash
docker compose -p argon -f compose/docker-compose.dev.yml up -d
```

Spins up an empty `ArgonDb` on `localhost:5432` (user `postgres`, password `Passw0rd!`). Schema is created by the WebApi on first startup via EF migrations.

## Dev secrets

Projects in this repo are wired for [.NET user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — values that shouldn't live in version control (real Authentik client ids, authority URLs, alternate connection strings) stay in your home directory and are loaded automatically by the configuration pipeline.

### WebApi

Auto-loaded in the Development environment:

```bash
dotnet user-secrets --project src/Argon.WebApi set "Auth:Authority" "https://auth.example.com/application/o/argon/"
dotnet user-secrets --project src/Argon.WebApi set "Auth:ClientId" "<your-client-id>"

dotnet user-secrets --project src/Argon.WebApi list
dotnet user-secrets --project src/Argon.WebApi remove "Key:Name"
```

Files land at `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json` (Linux/macOS) or `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json` (Windows). The `UserSecretsId` value is set in the project's csproj.

### CLI

Loaded unconditionally on every invocation (the CLI has no environment concept), via an explicit `AddUserSecrets` call in `Program.cs`. The `UserSecretsId` is `argon-cli`:

```bash
dotnet user-secrets --project src/Argon.Cli set "Auth:Authority" "https://auth.example.com/application/o/argon/"
dotnet user-secrets --project src/Argon.Cli set "Auth:ClientId" "<your-client-id>"
```

Same store regardless of whether you run via `dotnet run --project src/Argon.Cli` or the installed global `argon` tool.

## Restoring a backup

The dumps are `pg_dumpall` output. Drop the empty `ArgonDb` and pipe the dump in:

```bash
docker exec argon-db-1 psql -U postgres -c 'DROP DATABASE "ArgonDb";'
docker exec -i argon-db-1 psql -U postgres < argon_backup_*.sql
```

Sanity checks — total transaction count and most-recent edit timestamp:

```bash
docker exec argon-db-1 psql -U postgres -d ArgonDb -c 'SELECT COUNT(*) FROM "Transactions";'
docker exec argon-db-1 psql -U postgres -d ArgonDb -c 'SELECT t."LastModified" FROM "Transactions" AS t ORDER BY t."LastModified" DESC LIMIT 1;'
```

To wipe and start over: `docker compose -p argon -f compose/docker-compose.dev.yml down -v && … up -d`.
