# Explain Action

1. Read current-feature.md to understand what was implemented
2. List every file that was created or modified for this feature
3. For each file:
   - Show the file path
   - Give a 1-2 sentence explanation of what changed and why
   - Highlight any key functions or patterns added
4. End with a brief summary of how the pieces fit together -- the flow from a Dynamo node call (`Nodes/`) through `HttpRequest` / `HttpClientWrapper` to the HTTP call, and back through `HttpResponse` / `JsonHelper` to Dynamo types, where relevant

## Output Format

## Files Changed

**filename.cs / .md / .dyn** (new | modified)
What changed and why. Key methods or patterns added.

## How It All Connects

Brief summary of the request/response flow between the changed files, and how a Dynamo user would see the change.
