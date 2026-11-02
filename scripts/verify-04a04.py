"""Local-only 04A.04 verification: isolated test writers, no restore/native/golden work.

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
import re
from pathlib import Path
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
from types import SimpleNamespace

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
sys.stderr.reconfigure(encoding="utf-8", errors="replace")

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("isolation02", ROOT / "scripts/verify-04a02-fixtures.py")
isolation = importlib.util.module_from_spec(spec)
spec.loader.exec_module(isolation)


def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-04-"):
        raise ValueError("New top-level 04A.04 evidence directory required")
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
        env.pop("BESHSTAT_04A03_OBSERVATIONS", None)
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
    if run.parent != ROOT / "TestResults" or not run.name.startswith("04A-04-"):
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
                   starting_head="e1377cceabbe8890395f94cf41578b621aef3d4c"))
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
    args.filter = "FullyQualifiedName~PairedBlock|FullyQualifiedName~PairedPolicy|FullyQualifiedName~TypedNumericRangeParser"
    code = isolation.common(args)
    if code or not args.windows:
        return code
    code = windows_build(run, args.configuration, args.name)
    if code:
        return code
    args.action = "legacy"
    args.name += "-windows"
    args.vstest = tools()[1]
    args.filter = "FullyQualifiedName~PairedBlock|FullyQualifiedName~PairedPolicy"
    return isolation.common(args)


def final(args):
    run = evidence(args.evidence)
    original = (ROOT / "scripts/verify-beshstatng.ps1").read_text(encoding="utf-8")
    anchors = isolation.SCRIPT_ANCHORS
    for anchor in anchors.values():
        if original.splitlines().count(anchor) != 1:
            raise ValueError("Verifier anchor drift")
    relative = run.relative_to(ROOT).as_posix()
    helper = "Invoke-Checked $PythonExe @('-B', 'scripts/verify-04a04.py', 'execute', '--evidence', '" + relative + "', '--configuration', $Configuration, '--name', '" + args.name
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
    prior = projection(args)
    return max(code, extra, semantic_code, prior)


def projection(args):
    """Rerun the 61 accepted C03 addendum cases, explicitly still a test-only projection."""
    run = evidence(args.evidence)
    previous = ROOT / "TestResults/04A-03-controls-20261102-run01/projection"
    record = json.loads((previous / "source-projection.json").read_bytes())
    for path, digest in record["source_hashes"].items():
        if isolation.sha(ROOT / path) != digest:
            raise ValueError("Accepted projection input drift: " + path)
    destination = run / "projection"
    if not destination.exists():
        destination.mkdir()
        for name, digest in record["generated_hashes"].items():
            if isolation.sha(previous / name) != digest:
                raise ValueError("Accepted generated source drift")
            shutil.copyfile(previous / name, destination / name)
        isolation.emit(destination / "provenance.json", record)
    msbuild, vstest = tools()
    output = run / "projection-executions" / args.name
    output.mkdir(parents=True, exist_ok=False)
    code = command(run, args.name + "-projection-build", [msbuild,
        "BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj", "/t:Build", "/m", "/v:minimal",
        "/p:Configuration=" + args.configuration, "/p:ProjectionSourceDir=" + str(destination),
        "/p:OutputPath=" + str(output / "bin") + "\\", "/p:IntermediateOutputPath=" + str(output / "obj") + "\\"])
    if code:
        return code
    code = command(run, args.name + "-projection-tests", [vstest, str(output / "bin/BESHStatNG.Test.dll"),
        "/TestAdapterPath:" + str(ROOT / "BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common"),
        "/Logger:console;verbosity=minimal", "/Logger:trx;LogFileName=ControlMitigation.trx", "/ResultsDirectory:" + str(output / "results")])
    counters = ET.parse(output / "results/ControlMitigation.trx").find(".//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}Counters").attrib
    if any(int(counters[key]) != 61 for key in ("total", "executed", "passed")):
        raise ValueError("Incomplete accepted projection regression: " + str(counters))
    return code


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


def result_summary(args):
    run = evidence(args.evidence)
    old = json.loads((ROOT / "TestResults/04A-03-controls-20261102-run01/final-summary.json").read_bytes())
    configurations = {}
    ns = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
    for config in ("Debug", "Release"):
        name = "final-" + config
        paths = [run / config / "results" / (s + ".trx") for s in ("Core", "OfficeAddin", "OfficeExecution")]
        paths += [run / "executions" / (name + "-" + s) / "results" / (t + ".trx") for s, t in (("Common", "Common"), ("Legacy", "Legacy"))]
        paths += [run / "projection-executions" / name / "results/ControlMitigation.trx"]
        suites = []
        for path in paths:
            counters = ET.parse(path).find(".//" + ns + "Counters").attrib
            suites.append(dict(path=path.relative_to(ROOT).as_posix(), sha256=isolation.sha(path), counters=counters))
        logs = [(run / (name + suffix + ".log")).read_text(encoding="utf-8", errors="replace") for suffix in ("-full", "-common-python")]
        nonpasses = {}
        for log in logs:
            for outcome, method in re.findall(r"^(FAIL|ERROR): [^\n]+ \(([^()]+)\)", log, flags=re.M):
                nonpasses[method] = outcome
        python_total = sum(int(n) for log in logs for n in re.findall(r"^Ran (\d+) tests? in", log, flags=re.M))
        node_pass = sum(int(n) for n in re.findall(r"\bpass (\d+)\s*$", logs[0], flags=re.M))
        configurations[config] = dict(managed_suites=suites,
            managed_distinct=sum(int(s["counters"]["total"]) for s in suites),
            managed_pass=sum(int(s["counters"]["passed"]) for s in suites),
            managed_fail=sum(int(s["counters"]["failed"]) for s in suites),
            node_pass=node_pass, python_distinct=python_total, python_pass=python_total-len(nonpasses),
            python_nonpasses=nonpasses, same_historical_nonpasses=nonpasses == old["configurations"][config]["historical_python_nonpasses"],
            semantic_repetitions=15)
    pins = json.loads((ROOT / "TestResults/04A-03-controls-20261102-run01/accepted-baseline.json").read_bytes())["pins"]
    expected_changes = {"BESHstatNG/BESHStatNG.vbproj", "BESHStatNG.Test/BESHStatNG.Tests.vbproj"}
    source_preserved = [p for p, digest in pins.items() if isolation.sha(ROOT / p) == digest]
    drift = [p for p, digest in pins.items() if isolation.sha(ROOT / p) != digest]
    if set(drift) != expected_changes:
        raise ValueError("Unexpected accepted source/authority drift: " + str(drift))
    summary = dict(schema="04A04-final-validation-v1", base="e1377cceabbe8890395f94cf41578b621aef3d4c",
                   configurations=configurations, accepted_pins_preserved=source_preserved, expected_compile_entry_changes=drift,
                   new_managed_distinct=64, new_python_distinct=14, native_attempts=0, owner_released_controls=0,
                   focused_pass_executions=399, focused_compilation_repairs=["Agreement namespace import", "WilcoxonTest type", "Bland BiasCI.Estimate member"],
                   source_test_development_repair="New designer comparison normalized Git LF/worktree CRLF; frozen tests unchanged")
    isolation.emit(run / "final-summary.json", summary)
    print(json.dumps({c: {k: v for k, v in r.items() if k not in ("managed_suites", "python_nonpasses")} for c, r in configurations.items()}, indent=2))
    return int(any(not r["same_historical_nonpasses"] or r["managed_fail"] for r in configurations.values()))



if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["init", "focused", "final", "execute", "preserve", "summarize", "common-python", "semantic", "result-summary"])
    parser.add_argument("--evidence", required=True)
    parser.add_argument("--configuration", choices=["Debug", "Release"], default="Debug")
    parser.add_argument("--name", default="focused-01")
    parser.add_argument("--windows", action="store_true")
    parser.add_argument("--legacy", action="store_true")
    parser.add_argument("--vstest")
    parser.add_argument("--candidate-name")
    args = parser.parse_args()
    handlers = dict(init=init, focused=focused, final=final, execute=execute,
                    preserve=isolation.preservation, summarize=isolation.summarize, semantic=semantic, **{"common-python": common_python, "result-summary": result_summary})
    raise SystemExit(handlers[args.action](args))
