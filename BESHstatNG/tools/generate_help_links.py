#!/usr/bin/env python3
"""Generate VB.NET help link mapping from MkDocs metadata (methods.yml).

Source of truth: docs/_meta/methods.yml

Outputs (written into the VB project):
  - src/Help/HelpLinks.vb   (HelpTopic enum + URL builder)
  - src/Help/HelpContext.vb (Resolve help topic from Form.Tag or Form.Text)

Fix vs older generator:
  - Context resolution now uses (FormClassName + normalized Form.Text), so dialogs that share
    the same caption (e.g. "Paired T-test" analysis vs "Paired T-test" sample-size) resolve correctly.
  - If ambiguous/unresolved, falls back to Home (BaseUrl).

Usage:
  python tools/generate_help_links.py --methods-yml "..\\BESHStatNG_Help_MkDocs\\docs\\_meta\\methods.yml" --project-dir ".."
  Add --check to validate both checked-in files without writing files/directories.

03.05: deterministic source-hash headers; Common preserves latest URLs and stages
version-aware browser launches. No website probing runs on the Excel UI thread.

Dependencies:
  pip install pyyaml
"""

import argparse
import hashlib
import re
import sys
from pathlib import Path

try:
    import yaml  # type: ignore
except ImportError:
    print("Missing dependency: pyyaml. Install with: pip install pyyaml")
    sys.exit(2)


def vb_ident(name: str) -> str:
    """Make a safe VB identifier (enum member)."""
    s = re.sub(r"[^A-Za-z0-9_]", "", name or "")
    if not s:
        s = "Unknown"
    if s[0].isdigit():
        s = "_" + s
    return s


def normalize_for_key(s: str | None) -> str:
    """Normalize titles into a stable lookup key (lowercase, punctuation/whitespace collapsed)."""
    if not s:
        return ""
    s = s.strip().lower()
    # normalize dash variants
    s = s.replace("–", " ").replace("—", " ").replace("-", " ")
    # keep only letters/digits as tokens
    s = re.sub(r"[^a-z0-9]+", " ", s)
    s = re.sub(r"\s+", " ", s).strip()
    return s


def load_methods(meta: dict) -> list[dict]:
    """Supports either meta['methods'] or older category shapes."""
    if isinstance(meta.get("methods"), list):
        return meta["methods"]
    # fallback: categories/items style
    out: list[dict] = []
    if isinstance(meta.get("categories"), list):
        for c in meta["categories"]:
            out.extend(c.get("items", []) or [])
    return out


def load_pages(meta: dict) -> list[dict]:
    """Load non-method pages that still need help hooks (e.g. export-chart.md)."""
    pages = meta.get("pages")
    return pages if isinstance(pages, list) else []


def generate(methods_yml: Path, project_dir: Path, check: bool = False) -> None:
    from validate_methods_catalog import validate_catalog, _load_yaml
    problems, _ = validate_catalog(Path(__file__).resolve().parents[2], methods_yml)
    if problems:
        raise ValueError('Invalid methods.yml:\n' + '\n'.join(problems))
    meta = _load_yaml(methods_yml)
    methods = load_methods(meta)
    pages = load_pages(meta)

    # 1) Build HelpTopic enum + URL mapping
    topics: list[tuple[str, str]] = [("Home", "/")]

    # Method pages live under /methods/<slug>/
    for m in methods:
        key = m.get("key")
        slug = m.get("slug")
        if not key or not slug:
            continue
        topics.append((vb_ident(key), f"/methods/{slug}/"))

    # Non-method pages live at /<slug>/ (MkDocs page "export-chart.md" -> "/export-chart/")
    for p in pages:
        key = p.get("key")
        slug = p.get("slug")
        if not key or not slug:
            continue
        rel = slug if slug.startswith("/") else f"/{slug}/"
        if not rel.endswith("/"):
            rel += "/"
        topics.append((vb_ident(key), rel))

    # de-dupe by enum name (stable order)
    seen: set[str] = set()
    uniq_topics: list[tuple[str, str]] = []
    for k, rel in topics:
        if k in seen:
            continue
        seen.add(k)
        uniq_topics.append((k, rel))

    # 2) Build context mapping using (FormClassName + Form.Text)
    #    Key format: "FormClass|normalized_title"
    mapping: dict[str, str] = {}      # key -> HelpTopicName
    conflicts: set[str] = set()

    for m in methods:
        topic = vb_ident(m.get("key", ""))
        title = m.get("title") or ""
        for fr in (m.get("forms") or []):
            form = fr.get("form") or ""
            # use analysisTitle (preferred) else method title
            disp = fr.get("analysisTitle") or title
            map_key = f"{form}|{normalize_for_key(disp)}"
            if map_key in mapping and mapping[map_key] != topic:
                conflicts.add(map_key)
            else:
                mapping[map_key] = topic

    for p in pages:
        topic = vb_ident(p.get("key", ""))
        title = p.get("title") or ""
        for fr in (p.get("forms") or []):
            form = fr.get("form") or ""
            disp = fr.get("analysisTitle") or title
            map_key = f"{form}|{normalize_for_key(disp)}"
            if map_key in mapping and mapping[map_key] != topic:
                conflicts.add(map_key)
            else:
                mapping[map_key] = topic


    # remove ambiguous keys -> will fall back to Home
    for k in conflicts:
        mapping.pop(k, None)

    source_hash = hashlib.sha256(methods_yml.read_bytes()).hexdigest()

    # ---- HelpLinks.vb ----
    help_links_vb: list[str] = []
    help_links_vb += [
        "' <auto-generated>",
        f"' methods.yml SHA256: {source_hash}",
        "' Source: docs/_meta/methods.yml",
        "' </auto-generated>",
        "",
        "Option Strict On",
        "Option Explicit On",
        "",
        "Imports System.Diagnostics",
        "Imports BESHStatNG.ExcelCommon",
        "",
        "Public Enum HelpTopic",
    ]
    for k, _ in uniq_topics:
        help_links_vb.append(f"    {k}")
    help_links_vb += [
        "End Enum",
        "",
        "Public Module HelpLinks",
        '    Public Const BaseUrlRoot As String = GeneratedMethodMetadata.SiteBaseUrl',
        '    Public Const FallbackBaseUrl As String = GeneratedMethodMetadata.HomeUrl',
        '',
        '    Public ReadOnly Property BaseUrl As String',
        '        Get',
        '            Return FallbackBaseUrl.TrimEnd("/"c)',
        '        End Get',
        '    End Property',
        '',
        '    Public Function GetUrl(topic As HelpTopic) As String',
        '        If topic = HelpTopic.Home OrElse Not [Enum].IsDefined(GetType(HelpTopic), topic) Then Return FallbackBaseUrl',
        '        Dim key = topic.ToString()',
        '        Dim destination = HelpDestination.ForMethodKey(key)',
        '        If destination.IsFallback Then destination = HelpDestination.ForPageKey(key)',
        '        Return destination.Url',
        '    End Function',
        '',
        '    Public Sub OpenTopic(topic As HelpTopic)',
        '        OpenUrl(GetLaunchUrl(topic))',
        '    End Sub',
        '',
        '    Public Function GetLaunchUrl(topic As HelpTopic) As String',
        '        Return GetLaunchUrl(topic, AppInfrastructure.AppGlobals.InstalledProductVersion, HelpLaunchTarget.ResolverPublished)',
        '    End Function',
        '',
        '    Public Function GetLaunchUrl(topic As HelpTopic, version As ProductVersionDescriptor, published As Boolean) As String',
        '        If Not [Enum].IsDefined(GetType(HelpTopic), topic) Then Return FallbackBaseUrl',
        '        If topic = HelpTopic.Home Then Return HelpLaunchTarget.CreateHome(version, published).Url',
        '        Dim destination = HelpDestination.ForMethodKey(topic.ToString())',
        '        If destination.IsFallback Then destination = HelpDestination.ForPageKey(topic.ToString())',
        '        Return destination.GetLaunchTarget(version, published).Url',
        '    End Function',
        '',
        '    Public Function GetSafeOpenUrl(url As String) As String',
        '        Return HelpLaunchTarget.SafeOpenUrl(url)',
        '    End Function',
        '',
        '    Public Sub OpenUrl(url As String)',
        '        url = GetSafeOpenUrl(url)',
        '        Try',
        '            Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})',
        '        Catch',
        "            ' Missing browser/shell must never fail an analysis dialog.",
        '        End Try',
        '    End Sub',
        'End Module',
        '',
    ]

    # ---- HelpContext.vb ----
    help_context_vb: list[str] = []
    help_context_vb += [
        "' <auto-generated>",
        f"' methods.yml SHA256: {source_hash}",
        "' Source: docs/_meta/methods.yml",
        "' </auto-generated>",
        "",
        "Option Strict On",
        "Option Explicit On",
        "",
        "Imports System",
        "Imports System.Collections.Generic",
        "Imports System.Windows.Forms",
        "",
        "Public Module HelpContext",
        "    ' Resolution order:",
        "    '   1) frm.Tag as HelpTopic (or String enum name)",
        "    '   2) (Form class + frm.Text) using the generated map",
        "    '   3) fallback Home (BaseUrl)",
        "",
        "    Private ReadOnly FormTitleMap As New Dictionary(Of String, HelpTopic)(StringComparer.OrdinalIgnoreCase) From {",
    ]

    # emit map
    map_items = []
    for key, topic in sorted(mapping.items()):
        if not key.endswith("|"):  # allow empty title keys too, if any
            map_items.append(f"        {{\"{key}\", HelpTopic.{topic}}}")
        else:
            map_items.append(f"        {{\"{key}\", HelpTopic.{topic}}}")
    help_context_vb.append(",\n".join(map_items) if map_items else "        {\"\", HelpTopic.Home}")
    help_context_vb += [
        "    }",
        "",
        "    Public Function ResolveTopic(frm As Form) As HelpTopic",
        "        If frm Is Nothing Then Return HelpTopic.Home",
        "",
        "        ' 1) Explicit override (best if you ever need it)",
        "        If frm.Tag IsNot Nothing Then",
        "            If TypeOf frm.Tag Is HelpTopic Then Return DirectCast(frm.Tag, HelpTopic)",
        "            If TypeOf frm.Tag Is String Then",
        "                Dim s = DirectCast(frm.Tag, String)",
        "                Dim t As HelpTopic",
        "                If [Enum].TryParse(Of HelpTopic)(s, True, t) Then Return t",
        "            End If",
        "        End If",
        "",
        "        ' 2) Form class + title",
        "        Dim formName = frm.GetType().Name",
        "        Dim titleKey = Normalize(frm.Text)",
        "        Dim k = formName & \"|\" & titleKey",
        "        Dim topic As HelpTopic",
        "        If FormTitleMap.TryGetValue(k, topic) Then Return topic",
        "",
        "        ' Allow class-only match (empty title)",
        "        Dim k2 = formName & \"|\"",
        "        If FormTitleMap.TryGetValue(k2, topic) Then Return topic",
        "",
        "        Return HelpTopic.Home",
        "    End Function",
        "",
        "    Public Sub OpenHelp(frm As Form)",
        "        Dim topic = ResolveTopic(frm)",
        "        HelpLinks.OpenTopic(topic)",
        "    End Sub",
        "",
        "    Private Function Normalize(s As String) As String",
        "        If String.IsNullOrWhiteSpace(s) Then Return \"\"",
        "        s = s.Trim().ToLowerInvariant()",
        "        s = s.Replace(\"–\", \" \").Replace(\"—\", \" \").Replace(\"-\", \" \")",
        "        Dim sb As New System.Text.StringBuilder(s.Length)",
        "        For Each ch As Char In s",
        "            If Char.IsLetterOrDigit(ch) Then",
        "                sb.Append(ch)",
        "            Else",
        "                sb.Append(\" \"c)",
        "            End If",
        "        Next",
        "        Dim collapsed = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), \"\\s+\", \" \").Trim()",
        "        Return collapsed",
        "    End Function",
        "End Module",
        "",
    ]

    out_dir = project_dir / "src" / "Help"
    outputs = {out_dir / "HelpLinks.vb": "\n".join(help_links_vb),
               out_dir / "HelpContext.vb": "\n".join(help_context_vb)}
    if check:
        stale = [str(path) for path, content in outputs.items()
                 if not path.is_file() or path.read_text(encoding="utf-8-sig") != content]
        if stale:
            raise ValueError("Generated help is absent/stale: " + ", ".join(stale))
        print("PASS: Windows help is current (read-only)")
        return
    out_dir.mkdir(parents=True, exist_ok=True)
    for path, content in outputs.items():
        path.write_text(content, encoding="utf-8", newline="\n")
        print(f"Wrote {path}")
    if conflicts:
        print(f"WARNING: {len(conflicts)} ambiguous Form+Title keys were skipped (will fall back to Home).")


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--methods-yml", required=True, help="Path to docs/_meta/methods.yml")
    ap.add_argument("--project-dir", default=".", help="VB project root directory")
    ap.add_argument("--check", action="store_true", help="Read-only: fail if either generated file is stale")
    args = ap.parse_args()

    methods_yml = Path(args.methods_yml).resolve()
    project_dir = Path(args.project_dir).resolve()

    if not methods_yml.exists():
        raise SystemExit(f"methods.yml not found: {methods_yml}")

    try:
        generate(methods_yml, project_dir, args.check)
    except ValueError as exc:
        raise SystemExit(str(exc))


if __name__ == "__main__":
    main()
