# BESHStatNG development instructions for Codex

## Project boundaries
- Source of truth is the current checked-out Git repository. Inspect files and the master cross-platform plan before each numbered batch.
- The approved delivery sequence is one numbered batch per feature branch; never begin the next batch without explicit approval.
- `BESHStat.Core` and `BESHStat.Excel.Common` target `netstandard2.0`. Retain this target while .NET Framework 4.8 Excel-DNA needs it.
- `BESHStatNG` and `BESHStatNG.Test` are Windows .NET Framework 4.8 projects. Preserve 32/64-bit Excel-DNA packaging, public entry points, and existing UI behavior.
- The WASM smoke harness is not the production Office add-in; avoid expanding it into a permanent production UI.
- `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` is authored method/help metadata authority. Ribbon XML is Windows callback authority; `<ExcelFunction>` declarations are UDF export authority.
- LibreOffice is a future portability consideration, not an approved implementation scope.

## Code style and design
- New portable VB.NET code: `Option Strict On`; no late binding, WinForms, COM, Office.js, Microsoft.VisualBasic helpers, or filesystem dependencies in Core/Common.
- Reuse existing typed inputs, validation reports, Core math and Common analysis handlers; do not duplicate formula grammars or statistical algorithms.
- Preserve source row identity, missing-value semantics, finite-number checks, and native error metadata. Keep Excel-specific A1/date/error interpretations in host-specific policies.
- Do not silently change Core numerics, public APIs, serialization schemas, user-visible output tables, or p-value semantics.

## Required workflow
1. Inspect Git status. Do not overwrite unrelated modifications; work on a dedicated feature branch.
2. Review existing implementations/tests and list concrete files to change. Make only the requested batch.
3. Add focused unit tests for ordinary cases, edge cases, and compatibility. Do not weaken existing tests to achieve green status.
4. Run `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1` for portable validation. Where the Windows toolchain is available, also run the script with `-Legacy`.
5. If a test fails, investigate, fix the batch defect, and rerun relevant tests. Report pre-existing unrelated failures separately.
6. Before presenting results, run `git diff --check` and `git status --short`; verify only approved files changed.
7. Report the exact commands and outcomes, including tests not run, limitations, modified paths, and one/two relevant Excel/WASM manual smoke checks.
8. Never commit, push, alter remote branches, run installers, touch signing keys, deploy, or change framework targets without explicit approval.

## Safety
- Keep command execution inside the project directory and retain Codex sandbox permissions. Ask before network/package installs or destructive commands.
- Do not run live Excel, installer, or publishing workflows without approval. Do not expose workbook data, private keys, or license credentials in logs.
