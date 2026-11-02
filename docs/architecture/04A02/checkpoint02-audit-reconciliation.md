# Checkpoint 02 ? post-merge audit reconciliation

**SUPPLEMENTAL DIAGNOSTIC / OWNER REVIEW REQUIRED. No original audit failure is changed to PASS.**

The owner-accepted 04A.01 inventory decision and all 1,514 restricted obligations remain in force. Original strict full-contract incompleteness, Gate C2 UNMET, E1?E7 OPEN, GC3?GC8 DEFERRED and all 802 affected live-capture restrictions remain. No retroactive signature or sealed-record edit is made.

Checkpoint 02 verified the existing branch `codex/04A-02` and main at `12e25924aae93f0b36ab33937176c86c19641b50`, inventory ancestor `ec5e47a0cfc352ea5fa9fcd598e1f10b5b8fbd9b`, the empty index, and all original 14 Checkpoint 01 file hashes. ZIPs were not extracted.

Local retained evidence is **PRESENT**. The diagnostic freshly verified all 1,527 entries of the Checkpoint 01 artifact manifest. It reads original scripts/data without importing, executing, monkeypatching or changing their acceptance logic. It verified 554 product files against start-of-checkpoint hashes; stable post-build verification is reported in the checkpoint review.

Evidence root: `TestResults/04A-02-initial-fixtures-checkpoint02-20261030T135915Z`. Main records: `audit-compatibility-diagnostic-restored.json` (final source-pinned diagnostic), `historical-case-provenance.json`, `inventory-profile-failure-classification.json`, and retained current checker logs. The first complete diagnostic and interim source-drift diagnostics remain separately retained.

## Exact failed identities

All 24 below are FAILED in Checkpoint 01: 11 strict-exit expectations, 12 raw-prefix checks and one C30 whole-store check. The 20 distinct historical `before.json` records independently confirm `codex/04A-01` / `11fa5d99a040de2e62c75ec201c49852c1d6b289`. That SHA is a historical audit baseline, never the current source baseline.

Class/module identities are printed exactly as retained in the original logs. Each row has complete assertion text/block SHA, current test-source SHA and Git object, method line range, local-only snapshot path/hash and binary comparison links in the diagnostic JSON.

| Exact failed identity | Current source location | Expected ? observed | Source snapshot / diagnosis |
|---|---|---|---|
| `__main__.Continuation04Tests.test_new04_strict_blockers_are2_but_deleted_pin_is1` | `scripts/windows-migration-audit/test_reconciliation.py:381` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation05Tests.test_strict_open2_but_missing_new_reg_pin1` | `scripts/windows-migration-audit/test_reconciliation.py:459` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation06Tests.test_strict2_is_distinct_from_corruption1` | `scripts/windows-migration-audit/test_reconciliation.py:520` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation07Tests.test_incomplete_strict2_and_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:575` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation08Tests.test_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:647` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation09Tests.test_09_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:735` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation10Tests.test_10_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:818` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation11Tests.test_11_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected` | `scripts/windows-migration-audit/test_reconciliation.py:930` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-11/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation11Tests.test_11_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:911` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation12Tests.test_12_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected` | `scripts/windows-migration-audit/test_reconciliation.py:1061` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-12/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation12Tests.test_12_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:1042` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation13Tests.test_13_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected` | `scripts/windows-migration-audit/test_reconciliation.py:1198` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-13/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation13Tests.test_13_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:1179` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation14Tests.test_14_prior_raw_crlf_bytes_preserved_and_newline_drift_rejected` | `scripts/windows-migration-audit/test_reconciliation.py:1346` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-14/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation14Tests.test_14_strict_incomplete2_distinct_from_missing_pin_integrity1` | `scripts/windows-migration-audit/test_reconciliation.py:1327` | exit 2 ? exit 1 | `docs/architecture/WindowsMigrationLedger/{coverage,source-provenance}.json` Frozen HEAD/main/branch; original exit 1 retained. |
| `__main__.Continuation15Tests.test_15_prior_1662_ordered_literal_bytes_preserved` | `scripts/windows-migration-audit/test_reconciliation.py:1511` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-15/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation16EvidenceTests.test_16_prior1894_exact_literal_order` | `scripts/windows-migration-audit/test_reconciliation.py:1612` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-16/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation17EvidenceTests.test_17_prior2144_literal_order_preserved` | `scripts/windows-migration-audit/test_reconciliation.py:1710` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-17/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation18EvidenceTests.test_18_historical2350_literal_order_and_c17_mutation` | `scripts/windows-migration-audit/test_reconciliation.py:1832` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-18/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation19SourceControlTests.test_19_all_old_member_control_wiring_literal_prefixes_preserved` | `scripts/windows-migration-audit/test_reconciliation.py:1927` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-19/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation20SourceControlTests.test_20_all_prior_literals_and_twelve_c19_deferrals_preserved` | `scripts/windows-migration-audit/test_reconciliation.py:1979` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-20/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `test_continuation21.Continuation21SourceControlTests.test_21_prior_ordered_literals_and_normalization` | `scripts/windows-migration-audit/test_continuation21.py:74` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-21/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `test_continuation22.Continuation22SourceControlTests.test_22_prior_ordered_literals_and_normalization` | `scripts/windows-migration-audit/test_continuation22.py:76` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-22/original/scripts/windows-migration-audit/` Ordered member-prefix CRLF/LF difference; later appended decisions are outside that prefix assertion. |
| `__main__.Continuation30Tests.test_30_original_literal_bytes_remain_exact` | `scripts/windows-migration-audit/test_continuation30.py:67` | exact bytes / zero errors ? byte assertion failure | `TestResults/04A-01-continuation-30/original/scripts/windows-migration-audit/` Full member-store CRLF/LF difference; auxiliary control/wiring stores remain byte-identical. |

The strict tests use a temporary review override through the original analyzer. Those per-test TemporaryDirectory files were not retained by the original test implementation; exact overrides are **UNAVAILABLE** as artifacts. The original test source, assertion, SHA-bound log and final ledger/store inputs remain available. No temporary overrides were recreated and no new strict test PASS is inferred.

## Binary and semantic findings

All 39 historical ordered-store comparisons preserve prior IDs/order and exact independently parsed JSON meaning. Differing tested prefixes reconstruct in **both directions** by changing only recorded CRLF/LF endings; byte disagreements remain failures. Numeric meaning uses exact Decimal/integer parse trees, avoiding the binary64 alias under investigation in fixture serialization.

**Whole older files are not equal merely after newline normalization**: later audit decisions were appended between checkpoints. Therefore the diagnostic separately records full-file differences and the exact older ordered literal-object prefix tested. The comparison-stream first offset is labeled separately from the real whole-file offset. C30 has no later appended member records, and its whole member store differs only by line endings.

| Member snapshot | Original bytes | Current bytes | Whole first-difference offset | Prior/current objects | Original SHA-256 |
|---|---:|---:|---:|---|---|
| C11 | 8728605 | 38136416 | 1 | 1199 / 3866 | `e16bd2db150d7b0d7c0739ef760de7ee1935c9927cd6ff475499bce74ae8361c` |
| C12 | 9520711 | 38136416 | 1 | 1262 / 3866 | `7dfe4e10989ecfb0e012b69c8bb42f6a3622c8ae1c489c449cf597c785d0141e` |
| C13 | 10153329 | 38136416 | 1 | 1312 / 3866 | `4197a39a6c8b55bdadff8925690ec6a84b821820ce2d114797d55a09dcc13252` |
| C14 | 11872584 | 38136416 | 1 | 1446 / 3866 | `ecd5d22b1b5d616d2fda3e9ad38b8b420beb805421eddd6a5d3b4b65cb46c273` |
| C15 | 13268557 | 38136416 | 1 | 1662 / 3866 | `329e35b47e24e49d7a4dd866b6f677a50313137e2d4f33a85c8da43a8cc61af2` |
| C16 | 14849591 | 38136416 | 1 | 1894 / 3866 | `83474cf5a9f59bb604f7d0d37a62e35bff19c91f887dc57901616fb4afb3b88f` |
| C17 | 16814309 | 38136416 | 1 | 2144 / 3866 | `4d343d3dd572454e1308e7ba22f1f5e3e565d8cd062801bc9fb513edc9f0c96f` |
| C18 | 18309561 | 38136416 | 1 | 2350 / 3866 | `7b5f9deca21f478c271c3547f92701148eb1cf985c01abe22951b9d674282855` |
| C19 | 19360849 | 38136416 | 1 | 2494 / 3866 | `978abe1dfa0546e6bb7ce3e207fb80ceb8947b7bb8ecd9d6449efac3ed1e0a74` |
| C20 | 20494745 | 38136416 | 1 | 2616 / 3866 | `7b31f92772eb989ae7b51ea205092a876d7ceaa7d57e90fd467951f916f085f2` |
| C21 | 21937422 | 38136416 | 1 | 2737 / 3866 | `8da78737f3f7fa36befdbba36b0cfa103f2614e122937adeebc287610ae7fbbd` |
| C22 | 23547897 | 38136416 | 1 | 2867 / 3866 | `395a2c92aec677bd2fc60de010d931ac89074746c75df1d96d9c10c53766d9a2` |
| C30 | 38182330 | 38136416 | 1 | 3866 / 3866 | `81438c298d74380ec8bcc218baa2b07294ccf66db56f00552e61e60942a0ebc1` |

Current member-store SHA-256: `fc90980df4c62eac323d1ef04f4ee6fad131188bfd156c8f2a99dc5dc6f2875f`.

Current and inventory-commit Git member-store blob: `8d035507c532e27c1f32542db0cb1873f3f42e3b`. The working bytes match that Git object. Existing `git check-attr` reports `text=auto`, `eol=lf`. The member store did not exist as a Git path at the historical `11fa5d99...` baseline; it was local audit work before the inventory commit. Its historical raw snapshots are independent local evidence, not an invented old Git object.

| C30 auxiliary store | Bytes | Original/current SHA-256 | Actual byte status |
|---|---:|---|---|
| `control-review-decisions.json` | 271154252 | `da1564ad8b9ebbf44201fa6e961590280de337379a09af42b8febf3a34f3b28c` | BYTE_IDENTICAL |
| `wiring-review-decisions.json` | 4521593 | `66d2ba8a4b6f09c81d6ffbbdef83d159204983e3b94eb7f116d822f9ffc4d351` | BYTE_IDENTICAL |

The control-review store is local-only and absent from all three examined Git commits; exact local bytes are present now. A clone/ZIP without this 271,154,252-byte store or retained TestResults cannot reproduce the full dossier. The diagnostic explicitly reports EVIDENCE_UNAVAILABLE for missing sources/logs; it never synthesizes the missing audit store.

C30 member-store bytes contain 45,914 CRLF endings that became LF in the tracked checkout; the byte-length delta is 45,914. C13 has 1,312 prior member objects, with 1,199 raw-object byte mismatches and zero semantic differences. No actual historical source or evidence file was normalized.

## Original versus current checker meaning

| Check | Original validated scope | Current retained/current-run outcome |
|---|---|---|
| Original full-contract strict checker | Exit 2 for complete provenance with incomplete full contracts | Exit 1; frozen coverage/provenance naming old HEAD/main/branch does not reproduce |
| Accepted inventory-profile checker | Exit 0 with its complete original provenance and approved restrictions | Exit 1; frozen worktree/source/raw-byte conditions do not reproduce |
| 04A.02 supplemental diagnostic | Not an original acceptance checker | ORIGINAL_SNAPSHOT_NOT_REPRODUCIBLE_IN_CURRENT_CHECKOUT; explanatory CLI exit 2 is **not** strict-check exit 2 or a gate pass |

The original profile error list has 34 distinct conditions after deduplication: 15 accepted Office source/test changes, two authorized 04A.02 test-infrastructure edits, 14 audit-tooling/raw-store newline-only differences, and three frozen branch/worktree/raw-prefix conditions. Every original error is retained. No checker input, historical expectation or missing-file condition was suppressed.

The accepted Office commit changes real Help/navigation source behavior and tests; it is **not merely a provenance edit**. Its 15 changed paths and both Git object IDs are recorded. However, the strict analyzer inventories Windows/Core/Common and does not execute Office UI. The Office commit changes the source-provenance Git log record, while its source census remains 171 Windows compiled files / 3,866 members / 268 exports / 84 Ribbon entries. Current product files match the accepted current baseline; new Office behavior must not be compared to a pre-Office snapshot and called an unapproved regression.

## Owner decision memo and non-destructive resolution

04A.01 inventory acceptance with 1,514 restricted deferrals remains accepted by the owner. The current-checkout inability to replay old byte/provenance-bound checker success is an evidence reproducibility limitation, not a new owner rejection and not full-contract parity. All historical failures and restrictions remain visible.

Recommended next decision: preserve all original files; review this supplemental mapping and the current source-bound test evidence. A **new separately named/versioned branch-aware verifier is warranted for repeated future migration checks**, subject to separate scope approval, with explicit inventory/source pins, accepted Office commit handling, provenance rules, byte-vs-semantic rules and independent negative tests. Do not modify the old checker or rebaseline old raw stores, and do not normalize bytes to force PASS. This checkpoint does not implement that new acceptance verifier.

Supplemental evidence still needed for exact historical replay: the original branch/worktree/HEAD context and original uncommitted raw bytes as captured; original scratch override artifacts were not retained. Current snapshots/logs are present but do not supply a replayable Git worktree at that earlier decision point. Restoring the live checkout or manufacturing a historical Git object is not proposed.

New diagnostic negative tests deliberately alter only scratch JSON or scratch source bytes. They detect non-newline changes, same-length semantic drift, reordered prefix identity, exact-number distinctions and source-hash drift. They also reject absent local evidence and evidence overwrites. These are tests of explanatory diagnostics, never an alternative green 04A.01 acceptance gate.

Final historical protection, current checker invocations, exact test identities and source hashes are recorded in [Checkpoint 02 review](../batch-04A-02-checkpoint02-review.md). No native workbook or external numerical reference was executed.
