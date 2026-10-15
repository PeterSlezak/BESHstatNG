# BESHStatNG Help — MkDocs documentation source

This folder contains the source for the [published BESHStatNG help](https://beshstat.eu/beshstatng/help/latest/): method guides, worksheet-function reference pages, usage examples, screenshots, global settings and release notes. The published documentation primarily describes the **supported Windows Excel-DNA add-in**, not unreleased experimental clients.

## Content and metadata

- `docs/` — authored Markdown pages and assets.
- `docs/_meta/methods.yml` — canonical **authored user-facing method catalog** (identity, help slug, categorization and Ribbon correspondence).
- `mkdocs.base.yml` — main navigation and MkDocs configuration.
- `mkdocs_beshstatng.py` — previews/builds latest and versioned documentation using `../Directory.Build.props`.
- `generate_udf_docs_v2.py` — generates UDF reference pages from `<ExcelFunction>` declarations and VB XML comments in the Windows add-in source; supports curated method links.
- `udf_method_links.yml` — existing additional UDF-to-method link rules.

The actual Windows Ribbon registration remains `../BESHstatNG/Resources/RibbonXML.xml`; the exported UDF declarations remain in `../BESHstatNG/src/ExcelUDFs/`. `methods.yml` is **not** an executable analysis registry.

## Preview and build

Install Python, MkDocs and the theme/extensions used in `mkdocs.base.yml` (including Material and PyMdown Extensions). From this folder:

```powershell
python mkdocs_beshstatng.py --mode serve --project . --props ../Directory.Build.props
python mkdocs_beshstatng.py --mode build --project . --props ../Directory.Build.props
```

The build generates both a `latest` and a versioned site under `.site/`. Publish only through the established release workflow.

## Metadata and UDF validation

From the repository root:

```powershell
python BESHstatNG/tools/validate_methods_catalog.py
python BESHstatNG/tools/generate_analysis_metadata.py --check
python -m unittest discover -s BESHstatNG/tools/tests -p "test_*.py" -v
python -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p "test_*.py" -v
```

When regenerating UDF pages, run `generate_udf_docs_v2.py` with `--udfs-dir`, `--output-dir`, and the applicable `--method-links-yaml` / `--methods-yml` options. Review the generated diff before committing so manually authored method pages remain untouched. Full script options are visible with `--help`.

See the [main README](../README.md) for product status.
