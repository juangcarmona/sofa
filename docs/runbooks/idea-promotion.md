# Explore an idea

Open **Explore or revisit an idea** from Issues. Save a title, intended outcome, scope and configured repository. Drafts stay in the existing protected control journal, survive refresh/restart and do not start agents or workflows. Abandoning a draft retains its content without writing to GitHub.

The single-instance draft store retains at most 25 ideas, including abandoned and promoted ideas. At capacity, creating a new draft is blocked; existing drafts and receipts remain readable. There is no automatic deletion or retention cleanup in this increment.

Choose **Review creation** to verify the connected account and separate issue-write permission, inspect potentially related open and closed issues and review the exact title/body. Confirm only after checking that no candidate is equivalent. Equivalent normalized titles block creation; linking an existing issue requires read authority and never creates another issue. Saving edits invalidates the previous review.

The source check scans up to ten pages of 100 records, including pull-request records. Incomplete or unavailable coverage blocks creation rather than claiming no duplicates. Semantic equivalence still needs operator judgement; GitHub does not provide atomic uniqueness across independently created ideas.

Creation records a one-shot intent before sending. If the response is lost, refresh the draft and choose **Reconcile uncertain creation**. The stable marker and creating actor identify the source issue. Unmatched, multiple or unavailable results remain uncertain and never resend automatically, even after restart. Do not create a replacement draft to retry an uncertain effect.

Confirmed source identity stays attached to the idea. Open that issue to use the existing shared Lighthouse controls; promotion does not authorize a stage or execution. No new agent-assisted exploration capability is offered.

Checks cover create, link, denied permission, retained and abandoned drafts, and recovery after a lost response. The connected-user acceptance in #52 already covers this journey, so it does not need to be repeated. Publication and integration remain separate.
