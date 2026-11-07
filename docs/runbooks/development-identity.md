# Development identity and workspace

The supported application uses one HTTPS workspace and a separate development Keycloak container. Repository workflows and agent execution remain independent. The external-issuer profile provisions no provider; production storage, credentials and deployment belong to #58.

## Prepare private development configuration

Install Node 22+, .NET 10, Docker, OpenSSL and Chromium prerequisites. `npm ci` supplies the exact Aspire CLI paired with the AppHost SDK, including SDK 10.0.100 environments; no global CLI installation is required. Create the private setup as a non-root POSIX user. The generated `ProviderUser` matches that user UID with the image group 0 so 0600 files remain private and readable on GitHub runners as well as local hosts. Recreate the isolated setup when moving between OS users; do not make its keys world-readable. Keep Docker available to the AppHost host. Choose stable HTTPS application and issuer addresses reachable from the intended browser or phone. Both must use the same registered issuer hostname; an internal container hostname is not a replacement.

For a loopback development example:

```sh
npm ci
npx playwright install --with-deps chromium
node scripts/setup-development-identity.mjs \
  --directory "$HOME/.config/sofa-dev" \
  --origin https://127.0.0.1:5443 \
  --issuer https://127.0.0.1:8443/realms/sofa-dev \
  --test-accounts
SOFA_CONFIG_FILE="$HOME/.config/sofa-dev/development.json" npm start
```

For LAN/VPN use, replace the loopback addresses with stable reachable hostnames or the selected host address before generating configuration. The generator refuses an existing destination and any destination inside the checkout. It creates a private directory, separate application/provider certificates, the non-production realm import and synthetic owner/denied accounts. Passwords, client secret, owner subject and TLS keys remain in private files; output contains only setup locations and addresses. No global certificate trust is changed by this setup command.

Trust the generated public `ca.crt` only in the designated development browser/device, checking its fingerprint out of band. Keep `ca.key`, TLS keys, `provider.env`, `development.json`, account passwords and TOTP secrets private. The automated host test creates its own browser HOME/NSS database and removes its setup afterwards. It never disables HTTPS validation.

The first sign-in uses the synthetic owner from `test-accounts.json` and enrolls TOTP in the actual provider. The configured `(issuer, subject)` admits that owner only. Recreating the disposable provider discards enrolled factors; re-enroll rather than mapping a new account by email. A setup without synthetic accounts requires an explicit `--owner-subject` and operator-provisioned account in that development realm.

## Connect the real GitHub user

Create a separate **development GitHub App** in [GitHub developer settings](https://github.com/settings/apps/new). Use the configured application origin as its homepage and the exact callback:

```text
https://<application-host>:<port>/connections/github/callback
```

Configure repository permissions Metadata/read, Issues/read-write and Actions/read, expiring user access tokens, and no webhook. Install it only on the designated test repositories. Administration, contents-write and workflow-dispatch permissions are not required by this slice. A GitHub App registration alone does not constitute user consent.

Add its **client ID** and **client secret** to the private configuration, alongside `SoFa.Identity` and `SoFa.Repositories`:

```json
{
  "SoFa": {
    "GitHub": {
      "ClientId": "operator-supplied-development-client-id",
      "ClientSecret": "operator-supplied-private-client-secret"
    }
  }
}
```

Merge that fragment into the existing private file; preserve its identity, certificates and repository allowlist. Supply both fields or leave both absent. Restart the workspace, sign in and choose **Connect GitHub account**. Complete GitHub's user authorization in the browser. Confirm a different acting account explicitly before replacement. Verify the displayed numeric user identity, repository permission result and check time.

Never paste credentials into an issue, PR, chat, screenshot or tracked file. The application has no operator `gh`, PAT or installation-token fallback. Linking, checking, disconnecting and signing out do not post issue comments, change labels or request workflows. Native GitHub links remain source inspection paths; source command controls are separate work.

## Session and failure behavior

The browser holds an opaque Secure/HttpOnly owner cookie. Provider and source tokens stay server-side. The development session expires after fifteen idle minutes or four hours from the original provider authentication, whichever comes first. Observation/account polling does not extend it. Identity-provider authority is checked online at least every five minutes; a due failure blocks protected access, and recognized revocation destroys local authority.

Local sign-out and disconnect require CSRF-protected POSTs and remove local credentials even during provider outage. The provider may retain its own browser SSO session; the next SoFa sign-in still requires fresh password/TOTP. GitHub's separate grant can be revoked in [authorized GitHub Apps](https://github.com/settings/apps/authorizations). Restart loses process-local sessions/connections and requires fresh login/linking.

`npm start` restores/builds the pinned Release AppHost, then executes that exact assembly. Its application dashboard is disabled. Calling `dotnet run` directly can invoke Aspire's CLI setup/run hook and select another configuration; the launcher avoids that path. AppHost starts the workspace and development provider, with no execution database, supervisor or anonymous legacy listener. Aspire's stopped project-rebuilder helper is development tooling.

## Verification and physical-device acceptance

```sh
npm run verify
# Requires Docker and certutil (libnss3-tools on Ubuntu):
npm run verify:identity:host
```

The deterministic lane checks real middleware with signed synthetic issuer responses, callback forgery/replay, session deadlines, permission/callback isolation, no source fallback and module/compiler gates. The host lane composes the actual pinned provider through Aspire, validates TLS, MFA, logout, online refresh, revocation, provider outage, keyboard operation and axe checks. Linked/insufficient source UI states are explicitly browser fixtures. Safe screenshots and summary receipts go to `test-results/identity`; no HARs, traces, callback screenshots or raw browser call logs are retained.

For final #49 acceptance, record the real GitHub acting account and designated repository success/insufficient/revoked behavior, then use an actual phone on LAN/VPN to:

1. Reach both trusted HTTPS application and issuer addresses.
2. Complete fresh MFA, inspect the separate GitHub acting account and permissions, and finish source linking.
3. Sign out and confirm direct workspace/API access is denied.
4. Record device/browser, time and safe post-callback outcomes. Keep codes, secrets and provider-setup screens out of evidence.

Phone-width emulation does not prove these device steps. A successful unit suite, issuer discovery or synthetic source UI does not close the actual-source/device acceptance.

## Stop and rollback

Stop the launcher/AppHost to stop its workspace and disposable development provider. Process-local sessions and connections are discarded. Rollback never requests workflow cancellation or silently starts an anonymous observer. The historical `npm run observe -- owner/repository` CLI is separate read-only tooling using the operator's explicit `gh` authorization.
