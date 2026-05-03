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
    subgraph platform[Platform Apps]
      pg[(medelynas-db<br/>postgres:17-alpine)]
      otel[medelynas-otel<br/>OpenObserve]
    end
    subgraph web_app[App: lupira-web]
      be[backend]
      fe[frontend]
    end
    subgraph admin_app[App: lupira-admin]
      adminbe[admin-backend]
    end
    be -.medelynas_data.-> pg
    adminbe -.medelynas_data.-> pg
    be -.medelynas_telemetry.-> otel
    adminbe -.medelynas_telemetry.-> otel
  end
```

The platform owns two shared services: **`medelynas-db`** (Postgres, owns external network `medelynas_data`) and **`medelynas-otel`** (OpenObserve, owns external network `medelynas_telemetry`). LupiraWeb's two backend Apps join both as `external` — `medelynas_data` for Postgres, `medelynas_telemetry` for OTLP egress. Admin is a single-container service (MVC, no separate frontend image). LupiraWeb does not stand up its own database; see [Guides/shared-postgres-platform.md](../../DevOps/Guides/shared-postgres-platform.md) for the platform DB pattern.

## Pipelines

Two workflows in [.github/workflows/](../.github/workflows/).

### [ci.yml](../.github/workflows/ci.yml) — runs on every PR and non-main push

| Job | What it proves |
|---|---|
| `backend-test` | `dotnet build LupiraWeb.slnx` regenerates both public and admin OpenAPI specs; `dotnet test` runs all suites against a real Postgres via Testcontainers. |
| `contract-drift` | Both the committed [public spec](../lupiraweb.client/backend-openapi.json) and [admin spec](../lupiraweb.admin/backend-openapi.json) match what the build produces. Fails if a backend change regenerated a spec but the dev forgot to commit it. |
| `client-drift` | `npm run generate:api` produces the same Orval output as what's committed under [lupiraweb.client/src/api/](../lupiraweb.client/src/api/) (excluding the hand-written [fetcher.ts](../lupiraweb.client/src/api/fetcher.ts)). |
| `frontend-test` | `npm run lint` + `npm test` (Vitest). |

Playwright E2E is **not in CI** — run it locally with `npm run test:e2e` when it matters.

Make all four jobs required in branch protection for `main`.

### [release.yml](../.github/workflows/release.yml) — runs on push to `main` or a `v*` tag

1. Re-runs `ci.yml` via `workflow_call` — so nothing ships that didn't pass the same checks a PR did.
2. Builds three images in parallel (matrix) and pushes to Docker Hub:
   - `danbro96/lupiraweb-backend`
   - `danbro96/lupiraweb-frontend`
   - `danbro96/lupiraweb-admin-backend`

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

### 2. Provision the `lupiraweb` database on `medelynas-db`

LupiraWeb uses the platform Postgres (`medelynas-db`) — it does **not** stand up its own DB App. Prerequisites: `medelynas-db` and `medelynas-otel` Custom Apps already exist on the host. If they don't, see [Guides/shared-postgres-platform.md](../../DevOps/Guides/shared-postgres-platform.md) and [Guides/otel-collector.md](../../DevOps/Guides/otel-collector.md).

In the TrueNAS Shell:

```bash
docker exec -it medelynas-db psql -U medelynas_admin postgres
```

Then in the `psql` prompt:

```sql
CREATE ROLE lupiraweb_user WITH LOGIN PASSWORD '<strong password>';
CREATE DATABASE lupiraweb OWNER lupiraweb_user;
REVOKE ALL ON DATABASE lupiraweb FROM PUBLIC;
GRANT CONNECT ON DATABASE lupiraweb TO lupiraweb_user;
\q
```

Write down the password — both `lupira-web` and `lupira-admin` need it.

Verify:

```bash
docker network inspect medelynas_data    # should show medelynas-db attached
docker exec medelynas-db psql -U lupiraweb_user -d lupiraweb -c '\conninfo'
```

### 3. TrueNAS: the `lupira-web` App

1. **Discover Apps → Custom App.**
2. Application name: `lupira-web`.
3. Paste [deploy/web/compose.yaml](../deploy/web/compose.yaml). The compose joins external networks `medelynas_data` (for Postgres) and `medelynas_telemetry` (for OTLP); both must already exist.
4. Set env vars (see [deploy/web/.env.example](../deploy/web/.env.example)):
   - `POSTGRES_DB=lupiraweb`, `POSTGRES_USER=lupiraweb_user`, `POSTGRES_PASSWORD=<from Section 2>`.
   - `POSTGRES_HOST=postgres` (service name of the `medelynas-db` container on the shared network).
   - `IMAGE_TAG=latest` (or pin a `sha-*` for reproducibility).
   - `FRONTEND_PORT=40080`, `BACKEND_PORT=40081` — host ports the reverse proxy targets.
   - `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:5080/api/default` (OpenObserve hostname alias on `medelynas_telemetry`).
   - `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`.
   - `OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic <base64>` (OpenObserve OSS auth — generate per [Guides/otel-collector.md](../../DevOps/Guides/otel-collector.md)).
   - `OTEL_RESOURCE_ATTRIBUTES=deployment.environment=prod,host.name=medelynas`.
5. Save and start.

Smoke-test from the TrueNAS Shell:

```bash
docker exec lupira-backend curl -sf http://localhost:80/health
# → Healthy

docker exec lupira-backend curl -sf http://localhost:80/health/ready
# → Healthy  (if this fails, DB connectivity is the likely cause)
```

### 4. TrueNAS: the `lupira-admin` App

1. **Discover Apps → Custom App.**
2. Application name: `lupira-admin`.
3. Paste [deploy/admin/compose.yaml](../deploy/admin/compose.yaml).
4. Set env vars (see [deploy/admin/.env.example](../deploy/admin/.env.example)):
   - `POSTGRES_DB=lupiraweb`, `POSTGRES_USER=lupiraweb_user`, `POSTGRES_PASSWORD=<from Section 2>`.
   - `POSTGRES_HOST=postgres`.
   - `IMAGE_TAG=latest` (or pin a `sha-*`).
   - `ADMIN_BACKEND_PORT=40082`.
   - `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:5080/api/default`.
   - `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`.
   - `OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic <base64>` (same value as lupira-web).
   - `OTEL_RESOURCE_ATTRIBUTES=deployment.environment=prod,host.name=medelynas`.
5. Save and start.

Admin owns schema migrations. When events/projections change, apply schema once before rolling public:

```bash
docker exec -it lupira-admin-backend dotnet LupiraWeb.Admin.Server.dll --apply-schema
```

Keep the admin App behind LAN/VPN or an authenticated reverse proxy — it's a write-capable service.

### 5. Docker Hub login on the NAS (optional but recommended)

Avoids hitting the anonymous pull rate limit during rollouts:

```bash
docker login -u <dockerhub-user>
# paste a read-only token
```

### 6. Your reverse proxy

Point it at `<truenas-host>:40080` for the frontend, `<truenas-host>:40081` for the public backend, and `<truenas-host>:40082` for the admin backend. (The frontend already rewrites `/api/*` → the backend internally via Next.js, so in most topologies you only need to expose the frontend externally — the public backend port stays on your LAN. Admin should be LAN/VPN-only or behind an auth layer.)

## Routine deploys

1. Merge to `main`. [release.yml](../.github/workflows/release.yml) runs.
2. When green, `:latest` and `:sha-<short>` tags are live on Docker Hub for all three images.
3. For each TrueNAS App you want to roll (`lupira-web`, `lupira-admin`): open it → **Update** (or **Pull image** → **Restart**). If you use `IMAGE_TAG=latest`, that's it.
4. Watch logs briefly to confirm Marten connected and the health endpoint goes green.

Schema-changing deploys: apply schema via the admin `--apply-schema` CLI *before* updating the public `lupira-web` App (see [Prod-safe Marten](#prod-safe-marten)).

Nothing in this flow requires SSH, no secret sync, no Watchtower.

## Rollback

Fast path: change `IMAGE_TAG` on the web App from `latest` to a previous `sha-abc1234`, **Save**, **Pull image**, **Restart**. The old image is back in under a minute.

Find the previous `sha-*` in the commit history (`git log --oneline`) or in the Docker Hub tag list.

## Prod-safe Marten

Both backends run with Marten's `AutoCreateSchemaObjects` set to:

- `CreateOrUpdate` in **Development** — the old convenient behavior.
- `None` in **Production** — neither app mutates schema on boot.

**Admin owns schema.** Only the admin server exposes the `--apply-schema` CLI. Deploy ordering for a breaking schema change:

1. Release new images (merge to main).
2. On the NAS, apply schema via admin:
   ```bash
   docker exec -it lupira-admin-backend dotnet LupiraWeb.Admin.Server.dll --apply-schema
   ```
3. Roll the `lupira-admin` App (so admin's new code writes against the updated schema).
4. Roll the `lupira-web` App (so public reads the updated schema).

The shared kernel lives in [LupiraWeb.Domain](../LupiraWeb.Domain/): event records, aggregate documents, projection docs, and `UseLupiraProjections()` — the single place that registers all 17 projections. Both backends reference it; admin calls `UseLupiraProjections()` to wire the write-side logic, public calls the same helper because inline projections are idempotent and public never writes events in prod.

## Troubleshooting

**`contract-drift` fails in CI.** You edited a backend but didn't commit the regenerated spec. Run `dotnet build LupiraWeb.slnx` locally, then `git add lupiraweb.client/backend-openapi.json lupiraweb.admin/backend-openapi.json`, commit, push.

**`client-drift` fails in CI.** You bumped the OpenAPI spec but didn't regenerate the client. `cd lupiraweb.client && npm run generate:api`, commit the result.

**`backend-test` fails with `No such host is known` or Testcontainers hangs.** The GitHub-hosted runner lost its Docker daemon — rerun the job. If persistent, pin `ubuntu-latest` to a known-good runner version.

**Image pull fails on TrueNAS with `429 Too Many Requests`.** Docker Hub anonymous rate limit. `docker login` on the NAS with a read-only token.

**Backend boots but `/health/ready` stays red.** The backend can't reach Postgres. Check:
- `POSTGRES_HOST=postgres` matches the service name of the `medelynas-db` container.
- The backend container is on the `medelynas_data` network: `docker network inspect medelynas_data`.
- Credentials match the role you provisioned in Section 2 (`lupiraweb_user` / `<password>`).

**No traces or metrics in OpenObserve for `lupira-web` / `lupira-admin`.** Telemetry is silently failing or the wrong target. Check:
- `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:5080/api/default` — note `http`, not `https`, OpenObserve's HTTP port `5080`, and the `/api/default` org segment.
- `OTEL_EXPORTER_OTLP_HEADERS=Authorization=Basic <base64>` is set — without it OpenObserve rejects ingestion silently.
- The backend container is on `medelynas_telemetry`: `docker network inspect medelynas_telemetry`.
- OpenObserve logs: `sudo docker logs openobserve --tail 50 | grep -iE 'unauth|401|reject'`.

**Marten refuses to start in prod with a schema error.** You changed events/projections but didn't apply the schema change. Run the one-shot schema apply (see [Prod-safe Marten](#prod-safe-marten)).

**Need to pin a specific image forever.** Set `IMAGE_TAG=sha-abc1234` in the web App env. That tag is immutable.
