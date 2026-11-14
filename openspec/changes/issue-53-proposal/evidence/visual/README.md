# Configuration view captures

These screenshots show synthetic fixture data through the repository detail page, not live repository observations. They were captured against the PR #197 implementation candidate using the browser fixture server and explicit response stubs.

| Capture | Viewport | Theme | Fixture |
| --- | --- | --- | --- |
| `configuration-light-desktop.png` | 1280 × 1000 | Light | Declared Lighthouse CLI `0.12.68` compared with an explicitly selected synthetic `0.12.66` declaration; includes source links and partial-source reasons. |
| `configuration-dark-desktop.png` | 1280 × 1000 | Dark | Same fixture and comparison. |
| `configuration-light-390px.png` | 390 × 844 | Light | Same fixture and comparison. |
| `configuration-dark-390px.png` | 390 × 844 | Dark | Same fixture and comparison. |

The synthetic projections use fixed 40-character commits, repositories `example/repository` and `example/other`, a connected-account fixture scope, and `package.json` source links. The declared values are `0.12.68` and `0.12.66`. They contain no credentials or real fleet facts. The capture was produced by opening the repository page, selecting **Inspect configuration**, inspecting the second repository, choosing a baseline, then capturing the displayed state. A focused source link stayed focused through refresh. Only GET requests were sent to the fixture APIs. Axe reported zero violations and the document had no horizontal overflow at either viewport.

The captures document layout and explanation text only. They are not physical-device acceptance, schema-validation evidence, a complete eight-state browser suite, or a test of the live GitHub integration. See [the source receipt](../schemas/lighthouse-0.12.65/README.md) for the separate schema provenance boundary.
