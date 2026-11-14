# Lighthouse SDLC 0.22.4 source receipt

These unchanged files are copied from the published source tag `@juangcarmona/lighthouse-sdlc@0.22.4` in `juangcarmona/lighthouse`, commit `879c4e75efebd945f8aea5b8ee09b97db62edbea` (tagged 2026-11-08). The fixture is used only to check compatibility between its declared contract and its own closed schema; it does not make SDLC semantics supported by Control Center.

| File | SHA-256 |
| --- | --- |
| `contract.json` | `ebf4493b700b2ffdc669ba0a46d5b513c644f714c198c07450f02a2dbd512a23` |
| `sdlc-contract.schema.json` | `d014cc1f1170a4f6e68751a7cb1e7714758301c0a7d149a1d8b6be1212971a0b` |

The contract declares five `assets` keys that its schema neither allows nor includes in its required set: `browserE2eSchema`, `capabilityCatalog`, `capabilityCatalogSchema`, `commandAliases` and `commandAliasesSchema`. The test reads these pinned source files and checks the incompatibility against `additionalProperties: false`. It does not modify or relax the upstream schema.
