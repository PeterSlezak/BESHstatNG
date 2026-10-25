# Gate C — owner decision and 04A.01 authorization

**Decision date:** 25 October 2026  
**Decision authority:** Project owner, via explicit confirmation in the project conversation  
**Source/audit baseline:** `da1ffa27303d66c562881b8a6b78d0f2711bc3e3` on `codex/gate-c`; the merged source and later on-disk evidence must be rechecked before any implementation patch  
**Decision:** **GO WITH DOCUMENTED EXCEPTIONS — APPROVED**  
**Next authorized numbered batch:** **04A.01 — Comprehensive Windows Code Inventory and Portability Audit** (not yet begun or accepted)

## Owner declaration and scope

The owner confirmed that all four residual genuine Excel Online checks were fine and passed: actual Help-page opening; keyboard navigation and focus return; repeated-subject and duplicate validation; busy, Cancel and Retry behavior. When explicitly asked to approve **Gate C — GO WITH DOCUMENTED EXCEPTIONS**, accept E1–E7 and defer GC3–GC8, and authorize 04A.01, the owner answered **“yes, approved.”**

This is an explicit **owner attestation and governance decision**, **not** an independent rerun of the four cases, a newly archived native screenshot/log, or a cryptographically signed document. The prior audit's **NO_GO recommendation and missing native evidence observations remain historical facts**. The owner elects to accept the remaining evidence limitation and closes **GC1 for Gate C by owner acceptance**. **GC2 is closed by the explicit owner decision**. The current decision supersedes the earlier unsigned recommendation for governance, without rewriting prior test outcomes.

## Exception acceptance and deferred obligations

- **E1–E7:** previously accepted Gate B exceptions **carried forward OPEN**, with existing responsibility and triggers unchanged.
- **GC3–GC8:** owner-acknowledged **DEFERRED**, not repaired, waived, or silently closed. Keep their recorded owner roles, future batch/gates and triggers in `TestResults/GateC/exception-register.md`.
- **GC1:** closed for this gate by owner attestation and acceptance of the evidence limitation. Preserve the four case statuses as `OWNER-ATTESTED PASS`, never relabel as executed automation or current-source native captures.
- **GC2:** closed by this owner approval; this text documents consent, not a cryptographic signature.

## Evidence and platform boundaries

The Gate C audit recorded **4,075 distinct checks per Debug and Release configuration** and **21 simulated Edge scenarios**. Subsequent genuine Office observations include the real Ribbon, ANOVA/KW and **five verified Completed writes**, numeric p-values and colors, and LM widths **3 → 4 → 5**. The four final checks are **owner-reported**. The absence of independently captured evidence for those four remains disclosed, not filled with fabricated artifacts. The cited initial source/manifest and the source-bound live evidence must remain distinguished from the owner's later confirmation.

Windows desktop Excel command/XLL loading and Mac desktop qualification remain **NOT TESTED** at this gate and assigned to Phase 12/13 / Gate G. This decision does **not** enable Chart Export, New Workbook output, unsupported statistical methods, Office Marketplace deployment or other unimplemented capabilities.

## Action and evidence handling

1. Preserve the versioned [Gate C audit](gate-c-office-application-layer-exit.md), including its historical audit results and append-only owner decision addendum.
2. On the *actual Windows checkout*, append an `OWNER-ATTESTED` decision entry to the latest ignored `TestResults/GateC/evidence-index.json`, and add closing notes to `exception-register.md` and `verification-summary.md` **without replacing existing evidence or fabricating native run IDs, SHA-256 hashes or screenshots**. The ignored local artifacts were not present in the uploaded ZIP and are not recreated here.
3. If the audit branch is still uncommitted, commit/merge only after the owner reviews the documentation changes. No source, installer, deployment, README, method catalog, manifest, or version changes are part of this decision.
4. Begin **04A.01** on a new approved feature branch from the current merged `main`, using fresh source inspection and the [Phase 04A plan](../../planning/BESHStatNG_CrossPlatform_MasterPlan.md). Gate C is **not** a numbered batch.

**Roadmap checkpoint remains: 40/223 accepted, 183 remaining; Phase 04A 0/75 implemented.**
