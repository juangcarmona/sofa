# wip-query: GitHub Projects

```bash
# Lighthouse implementation requests and PR projections: paginate and deduplicate issues
gh issue list --state open --json number,title,labels --limit 1000 \
  | jq '[.[] | select(any(.labels[].name; . == "implement" or . == "pr-draft" or . == "pr-ready"))]'

# Projects field-based state: query all project items, then filter
gh project item-list <project-number> --owner <owner> --format json --limit 100
```

`gh issue list` is paginated and its default page can undercount active items. Page through all results before counting.

Count each issue once even when it has both an active Lighthouse implementation request and a PR projection. Exclude container types (epics, parents, or any type the project uses to group work) before comparing the total with the configured cap.

Where the lifecycle state lives in a Projects field rather than a label, query the project's items, filter on that field, exclude container items, and paginate before counting.
