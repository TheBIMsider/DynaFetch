# DynaFetch -- Project Overview

## What This Project Is

DynaFetch is a modern REST API package for Dynamo 3.0+ (.NET 8) that replaces the DynaWeb package with a cleaner, easier-to-maintain design. It ships as ZeroTouch nodes for making HTTP requests and handling JSON inside Dynamo graphs. Tagline: "Your Data Retriever". Logo: a Golden Retriever silhouette carrying a JSON bracket `{` as a stick.

It is an open-source, community-focused package, distributed through GitHub releases and the official Dynamo Package Manager. Carl Storms (The BIMsider) is the sole developer; work is done in VS Code with C# Dev Kit.

**Attribution:** Inspired by DynaWeb by Radu Gidei (https://github.com/radumg/DynaWeb, AGPL 3.0). DynaFetch is a clean-room implementation with original code and proper attribution, licensed BSD-3-Clause. Keep the attribution intact in the README, docs, and the migration guide.

---

## Current Status

**Production, v1.0.10 (released; tags v1.0.0 through v1.0.10 on GitHub), in maintenance and enhancement mode.** Core development (Phases 1-10) is complete and distributed.

The original roadmaps stopped at Session 25 (v1.0.7, 2025-11-24). Work since then, confirmed in the code and release notes:

- Custom `Content-Type` support is implemented. `ExecuteNodes.POST`, `PUT` and `PATCH` use a `Content-Type` header set via `AddHeader` (for example `application/merge-patch+json`) and default to `application/json` only when none is set. This was the Session 25 "critical issue", now resolved across two releases:
  - **v1.0.8 (24 Nov 2025):** first fix. Previously the `application/json` default overrode the custom header, causing 415 Unsupported Media Type errors. Changed `HttpRequest.ToHttpRequestMessage()` and the `ExecuteNodes` docs for POST/PUT/PATCH.
  - **v1.0.9 (1 Dec 2025):** follow-up. `ToHttpRequestMessage()` now recreates the content with the custom type, and the POST/PUT/PATCH handling in `ExecuteNodes` was updated to match. Files changed: `HttpRequest.cs`, `ExecuteNodes.cs`.
  - Neither release needs workflow changes. The issue was reported by Robert Manna (credited in the release notes).
- File uploads are supported through `HttpRequest` with `AddFile` (multipart), which covers the old deferred "Upload File to APS" idea.
- The repo includes a 7th DynaWeb migration sample, "GET ACC Projects and Users".

Tests: `dotnet test` run on 2026-10-24 passed 25 of 25 (live-API integration tests included).

Known build warnings (2026-10-24): `System.Text.Json` 8.0.4 carries a high-severity advisory (GHSA-8g4q-xg66-9fp4), and the Dynamo packages restore as 3.0.0.7186 because 3.0.0 is not on NuGet. Both are candidates for a future patch release.

Still to confirm with Carl: the release notes for v1.0.10, and the current state of the BIMtrack multi-user sample.

### Next Priorities

To be set by Carl. Update this section and `current-feature.md` as work is chosen.

---

## Tech Stack

- **Language / framework:** C#, .NET 8
- **Platform:** Dynamo 3.0+ ZeroTouch package (`DynamoVisualProgramming.DynamoServices` 3.0.x)
- **HTTP:** built-in `System.Net.Http.HttpClient` (async/await throughout; not RestSharp)
- **JSON:** dual engine -- `System.Text.Json` (default, performance) with `Newtonsoft.Json` (compatibility layer)
- **Tests:** NUnit + Moq project in `tests/DynaFetch.Tests` (unit and live-API integration tests; JSONPlaceholder and httpbin.org)
- **IDE:** VS Code + C# Dev Kit
- **Distribution:** GitHub releases + Dynamo Package Manager (`pkg.json`)
- **Landing page:** GitHub Pages, vanilla HTML/CSS/JS at `landing/`

---

## File Structure

```
DynaFetch/
├── CLAUDE.md
├── DynaFetch.sln
├── pkg.json                    # Dynamo Package Manager manifest
├── README.md / CONTRIBUTING.md / LICENSE / samples.md
├── DynaFetch_Logo_{B,O,W}.png
│
├── src/DynaFetch/
│   ├── DynaFetch.csproj
│   ├── Core/
│   │   ├── HttpClientWrapper.cs   # connection management and settings
│   │   ├── HttpRequest.cs         # request building and configuration
│   │   ├── HttpResponse.cs        # response handling and parsing
│   │   └── ErrorHandling.cs       # validation and exceptions
│   ├── Nodes/                     # Dynamo-facing ZeroTouch node classes
│   │   ├── ClientNodes.cs
│   │   ├── RequestNodes.cs
│   │   ├── ExecuteNodes.cs
│   │   └── JsonNodes.cs
│   └── Utilities/
│       └── JsonHelper.cs          # serialization utilities
│
├── tests/DynaFetch.Tests/         # BasicTests + Integration/ (API, Authentication, POST)
├── bin/DynaFetch.dll              # committed build output used for packaging
│
├── samples/
│   ├── DynaFetch_Samples/         # 7 original workflow graphs (.dyn + .png)
│   ├── DynaFetched_DynaWeb_Samples/  # DynaWeb migration samples 1-7
│   └── DynaFetch_Nodes_Groups/    # node-library reference graphs
│
├── docs/                          # API-Documentation, Node-Library, Advanced-Node-Library,
│                                  # Migration-Guide, Best-Practices, Troubleshooting (.md + .pdf)
├── landing/                       # GitHub Pages site (index.html, styles.css, script.js, assets/)
├── .github/ISSUE_TEMPLATE/        # bug, feature, support, PR templates
│
├── context/                       # Claude Code context files
└── .claude/                       # Claude Code skills/agents
```

---

## Architecture

### Core components (`src/DynaFetch/Core`, `Utilities`)

- **HttpClientWrapper** -- connection management and settings, client-level persistent auth
- **HttpRequest** -- request building and configuration (headers, parameters, body)
- **HttpResponse** -- response handling and parsing
- **ErrorHandling** -- validation and exception management with helpful user-facing messages
- **JsonHelper** -- serialization and Dynamo-friendly type conversion (JSON to Dynamo lists/dictionaries)

### Node categories (`src/DynaFetch/Nodes`)

| Category | Purpose | Examples |
|---|---|---|
| Client | Connection and configuration | Create, SetBaseUrl, SetTimeout, SetHeaders, SetUserAgent, auth |
| Request | Request building | ByUrl, ByEndpoint, AddHeader, AddParameter, AddJsonBody, SetMethod |
| Execute | HTTP method execution | GET, POST, PUT, DELETE, PATCH |
| JSON | Data transformation | Deserialize, Serialize, ToDictionary, FromDictionary |

About 48 workflow methods and 220+ total nodes (including inherited system nodes shown in the node library docs).

### Design principles

1. Modern .NET 8 -- built-in HttpClient, System.Text.Json, async/await
2. Clean API -- fewer classes and clearer workflows than DynaWeb
3. Backward compatible -- node patterns similar to DynaWeb to ease migration
4. Extensible -- easy to add HTTP methods and data formats
5. Maintainable -- well documented and testable
6. Community-focused -- open source, welcoming, approachable

### DynaWeb to DynaFetch mapping

| DynaWeb | DynaFetch | Notes |
|---|---|---|
| WebClient.ByUrl | Client.Create | Simplified creation |
| WebRequest.ByUrl | Request.ByUrl | Same pattern |
| Execute.GET | Execute.GET | Direct mapping |
| Helpers.Deserialise | JSON.Deserialize | Enhanced functionality |

Breaking changes from DynaWeb: namespace `DynaWeb` to `DynaFetch`, some node consolidation, stricter error handling.

---

## Established Workflow Patterns (Session 25)

- **Path parameter substitution:** build the URL from a template with `String.Replace("{hubId}", id)` etc., then pass the finished URL to `ClientNodes.CreateClient`.
- **Batch loop:** index each parallel input list (`List.GetItemAtIndex`), build a `Dictionary.ByKeysValues` per item, call `ExecuteNodes.POST` per item.
- **Dictionary with array values:** list values inside a dictionary serialize to JSON arrays (for example `ProjectTeams: [101, 102]`).
- **Authentication:** Bearer tokens, API keys, custom headers, and persistent client-level auth.

---

## Key Facts

- **Package name:** DynaFetch. Current version 1.0.10 (see `pkg.json` and `DynaFetch.csproj`; keep both in sync).
- **Target:** .NET 8, Dynamo 3.0+ (tested with Revit 2025 / Dynamo Sandbox).
- **License:** BSD-3-Clause.
- **Landing page:** https://thebimisider.github.io/DynaFetch/landing/
- **Testing:** NUnit suite plus manual verification in Dynamo with sample graphs. Live-API tests depend on external services (JSONPlaceholder, httpbin.org).
- **History:** git tags `v1.0.0` to `v1.0.10` and the GitHub release notes are the record of what shipped. The original Session 1-25 roadmap files were retired in favor of this file and `current-feature.md`.

---

## Future Ideas (Unscheduled Backlog)

Not planned or committed; collected from earlier brainstorming. Pick from here only when Carl chooses.

- **Authentication:** OAuth 2.0 flows, automatic token refresh, multi-provider auth (Azure AD, AWS Cognito), client-certificate auth
- **Data handling:** streaming for large datasets, batch operations with queuing, XML/CSV serialization (binary upload is already supported via `AddFile`)
- **Resilience:** automatic retry with exponential backoff, rate limiting, circuit breaker, response caching, built-in metrics
- **Protocols:** GraphQL, gRPC, WebSockets
- **Integrations:** Dynamo Player compatibility, Revit and Civil 3D patterns, Excel, PowerBI, database connectors, cloud storage (S3, Azure Blob, Google Cloud Storage)
- **Community and docs:** video tutorials, landing-page screenshots, industry-specific sample graphs, workshop materials, curated community examples
- **Tooling:** documentation generated from code comments, API wrapper generator, a performance profiler, load and security testing

---

## Known Risks -- Do Not Skip

| Risk | Why it matters |
|---|---|
| Content-type handling | Execute nodes honor a custom `Content-Type` and default to `application/json` only when none is set. Any change here affects every POST/PUT/PATCH user -- preserve that default. |
| Breaking node signatures | Published graphs (including the DynaWeb migration samples) depend on node names and input port order, for example `ExecuteNodes.POST` input order. Renaming or reordering ports breaks users' existing `.dyn` files. |
| Docs/sample drift | Node changes need matching updates to `docs/` (Markdown and PDF), `samples/`, and the landing page. |
| Version bump discipline | A behavior change needs a deliberate patch vs. minor decision, `pkg.json` update, and release notes. |
| Attribution and license | Keep the DynaWeb attribution and BSD-3-Clause notices intact. Do not copy DynaWeb (AGPL) code. |

---

## Distribution

Releases go out through GitHub releases and the Dynamo Package Manager. Each release needs: updated `pkg.json` version, a built `bin/DynaFetch.dll`, refreshed docs and samples, release notes (flag any behavior changes), and an installation test in Dynamo.
