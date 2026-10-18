# BESHStatNG — Batch 00.02A: Canonical methods.yml reconciliation

**Source baseline:** `BESHstatNG(20261014-124457).zip`, updated plan 1.1, 14 October 2026.  
**Scope:** authored metadata and read-only validation; no changes to Core, Common APIs, execution, Ribbon XML or Office.js.

## Architectural contract

- `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` authors user-facing *method* keys, titles, categories, short descriptions, slugs, WinForms help associations, Ribbon mappings and **optional** `analysisId` associations.
- `BESHstatNG/Resources/RibbonXML.xml` remains the deployed authority for Ribbon button IDs and `onAction` callbacks. `methods.yml` never generates or changes this XML.
- `BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb` remains the compiled authority for the existing runtime analysis IDs; `methods.yml` **does not** register executable handlers. In 00.02A only `OneWayANOVA` and `KruskalWallisTest` receive `analysisId` mappings.
- Five non-analysis Ribbon commands are separately tracked under `commands`, with `pageKey` links for Settings and Export Chart. `pages` stays unchanged, preserving the existing `generate_help_links.py` consumer.
- `<ExcelFunction>` attributes, not `methods.yml` or old inventory CSVs, remain authoritative for exported UDFs. The UDF documentation parser is **not** changed here; it belongs to 00.02B.

## Manifest corrections

| Method | Previous problem | Checked-in correction |
|---|---|---|
| Bullet Chart | `Ui01BulletChart` entered as callback/handler; missing from curated Graphics list | `OnbtmBulletPressed`; Graphics nav entry added |
| Bland–Altman | `Ui9ANOVA2nested` mistakenly used as Ribbon ID | `btn10_3Bland` |
| Lin CCC | `UiTwoInputRefedits` mistakenly used as Ribbon ID | `btn10_4LCCC` |
| Cohen/Weighted Kappa | `UiTwoInputRefedits` mistakenly used as Ribbon ID | `btn10_5Cohen` |
| ICC | `btn10_3icc` stale | `btn10_6icc` |
| Parametric/Sample Size nav | Five category slugs disagreed with method slugs and MkDocs pages | Paired test + four sample-size navigation slugs reconciled |
| One-way ANOVA | No runtime identity link | `analysisId: anova.oneway` |
| Kruskal–Wallis | No runtime identity link | `analysisId: nonparametric.kruskal-wallis` |

Unchanged: the 76 method `key`/`slug` pairs, two page slugs, form names, form titles, statistical code and registered runtime IDs. Some methods legitimately appear in more than one curated category (Kaplan–Meier, correspondence analysis and Cox regression); these are not errors.

## Source-based reconciliation and expected current checkpoint

Run from the **solution ZIP extraction root**:

```powershell
python -m pip install pyyaml
python BESHstatNG/tools/validate_methods_catalog.py
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
```

Optional machine-readable report:

```powershell
python BESHstatNG/tools/validate_methods_catalog.py --json-report methods-catalog-validation.json
```

Current source-derived counts: **76 methods, 76 distinct method slugs, two pages, 12 categories, 79 distinct method→Ribbon mappings, 84 total XML buttons, five explicit utility commands, 79 curated category references, two registered execution IDs**. These counts are fixed as a **checkpoint regression test**; the validator itself does not impose a permanent product feature limit.

The validator exits non-zero on invalid YAML, missing method docs and MkDocs navigation files, unknown or duplicated Ribbon IDs, wrong callbacks, unknown form classes/handlers, ambiguous normalized form+title help keys, stale curated category slugs, missing method category entries, undeclared runtime IDs, or orphan Ribbon commands. It compares the checked-in VB `AnalysisIds.vb`, declared Ribbon XML and existing `.md` files. It creates no files unless `--json-report` is explicitly provided.

The 12 Python `unittest` tests also exercise representative negative cases and check that regenerating `HelpLinks.vb` and `HelpContext.vb` yields content **identical to the checked-in files apart from the timestamp comment**. Because only callback/Ribbon/category metadata changed, regenerated help source does not contain a semantic delta; **there are no Windows VB changes in this batch**. `generate_help_links.py`, `mkdocs_beshstatng.py`, `mkdocs.base.yml`, and `generate_udf_docs_v2.py` are intentionally unchanged.

## Recommended developer verification

1. Apply the patch (or copy the four standalone replacement files). Run the Python validator and 12 tests above. Build the full Windows solution and run existing `BESHStat.Core.Tests`, `BESHStat.Excel.Common.Tests`, and legacy `BESHStatNG.Test` as usual; 00.01 golden fixtures and 00.02 descriptor tests must remain green.
2. Launch Windows Excel-DNA and open **One-Way ANOVA** help, then **Bland–Altman**, **Lin CCC**, **Cohen/Weighted Kappa**, **ICC**, and **Bullet Chart** from the Ribbon. Confirm forms open, their help routes to the intended method pages, and Settings/Help/Export Chart remain accessible. These callbacks were **not changed** in this patch.
3. Optionally run your existing MkDocs build. The manually curated `mkdocs.base.yml` nav was not regenerated. Inspect `methods.yml` categorization for Bullet Chart and the corrected sample-size labels/links.

**Environment caveat:** Python validation/tests were executed while preparing this patch. `.NET` build, Excel GUI and MkDocs command execution were not available in this environment; those require local validation. No claim of new runtime numerical parity beyond your already accepted 00.01/00.02 baseline is made.

## Rollback and next step

Rollback consists of reverting only the modified `methods.yml` and removing the two Python validation files and this document. No compiled application code or public DTO changes were introduced.

Once accepted, **00.02B** can generate descriptive metadata from the validated YAML and repair `generate_udf_docs_v2.py` so all **268** exported UDFs—including the four currently omitted PSM functions—are represented, before **00.03** introduces typed handlers.
