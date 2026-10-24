# Test Action

1. Read current-feature.md to understand what was implemented
2. Generate a test checklist for this feature. Testing has two layers: the automated NUnit suite in `tests/DynaFetch.Tests` (`dotnet test`; integration tests need internet access), and manual checks inside Dynamo with the relevant sample graphs, since the suite cannot exercise the Dynamo UI
3. Checklist should cover:
   - Build and automated tests: `dotnet build DynaFetch.sln` succeeds, `dotnet test tests/DynaFetch.Tests` passes, and a new or updated test covers the change where practical
   - Happy path: the feature works end to end in a Dynamo graph (load the package build, run the relevant node or sample)
   - Error cases: null/empty/malformed URL, unreachable host, timeout, non-success status codes (4xx/5xx), invalid JSON -- each should give a clear, actionable message and never an unhandled exception in the graph
   - Edge cases relevant to this feature (e.g. custom vs. default Content-Type, header case-insensitivity, empty body, list inputs and lacing, large responses, file uploads)
   - Backward compatibility: existing sample graphs in `samples/` (and the DynaWeb migration samples) still open and run with the same node names and port order
   - If auth is touched: Bearer token, API key, and custom header paths, and confirm credentials never appear in logs or error messages
   - If docs/samples changed: links work, node names in `docs/` match the Dynamo library, and sample screenshots still match the graphs
   - Test against public APIs only (JSONPlaceholder, httpbin.org) -- no real credentials in samples or tests
4. Present the checklist clearly so it can be worked through in order
5. Note any cases that can only be verified in a real Dynamo install (for example node tooltips, port names and defaults, lacing behavior, and package loading), as opposed to `dotnet test`
