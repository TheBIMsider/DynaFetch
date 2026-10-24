# Complexity Action

An optional, opt-in check for over-engineering. Not part of the standard review → complete path -- run it manually (`/feature complexity`) on features where the diff feels heavier than the goal warranted, or where you suspect a monolith is forming.

1. Read current-feature.md to understand the goals
2. Read all files modified for this feature
3. For each modified file, check against `context/coding-standards.md` (File Responsibilities, Dynamo / ZeroTouch Rules, Making Changes):
   - ⚠️ Line-count vs. goal -- does the size of the change match the size of the problem? Flag code that could plausibly be a fraction of its current length
   - ⚠️ Premature abstraction -- helpers, interfaces, base classes, or option objects built for one caller or one case
   - ⚠️ Monolith risk -- a file or class absorbing responsibilities that belong elsewhere per the layering (`Nodes/` are thin Dynamo-facing wrappers; HTTP logic lives in `Core/`; JSON logic in `Utilities/JsonHelper.cs`)
   - ⚠️ Duplicate logic -- new code that re-implements something already available elsewhere (note the custom Content-Type block is repeated across POST/PUT/PATCH in `ExecuteNodes.cs` -- flag new duplication, or consolidation chances, in that area)
   - ⚠️ Speculative generality -- flags, options, or branches for cases the current feature doesn't need (e.g. building a plugin or retry framework when one fix was asked for)
   - ⚠️ Compact-but-unreadable code -- logic golfed into unreadability in the name of being "simple"
4. For each flagged item, state the concrete simpler alternative (not just "this could be simpler" -- show the smaller shape)
5. Final verdict: Clean, or list specific items to simplify with the suggested reduction
6. Recommend next steps based on what was found:
   - If items were flagged that are reuse/efficiency/dead-code cleanups (not architecture violations) -- recommend running `/simplify` to apply them
   - If items were flagged that look like correctness risks introduced by the complexity itself (e.g. duplicated logic that could drift, error handling lost in an abstraction) -- recommend running `/code-review`
   - If nothing was flagged, say so and skip the recommendation

Skip items that are already covered by an explicit goal or Notes entry in current-feature.md (e.g. a genuinely reusable component was part of the ask) -- this action is about unrequested weight, not every abstraction.
