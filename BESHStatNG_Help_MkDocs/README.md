# BESHStatNG_Help_MkDocs — method and UDF documentation

This project contains the source for the [published BESHStatNG help](https://beshstat.eu/beshstatng/help/latest/), including authored method guides, UDF reference pages, examples, downloadable datasets, screenshots, settings guidance and release notes. The published material primarily documents the **supported Windows Excel-DNA product**; Office.js/WebAssembly remains a development client. Its current four-tab task pane links to version-aware home, method, global-settings and Chart Export help; chart export itself is not implemented in Office.

## Sources of truth

- `docs/_meta/methods.yml` is the canonical **authored method/help metadata catalog** (stable identity, classification, URLs and Windows Ribbon correspondence).
- `../BESHstatNG/Resources/RibbonXML.xml` is authoritative for buttons/callbacks actually declared by the Windows Ribbon.
- `<ExcelFunction>` attributes in `../BESHstatNG/src/ExcelUDFs/` are authoritative for actual worksheet-function exports.
- `udf_method_links.yml` provides curated links from UDF entries to method help; method metadata should be reconciled, not duplicated in independently edited inventories.

The YAML describes **documented methods**, not proof that the Office.js application exposes an executable handler for each. The generated Common method catalog is navigation/help metadata; actual execution is guarded by the typed analysis-handler registry.

## Relevant scripts

| File | Purpose |
| --- | --- |
| `mkdocs_beshstatng.py` | Builds local latest and versioned help trees based on `../Directory.Build.props` |
| `generate_udf_docs_v2.py` | Generates UDF reference pages from VB attributes/comments with curated method links |
| `generate_help_resolver_routes.py` | Generates the version-aware method-help route allowlist from validated metadata |
| `mkdocs_hooks.py` | Static build hooks, including JavaScript module MIME support for latest help |
| `update_udf_docs.ps1` | UDF documentation update workflow |

Windows generated `HelpLinks.vb`/`HelpContext.vb` are produced by tooling under `../BESHstatNG/tools/` and should not become a second hand-edited catalog.

## Build and preview

Install Python, MkDocs and the configured theme/extensions. From this directory:

```powershell
python mkdocs_beshstatng.py --mode serve --project . --props ../Directory.Build.props
python mkdocs_beshstatng.py --mode build --project . --props ../Directory.Build.props
```

The build creates a latest site and a version-specific site under `.site/`. The documentation version derives from the shared four-component `BeshVersion`; use the value present in the checked-in build properties rather than a fixed value in documentation. Do not assume rebuilding local pages publishes them.

The Help resolver first verifies the exact requested published version's method page and then falls back to the latest equivalent; it is bounded and does not enumerate older releases. Latest output includes `assets/javascripts/.htaccess` to serve `.mjs` with a JavaScript MIME type when the web server honors that configuration. Upload hidden files and test real HTTP status, content type, redirects and canonical URLs after publication. The shared `BeshHelpResolverPublished` build setting in `Directory.Build.props` must reflect the owner's verified publication state; coordinate host rebuilds if changing it.

## Validation before documentation changes

From the repository root:

```powershell
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -B BESHStatNG_Help_MkDocs/generate_help_resolver_routes.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v
node --test BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs
```

For UDF updates inspect `python generate_udf_docs_v2.py --help`; use its supported source/output and method-link arguments, review generated diffs and validate that the exported function inventory is represented. Keep hand-authored method pages and historical released-version trees intact unless explicitly updated.

## Development direction

Maintain one consistent method identity across published help, Windows Ribbon behavior and Common navigation. As the Office add-in grows, clearly distinguish the Windows Chart Export guide from the Office Export Chart informational tab, and describe only supported Office UI/host features in client-specific material; do not silently portray every Windows method or UDF as already available on Mac/Excel Online. Continue generating help/UDF pages from their appropriate authorities and testing the output before publishing.
