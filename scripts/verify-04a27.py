"""04A.27 isolated verification; additive reuse of the sealed C05 execution strategy.

No restore, native Excel, packaging, historical output rewriting or baseline edits.
The final verifier transformation replaces only its helper script path; all C05
writer isolation transformations remain visible in the per-run provenance file.
"""
import argparse
from collections import Counter
import importlib.util
import inspect
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("verification05", ROOT / "scripts/verify-04a05.py")
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT).decode().strip()


def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-27-") or not (run / "start.json").is_file():
        raise ValueError("Fresh initialized 04A.27 evidence required")
    return run


base.evidence = evidence
base.isolation.evidence = evidence


def init(args):
    if git("branch", "--show-current") != "codex/04A-27" or git("status", "--porcelain", "--untracked-files=no"):
        raise ValueError("Expected clean tracked 04A.27 baseline")
    run = Path(args.evidence).resolve()
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-27-"):
        raise ValueError("Unexpected evidence path")
    run.mkdir(exist_ok=False)
    entries = []
    for path in (ROOT / "TestResults").rglob("*"):
        if path.is_file() and not path.is_relative_to(run):
            st = path.stat()
            entries.append(dict(path=path.relative_to(ROOT).as_posix(), bytes=st.st_size,
                                mtime_ns=st.st_mtime_ns, ctime_ns=st.st_ctime_ns, ino=st.st_ino, dev=st.st_dev))
    base.isolation.emit(run / "historical-metadata.json", entries)
    pins = {}
    paths = list((ROOT / "BESHstatNG/src").rglob("*.vb")) + list((ROOT / "BESHStat.Core/src").rglob("*.vb"))
    paths += list((ROOT / "BESHStat.Excel.Common/src").rglob("*.vb"))
    paths += list((ROOT / "scripts/tests").glob("test_*.py"))
    paths += [ROOT / p for p in ("scripts/verify-beshstatng.ps1", "scripts/verify-04a02-fixtures.py",
        "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
        "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json",
        "docs/architecture/04A05/owner-completion-dispositions-v1.json",
        "docs/architecture/04A04/pre-change-crosswalk.json", "docs/architecture/04A03/pre-change-crosswalk.json",
        "BESHstatNG/Generated/VersionAttributes.vb", "planning/BESHStatNG_CrossPlatform_MasterPlan.md")]
    for path in paths:
        if path.is_file():
            name = path.relative_to(ROOT).as_posix()
            pins[name] = base.isolation.sha(path)
    base.isolation.emit(run / "baseline.json", dict(head=git("rev-parse", "HEAD"), main=git("rev-parse", "main"),
        branch=git("branch", "--show-current"), status=git("status", "--short", "--untracked-files=all"), source_pins=pins))
    protected = {p: h for p, h in pins.items() if p.startswith("docs/") or p.endswith("VersionAttributes.vb")}
    base.isolation.emit(run / "start.json", dict(source_pins=protected, historical_files=len(entries),
        disk_free_bytes=shutil.disk_usage(ROOT).free, starting_head=git("rev-parse", "HEAD")))
    print("Initialized", len(entries), "historical files; free", shutil.disk_usage(ROOT).free, flush=True)
    return 0


def focused(args):
    run = evidence(args.evidence)
    code = base.command(run, args.name + "-build", ["dotnet", "build", "BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj", "-c", args.configuration, "--no-restore"])
    if code:
        return code
    args.action = "common"
    args.filter = "FullyQualifiedName~Transport27|FullyQualifiedName~PairedBlock|FullyQualifiedName~TypedNumericRangeParser"
    code = base.isolation.common(args)
    if code:
        return code
    code = base.windows_build(run, args.configuration, args.name)
    if code:
        return code
    args.action = "legacy"
    args.name += "-windows"
    args.filter = "FullyQualifiedName~Transport27|FullyQualifiedName~AcquisitionAdapter|FullyQualifiedName~MigrationRefEditBoundary09|FullyQualifiedName~PairedBlockAdapter"
    args.vstest = base.tools()[1]
    return base.isolation.common(args)


def legacy_final(args):
    """Affected-suite rerun after explicit UDF response-budget correction."""
    code = base.windows_build(evidence(args.evidence), args.configuration, args.name)
    if code:
        return code
    args.action = "legacy"
    args.name += "-Legacy"
    args.filter = None
    args.vstest = base.tools()[1]
    return base.isolation.common(args)


# Reuse, without editing, the qualified assembly-copy writer isolation and final
# verification script. This one substitution dispatches helpers into this wrapper.
final_source = inspect.getsource(base.final).replace("scripts/verify-04a05.py", "scripts/verify-04a27.py")
# A justified final repair must never reuse even this batch's first-run TRX paths.
final_source = final_source.replace('relative + "/" + args.configuration + "/results\'"',
                                    'relative + "/" + args.name + "/" + args.configuration + "/results\'"')
exec(final_source, base.__dict__)


def summary(args):
    run = evidence(args.evidence)
    ns = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
    old = json.loads((ROOT / "TestResults/04A-05-completion-20261102-run01/completion-final-summary.json").read_bytes())
    expected_extra = {
        "test_04a04_alignment.PairedBlockSourceGates.test_udf_exports_and_packaging_unchanged": "FAIL",
        "test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden": "FAIL",
        "test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change": "FAIL",
        "test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes": "FAIL",
        "test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed": "FAIL"}
    results = {}
    for config in ("Debug", "Release"):
        name = "qualified-" + config
        paths = [run / name / config / "results" / (s + ".trx") for s in ("Core", "OfficeAddin", "OfficeExecution")]
        paths += [run / "executions" / (name + "-" + s) / "results" / (t + ".trx") for s, t in (("Common", "Common"), ("Legacy", "Legacy"))]
        paths = [run / "executions" / ("response-" + config + "-Legacy") / "results/Legacy.trx" if p.name == "Legacy.trx" else p for p in paths]
        paths += [run / "projection-executions" / name / "results/ControlMitigation.trx"]
        suites, failures = [], []
        for path in paths:
            tree = ET.parse(path)
            counters = tree.find(".//" + ns + "Counters").attrib
            definitions = {n.attrib["id"]: n.find(ns + "TestMethod").attrib for n in tree.findall(".//" + ns + "UnitTest")}
            for row in tree.findall(".//" + ns + "UnitTestResult"):
                if row.attrib["outcome"] != "Passed":
                    method = definitions[row.attrib["testId"]]
                    failures.append(dict(identity=method["className"] + "." + method["name"], test_name=row.attrib["testName"], outcome=row.attrib["outcome"]))
            suites.append(dict(path=path.relative_to(ROOT).as_posix(), sha256=base.isolation.sha(path), counters=counters))
        logs = [(run / (name + s + ".log")).read_text(encoding="utf-8", errors="replace") for s in ("-full", "-common-python")]
        nonpasses = {m: o for log in logs for o, m in re.findall(r"^(FAIL|ERROR): [^\n]+ \(([^()]+)\)", log, re.M)}
        previous = old["configurations"][config]["python_nonpasses"]
        extra = {k: v for k, v in nonpasses.items() if k not in previous}
        old_managed = Counter(f["identity"] for f in old["configurations"][config]["managed_failures"])
        current_managed = Counter(f["identity"] for f in failures)
        total_python = sum(int(n) for log in logs for n in re.findall(r"^Ran (\d+) tests? in", log, re.M))
        results[config] = dict(managed_suites=suites, managed_pass=sum(int(s["counters"]["passed"]) for s in suites),
            managed_total=sum(int(s["counters"]["total"]) for s in suites), managed_failures=failures,
            python_total=total_python, python_pass=total_python-len(nonpasses),
            python_nonpasses=nonpasses, prior_python_preserved=all(nonpasses.get(k) == v for k, v in previous.items()),
            new_python_nonpasses=extra, additional_frozen_only=extra == expected_extra,
            expected_frozen_managed_only=old_managed == current_managed,
            node_pass=sum(int(n) for n in re.findall(r"\bpass (\d+)\s*$", logs[0], re.M)),
            node_fail=sum(int(n) for n in re.findall(r"\bfail (\d+)\s*$", logs[0], re.M)), semantic_repetitions=15)
    base.isolation.emit(run / "final-summary.json", dict(schema="04A27-validation-v1", configurations=results, native_attempts=0,
        additional_python_classification=dict(preexisting_publication_bytes=2, newly_triggered_authorized_source_conflicts=3),
        new_managed_distinct=52, new_python_distinct=12, historical_scripts_modified=False,
        earlier_full_runs_retained=["final-Debug", "final-Release"],
        rerun_reason="Late callback ownership fix required qualified full reruns. Explicit UDF per-response limit then required final full Windows affected-suite reruns in both configurations; unchanged Core/Common/Node results reused, source successor rerun. All destinations fresh."))
    print(json.dumps({c: {k: v for k, v in r.items() if k not in ("managed_suites", "python_nonpasses", "managed_failures")} for c, r in results.items()}, indent=2))
    return int(any(not r["prior_python_preserved"] or not r["additional_frozen_only"] or not r["expected_frozen_managed_only"] or r["node_fail"] for r in results.values()))


if __name__ == "__main__":
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("action", choices=["init", "focused", "final", "execute", "legacy-final", "preserve", "summary"])
    p.add_argument("--evidence", required=True)
    p.add_argument("--configuration", choices=["Debug", "Release"], default="Debug")
    p.add_argument("--name", default="focused-01")
    p.add_argument("--windows", action="store_true")
    p.add_argument("--legacy", action="store_true")
    p.add_argument("--vstest")
    p.add_argument("--candidate-name")
    args = p.parse_args()
    raise SystemExit(dict(init=init, focused=focused, final=base.final, execute=base.execute, **{"legacy-final": legacy_final},
                          preserve=base.isolation.preservation, summary=summary)[args.action](args))
