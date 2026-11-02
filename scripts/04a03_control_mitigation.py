"""Bounded 04A.03 addendum: source reconciliation, safe test projection, no native executor."""
import argparse
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / "docs/architecture/04A03/control-mitigation"
HOST = "BESHstatNG/src/UI/UibyID.vb"
DESIGNER = "BESHstatNG/src/UI/UibyID.Designer.vb"
CASES = ("04A02-ANOVA-NATIVE-01", "04A02-NORMALITY-NATIVE-01")
CONTAINERS = ("TabControl1", "TabPage1", "TabPage_Options", "grpInput", "grpANOVA1MCP",
              "grpHomogeneityVariances", "lblAlpha", "lblRefedit1", "lblRefedit2")
OPTIONS = ("ckDescriptiveStatistics", "ckBoxPlot", "ckWelch", "ckLevene", "ckBartlett",
           "ckFlignerKilleen", "ckSquaredRanks", "ckBonferroni", "ckLSD", "ckTukey", "ckGamesHowell")
TARGETS = CONTAINERS + OPTIONS + ("btnHelp",)
PREFIX = "control:BESHStatNG.UibyID."
AMENDMENT = "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json"
LEDGER = "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"
ACCEPTED = [HOST, DESIGNER, "BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb",
            "BESHstatNG/src/DM/WindowsUnivariateInputAdapter.vb",
            "BESHStat.Excel.Common/src/Input/UnivariateSelectionPolicy.vb",
            "BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb",
            "BESHStatNG.Test/BESHStatNG.Tests.vbproj", "BESHstatNG/BESHStatNG.vbproj",
            "BESHStatNG.Test/src/UnivariateSelectionAdapterTests.vb",
            "BESHStat.Excel.Common.Tests/src/UnivariateSelectionPolicyTests.vb",
            "scripts/verify-04a03.py", "scripts/tests/test_04a03_selection.py",
            "docs/architecture/04A03/pre-change-crosswalk.json",
            "docs/architecture/batch-04A-03-owner-review.md", AMENDMENT, LEDGER,
            "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
            "docs/architecture/04A02/04A02-control-root-cause-matrix-v1.json",
            "docs/architecture/04A02/04A02-C07-exact-ID-disposition.json",
            "docs/architecture/04A02/04A02-C14-N1-risk-review.pending.json"]
spec = importlib.util.spec_from_file_location("accepted03", ROOT / "scripts/verify-04a03.py")
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write(path, value):
    base.isolation.emit(path, value)


def method(source, name):
    match = re.search(r"(?im)^    (?:Private |Public |Friend )?(?:Shared )?(?:Async )?(Sub|Function) " + re.escape(name) + r"\b.*?$", source)
    if match is None:
        raise ValueError("Missing method " + name)
    end = re.search(r"(?im)^    End " + match[1] + "$", source[match.end():])
    if end is None:
        raise ValueError("Missing method end " + name)
    return source[match.start():match.end() + end.end()]


def freeze(args):
    run = base.evidence(args.evidence)
    records = {}
    for relative in ACCEPTED:
        path = ROOT / relative
        records[relative] = sha(path)
        destination = run / "accepted-baseline" / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        with destination.open("xb") as stream:
            stream.write(path.read_bytes())
    write(run / "accepted-baseline.json", dict(schema="04A03-controls-addendum-baseline-v1", pins=records,
          branch="codex/04A-03", authority="Owner's current explicit accepted-scope addendum instruction, 2026-11-02; no cryptographic signature asserted",
          prior_owner_record="No later local 04A.03 decision file found; prior dossier retained with its historical PARTIAL text",
          accepted_04A03=True, four_provenance_conflicts="OWNER_DISPOSITIONED_NOT_ERASED",
          head=subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()))
    with (run / "accepted-tracked-diff.patch").open("xb") as stream:
        stream.write(subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), "diff", "--binary"], cwd=ROOT))
    print("Accepted baseline protected", len(records))


def verify_accepted(run):
    pins = json.loads((run / "accepted-baseline.json").read_bytes())["pins"]
    drift = [path for path, expected in pins.items() if sha(ROOT / path) != expected]
    if drift:
        raise ValueError("Accepted implementation/authority drift: " + ", ".join(drift))
    return pins


def load_population():
    if sha(ROOT / AMENDMENT) != "8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307":
        raise ValueError("Accepted technical amendment drift")
    if sha(ROOT / LEDGER) != "363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10":
        raise ValueError("Original ledger drift")
    amendment = json.loads((ROOT / AMENDMENT).read_bytes())
    cases = {row["case_id"]: row for row in amendment["case_transfers"]}
    controls = [set(cases[case]["blocked_control_ids"]) for case in CASES]
    obligations = [set(cases[case]["deferred_obligation_ids"]) for case in CASES]
    if controls[0] != controls[1] or obligations[0] != obligations[1]:
        raise ValueError("Shared case population changed")
    all_controls = set().union(*(set(c["blocked_control_ids"]) for c in cases.values()))
    all_obligations = set().union(*(set(c["deferred_obligation_ids"]) for c in cases.values()))
    population = dict(shared_blocked_controls=len(controls[0]), shared_deferred_obligations=len(obligations[0]),
                      wider_four_case_controls=len(all_controls), wider_four_case_obligations=len(all_obligations),
                      broader_restricted_controls=len(set(amendment["upstream_restrictions"]["restricted_live_control_ids"])),
                      original_obligations=len(json.loads((ROOT / LEDGER).read_bytes())))
    if list(population.values()) != [141, 160, 218, 239, 802, 1514]:
        raise ValueError("Population denominators changed")
    return amendment, controls[0], obligations[0], population


def validate_successor(value, originals):
    if value["populations_before"] != value["populations_after"] or value["full_obligations_ready_for_release"]:
        raise ValueError("Unapproved denominator/release reduction")
    rows = value["targets"]
    ids = [row["obligation_id"] for row in rows]
    if len(ids) != 21 or set(ids) != {PREFIX + name for name in TARGETS}:
        raise ValueError("Missing/duplicate/substituted target")
    if len(set(ids)) != len(ids):
        raise ValueError("Shared obligations counted twice")
    if value["selected_blocked_controls"] != 20 or value["selected_obligations"] != 21:
        raise ValueError("Help or shared controls counted incorrectly")
    for row in rows:
        original = originals[row["obligation_id"]]
        if set(row["case_ids"]) != set(CASES) or set(original["case_ids"]) != set(CASES):
            raise ValueError("Case coverage changed")
        if row["blocked_control"] != original["in_blocked_control_set"]:
            raise ValueError("Fabricated blocked control")
        if row["original_row_sha256"] != original["source_record"]["row_sha256"]:
            raise ValueError("Fabricated source row")
        if row["complete_obligation_status"] != "OPEN" or row["owner_released"] or row["native_executed"]:
            raise ValueError("No exact owner release/native evidence exists in this addendum")
        if row["release_classification"] != "NATIVE_EVIDENCE_PENDING":
            raise ValueError("Partial fields cannot release the complete obligation")
        if row["runtime_scope"] != "SOURCE_PROJECTED_REAL_MANAGED_CONTROLS_NOT_SHIPPING_CONSTRUCTOR":
            raise ValueError("Host-free projection cannot be labeled native")
        if row["partial_field_disposition"] != "READY_FOR_EXPLICIT_OWNER_DISPOSITION":
            raise ValueError("No owner disposition has been recorded")
    if (value["N1"], value["N2"], value["P"], value["Gate_C2"]) != ("NO_GO", "NO", "NO", "UNMET"):
        raise ValueError("Unauthorized gate release")
    if value["released_controls"] or value["discharged_obligations"]:
        raise ValueError("No reduction without real exact-ID owner approval")


def reviewed_identity_gate(plan, observed):
    """Offline predicate only; never discovers a process, loads UIA/COM or launches anything."""
    for key in ("pid", "start_utc_ticks", "session_id", "owner_sid", "hwnd"):
        actual = observed.get(key)
        expected = plan.get("reviewed_context", {}).get(key)
        if key != "owner_sid" and (type(actual) is not int or type(expected) is not int or actual <= 0):
            raise ValueError("Owned reviewed/live identity type/value mismatch: " + key)
        if actual is None or actual != expected:
            raise ValueError("Owned reviewed/live identity mismatch: " + key)
    if not observed.get("loaded_build_attribution_verified"):
        raise ValueError("Actual loaded XLL/source/build attribution missing")
    if observed.get("foreground_hwnd") != observed["hwnd"]:
        raise ValueError("Foreground HWND mismatch")
    # Synthetic data may exercise the comparison, never supply independent authority.
    raise ValueError("NO_GO: this version has no native executor or independent/owner N1 release")


def prepare(args):
    run = base.evidence(args.evidence)
    verify_accepted(run)
    directory = run / "projection"
    directory.mkdir(exist_ok=False)
    source = (ROOT / HOST).read_text(encoding="utf-8-sig")
    designer = (ROOT / DESIGNER).read_text(encoding="utf-8-sig")
    prefix = source.split("    Private Sub BindPilotLifecycle", 1)[0].replace("Public Class UibyID", "Public Class UibyIDControlProjection")
    constructor = method(source, "New")
    omissions = []
    for name in ("RefEdit1", "RefEdit2", "RefEditOutput"):
        line = "        Me." + name + ".ExcelConnector = AppGlobals.app"
        if constructor.count(line) != 1:
            raise ValueError("Constructor connector anchor drift")
        constructor = constructor.replace(line, "        ' OMITTED in test projection: " + name + " host connector")
        omissions.append(line.strip())
    constructor, count = re.subn(r"Me\.(spinBtnAlpha\w*)\.Value = AppGlobals.GetDefaultAlphaDecimal\([^\n]+", r"Me.\1.Value = 0.05D ' TEST INPUT; preferences not exercised", constructor)
    if count != 3:
        raise ValueError("Alpha anchor drift")
    omissions.append("Three AppGlobals alpha preference lookups; test supplies 0.05")
    constructor = constructor.replace("        Me.RefEdit1.txtAddress.Select()", "        ' OMITTED initial host-focus selection")
    omissions.append("Initial RefEdit focus selection")
    helpers = ""
    for name in ("UpdateBoxPlotOptionState", "UpdateCDFOptionState", "UpdateCategoricalHistogramOptionState",
                 "UpdateUnpairedTtestOptionVisibility", "ApplyUnpairedTtestInputLabels"):
        helpers += "\n    Private Sub " + name + "()\n        Throw New NotSupportedException(\"Out-of-cohort projection mode\")\n    End Sub\n"
    analysis = method(source, "RunOneWayANOVAThroughSharedLayer")
    initializer = re.search(r"        Dim analysisOptions As New [\s\S]+?\n        }", analysis)[0]
    capture = "\n    Public Function CaptureOptions(alpha As Double) As OneWayAnovaAnalysisOptions\n" + initializer + "\n        Return analysisOptions\n    End Function\n"
    projection = prefix + constructor + "\n" + method(source, "optByColumn_Click") + "\n" + method(source, "optByID_Click") + "\n" + method(source, "ComputeVarianceHomogeneity") + capture + helpers + "\nEnd Class\n"
    designer = designer.replace("Partial Class UibyID", "Partial Class UibyIDControlProjection").replace("GetType(UibyID)", "GetType(Global.BESHStatNG.UibyID)")
    for path, text in [(directory / "Projection.vb", projection), (directory / "Projection.Designer.vb", designer)]:
        with path.open("x", encoding="utf-8", newline="\n") as stream:
            stream.write(text)
    write(directory / "source-projection.json", dict(schema="04A03-controls-source-projection-v1", native=False,
          shipping_constructor_executed=False, scope="Test-only source projection; initialized real managed controls, never shown; actual shipping full constructor NOT executed",
          source_hashes={HOST: sha(ROOT / HOST), DESIGNER: sha(ROOT / DESIGNER)}, omissions=omissions,
          fail_closed_out_of_cohort_helpers=5, exact_methods=["optByColumn_Click", "optByID_Click", "ComputeVarianceHomogeneity"],
          initializer_sha256=hashlib.sha256(initializer.encode()).hexdigest(),
          generated_hashes={p.name: sha(p) for p in directory.glob("*.vb")}))
    print("Source projection prepared; no shipping constructor or native execution")


def managed(args):
    run = base.evidence(args.evidence)
    verify_accepted(run)
    output = run / "projection-executions" / args.name
    output.mkdir(parents=True, exist_ok=False)
    msbuild, vstest = base.tools()
    code = base.command(run, args.name + "-projection-build", [msbuild,
        "BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj", "/t:Build", "/m", "/v:minimal",
        "/p:Configuration=" + args.configuration, "/p:ProjectionSourceDir=" + str(run / "projection"),
        "/p:OutputPath=" + str(output / "bin") + "\\", "/p:IntermediateOutputPath=" + str(output / "obj") + "\\"])
    if code:
        return code
    adapter = ROOT / "BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common"
    code = base.command(run, args.name + "-projection-tests", [vstest, str(output / "bin/BESHStatNG.Test.dll"),
        "/TestAdapterPath:" + str(adapter), "/Logger:console;verbosity=minimal",
        "/Logger:trx;LogFileName=ControlMitigation.trx", "/ResultsDirectory:" + str(output / "results")])
    counters = ET.parse(output / "results/ControlMitigation.trx").find(".//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters").attrib
    if int(counters["total"]) != 61 or int(counters["executed"]) != 61:
        raise ValueError("Projection test discovery/execution incomplete: " + str(counters))
    print("Projection executed", counters["executed"], "passed", counters["passed"], flush=True)
    return code


def qualified_receipt(run):
    """An exit code alone, including zero-test discovery, cannot qualify fields."""
    candidates = []
    for path in (run / "projection-executions").glob("*/results/ControlMitigation.trx"):
        counters = ET.parse(path).find(".//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters").attrib
        if all(int(counters[key]) == 61 for key in ("total", "executed", "passed")) and int(counters["failed"]) == 0:
            candidates.append(path)
    if not candidates:
        raise ValueError("No qualifying executed host-free regression receipt")
    path = max(candidates, key=lambda item: item.stat().st_mtime_ns)
    return dict(path=path.relative_to(ROOT).as_posix(), sha256=sha(path), executed=61, passed=61, native=False,
                assembly_sha256=sha(path.parents[1] / "bin/BESHStatNG.Test.dll"))


def matrix(args):
    run = base.evidence(args.evidence)
    pins = verify_accepted(run)
    executed = qualified_receipt(run)
    amendment, blocked, obligations, population = load_population()
    originals = {r["obligation_id"]: r for r in amendment["obligation_transfers"]}
    roots = {r["obligation_id"]: r for r in json.loads((ROOT / "docs/architecture/04A02/04A02-control-root-cause-matrix-v1.json").read_bytes())["per_id"]}
    prior = {r["obligation_id"]: r for r in json.loads((ROOT / "docs/architecture/04A02/04A02-C07-exact-ID-disposition.json").read_bytes())["per_id"]}
    ledger = {r["obligation_id"]: r for r in json.loads((ROOT / LEDGER).read_bytes())}
    csv_path = ROOT / "docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv"
    with csv_path.open(encoding="utf-8-sig", newline="") as stream:
        rows = {r["control_id"]: r for r in csv.DictReader(stream)}
    records = []
    for name in TARGETS:
        oid = PREFIX + name
        original, root, previous = originals[oid], roots[oid], prior[oid]
        source_row = rows["BESHStatNG.UibyID." + name]
        digest = hashlib.sha256(json.dumps(source_row, sort_keys=True, ensure_ascii=False).encode()).hexdigest()
        if digest != original["source_record"]["row_sha256"] or digest != ledger[oid]["source_record_sha256"]:
            raise ValueError("Original source/ledger row mismatch: " + oid)
        if oid not in obligations or (name != "btnHelp") != ("BESHStatNG.UibyID." + name in blocked):
            raise ValueError("Target cohort membership mismatch: " + oid)
        current_source = (ROOT / HOST).read_text(encoding="utf-8-sig")
        current_designer = (ROOT / DESIGNER).read_text(encoding="utf-8-sig")
        for declaration in root["source"]["declarations"]:
            if declaration["text"] not in current_designer:
                raise ValueError("Unbound declaration: " + oid)
        for field in ("explicit_defaults", "parent"):
            for evidence in previous["source_fields"][field]["source_evidence"]:
                if evidence["text"] not in current_designer:
                    raise ValueError("Unbound designer field: " + oid)
        for use in root["source"]["runtime_uses"]:
            if use["text"] not in current_source:
                raise ValueError("Unbound runtime mapping: " + oid)
        active_modes = ["ANOVA", "KW", "normality"]
        if name in ("grpANOVA1MCP", "ckWelch", "ckBonferroni", "ckLSD", "ckTukey", "ckGamesHowell"):
            active_modes = ["ANOVA"]
        elif name in ("grpHomogeneityVariances", "ckLevene", "ckBartlett", "ckFlignerKilleen", "ckSquaredRanks"):
            active_modes = ["ANOVA", "KW"]
        elif name == "lblAlpha":
            active_modes = []
        tests = ["ModeProjection_ParentDefaultsAndLocalAvailability"]
        if name in ("lblRefedit1", "lblRefedit2"):
            tests.append("ActualManagedOrientationHandlers_ReentryAndEventOrder")
        if name in ("ckWelch", "ckBonferroni", "ckLSD", "ckTukey", "ckGamesHowell"):
            tests.append("ActualOptionInitializer_32Combinations_RealCommonRequestAndResultMetadata")
        if name in ("ckLevene", "ckBartlett", "ckFlignerKilleen", "ckSquaredRanks"):
            tests.append("ActualVarianceFunction_16Combinations_OrderAndPValueMetadata")
        if name == "btnHelp":
            tests = ["HelpResolverAndSubscriptionInventory_NoBrowserLaunch"]
        records.append(dict(obligation_id=oid, blocked_control=name != "btnHelp", case_ids=original["case_ids"],
            original_row_sha256=digest, original_ledger_sha256=sha(ROOT / LEDGER), source_record=original["source_record"],
            transfer_status=original["status"], original_closure_status=original["closure_status"],
            original_decision_row_sha256=hashlib.sha256(json.dumps(ledger[oid], sort_keys=True, ensure_ascii=False).encode()).hexdigest(),
            historical_source_pins=previous["source_pins"], accepted_pre_source_sha256=pins[HOST], post_source_sha256=sha(ROOT / HOST),
            risk_tiers=original["risk_tiers"], owner_roles=original["owner_roles"], clusters=root["clusters"],
            scientific_interpretation_required=root["scientific_interpretation_required"],
            prior_source_field_states={k: v["status"] for k, v in previous["source_fields"].items()},
            newly_checked_source_fields={"accepted_source_chain": "SOURCE_PROVEN", "mode_consumer_or_help_map": "SOURCE_PROVEN"},
            source_consumer_map=root["source"]["runtime_uses"], source_declarations=root["source"]["declarations"],
            mode_applicability=active_modes, help_or_option_consumer_test="test_per_mode_consumers_and_branch_order",
            host_free_test_ids=["ControlMitigationProjectionTests." + test for test in tests],
            runtime_scope="SOURCE_PROJECTED_REAL_MANAGED_CONTROLS_NOT_SHIPPING_CONSTRUCTOR",
            evidence_states=["SOURCE_PROVEN", "HOST_FREE_REGRESSION_PASSED", "NATIVE_EVIDENCE_PENDING", "INDEPENDENT_RISK_REVIEW_PENDING"],
            partial_field_disposition="READY_FOR_EXPLICIT_OWNER_DISPOSITION",
            release_classification="NATIVE_EVIDENCE_PENDING", complete_obligation_status="OPEN", native_executed=False,
            independent_review=None, owner_released=False, owner_release_record=None,
            missing=["Exact initialized native instance/control/provider observation", "Independent Windows/COM risk review", "Case-specific owner N1 GO", "Exact-ID successor owner release",
                     "Other-mode/shared-edge applicability remains OPEN; three-mode projection is not a whole-control contract",
                     "Browser launch not exercised" if name == "btnHelp" else "Actual form/rendering/event/COM composition not exercised"],
            original_pre_change_gate=original["pre_change_gate"], full_closure_artifacts=original["closure_artifacts"],
            original_owner_batches=original["ledger_earliest_approved_batches"], future_owners=["04A.06 normality", "04A.07 ANOVA", "04A.27 shared RefEdit", "04A.45-47 GUI"],
            estimated_remaining_effort="Shared independent review 45-90 min; separately authorized calibration 30-60 min; per-mode observation 5-10 min; estimates only"))
    value = dict(schema="04A03-controls-successor-candidate-v1", populations_before=population, populations_after=population,
                 selected_blocked_controls=20, selected_obligations=21, released_controls=0, discharged_obligations=0,
                 full_obligations_ready_for_release=0, newly_checked_source_fields=42, targets=records,
                 authority="Current owner addendum accepts prior 04A.03 scope/provenance disposition, no control release",
                 host_free_execution_receipt=executed, N1="NO_GO", N2="NO", P="NO", Gate_C2="UNMET")
    validate_successor(value, originals)
    write(DOC / "successor-candidates.json", value)
    print("21 exact rows verified; 42 source field assertions; released/discharged 0/0")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["freeze", "prepare", "managed", "matrix"])
    parser.add_argument("--evidence", required=True)
    parser.add_argument("--name", default="focused01")
    parser.add_argument("--configuration", choices=["Debug", "Release"], default="Debug")
    args = parser.parse_args()
    raise SystemExit(globals()[args.action](args) or 0)
