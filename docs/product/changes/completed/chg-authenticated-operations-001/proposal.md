
<!-- Infrastructure host names are redacted in this historical record. -->
# Authenticated factory operations interview

This is the working memory for `CHG-AUTHENTICATED-OPERATIONS-001`. Its semantic delta lives in [change.md](change.md) and the complete artifacts in `proposed/`; the accepted model remains untouched. Resume this same change. Product approval, apply and merge acceptance are separate actions.

## Scope of this sitting

Establish authenticated access and the first issue-operation journey, preserving the owner's broader factory objective. Prepare the product delta before changing current architecture or implementing controls. The owner requested questions one at a time and authorized recording the need in the backlog.

## Source evidence

- SoFa main and origin HEAD were checked at `ae846c7faba9b92fcf3ce2bf7e152a3b803050d2` on 2026-10-26. The original checkout is on an older architecture branch and contains authored untracked assets; it is preserved. This change uses an independent worktree and the repository's numbered branch convention.
- The full accepted ProductShape model has 52 artifacts and validated without errors or warnings. There were no live Product Changes before this draft.
- [Issue #45](https://github.com/juangcarmona/sofa/issues/45) is closed and [PR #46](https://github.com/juangcarmona/sofa/pull/46) is merged. The read-only server in `src/web/server.mjs` accepts only GET/HEAD, and the collector in `src/observation/github.mjs` reads the latest 20 runs and first 100 open issue records. It has no operational issue-detail or conversation API.
- [Issue #47](https://github.com/juangcarmona/sofa/issues/47) retains its original authenticated Refine scope and [refinement report](https://github.com/juangcarmona/sofa/issues/47#issuecomment-6410966096). The report refers to run `40287921209` and source `ae846c7`. No body, comment or lifecycle label was changed during this interview. The installed Lighthouse proposal contract checks a task digest, so changing the refined scope requires fresh refinement.
- [Issue #48](https://github.com/juangcarmona/sofa/issues/48) records the related operational issue journey in English as `status:backlog`. Its delivery is not implied by #47's Ready verdict.
- ADR-0006 is accepted: GitHub workflows and independent runtime execution continue without SoFa. `CON-TRUSTED-LAN` still restricts privileged control to loopback; the new mobile requirement needs the proposed change.
- The installed Lighthouse SDLC 0.13.0 router supports explicit Refine and Propose. Owner comments on `needs-info` may resume refinement. The dispatch interface has no client command identifier. These gaps require assessment in the shared authority, not a copied SoFa policy.
- DomusMind main matched origin at `b4f77ad01ca87d2de33682d24ea2b7eee9e4cb15`. Its accepted ADR-0002 keeps local authentication inside the monolith with account identity separate from household membership. The MCP/OpenIddict worktree contains uncommitted changes and a Proposed ADR-0012; it is not an accepted common identity service. Both checkouts were inspected without changes. DomusMind migration is excluded from this sitting.

## Decisions and exact answers

### Q1: Authentication for reading as well as operating

The owner was asked whether the new operational workspace should require sign-in for consultation or retain anonymous reading with sign-in only for actions. The recommendation was to authenticate the complete operational workspace.

Exact answer:

> Si, deberíamos autenticar... Y me planteo si debemos usar mi EntraID, el de la org de jgcarmona (que casi no lo usao para nada...) o un keycloak para todos mis servicios caseros (incluído este) u otro proveedor de auth...

Resolved effect: UC-ACCESS-FACTORY, CON-TRUSTED-LAN, BR-LAN-PRIVILEGE-BOUNDARY and QR-EXECUTION-SECURITY require authenticated operational reading and actions. No provider was chosen by this answer.

### Q2: Reuse or extract DomusMind identity

The owner asked:

> No, espera, a ver, Domusmind tenía autenticación, sabes? entonces... ¿Cómo lo hicimos con DomusMind? ¿Merece la pena replicarlo o nos sacamos la autenticaciónd e DomusMind a un servicio externo?

After the code and ADR inspection, the recommendation was an existing external provider for SoFa and optional future federation for DomusMind, preserving its standalone mode. The owner then said:

> Vamos a hacer con SoFa lo del proveedor, vale? y vamos a hacerlo de forma profesional, si?

Resolved effect: ACT-IDENTITY-PROVIDER and UC-ACCESS-FACTORY use an external authority. Extraction, replication and migration of DomusMind are not authorized by this answer. The request for professional execution motivates explicit security and failure acceptance; it is not an answer about specific session durations or approval of the entire product delta.

### Q3: Provider for the first SoFa deployment

The question offered Entra ID in the owner-managed jgcarmona tenant versus a home-hosted provider. The recommendation was Entra ID to avoid adding an identity deployment to [development host].

Exact answer:

> Entra ID de jgcarmona (recomendado)

Historical effect: Entra ID was selected for the first deployment, then explicitly reopened in Q4. Provider sign-in remains separate from GitHub account connection and action authority. No tenant or application registration has been changed.

### Q4: Reopen provider choice and examine the operating model

The owner redirected the interview before answering the active-execution comment question. Owner response (host names redacted where applicable):

> Pues mira, estoy discutiendo sobre esto en otra conversación, ¿vale? Sobre Lighthouse, sobre cuál es el estándar. Realmente creo que Entra, para una organización donde ya hay cosas Microsoft, guay. Pero, sin embargo, Keycloak te da muchísima versatilidad, te permite social login, ¿vale? Lo puedes alojar tú, puedes tener un único Keycloak para toda tu organización o toda tu familia, o puedes tener un Keycloak por aplicación o por sistema. Entonces, ¿qué me merece la pena tener en Sofa? Pues no lo tengo claro. Realmente creo que tiene sentido. A ver, en este caso, porque es para mí, tienen sentido los dos, tiene sentido Entra y tiene sentido Keycloak. El tema de Keycloak es, si yo quisiera acceder desde fuera de mi red local, tendría que exponer los dos servicios, tipo reverse proxy, etcétera. Cosa que no debería ser mucho problema, ¿vale? pero ya hay que protegerlo, configurarlo, mantenerlo, irme a mi VPS, en fin, todo eso. Vamos a dar una vuelta, no tomemos esta decisión así a la ligera, ¿vale?

Resolved effect: reopen provider selection without reversing authenticated workspace access or external identity. Neither vendor is selected. The complete Product Change remains draft. No infrastructure deployment or expansion of the first delivery to household administration is inferred.

The conversation “Diseñar la plataforma software” (01a16d29-8745-7482-9165-79093692f332), read on 2026-10-26, recommends OIDC interoperability, Keycloak as a self-hosted reference and connecting an existing provider as an alternative. It explicitly reports no contract changes or deployment. Its newest owner message considers shared family authentication. These are discussion evidence, not accepted Lighthouse contracts or authorization to choose Keycloak for SoFa.

Assessment to discuss: a provider interoperability contract and the selected deployment are separate decisions. A shared Keycloak can serve several application clients; separate realms or installations need a concrete isolation reason. Sharing authentication does not share application permissions. With VPN access, browser and server can reach private SoFa and identity endpoints without public application ingress; a public URL experience adds a separate exposure and recovery decision. Managed Entra reduces identity-service operations, while self-hosted Keycloak gives direct operational control and creates a shared dependency requiring backup and restoration. Changing providers later can require identity/account-link migration; interoperability does not promise a configuration-only migration.

Evidence: [Keycloak administration and identity brokering](https://www.keycloak.org/docs/latest/server_admin/index.html), [production configuration](https://www.keycloak.org/server/configuration-production), [reverse proxy boundaries](https://www.keycloak.org/server/reverseproxy), and [Entra redirect URI requirements](https://learn.microsoft.com/entra/identity-platform/reply-url). Network conclusions are deployment inferences, not tested SoFa configurations.

### Q5: Select shared Keycloak on [development host]

The owner supplied the following direction:

> Para SoFa, concretaría la dirección en **Keycloak compartido en [development host] para iniciar sesión**, más una conexión autorizada con GitHub para actuar en tu nombre. La integración con GitHub sigue siendo independiente del login.

Resolved effect: select shared Keycloak on [development host] for SoFa sign-in, with an independent authorized GitHub connection for source actions. This supersedes the initial Entra selection and the parked provider choice. No deployment, application registration or accepted architecture change is implied. Shared identity does not admit every household user to SoFa; the configured owner boundary remains unchanged. The original LAN/VPN delivery requirement still applies; this answer does not request public access or answer the previous VPN question.

Graph check: ACT-IDENTITY-PROVIDER and UC-ACCESS-FACTORY already require external identity, so their provider-neutral semantics remain valid. BR-WORK-AUTHORIZATION and FR-AUTHORIZED-ENTRY retain independent GitHub authority. CON-TRUSTED-LAN and QR-MOBILE-OPERATIONS retain LAN/VPN coverage. Placement, protocol integration and recovery belong to the upcoming ArcShape proposal, not new product artifacts.

### Q6: Active-work comments and infrastructure recovery

Owner response (host names redacted where applicable):

> OK...
>
> ¿Y cómo hacemos ahora que todavía no hay Keycloak en la máquina ni está configurado ni nada? ¿Y cómo hacemos también para todo ese tema de secretos, seguridad, etcétera, para que no se pierdan? ¿Esto nos lo llevamos a un repositorio privado aparte con nuestro Keycloak o lo tenemos en local? ¿Esto cómo lo vamos a hacer? Dale una buena pensada.

The opening OK is recorded as assent to the immediately preceding recommendation: allow ordinary comments during active execution, with a warning when source policy can invalidate pending results. It does not authorize deployment. The owner also requests a concrete infrastructure and secret-recovery proposal. FR-ISSUE-CONVERSATION now makes the warning explicit; provider-neutral access and shared source authority remain unchanged.

Read-only inspection found an existing private infrastructure repository and [development host] stack/data/secret conventions. The active runner mounts the host Docker socket, so host-secret isolation needs explicit remediation before shared identity use. The technical proposal is owned by Proposed ADR-0007; it is not accepted architecture or an implemented infrastructure service. Password-manager choice, off-host backup destination and recovery objectives remain unverified.

### Q7: Separate production identity from development

Owner response (host names redacted where applicable):

> Keycloak lo podemos poner en otra máquina, en [production host], por ejemplo... Digamos que [production host] es producción y [development host] es desarrollo

Resolved direction: use [production host] as the proposed production identity host and [development host] for development and repository execution. This updates Q5's initial [development host] placement while preserving shared Keycloak and independent GitHub authority. It is a topology direction, not evidence of deployment, available capacity or verified connectivity. Proposed ADR-0007 is revised in place because it has not been accepted.

The same-host runner VM is no longer a prerequisite for protecting Keycloak on [production host]. Instead, verify that [development host] runners have no production host administration, Docker API, filesystem or backup credentials. Runner hardening remains separate infrastructure work. The production SoFa service is recommended for [production host] too, so real GitHub tokens and production sessions do not reside on the development host; this is a proposed consequence to review, not an observed deployment. Development uses separate test identity and source credentials. Secret custody and off-host recovery remain unresolved and independent of either host.

### Q8: Close identity direction and complete the whole product proposal

Owner response (host names redacted where applicable):

> Vale, y a ti te digo, hemos tenido una larga conversación sobre este tema, la decisión ya está tomada. Vamos a tener un Keycloak en [production host], aún no sé cómo ni dónde, pero eso es indiferente, en producción, ¿vale? Durante el desarrollo lo que voy a hacer es que vamos a tener un contenedor dentro de la solución Aspire que me permite ir desarrollando y probando funcionalidad en desarrollo y poco más. Luego te he dado en el primer prompt de esta misma conversación un montón de funcionalidades y de ejemplos de funcionalidades que quiero que tenga el producto. Me has dicho lo primero: ¿qué hacemos con la autenticación? Vale, ya le hemos dado una vuelta, ¿y todo lo demás qué? Hay que seguir. Hay que documentar, hay que especificar y hay que dejar listo, ¿vale?

Resolved direction: production Keycloak on [production host]; development Keycloak in a container composed by the existing Aspire solution. Provider and development mode are closed decisions. Infrastructure sizing, secret custody and production installation remain delivery details, not blockers to product shaping. No implementation or complete Product Change acceptance is inferred.

The sitting now covers the original whole-product request. New proposed artifacts elaborate its stated capabilities: daily factory journey, repository discovery, intervention handling, project views, explainable health/configuration signals and idea promotion. ACT-FACTORY-OWNER is reused; ACT-ENGINEER and ACT-REQUESTER keep existing boundaries. No new multi-user, terminal, file-browser, scheduler or score requirement is introduced. Reference screenshots supply experience cues only, not data or policy. Full-product direction does not expand the first operational delivery.

Refreshed issue evidence: #47 now also has a blocked Propose report at [the source comment](https://github.com/juangcarmona/sofa/issues/47#issuecomment-6412062941) (run 40294746514). Its two unresolved inputs are identity/deployment and exact correlation. Preserve both reports and the existing issue body; related stories and a shared Lighthouse contract dependency resolve these inputs without invalidating the historical refinement. The current labels are ready and waiting-review, not execution authority. PR #46 remains the merged observation baseline; no #47 proposal PR was found in the latest PR listing.

Widened graph review: FR-BACKLOG-PROJECTION impact was inspected. UC-TRIAGE-BACKLOG is affected for explicit exploration/promotion and duplicate reconciliation. FR-PROJECT-DISCOVERY is affected for inventory provenance; FR-RUNTIME-HEALTH is affected for observable resource/capacity limits. Existing BC contexts, BR-GITHUB-SOURCE, BR-WORK-AUTHORIZATION, BR-CAPACITY-DEFAULTS and BR-HUMAN-DECISION-BOUNDARIES remain valid. FR-DELIVERY-EVIDENCE and JRN-ENGINEERING-EXECUTION remain the complete delivery authority; the daily journey complements them. QR-EXECUTION-RECOVERY's stale worker/fencing verification is corrected to the current observer responsibility. Legacy duplicate ACT-JUAN/ACT-FACTORY-OWNER and ACT-GITHUB/ACT-GITHUB-BACKLOG are preserved rather than mixed into this change as unrelated identity cleanup.

## Graph review

The CLI's impact output is structural reachability, not a semantic instruction. `prodshape impact CON-TRUSTED-LAN`, `prodshape impact BR-WORK-AUTHORIZATION` and `prodshape impact FR-AUTHORIZED-ENTRY` were inspected against the accepted graph.

| Candidate | Relationship and impact | Disposition |
| --- | --- | --- |
| CON-TRUSTED-LAN | Governs execution, delivery and runtime observations; FR-RECOVERABLE-EXECUTION derives from it. Its loopback restriction conflicts with phone operation. | Affected: change the permitted authenticated access boundary. |
| BR-LAN-PRIVILEGE-BOUNDARY, QR-EXECUTION-SECURITY | The old anonymous read boundary does not cover authentication for all operational reading. | Affected: retain anonymous denial of privilege and add authenticated workspace protections. |
| FR-AUTHORIZED-ENTRY, FR-WORK-UNIT-SHAPING | Both currently permit optional future controls; the requested journey requires supported Refine/Propose operations. | Affected: make explicit interaction required and source-authorized. |
| BR-WORK-AUTHORIZATION | Governs intake, shaping and execution; authorizes a future authenticated adapter through the source route. | Checked and unchanged: its authority ownership remains valid. The new command rule makes interaction semantics explicit. |
| BC-EXECUTION, BC-DELIVERY, BC-RUNTIME-OPERATIONS, FR-RECOVERABLE-EXECUTION | Structural neighbors of the LAN constraint describe independent source execution and evidence. | Checked and unchanged: identity and UI commands do not move execution ownership into SoFa. |
| BR-CAPACITY-DEFAULTS, BR-SCHEDULER-SAFETY, CON-FACTORY-RUNTIME | Govern source capacity, scheduling and optional SoFa availability. | Checked and unchanged: [development host] and repository controls retain enforcement. |
| UC-SHAPE-WORK-UNIT | A matching answer may resume a pending question within its grant. | Checked and unchanged: an explicit reply-and-continue action can realize this; ordinary comments must remain distinct. |
| JRN-ENGINEERING-EXECUTION, FR-DELIVERY-EVIDENCE | Describe the complete delivery journey, beyond the first operating slice. | Checked and unchanged: retain full-product direction; do not require complete delivery verification to ship the first command journey. |
| ACT-FACTORY-OWNER, ACT-ENGINEER, ACT-REQUESTER | Existing roles separate personal operation and repository authority. | Checked and unchanged: first deployment admits the configured owner; no new multi-user administration. |
| TERM-WORK-COMMAND, BR-EXPLICIT-WORK-COMMANDS | No accepted node separates user command intent from issue/execution/reservation/human wait. | Affected additions grounded in the original request. |
| UC-ACCESS-FACTORY, ACT-IDENTITY-PROVIDER | No accepted journey covers external-provider access to operational reading. | Affected additions grounded in Q1–Q3. |
| UC-OPERATE-ISSUE, FR-ISSUE-CONTEXT, FR-ISSUE-CONVERSATION, QR-MOBILE-OPERATIONS | The requested end-to-end issue journey has no complete accepted owner. | Affected additions grounded in the original request; shared conversation behavior needs reconciliation. |
| Ordinary comments during active execution | Lighthouse task digests include owner conversation; new content can invalidate publication even without requesting continuation. | Resolved by Q6: allow with source-consequence warning; do not promise no execution consequence. |

## Remaining decisions and delivery details

The original request is sufficient to draft the whole product without another interview round. No provider question remains. Ordinary comments with source-consequence warnings are agreed. The proposed delivery ordering is in the canonical roadmap and GitHub backlog; none of the later capabilities gates the first delivery.

The owner still reviews the complete Product Change and proposed ADRs. Session assurance/duration and retention periods need an explicit policy before production; architecture can propose their mechanics without pretending numerical values were accepted. Production deployment, secret custody, backup destination and recovery objectives belong to infrastructure readiness. Health formulas and comparison windows must come from supported shared/source contracts; absent contracts produce unavailable signals rather than guessed measurements. These details are scoped to their owning delivery stories.

## Canonical follow-through

Keep product semantics in this change. Use ArcShape for the provider/session/source-account architecture after product direction is resolved; keep proposed realization visibly separate from current architecture. Use #47 and #48 as delivery records and add owner-specific shared-contract dependencies only after inspecting existing Lighthouse issues to avoid duplicates. Preserve triage → refine → propose → implement → integrate → review and all human gates.

This sitting has resolved authentication coverage and external-provider use. Shared Keycloak remains selected, now directed to [production host] for production, with [development host] serving development and a separate authorized GitHub connection. The complete change remains draft, with the questions above unresolved. Nothing has been applied, approved, committed, merged or deployed by this interview.

## Delivery handover

The canonical roadmap and implementation-backlog index now link first delivery (#49, #50, existing #47 and #48), production readiness (#58), next increment (#51–#53) and later horizon (#54–#57). Lighthouse #119 owns the shared action/receipt/continuation gap; existing OIDC guidance #116 is reused. All new stories have value, acceptance criteria, dependencies, product IDs and verification scope, and remain backlog items. No source mutation was made to #47 or its labels/comments.

Official GitHub documentation inspected during this sitting now describes an exact run identifier in a successful dispatch response. ADR-0008 uses that evidence while retaining shared request reconciliation for lost responses. This is a documented capability, not a live dispatch test. The full proposal is ready for owner review; product acceptance and implementation are still separate.
