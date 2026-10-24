# AI Interaction Guidelines

## Communication

- Be concise and direct -- no corporate speak, no unnecessary positivity
- Explain non-obvious decisions briefly
- Ask before making architectural changes or anything that touches multiple files
- Do not add features not explicitly requested
- Do not delete or rewrite files without confirmation
- If something isn't working after 2-3 attempts, stop and explain the problem -- do not keep trying random fixes
- Ask for clarification if requirements are unclear before writing code
- Write code and explanations a beginner could follow: step-by-step, with the first and last few lines of any code being replaced so the location is easy to find in VS Code, then the new code. Code should be clear enough that someone could update it in 6 days, 6 months, or 6 years

---

## Workflow

Every feature or fix follows this order:

1. **Document** -- Describe the feature/fix in `@context/current-feature.md` before writing any code
2. **Branch** -- Create a feature branch from `main` (e.g. `feature/short-slug`) and push it to GitHub with `git push -u origin <branch>`
3. **Read** -- Read the relevant section of the actual current file before suggesting changes. Do not assume structure from memory.
4. **Implement** -- Make the change using targeted edits (see Coding Standards)
5. **Test** -- Run `dotnet build` and `dotnet test`, and for node changes verify in Dynamo with a sample graph. Report results before moving to the next change.
6. **Iterate** -- Fix issues if found. One change at a time.
7. **PR** -- Push all commits, open a pull request targeting `main` via `gh pr create`, share the PR URL
8. **Merge & Clean up** -- After the PR is approved and merged: pull `main`, delete the local feature branch (`git branch -d`), delete the remote branch (`git push origin --delete`)

Releases are a separate step that follows the Release Checklist in Coding Standards: version bump in `pkg.json` and the csproj, rebuild, docs and samples refreshed, GitHub release with notes, Dynamo Package Manager update.

---

## Session Structure

`@context/current-feature.md`'s History log (append-only, one line per completed feature) is the record of what shipped. Do not create new numbered roadmap or session recap files unless asked.

---

## Code Changes

- Make minimal, targeted changes -- do not refactor unrelated code unless explicitly asked
- Do not add "nice to have" features beyond the stated scope
- Preserve existing patterns in the codebase
- One change at a time -- test before moving to the next
- Treat node names, input port order, and public method signatures as a published API: do not change them without explicit approval

---

## Code Review

Review AI-generated code regularly, especially for:

- Breaking changes to published nodes (renamed nodes, reordered or retyped ports) that would break users' existing `.dyn` graphs, including the DynaWeb migration samples
- HTTP behavior regressions (default Content-Type, header handling, auth, timeouts, redirects) -- changes to `HttpRequest` and `ExecuteNodes` affect every user
- Security (credentials, tokens, and API keys must never be logged, hardcoded, or committed; test credentials stay out of the repo)
- Error handling (helpful user-facing messages, no unhandled exceptions escaping into Dynamo, no silent failures)
- Async misuse (blocking on `.Result`/`.Wait()`, missing `await`, unobserved tasks)
- Licensing and attribution (no DynaWeb code copied; attribution notices intact)
- Pattern consistency (does this match how the rest of the codebase handles the same problem?)
