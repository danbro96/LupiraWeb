# Docker Deployment Strategy (TrueNAS SCALE 25.10)

This project is deployed as **two separate Docker images**:
- **backend** — ASP.NET Core 10 minimal API
- **frontend** — Next.js 16 (standalone Node runtime)

They are orchestrated together using **Docker Compose**, the recommended and supported approach on **TrueNAS SCALE**.

This document explains how the pieces fit together, why certain settings exist, and how to deploy and develop safely — including from Visual Studio.

---

## High-level Architecture

```
┌────────────┐        http://backend:80/api
│  Frontend  │  ───────────────────────────▶  Backend API
│ (Next.js)  │
│            │  Browser access
└─────▲──────┘        http(s)://frontend-host
      │
      │
User Browser
```

Key points:
- Frontend and backend run in **separate containers**.
- Containers communicate over the **internal Docker network**.
- The browser only talks to the **frontend**.
- The frontend proxies `/api` calls to the backend via Next.js rewrites.

---

## Backend (ASP.NET Core)

### launchSettings.json

The backend supports three execution modes:
- Local HTTP
- Local HTTPS
- Docker (Visual Studio "Container (Dockerfile)" profile)

Relevant Docker profile — see [LupiraWeb.Server/Properties/launchSettings.json](LupiraWeb.Server/Properties/launchSettings.json):

```json
"Container (Dockerfile)": {
  "commandName": "Docker",
  "launchUrl": "{Scheme}://{ServiceHost}:{ServicePort}",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS": "http://+:80;https://+:443"
  },
  "publishAllPorts": true,
  "useSSL": true
}
```

### Why this works in Docker

- `ASPNETCORE_URLS=http://+:80` lets Kestrel bind to **all interfaces**.
- Docker exposes port `80` internally.
- No hardcoded hostnames or ports.
- HTTPS can be terminated externally (recommended).

### API structure

All endpoints are grouped under `/api` (mapped per feature via endpoint extension classes — see [CLAUDE.md](CLAUDE.md) → Backend conventions).

The OpenAPI document is emitted at build time to [lupiraweb.client/backend-openapi.json](lupiraweb.client/backend-openapi.json) and consumed by **Orval** to generate the typed client.

---

## Frontend (Next.js)

### Dev proxy

`/api/*` is rewritten to `API_BASE_URL` in [lupiraweb.client/next.config.ts](lupiraweb.client/next.config.ts):

```ts
// default: http://localhost:5188 (local dev)
// in Docker:  http://backend:80   (service name DNS)
```

The custom fetcher at [lupiraweb.client/src/api/fetcher.ts](lupiraweb.client/src/api/fetcher.ts) resolves the base URL based on `typeof window` — absolute URL on the server (RSC/route handlers), relative `/api` in the browser.

### What this achieves

- During local dev: frontend on `localhost:3000`, API on `localhost:5188`, Next.js proxies `/api`.
- Inside Docker: `API_BASE_URL=http://backend:80`; frontend talks to backend via Docker DNS.

No CORS. No environment-specific code. Same `/api` paths everywhere.

---

## Orval Integration

- Orval consumes the backend OpenAPI definition emitted at `dotnet build`.
- Generates a typed API client for the frontend.
- Uses relative `/api` paths.

Because `/api` is stable across environments, the generated client works locally, in Docker, and in production with no regeneration for deployment.

---

## Docker Compose

Two layered files following the standard Compose convention:

### Base / prod — [docker-compose.yml](docker-compose.yml)

- Backend + frontend run from prebuilt images (`yourrepo/backend:latest`, `yourrepo/frontend:latest`).
- Backend env `ASPNETCORE_ENVIRONMENT=Production`; frontend env `API_BASE_URL=http://backend:80`.
- Backend data volume `lupira-db`.
- Both services `restart: unless-stopped`.

Run prod-style (ignores the dev override):
```bash
docker compose -f docker-compose.yml up
```

### Dev override — [docker-compose.override.yml](docker-compose.override.yml)

- Backend built from [LupiraWeb.Server/Dockerfile](LupiraWeb.Server/Dockerfile), exposed on `localhost:5188`.
- Frontend runs `node:22` with [lupiraweb.client/](lupiraweb.client/) bind-mounted at `/app` and `npm run dev`, exposed on `localhost:3000`.
- SQLite volume `lupira-db-dev` persisted at `/app/data` inside the backend.
- Backend healthcheck hits `/health`; compose waits before the frontend starts.

Plain `docker compose up` merges both files automatically — this is the default dev command:
```bash
docker compose up --build
```

### Notes

- `backend` is reachable internally as `http://backend:80`.
- `frontend` exposes port `3000` to the host.
- Browser only connects to the frontend.
- Backend does **not** need to be public.
- In production, don't ship the `override.yml` alongside the base file — it's the dev override by Docker convention.

---

## Running from Visual Studio 2022

The solution [LupiraWeb.slnx](LupiraWeb.slnx) ships a **Docker Compose project** — [docker-compose.dcproj](docker-compose.dcproj) — that wraps the hand-authored compose files. Requires **Docker Desktop** running and the **Container Tools** workload installed via the Visual Studio Installer.

### Option A — One-key full stack (recommended)

1. Right-click **docker-compose** in Solution Explorer → **Set as Startup Project**.
2. Press **F5**.

Visual Studio:
- Merges [docker-compose.yml](docker-compose.yml) + [docker-compose.override.yml](docker-compose.override.yml) and layers its own debug override on top (volume-mounts the backend build output, injects the remote debugger).
- Builds images, starts both services, attaches the backend debugger.
- Opens the browser at <http://localhost:3000> (configured via `DockerServiceUrl` in the dcproj).

Breakpoints in controllers/handlers hit on the next request. Frontend code hot-reloads via the bind mount. When you edit backend code, Stop (Shift+F5) and F5 again — VS does an incremental rebuild, not a full image rebuild.

### Option B — Backend only, containerized, with the debugger attached

1. Set `LupiraWeb.Server` as the startup project.
2. Launch profile: **Container (Dockerfile)**.
3. Press **F5**.

Same fast-mode debugging, but only the backend runs. Start the frontend separately with `npm run dev` in [lupiraweb.client/](lupiraweb.client/). Use this when you're doing backend-only work and don't want to wait for the frontend container.

### Option C — Skip VS tooling, use the terminal

1. Open **View → Terminal** (`` Ctrl+` ``).
2. From the solution root:
   ```bash
   docker compose up --build
   ```
3. Backend: <http://localhost:5188> · Frontend: <http://localhost:3000>.

No debugger attach, but useful when you want to inspect the exact behavior VS would see, without its injected debug override.

---

## Production Considerations

### HTTPS

Recommended:
- Terminate TLS at a reverse proxy (Traefik, Nginx, TrueNAS ingress).
- Run containers over HTTP internally.

Avoid:
- Managing certificates inside containers.
- Exposing Kestrel HTTPS directly.

### Environment variables

- Use `Production` for the backend in deployment.
- Do **not** rely on `launchSettings.json` in production.
- `Dockerfile` + environment variables define runtime behavior.

---

## Why This Strategy Is Solid

- Clear API boundary (`/api`).
- No CORS complexity.
- Docker-native networking.
- Works identically in dev and prod.
- Compatible with the TrueNAS SCALE app model.
- Frontend and backend can scale independently.

---

## Summary

This setup provides:
- Clean separation of frontend and backend.
- Predictable networking.
- Simple deployment on TrueNAS SCALE.
- Zero environment-specific frontend code.
- Safe OpenAPI-driven client generation.
- First-class Visual Studio debugging via the `docker-compose.dcproj` (full stack) or `Container (Dockerfile)` (backend only) profiles.

A production-grade Docker deployment pattern for ASP.NET Core + Next.js.
