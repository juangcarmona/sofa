---
name: adr
description: "Author an Architecture Decision Record from the shared template, assigning the next sequential number, status and date, and updating the index. Use when a decision has trade-offs a future contributor will need."
license: MIT
metadata:
  author: juangcarmona
  source: https://github.com/juangcarmona/agent-toolkit
---

# adr

## Purpose

One responsibility: record one significant decision as an Architecture Decision Record, so the _why_ and the road not taken survive the people who were in the room.

The template is the single source of the document's shape. Copy it; never hand-roll the sections.

## When to use

- When `propose` reaches a decision with lasting trade-offs.
- When `review` finds that a lesson has graduated into an architectural decision worth recording.
- Standalone, when a decision shapes the architecture, a dependency, a cross-cutting pattern, or anything hard to reverse: anything a future contributor would want the reasoning for.

## When not to use

- A routine implementation choice with no lasting trade-off. It belongs in the commit narrative.
- A temporary decision relevant only to delivering the current change. Keep it in the plan.
- Reversing an existing decision by rewriting it. Write a new record and mark the old one superseded.

Create an ADR when a decision is architecturally significant or has durable trade-offs that future contributors will need to understand. If the choice is local, easily reversible, or adequately explained by code and normal change history, do not create an ADR.

A plan explains how a particular change will be delivered; an ADR preserves a durable architectural decision and its rejected alternatives. Create an ADR for lasting architectural consequences even when the decision first appeared in a plan. Link the ADR and plan or change instead of copying the plan narrative into the ADR.

## Workflow

When step 8 applies, preserve this section 09 table structure:

```markdown
| Decision | ADR | Affected architecture |
| --- | --- | --- |
```

1. **Derive the title.** A short, present-tense phrase naming the decision ("Store sessions in the cache tier", not "Sessions"). It becomes the heading. _Done when:_ the title names a decision, not a topic.
2. **Locate the records directory.** Default `docs/adr/`. Absent means ask where the project keeps decision records rather than creating a directory the team did not ask for. _Done when:_ the directory is known and exists.
3. **Compute the next number.** Take the highest existing four-digit prefix and add one, zero-padded. Numbers only ever increase and are never reused, not even a retired one. _Done when:_ the number is unique and next in sequence.
4. **Slug the filename.** Lowercase the title, hyphenate spaces, drop punctuation: `NNNN-<slug>.md`. _Done when:_ the filename carries the number and the slug.
5. **Fill the template** from [`references/template.md`](references/template.md). Complete the frontmatter with a stable lowercase `name`, one-sentence `description`, valid `status`, `date`, and any known `deciders` and `tags`, then complete every body section: Context (neutral facts, why a decision is needed), Decision (active voice, "We will …"), Consequences (positive, negative, neutral), Alternatives considered (each with a one-line reason for rejection), References. _Done when:_ no placeholder text remains anywhere in the document.
6. **Set the metadata.** Status `Proposed` and today's date for a new record, unless the human states the decision is already accepted. `deciders` and `tags` are optional; include them when known and use empty lists when they are not. The metadata belongs only in frontmatter so tools can read it without parsing the body. _Done when:_ status and date are set from the vocabulary below, and optional metadata is either populated or represented by an empty list.
7. **Write the file**, then **update the ADR index**: a row carrying the number, the title linked to the file, the status and the date, keeping the table ordered by number. No ADR index in the project means say so rather than inventing one. _Done when:_ the file exists and the ADR index has a matching row, or its absence is reported.
8. **Update the architecture decision index when applicable.** If the ADR is `Accepted` and `docs/architecture/09-architecture-decisions.md` exists, add or update its row using `Decision`, `ADR`, and `Affected architecture`. Identify the relevant arc42 sections or architectural areas. Keep status, context, rationale, alternatives, consequences and history only in the ADR. Do not add a `Proposed` ADR as current architecture. When the accepted ADR supersedes another decision, make section 09 point to the ADR representing the current architecture. Do not modify other arc42 documents; reflecting the resulting system shape belongs to the architecture-maintenance workflow. _Done when:_ an accepted architectural decision is indexed without duplicated ADR content, or section 09 is absent or not applicable and remains unchanged.
9. **Report** the created path and the assigned number. _Done when:_ the caller can link the record.

### Status vocabulary

Drawn from a fixed set. Never invent a value.

| Status                   | Meaning                                        |
| ------------------------ | ---------------------------------------------- |
| `Proposed`               | Under discussion; the default for a new record |
| `Accepted`               | Agreed and in effect                           |
| `Rejected`               | Considered and declined, kept for the record   |
| `Deprecated`             | No longer applies, and not replaced            |
| `Superseded by ADR-NNNN` | Replaced by a later decision; link it          |

Records are immutable in spirit. To reverse a decision, write a new record and mark the old one superseded, never rewrite history. The date records when the status last changed.

## Validation

- The filename carries a zero-padded, unique, next-in-sequence number.
- The document starts with YAML frontmatter containing a stable lowercase `name` and a one-sentence `description`.
- The document has its heading, valid frontmatter, and all five sections in order, with no leftover placeholders.
- The status is a valid value from the vocabulary and the date is `YYYY-MM-DD`.
- `deciders` and `tags` are YAML lists when present; either may be an empty list.
- Alternatives considered names at least the main rejected option, with a reason: this is what separates a decision record from a commit message.
- The index has a matching row, or the project's lack of an index is reported.
- When section 09 exists, it indexes accepted architectural decisions using only `Decision`, `ADR`, and `Affected architecture`; proposed decisions are absent, and an entry for a superseded decision points to the ADR representing the current architecture.

## Common pitfalls

| Pitfall | Fix |
| --- | --- |
| Hand-rolling the sections | Copy `references/template.md`. It is the single source of the document's shape |
| Reusing a number, including a retired one | Numbers only ever increase |
| Inventing a status value | Use the fixed vocabulary, `Superseded by ADR-NNNN` included |
| Rewriting an existing record to reverse it | New record, old one marked superseded. The history stays readable |
| Creating `docs/adr/` because it was missing | Ask where the project keeps decision records |
| Inventing an index the project does not have | Report its absence instead |
| Titling the record with a topic | The title names the decision, in present tense |
| Leaving Alternatives considered empty | Without the rejected option and its reason, this is a commit message |
| Leaving template placeholder text in place | Every section is filled before the file is written |
| Recording a routine implementation choice | Use an ADR only for significant decisions or durable trade-offs |
| Treating a plan as the ADR | Keep delivery detail in the plan; link a durable ADR without copying it |
| Adding a proposed ADR to arc42 section 09 | Index it only after acceptance |
| Copying ADR details into arc42 section 09 | Keep section 09 to decision, ADR link and affected architecture |
