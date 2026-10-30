"""Read-only authored-catalog crosswalk + independent ledger integrity checks.

Requires installed Python 3.10+ / PyYAML; never installs packages or regenerates
owner metadata. Roslyn supplies VB declarations; YAML/XML use their own parsers.
"""
from __future__ import annotations
import argparse
import collections
import csv
import hashlib
import io
import json
import pathlib
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
import yaml
import semantic

ROOT = pathlib.Path(__file__).resolve().parents[2]
DEFAULT = ROOT / "docs/architecture/WindowsMigrationLedger"


def load(ledger, name):
    with (ledger / name).open(encoding="utf-8-sig", newline="") as stream:
        return list(csv.DictReader(stream))


def render(rows):
    if not rows:
        return ""
    stream = io.StringIO(newline="")
    writer = csv.DictWriter(stream, list(dict.fromkeys(k for r in rows for k in r)), lineterminator="\n", quoting=csv.QUOTE_ALL)
    writer.writeheader()
    writer.writerows(rows)
    return stream.getvalue()


def derive(ledger):
    path = ROOT / "BESHStatNG_Help_MkDocs/docs/_meta/methods.yml"
    text = path.read_text(encoding="utf-8-sig")
    catalog = yaml.safe_load(text)
    # YAML compose retains authored entry lines independently of generated inventories.
    tree = yaml.compose(text)
    sections = {key.value: value for key, value in tree.value}
    source_lines = {(section, next(value.value for key, value in node.value if key.value == "key")): node.start_mark.line + 1
                    for section in ("methods", "commands", "pages") for node in sections[section].value}
    ribbons = {r["ribbon_id"]: r for r in load(ledger, "ribbon-crosswalk.csv")}
    symbols = load(ledger, "symbols.csv")
    registry = load(ledger, "common-registry.csv")
    known = {"OneWayANOVA": ("OneWayAnova", "anova.oneway"),
             "KruskalWallisTest": ("KruskalWallis", "nonparametric.kruskal-wallis")}
    rows = []
    for section in ("methods", "commands", "pages"):
        for entry in catalog[section]:
            ids = entry.get("ribbonIds", [entry["ribbonId"]] if "ribbonId" in entry else [""])
            for ribbon_id in ids:
                ribbon = ribbons.get(ribbon_id, {})
                key = entry["key"]
                handler = ribbon.get("onAction", "")
                forms = [f for f in entry.get("forms", []) if not f.get("handler") or f["handler"] == handler]
                regs = [r for r in registry if key in known and "AnalysisIds." + known[key][0] + "," in r["registration"]]
                rows.append({
                    "catalog_id": key, "kind": section, "source_path": path.relative_to(ROOT).as_posix(),
                    "source_line": source_lines[section, key], "ribbon_id": ribbon_id,
                    "onAction": handler or entry.get("onAction", ""),
                    "authored_actions": "; ".join(entry.get("onActions", [entry["onAction"]] if "onAction" in entry else [])),
                    "authored_forms": json.dumps(forms, ensure_ascii=False, sort_keys=True),
                    "handler_ids": ribbon.get("handler_ids", "page-only"),
                    "common_analysis_id": known[key][1] if key in known else "no registered handler verified",
                    "common_registry_evidence": "; ".join(f'{r["path"]}:{r["line"]} | {r["registration"]}' for r in regs) or "none",
                    "office_status": "registered ANOVA/KW; actual execution also requires host capability/editor" if regs else "metadata/command/page only; no analysis execution inferred",
                    "status": "ribbon-and-lexical-handler-resolved" if ribbon else "page-only" if section == "pages" else "UNRESOLVED mapping",
                    "owner_role": "Catalog / Windows Ribbon / Common registry maintainers", "future_batch": "04A.45 / Gate C2",
                    "verification": "Compare exact ID/action/form/tag to owner Ribbon activation; metadata is descriptive, not executable capability",
                    "evidence_status": "static",
                })
    # Every callback implementation remains discoverable, including non-Ribbon public members.
    callbacks = [{"symbol_id": s["symbol_id"], "path": s["path"], "line": s["start_line"], "name": s["name"],
                  "ribbon_ids": "; ".join(r["ribbon_id"] for r in ribbons.values() if r["onAction"] == s["name"]),
                  "status": "referenced by Ribbon" if any(r["onAction"] == s["name"] for r in ribbons.values()) else "not onAction-referenced; lifecycle/getImage/other caller review required",
                  "owner_role": "Windows Ribbon maintainer", "future_batch": "04A.45", "verification": "Review non-action XML callbacks and public/lifecycle references before declaring dead"}
                 for s in symbols if s["path"] == "BESHstatNG/BESHStatAddIn.vb"]
    # Preserve every XML callback attribute, not just buttons/onAction.
    xml_callbacks = []
    external_path = ledger / "external-callback-evidence.json"
    external = json.loads(external_path.read_text(encoding="utf-8-sig")) if external_path.exists() else {}
    for element in ET.parse(ROOT / "BESHstatNG/Resources/RibbonXML.xml").iter():
        for attr, value in element.attrib.items():
            if attr in ("onAction", "onLoad", "loadImage") or attr.startswith("get"):
                targets = [s for s in symbols if s["path"] == "BESHstatNG/BESHStatAddIn.vb" and s["name"] == value]
                inherited = value == external.get("callback") and hashlib.sha256((ROOT / external["assembly_path"]).read_bytes()).hexdigest() == external["assembly_sha256"]
                xml_callbacks.append({"element": element.tag.rsplit("}", 1)[-1], "id": element.get("id", "ribbon-root"),
                                      "attribute": attr, "callback": value, "target_ids": "; ".join(s["symbol_id"] for s in targets),
                                      "status": "resolved-syntax; runtime unproven" if targets else "resolved-inherited-assembly-declaration; runtime unproven" if inherited else "UNRESOLVED",
                                      "external_evidence": json.dumps(external, sort_keys=True) if inherited else "",
                                      "owner_role": "Windows Ribbon maintainer", "future_batch": "04A.45",
                                      "verification": "Owner native Excel callback signature/dispatch smoke"})
    coverage = {
        "methods": len(catalog["methods"]), "commands": len(catalog["commands"]), "pages": len(catalog["pages"]),
        "method_button_mappings": sum(len(x.get("ribbonIds", [])) for x in catalog["methods"]),
        "registry_handler_instances": len(registry), "registered_analysis_ids": sorted({r["common_analysis_id"] for r in rows if r["common_registry_evidence"] != "none"}),
        "unmapped_ribbon_ids": sorted(set(ribbons) - {r["ribbon_id"] for r in rows}),
        "unresolved_catalog_ids": [r["catalog_id"] for r in rows if r["status"].startswith("UNRESOLVED")],
        "xml_callback_attributes": len(xml_callbacks), "unresolved_xml_callbacks": [r for r in xml_callbacks if r["status"] == "UNRESOLVED"],
        "catalog_sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
    }
    state = load(ledger, "state.csv")
    edges = load(ledger, "ui-wiring.csv")
    config_index, handles_index = collections.defaultdict(list), collections.defaultdict(list)
    for e in edges:
        if e["kind"] == "Designer-configuration" and e["event_or_control"].startswith("Me."):
            config_index[e["containing_type"], e["event_or_control"].split(".")[1]].append(e)
        if e["kind"] == "Handles":
            handles_index[e["containing_type"], e["event_or_control"].split(".")[0].lower()].append(e)
    controls = []
    control_reviews = {r["control_id"]: r for r in semantic.read_store("control-review-decisions.json")}
    for field in state:
        if field["with_events"] != "True" or "/UI/" not in field["path"]:
            continue
        prefix = "Me." + field["name"] + "."
        config = config_index[field["containing_type"], field["name"]]
        handles = handles_index[field["containing_type"], field["name"].lower()]
        decorative = bool(re.search(r"\b(Label|Panel|GroupBox|TabPage|ToolTip)\b", field["declaration"]))
        controls.append({"control_id": field["containing_type"] + "." + field["name"], "path": field["path"], "line": field["start_line"], "type_declaration": field["declaration"],
                         "initialization_defaults": "; ".join(e["event_or_control"] + "=" + e["target"] + " @" + e["path"] + ":" + e["start_line"] for e in config),
                         "handles_edges": "; ".join(e["edge_id"] for e in handles),
                         "state": "wired-static-Handles; runtime unproven" if handles else "no-event decorative/container candidate; intentionality unverified" if decorative else "no-Handles; dynamic/default/indirect wiring requires review",
                         "enable_reset_validation": "Configured assignments above; dynamic changes in owning form symbols/references require per-option review",
                         "owner_role": "Windows UI maintainer", "future_batch": "04A.45", "verification": "04A.02 initial/reset/enable/output/range/validation/focus goldens; 04A.46/47 fake-host UI wiring"})
        contract = control_reviews.get(controls[-1]["control_id"], {})
        controls[-1]["baseline_heuristic_state"] = controls[-1]["state"]
        if contract:
            controls[-1]["state"] = "source-traced indirect/dynamic control intent; runtime unproven"
        controls[-1].update({"intent_review_status": contract.get("review_status", "unreviewed"),
                            "reviewed_intent": contract.get("reviewed_intent", "unreviewed; no intentionality inferred"),
                            "source_traced_edges": contract.get("source_edges", ""), "handler_symbol_ids": contract.get("handler_symbol_ids", ""),
                            "intent_rationale": contract.get("rationale", ""), "option_contract_review": contract.get("option_contract", "unreviewed")})
        for key in ("review_contract", "defaults_contract", "enabled_reset_contract", "validation_output_contract", "option_contract"):
            controls[-1][key] = contract.get(key, "unreviewed")
    findings = [{"finding_id": "MEMBER|" + s["symbol_id"], "path": s["path"], "line": s["start_line"], "reason": s["rationale"], "owner_role": s["owner_role"],
                 "future_batch": s["future_batch"], "verification": s["proposed_test"], "risk": s["risk"], "status": "OPEN"} for s in symbols if s["disposition"] == "needs-investigation"]
    findings += [{"finding_id": "EDGE|" + e["edge_id"], "path": e["path"], "line": e["start_line"], "reason": e["kind"] + ": " + e["event_or_control"] + " -> " + e["target"] + "; " + e["target_status"],
                  "owner_role": e.get("owner_role") or "Windows UI/host wiring maintainer", "future_batch": e.get("future_batch") or "04A.45",
                  "verification": e.get("verification") or "Resolve delegate type, target instance and attach/detach/focus/default behavior with fake/native host",
                  "risk": "Static matching does not prove dispatch, no-event intent or lifecycle detach", "status": "OPEN"}
                 for e in edges if "unresolved" in e["target_status"]]
    files = load(ledger, "files.csv")
    review = [{"path": f["path"], "parsed_records": f["parsed_member_count"], "manually_reviewed_static_records": sum(s["review_status"] == "manually-reviewed-static" for s in symbols if s["path"] == f["path"]),
               "needs_investigation": sum(s["disposition"] == "needs-investigation" for s in symbols if s["path"] == f["path"]), "runtime_or_semantic_parity_proven": 0} for f in files]
    physical = [{"path": p.relative_to(ROOT).as_posix(), "explicit_compile": p.relative_to(ROOT).as_posix() in {f["path"] for f in files},
                 "kind": "compiled source" if p.relative_to(ROOT).as_posix() in {f["path"] for f in files} else "non-Compile physical VB; ignored/generated output or exclusion; excluded from source denominator",
                 "sha256": hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted((ROOT / "BESHstatNG").rglob("*.vb")) if ".vs" not in p.parts]
    # Ignored outputs are deliberately volatile; capture their existence without byte-comparison claims.
    physical = [p for p in physical if p["explicit_compile"] or not any(x.lower() in ("bin", "obj") for x in pathlib.PurePosixPath(p["path"]).parts)]
    review_coverage = {"parsed_records": len(symbols), "manually_reviewed_static_records": sum(s["review_status"] == "manually-reviewed-static" for s in symbols),
                      "unreviewed_static_records": sum(s["review_status"] != "manually-reviewed-static" for s in symbols), "runtime_or_semantic_parity_proven": 0,
                      "member_investigations": sum(s["disposition"] == "needs-investigation" for s in symbols), "unresolved_edge_rows": sum("unresolved" in e["target_status"] for e in edges),
                      "controls": len(controls), "control_states": dict(collections.Counter(c["state"] for c in controls)),
                      "baseline_control_states": dict(collections.Counter(c["baseline_heuristic_state"] for c in controls)),
                      "files_without_manual_member_review": [r["path"] for r in review if r["manually_reviewed_static_records"] == 0], "acceptance_status": "NOT READY FOR ACCEPTANCE"}
    semantic_coverage = semantic.summary(symbols, edges, controls, files)
    review_coverage.update({"qualified_source_reviews": semantic_coverage["qualified_source_reviews"],
                           "semantic_unreviewed_records": semantic_coverage["unreviewed_for_acceptance"],
                           "legacy_reviews_requiring_details": semantic_coverage["legacy_pinned_reviews_requiring_details"],
                           "independently_checked_control_intents": semantic_coverage["independently_checked_control_intents"]})
    remaining = [{"symbol_id": s["symbol_id"], "path": s["path"], "start_line": s["start_line"], "end_line": s["end_line"], "family": s["family"],
                  "review_reason": "legacy pinned review lacks member-v1 details" if s["review_status"] == "manually-reviewed-static" else "source-specific review not yet transcribed",
                  "disposition": s["disposition"], "owner_role": s["owner_role"], "provisional_batch": s["future_batch"],
                  "required_action": "Inspect body and meaningful dependencies in 04A.01; pin precise responsibility, callers, whole/helper reuse, destination, approved follow-up, risks and named fixture; do not treat provisional batch as accepted allocation."}
                 for s in symbols if not semantic.qualified(s)]
    return {"catalog-crosswalk.csv": render(rows), "callback-index.csv": render(callbacks),
            "xml-callbacks.csv": render(xml_callbacks), "catalog-coverage.json": json.dumps(coverage, indent=2, ensure_ascii=False) + "\n",
            "ui-controls.csv": render(controls), "investigations.csv": render(findings), "review-by-file.csv": render(review),
            "physical-source-census.csv": render(physical), "review-coverage.json": json.dumps(review_coverage, indent=2) + "\n",
            "semantic-review-coverage.json": json.dumps(semantic_coverage, indent=2) + "\n", "remaining-member-review.csv": render(remaining)}


def verify(ledger):
    errors = []
    ns = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
    project = ET.parse(ROOT / "BESHstatNG/BESHStatNG.vbproj")
    expected = {"BESHstatNG/" + x.attrib["Include"].replace("\\", "/") for x in project.findall(".//m:Compile", ns)}
    files = load(ledger, "files.csv")
    actual = [r["path"] for r in files]
    if set(actual) != expected or len(actual) != len(expected):
        errors.append("compiled file ledger differs: missing=" + repr(sorted(expected - set(actual))) + "; extra=" + repr(sorted(set(actual) - expected)))
    for row in files:
        if hashlib.sha256((ROOT / row["path"]).read_bytes()).hexdigest() != row["sha256"]:
            errors.append("source hash drift: " + row["path"])
    syms = load(ledger, "symbols.csv")
    errors.extend(semantic.verify(syms, load(ledger, "ui-wiring.csv"), load(ledger, "ui-controls.csv")))
    counts = collections.Counter(r["path"] for r in syms)
    ids = {r["symbol_id"] for r in syms}
    coverage = json.loads((ledger / "coverage.json").read_text(encoding="utf-8"))
    if len(ids) != len(syms) or ids != set(coverage["inventoried_ids"]):
        errors.append("symbol IDs missing/extra/duplicate versus independently re-enumerated Roslyn coverage; run analyzer --check")
    for f in files:
        if counts[f["path"]] != int(f["parsed_member_count"]):
            errors.append("member count mismatch: " + f["path"])
    allowed = {"already-in-Core", "already-in-Common", "extract-to-Core", "extract-to-Common", "retain-Windows-host-only", "compatibility-facade-to-preserve", "needs-investigation"}
    for s in syms:
        if s["disposition"] not in allowed or any(not s.get(k) for k in ("symbol_id", "signature_key", "path", "signature", "owner_role", "future_batch", "rationale", "proposed_test")):
            errors.append("unclassified/incomplete member: " + s["symbol_id"])
        if s["disposition"].startswith("already-in-") and s["core_common_equivalent"] == "none verified":
            errors.append("unverified portable-equivalent claim: " + s["symbol_id"])
    for name, id_key in (("ui-wiring.csv", "edge_id"), ("udf-exports.csv", "export_id")):
        rows = load(ledger, name)
        if len({r[id_key] for r in rows}) != len(rows):
            errors.append("duplicate IDs: " + name)
        if name == "udf-exports.csv" and (len(rows) != coverage["udf_attribute_denominator"] or any(r["symbol_id"] not in ids for r in rows)):
            errors.append("missing/unbound UDF export")
        if name == "ui-wiring.csv" and collections.Counter(r["kind"] for r in rows) != collections.Counter(coverage["edges_by_kind"]):
            errors.append("missing/extra event or Designer/control edge")
    for name, content in derive(ledger).items():
        if not (ledger / name).exists() or (ledger / name).read_text(encoding="utf-8") != content:
            errors.append("catalog derived data drift: " + name)
    return errors


def main():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument("--ledger", type=pathlib.Path, default=DEFAULT)
    group = parser.add_mutually_exclusive_group(required=True)
    group.add_argument("--write", action="store_true")
    group.add_argument("--check", action="store_true")
    args = parser.parse_args()
    if args.write:
        for name, content in derive(args.ledger).items():
            (args.ledger / name).write_text(content, encoding="utf-8", newline="")
    errors = verify(args.ledger)
    print(json.dumps({"result": "FAIL" if errors else "PASS with explicit open semantic/wiring risks", "errors": errors}, indent=2))
    return bool(errors)


if __name__ == "__main__":
    sys.exit(main())
