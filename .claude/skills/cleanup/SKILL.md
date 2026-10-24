---
name: cleanup
description: Clean up DynaFetch project housekeeping tasks (add "run" to execute fixes)
argument-hint: run|check
---

Review the codebase for cleanup tasks:

1. Make sure the history in @context/current-feature.md is in order from oldest to newest
2. Find leftover debug output (`Console.WriteLine`, `Debug.WriteLine`, `Trace`) in `src/DynaFetch/**/*.cs` -- flag any that are debug-only noise
3. Find unused private/internal methods, fields, usings, and classes in `src/DynaFetch/` (public members are the node surface -- do not flag them as unused)
4. Check for stale TODO or FIXME comments in any core file
5. Check that all context files match actual project state -- version numbers, feature list, node categories, file structure.6. Check that no API key, token, password, or Authorization value is committed in source, tests, `samples/*.dyn`, `docs/`, or `README.md`
7. Check that the version in `pkg.json` (both the `version` field and the assembly reference line) matches `Version`/`AssemblyVersion`/`FileVersion` in `src/DynaFetch/DynaFetch.csproj` and the current version documented in `@context/project-overview.md` and `CLAUDE.md`
8. Check `@context/current-feature.md` Status is accurate -- if no feature is in progress it should say "Idle"
9. Check that docs match the code: node names in `docs/Node-Library.md` and `docs/Advanced-Node-Library.md` exist in `src/DynaFetch/Nodes/`, and every public node is documented. Also flag any `docs/*.md` whose matching `.pdf` is likely out of date
10. Check `.gitignore` covers build output (`obj/`, and `bin/` except the intentionally committed `bin/DynaFetch.dll`) -- `git status` currently shows tracked `obj/` files as modified, which suggests they were committed in the past and should be removed from the index

**Mode: $ARGUMENTS**

If no argument or argument is "check":

- Only report findings, do not modify anything
- List what WOULD be cleaned up

If the argument is "run" or "fix":

- First, report all findings with numbered items
- Then ask: "Which items would you like me to fix? (enter numbers like 1,3,5 or 'all' or 'none')"
- Wait for user response before making any changes
- Only fix the items the user specifies
- Report what was changed
