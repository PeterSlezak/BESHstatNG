# Official 04A.10 review evidence

Owner decision **PENDING**. Candidate software evidence only: **NOT_GOLDEN**,
not native Excel verification, not independent scientific review.

Base local `main` and unchanged HEAD: `49fcc30a6b39f6a2bebcc9353ce2dea0d3e13055`.
Authorized branch: `codex/04A-10`. The entry worktree/index were clean before
creating the new capture harness. `prechange-manifest.json` records the subsequent
pre-production-edit inventory, which already includes the two authorized new
capture/harness files in its untracked status. No owner file was removed.

| Artifact | Meaning |
|---|---|
| `prechange-manifest.json` | SHA-256 inventory before production edits; historical TestResults metadata count |
| `baseline-registry.json`, `baselines/*.txt` | Six real-Core cases on each runtime, captured before production edits; original 04A.09 captures separately pinned |
| `baseline-byte-provenance.json` | All 24 original 04A.09 files retain entry bytes; frozen receipt CRLF hashes versus current main LF checkout explained without changing expectations |
| `numerical-comparisons.json` | Readable coefficient/table/fitted excerpts; complete typed cells and residuals remain in captures |
| `source-delta.json` | Exactly two changed form members; unchanged native tails after the single documented classification-list substitution; all protected tracked source hashes |
| `restriction-crosswalk.json` | Exact original 34-control/seven-obligation union, original row hashes, no releases |
| `validation.json`, `frozen-pin-successor.json` | Current Debug/Release outcomes, every failure identity, inherited versus newly stale source expectations |
| `command-outcomes.json`, `historical-checkers.json` | Sanitized commands/exit codes, original raw receipt hashes, execution-copy diffs and checker nonpasses |
| `preservation.json` | Historical membership/metadata and accepted-source preservation scope and result |
| `publication-manifest.json` | Current candidate files, SHA-256, Git-ignore/privacy checks; self excluded |

Source receipts use raw SHA-256 for files;
method-block hashes normalize BOM/newlines to UTF-8 LF for textual member
comparison. Whole-file pre/post hashes retain the exact byte provenance.

The line capture encoding reuses the accepted 04A.09 test helper, hence its
`04A09-host-free-v1` first line. This does not make these files accepted 04A.09
artifacts. `TABLE` records include shape, title/header/footer counts and p-value
annotations; `D:` cells use invariant round-trip Double strings; other scalar
types use base64 UTF-8. `NULL`, NaN and infinity stay distinct. Only elapsed-time
footnote text is masked. Runtime-specific embedded Core strings are separately
captured, not normalized to claim parity. LM capture convergence is `UNKNOWN`
because the legacy encoder has no non-iterative value; its typed report explicitly
uses `NotIterative`.

The original Gaussian and binomial CSVs are committed synthetic Core fixtures.
Their observations are not published anew here. The new captures intentionally
retain reversed noncontiguous identities without reversing the input data order.
Original 04A.09 GLM ordinary is **Poisson**, alternate is binomial.

Reproduction uses `scripts/verify-04a10.py`, `capture-04a10-contract.py` and
`summarize-04a10.py` with the locally qualified Python executable. Full commands
are in the dossier and command receipt. Capture flags are absent in normal tests;
explicit capture uses create-new files and cannot overwrite a baseline. Never
use capture to promote a numerical reference. Old accepted evidence remains
read-only. Current raw logs, TRXs, preimages and the historical metadata inventory
stay ignored under the fresh `TestResults/04A-10-20261103-run01` directory.

**P NO; N1 NO GO; N2 NO; Gate C2 UNMET.** `C09-NATIVE-STRICT-01` stays deferred;
its patch remains unapplied. The historical 1,514-obligation / 802-control universe
and overlapping subsets remain OPEN. Excel/COM/UIA, installers, deployment,
signing, Git staging/commit/push/merge and 04A.11 are not authorized by this receipt.
