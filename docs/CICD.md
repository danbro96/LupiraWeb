# CI/CD & Deployment Guide

End-to-end walkthrough of how LupiraWeb ships from `git push` on your laptop to a running stack on TrueNAS. If you only read one thing, read the [topology diagram](#topology) and [routine deploys](#routine-deploys).

## Topology

```mermaid
flowchart LR
  dev[Local dev] -->|git push| gh[GitHub]
  gh -->|ci.yml| tests[Tests + contract drift]
  tests -->|release.yml on main/tag| dh[(Docker Hub)]
  dh -->|manual pull in TrueNAS UI| nas[TrueNAS 25.10.1]

  subgraph nas[TrueNAS host]
    subgraph db_app[App: lupira-db]
      pg[(postgres:17-alpine)]
    end
    subgraph web_app[App: lupira-web]
      be[backend]
      fe[frontend]
    end
    subgraph admin_app[App: lupira-admin - future]
      adminbe[admin-backend]
    end
    be -.joins.-> netdata
    adminbe -.joins.-> netdata
    pg --- netdata((lupira_data external network))
  end
```

Three independent TrueNAS Apps share a single Docker network called `lupira_data`. The DB App owns the network; the web App (and later the admin App) join it as `external`. That's the whole trick — it's how the future admin API will reach the same Postgres without coupling its lifecycle to this repo's deploys.

## Pipelines

Two workflows in [.github/workflows/](../.github/workflows/).

### [ci.yml](../.github/workflows/ci.yml) — runs on every PR and non-main push

| Job | What it proves |
|---|---|
| `backend-test` | `dotnet build` regenerates the OpenAPI spec; `dotnet test` runs against a real Postgres via Testcontainers. |
| `contract-drift` | The regenerated [backend-openapi.json](../lupiraweb.client/backend-openapi.json) matches what's committed. Fails if a backend change regenerated the spec but the dev forgot to commit it. |
| `client-drift` | `npm run generate:api` produces the same Orval output as what's committed under [lupiraweb.client/src/api/](../lupiraweb.client/src/api/) (excluding the hand-written [fetcher.ts](../lupiraweb.client/src/api/fetcher.ts)). |
| `frontend-test` | `npm run lint` + `npm test` (Vitest). |

Playwright E2E is **not in CI** — run it locally with `npm run test:e2e` when it matters.

Make all four jobs required in branch protection for `main`.

### [release.yml](../.github/workflows/release.yml) — runs on push to `main` or a `v*` tag

1. Re-runs `ci.yml` via `workflow_call` — so nothing ships that didn't pass the same checks a PR did.
2. Builds `danbro96/lupiraweb-backend` and `danbro96/lupiraweb-frontend` in parallel (matrix), pushes to Docker Hub.

No deploy job. You decide when the NAS picks up the new image (see [Routine deploys](#routine-deploys)).

## Image tags

Emitted by `docker/metadata-action@v5`:

| Tag | When | Purpose |
|---|---|---|
| `sha-<7char>` | every push to main/tag | Immutable — **use for rollback**. |
| `latest` | push to `main` | Default tag the TrueNAS App tracks. |
| `<branch>` | push to any branch | Ad-hoc pulls from another machine. |
| `1.2.3`, `1.2`, `1` | git tag `v1.2.3` | Optional pinned releases. |

**Never deploy from `latest` expecting reproducibility.** `latest` is for humans; `sha-*` is for machines. If you need to know exactly what's running, read the `sha-*` tag off the container.

## First-time setup

### 1. GitHub secrets

Under **Settings → Secrets and variables → Actions**:

- `DOCKERHUB_USERNAME` — your Docker Hub login.
- `DOCKERHUB_TOKEN` — a Docker Hub access token with **Read & Write** scope (Account Settings → Security → New Access Token).

That's all. No production database credentials anywhere in GitHub.

### 2. TrueNAS: the `lupira-db` App

1. **Apps → Discover Apps → Custom App.**
2. Application name: `lupira-db`.
3. Paste the contents of [deploy/db/compose.yaml](../deploy/db/compose.yaml) into the YAML editor.
4. Set the required env vars (see [deploy/db/.env.example](../deploy/db/.env.example)):
   - `POSTGRES_DB=lupiraweb`
   - `POSTGRES_USER=lupira`
   - `POSTGRES_PASSWORD=<strong password>` ← write this down; the web App needs the same value.
5. Pick a TrueNAS dataset for the `pgdata` volume.
6. Save and start.

Verify the shared network exists:

```bash
# In the TrueNAS Shell
docker network inspect lupira_data
```

### 3. TrueNAS: the `lupira-web` App

1. **Discover Apps → Custom App.**
2. Application name: `lupira-web`.
3. Paste [deploy/web/compose.yaml](../deploy/web/compose.yaml).
4. Set env vars (see [deploy/web/.env.example](../deploy/web/.env.example)):
   - `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` — **must match the `lupira-db` App**.
   - `POSTGRES_HOST=postgres` (service name on the shared network).
   - `IMAGE_TAG=latest` (or pin a `sha-*` for reproducibility).
   - `FRONTEND_PORT=40080`, `BACKEND_PORT=40081` — host ports the reverse proxy targets.
5. Save and start.

Smoke-test from the TrueNAS Shell:

```bash
docker exec lupira-backend curl -sf http://localhost:80/health
# → Healthy

docker exec lupira-backend curl -sf http://localhost:80/health/ready
# → Healthy  (if this fails, DB connectivity is the likely cause)
```

### 4. Docker Hub login on the NAS (optional but recommended)

Avoids hitting the anonymous pull rate limit during rollouts:

```bash
docker login -u <dockerhub-user>
# paste a read-only token
```

### 5. Your reverse proxy

Point it at `<truenas-host>:40080` for the frontend and `<truenas-host>:40081` for the backend. (The frontend already rewrites `/api/*` → the backend internally via Next.js, so in most topologies you only need to expose the frontend externally — the backend port stays on your LAN.)

## Routine deploys

1. Merge to `main`. [release.yml](../.github/workflows/release.yml) runs.
2. When green, both `:latest` and `:sha-<short>` are live on Docker Hub.
3. Open the TrueNAS App (`lupira-web`) → **Update** (or **Pull image** → **Restart**). If you use `IMAGE_TAG=latest`, that's it.
4. Watch logs briefly to confirm Marten connected and the health endpoint goes green.

Nothing in this flow requires SSH, no secret sync, no Watchtower.

## Rollback

Fast path: change `IMAGE_TAG` on the web App from `latest` to a previous `sha-abc1234`, **Save**, **Pull image**, **Restart**. The old image is back in under a minute.

Find the previous `sha-*` in the commit history (`git log --oneline`) or in the Docker Hub tag list.

## Adding the admin stack later

When the admin-API repo is ready, it ships its own `deploy/admin/compose.yaml`. That file:

1. Declares `lupira_data` as `external: true`.
2. Attaches its backend service to it.
3. Uses the same `POSTGRES_*` env vars to build a connection string.

The DB App does not change. The web App does not change. The two-App pattern was designed for this.

If the admin API needs write tables that public reads shouldn't see, handle that with Marten schemas / Postgres roles inside the DB — not by forking the stack.

## Prod-safe Marten

In [LupiraWeb.Server/Program.cs](../LupiraWeb.Server/Program.cs), Marten's `AutoCreateSchemaObjects` is set to:

- `CreateOrUpdate` in **Development** — the old convenient behavior.
- `None` in **Production** — the app will not mutate schema on boot.

Applying schema changes in prod is a deliberate, one-shot operation. On the NAS:

```bash
docker exec -it lupira-backend dotnet LupiraWeb.Server.dll --apply-schema
```

(If the `--apply-schema` CLI hook isn't in place yet, either add it or use a transient script — the important thing is: booting the backend is not how schema changes land in prod.)

## Troubleshooting

**`contract-drift` fails in CI.** You edited the backend but didn't commit the regenerated spec. Run `dotnet build LupiraWeb.Server` locally, `git add lupiraweb.client/backend-openapi.json`, commit, push.

**`client-drift` fails in CI.** You bumped the OpenAPI spec but didn't regenerate the client. `cd lupiraweb.client && npm run generate:api`, commit the result.

**`backend-test` fails with `No such host is known` or Testcontainers hangs.** The GitHub-hosted runner lost its Docker daemon — rerun the job. If persistent, pin `ubuntu-latest` to a known-good runner version.

**Image pull fails on TrueNAS with `429 Too Many Requests`.** Docker Hub anonymous rate limit. `docker login` on the NAS with a read-only token.

**Backend boots but `/health/ready` stays red.** The backend can't reach Postgres. Check:
- `POSTGRES_HOST` in the web App env matches the service name in `deploy/db/compose.yaml` (default: `postgres`).
- Both containers are on the `lupira_data` network: `docker network inspect lupira_data`.
- Credentials match between the two Apps.

**Marten refuses to start in prod with a schema error.** You changed events/projections but didn't apply the schema change. Run the one-shot schema apply (see [Prod-safe Marten](#prod-safe-marten)).

**Need to pin a specific image forever.** Set `IMAGE_TAG=sha-abc1234` in the web App env. That tag is immutable.
