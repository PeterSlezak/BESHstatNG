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

Both this wrapper and direct `python -B -m mkdocs build --config-file mkdocs.latest.yml`
builds automatically generate `.site/latest/assets/javascripts/.htaccess` in the
final static output. The shared `mkdocs.base.yml` post-build hook uses the configured
`site_url` ending in `/latest/` and the actual `site_dir`, including `--site-dir`
overrides. It recreates/repairs the file on every build and verifies its UTF-8/LF
bytes; a write or validation failure fails the build. Versioned builds do not
generate this file. The directive is limited to `.mjs`:

```apacheconf
<IfModule mod_mime.c>
    AddType text/javascript .mjs
</IfModule>
```

When uploading `.site/latest/`, include hidden/dotfiles and explicitly verify
`assets/javascripts/.htaccess` was uploaded: some FTP/File Manager tools omit
dotfiles by default. Preserve existing published version directories; uploading
latest must not replace them. Apache/LiteSpeed must honor `.htaccess` and permit
`AddType` (with `mod_mime` available). Static generation cannot set HTTP headers
when the host ignores this file.

After deployment, use browser **DevTools → Network** to check that both
`help-resolver.mjs` and `help-routes.mjs` return HTTP 200 and an accepted JavaScript
MIME type (`text/javascript` recommended). Then test version-first Help and the
same-page `/latest/` fallback from Windows Excel-DNA and Excel Online. Local
generated-artifact tests do not verify live hosting headers or browser routing.

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

## Batch 03.05 version-first Help: publication and activation

The unlisted resolver source is `docs/help-resolver/index.html`, with local
modules `docs/assets/javascripts/help-resolver.mjs` and generated `help-routes.mjs`.
It is a standalone static page copied by MkDocs, so Material's instant-navigation
or Markdown processing does not strip its module script. It is not a new method,
utility or navigation entry. Nothing in this branch publishes the site.

Required published URL:
`https://www.beshstat.eu/beshstatng/help/latest/help-resolver/`.
Its modules must be available under the same `/latest/assets/javascripts/` root.
The native Windows/Office browser launch supplies only a canonical four-part
version and a known catalog key, for example
`?version=1.2.5.0&route=OneWayANOVA`. The browser checks that exact version's
method page, then the corresponding latest page. It never searches older versions.

Offline preparation, from the repository root, using the installed Python with
PyYAML, MkDocs and Material (no automatic installs):

```powershell
python -B BESHStatNG_Help_MkDocs/generate_help_resolver_routes.py --check
python -B -m mkdocs build --config-file BESHStatNG_Help_MkDocs/mkdocs.latest.yml
node --test BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs
python -B -m unittest discover -s BESHStatNG_Help_MkDocs/tests -p test_help_resolver.py -v
```

If metadata is intentionally changed, run `generate_help_resolver_routes.py`
without `--check`, review its deterministic diff, then rerun checks. Routes derive
from validated `methods.yml`; do not edit the generated allowlist. Full accepted
gates invoke the route check, MkDocs output tests and resolver Node suite.

Owner publication/readiness checklist (not performed by Codex):

1. Review `.site/latest/help-resolver/index.html` and the two local module files.
   Publish the latest site through the established owner workflow; preserve
   existing version folders. Publishing this resolver does not require publishing
   a new `v1.2.6.0` tree or changing DNS/server redirects. Do not replace a version
   directory with latest content.
2. Verify the exact resolver URL and both modules load with HTTP 200 at the
   canonical `https://www.beshstat.eu` origin. Serve `.mjs` with a JavaScript MIME
   type; permit same-origin module scripts and same-origin HEAD/GET in CSP.
   Build tests prove file paths/contents, not live server MIME/CSP behavior.
3. Open the resolver directly with `version=1.2.5.0&route=OneWayANOVA`, then
   `KruskalWallisTest`. If those exact version pages are published, the final
   browser destinations must remain under `/v1.2.5.0/methods/…/`. Test a version
   with a missing method page, including a version whose root alone exists:
   the final destination must be the identical `/latest/methods/…/` page.
4. Verify redirects/custom 404s do not claim success: the final response URL,
   canonical link and a non-error Material document heading must identify the
   requested page. HEAD 405/501 is handled by GET. All checks (HEAD, GET and body)
   share a 1.5-second deadline per candidate, at most two candidates/three seconds.
   Redirects, non-200, opaque responses, wrong canonicals, error headings, unreadable
   or over-512-KiB documents are not confirmed. Missing latest remains on the
   resolver with an offline/help-unavailable message and trusted home link.
5. Only after these checks, explicitly build **both** Windows and the WASM pilot
   with `/p:BeshHelpResolverPublished=true` (MSBuild) or
   `-p:BeshHelpResolverPublished=true` (dotnet). The default in Directory.Build.props
   is false; this shared Common compilation flag keeps release-facing Help on
   working latest links until publication is confirmed. It is not a visible setting
   or a runtime URL/environment override. Rebuild both hosts when changing it.
6. Run the amendment's Windows/Online owner acceptance matrix in the architecture
   report. Confirm actual running assembly versions, correct selected page,
   responsive Excel, native separate-tab Office behavior and unchanged consent.
   If publication is withdrawn, rebuild with the flag false before distributing.

Hosting caveat: legacy Material pages must retain accurate canonical URLs. A
server that fabricates a matching canonical and normal document heading on an
HTTP-200 error defeats those identity signals; correct that publication behavior
before activation. Unexpected redirects are deliberately rejected, even if a
server considers them benign. No live route/MIME/CSP check is claimed here.
If the resolver page itself cannot load offline, the browser may show its normal
offline error; Excel and the task pane never wait for network probes. With JS
disabled or unavailable, a static accessible latest Help link remains.
