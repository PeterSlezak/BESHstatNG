# W3 independent review transport

This is a software verification candidate. Phase 04A.38 is not accepted, and no native, scientific, manual, installation, signing, deployment or 04A.39 gate is released.

The transport has complementary components:

- New `BESHstatNG_04A38_W3_ACTIVE_v1.zip`.
- Ten unchanged W2 v17 post-correction core ZIPs, plus the separate unchanged W2 `POSTSEAL_SUPPLEMENT.zip`.
- A separately named final `BESHstatNG_04A38_W3_POSTSEAL_v1.zip`, created after the main transport readback and preservation audit.

The final delivery receipt supplies actual physical hashes, the common logical inventory, final source/Git receipts and private union accounting. Existing W2 archives are reused as immutable reference components; they are not rebuilt or claimed newly uploaded. These thirteen ZIPs are independent complementary packages, not binary volumes. Each physical archive is below 180 MiB when its verification receipt passes.

For a flat folder containing the twelve main ZIPs, use the standalone reader and extracted metadata:

```text
python -I -B verify-review-v1.py transport-manifest-v1.json parts-index-v1.json <flat-zip-folder>
python -I -B verify-postseal-v1.py postseal-physical-readback-v1.json <W3-POSTSEAL.zip>
```

The first reader verifies every physical ZIP hash, complete entry sets, safe paths, case-sensitive and case-insensitive uniqueness, sizes, SHA-256 and CRC. It independently reconciles published managed and Python censuses. The second verifies the separate final supplement. Local main readback also extracts the new ACTIVE payloads to a new ignored directory; unchanged reference parts receive complete streaming payload readback.

These readers attest derivative bytes and published censuses. They do not reproduce live Git, the private file system, compilation, raw TRX or worker execution. Private raw files, owner documents, all historical physical archives, live `.git/index` and the large interrupted artifact remain local. Authenticated exclusions and separately named sanitized derivatives preserve their raw authority hashes. No repository-wide privacy PASS or new privacy waiver is asserted.

Read the final `owner-review.md`, `readiness.json`, unresolved-exceptions register and `next-gate-handover.md`. Keep inherited managed, Python, strict and profile nonpasses distinct from passing checks. Keep the 28,966 pre-edit and 458 supplemental cases separate. Review final postseal receipts as additive evidence; do not discard failed/stopped attempts.

If the owner later chooses 7-Zip binary splitting around a 0-store outer ZIP, concatenate `.001`, `.002`, etc. in numerical order and then extract the outer archive to obtain the original ZIP. Such volumes are not independent source packages. No splitting or external upload is performed by this task.

`W2_OWNER_ACCEPTED; W3_VERIFICATION_CANDIDATE_OR_BLOCKED; NOT_READY_04A38; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED`
