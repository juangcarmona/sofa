---
id: QR-MOBILE-OPERATIONS
type: quality-requirement
title: Complete the operational journey from desktop and phone
status: draft
quality-attribute: usability
applies-to: [UC-ACCESS-FACTORY, UC-OPERATE-ISSUE]
uses-terms: [TERM-WORK-COMMAND, TERM-EXECUTION-RUN]
verification:
  - scenario: On a real phone connected through LAN or VPN, the owner signs in, opens a real issue, requests an eligible command, posts a comment and reads the source result in SoFa.
  - scenario: Keyboard-only interaction reaches issue context, action explanations, confirmation and outcomes with visible focus and no mutation on refresh.
  - scenario: Reconnecting after a command submission shows the existing request and any correlated execution rather than submitting the action again.
---

## Requirement

The complete authenticated issue journey MUST be usable on a desktop and phone over the permitted private network, including sign-in, source authorization, command explanations, conversation and results. Keyboard users MUST be able to perform the same actions. Refresh, navigation and reconnect MUST NOT implicitly submit or repeat work.

## Measurement

Demonstrate the journey on a real issue using desktop keyboard navigation and a real phone on LAN or VPN. Retain evidence of the acting identity, source command/run/comment association, rejection and uncertain outcomes. A narrow browser screenshot alone does not prove mobile identity-provider redirects, network reachability or source execution.
