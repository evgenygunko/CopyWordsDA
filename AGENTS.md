# CopyWordsDA repository instructions

This repository contains the .NET MAUI client and its shared application logic.

## Validation

- For client-only logic changes, run `dotnet test source/CopyWords.Core.Tests/CopyWords.Core.Tests.csproj` from this repository root.
- For changes affecting the MAUI application, also run the smallest relevant `dotnet build` that supports the current platform.
- If validation cannot run because of environment limitations, report that explicitly.

## Git and Azure DevOps pull request workflow

When the user explicitly asks to commit a change and create a pull request:

- Preserve unrelated and pre-existing user changes. Commit only files belonging to the requested change.
- Create a new branch named `codex/<short-kebab-case-description>`. Never commit directly to the default branch.
- Use the repository's configured writable remote, currently `origin`, and target its default branch, currently `master`.
- Commit the requested change with a concise commit message.
- Push the new branch and establish tracking with `git push -u <remote> <branch>`.
- Create an active, non-draft pull request using the Azure DevOps MCP server's `repo_pull_request_write` operation with action `create`.
- Use `refs/heads/<branch>` as the source and `refs/heads/master` as the target.
- Do not use a browser, Azure CLI, `az repos pr`, or another API client to create the pull request.
- If Azure DevOps MCP is unavailable or rejects the request, stop and report the failure instead of silently using another PR creation method.
- Report the repository, branch, commit SHA, validation results, and Azure DevOps pull request link or ID.

If a change also affects the `Translations` repository, create a separate branch, commit, push, and pull request in that repository.
