# 04A.09 review evidence

This directory is an unstaged publication proposal. Every input is synthetic or
already committed. No native Excel observation or statistical-golden promotion
is represented. **P NO; N1 NO GO; N2 NO; C2 UNMET.**

* `baseline-manifest.json` pins the pre-production-edit .NET 10 capture, Core
  source, input CSVs and original form methods. `baselines/` retains all twelve
  complete table/fitted/residual snapshots, plus separate .NET Framework 4.8
  snapshots. The Framework captures ran after the bridge edit against unchanged
  Core; they are explicitly distinguished in `runtime-baselines.json`.
* Baseline text is typed, row-major data: `D:` is invariant round-trip Double,
  `NULL` is an absent cell, and `String:` contains base64 UTF-8. Other scalar
  kinds retain their CLR name. `TABLE` includes dimensions and p-value metadata.
  Only computational-time text is masked. All numeric cells are compared with
  `max(1e-9, abs(expected)*1e-9)`; string/type/ordering comparisons are exact within
  each runtime. See the managed capture/reader for the complete format.
* `source-delta-final.json` supersedes the earlier current-batch
  `source-delta.json` solely for final file-byte fingerprints after restoring
  original UTF-8 BOMs. Member bodies and writer-tail parity are the same.
* `restriction-crosswalk-v2.json` expands the initial direct-handler crosswalk
  to unchanged transitive input/validation dependencies. It preserves 34
  associated control IDs and seven original obligations. Other associated
  controls acquire no invented obligation. Native status remains restricted.
* `historical-source-protection.json` pins original tests and reference inputs;
  the full historical TestResults inventory remains private and in place.
* `validation-v2.json` is the final result summary. The retained v1
  `validation.json` has a disclosed aggregation error: four extra subtest events
  were counted as method identities. V2 preserves all events and correctly
  reports 642/693 Python methods; no test outcome was altered.
* `command-outcomes.json` carries sanitized commands, exact exit codes, raw-byte
  hashes and execution-copy provenance. `preservation-final.json` carries the
  final 173,832-file preservation receipt with a separate original-byte hash.
* `strict-validation-activation.UNAPPLIED.patch` is review material for the one
  bounded disposition `C09-NATIVE-STRICT-01`. It has **not** been applied. Shipping
  Windows observes Common strict diagnostics and retains legacy refusal behavior.

The final owner dossier is `docs/architecture/batch-04A-09-owner-review.md`.
`publication-manifest.json` records proposal hashes without hashing itself.
Raw command logs, TRXs, historical metadata, source preimages and development
attempts remain under the new ignored `TestResults/04A-09-20261103-run01/`.

Public derivatives replace repository/home prefixes, the qualified portable
Python executable, the VS installation prefix and owner SID with `<REPO>`,
`<OWNER_HOME>`, `<PYTHON_EXE>`, `<VS2026>` and `<OWNER_SID>`. Original local bytes
and their separate SHA-256 hashes remain untouched. No raw logs or historical
archive are proposed for staging.
