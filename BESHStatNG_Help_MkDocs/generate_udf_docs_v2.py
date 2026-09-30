#!/usr/bin/env python3
"""Generate MkDocs markdown pages for Excel-exposed UDFs.

Design goals (for BESHStatNG):
- Only functions defined in VB files under a *project-local* `udfs/` folder are documented.
- Documentation is produced from the VB XML doc comments (''' <summary>...</summary> etc.) that live
  immediately above each exported UDF.
- Export selection is based on the presence of Excel-DNA attributes: <ExcelFunction(...)>.
- Output is grouped by Excel category into `docs/udf/<slug>.md`.
- Additionally generates an overview page `docs/udf/index.md` listing all UDF groups and links to
  related "main" (Windows Forms) documentation pages under `docs/methods/`, when available.

Usage (run from the MkDocs repository root):
  python tools/generate_udf_docs.py \
    --udfs-dir "../BESHStatNG/udfs" \
    --output-dir "docs/udf"

This script overwrites generated files.
"""

from __future__ import annotations

import argparse
import re
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Tuple


@dataclass
class UdfDoc:
    excel_name: str
    category: str
    description: str
    help_topic: Optional[str]
    summary: str
    params: List[Tuple[str, str]]
    returns: str
    remarks: str
    example: str


# ---------------------------------------------------------------------------
# Linking UDFs to "main" (Windows Forms) help pages
# ---------------------------------------------------------------------------
#
# The UI documentation already lives under docs/methods/*.md in your MkDocs tree.
# We auto-link UDF groups (and the overview page) to these method pages using:
#   1) A small built-in heuristic mapping (can be extended here).
#   2) Optional per-repository YAML overrides via --method-links-yaml.
#
# YAML format:
#   patterns:
#     - match: "MW_"            # substring or regex
#       doc: "methods/mann-whitney-test.md"
#     - match: "WILCOX"
#       doc: "methods/wilcoxon-signed-rank-test.md"
#   groups:
#     Distributions:
#       doc: "methods/some-distributions-page.md"
#
# Notes:
# - Paths are relative to the MkDocs docs/ folder.
# - If a doc path doesn't exist, it will be omitted from output ("if available").


DEFAULT_METHOD_LINK_PATTERNS: List[Tuple[str, str]] = [
    # Nonparametric tests you already document in the UI
    (r"\bMW_", "methods/mann-whitney-test.md"),
    (r"\bMANN\s*WHITNEY", "methods/mann-whitney-test.md"),
    (r"\bWILCOX", "methods/wilcoxon-signed-rank-test.md"),
    (r"\bWILCOXON", "methods/wilcoxon-signed-rank-test.md"),
    (r"\bKRUSKAL", "methods/kruskal-wallis-test.md"),
    (r"\bFRIEDMAN", "methods/friedman-test.md"),
    (r"\bCOCHRAN", "methods/cochrans-q-test.md"),
    (r"\bSKILLINGS", "methods/skillings-mack-test.md"),
    (r"\bSPEARMAN", "methods/spearman-rank-correlation.md"),
    (r"\bKENDALL", "methods/kendalls-rank-correlation.md"),
    (r"\bTHEIL", "methods/theil-sen-simple-regression.md"),
]

DEFAULT_GROUP_SUMMARIES: Dict[str, str] = {
    "Distributions": "Probability distribution helper functions (PDF/CDF/quantiles and related utilities).",
    "Nonparametric": "Rank-based and other nonparametric hypothesis tests and related statistics.",
}


def _load_method_links_yaml(path: Optional[Path]) -> Tuple[List[Tuple[str, str]], Dict[str, str]]:
    """Return (pattern_links, group_links) from an optional YAML config."""
    if path is None:
        return [], {}
    if not path.exists():
        raise SystemExit(f"method-links-yaml does not exist: {path}")
    import yaml  # optional dependency; only needed if this flag is used

    data = yaml.safe_load(path.read_text(encoding="utf-8")) or {}
    patterns: List[Tuple[str, str]] = []
    for item in (data.get("patterns") or []):
        m = str(item.get("match", "")).strip()
        d = str(item.get("doc", "")).strip()
        if m and d:
            patterns.append((m, d))

    groups: Dict[str, str] = {}
    for k, v in (data.get("groups") or {}).items():
        if isinstance(v, dict):
            doc = str(v.get("doc", "")).strip()
        else:
            doc = str(v).strip()
        if k and doc:
            groups[str(k).strip()] = doc

    return patterns, groups


# ---------------------------------------------------------------------------
# Parsing Excel-DNA attributes + VB XML doc comments
# ---------------------------------------------------------------------------

_RE_EXCELFUNC_START = re.compile(r"^\s*<ExcelFunction\(")
_RE_EXCELFUNC_NAME = re.compile(r"Name\s*:=\s*\"([^\"]+)\"")
_RE_EXCELFUNC_CATEGORY = re.compile(r"Category\s*:=\s*\"([^\"]+)\"")
_RE_EXCELFUNC_DESCRIPTION = re.compile(r"Description\s*:=\s*\"([^\"]+)\"")
_RE_EXCELFUNC_HELPTOPIC = re.compile(r"HelpTopic\s*:=\s*\"([^\"]+)\"")
_RE_ATTR_END = re.compile(r"\)\s*>\s*$")


def _strip_doc_prefix(line: str) -> str:
    # VB XML doc comments start with three apostrophes.
    # Preserve the inner XML as-is (minus leading whitespace).
    s = line.lstrip()
    if s.startswith("'''"):
        return s[3:].lstrip()
    return ""


def _xml_to_markdown_text(elem: ET.Element) -> str:
    """Convert an XML doc node to plain-ish Markdown."""

    def walk(node: ET.Element) -> str:
        parts: List[str] = []
        if node.text:
            parts.append(node.text)

        for child in list(node):
            tag = child.tag.lower()

            if tag == "c":
                parts.append(f"`{(child.text or '').strip()}`")
            elif tag == "paramref":
                name = child.attrib.get("name", "")
                parts.append(f"`{name}`")
            elif tag == "see":
                # Prefer inner text; fall back to cref.
                t = (child.text or "").strip()
                if t:
                    parts.append(t)
                else:
                    parts.append(child.attrib.get("cref", ""))
            elif tag == "br":
                parts.append("\n")
            elif tag == "para":
                parts.append("\n\n" + walk(child).strip() + "\n\n")
            elif tag == "list":
                parts.append(_render_list(child))
            elif tag == "code":
                code = (child.text or "").rstrip("\n")
                parts.append(f"\n\n```\n{code}\n```\n\n")
            else:
                parts.append(walk(child))

            if child.tail:
                parts.append(child.tail)

        return "".join(parts)

    def _render_list(list_node: ET.Element) -> str:
        items = []
        for item in list_node.findall("item"):
            desc = item.find("description")
            if desc is not None:
                txt = " ".join(_xml_to_markdown_text(desc).split())
            else:
                txt = " ".join((item.text or "").split())
            if txt:
                items.append(f"- {txt}")
        return "\n" + "\n".join(items) + "\n"

    out = walk(elem)
    # Normalize whitespace a bit.
    out = out.replace("\r\n", "\n")
    out = re.sub(r"\n{3,}", "\n\n", out)
    return out.strip()


def _get_text(parent: ET.Element, tag: str) -> str:
    child = parent.find(tag)
    if child is None:
        return ""
    return _xml_to_markdown_text(child).strip()


def _get_params(parent: ET.Element) -> List[Tuple[str, str]]:
    out: List[Tuple[str, str]] = []
    for p in parent.findall("param"):
        name = p.attrib.get("name", "").strip()
        txt = _xml_to_markdown_text(p).strip()
        if name:
            out.append((name, txt))
    return out


def parse_udfs_from_vb(vb_path: Path) -> List[UdfDoc]:
    """Parse a VB file and extract UDF docs for members decorated with ExcelFunction."""
    lines = vb_path.read_text(encoding="utf-8-sig").splitlines()
    docs: List[UdfDoc] = []

    i = 0
    while i < len(lines):
        line = lines[i]

        if _RE_EXCELFUNC_START.search(line):
            # Collect attribute block (may span multiple lines) and preceding XML doc comments.
            attr_lines = [line]
            j = i + 1
            while j < len(lines) and not _RE_ATTR_END.search(lines[j]):
                attr_lines.append(lines[j])
                j += 1
            if j < len(lines):
                attr_lines.append(lines[j])

            attr_text = "\n".join(attr_lines)
            excel_name = _RE_EXCELFUNC_NAME.search(attr_text).group(1) if _RE_EXCELFUNC_NAME.search(attr_text) else ""
            category = _RE_EXCELFUNC_CATEGORY.search(attr_text).group(1) if _RE_EXCELFUNC_CATEGORY.search(attr_text) else ""
            description = _RE_EXCELFUNC_DESCRIPTION.search(attr_text).group(1) if _RE_EXCELFUNC_DESCRIPTION.search(attr_text) else ""
            ht = _RE_EXCELFUNC_HELPTOPIC.search(attr_text).group(1) if _RE_EXCELFUNC_HELPTOPIC.search(attr_text) else None

            # Backtrack to capture XML doc comment block immediately above the attribute.
            k = i - 1
            doc_lines: List[str] = []
            while k >= 0:
                s = lines[k].lstrip()
                if s.startswith("'''"):
                    doc_lines.append(_strip_doc_prefix(lines[k]))
                    k -= 1
                    continue
                # Stop at first non-doc line.
                break
            doc_lines.reverse()

            if not excel_name:
                # If Name:= is missing, skip (not expected in your convention).
                i = j + 1
                continue

            xml_blob = "\n".join(doc_lines).strip()
            # Guard: ensure it parses as XML even if empty.
            try:
                root = ET.fromstring(f"<root>\n{xml_blob}\n</root>")
            except ET.ParseError:
                # If malformed, still emit a stub so you notice it.
                root = ET.fromstring("<root><summary>Documentation parse error.</summary></root>")

            docs.append(
                UdfDoc(
                    excel_name=excel_name,
                    category=category,
                    description=description,
                    help_topic=ht,
                    summary=_get_text(root, "summary"),
                    params=_get_params(root),
                    returns=_get_text(root, "returns"),
                    remarks=_get_text(root, "remarks"),
                    example=_get_text(root, "example"),
                )
            )

            i = j + 1
            continue

        i += 1

    return docs


# ---------------------------------------------------------------------------
# Rendering
# ---------------------------------------------------------------------------

def category_slug(category: str) -> str:
    # "BESHStatNG - Distributions" -> "distributions"
    name = category
    if "-" in category:
        name = category.split("-", 1)[1].strip()
    slug = re.sub(r"[^a-z0-9]+", "-", name.lower()).strip("-")
    return slug or "udfs"


def category_title(category: str) -> str:
    if "-" in category:
        return category.split("-", 1)[1].strip()
    return category.strip() or "UDF"


def _normalize_docs_root(docs_root: Path, out_dir: Path) -> Path:
    """Normalize docs_root so method page checks/links work even if caller passes the MkDocs repo root.

    Expected layout:
      <docs_root>/methods/*.md
      <docs_root>/udf/*.md  (this script's output)

    If the caller passes a repository root containing a `docs/` folder, we transparently
    switch to that `docs/` folder.
    """
    dr = docs_root
    # If docs_root is the MkDocs repo root, prefer its docs/ subfolder.
    if not (dr / "methods").exists() and (dr / "docs" / "methods").exists():
        dr = dr / "docs"

    # If docs_root accidentally points at the output folder itself (docs/udf),
    # move up one level.
    if dr.resolve() == out_dir.resolve():
        parent = out_dir.parent
        if (parent / "methods").exists():
            dr = parent

    return dr.resolve()


def _links_for_doc(
    doc: UdfDoc,
    docs_root: Path,
    patterns: List[Tuple[str, str]],
) -> List[str]:
    """Return a list of existing docs paths (relative to docs_root) related to this UDF."""
    hits: List[str] = []
    hay = f"{doc.excel_name}".upper()

    for pat, rel in patterns:
        try:
            if re.search(pat, hay, flags=re.IGNORECASE):
                if (docs_root / rel).exists():
                    hits.append(rel)
        except re.error:
            # If user provided a non-regex pattern, fall back to substring.
            if pat.upper() in hay and (docs_root / rel).exists():
                hits.append(rel)

    # Also allow HelpTopic itself if it points into the docs tree (rare, but supported)
    if doc.help_topic:
        ht = doc.help_topic.strip()
        if ht.startswith("methods/") and (docs_root / ht).exists():
            hits.append(ht)

    # Unique preserve order
    seen = set()
    out = []
    for h in hits:
        if h not in seen:
            out.append(h)
            seen.add(h)
    return out


def _md_link(from_page: Path, to_rel_docs_path: str, docs_root: Path) -> str:
    """Create a relative markdown link from from_page (an .md file) to a docs-root-relative target."""
    target = docs_root / to_rel_docs_path
    import os
    rel = os.path.relpath(str(target), start=str(from_page.parent.resolve()))
    return rel.replace('\\', '/')


def render_udf(doc: UdfDoc) -> str:
    lines: List[str] = []
    lines.append(f"## {doc.excel_name}")
    lines.append("")

    if doc.summary:
        lines.append(doc.summary)
        lines.append("")

    if doc.description:
        lines.append(f"**Function wizard:** {doc.description}")
        lines.append("")

    # Syntax: derive from Excel name (best effort)
    if doc.params:
        arglist = ", ".join([name for name, _ in doc.params])
        lines.append("### Syntax")
        lines.append("")
        lines.append(f"`={doc.excel_name}({arglist})`")
        lines.append("")
        lines.append("### Parameters")
        lines.append("")
        for name, desc in doc.params:
            lines.append(f"- **{name}** — {desc.strip()}")
        lines.append("")

    if doc.returns:
        lines.append("### Returns")
        lines.append("")
        lines.append(doc.returns)
        lines.append("")

    if doc.remarks:
        lines.append("### Notes")
        lines.append("")
        lines.append(doc.remarks)
        lines.append("")

    if doc.example:
        lines.append("### Example")
        lines.append("")
        if "```" in doc.example:
            lines.append(doc.example)
        else:
            lines.append("```")
            lines.append(doc.example)
            lines.append("```")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def _render_related_section(
    from_page: Path,
    docs_root: Path,
    related_rel_paths: List[str],
    heading: str = "Related dialog documentation",
) -> str:
    if not related_rel_paths:
        return ""
    lines = [f"## {heading}", ""]
    for rel in related_rel_paths:
        link = _md_link(from_page, rel, docs_root)
        # Display name: derive from filename
        title = Path(rel).stem.replace("-", " ").title()
        lines.append(f"- [{title}]({link})")
    lines.append("")
    return "\n".join(lines)


def _render_overview(
    out_dir: Path,
    docs_root: Path,
    by_cat: Dict[str, List[UdfDoc]],
    cat_to_page: Dict[str, Path],
    method_patterns: List[Tuple[str, str]],
    group_links_override: Dict[str, str],
) -> str:
    """Build docs/udf/index.md content."""
    index_path = out_dir / "index.md"

    header = [
        "# User Defined Functions (UDFs)",
        "",
        "_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._",
        "",
        "The pages below document the worksheet functions exposed by the add-in. Each group corresponds to the",
        "Excel Function Wizard category (e.g. `BESHStatNG - Nonparametric`).",
        "",
        "## Groups",
        "",
        "| Group | What it covers | Functions | Related dialog documentation |",
        "|---|---|---:|---|",
    ]

    rows: List[str] = []
    for cat, docs in sorted(by_cat.items(), key=lambda x: category_title(x[0]).lower()):
        title = category_title(cat)
        slug = category_slug(cat)
        page = cat_to_page[cat]
        group_link = f"[{title}]({(page.name)})"

        summary = DEFAULT_GROUP_SUMMARIES.get(title, "Worksheet functions in this category.")
        count = len(docs)

        # Related: union of related pages for functions in this category, plus optional override
        related = []
        if title in group_links_override:
            rel = group_links_override[title]
            if (docs_root / rel).exists():
                related.append(rel)

        for d in docs:
            related.extend(_links_for_doc(d, docs_root, method_patterns))

        # unique
        seen=set(); rel_u=[]
        for r in related:
            if r not in seen:
                rel_u.append(r); seen.add(r)

        if rel_u:
            rel_links = ", ".join([f"[{Path(r).stem.replace('-', ' ').title()}]({_md_link(index_path, r, docs_root)})" for r in rel_u])
        else:
            rel_links = "—"

        rows.append(f"| {group_link} | {summary} | {count} | {rel_links} |")

    # Add a mini TOC of functions per group
    toc_lines = ["", "## Function index", ""]
    for cat, docs in sorted(by_cat.items(), key=lambda x: category_title(x[0]).lower()):
        title = category_title(cat)
        page = cat_to_page[cat]
        toc_lines.append(f"### {title}")
        toc_lines.append("")
        for d in sorted(docs, key=lambda x: x.excel_name):
            toc_lines.append(f"- `{d.excel_name}` — {d.description or d.summary}")
        toc_lines.append("")
        toc_lines.append(f"See: [{title} UDFs]({page.name})")
        toc_lines.append("")
    return "\n".join(header + rows + toc_lines).rstrip() + "\n"


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--udfs-dir", required=True, help="Path to the VB project udfs/ folder")
    ap.add_argument("--output-dir", required=True, help="Output docs folder (e.g. docs/udf)")
    ap.add_argument(
        "--docs-root",
        default=None,
        help="Path to MkDocs docs/ folder (default: parent of output-dir)",
    )
    ap.add_argument(
        "--method-links-yaml",
        default=None,
        help="Optional YAML file to extend/override UDF→method doc links (see script header)",
    )
    args = ap.parse_args()

    udfs_dir = Path(args.udfs_dir).resolve()
    out_dir = Path(args.output_dir).resolve()
    docs_root = Path(args.docs_root).resolve() if args.docs_root else out_dir.parent.resolve()
    docs_root = _normalize_docs_root(docs_root, out_dir)

    if not udfs_dir.exists():
        raise SystemExit(f"udfs-dir does not exist: {udfs_dir}")

    vb_files = sorted(udfs_dir.rglob("*.vb"))
    if not vb_files:
        raise SystemExit(f"No .vb files found under: {udfs_dir}")

    extra_patterns, group_links_override = _load_method_links_yaml(Path(args.method_links_yaml).resolve() if args.method_links_yaml else None)
    method_patterns = DEFAULT_METHOD_LINK_PATTERNS + extra_patterns

    all_docs: List[UdfDoc] = []
    for f in vb_files:
        all_docs.extend(parse_udfs_from_vb(f))

    # Group by category
    by_cat: Dict[str, List[UdfDoc]] = {}
    for d in all_docs:
        by_cat.setdefault(d.category or "BESHStatNG - UDF", []).append(d)

    out_dir.mkdir(parents=True, exist_ok=True)

    # Keep track of output page per category for linking from overview.
    cat_to_page: Dict[str, Path] = {}

    for cat, docs in by_cat.items():
        docs.sort(key=lambda d: d.excel_name)
        slug = category_slug(cat)
        title = category_title(cat)
        out_path = out_dir / f"{slug}.md"
        cat_to_page[cat] = out_path

        # Related method pages for this category (union)
        related_rel: List[str] = []
        if title in group_links_override:
            rel = group_links_override[title]
            if (docs_root / rel).exists():
                related_rel.append(rel)
        for d in docs:
            related_rel.extend(_links_for_doc(d, docs_root, method_patterns))
        # unique
        seen=set(); related_rel_unique=[]
        for r in related_rel:
            if r not in seen:
                related_rel_unique.append(r); seen.add(r)

        header = [
            f"# {title} UDFs",
            "",
            "_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._",
            "",
        ]
        related_section = _render_related_section(out_path, docs_root, related_rel_unique)

        body = "\n".join(header) + related_section + "\n" + "\n".join(render_udf(d) for d in docs)
        out_path.write_text(body, encoding="utf-8")
        print(f"Wrote {out_path} ({len(docs)} functions)")

    # Write overview page
    overview = _render_overview(out_dir, docs_root, by_cat, cat_to_page, method_patterns, group_links_override)
    (out_dir / "index.md").write_text(overview, encoding="utf-8")
    print(f"Wrote {out_dir / 'index.md'}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
