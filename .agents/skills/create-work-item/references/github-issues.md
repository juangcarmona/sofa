# create-work-item: GitHub Issues

```bash
gh issue create --title "<title>" --body "<body>" [--label "<applicable-classification-labels>"]
```

Do not add a `status:*` lifecycle label. A newly created open issue waits in the backlog without a Lighthouse request label. Add a Lighthouse action label (such as `triage`, `refine`, `propose`, or `implement`) in a separate owner-authorized operation when that stage should start. Omit `--label` when no classification applies; never add every available label.

Where the project uses an issue template, pass the body in the template's shape so the sections the project parses are present.
