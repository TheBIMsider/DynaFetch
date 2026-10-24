# Coding Standards

## Language & Runtime

- C# targeting .NET 8, Dynamo 3.0+ ZeroTouch package
- Nullable reference types and implicit usings as already configured in `DynaFetch.csproj` -- do not change project-wide settings without discussion
- `System.Net.Http.HttpClient` for all HTTP (no RestSharp or other HTTP libraries)
- `System.Text.Json` by default; `Newtonsoft.Json` only where compatibility requires it (the existing `JsonHelper` layer)
- New NuGet dependencies need explicit approval -- every dependency ships inside a user's Dynamo session
- Stay compatible with Dynamo's loaded assemblies: be careful with package versions that Dynamo already loads (for example Newtonsoft.Json) to avoid version conflicts

## Style

- `async/await` throughout -- never block with `.Result` or `.Wait()`. At the ZeroTouch boundary (where a node must return synchronously), keep the sync-over-async handling in one place and follow the pattern already used
- `var` only when the type is obvious from the right-hand side
- Expression-bodied members and pattern matching where they aid readability, not to be clever
- `using` statements/declarations for disposables; reuse a single `HttpClient` per `HttpClientWrapper` rather than creating one per request
- No commented-out code left in the codebase unless it explains a non-obvious decision
- Beginner-friendly: prefer clear, explicit code over dense one-liners

## Naming

- Classes, methods, properties, public members: `PascalCase`
- Parameters and local variables: `camelCase`
- Private fields: `_camelCase`
- Interfaces: `IPascalCase`; async methods end in `Async` where they return `Task`
- Files named after the primary class they contain (`HttpRequest.cs`)
- Node class and method names are user-facing (they appear in the Dynamo library) -- choose clear, DynaWeb-familiar names and keep them stable

## Comments

- XML doc comments (`///`) on every public class and method, since these drive Dynamo node tooltips: `<summary>`, `<param>`, `<returns>`, and a short usage example for non-obvious nodes
- Inline comments on anything non-obvious, especially HTTP quirks (Content-Type handling, header ordering, redirect behavior) and Dynamo-specific workarounds
- Comments explain *why*, not just *what*
- Readability standard: someone unfamiliar with this codebase should be able to understand any class or method without needing project history
- Keep stage/session numbers out of code comments -- they go stale. Reference behavior, not project history

## File Responsibilities

| Path | Owns |
|---|---|
| `src/DynaFetch/Core/HttpClientWrapper.cs` | Connection management, client settings, persistent auth |
| `src/DynaFetch/Core/HttpRequest.cs` | Request building and configuration (headers, parameters, body, files); builds the `HttpRequestMessage` |
| `src/DynaFetch/Core/HttpResponse.cs` | Response handling and parsing |
| `src/DynaFetch/Core/ErrorHandling.cs` | Validation and exception handling, user-facing error messages |
| `src/DynaFetch/Nodes/ClientNodes.cs` | Dynamo-facing Client nodes |
| `src/DynaFetch/Nodes/RequestNodes.cs` | Dynamo-facing Request nodes |
| `src/DynaFetch/Nodes/ExecuteNodes.cs` | Dynamo-facing Execute nodes (GET, POST, PUT, DELETE, PATCH) |
| `src/DynaFetch/Nodes/JsonNodes.cs` | Dynamo-facing JSON nodes |
| `src/DynaFetch/Utilities/JsonHelper.cs` | Serialization and JSON-to-Dynamo type conversion |
| `tests/DynaFetch.Tests/` | NUnit + Moq tests: `BasicTests` plus `Integration/` |
| `pkg.json` | Dynamo Package Manager manifest (version, description, assembly reference) |
| `docs/`, `samples/`, `landing/` | User documentation (Markdown + PDF), example graphs, GitHub Pages site |

Keep the layering: `Nodes/` are thin Dynamo-facing wrappers; real HTTP and JSON logic lives in `Core/` and `Utilities/`. Do not move logic between these without a clear reason and explicit discussion.

## Dynamo / ZeroTouch Rules

- Public static methods on public classes in `Nodes/` become Dynamo nodes. Anything public is part of the user-facing surface -- make helpers `internal` or `private`
- Renaming a node, or changing the order, names, or types of its input ports, breaks existing user graphs. Add new overloads or optional parameters (with defaults) instead of changing existing signatures
- Use Dynamo-friendly types at the boundary: `string`, `double`/`int`, `bool`, `List<object>`, `Dictionary<string, object>`. Convert JSON to native Dynamo lists/dictionaries via `JsonHelper`
- Use `[MultiReturn]` and `[IsVisibleInDynamoLibrary(false)]` / `[NodeCategory]` attributes consistently with the existing nodes
- Nodes must never throw unhandled exceptions into the graph: catch, and return or throw a clear, actionable message via `ErrorHandling`

## HTTP Rules

- Default `Content-Type` is `application/json` for string/JSON bodies, but a custom `Content-Type` set with `AddHeader` must be honored (needed for `application/merge-patch+json` and similar). Preserve both behaviors
- Header names are case-insensitive -- compare accordingly
- Always set sensible timeouts and surface timeout errors with a clear message
- Dispose `HttpResponseMessage`, `HttpContent`, and streams properly, especially for multipart file uploads
- Never log request bodies or headers that may contain `Authorization`, API keys, or tokens

## Error Handling

- Validate inputs up front (null/empty URL, malformed URL, null body where required) with clear plain-English messages that tell the Dynamo user what to fix
- Catch `HttpRequestException`, `TaskCanceledException` (timeout), and `JsonException` explicitly; do not use blanket `catch (Exception)` that hides the cause
- Include status code and a trimmed response body in error output for non-success responses; never swallow errors silently
- Full technical detail goes to the exception; the user-facing message stays short and actionable

## Security

- No credentials, tokens, or API keys committed to the repo -- including in tests, `.dyn` sample graphs, docs, and screenshots. Sample graphs use placeholder tokens and public test APIs
- Integration tests that need secrets read them from environment variables, never from source
- Do not disable TLS certificate validation
- Treat all response data as untrusted; do not evaluate or execute it

## Testing

- NUnit + Moq in `tests/DynaFetch.Tests/`. Unit tests mock HTTP responses; integration tests hit public APIs (JSONPlaceholder, httpbin.org) and need internet access
- Every bug fix gets a regression test where practical (for example, a custom Content-Type test for the v1.0.8/v1.0.9 fix)
- Node changes also need a manual check in Dynamo with the relevant sample graph, since the test suite cannot exercise the Dynamo UI
- Run `dotnet build DynaFetch.sln` and `dotnet test tests/DynaFetch.Tests` before opening a PR

## Making Changes -- Core Rules

**Always read the current file before editing.** Don't assume structure from memory, especially for `HttpRequest.cs` and `ExecuteNodes.cs`, which many nodes depend on.

Use targeted edits over full-file rewrites:
- Show enough surrounding context that the location of a change is unambiguous
- Avoid full-file rewrites on files with many callers (`HttpRequest.cs`, `ExecuteNodes.cs`, `JsonHelper.cs`) -- a small wrong assumption there breaks every node that depends on it

Change order:
1. Read the relevant section of the current file
2. Make one focused change
3. Build and run the relevant tests before making the next change
4. Report results before proceeding

## Release Checklist

DynaFetch is distributed software with versioned releases (patch for fixes, minor for new features or notable behavior changes). Before tagging a release:

- [ ] `dotnet build` and `dotnet test` pass
- [ ] Version updated in both `pkg.json` (including the assembly reference line) and `src/DynaFetch/DynaFetch.csproj` (`Version`, `AssemblyVersion`, `FileVersion`)
- [ ] No node renamed or port reordered, or the break is documented with a migration note
- [ ] `bin/DynaFetch.dll` rebuilt for the release package
- [ ] Verified in Dynamo with the relevant sample graphs
- [ ] `docs/` (Markdown and PDF), `samples/`, `samples.md`, `README.md`, and the landing page updated for any new or changed nodes
- [ ] No secrets or tokens in the diff, samples, or docs
- [ ] GitHub release created with tag `vX.Y.Z` and release notes (what changed, files changed, migration impact, credit to reporters)
- [ ] Dynamo Package Manager updated with the new version
