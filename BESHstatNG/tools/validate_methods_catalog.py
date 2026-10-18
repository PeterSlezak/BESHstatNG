#!/usr/bin/env python3
"""Validate the authored method catalog against the deployed Windows Ribbon and docs.

Batch 00.02A. This is a read-only build/CI preflight; it never generates
Ribbon XML or modifies the .NET runtime analysis catalog.

Usage (from the solution/repository root):
    python BESHstatNG/tools/validate_methods_catalog.py
    python BESHstatNG/tools/validate_methods_catalog.py --json-report report.json

Requires PyYAML: pip install pyyaml
"""
from __future__ import annotations

import argparse
from collections import Counter
import json
from pathlib import Path
import re
import sys
from typing import Any
import xml.etree.ElementTree as ET

try:
    import yaml
except ImportError:
    raise SystemExit("PyYAML is required: pip install pyyaml")

# These describe current independent Windows ribbon commands, not statistical
# methods. The actual callbacks come from XML and are checked against YAML.
UTILITY_IDS = frozenset({
    "btn00_1About", "btn00_2Help", "btn00_3Settings",
    "btn00_4logShow", "btn00_5ExportChart",
})
SLUG_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
VB_IDENTIFIER_RE = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
ANALYSIS_ID_RE = re.compile(r"^[a-z][a-z0-9]*(?:[.-][a-z][a-z0-9]*)+$")


def _vb_identifier(value: str) -> str:
    name = re.sub(r"[^A-Za-z0-9_]", "", value)
    return "_" + name if name and name[0].isdigit() else (name or "Unknown")


def _normalized_title(value: str | None) -> str:
    # Identical to generate_help_links.py's authored Python normalization.
    value = (value or "").strip().lower().replace("–", " ").replace("—", " ").replace("-", " ")
    return re.sub(r"\s+", " ", re.sub(r"[^a-z0-9]+", " ", value)).strip()


class _UniqueKeyLoader(yaml.SafeLoader):
    """Do not silently accept duplicate mapping keys in authored YAML."""


def _construct_mapping(loader: yaml.SafeLoader, node: yaml.nodes.MappingNode, deep: bool = False) -> dict:
    result: dict = {}
    for key_node, value_node in node.value:
        key = loader.construct_object(key_node, deep=deep)
        if key in result:
            raise ValueError(f"Duplicate YAML mapping key {key!r} (line {key_node.start_mark.line + 1})")
        result[key] = loader.construct_object(value_node, deep=deep)
    return result


_UniqueKeyLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG, _construct_mapping)


def _load_yaml(path: Path) -> Any:
    return yaml.load(path.read_text(encoding="utf-8"), Loader=_UniqueKeyLoader)


def _str(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def _list(value: Any) -> bool:
    return isinstance(value, list)


def _nav_documents(value: Any):
    if isinstance(value, list):
        for v in value:
            yield from _nav_documents(v)
    elif isinstance(value, dict):
        for v in value.values():
            yield from _nav_documents(v)
    elif isinstance(value, str):
        yield value


def _read_registered_ids(path: Path) -> dict[str, str]:
    text = path.read_text(encoding="utf-8-sig")
    # AnalysisIds.vb is a checked-in compile-time source, not runtime YAML.
    return dict(re.findall(r'Public Const\s+(\w+)\s+As String\s*=\s*"([^"]+)"', text))


def _read_ribbon(path: Path, issues: list[str]) -> dict[str, str]:
    try:
        xml = ET.parse(path)
    except (ET.ParseError, OSError) as exc:
        issues.append(f"Ribbon XML cannot be parsed: {exc}")
        return {}
    buttons: dict[str, str] = {}
    for element in xml.iter():
        if element.tag.rsplit("}", 1)[-1] != "button":
            continue
        ribbon_id, action = element.get("id"), element.get("onAction")
        if not _str(ribbon_id) or not _str(action):
            issues.append(f"Ribbon button missing id/onAction: {ET.tostring(element, encoding='unicode')[:160]}")
        elif ribbon_id in buttons:
            issues.append(f"Duplicate Ribbon button ID: {ribbon_id}")
        else:
            buttons[ribbon_id] = action
    return buttons


def validate_catalog(root: Path, methods_yml: Path | None = None) -> tuple[list[str], dict[str, int]]:
    """Return actionable errors and counts; do not mutate any checked-in files."""
    root = Path(root).resolve()
    yaml_path = Path(methods_yml).resolve() if methods_yml else root / "BESHStatNG_Help_MkDocs/docs/_meta/methods.yml"
    docs = root / "BESHStatNG_Help_MkDocs/docs"
    issues: list[str] = []
    stats: dict[str, int] = {}
    try:
        meta = _load_yaml(yaml_path)
    except (OSError, yaml.YAMLError, ValueError) as exc:
        return [f"Cannot read catalog {yaml_path}: {exc}"], stats
    if not isinstance(meta, dict):
        return ["Catalog root must be a YAML mapping"], stats

    methods, pages, commands, categories = [meta.get(k) for k in ("methods", "pages", "commands", "categories")]
    for name, obj in (("methods", methods), ("pages", pages), ("commands", commands), ("categories", categories)):
        if not _list(obj):
            issues.append(f"{name} must be a list")
    if issues:
        return issues, stats

    ribbon = _read_ribbon(root / "BESHstatNG/Resources/RibbonXML.xml", issues)
    stats["ribbon_buttons"] = len(ribbon)
    registered_ids = _read_registered_ids(root / "BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb")
    if len(set(registered_ids.values())) != len(registered_ids):
        issues.append("AnalysisIds.vb contains duplicate runtime ID constants")
    registered_values = set(registered_ids.values())
    all_docs: dict[str, dict] = {}
    slugs: dict[str, str] = {}
    enum_names: dict[str, str] = {"Home": "Home"}
    form_title: dict[str, str] = {}
    assigned: dict[str, str] = {}
    analysis_ids: dict[str, str] = {}
    source_vb = "\n".join(p.read_text(encoding="utf-8-sig", errors="replace") for p in (root / "BESHstatNG").rglob("*.vb") if not any(part in ("bin", "obj") for part in p.parts))
    forms_dir = root / "BESHstatNG/src/UI"
    if not forms_dir.exists():
        issues.append(f"Missing WinForms source directory: {forms_dir}")

    def check_entry(entry: Any, section: str, index: int) -> None:
        if not isinstance(entry, dict):
            issues.append(f"{section}[{index}] must be a mapping")
            return
        key = entry.get("key")
        slug = entry.get("slug")
        title = entry.get("title")
        where = f"{section}[{index}] ({key})"
        if not all(_str(v) for v in (key, slug, title)):
            issues.append(f"{where}: key, title, slug are mandatory non-empty strings")
            return
        if key in all_docs:
            issues.append(f"Duplicate method/page key {key!r}")
        all_docs[key] = entry
        if not SLUG_RE.fullmatch(slug):
            issues.append(f"{where}: invalid URL slug {slug!r}")
        if slug in slugs:
            issues.append(f"Duplicate method/page slug {slug!r}: {key} and {slugs[slug]}")
        slugs[slug] = key
        enum_name = _vb_identifier(key)
        if not VB_IDENTIFIER_RE.fullmatch(enum_name) or enum_name in enum_names:
            issues.append(f"Duplicate/invalid HelpTopic enum name {enum_name!r} for {key}")
        enum_names[enum_name] = key
        relative = f"methods/{slug}.md" if section == "methods" else f"{slug}.md"
        if not (docs / relative).is_file():
            issues.append(f"{where}: missing documentation page docs/{relative}")
        forms = entry.get("forms", [])
        if not _list(forms):
            issues.append(f"{where}: forms must be a list")
            forms = []
        for f in forms:
            if not isinstance(f, dict) or not _str(f.get("form")):
                issues.append(f"{where}: forms[].form must be non-empty")
                continue
            form = f["form"]
            if not (forms_dir / f"{form}.vb").is_file():
                issues.append(f"{where}: unknown WinForms class {form}")
            display_title = f.get("analysisTitle") or title
            if not _str(display_title):
                issues.append(f"{where}: form analysisTitle must be null or a string")
                continue
            norm_key = f"{form}|{_normalized_title(display_title)}"
            if norm_key in form_title and form_title[norm_key] != key:
                issues.append(f"Ambiguous HelpContext form/title {norm_key!r}: {form_title[norm_key]} and {key}")
            form_title[norm_key] = key
            if section == "methods":
                handler = f.get("handler")
                if not _str(handler) or handler not in entry.get("onActions", []):
                    issues.append(f"{where}: form handler {handler!r} not in method onActions")

    for i, m in enumerate(methods):
        check_entry(m, "methods", i)
        if not isinstance(m, dict) or not _str(m.get("key")):
            continue
        key = m["key"]
        where = f"method {key}"
        if not _str(m.get("short")):
            issues.append(f"{where}: missing short description")
        method_cats = m.get("categories")
        if not _list(method_cats) or not method_cats or not all(_str(c) for c in method_cats):
            issues.append(f"{where}: categories must be a non-empty string list")
        rid, actions = m.get("ribbonIds"), m.get("onActions")
        if not _list(rid) or not _list(actions) or not rid or len(rid) != len(actions):
            issues.append(f"{where}: ribbonIds/onActions must be equal-length non-empty lists")
            continue
        for button, action in zip(rid, actions):
            if not _str(button) or not _str(action):
                issues.append(f"{where}: invalid Ribbon ID or callback {button!r}/{action!r}")
                continue
            if button in assigned:
                issues.append(f"Ribbon ID {button} assigned twice: {assigned[button]} and {key}")
            assigned[button] = key
            if button not in ribbon:
                issues.append(f"{where}: unknown Ribbon button {button}")
            elif ribbon[button] != action:
                issues.append(f"{where}: {button} callback {action!r} differs from XML {ribbon[button]!r}")
        for form in m.get("forms", []):
            if isinstance(form, dict) and _str(form.get("handler")):
                if not re.search(r"\b(?:Public\s+)?Sub\s+" + re.escape(form["handler"]) + r"\s*\(", source_vb, re.I):
                    issues.append(f"{where}: handler {form['handler']} has no VB Sub declaration")
        aid = m.get("analysisId")
        if aid is not None:
            if not _str(aid) or not ANALYSIS_ID_RE.fullmatch(aid):
                issues.append(f"{where}: invalid analysisId {aid!r}")
            else:
                if aid in analysis_ids:
                    issues.append(f"Duplicate analysisId {aid}: {analysis_ids[aid]} and {key}")
                analysis_ids[aid] = key
                if aid not in registered_values:
                    issues.append(f"{where}: analysisId {aid} is not present in compiled AnalysisIds.vb")

    for i, page in enumerate(pages):
        check_entry(page, "pages", i)
    stats["methods"] = len(methods)
    stats["pages"] = len(pages)
    stats["method_button_mappings"] = len(assigned)
    stats["analysis_ids"] = len(analysis_ids)
    if set(analysis_ids) != registered_values:
        issues.append(f"Registered analysis IDs missing from catalog: {sorted(registered_values - set(analysis_ids))}")

    if not isinstance(meta.get("site"), dict) or not _str(meta["site"].get("baseUrl")):
        issues.append("site.baseUrl is required for generated help URLs")
    if any(not isinstance(c, dict) or not _str(c.get("key")) for c in commands):
        issues.append("commands must contain mappings with key")
    cmd_ids: dict[str, str] = {}
    cmd_keys: set[str] = set()
    for cmd in commands:
        if not isinstance(cmd, dict):
            continue
        key, bid, action = cmd.get("key"), cmd.get("ribbonId"), cmd.get("onAction")
        if not all(_str(v) for v in (key, bid, action)):
            issues.append(f"Command requires key/ribbonId/onAction: {cmd}")
            continue
        if key in cmd_keys:
            issues.append(f"Duplicate command key: {key}")
        cmd_keys.add(key)
        if bid in cmd_ids:
            issues.append(f"Duplicate command Ribbon ID: {bid}")
        cmd_ids[bid] = key
        if bid not in UTILITY_IDS:
            issues.append(f"Unapproved command ID {bid}")
        if ribbon.get(bid) != action:
            issues.append(f"Command {key}: callback mismatch for {bid}: YAML={action}, XML={ribbon.get(bid)}")
        if "pageKey" in cmd and (cmd["pageKey"] not in {p.get("key") for p in pages if isinstance(p, dict)}):
            issues.append(f"Command {key}: unknown pageKey {cmd['pageKey']}")
    if set(cmd_ids) != UTILITY_IDS:
        issues.append(f"Utility commands mismatch: missing={sorted(UTILITY_IDS - set(cmd_ids))}, excess={sorted(set(cmd_ids) - UTILITY_IDS)}")
    stats["utility_commands"] = len(cmd_ids)
    if set(assigned) & set(cmd_ids):
        issues.append(f"Buttons mapped as both method and command: {sorted(set(assigned) & set(cmd_ids))}")
    unmapped = set(ribbon) - set(assigned) - set(cmd_ids)
    if unmapped:
        issues.append(f"Unmapped XML Ribbon button(s): {sorted(unmapped)}")

    cat_names: set[str] = set()
    nav_ref_counts: Counter[str] = Counter()
    nav_count = 0
    for category in categories:
        if not isinstance(category, dict) or not _str(category.get("name")) or not _list(category.get("items")):
            issues.append(f"Invalid category structure {category!r}")
            continue
        name = category["name"]
        if name in cat_names:
            issues.append(f"Duplicate category {name}")
        cat_names.add(name)
        entries_in_cat: set[str] = set()
        for item in category["items"]:
            nav_count += 1
            if not isinstance(item, dict):
                issues.append(f"Category {name} has non-mapping item")
                continue
            ref = item.get("ref")
            if not _str(ref) or ref not in all_docs or ref not in {m.get('key') for m in methods if isinstance(m, dict)}:
                issues.append(f"Category {name}: unknown method ref {ref!r}")
                continue
            m = all_docs[ref]
            nav_ref_counts[ref] += 1
            if ref in entries_in_cat:
                issues.append(f"Category {name}: duplicate method {ref}")
            entries_in_cat.add(ref)
            if name not in m.get("categories", []):
                issues.append(f"Category {name}: {ref} does not list category in method record")
            if item.get("slug") != m["slug"]:
                issues.append(f"Category {name}: stale slug {item.get('slug')!r} for {ref} (expected {m['slug']!r})")
            if not _str(item.get("title")):
                issues.append(f"Category {name}: {ref} lacks navigation title")
            if item.get("ribbonId") not in m.get("ribbonIds", []):
                issues.append(f"Category {name}: {ref} has unregistered ribbonId {item.get('ribbonId')}")
    stats["categories"] = len(categories)
    stats["curated_navigation_entries"] = nav_count
    for m in methods:
        if isinstance(m, dict) and _str(m.get("key")):
            if nav_ref_counts[m["key"]] == 0:
                issues.append(f"Method absent from curated categories: {m['key']}")
            for category in m.get("categories", []):
                if category not in cat_names:
                    issues.append(f"Method {m['key']}: undefined category {category}")

    # Callback names must actually exist as VB Subs, not just match in YAML/XML.
    ribbon_source = (root / "BESHstatNG/BESHStatAddIn.vb").read_text(encoding="utf-8-sig")
    for action in set(ribbon.values()):
        if not re.search(r"\bPublic\s+Sub\s+" + re.escape(action) + r"\s*\(", ribbon_source, re.I):
            issues.append(f"Ribbon callback {action} has no Public Sub in BESHStatAddIn.vb")

    # Keep MkDocs manual navigation (including nested LMM/MMRM and independent
    # resampling articles). It is checked but never generated or replaced here.
    nav_path = root / "BESHStatNG_Help_MkDocs/mkdocs.base.yml"
    try:
        mkdocs = _load_yaml(nav_path)
        nav_paths = set(_nav_documents(mkdocs.get("nav", [])))
        for file in nav_paths:
            if not (docs / file).is_file():
                issues.append(f"MkDocs nav points to missing docs/{file}")
        for m in methods:
            if isinstance(m, dict) and _str(m.get("slug")):
                target = f"methods/{m['slug']}.md"
                if target not in nav_paths:
                    issues.append(f"MkDocs nav is missing method page {target}")
        for p in pages:
            if isinstance(p, dict) and _str(p.get("slug")):
                target = f"{p['slug']}.md"
                if target not in nav_paths:
                    issues.append(f"MkDocs nav is missing utility page {target}")
    except (OSError, yaml.YAMLError, ValueError, AttributeError) as exc:
        issues.append(f"Unable to validate MkDocs nav: {exc}")

    return issues, stats


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2], help="Solution ZIP extraction root")
    ap.add_argument("--methods-yml", type=Path, help="Override the canonical manifest path (e.g. negative tests)")
    ap.add_argument("--json-report", type=Path, help="Optional read-only validation report destination")
    args = ap.parse_args(argv)
    issues, counts = validate_catalog(args.root, args.methods_yml)
    print("Batch 00.02A methods.yml validation")
    for key, value in counts.items():
        print(f"  {key}: {value}")
    for error in issues:
        print(f"ERROR: {error}")
    if args.json_report:
        args.json_report.write_text(json.dumps({"valid": not issues, "counts": counts, "issues": issues}, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"{'PASS' if not issues else 'FAIL'} ({len(issues)} errors)")
    return 0 if not issues else 1


if __name__ == "__main__":
    sys.exit(main())
