"""Owner-authorized 04A.27 completion; fresh outputs, unchanged historical verifiers."""
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
spec = importlib.util.spec_from_file_location("completion27_base05", ROOT / "scripts/verify-04a05.py")
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT).decode().strip()


def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-27-") or not (run / "start.json").is_file():
        raise ValueError("Fresh initialized 04A.27 completion evidence required")
    return run


base.evidence = evidence
base.isolation.evidence = evidence
final_source = inspect.getsource(base.final).replace("scripts/verify-04a05.py", "scripts/verify-04a27-completion.py")
final_source = final_source.replace('relative + "/" + args.configuration + "/results\'"',
                                    'relative + "/" + args.name + "/" + args.configuration + "/results\'"')
exec(final_source, base.__dict__)


def init(args):
    if git("branch", "--show-current") != "codex/04A-27" or git("rev-parse", "HEAD") != "865a4e73b4668fbcbb7a2a24cceda02cc64bc22c":
        raise ValueError("Unexpected branch/base; do not mutate Git")
    if git("diff", "--cached", "--name-only"):
        raise ValueError("Index must remain empty")
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
    names = set(git("ls-files").splitlines()) | set(git("ls-files", "--others", "--exclude-standard").splitlines())
    names |= {"docs/architecture/04A27/import-physical-geometry-PROPOSED.patch", "docs/architecture/04A27/transport-restrictions-v1.json",
        "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json", "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
        "docs/architecture/04A04/compatibility-crosswalk.json", "docs/architecture/04A05/owner-completion-dispositions-v1.json",
        "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"}
    pins = {}
    for name in sorted(names):
        path = ROOT / name
        if path.is_file():
            pins[name] = base.isolation.sha(path)
    dirty = git("diff", "--name-only").splitlines() + git("ls-files", "--others", "--exclude-standard").splitlines()
    for name in set(dirty + ["BESHstatNG/src/DM/ExcelDnaDataImporter.vb", "BESHStatNG.Test/BESHStatNG.Tests.vbproj"]):
        target = run / "preimages" / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(ROOT / name, target)
    base.isolation.emit(run / "baseline.json", dict(head=git("rev-parse", "HEAD"), main=git("rev-parse", "main"),
        branch=git("branch", "--show-current"), status=git("status", "--short", "--untracked-files=all"),
        diff_stat=git("diff", "--stat"), source_pins=pins, initial_dirty=dirty))
    protected = {p: h for p, h in pins.items() if p.startswith("docs/") or p.startswith("planning/") or p.endswith("VersionAttributes.vb")}
    base.isolation.emit(run / "start.json", dict(source_pins=protected, historical_files=len(entries),
        disk_free_bytes=shutil.disk_usage(ROOT).free, starting_head=git("rev-parse", "HEAD")))
    print("Initialized", len(entries), "historical files; free bytes", shutil.disk_usage(ROOT).free, flush=True)
    return 0


def focused(args):
    run = evidence(args.evidence)
    code = base.command(run, args.name + "-common-build", ["dotnet", "build", "BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj", "-c", args.configuration, "--no-restore"])
    if code:
        return code
    args.action = "common"
    args.filter = "FullyQualifiedName~PhysicalImporter27|FullyQualifiedName~Transport27|FullyQualifiedName~PairedBlock|FullyQualifiedName~TypedNumericRangeParser|FullyQualifiedName~UnivariateSelectionPolicy|FullyQualifiedName~Completion05Common|FullyQualifiedName~ContingencyPolicy"
    code = base.isolation.common(args)
    if code:
        return code
    code = base.windows_build(run, args.configuration, args.name)
    if code:
        return code
    args.action = "legacy"
    args.name += "-windows"
    args.filter = "FullyQualifiedName~PhysicalImporter27|FullyQualifiedName~Transport27|FullyQualifiedName~AcquisitionAdapter|FullyQualifiedName~MigrationRefEditBoundary09|FullyQualifiedName~PairedBlockAdapter|FullyQualifiedName~UnivariateSelectionAdapter|FullyQualifiedName~Completion05Adapter"
    args.vstest = base.tools()[1]
    return base.isolation.common(args)


def summary(args):
    run = evidence(args.evidence)
    ns = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
    old = json.loads((ROOT / "TestResults/04A-27-20261102-run01/final-summary.json").read_bytes())
    configurations = {}
    for config in ("Debug", "Release"):
        name = "final-" + config
        paths = [run / name / config / "results" / (s + ".trx") for s in ("Core", "OfficeAddin", "OfficeExecution")]
        paths += [run / "executions" / (name + "-" + s) / "results" / (t + ".trx") for s, t in (("Common", "Common"), ("Legacy", "Legacy"))]
        paths += [run / "projection-executions" / name / "results/ControlMitigation.trx"]
        suites, failures = [], []
        for path in paths:
            tree = ET.parse(path)
            definitions = {n.attrib["id"]: n.find(ns + "TestMethod").attrib for n in tree.findall(".//" + ns + "UnitTest")}
            for row in tree.findall(".//" + ns + "UnitTestResult"):
                if row.attrib["outcome"] != "Passed":
                    method = definitions[row.attrib["testId"]]
                    failures.append(dict(identity=method["className"] + "." + method["name"], test_name=row.attrib["testName"], outcome=row.attrib["outcome"]))
            suites.append(dict(path=path.relative_to(ROOT).as_posix(), sha256=base.isolation.sha(path), counters=tree.find(".//" + ns + "Counters").attrib))
        logs = [(run / (name + s + ".log")).read_text(encoding="utf-8", errors="replace") for s in ("-full", "-common-python")]
        nonpasses = {m: o for log in logs for o, m in re.findall(r"^(FAIL|ERROR): [^\n]+ \(([^()]+)\)", log, re.M)}
        previous = old["configurations"][config]
        total_python = sum(int(n) for log in logs for n in re.findall(r"^Ran (\d+) tests? in", log, re.M))
        expected_new = {
            "test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden": "FAIL",
            "test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch": "FAIL",
            "test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged": "FAIL"}
        new = {k: v for k, v in nonpasses.items() if k not in previous["python_nonpasses"]}
        configurations[config] = dict(managed_suites=suites, managed_pass=sum(int(s["counters"]["passed"]) for s in suites),
            managed_total=sum(int(s["counters"]["total"]) for s in suites), managed_failures=failures,
            expected_frozen_managed_only=Counter(f["identity"] for f in failures) == Counter(f["identity"] for f in previous["managed_failures"]),
            python_total=total_python, python_pass=total_python-len(nonpasses), python_nonpasses=nonpasses,
            prior_python_preserved=all(nonpasses.get(k) == v for k, v in previous["python_nonpasses"].items()),
            additional_source_conflicts=new, expected_source_conflicts_only=new == expected_new,
            node_pass=sum(int(n) for n in re.findall(r"\bpass (\d+)\s*$", logs[0], re.M)),
            node_fail=sum(int(n) for n in re.findall(r"\bfail (\d+)\s*$", logs[0], re.M)), semantic_repetitions=15)
    base.isolation.emit(run / "final-summary.json", dict(schema="04A27-geometry-completion-validation-v1", configurations=configurations,
        native_attempts=0, owner_released_controls=0, discharged_obligations=0))
    print(json.dumps({c: {k: v for k, v in r.items() if k not in ("managed_suites", "python_nonpasses", "managed_failures")} for c, r in configurations.items()}, indent=2))
    return int(any(not r["expected_frozen_managed_only"] or not r["prior_python_preserved"] or not r["expected_source_conflicts_only"] or r["node_fail"] for r in configurations.values()))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["init", "focused", "final", "execute", "summary", "preserve"])
    parser.add_argument("--evidence", required=True)
    parser.add_argument("--configuration", choices=["Debug", "Release"], default="Debug")
    parser.add_argument("--name", default="focused-01")
    parser.add_argument("--windows", action="store_true")
    parser.add_argument("--legacy", action="store_true")
    parser.add_argument("--vstest")
    parser.add_argument("--candidate-name")
    args = parser.parse_args()
    raise SystemExit(dict(init=init, focused=focused, final=base.final, execute=base.execute,
                          summary=summary, preserve=base.isolation.preservation)[args.action](args))
