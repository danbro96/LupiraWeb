# Docker Deployment Strategy (TrueNAS SCALE 25.10)

This project is deployed using **two separate Docker images**:
- **backend**: ASP.NET Core API
- **frontend**: Vite + React SPA

They are orchestrated together using **docker-compose**, which is the recommended and supported approach on **TrueNAS SCALE**.

This document explains how the pieces fit together, why certain settings exist, and how to deploy safely.

---

## High-level Architecture

```
┌────────────┐        http://backend:80/api
│  Frontend  │  ───────────────────────────▶  Backend API
│  (Vite)    │
│            │  Browser access
└─────▲──────┘        http(s)://frontend-host
      │
      │
User Browser
```

Key points:
- Frontend and backend run in **separate containers**
- Containers communicate over the **internal Docker network**
- The browser only talks to the **frontend**
- The frontend proxies `/api` calls to the backend

---

## Backend (ASP.NET Core)

### launchSettings.json

The backend supports three execution modes:
- Local HTTP
- Local HTTPS
- Docker

Relevant Docker profile:

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

- `ASPNETCORE_URLS=http://+:80` allows Kestrel to bind to **all interfaces**
- Docker exposes port `80` internally
- No hardcoded hostnames or ports
- HTTPS can be terminated externally (recommended)

### API structure

All endpoints are grouped under `/api`:

```csharp
var api = app.MapGroup("/api");

api.MapGet("/weatherforecast", () => { ... });
```

This is intentional:
- Makes reverse proxying simple
- Clean separation between API and SPA routing
- Required for Vite proxying

The OpenAPI document is generated from this API and consumed by **orval**.

---

## Frontend (Vite + React)

### Vite dev proxy

```ts
const devApiTarget = process.env.VITE_DEV_API || "http://localhost:5188";

server: {
  host: true,
  port: 5173,
  proxy: {
    "/api": {
      target: devApiTarget,
      changeOrigin: true,
      secure: false
    }
  }
}
```

### What this achieves

- During development:
  - Frontend runs on `localhost:5173`
  - API runs on `localhost:5188`
  - Vite transparently proxies `/api` requests
- Inside Docker:
  - `VITE_DEV_API=http://backend:80`
  - Frontend talks to backend via Docker DNS

No CORS.
No environment-specific code.
Same `/api` paths everywhere.

---

## Orval Integration

- Orval consumes the backend OpenAPI definition
- Generates typed API clients for the frontend
- Uses relative `/api` paths

Because `/api` is stable across environments:
- Generated clients work locally
- Generated clients work in Docker
- Generated clients work in production

No regeneration is required for deployment.

---

## Docker Compose Strategy

### Why docker-compose

- Native support in TrueNAS SCALE
- Simple service discovery (`backend`, `frontend`)
- Clear separation of concerns
- Independent image updates

### Recommended docker-compose.yml

```yaml
version: "3.9"

services:
  backend:
    image: yourrepo/backend:latest
    container_name: backend
    environment:
      ASPNETCORE_ENVIRONMENT: Production
    ports:
      - "8080:80"
    restart: unless-stopped

  frontend:
    image: yourrepo/frontend:latest
    container_name: frontend
    environment:
      VITE_DEV_API: http://backend:80
    ports:
      - "3000:80"
    depends_on:
      - backend
    restart: unless-stopped
```

### Notes

- `backend` is reachable internally as `http://backend:80`
- `frontend` exposes port `80` to the host
- Browser only connects to frontend
- Backend does **not** need to be public

---

## Production Considerations

### HTTPS

Recommended:
- Terminate TLS at a reverse proxy (Traefik, Nginx, TrueNAS ingress)
- Run containers over HTTP internally

Avoid:
- Managing certificates inside containers
- Exposing Kestrel HTTPS directly

### Environment variables

- Use `Production` for backend in deployment
- Do **not** use `launchSettings.json` in production
- Dockerfile + environment variables define runtime behavior

---

## Why This Strategy Is Solid

- Clear API boundary (`/api`)
- No CORS complexity
- Docker-native networking
- Works identically in dev and prod
- Compatible with TrueNAS SCALE app model
- Frontend and backend can scale independently

---

## Summary

This setup provides:
- Clean separation of frontend and backend
- Predictable networking
- Simple deployment on TrueNAS SCALE
- Zero environment-specific frontend code
- Safe OpenAPI-driven client generation

This is a production-grade Docker deployment pattern for ASP.NET Core + Vite.
