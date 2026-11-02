# 04A.02 Checkpoint 01 fixture foundation

**04A.02 PARTIAL / OWNER REVIEW REQUIRED.** Two test-only fixtures use
`04A02-typed-preparation-v1` under `BESHStat.Excel.Common.Tests/TestData/04A02/`.
There are no approved numerical expected values. Source C30 proposals remain
unchanged and retain their original NOT RUN status. New execution observations
belong exclusively to new run evidence.

`MigrationFixture04A02Support.vb` uses the project's existing System.Text.Json.
Canonical JSON sorts object keys with ordinal comparison, preserves array order,
uses invariant UTF-8 numeric tokens and rejects nonfinite doubles. Round trips
are canonicalized before comparison; raw source indentation is not a comparison
key. SHA-256 of canonical dataset JSON covers labels, values and provenance
RowIds separately from production service transport. No timestamps or machine
paths enter fixture comparison fields. Source commit plus relative source paths
and file SHA-256 values pin the inputs/options/service/proposal. No regeneration
or snapshot approval mode exists.

Candidate observations have schema `04A02-candidate-observation-v1`. They retain
both KW H/p pairs, ANOVA primary F/p, ordered tables, CLR cell types, explicit
nulls, header/footer/title counts and p-value column/cell addresses. They are
semantic candidates, not rendered workbooks or complete frozen Common documents.
G3 still requires approved warning/error/layout/format/document fixtures.

The tests assert fixture integrity, input non-mutation, finite typed metrics,
nonempty tables, group identity/counts and invariant/sk-SK stability. They do not
assert computed numbers against a local self-generated expected vector. An
explicit observation directory must be under a new `TestResults/04A-02-initial-fixtures-*`
run; files are created exclusively and cannot overwrite existing observations.

`scripts/verify-04a02-fixtures.py` copies assemblies, every dependency and TestData
before executing Common or legacy suites; copy hashes are checked. Common
four-parent writers stay below the execution sandbox; legacy CSV exports are
explicitly redirected. The helper creates a hash-bound, diff-recorded copy of the
live validation script to route outputs and prohibit implicit package restores.
The checked-in validation script and historical evidence remain unchanged.

Review [scientific decisions](scientific-decisions.md), the machine-readable
[full coverage register](coverage-register-v1.json), [exact restriction crosswalk](native-restriction-crosswalk-v1.json)
and [current owner status](current-owner-status.md). Future native cases remain
NOT RUN. The crosswalk lists all flagged controls in each source form, rather
than assuming every control is exercised by every option; the executing owner
must establish the exact exercised subset and release every applicable control
and shared RefEdit edge before capture.
