#!/usr/bin/env bash
set -euo pipefail
engine_binary=${1:?Usage: build-engine.sh /absolute/path/to/opencode}
[ "$("$engine_binary" --version)" = "opencode v2.1.0" ] || { echo 'OpenCode 2.1.0 is required' >&2; exit 1; }
context=$(mktemp -d)
trap 'rm -rf "$context"' EXIT
cp src/execution/* "$context/"
cp "$engine_binary" "$context/opencode"
docker build -t sofa-engine:2.1.0 "$context"

docker run --rm --entrypoint node sofa-engine:2.1.0 -e 'const c = require("/etc/opencode/opencode.json"); if (!c.policies.some(p => p.action === "tool.use" && p.resource === "*:*" && p.effect === "deny")) throw Error("Missing readable managed policy");'
