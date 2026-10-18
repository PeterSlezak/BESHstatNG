# Batch 00.02B — generated Common metadata and complete UDF help

**Baseline:** `BESHstatNG(20261014-133949).zip`, following developer-approved 00.02A.
**Master plan:** Stage304 revision 1.1, Batch 00.02B.
**Scope:** metadata projection, UDF documentation parsing, tests and generated docs only. This is not a handler migration.

## Source-of-truth boundaries

- `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml` owns authored method keys/titles, descriptions, categories, documentation slugs, Ribbon mappings and **optional** curated UDF associations (`udfNames`, or future `udfPatterns`). The two published `analysisId` fields are unchanged.
- `BESHstatNG/Resources/RibbonXML.xml` owns the deployed Windows Ribbon buttons and callbacks. `BESHstatNG/tools/validate_methods_catalog.py` continues to verify the manifest against the Ribbon (76 methods / 79 method buttons / 84 XML buttons / 5 utility commands as of this baseline).
- `BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb` and typed `AnalysisCatalog.vb` remain the authorities for **executable** registrations, option factories, schemas and the legacy enum. Exactly **two** analyses are registered. `methods.yml` cannot register handlers.
- Actual `<ExcelFunction>` declarations in `BESHstatNG/src/ExcelUDFs/*.vb` remain the authority for worksheet function names/signatures. The corrected documentation generator now extracts all **268** declared exports, across **12** categories.

## What changed

1. `BESHstatNG/tools/generate_analysis_metadata.py` validates the authored manifest through the 00.02A validator and deterministically generates `BESHStat.Excel.Common/src/Analysis/GeneratedMethodMetadata.vb`. The generated read-only `GeneratedMethodMetadata` catalog contains **76 method descriptions**, with only the two already-published `analysisId` values. The SHA256 of the source YAML is recorded as a comment; no runtime YAML dependency, host adapter, runtime reflection or handler dispatch is introduced.
2. `AnalysisCatalog.vb` now obtains **display name, category, plain-text description and help URL** from `GeneratedMethodMetadata.GetByAnalysisId`. Numeric algorithms, typed option factories, `ContractVersion`, IDs, schemas, output flags, availability and enum mapping are unchanged. Canonical authored labels are now **One-Way ANOVA** / **Kruskal-Wallis Test**, with categories **Parametric** / **Nonparametric**. The corresponding earlier label assertions in `AnalysisCatalogTests.vb` are deliberately updated; no executable contract changed.
3. `generate_udf_docs_v2.py` now recognizes an ExcelFunction attribute that starts **and closes on the same line**, fixing four missed exports (`BESH.PS.SCORES`, `.WEIGHTS`, `.EFFECT`, `.TABLE`). It rejects duplicate export names and malformed unterminated attributes.
4. New optional `--methods-yml` supports `udfNames` or regex `udfPatterns` associated with a documented method key/slug. The six approved associations under OneWayANOVA and KruskalWallisTest are verified against source function names and real help pages. The existing `--method-links-yaml` mechanism remains independent and fully compatible; use **both** flags when regenerating the current checked-in help pages so previously authored links are not lost.
5. `docs/udf/causal-inference.md` and `docs/udf/index.md` regenerated. All 12 group pages were regenerated/compared; the other 11 group pages remain byte-for-byte unchanged. Independent `udf-cookbook.md` and formula-syntax pages are untouched.
6. Additional .NET catalog tests and Python generation/parsing/compatibility/negative tests exercise metadata round-trips, deterministic checks, invalid references, exact source-export parity and link resolution.

## Regeneration and verification commands

Run from the **solution root** with Python 3.10+ and PyYAML installed:

```powershell
python -m pip install pyyaml
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v
```

When `methods.yml` changes after validation, deliberately regenerate and review the source diff:

```powershell
python BESHstatNG/tools/generate_analysis_metadata.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
```

Regenerate UDF help pages **without losing the existing links**:

```powershell
python BESHStatNG_Help_MkDocs/generate_udf_docs_v2.py `
  --udfs-dir BESHstatNG/src/ExcelUDFs `
  --output-dir BESHStatNG_Help_MkDocs/docs/udf `
  --docs-root BESHStatNG_Help_MkDocs/docs `
  --method-links-yaml BESHStatNG_Help_MkDocs/udf_method_links.yml `
  --methods-yml BESHStatNG_Help_MkDocs/docs/_meta/methods.yml
```

.NET regression (requires the local SDK/Visual Studio):

```powershell
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
# Also build the Windows .sln and run BESHStatNG.Test in Visual Studio.
```

Build the MkDocs help site with the project's existing MkDocs configuration/workflow and verify its method/UDF links. No `mkdocs_beshstatng.py` change was needed: the runner does not consume `methods.yml` directly.

## Manual smoke and expected acceptance

1. In Windows Excel-DNA, open One-Way ANOVA and Kruskal–Wallis help and run the existing ANOVA (Welch/post-hoc) and KW (Dunn) golden examples; output layout/p-values must match 00.01 fixtures.
2. In WasmSmoke/Excel Online, the catalog dropdown now displays the canonical **One-Way ANOVA** and **Kruskal-Wallis Test** labels; both must execute and write as before. No other YAML-only method should become selectable.
3. In MkDocs, open UDF categories **Parametric**, **Nonparametric**, **Causal Inference** and the UDF index. The four PSM function headings must each appear once, and previously curated dialog cross-links must remain present.

**Validation performed here:** all 12 00.02A Python tests plus 3 metadata tests, all 5 UDF documentation tests, manifest reconciliation, generator `--check`, exact count/uniqueness and Markdown-link checks. **Not executed here:** .NET builds/unit tests, Windows/Office.js Excel GUI smoke tests, MkDocs site build. These require your development environment.

## Backward compatibility and rollback

No `BESHStat.Core` changes, Windows host VB changes, WASM host changes, or ExcelFunction exports. `SpreadsheetAnalysisKind` numeric values, serialized IDs, option factories, help URI paths, and statistical results remain unchanged. Display labels/categories/descriptions now follow the authored YAML; tests intentionally assert the new values.

Revert the patch, or restore its listed replacement files from the baseline ZIP, if validation fails. Generated metadata can always be recreated using `--check` and without running a browser or Excel. Once locally approved, proceed with **Batch 00.03 — strongly typed handler registry**, using the two existing IDs and 00.01 fixtures.
