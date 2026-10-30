"""Separate source-pinned review coverage from deterministic syntax census.

No keyword/family rule confers manual review. Legacy decisions remain visible but
do not satisfy the detailed member-v1 acceptance contract by implication.
"""
from __future__ import annotations
import collections
import csv
import functools
import hashlib
import json
import pathlib

ROOT = pathlib.Path(__file__).resolve().parents[2]
TOOLING = ROOT / "scripts/windows-migration-audit"
FIELDS = ("responsibility", "source_purpose", "separable_responsibilities", "equivalence_scope", "equivalent_evidence",
          "reusable_operation", "destination", "future_batch", "risk", "exception_tags", "proposed_test", "rationale", "owner_role", "runtime_boundary")


def qualified(row):
    return (row.get("review_contract") == "04A.01-member-v1" and row.get("review_status") == "manually-reviewed-static"
            and all(row.get(k, "").strip() for k in FIELDS)
            and row.get("equivalence_scope") in ("none", "helper-only", "whole-method"))


def read_store(name):
    return json.loads((TOOLING / name).read_text(encoding="utf-8-sig"))


def control_qualified(row):
    return (row.get("review_contract") == "04A.01-control-v1"
            and all(row.get(k, "").strip() and "unreviewed" not in row[k].lower()
                    for k in ("defaults_contract", "enabled_reset_contract", "validation_output_contract", "option_contract")))


def verify_pins(records, label):
    errors = []
    observed = {}
    for record in records:
        for path, digest in json.loads(record.get("dependency_pins", "{}")).items():
            target = ROOT / path
            if path not in observed:
                observed[path] = hashlib.sha256(target.read_bytes()).hexdigest() if target.is_file() else None
            if observed[path] != digest:
                errors.append(label + " stale dependency: " + path)
    return errors


@functools.lru_cache(maxsize=1)
def portable_sources():
    with (ROOT / 'docs/architecture/WindowsMigrationLedger/portable-symbols.csv').open(encoding='utf-8-sig', newline='') as stream:
        return {s['symbol_id']: s for s in csv.DictReader(stream)}


def verify_member_contract(decision, source):
    """Continuation02 adds exact identity and an explicit dependency manifest.

    Older qualified reviews retain their existing contract. This revision never
    grants qualification to generic/family-generated classifications.
    """
    if decision.get("review_revision") not in ("04A.01-continuation-02", "04A.01-continuation-03"):
        return []
    errors = []
    identity_fields = ("symbol_id", "path", "start_line", "end_line", "signature", "containing_type", "body_sha256")
    try:
        if json.loads(decision.get("source_identity", "null")) != {k: source[k] for k in identity_fields}:
            errors.append("incorrect exact source identity: " + source["symbol_id"])
    except (TypeError, ValueError):
        errors.append("invalid exact source identity: " + source["symbol_id"])
    try:
        required = json.loads(decision.get("required_dependency_paths", "null"))
        pins = json.loads(decision.get("dependency_pins", "{}"))
        if (not isinstance(required, list) or not required or len(required) != len(set(required))
                or set(required) != set(pins) or source["path"] not in required):
            errors.append("incomplete required dependency manifest: " + source["symbol_id"])
    except (TypeError, ValueError):
        errors.append("invalid required dependency manifest: " + source["symbol_id"])
    if not decision.get("exception_applicability", "").strip():
        errors.append("missing exception applicability: " + source["symbol_id"])
    if decision.get('review_revision') == '04A.01-continuation-03':
        if any(decision.get('source_'+k) != source.get(k) for k in ('profile','span_start','span_length')):
            errors.append('incorrect source profile/span: ' + source['symbol_id'])
        try:
            assessment = json.loads(decision.get('equivalence_assessment','{}'))
            targets = json.loads(decision.get('portable_equivalent_pins','[]'))
            if 'compared_portable_declarations' not in decision:
                errors.append('missing comparison-only declaration field: ' + source['symbol_id'])
            comparisons = json.loads(decision.get('compared_portable_declarations','[]'))
            ids = [t['symbol_id'] for t in targets]
            scope = decision.get('equivalence_scope')
            if (assessment.get('scope') != scope or sorted(assessment.get('target_ids',[])) != sorted(ids)
                    or len(ids) != len(set(ids)) or (scope != 'none' and not ids)
                    or (scope == 'whole-method' and len(ids) != 1) or not assessment.get('proof','').strip()):
                errors.append('unsupported whole/helper equivalence assertion: ' + source['symbol_id'])
            for target in targets + comparisons:
                actual = portable_sources().get(target['symbol_id'])
                if actual is None or any(target.get(k) != actual.get(k) for k in identity_fields):
                    errors.append('stale portable equivalent declaration: ' + source['symbol_id'])
        except (KeyError,TypeError,ValueError):
            errors.append('invalid portable equivalence assertion: ' + source['symbol_id'])
    return errors


def verify(symbols, edges, controls, decisions=None, wiring=None, intents=None):
    decisions = read_store("review-decisions.json") if decisions is None else decisions
    wiring = read_store("wiring-review-decisions.json") if wiring is None else wiring
    intents = read_store("control-review-decisions.json") if intents is None else intents
    errors = verify_pins(decisions, "member") + verify_pins(wiring, "wiring") + verify_pins(intents, "control")
    pinned_ids = set()
    for d in decisions:
        matches = [s for s in symbols if s["signature_key"] == d["signature_key"] and s["body_sha256"] == d["body_sha256"]
                   and ("symbol_id" not in d or s["symbol_id"] == d["symbol_id"])]
        if len(matches) != 1:
            errors.append("stale/ambiguous pinned ownership decision: " + d["signature_key"])
            continue
        s = matches[0]
        errors += verify_member_contract(d, s)
        if s["symbol_id"] in pinned_ids:
            errors.append("duplicate ownership decision: " + s["symbol_id"])
        pinned_ids.add(s["symbol_id"])
        if any(s.get(k) != v for k, v in d.items()):
            errors.append("pinned ownership decision drift: " + s["symbol_id"])
        if d.get("review_contract") == "04A.01-member-v1" and not qualified(d):
            errors.append("incomplete semantic ownership decision: " + s["symbol_id"])
    for s in symbols:
        if s.get("review_status") == "manually-reviewed-static" and s["symbol_id"] not in pinned_ids:
            errors.append("unbacked pinned ownership decision: " + s["symbol_id"])
    edge_index = {e["edge_id"]: e for e in edges}
    pinned_edges = set()
    for d in wiring:
        e = edge_index.get(d["edge_id"])
        if e is None:
            errors.append("missing pinned wiring edge: " + d["edge_id"])
            continue
        identity = "|".join(e[k] for k in ("path", "kind", "event_or_control", "target", "start_line", "end_line", "owner_id"))
        if hashlib.sha256(identity.encode()).hexdigest() != d["edge_sha256"]:
            errors.append("stale wiring edge hash: " + d["edge_id"])
        if d["edge_id"] in pinned_edges:
            errors.append("duplicate wiring decision: " + d["edge_id"])
        pinned_edges.add(d["edge_id"])
        if any(e.get(k) != v for k, v in d.items() if k not in ("edge_sha256", "dependency_pins")):
            errors.append("pinned wiring decision drift: " + d["edge_id"])
        if any(sid not in {s["symbol_id"] for s in symbols} for sid in d["target_ids"].split("; ") if sid):
            errors.append("unbound reviewed handler symbol: " + d["edge_id"])
    for e in edges:
        if e.get("review_status") == "manually-reviewed-static-edge" and e["edge_id"] not in pinned_edges:
            errors.append("unbacked wiring review: " + e["edge_id"])
    control_index = {c["control_id"]: c for c in controls}
    full_controls = set()
    for d in intents:
        c = control_index.get(d["control_id"])
        if c is None or c["path"] != d["declaration_path"] or c["line"] != d["declaration_line"] or c["type_declaration"] != d["type_declaration"]:
            errors.append("stale control intent: " + d["control_id"])
        if any(eid not in pinned_edges for eid in d["source_edges"].split("; ") if eid):
            errors.append("unbacked control binding: " + d["control_id"])
        if d.get("review_contract") == "04A.01-control-v1":
            full_controls.add(d["control_id"])
            if not control_qualified(d):
                errors.append("incomplete full control contract: " + d["control_id"])
            if c is not None and any(c.get(k) != d.get(k) for k in
                    ("review_contract", "defaults_contract", "enabled_reset_contract", "validation_output_contract", "option_contract")):
                errors.append("full control contract drift: " + d["control_id"])
    for c in controls:
        if c.get("review_contract") == "04A.01-control-v1" and c["control_id"] not in full_controls:
            errors.append("unbacked full control contract: " + c["control_id"])
    if any(d.get('checkpoint_id') == '04A.01-continuation-19' for d in intents):
        import continuation19_controls
        errors += continuation19_controls.verify(intents, symbols, controls, edges)
    if any(d.get('checkpoint_id') == '04A.01-continuation-20' for d in intents):
        import continuation20_controls
        errors += continuation20_controls.verify(intents, symbols, controls, edges)
    if any(d.get('checkpoint_id') == '04A.01-continuation-21' for d in intents):
        import continuation21_controls
        errors += continuation21_controls.verify(intents, symbols, controls, edges)
        import continuation22_controls
        errors += continuation22_controls.verify(intents, symbols, controls, edges)
    if any(d.get('checkpoint_id') == '04A.01-continuation-23' for d in intents):
        import continuation23_report
        errors += continuation23_report.verify_current(intents, symbols, controls, edges)
    if any(d.get('checkpoint_id') == '04A.01-continuation-24' for d in intents):
        import continuation24_report
        errors += continuation24_report.verify_current(intents, symbols, controls, edges)
    if any(d.get('checkpoint_id') == '04A.01-continuation-25' for d in intents):
        import continuation25_report
        errors += continuation25_report.verify_current(intents, symbols, controls, edges)
    if any(d.get('checkpoint_id') == '04A.01-continuation-26' for d in decisions):
        import continuation26_report
        errors += continuation26_report.verify_current(symbols, controls, edges, decisions)
    if any(d.get('checkpoint_id') == '04A.01-continuation-27' for d in decisions):
        import continuation27_report
        errors += continuation27_report.verify_current(symbols, controls, edges, decisions)
    return errors


def summary(symbols, edges, controls, files):
    reviewed = [s for s in symbols if qualified(s)]
    pinned = [s for s in symbols if s.get("review_status") == "manually-reviewed-static"]
    pinned_ids = {s["symbol_id"] for s in pinned}
    intents = read_store("control-review-decisions.json")
    rows = []
    for family in sorted({s["family"] for s in symbols}):
        all_rows = [s for s in symbols if s["family"] == family]
        q = [s for s in all_rows if qualified(s)]
        rows.append({"family": family, "parsed_records": len(all_rows), "pinned_reviews": sum(s["symbol_id"] in pinned_ids for s in all_rows),
                     "qualified_source_reviews": len(q), "legacy_reviews_requiring_details": sum(s["symbol_id"] in pinned_ids and not qualified(s) for s in all_rows),
                     "generated_rule_verified": 0, "unreviewed_for_acceptance": len(all_rows)-len(q),
                     "all_dispositions": dict(collections.Counter(s["disposition"] for s in all_rows)),
                     "reviewed_dispositions": dict(collections.Counter(s["disposition"] for s in q)),
                     "needs_investigation": sum(s["disposition"] == "needs-investigation" for s in all_rows),
                     "verified_whole_equivalents": sum(s.get("equivalence_scope") == "whole-method" for s in q),
                     "verified_helper_reuse": sum(s.get("equivalence_scope") == "helper-only" for s in q),
                     "inspected_extraction_candidates": sum(s["disposition"].startswith("extract-to-") for s in q),
                     "runtime_deferred_reviews": sum(s.get("runtime_boundary", "").startswith("04A.") for s in q)})
    # Preserve old table predicates, but explicitly show why their denominators differ.
    handles = {(e["containing_type"], e["event_or_control"].split(".")[0].lower()) for e in edges if e["kind"] == "Handles"}
    broad = [e for e in edges if e["kind"] == "WithEvents-control-or-source"]
    not_ui = [e for e in broad if "/UI/" not in e["path"]]
    # Use the original predicate (any prefix-matching source edge), independent of amended review annotations.
    prefixes = {(e["containing_type"],e["event_or_control"].split(".")[0].lower()) for e in edges
                if e["kind"] != "WithEvents-control-or-source" and "." in e["event_or_control"]}
    prefix_matched = [e for e in broad if "/UI/" in e["path"] and (e["containing_type"],e["event_or_control"].lower()) not in handles
                      and (e["containing_type"],e["event_or_control"].lower()) in prefixes]
    batches = []
    for family, disposition, batch in sorted({(s["family"],s["disposition"],s["future_batch"]) for s in symbols}):
        selected = [s for s in symbols if (s["family"],s["disposition"],s["future_batch"])==(family,disposition,batch)]
        batches.append({"family":family,"disposition":disposition,"batch":batch,"records":len(selected),
                        "qualified_reviewed":sum(qualified(s) for s in selected),"provisional_unreviewed":sum(not qualified(s) for s in selected)})
    return {"status":"NOT READY FOR ACCEPTANCE", "denominator":len(symbols),"pinned_reviews":len(pinned),
            "qualified_source_reviews":len(reviewed),"generated_rule_verified":0,"unreviewed_for_acceptance":len(symbols)-len(reviewed),
            "legacy_pinned_reviews_requiring_details":len(pinned)-len(reviewed),
            "files_without_qualified_review":[f["path"] for f in files if not any(s["path"]==f["path"] for s in reviewed)],
            "families":rows,"batch_allocations":batches,
            "control_review_denominator":len(controls),"independently_checked_control_intents":len(intents),
            "qualified_full_control_contracts":sum(control_qualified(c) for c in intents),
            "unreviewed_control_intents":len(controls)-len(intents),
            "reviewed_wiring_edges":sum(e.get("review_status")=="manually-reviewed-static-edge" for e in edges),
            "remaining_unresolved_edges_by_kind":dict(collections.Counter(e["kind"] for e in edges if "unresolved" in e["target_status"])),
            "runtime_only_delegate_boundaries":sum(e["target_status"]=="reviewed-runtime-supplied-delegate; runtime deferred" for e in edges),
            "denominator_explanation":{"ui_controls":len(controls),"all_WithEvents_edges":len(broad),
                "non_ui_WithEvents_edges":[e["edge_id"] for e in not_ui],
                "no_direct_Handles_but_prefix_matched_other_edge":[e["edge_id"] for e in prefix_matched],
                "formula":"1667 no-Handles UI controls - 37 UI prefix matches to other edge kinds = 1630 original no-Handles-found edges. The 429 broad matched edges comprise 389 direct-Handles UI controls + 37 other UI prefix matches + 3 non-UI chart fields; all three non-UI fields already match handlers. Broad prefix is not an independent intent review."}}
