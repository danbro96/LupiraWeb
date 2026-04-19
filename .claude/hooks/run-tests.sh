#!/usr/bin/env bash
# PostToolUse hook: runs relevant tests after Edit/Write and surfaces results.
# Never exits non-zero — TDD means red is a valid intermediate state. Claude
# reads stdout to decide whether more work is needed.

set -u

# Hook payload is JSON on stdin. Extract tool_input.file_path (may be absent).
payload="$(cat || true)"
file=""
if command -v jq >/dev/null 2>&1; then
  file="$(printf '%s' "$payload" | jq -r '.tool_input.file_path // empty' 2>/dev/null || true)"
else
  # jq-less fallback: pull the first file_path value out of the JSON with sed.
  file="$(printf '%s' "$payload" | sed -n 's/.*"file_path"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' | head -n1)"
fi

# Normalize to forward slashes for matching.
file="${file//\\//}"

skip_ext_regex='\.(md|json|jsonc|yml|yaml|toml|ico|png|jpg|jpeg|svg|pdf|lock|gitignore|dockerignore|env|example)$'
skip_path_regex='(^|/)(\.next|node_modules|bin|obj|out|\.turbo|\.vercel|\.git|docs|public)(/|$)'

if [[ -n "$file" ]]; then
  if [[ "$file" =~ $skip_ext_regex ]]; then exit 0; fi
  if [[ "$file" =~ $skip_path_regex ]]; then exit 0; fi
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

ran_any=0

# --- Frontend tests ---
if [[ -z "$file" ]] || [[ "$file" =~ (^|/)lupiraweb\.client/ ]] || [[ "$file" =~ \.(ts|tsx|css)$ ]]; then
  if [[ -f "$repo_root/lupiraweb.client/package.json" ]]; then
    echo "▶ vitest (frontend)"
    (
      cd "$repo_root/lupiraweb.client" \
        && npx --no-install vitest run --passWithNoTests --reporter=dot 2>&1 | tail -40
    ) || true
    ran_any=1
  fi
fi

# --- Backend tests ---
if [[ -z "$file" ]] || [[ "$file" =~ (^|/)LupiraWeb\.Server ]] || [[ "$file" =~ \.cs$ ]]; then
  if [[ -d "$repo_root/LupiraWeb.Server.Tests" ]]; then
    echo "▶ dotnet test (backend)"
    (
      cd "$repo_root" \
        && dotnet test LupiraWeb.Server.Tests/LupiraWeb.Server.Tests.csproj \
            --nologo --verbosity minimal 2>&1 | tail -40
    ) || true
    ran_any=1
  fi
fi

if [[ "$ran_any" -eq 0 ]]; then
  exit 0
fi

exit 0
