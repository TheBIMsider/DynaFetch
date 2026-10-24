# Start Action

1. Read current-feature.md -- verify Goals are populated
2. If Goals are empty, stop: "Run /feature load first"
3. Set Status to "In Progress"
4. Create and check out a feature branch derived from the H1 heading (e.g. `feature/short-slug`).
5. Push the branch to GitHub and set the upstream: `git push -u origin <branch>`
6. Read the relevant sections of the actual current code files before touching anything -- do not assume structure from memory
7. List the goals, then implement them one at a time:
   - Make one focused change
   - Stop and verify it works (`dotnet build`, relevant `dotnet test`, and a check in Dynamo for node changes) before moving to the next goal
   - Report results before proceeding

Rules:

- branch from `main` -- ensure `main` is up to date before branching
- keep the work inside the current feature scope
