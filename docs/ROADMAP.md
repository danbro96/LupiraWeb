# Roadmap

Short-lived document: as items ship, collapse them into `CLAUDE.md`'s architecture section and delete from here. Kept out of `CLAUDE.md` so the brief stays short.

## 1. Experiences database (event-sourced)

Goal: a single store that records the history of personal and professional activity, queryable in multiple shapes without reshaping the source of truth.

**Tentative entities:**
- `Employment` — a job/role with start/end dates, company, title.
- `WorkProject` — a project inside or outside an employment.
- `Skill` — a tag; may be technology, method, or domain.
- `Experience` — a dated entry with free-form description, linked to zero or more of the above.
- Link tables for many-to-many (`ExperienceSkill`, `EmploymentSkill`, `WorkProjectSkill`, etc.).

**Event sourcing:** commands produce events (`ExperienceAdded`, `SkillTaggedToEmployment`, `WorkProjectRenamed`, ...) appended to an event log. Read-models (projections) materialize the shapes the API returns. Decisions to make:
- **Store**: Postgres with an `events` table + projection tables (simplest); Marten (Postgres + event sourcing library for .NET); EventStoreDB (heavier). Leaning Postgres + Marten.
- **Projection refresh**: synchronous on commit vs async worker. Start sync.
- **Snapshot cadence**: none initially; add if replay latency becomes a problem.

**Constraints / principles:**
- Immutable event log; corrections happen via compensating events, never edits.
- Every write goes through a command handler — no direct projection writes.
- Projections are disposable; `REBUILD` should always be possible from the event log.

## 2. API evolution

New endpoints on top of the experiences DB:

- `GET /api/experiences?from&to` — chronological slice.
- `GET /api/experiences/by-skill/{skillId}` — filter by skill.
- `GET /api/experiences/by-employment/{employmentId}` — filter by employment.
- `GET /api/skills`, `/employments`, `/work-projects` — lookup lists.

All wired through the same OpenAPI build-time generation → Orval pipeline. No manual TypeScript client edits.

## 3. Public UI views

One `/experience` page with a view-mode toggle:
- **Timeline** (default): chronological, newest first.
- **By employment**: grouped sections per job.
- **By skill / area**: grouped sections per skill tag.

Shared data fetch in an RSC; view-mode is a URL query param (`?view=timeline|employment|skill`) so links are shareable and server-render consistently.

## 4. Admin site

`lupiraweb.admin/` — separate Next.js App Router project. Not part of the public docker-compose.

- Authenticated (TBD: simple username+password with argon2 + a single admin row, or delegate to an IdP).
- Forms that issue commands to the backend command endpoints (`POST /api/admin/experiences`, etc.).
- Lives in the same repo; shares the generated API client via a small internal workspace package (or re-runs Orval against the same spec).

## 5. React Native port of admin

Once the admin web app is stable, port it to Expo + React Native.

- Reuse the generated API client verbatim (just `fetch` under the hood).
- Replace Next-specific layout primitives with RN equivalents.
- Auth flow uses the same backend endpoints + secure storage on device.
- Goal: add experiences from a phone without opening a laptop.

## Non-goals (current)

- Multi-user support. The site is single-author.
- CMS-style rich text. Plain markdown on experiences is enough.
- Analytics / tracking of visitors. See [app/cookies/page.tsx](lupiraweb.client/app/cookies/page.tsx).
