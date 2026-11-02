"""Read-only successor source/gate checks and additive 04A.04 crosswalk production.

No historical record, native authority or numerical golden is changed.
"""
import argparse
import csv
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
BASE = "e1377cceabbe8890395f94cf41578b621aef3d4c"
LEDGER = "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"
AMENDMENT = "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json"
DESTINATION = "docs/architecture/04A04/compatibility-crosswalk.json"
TARGETS = {
    "UiTwoInputRefedits": ["RefEdit1", "RefEdit2", "btCompute", "optPaired", "optSingle", "spinBtnAlphaGlobal"],
    "Ui0OneRefeditMulticol": ["RefEdit1", "btCompute"],
    "Ui9ANOVA2nested": ["RefEdit1_Group", "RefEdit2_nested", "RefEdit3_Data", "btCompute", "ckBlandDecisionLimitsEnable",
        "tbBlandLowerAllowable", "tbBlandUpperAllowable", "cmbBlandMode", "cmbBlandScale", "spinBtnBlandAlpha",
        "optBlandBootstrap", "optBlandBootstrapBCa", "optBlandJackknife"],
}
PRODUCTION = ["BESHStat.Excel.Common/src/Input/PairedBlockInputPolicy.vb", "BESHStat.Excel.Common/src/Input/PairedOptionPolicy.vb",
              "BESHstatNG/src/DM/WindowsPairedInputAdapter.vb"] + [f"BESHstatNG/src/UI/{f}.vb" for f in TARGETS]


def sha(path):
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT)


def populations():
    amendment = json.loads((ROOT / AMENDMENT).read_bytes())
    ledger = json.loads((ROOT / LEDGER).read_bytes())
    shared = [c for c in amendment["case_transfers"] if c["case_id"] in ("04A02-ANOVA-NATIVE-01", "04A02-NORMALITY-NATIVE-01")]
    controls = set().union(*(set(c["blocked_control_ids"]) for c in shared))
    obligations = set().union(*(set(c["deferred_obligation_ids"]) for c in shared))
    return amendment, ledger, dict(shared_controls=len(controls), shared_obligations=len(obligations),
        transferred_controls=len(set(amendment["blocked_control_ids"])), transferred_obligations=len(set(amendment["distinct_deferred_obligation_ids"])),
        original_obligations=len(ledger), original_restricted_controls=len({r["original_id"] for r in ledger if r["id_kind"] == "control" and r["affected_live_golden_blocked"]}))


def build_crosswalk():
    amendment, ledger, counts = populations()
    by_id = {r["obligation_id"]: r for r in ledger}
    records = []
    for form, controls in TARGETS.items():
        designer = (ROOT / f"BESHstatNG/src/UI/{form}.Designer.vb").read_text(encoding="utf-8-sig")
        for name in controls:
            if not re.search(r"\b" + re.escape(name) + r"\b", designer):
                raise ValueError("Not a real designer control: " + form + "." + name)
            cid = "BESHStatNG." + form + "." + name
            oid = "control:" + cid
            original = by_id.get(oid)
            item = dict(control_id=cid, obligation_id=oid if original else None,
                        transfer_cases=[c["case_id"] for c in amendment["case_transfers"] if cid in c["blocked_control_ids"]],
                        source_path=f"BESHstatNG/src/UI/{form}.vb", native_executed=False, owner_released=False,
                        disposition="OPEN" if original else "NO_ORIGINAL_OBLIGATION_ROW_FOUND",
                        evidence="HOST_FREE_POLICY_REGRESSION; no control/default/event/native qualification")
            if original:
                for key in ["source_record_path", "source_record_sha256", "source_evidence", "risk_tiers", "owner_roles",
                            "earliest_approved_batches", "dependent_batches", "blocked_gate", "safe_capture_preconditions", "acceptance_condition", "verification_status", "owner_decision_status"]:
                    item[key] = original[key]
                item["original_ledger_row_sha256"] = hashlib.sha256(json.dumps(original, sort_keys=True, ensure_ascii=False).encode()).hexdigest()
            records.append(item)
    return dict(schema="04A04-paired-block-successor-crosswalk-v1", base=BASE, counts=counts,
        restrictions=dict(P="NO", N1="NO GO", N2="NO", Gate_C2="UNMET", released_controls=0, discharged_obligations=0),
        authorities={p: sha(p) for p in [LEDGER, AMENDMENT, "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
            "docs/architecture/04A03/pre-change-crosswalk.json", "docs/architecture/04A03/control-mitigation/successor-candidates.json",
            "docs/architecture/04A03/control-mitigation/native-readonly-plan.json"]},
        production={p: dict(after_sha256=sha(p), before_sha256=hashlib.sha256(git("show", BASE + ":" + p)).hexdigest()
                    if p.startswith("BESHstatNG/src/UI/") else None) for p in PRODUCTION},
        controls=records,
        compatibility=[
            dict(path="paired t/Wilcoxon", input_rows=[20,21,22,23,24,25,26], excluded=[20,23], retained=[21,22,24,25,26],
                 values=[[1,1],[2,1],[3,2],[4,3],[5,5]], direction="first-minus-second", reference="ParametricTests.PairedTtest_compute_matches_reference",
                 t=2.4494897427831779, p=0.070483996910219962, claim="existing reference reused, not promoted"),
            dict(path="RM ANOVA/Friedman", retained=[[1,2,3],[2,2,4],[3,4,5],[4,3,6]], treatment_order=["A","B","C"],
                 F=15.545454545454524, p=0.0042330297170771669, reference="ParametricTests.OneWayRmANOVA_compute_matches_reference"),
            dict(path="Hotelling", first_rows=[20,22,23], second_rows=[21,22], before_intersection=[22], after_intersection=[22]),
            dict(path="nested", policy="parent+child tuple membership, complete raw rows, original categorical runtime types and extra responses preserved; Core balancing/sorting unchanged"),
            dict(path="Bland", scales=["Raw difference","% of paired mean","% of reference","% of test","Log ratio"],
                 direction="test-minus-reference", limits="active-scale units; disabled ignored; order message unchanged; nonfinite active limits diagnosed",
                 repeated_missing_subject="preserve AllowFallbackToSimple", ci_precedence=["percentile","BCa","jackknife","analytical"])
        ],
        unresolved=["Legacy combined-reference import uses the first area's geometry for every area; no original-selection equivalence or correction is claimed.",
                    "Native pre-change control safety remains OPEN; existing COM/default/event/report/writer code remains Windows-owned.",
                    "No capture authorizations, public UDF changes, new goldens, 04A.05 or 04A.27 work."])


def check():
    data = json.loads((ROOT / DESTINATION).read_bytes())
    assert data == build_crosswalk(), "Successor crosswalk/source drift"
    assert data["counts"] == dict(shared_controls=141, shared_obligations=160, transferred_controls=218, transferred_obligations=239,
                                 original_obligations=1514, original_restricted_controls=802)
    assert all(not c["owner_released"] and not c["native_executed"] for c in data["controls"])
    assert all(not c["transfer_cases"] for c in data["controls"]), "Do not invent a four-case transfer for these forms"
    for p in PRODUCTION[:2]:
        source = (ROOT / p).read_text(encoding="utf-8-sig")
        assert "Option Strict On" in source
        assert not re.search(r"Microsoft\.(Office|VisualBasic)|ExcelDna|System\.Windows\.Forms|\bDataObj\b|AppGlobals|System\.IO", source)
    assert not git("diff", "--cached", "--name-only").strip(), "Staging is unauthorized"
    print("04A.04 successor contracts and exact-ID restrictions preserved")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["create", "refresh-working-draft", "check"])
    args = parser.parse_args()
    if args.action in ("create", "refresh-working-draft"):
        path = ROOT / DESTINATION
        path.parent.mkdir(parents=True, exist_ok=True)
        if args.action == "refresh-working-draft":
            previous = json.loads(path.read_bytes())
            assert previous["schema"] == "04A04-paired-block-successor-crosswalk-v1" and previous["base"] == BASE
            assert not git("ls-files", "--", DESTINATION).strip(), "Cannot replace a published record"
        with path.open("x" if args.action == "create" else "w", encoding="utf-8", newline="\n") as out:
            json.dump(build_crosswalk(), out, indent=2, ensure_ascii=False)
            out.write("\n")
        run = ROOT / "TestResults/04A-04-20261102-run01"
        if run.is_dir():
            for form in TARGETS:
                before = run / (form + "-before.vb")
                if not before.exists():
                    before.write_bytes(git("show", BASE + f":BESHstatNG/src/UI/{form}.vb"))
            correction = run / "preflight-head-correction.json"
            if not correction.exists():
                correction.write_text(json.dumps(dict(actual_starting_head=BASE, branch="codex/04A-04",
                    note="Initial start.json copied the previous verifier's historical HEAD literal. This additive correction and source preimages establish the actual 04A.04 baseline; historical inventory/pins remain unchanged."), indent=2) + "\n", encoding="utf-8")
    else:
        check()
