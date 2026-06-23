# Roadmap

Short-lived document: as items ship, collapse them into `CLAUDE.md`'s architecture section and delete from here. Kept out of `CLAUDE.md` so the brief stays short.

LupiraWeb is now a read-only BFF + frontend. The career domain (event-sourced Employments/Projects/Skills, query-shape endpoints) lives in **[LupiraCareerApi](https://github.com/danbro96/LupiraCareerApi)**; the authenticated writer lives in **[LupiraFamilyWeb](https://github.com/danbro96/LupiraFamilyWeb)** (a React + .NET BFF SSO admin that writes the domain by calling CareerApi over HTTP). What remains below is what LupiraWeb itself still owns.

## Public UI views

One `/experience` page with a view-mode toggle:
- **Timeline** (default): chronological, newest first.
- **By employment**: grouped sections per job.
- **By skill / area**: grouped sections per skill tag.

Shared data fetch in an RSC reading from the public API; view-mode is a URL query param (`?view=timeline|employment|skill`) so links are shareable and server-render consistently.

## Moved elsewhere

- **Career domain + event sourcing** (Employments, WorkProjects, Skills, Experiences; commands → events → projections) → [LupiraCareerApi](https://github.com/danbro96/LupiraCareerApi).
- **Query-shape API** (chronological / by-skill / by-employment, lookup lists) → CareerApi; LupiraWeb consumes it via the typed client and re-exposes the read shapes its UI needs.
- **Admin / writer + any RN port** → [LupiraFamilyWeb](https://github.com/danbro96/LupiraFamilyWeb).

## Non-goals (current)

- Multi-user support. The site is single-author.
- CMS-style rich text. Plain markdown on experiences is enough.
- Analytics / tracking of visitors. See [app/cookies/page.tsx](lupiraweb.client/app/cookies/page.tsx).
