#!/usr/bin/env bash
set -euo pipefail

ACTIONS_VERSION="v1.11.0"
SCRIPT_URL="https://raw.githubusercontent.com/grovegs/actions/${ACTIONS_VERSION}/.github/actions/build-agent-skills/scripts/build_agent_skills.sh"
SKILLS_CHECK="false"

if [ "${1:-}" = "--check" ]; then
  SKILLS_CHECK="true"
elif [ -n "${1:-}" ]; then
  echo "Usage: scripts/build_skills.sh [--check]"
  exit 2
fi

cd "$(dirname "${BASH_SOURCE[0]}")/.."
curl -fsSL "${SCRIPT_URL}" | SKILLS_ROOT="." SKILLS_CHECK="${SKILLS_CHECK}" bash
