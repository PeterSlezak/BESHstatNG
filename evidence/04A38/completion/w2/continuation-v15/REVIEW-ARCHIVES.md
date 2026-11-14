# Continuation v15 review archives

This is a partial W2 checkpoint: NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED.

The external `continuation-v15-review-parts-index-v1.json` identifies every actual ZIP by size and SHA-256 and every logical member by normalized path, size, SHA-256 and stored CRC32. Each ZIP is independently valid and below180MiB. ACTIVE and REFERENCE parts form one unique complementary logical union. They are not binary `.zip.001` volumes and require no concatenation or outer-wrapper reassembly.

Extract every part into the same new empty directory, preserving repository-relative paths. Do not create a partial `.git` directory. No payload is duplicated between parts. Keep the external parts index and postseal handover alongside the archives as separate review authorities.

From the extracted repository root, run the actual final publication reader:

```text
<python> -B evidence/04A38/completion/w2/continuation-v15/verify_c15_publication_v2.py --index <external-index.json>
```

To validate the physical bytes received by the reviewer as well:

```text
<python> -B evidence/04A38/completion/w2/continuation-v15/verify_c15_publication_v2.py --index <external-index.json> --parts --parts-dir <directory-containing-the-ZIPs>
```

The actual copied-union replay is performed locally with the mandated Windows Python interpreter. No production installation, cross-platform standalone validation or external upload is claimed. The physical archive SHA values must be checked against the actual bytes uploaded or downloaded; hashes of an outer wrapper do not substitute for the indexed source ZIP hashes. Locally retained second serializations prove determinism, but those duplicate physical copies need not be uploaded; the reader distinguishes supplied versus omitted reproduction bytes.

Read `owner-review.md`, `review-seal-v1.json`, the current ledger `current-ownership-v3.json`, historical ledger `historical-ownership-v2.json`, exact next queue `remaining-work-queue-v2.json`, and the external postseal handover. Earlier drafts and failed/interrupted attempts remain preserved and are not substituted for the designated final readers.

Review-only exclusions retain their original local bytes. Separately named cleaned derivatives preserve raw-source hashes and do not overwrite authority. The two historical Python privacy JSON ERRORs remain ERROR, including the immutable malformed v9 aggregation; selected review-payload policy verification does not waive them. The independent private-preservation and live Git/index receipts require the retained original workspace and are not inferred from this review copy.

The owner authorized one replacement comprehensive Debug campaign and one replacement interrupted40-parent Python scanner lane. The916 completed Python parents were not rerun. Their972 known total parent starts and956 complete unique effective results are explicitly reconciled, including13 partial prior outcomes and one interrupted unknown outcome. Full and selected Release unit suites remain NOT_RUN_OWNER_PREFERENCE.

The owner additionally approved two focused privacy-parent corrections for an accelerator exception-translation defect. The two raw accelerator FAILs remain preserved; actual original-scanner ERRORs and prior malformed-JSON ERROR chronology remain distinct. Authenticated successful per-file results are reused without another bulk scan.
