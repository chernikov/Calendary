# Codex Agents Instructions

## Purpose
Allow Codex agents to execute `gh` commands via CLI for repository automation: branches, pull requests, and issues.

## Environment
- OS: WSL (Ubuntu)
- Pre-installed tools: `gh` (GitHub CLI), `codex` (Codex CLI)
- Authentication: `gh auth login` already completed; `gh` has access to the required GitHub account.

## Commands allowed
- `gh repo clone <owner>/<repo>`
- `gh pr create --title "<title>" --body "<body>"`
- `gh issue create --title "<title>" --body "<body>"`
- `gh agent-task create "<task description>"` *(if using the agent-tasks feature in gh CLI)*

## Workflow
1. Start the agent with `codex` or `codex exec --auto`.
2. The agents read the instructions in this `AGENTS.md`.
3. Agents request approval according to the `/approvals` setting (see below).
4. After approval, agents use the `gh` commands to perform the necessary actions.
5. Change logs can be viewed via `gh agent-task view <id> --log`.

## Policies & Approvals
- Minimum: only the `gh` commands listed under “Commands allowed” are permitted.
- If an agent wants to execute a non-standard command, it must request approval:  
  `Agent requests approval to run: <command>`
- The `/approvals` setting must conform to the policy: `suggest` / `manual` / `auto`.

## Model & Reasoning
- **Model:** gpt-5-codex
- **Reasoning effort level:** medium (use high for complex refactorings).

## Review Process
- After each task is executed, the agent creates a pull request or an issue.
- A reviewer confirms via `gh pr merge` or closes the PR.
- Use the `/review` command for a Codex CLI-driven review of the changes before merge.
