---
name: feature
description: Manage current feature workflow - load, start, review, test, explain, complexity or complete
argument-hint: load|start|review|test|explain|complexity|complete
---

# Feature Workflow

Manages the full lifecycle of a feature from spec to completion.

## Working File

@context/current-feature.md

### File Structure

current-feature.md has these sections:

- `# Current Feature` - H1 heading with feature name when active
- `## Status` - Idle | Not Started | In Progress | Complete
- `## Goals` - Bullet points of what success looks like
- `## Notes` - Additional context, constraints, API quirks, decisions
- `## History` - Completed features (append only, oldest to newest)

## Task

Execute the requested action: $ARGUMENTS

| Action     | Description                                                  |
| ---------- | ------------------------------------------------------------ |
| `load`     | Load a feature spec or inline description into current-feature.md |
| `start`    | Verify goals are set, mark In Progress, begin implementation |
| `review`   | Check goals met, code quality, no scope creep                |
| `test`     | Test checklist: automated tests plus manual checks in Dynamo |
| `explain`  | Document what changed and why                                |
| `complexity` | Optional check for over-engineering and duplicated logic   |
| `complete` | Reset current-feature.md and add to history                  |

See [actions/](actions/) for detailed instructions.

If no action is provided, explain the available options.
