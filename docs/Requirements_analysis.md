# BlogApi — Requirements Analysis

**Project**: BlogApi (personal blog / portfolio platform)
**Date**: 2026-08-14
**Author**: Alex Yoo

**Objective**: Build personal blog API for my own knowledge base / personal logs
**Deliverable**: A personal blog site backed by ASP.NET Core 10 + EF Core + SQLite, with a Vite + React + TypeScript frontend
**Document status**: Retrospective requirements analysis, compiled to formally document the system as designed and built. Written as a standard requirements specification would be, for portfolio and planning-continuity purposes.

---

## 1. Purpose and Background

The system is a personal blog intended to serve two purposes simultaneously:

1. A **public-facing content platform** where visitors can read posts published by the site owner.
2. A **learning vehicle** for gaining production-representative experience with the ASP.NET Core / Entity Framework Core stack, to complement existing experience with FastAPI (Python) and Express (TypeScript).

The scope was deliberately kept to a single-owner blog (one `Author` record functioning as the site's sole content owner) rather than a multi-tenant publishing platform, since the primary driver was backend fundamentals, not multi-user product design.

---

## 2. Stakeholders

| Stakeholder | Interest |
|---|---|
| Site owner (Alex) | Publishes and maintains blog content; sole authenticated user; uses the project as a portfolio artifact |
| Site visitors | Read published posts; require no account or authentication |
| Future reviewers (recruiters, interviewers) | Evaluate the codebase and this documentation as evidence of backend/full-stack capability |

---

## 3. Functional Requirements

### 3.1 Content Management (Posts)
- **FR-1**: The system shall allow the authenticated owner to create a post with a title, URL slug, body content, optional summary, publication status, and zero or more tags.
- **FR-2**: The system shall allow the authenticated owner to update all fields of an existing post, including replacing its tag set.
- **FR-3**: The system shall allow the authenticated owner to delete a post.
- **FR-4**: The system shall allow any visitor to list all **published** posts, ordered by publish date descending.
- **FR-5**: The system shall allow any visitor to retrieve a single post by its slug.
- **FR-6**: The system shall reject post creation or update if the request is not authenticated.
- **FR-7**: The system shall enforce slug uniqueness across all posts.
- **FR-8**: The system shall record a creation timestamp automatically and a publish timestamp at the moment a post transitions from draft to published.

### 3.2 Tagging
- **FR-9**: The system shall allow posts to be associated with any number of tags, and tags to be associated with any number of posts (many-to-many).
- **FR-10**: When a post references a tag name that does not yet exist, the system shall create it automatically rather than rejecting the request.
- **FR-11**: The system shall allow any visitor to list all tags, including the count of published posts using each tag.
- **FR-12**: The system shall allow any visitor to retrieve all published posts associated with a given tag.

### 3.3 Author Management
- **FR-13**: The system shall allow creation of an Author record with a name, optional bio, and optional email.
- **FR-14**: The system shall generate an API key automatically upon Author creation, using a cryptographically secure random source; the key shall not be client-suppliable.
- **FR-15**: The system shall expose the generated API key to the client exactly once, at creation time, and never include it in any subsequent read response.
- **FR-16**: The system shall allow an Author's profile fields (name, bio, email) to be updated.
- **FR-17**: The system shall allow an Author record to be deleted.
- **FR-18**: The system shall provide a mechanism to regenerate an Author's API key in the event the original key is lost.

### 3.4 Authentication and Authorization
- **FR-19**: The system shall require a valid API key, supplied via a request header, for all state-changing operations (create, update, delete) on posts and authors.
- **FR-20**: The system shall allow all read operations (listing and retrieving posts and tags) without authentication.
- **FR-21**: The system shall reject requests bearing an invalid or missing API key on protected endpoints with a 401 Unauthorized response.

### 3.5 Frontend
- **FR-22**: The system shall provide a public list view of published posts.
- **FR-23**: The system shall provide a public detail view of an individual post.
- **FR-24**: The system shall provide an authenticated-only creation form for new posts.
- **FR-25**: The system shall provide an authenticated-only edit form pre-populated with an existing post's data.
- **FR-26**: The system shall provide an authenticated-only control to delete a post, with a confirmation step before the action is executed.
- **FR-27**: The system shall persist the API key for the duration of a browser session, so the owner is not required to re-enter it on every page navigation.

---

## 4. Non-Functional Requirements

| ID | Category | Requirement |
|---|---|---|
| NFR-1 | Security | API keys must never appear in a GET response body under any circumstance. |
| NFR-2 | Security | Cross-origin requests shall only be accepted from an explicitly allow-listed frontend origin, not from arbitrary origins. |
| NFR-3 | Maintainability | Domain entities (EF Core models) shall not be serialized directly in API responses; all responses shall pass through an explicit DTO layer. |
| NFR-4 | Maintainability | The authentication mechanism shall be implemented against the framework's standard authentication pipeline (not ad hoc middleware), so that the API key scheme can later be replaced (e.g. with JWT) without modifying controller-level authorization logic. |
| NFR-5 | Usability | API responses shall use client-convenient shapes (e.g. `authorName` instead of a bare `authorId`, tag names instead of full tag objects) rather than mirroring the database schema. |
| NFR-6 | Data integrity | Post slugs must be enforced as unique at the database level, not only at the application level. |
| NFR-7 | Portability | The backend and frontend shall be independently deployable and shall communicate exclusively over HTTP(S), with no shared runtime or process. |
| NFR-8 | Documentation | The system's design decisions, known limitations, and development history shall be recorded in `docs/` alongside the source, not only in commit messages. |

---

## 5. User Stories

- *As a site visitor*, I want to browse a list of published posts, so that I can find something to read without needing an account.
- *As a site visitor*, I want to filter posts by tag, so that I can find content on a specific topic.
- *As the site owner*, I want to write and publish a new post from a form, so that I don't need to use the API directly (e.g. via Swagger) for routine content updates.
- *As the site owner*, I want to save a post as a draft (unpublished) and finish it later, so that incomplete posts are never visible to visitors.
- *As the site owner*, I want my write actions to require authentication, so that only I can modify site content even though the API is publicly reachable.
- *As the site owner*, I want to recover access if I lose my API key, so that a single lost credential doesn't permanently lock me out of my own content.

---

## 6. Constraints and Assumptions

- **Single-author assumption**: The data model supports multiple `Author` records, but the current system has exactly one active author in practice. Multi-author editorial workflows (e.g. author-to-author permissions, co-authoring) are out of scope.
- **Local-first development**: SQLite was chosen for development velocity and zero external dependencies; the schema is designed to be portable to a server-grade RDBMS (e.g. PostgreSQL) without structural changes, but no such migration has been performed.
- **No user accounts for readers**: The system assumes an anonymous, unauthenticated readership. Comments, reactions, or reader accounts are not part of the current scope.
- **Trust boundary**: The API key regeneration endpoint (FR-18) is currently unauthenticated by design necessity (a locked-out owner cannot present a key to unlock themselves). This is a known, accepted risk at the current single-user, local-development stage — see Section 8.

---

## 7. Out of Scope (Current Iteration)

- Multi-author collaboration and per-author permissions
- Reader accounts, comments, likes, or any reader-generated content
- Full-text search
- Pagination of post or tag listings
- Rich-text / WYSIWYG post authoring (plain text content only)
- Production deployment, HTTPS certificate management, and environment-specific configuration
- Automated test coverage (unit or integration)
- Rate limiting or abuse prevention on public read endpoints

---

## 8. Known Gaps Against Requirements

This section cross-references the [development log](./BlogApi_Dev_Log.md) and records where the delivered system does not yet fully satisfy the requirements above.

| Requirement | Gap |
|---|---|
| NFR-1 (API keys never in GET responses) | Satisfied for standard reads; however, the key is stored in plaintext at rest (not hashed), which is a related but distinct storage-layer risk not covered by NFR-1 as stated. |
| FR-18 (key recovery) | Implemented, but without any secondary verification (e.g. email confirmation), meaning anyone with an Author's numeric ID can currently trigger a key reset. Acceptable only because the system has a single trusted operator and is not internet-facing in its current deployment state. |
| General | No automated tests exist to verify any requirement in this document going forward; conformance has been checked manually via Swagger and the frontend during development. |

---

## 9. Relationship to Other Project Documents

- **[BlogApi_Dev_Log.md](./BlogApi_Dev_Log.md)** — chronological account of implementation, issues encountered, and lessons learned while building toward these requirements.
