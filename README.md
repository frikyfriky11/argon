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

## PostgreSQL major-version upgrades

The compose files run **PostgreSQL 18** (`postgres:18.4-alpine`). Crossing a major version (the original 15 → 18, or any future bump) is **not** a plain image-tag swap: the on-disk data format changes and the stock `postgres:18` image refuses to boot on an older data dir. Two things make the crossing a single `docker compose up`:

1. **`PGDATA` is pinned to `/var/lib/postgresql/data`** on the `db` service in every compose file. PostgreSQL 18's image moved the default `PGDATA` to `/var/lib/postgresql/18/docker`; **without the pin the image ignores the existing volume and initialises a fresh, empty cluster.** Keep the pin.
2. **A one-time transition through [`pgautoupgrade`](https://github.com/pgautoupgrade/docker-pgautoupgrade)** runs `pg_upgrade` in place on first boot.

### Performing the upgrade (15 → 18, and the pattern for future majors)

1. **Back up first** — `pg_upgrade` runs in link mode and is one-way:
   ```bash
   docker exec argon-db-1 pg_dumpall -U postgres > argon_pre18_$(date +%Y%m%d).sql
   ```
2. **Transition deploy** — temporarily point the `db` image at the auto-upgrade variant (same `PGDATA` pin, same volume):
   ```yaml
   image: "pgautoupgrade/pgautoupgrade:18.4-alpine"
   ```
   `docker compose … up -d --pull always` then detects the old data dir and upgrades it in place to 18.4 before serving. The prod/test healthcheck `start_period: 60s` keeps `webapi` (`depends_on: service_healthy`) waiting while the upgrade runs.
3. **Refresh planner statistics** — `pg_upgrade` does not carry them over:
   ```bash
   docker exec <db-container> vacuumdb -U postgres --all --analyze-in-stages
   ```
4. **Steady-state deploy** — flip the `db` image back to the lean stock image and redeploy:
   ```yaml
   image: "postgres:18.4-alpine"
   ```

The committed compose files are already at step 4 (stock `postgres:18.4-alpine`); the `pgautoupgrade` image is only needed for the single transition deploy. The dev path is identical — the dev container's data carries across `up` as long as you don't `down -v`. Rehearse against a clone of the volume before touching prod.
