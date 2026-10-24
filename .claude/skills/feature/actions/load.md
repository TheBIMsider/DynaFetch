# Load Action

1. Check $ARGUMENTS (after "load"):
   - If it looks like a filename (single word, no spaces): Look for `context/features/{name}.md`
   - If it's multiple words: Use as inline feature description and generate goals from it
   - If empty: Error -- "load requires a spec filename or feature description"

2. Update current-feature.md:
   - Update H1 heading to include feature name (e.g., `# Current Feature: User Groups`)
   - Write goals as bullet points under `## Goals` -- be specific, each goal should be independently verifiable
   - Write any additional context, constraints, or API quirks relevant to this feature under `## Notes`
   - Set Status to "Not Started"

3. Confirm spec is loaded and show the feature summary
