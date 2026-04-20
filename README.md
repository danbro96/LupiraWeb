# LupiraWeb

Personal portfolio site by Daniel Broström. The code doubles as a DevOps showcase: reproducible builds, a build-time OpenAPI contract, containerized dev/prod, and tests across every layer.

- **Frontend** — [lupiraweb.client/](lupiraweb.client/) — Next.js 16 (App Router), Tailwind v4, Orval-generated API client.
- **Backend** — [LupiraWeb.Server/](LupiraWeb.Server/) — .NET 10 minimal API, emits OpenAPI at build time.
- **Containers** — [docker-compose.yml](docker-compose.yml) (base / prod) + [docker-compose.override.yml](docker-compose.override.yml) (dev override) + [docker-compose.dcproj](docker-compose.dcproj) (VS integration).

Architecture overview and working rules live in [CLAUDE.md](CLAUDE.md). Deployment details live in [ReadmeDocker.md](ReadmeDocker.md). Roadmap in [docs/ROADMAP.md](docs/ROADMAP.md).

## Run from the command line

```bash
# Generate the OpenAPI spec + typed client
dotnet build LupiraWeb.Server
cd lupiraweb.client && npm ci && npm run generate:api

# Dev (two terminals)
dotnet run --project LupiraWeb.Server --urls http://localhost:5188
cd lupiraweb.client && npm run dev        # → http://localhost:3000

# Full stack via Docker (auto-merges docker-compose.yml + docker-compose.override.yml)
docker compose up --build
```

For prod-style runs that ignore the dev override:

```bash
docker compose -f docker-compose.yml up
```

## Run from Visual Studio 2022

Open [LupiraWeb.slnx](LupiraWeb.slnx). The solution already includes a `docker-compose` project ([docker-compose.dcproj](docker-compose.dcproj)).

**One-key full stack (recommended)**

1. Right-click **docker-compose** in Solution Explorer → **Set as Startup Project**.
2. Press **F5**.
3. Visual Studio builds the images, brings up both containers via Docker Compose, attaches the debugger to the backend, and opens the browser at <http://localhost:3000>.

Breakpoints in controllers/handlers hit immediately. The frontend container live-mounts [lupiraweb.client/](lupiraweb.client/), so edits hot-reload. Backend code changes: stop (Shift+F5) and F5 again — VS rebuilds only what changed.

**Backend only (faster inner loop)**

1. Set `LupiraWeb.Server` as the startup project.
2. Launch profile: **Container (Dockerfile)**.
3. **F5** — backend runs in a container, debugger attached. Frontend you start separately with `npm run dev`.

Requires **Docker Desktop** running, the **Container Tools** workload in the Visual Studio Installer, and the **.NET 10 SDK**.

## Tests

```bash
dotnet test                                # backend unit + integration
cd lupiraweb.client && npm test            # frontend unit (Vitest + RTL)
cd lupiraweb.client && npm run test:e2e    # e2e (Playwright; needs backend running)
```
