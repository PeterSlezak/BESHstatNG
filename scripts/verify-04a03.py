"""Local-only 04A.03 verification: isolated test writers, no restore/native/golden work.

Reuses the accepted 04A.02 assembly-copy execution helper, without editing it.
The execution copy of verify-beshstatng.ps1 records every transformation. Windows
Compile/CopyFilesToOutputDirectory builds the real production project without its
BeforeBuild generators or AfterBuild Excel-DNA packaging/signing targets.
"""
import argparse
import csv
import difflib
import importlib.util
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
from types import SimpleNamespace

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
sys.stderr.reconfigure(encoding="utf-8", errors="replace")

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("isolation02", ROOT / "scripts/verify-04a02-fixtures.py")
isolation = importlib.util.module_from_spec(spec)
spec.loader.exec_module(isolation)


def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-03-"):
        raise ValueError("New top-level 04A.03 evidence directory required")
    if not (run / "start.json").is_file():
        raise ValueError("Initial protection record required")
    return run


isolation.evidence = evidence


def run_isolated(*args, **kwargs):
    # Frozen C02 tests only admit C02-named observation writers. Disable their
    # optional writer; new C03 tests use their own explicit, guarded destination.
    # The source/test files and their assertions remain byte-for-byte unchanged.
    env = kwargs.get("env")
    args[0].append("/Logger:console;verbosity=minimal")
    if env is not None:
        semantic_only = any("/TestCaseFilter:FullyQualifiedName~MigrationSemantic04A02Tests" in str(value) for value in args[0])
        destination = env.get("BESHSTAT_04A02_OBSERVATIONS") if semantic_only else env.pop("BESHSTAT_04A02_OBSERVATIONS", None)
        if destination:
            env["BESHSTAT_04A03_OBSERVATIONS"] = destination
    return subprocess.run(*args, **kwargs)


isolation.subprocess = SimpleNamespace(run=run_isolated, STDOUT=subprocess.STDOUT)


def command(run, name, args, env=None):
    start = time.monotonic()
    with (run / (name + ".log")).open("x", encoding="utf-8") as log:
        result = subprocess.run(args, cwd=ROOT, env=env, stdout=log, stderr=subprocess.STDOUT)
    isolation.emit(run / (name + "-command.json"), dict(command=args, exit_code=result.returncode,
                   seconds=time.monotonic() - start))
    print(name, result.returncode, flush=True)
    if result.returncode:
        lines = (run / (name + ".log")).read_text(encoding="utf-8", errors="replace").splitlines()
        useful = [line for line in lines if line.startswith(("FAIL:", "ERROR:", "FAILED", "Ran ", "Passed!", "Failed!"))]
        print("\n".join(useful[-25:] or lines[-12:]), flush=True)
    return result.returncode


def init(args):
    run = Path(args.evidence).resolve()
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-03-"):
        raise ValueError("Unexpected evidence directory")
    run.mkdir(exist_ok=False)
    # One initial inventory; no historical content is rewritten or repinned.
    entries = []
    for path in (ROOT / "TestResults").rglob("*"):
        if path.is_file() and not path.is_relative_to(run):
            st = path.stat()
            entries.append(dict(path=path.relative_to(ROOT).as_posix(), bytes=st.st_size,
                                mtime_ns=st.st_mtime_ns, ctime_ns=st.st_ctime_ns, ino=st.st_ino, dev=st.st_dev))
    isolation.emit(run / "historical-metadata.json", entries)
    authorities = ["scripts/verify-beshstatng.ps1", "scripts/verify-04a02-fixtures.py",
                   "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
                   "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json",
                   "BESHstatNG/Generated/VersionAttributes.vb"]
    isolation.emit(run / "start.json", dict(source_pins={p: isolation.sha(ROOT / p) for p in authorities},
                   historical_files=len(entries), disk_free_bytes=shutil.disk_usage(ROOT).free,
                   starting_head="9e2326d4b562cc94660745f1453b231df938bff0"))
    # Git's preimage, not the modified working source, is the characterization authority.
    before = subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), "show",
                                      "main:BESHstatNG/src/UI/UibyID.vb"], cwd=ROOT)
    (run / "UibyID-before.vb").write_bytes(before)
    print(run.relative_to(ROOT), len(entries), flush=True)
    return 0


def tools():
    vswhere = Path(os.environ["ProgramFiles(x86)"]) / "Microsoft Visual Studio/Installer/vswhere.exe"
    common = [str(vswhere), "-version", "[18.0,19.0)", "-products", "*"]
    msbuild = subprocess.check_output(common + ["-requires", "Microsoft.Component.MSBuild", "-find",
                                              r"MSBuild\Current\Bin\MSBuild.exe"], text=True).splitlines()[0]
    vs = subprocess.check_output(common + ["-property", "installationPath"], text=True).splitlines()[0]
    return msbuild, str(Path(vs) / "Common7/IDE/Extensions/TestPlatform/vstest.console.exe")


def windows_build(run, config, name):
    msbuild, _ = tools()
    for suffix, project, target in [
        ("host", "BESHstatNG/BESHStatNG.vbproj", "/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory"),
        ("tests", "BESHStatNG.Test/BESHStatNG.Tests.vbproj", "/t:Build")]:
        code = command(run, name + "-" + suffix, [msbuild, project, target, "/p:Configuration=" + config,
                       "/p:BuildProjectReferences=false", "/p:PythonExe=" + sys.executable, "/m", "/v:minimal"])
        if code:
            return code
    return 0


def focused(args):
    run = evidence(args.evidence)
    code = command(run, args.name + "-build", ["dotnet", "build", "BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj",
                   "-c", args.configuration, "--no-restore"])
    if code:
        return code
    args.action = "common"
    args.filter = "FullyQualifiedName~UnivariateSelection|FullyQualifiedName~SpreadsheetGroupedValidation"
    code = isolation.common(args)
    if code or not args.windows:
        return code
    code = windows_build(run, args.configuration, args.name)
    if code:
        return code
    args.action = "legacy"
    args.name += "-windows"
    args.vstest = tools()[1]
    args.filter = "FullyQualifiedName~UnivariateSelection|FullyQualifiedName~MigrationRefEditBoundary09"
    return isolation.common(args)


def final(args):
    run = evidence(args.evidence)
    original = (ROOT / "scripts/verify-beshstatng.ps1").read_text(encoding="utf-8")
    anchors = isolation.SCRIPT_ANCHORS
    for anchor in anchors.values():
        if original.splitlines().count(anchor) != 1:
            raise ValueError("Verifier anchor drift")
    relative = run.relative_to(ROOT).as_posix()
    helper = "Invoke-Checked $PythonExe @('-B', 'scripts/verify-04a03.py', 'execute', '--evidence', '" + relative + "', '--configuration', $Configuration, '--name', '" + args.name
    replacements = {
        "root": "$root = (Get-Location).Path",
        "results": "$results = Join-Path $root '" + relative + "/" + args.configuration + "/results'",
        "python": "    $resolvedPython = & (Join-Path $root 'scripts/resolve-beshstatng-python.ps1') -PythonExe $PythonExe",
        "common": "Invoke-Checked 'dotnet' @('build', '.\\BESHStat.Excel.Common.Tests\\BESHStat.Excel.Common.Tests.vbproj', '-c', $Configuration, '--no-restore')\n" + helper + "-Common')",
        "legacy-build": "    " + helper + "-build', '--windows')",
        "legacy-test": "    " + helper + "-Legacy', '--legacy', '--vstest', $vstest)",
    }
    for key in ("core", "office-test", "smoke-test"):
        replacements[key] = anchors[key].replace("'--logger'", "'--no-restore', '--logger'")
    for key in ("office-build", "smoke-build"):
        replacements[key] = anchors[key][:-1] + ", '--no-restore')"
    transformed = original
    for key, replacement in replacements.items():
        transformed = transformed.replace(anchors[key], replacement)
    # The replacement performs both managed legacy builds; do not leave the
    # original generator/packaging Build invocation after it.
    for line in ['    if ($PythonExe) { $legacyArguments += "/p:PythonExe=$PythonExe" }',
                 '    Invoke-Checked $msbuild $legacyArguments']:
        if transformed.splitlines().count(line) != 1:
            raise ValueError("Legacy invocation anchor drift")
        transformed = transformed.replace(line + "\n", "")
    # Continue independent suites after known Python compatibility nonpasses.
    failure = '    if ($LASTEXITCODE -ne 0) { throw "Command failed with exit code ${LASTEXITCODE}: $Exe" }'
    if transformed.count(failure) != 1:
        raise ValueError("Verifier failure-policy drift")
    transformed = transformed.replace(failure, '    if ($LASTEXITCODE -ne 0) { $script:failedCommands += "$Exe $($Arguments -join \' \')" }')
    transformed = transformed.replace("$ErrorActionPreference = 'Stop'", "$ErrorActionPreference = 'Stop'\n$script:failedCommands = @()")
    transformed += '\nif ($script:failedCommands.Count) { $script:failedCommands | Write-Host; exit 1 }\n'
    target = run / (args.name + "-verify-isolated.ps1")
    with target.open("x", encoding="utf-8", newline="\n") as stream:
        stream.write(transformed)
    isolation.emit(target.with_suffix(".provenance.json"), dict(source_sha256=isolation.sha(ROOT / "scripts/verify-beshstatng.ps1"),
                   execution_sha256=isolation.sha(target), diff="".join(difflib.unified_diff(original.splitlines(True), transformed.splitlines(True)))))
    code = command(run, args.name + "-full", ["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(target),
                   "-Legacy", "-Wasm", "-Js", "-Python", "-VisualStudio", "2026", "-PythonExe", sys.executable,
                   "-Configuration", args.configuration])
    # This additional source suite is not included by the standard verifier.
    semantic_code = semantic(args)
    extra = common_python(args)
    return max(code, extra, semantic_code)


def semantic(args):
    capture = argparse.Namespace(**vars(args))
    capture.action = "common"
    capture.name += "-semantic"
    capture.filter = "FullyQualifiedName~MigrationSemantic04A02Tests"
    return isolation.common(capture)


def common_python(args):
    run = evidence(args.evidence)
    env = os.environ.copy()
    candidate_name = args.candidate_name or args.name
    env["BESHSTAT_04A02_G3_CANDIDATES"] = str(run / "executions" / (candidate_name + "-semantic") / "observations")
    return command(run, args.name + "-common-python", [sys.executable, "-B", "-m", "unittest", "discover",
                   "-s", "BESHStat.Excel.Common.Tests/src", "-p", "test_*.py", "-v"], env)


def execute(args):
    if args.windows:
        return windows_build(evidence(args.evidence), args.configuration, args.name)
    args.action = "legacy" if args.legacy else "common"
    args.filter = None
    return isolation.common(args)


def crosswalk(args):
    run = evidence(args.evidence)
    proposal_path = ROOT / "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json"
    if isolation.sha(proposal_path) != "8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307":
        raise ValueError("Accepted amendment drift")
    proposal = json.loads(proposal_path.read_bytes())
    cases = [c for c in proposal["case_transfers"] if c["case_id"] in
             ("04A02-ANOVA-NATIVE-01", "04A02-NORMALITY-NATIVE-01")]
    touched = {"control:BESHStatNG.UibyID." + suffix for suffix in
               ("btCompute", "RefEdit1", "RefEdit2", "optByID", "optByColumn")}
    obligations = [row for row in proposal["obligation_transfers"] if row["obligation_id"] in touched]
    ledger_path = ROOT / "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"
    ledger = {row["obligation_id"]: row for row in json.loads(ledger_path.read_bytes())}
    csv_path = ROOT / "docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv"
    with csv_path.open(encoding="utf-8-sig", newline="") as stream:
        source_rows = {row["control_id"]: row for row in csv.DictReader(stream)
                       if "control:" + row["control_id"] in touched}
    records = []
    for obligation in obligations:
        original = source_rows[obligation["original_control_id"]]
        digest = hashlib.sha256(json.dumps(original, sort_keys=True, ensure_ascii=False).encode("utf-8")).hexdigest()
        if (digest != obligation["source_record"]["row_sha256"] or
                digest != ledger[obligation["obligation_id"]]["source_record_sha256"] or
                isolation.sha(ledger_path) != obligation["ledger"]["sha256"] or
                isolation.sha(csv_path) != obligation["source_record"]["file_sha256"]):
            raise ValueError("Historical obligation/source row mismatch: " + obligation["obligation_id"])
        records.append(dict(obligation_id=obligation["obligation_id"], case_ids=obligation["case_ids"],
                            declaration_id=original["source_pin"], source_row_sha256=digest,
                            source_record=obligation["source_record"], risk_tier=original["risk_tier"],
                            risk_basis=obligation["original_risk_basis"],
                            mitigation_requirements=obligation["mitigation_requirements"],
                            pre_change_gate=obligation["pre_change_gate"],
                            capture_authorized=obligation["capture_authorized"],
                            closure_status=obligation["closure_status"],
                            disposition="Behavior-preserving managed extraction only; native behavior/lifetime and gates retained; no closure credit"))
    if len(records) != 5:
        raise ValueError("Incomplete exact-ID crosswalk")
    result = dict(schema="04A03-exact-pre-change-crosswalk-v1", starting_head="9e2326d4b562cc94660745f1453b231df938bff0",
                  accepted_amendment_sha256=isolation.sha(proposal_path), ledger_sha256=isolation.sha(ledger_path),
                  shared_controls=len(set(cases[0]["blocked_control_ids"])),
                  shared_obligations=len(set(cases[0]["deferred_obligation_ids"])),
                  cases=[{k: c[k] for k in ("case_id", "primary_integration_owner", "earliest_actual_consuming_batch")} for c in cases],
                  touched=records, N1="NO_GO", N2="NO", P="NO", Gate_C2="UNMET")
    isolation.emit(ROOT / "docs/architecture/04A03/pre-change-crosswalk.json", result)
    isolation.emit(run / "crosswalk-verification.json", dict(status="PASS", exact_source_and_ledger_rows_verified=len(records),
                   result_sha256=isolation.sha(ROOT / "docs/architecture/04A03/pre-change-crosswalk.json")))
    print("Exact source/ledger crosswalk PASS", len(records), flush=True)
    return 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["init", "focused", "final", "execute", "preserve", "summarize", "crosswalk", "common-python", "semantic"])
    parser.add_argument("--evidence", required=True)
    parser.add_argument("--configuration", choices=["Debug", "Release"], default="Debug")
    parser.add_argument("--name", default="focused-01")
    parser.add_argument("--windows", action="store_true")
    parser.add_argument("--legacy", action="store_true")
    parser.add_argument("--vstest")
    parser.add_argument("--candidate-name")
    args = parser.parse_args()
    handlers = dict(init=init, focused=focused, final=final, execute=execute,
                    preserve=isolation.preservation, summarize=isolation.summarize, crosswalk=crosswalk, semantic=semantic, **{"common-python": common_python})
    raise SystemExit(handlers[args.action](args))
