# 04A.11 public-evidence sanitization — 2026-11-03

**READY FOR OWNER GIT PUBLICATION REVIEW.** This is the owner's authorized
administrative publication preflight after acceptance of the 04A.11 software scope,
not another statistical acceptance or an authorization to publish. The substantive
[software dossier](batch-04A-11-owner-review.md) remains byte-identical; its pending
decision wording records the preceding review state.

Verified branch: `codex/04A-11`; local `main` and `HEAD`:
`463aaa12a6342cedfa084e266d5a985461de9d3a`. No branch switch, remote query,
fetch, reset or other Git publication action occurred. Initial work was the accepted
11 tracked modifications and 64 untracked candidates, with an empty index.

All 75 initial public candidates were copied byte-for-byte to an ACL-protected
owner-local vault outside the repository and TestResults before redaction. The vault
holds original manifests, raw files, initial inventory and private reverse mappings.
Owner/system/administrator access is retained; the local sandbox validator has read
access. Private directory names and mappings are excluded from this publication.
Mapping-plan revisions were retained privately when the complete scan exposed
shortened identifiers repeated within deployment subfolders.

| Existing public path under `evidence/04A11/` | Original raw SHA-256 | Raw bytes | Public derivative SHA-256 | Public bytes |
|---|---|---:|---|---:|
| `prechange-manifest.json` | `718194351f8744a346de084d07ec968536f041de7ae54973a3fcc47072df1404` | 471889 | `2c56fc732a5fa984213c4fe0f196812a877fd67593561918aa367e252a3f8ada` | 472932 |
| `source-delta.json` | `c9b928dc7f709e06c4e5e46c0d1f0f1eff7ca967dcaafa0adfa95a1f367828b4` | 480359 | `3ac0138476d75ce71c83f42061e9e9767c73ac05756612a6d9bc9359b3f9c535` | 481402 |
| `source-delta-v2.json` | `d0c4fcc8546b098a85c0f2583185ee842b25fa1ce1ce7dca52e684973ab18169` | 480479 | `8649648e6b75b590e11ba097e087e555b87ff993dcef105287722a717b0aec24` | 481522 |
| `source-delta-final.json` | `5eb53e69b926a4663285f3d142eef133793c005a92d6be466ad70a16f4ba8b10` | 480794 | `acd6fa204c5c29180f224a486ba8272c8ffe15789ee5bd32103e8f1a351ceb39` | 481837 |

Each derivative retains **70 unique mappings**: 70 dated components plus 63 repeated
deployment components, or **133 components per file / 532 total**. The neutral
`REDACTED_OWNER_HOST` alias retains dates, uniqueness suffixes and artifact types.
It denotes a display identity, never a real historical filename. Private reversal
restores the complete original JSON structure, dictionary order and every value.
All source SHA-256 values, other subtrees, native controls and obligations are invariant;
no schema keys were added to these four documents. Their new byte hashes intentionally
differ from the privately authoritative raw hashes.

The [complete authoritative inventory](../../evidence/04A11/publication-manifest-sanitized.json)
enumerates **82 candidates: 81 external file hashes plus itself**. Self hashing is
omitted to avoid cycles; the self file remains in privacy and trackability scope.
The original 72/74-entry manifests are privately preserved, while their public
revisions retain historical subset membership with current hashes/byte counts and
explicit successor references. The manifest dependency order is earlier subset,
final historical subset, then complete sanitization successor. Old raw hashes in
the companion, this note and verifier are intentional provenance, never advertised
current-file identities.

This task changes only the four public derivatives, two subset manifests and README,
and adds this note, the complete successor manifest, three companion receipts,
the publication verifier and its focused tests. All 68 other initial candidates,
including production VB, project files, original tests, six Help pages, accepted
software dossier, baseline captures and dormant patches, remain byte-identical to
task entry. Root `.gitignore` is unchanged. Private vaults, mappings, ignored raw run
records/`baseline.json`, all historical TestResults, caches, compiled outputs and
unrelated local files are excluded.

The [privacy receipt](../../evidence/04A11/publication-privacy.json) covers complete
candidate names/content, decoded JSON keys/values, Unicode/case and slash variants,
and decodable base64 payloads. It checks known private identifiers and components,
profile, AppData, home, drive and UNC paths, SIDs, credentials, emails and internal hostnames.
Six reviewed false-positive groups comprise product-brand text and established
comparison-fixture basenames; their context is public product naming. No matched
private values or reverse mapping is published. This scan and targeted manual review
are bounded assurance, not proof that no conceivable secret exists.

Focused outcomes and exact commands, using `<LOCAL_PYTHON>` for the established
portable executable, are recorded in
[publication-validation.json](../../evidence/04A11/publication-validation.json).
The unchanged original nine-test 04A11 source suite returned **7 passed / 9**, exit 1:

- `Regression11SuccessorTests.test_exact_six_form_deltas_and_native_writer_tails`:
  FAIL comparing raw generator output with the now-redacted public document.
- `Regression11SuccessorTests.test_unaffected_sources_and_all_old_evidence_are_byte_protected`:
  ERROR trying to open a display alias as a real filename.

These are two explicitly reported publication-mode incompatibilities. Original
assertions remain untouched. The new successor verifies raw-local pins and writer
tails against private originals without overwriting any receipt, then verifies the
public projection and manifest separately. They are not reported as passing old tests.
The verifier's `public` action checks published hashes/bytes, manifest relationships,
aliases and Git trackability without private-vault access; its `verify` action adds
private-original equivalence, initial-work preservation and the empty-index guard.
The **six prior logical managed nonpasses** (five inherited plus one frozen
constant-response expectation) and **61 prior Python nonpasses** remain real and
unchanged; these two administrative incompatibilities are additional, separately
identified nonpasses. No full managed/Node/Python/R/MkDocs suite was rerun.

The available retention checker passed for **180375 historical files**, with zero
errors, by complete initial membership, size, creation/write metadata and file identity
plus frozen source hashes. It does not claim a full byte audit of historical results.
The new retention receipt was written only to a fresh path in the existing ignored
run. No historic file was deleted, rewritten, moved, compressed or rebaselined.
Reported free space was 532598468608 bytes.

`C09-NATIVE-STRICT-01` and `C11-NATIVE-CORRECTIONS-01` remain UNAPPLIED/deferred.
**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET**. All seven UiGLM obligations remain
OPEN, all 34 controls restricted, zero releases/discharges. No native Excel/COM/UIA
execution, new statistical validation or golden promotion occurred. No 04A.12 work.
Existing later-batch duties and all original overlapping obligation populations remain.

Focused publication successor: **10/10 passed**, exit 0. This includes actual raw-local
verification of 3198 protected pins and the original writer-tail/source computation,
intercepting its output in memory for comparison with the private original. Public
aliases are never opened as historical files. Complete privacy rescan: **82/82 files**,
zero unresolved findings. External hashes/byte counts: **81/81 verified**, plus **82/82
trackability checks** including the self file. Six pre-existing tracked Help pages
match broad ignore rules when tested with `--no-index`; they remain tracked and
unchanged. All 71 untracked publication candidates are non-ignored. Both historical
subset manifests also match actual current bytes for every advertised entry.

`git diff --check` passes; index empty. Current status remains **11 previously modified
tracked files and 71 untracked candidates**, all unstaged/uncommitted. The only
tracked deltas against HEAD are the already-accepted implementation; this task
changes seven of its untracked evidence candidates and adds seven administrative
candidates. No substantive product/project/Office/GUI/Help change was made by this task.
No staging, commit, merge, push, publishing or deployment was performed.

Owner action: review the privacy preflight and exact 82-path inventory before any
separately performed Git publication. This recommendation grants no publication
authority, native qualification, scientific approval or next-batch authorization.
