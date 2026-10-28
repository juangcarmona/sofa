# transition-work-item: GitHub Issues

Lighthouse owns issue actions, stage results, gates and workflow-state labels. SoFa does not model transitions with `status:*` labels.

For an owner-requested stage, reread the issue and its current Lighthouse evidence, verify the requested action is eligible under the installed Lighthouse contract, and add exactly one request label:

```bash
gh issue view <number> --json state,labels,comments
gh issue edit <number> --add-label "<owner-requested-stage>"
```

Supported request labels are defined by the installed Lighthouse router. Do not add a request label for a stage that Lighthouse already dispatched, remove Lighthouse result or reservation labels, or infer a passed gate from a green workflow check. Verify the router receipt and authenticated stage result after dispatch.

## Projects single-select

```bash
gh project item-edit --id <item-id> --field-id <field-id> \
   --project-id <project-id> --single-select-option-id <option-id>
```

All four identifiers are per-project and are resolved during adoption.

## Closing

Integration and issue closure follow Lighthouse's completion contract and explicit owner merge decision. GitHub's open/closed state is authoritative; do not add a replacement `status:done` label.
