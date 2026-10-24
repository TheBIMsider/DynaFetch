# DynaFetch

DynaFetch is a modern REST API package for Dynamo 3.0+ (.NET 8) that replaces DynaWeb with a cleaner, easier-to-maintain design. It ships as ZeroTouch nodes for making HTTP requests and handling JSON inside Dynamo graphs. Open source (BSD-3-Clause), community-focused, distributed through GitHub releases and the Dynamo Package Manager. Tagline: "Your Data Retriever". Inspired by DynaWeb by Radu Gidei (clean-room implementation, attribution must stay intact). Currently v1.0.10, in maintenance and enhancement mode.

## Context Files

Read these for full project context:

- @context/project-overview.md: Status, architecture, node categories, patterns, risks
- @context/coding-standards.md: Code conventions and patterns
- @context/ai-interaction.md: Workflow and communication guidelines
- @context/current-feature.md: What we are currently working on

## Tech Stack

- **Language / framework:** C#, .NET 8
- **Platform:** Dynamo 3.0+ ZeroTouch package (`DynamoVisualProgramming.DynamoServices` and `ZeroTouchLibrary` 3.0.0)
- **HTTP:** built-in `System.Net.Http.HttpClient`, async/await throughout (not RestSharp)
- **JSON:** `System.Text.Json` by default, `Newtonsoft.Json` as the compatibility layer
- **Source layout:** `src/DynaFetch/` with `Core/` (HttpClientWrapper, HttpRequest, HttpResponse, ErrorHandling), `Nodes/` (ClientNodes, RequestNodes, ExecuteNodes, JsonNodes), `Utilities/` (JsonHelper)
- **Node categories:** Client, Request, Execute, JSON
- **Testing:** NUnit + Moq in `tests/DynaFetch.Tests/` (unit tests plus live-API integration tests against JSONPlaceholder and httpbin.org), and manual verification in Dynamo with the sample graphs in `samples/`
- **Docs:** `docs/` (Markdown and PDF), `README.md`, `samples.md`, GitHub Pages landing page in `landing/`
- **Distribution:** GitHub releases (tags `vX.Y.Z`) and the Dynamo Package Manager via `pkg.json`
- **Repo:** GitHub, public, https://github.com/TheBIMsider/DynaFetch
- **IDE:** VS Code + C# Dev Kit

## Rules to Keep in Mind

- Do not rename nodes or reorder input ports. Published `.dyn` graphs, including the DynaWeb migration samples, depend on them.
- Execute nodes keep `application/json` as the default and honor a custom `Content-Type` set via `AddHeader`.
- Version lives in both `pkg.json` and `src/DynaFetch/DynaFetch.csproj`. Keep them in sync.
- A node change needs matching updates to `docs/`, `samples/`, and the landing page where relevant.
- Never copy DynaWeb (AGPL) code. Keep the attribution and BSD-3-Clause notices.

## Quick Commands

- **Build**: `dotnet build DynaFetch.sln`
- **Run tests**: `dotnet test tests/DynaFetch.Tests` (integration tests need internet access)
- **Feature branch**: `git checkout -b feature/<slug> && git push -u origin feature/<slug>`
- **Open PR**: `gh pr create --base main`
- **Convert Docs**: Use global CLI `markitdown input_file -o output.md` to parse raw docs (.pdf, .xlsx, .docx) before ingestion
