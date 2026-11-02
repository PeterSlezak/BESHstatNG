# C07 scientific decision form: answer APPROVE, AMEND or DECLINE per case

**RECOMMENDATIONS ONLY. S remains PENDING; R/P/N remain NO.** No statistical R process, reference result or new golden is created. A new attributable domain-owner answer must identify the exact policy/fixture/script/hash, reviewer, date/source and every amended field; the agent cannot supply it. Separate roadmap R permission is still required.

Recommendation: use the frozen classical/asymptotic methods below for these small finite synthetic datasets, with the deliberately case/field-specific budgets. Ordinary p domain proposal is [1e-12, 1-1e-12], inclusive; outside it stop for a new extreme/underflow policy. This domain protects unreviewed tails, not a claim of uniform relative accuracy. These recommendations precede independent comparison and are not fitted to agreement.

All labels/group/RowId ordering, exact integer tokens, options/alpha/seed/culture, schema/types/nulls/errors and table/p coordinates are exact. GroupedNumericData does not transport RowIds. Finite comparison rule, where approved: abs(diff) <= max(atol, rtol*abs(reference)); zero budgets mean exact. Native rendered text/NumberFormat/geometry are not approved by this form.

## C30-SYN-ANOVA / C07-S-ANOVA-v1

Input: `{"group_labels": ["A", "B", "C"], "values": [[1, 2, 3], [2, 4, 6], [3, 5, 7]], "row_ids": [[101, 103, 107], [109, 113, 127], [131, 137, 139]]}`; alpha .05, seed N/A, all optional posthoc/Welch/Dunn flags false. Fixture/data/script hashes and exact source field/table mappings are in the companion JSON.

Classical equal-variance ANOVA: df between2/within6/total8, upper F tail; R stats::oneway.test(var.equal=TRUE) and independent anova(lm(...)) SS/MS. No Welch.

| Actual reference field | Proposed atol / rtol | Subject mapping / rationale |
|---|---|---|
| SSBetween | 1e-12 / 2e-13 | table0[1,1]; N=9, bounded finite inputs 1..7; small accumulation/division scale, separate sum/mean budget; not tuned to agreement |
| SSWithin | 1e-12 / 2e-13 | table0[2,1]; N=9, bounded finite inputs 1..7; small accumulation/division scale, separate sum/mean budget; not tuned to agreement |
| SSTotal | 1e-12 / 2e-13 | table0[3,1]; N=9, bounded finite inputs 1..7; small accumulation/division scale, separate sum/mean budget; not tuned to agreement |
| MSBetween | 1e-12 / 2e-13 | table0[1,3]; N=9, bounded finite inputs 1..7; small accumulation/division scale, separate sum/mean budget; not tuned to agreement |
| MSWithin | 1e-12 / 2e-13 | table0[2,3]; N=9, bounded finite inputs 1..7; small accumulation/division scale, separate sum/mean budget; not tuned to agreement |
| FStatistic | 5e-13 / 5e-12 | service.FStatistic / table0[1,4]; Ratio of bounded sums with subtractive/rounding allowance distinct from tail function |
| PValue | 2e-14 / 2e-10 | service.PValue / table0[1,5]; Independent upper F tail approximation budget; only the separately proposed ordinary-p domain |
| DFBetween | 0 / 0 | table0[1,2]; Structural integer degrees of freedom; no floating allowance |
| DFWithin | 0 / 0 | table0[2,2]; Structural integer degrees of freedom; no floating allowance |
| DFTotal | 0 / 0 | table0[3,2]; Structural integer degrees of freedom; no floating allowance |

For every field: APPROVE exact recommendation / AMEND with exact value and rationale / DECLINE: **PENDING**. Domain owner / role / date / source / policy hash: **not supplied**.

## C30-SYN-KW / C07-S-KW-v1

Input: `{"group_labels": ["A", "B", "C"], "values": [[1, 2, 2], [2, 4, 6], [3, 5, 7]], "row_ids": [[101, 103, 107], [109, 113, 127], [131, 137, 139]]}`; alpha .05, seed N/A, all optional posthoc/Welch/Dunn flags false. Fixture/data/script hashes and exact source field/table mappings are in the companion JSON.

KW: average pooled midranks, raw H and corrected H/C kept separate; R kruskal.test corrected pair plus separately reviewed raw-H/tie formula and raw upper pchisq. df2 asymptotic upper chi-square tails only, no exact/permutation/Dunn.

| Actual reference field | Proposed atol / rtol | Subject mapping / rationale |
|---|---|---|
| HStatistic | 5e-13 / 5e-12 | service.HStatistic / table0[3,1]; Rank-sum accumulation/cancellation and correction division, preserve each named statistic separately |
| TieCorrectedHStatistic | 5e-13 / 5e-12 | service.TieCorrectedHStatistic / table0[5,1]; Rank-sum accumulation/cancellation and correction division, preserve each named statistic separately |
| formula_corrected_H_for_separate_review | 5e-13 / 5e-12 | independent reference auxiliary H/C, reviewed against corrected oracle; Rank-sum accumulation/cancellation and correction division, preserve each named statistic separately |
| PValue | 2e-14 / 2e-10 | service.PValue / table0[4,1]; Separate raw/corrected upper chi-square tails; never compare corrected reference to raw service field |
| TieCorrectedPValue | 2e-14 / 2e-10 | service.TieCorrectedPValue / table0[6,1]; Separate raw/corrected upper chi-square tails; never compare corrected reference to raw service field |
| tie_correction | 5e-15 / 1e-13 | reference-only auxiliary; current application result has no tie-factor field; Dimensionless correction near unit scale; reject C<=0; reviewed denominator, no generic statistic tolerance |
| rank_sums[A,B,C] | 0 / 0 | reference-only auxiliary ordered A/B/C; current service has no rank-sum fields; For N=9 pooled midranks are integer/half-integer and bounded sums are exactly binary64 representable; no tie-breaking by RowId |
| DF | 0 / 0 | reference-only auxiliary / source k-1; Structural k-1 integer, not approximate inference degree |

For every field: APPROVE exact recommendation / AMEND with exact value and rationale / DECLINE: **PENDING**. Domain owner / role / date / source / policy hash: **not supplied**.

## Unsupported/edge policy requiring explicit adoption

Reject absent/nonfinite/empty/inconsistent inputs and outputs; no imputation/drop/coercion-to-zero. New singleton/all-equal/zero within-variance/zero-or-negative tie-factor/singular/overflow/underflow/warning cases are outside the two fixed references. Reject invalid JSON, duplicate/missing fields, stderr/warnings, nonzero/partial output and unapproved tail boundaries. Do not redefine production behavior for these cases; propose a separately reviewed negative numerical batch. No CI/multiplicity/extra method is implied.

KW rank sums are reference-only auxiliaries: current Common result does not expose them or tie C; do not invent service fields. Raw p maps to raw service PValue, corrected p to TieCorrectedPValue. ANOVA SS/MS/df map to table cells; primary F/p to both typed fields and matching cells. Exact output ordering/type/p metadata is independent of numeric tolerance.

The owner prompt supplied provisional plausibility F=7/3 and p=729/4096. These are **not admitted expected values**, were not newly evaluated as a substitute oracle and were not used to tune tolerances. Current source records retain numeric_expected_values=null.

After S: separately decide R for the exact runtime/operator/script/output root and ACL/loader review. Frozen operator private HOME/library readiness gap CR-R-PRIVATE-ENV-07 must be resolved with a reviewed new version, not silently patched. After actual authorized output and real comparison: two independent reviewers and later P. No promotion now.
