# Argon

Personal finance management app. See [`CLAUDE.md`](CLAUDE.md) for architecture.

## Dev database

```bash
docker compose -p argon -f compose/docker-compose.dev.yml up -d
```

Spins up an empty `ArgonDb` on `localhost:5432` (user `postgres`, password `Passw0rd!`). Schema is created by the WebApi on first startup via EF migrations.

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
