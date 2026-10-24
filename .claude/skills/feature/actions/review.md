# Review Action
1. Read current-feature.md to understand the goals
2. Review all code/doc changes made for this feature
3. Check for:
   - ✅ Goals met -- each goal from current-feature.md is implemented and working
   - ❌ Goals missing or incomplete
   - ⚠️ Code quality -- efficiency, missing error handling, async misuse (`.Result`/`.Wait()`, missing `await`), undisposed `HttpResponseMessage`/`HttpContent`, XML doc comments missing on new public members
   - ⚠️ Breaking changes -- any renamed node, or reordered/renamed/retyped input port, which would break users' existing `.dyn` graphs (including the DynaWeb migration samples). Prefer new overloads or optional parameters
   - ⚠️ HTTP behavior -- default `application/json` Content-Type preserved when no custom one is set, custom Content-Type honored, headers compared case-insensitively
   - ⚠️ Version/doc drift -- if behavior or nodes changed: `docs/`, `samples/`, `samples.md`, `README.md`, and the landing page updated; `pkg.json` and `DynaFetch.csproj` versions in sync if a release is planned
   - 🚫 Scope creep -- code or changes beyond what the goals describe
   - 🔒 Security -- no credentials, tokens, or API keys in code, tests, samples, or docs; no logging of Authorization headers; TLS validation not disabled
   - ⚖️ Licensing -- no DynaWeb (AGPL) code copied; attribution notices intact
   - 🔁 Regressions -- what existing nodes or sample graphs could this break
   - 🧪 Testing -- a regression test where practical, and `dotnet test` passing; skip for trivial/cosmetic changes
4. Final verdict: Ready to complete or needs changes (list specific items to fix)
