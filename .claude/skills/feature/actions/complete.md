# Complete Action

1. Confirm all goals in current-feature.md are met and testing is done -- do not proceed if anything is unresolved
2. GitHub PR workflow:
   a. Ensure all changes are committed on the feature branch
   b. Push the feature branch to GitHub: `git push origin <branch>` (or `git push` if upstream is already set)
   c. Create a PR targeting `main` using `gh pr create` -- include a summary of what was built and a test checklist
   d. Share the PR URL with the user and wait for them to confirm it has been approved and merged
3. Post-merge cleanup (after user confirms the PR is merged):
   a. Switch to `main` and pull: `git checkout main && git pull`
   b. Delete the local feature branch: `git branch -d <branch>`
   c. Delete the remote feature branch: `git push origin --delete <branch>`
   d. Confirm only `main` remains
4. Release check -- ask the user: "Does this feature ship in a new release?"
   - If no (docs, tests, internal cleanup, or work being batched for a later release): skip to step 5
   - If yes: walk through the Release Checklist in `context/coding-standards.md` and report which items are done and which remain. Do not tag, publish, or upload anything without explicit confirmation. Specifically confirm:
     a. Patch vs. minor version -- patch for fixes, minor for new features or notable behavior changes
     b. Version updated in both `pkg.json` (the `version` field and the assembly reference line) and `src/DynaFetch/DynaFetch.csproj` (`Version`, `AssemblyVersion`, `FileVersion`)
     c. `bin/DynaFetch.dll` rebuilt, and `docs/` (Markdown and PDF), `samples/`, `README.md`, and the landing page refreshed for any changed nodes
     d. Release notes drafted: what changed, files changed, migration impact (state clearly if existing graphs are unaffected), and credit to any reporter
     e. GitHub release with tag `vX.Y.Z`, then the Dynamo Package Manager update -- both are the user's step to perform or explicitly approve
   - Include the version in the History entry in step 5
5. Reset current-feature.md:
   - Change H1 back to `# Current Feature`
   - Set Status to "Idle"
   - Clear Goals and Notes sections (keep the section headings and placeholder comments)
   - Add a one-line feature summary to the END of History in the format: `- YYYY-MM-DD: [Feature name] -- brief description (completed)`
6. Confirm completion and show the history entry that was added

Note: this project has no `docs/session_docs/` recap/roadmap deliverables. `current-feature.md`'s History log is the sole record of what shipped.
