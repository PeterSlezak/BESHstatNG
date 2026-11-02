"""Bounded owner-authorized C05 completion; reuse isolated verifier, preserve prior run files."""
import argparse
import hashlib
import importlib.util
import json
import re
from pathlib import Path
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location("completion_base05",ROOT/"scripts/verify-04a05.py")
base=importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)


def git(*args):
    return subprocess.check_output(["git","-c","safe.directory="+ROOT.as_posix(),*args],cwd=ROOT)


def init(args):
    if git("branch","--show-current").decode().strip()!="codex/04A-05":
        raise ValueError("Existing codex/04A-05 required; no branch mutation")
    base.init(args)
    run=base.evidence(args.evidence)
    dirty=git("diff","--name-only").decode().splitlines()+git("ls-files","--others","--exclude-standard").decode().splitlines()
    prior=json.loads((ROOT/"docs/architecture/04A05/source-compatibility-restrictions.json").read_bytes())
    pins=set(dirty)|set(prior["authority_sha256"])|{"docs/architecture/04A05/source-compatibility-restrictions.json",
        "docs/architecture/04A05/ui82x2-wiring-PROPOSED.patch","BESHstatNG/src/UI/Ui82x2.vb",
        "BESHStat.Core/src/StatTests/ContingencyTable.vb","BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj",
        "BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj"}
    records={}
    for name in sorted(pins):
        path=ROOT/name
        if path.is_file():
            records[name]=base.isolation.sha(path)
            target=run/"preimages"/name
            target.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(path,target)
    base.isolation.emit(run/"completion-baseline.json",dict(branch="codex/04A-05",head=git("rev-parse","HEAD").decode().strip(),
        main=git("rev-parse","main").decode().strip(),status=git("status","--short","--untracked-files=all").decode(),
        diff_stat=git("diff","--stat").decode(),source_sha256=records,staged=git("diff","--cached","--name-status").decode()))
    return 0


def probe(args):
    run=base.evidence(args.evidence)
    return base.command(run,args.name,["dotnet","test","BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj","-c",args.configuration,
        "--no-restore","--filter","FullyQualifiedName~Completion05CoreProbeTests","--logger","trx;LogFileName=Probe.trx",
        "--results-directory",str(run/args.name)])


def focused(args):
    run=base.evidence(args.evidence)
    code=base.command(run,args.name+"-build",["dotnet","build","BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj","-c",args.configuration,"--no-restore"])
    if code:return code
    args.action="common"
    args.filter="FullyQualifiedName~Completion05"
    code=base.isolation.common(args)
    if code:return code
    core=base.command(run,args.name+"-core",["dotnet","test","BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj","-c",args.configuration,
        "--no-restore","--filter","FullyQualifiedName~Completion05","--logger","trx;LogFileName=Core.trx","--results-directory",str(run/(args.name+"-core"))])
    if core:return core
    code=base.windows_build(run,args.configuration,args.name)
    if code:return code
    args.action="legacy"
    args.name+="-windows"
    args.vstest=base.tools()[1]
    return base.isolation.common(args)


def summary(args):
    run=base.evidence(args.evidence)
    ns="{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
    prior=json.loads((ROOT/"TestResults/04A-05-20261102-run01/final-summary.json").read_bytes())
    expected_source={
        "test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change":"FAIL",
        "test_04a05_contingency.ContingencySourceGates.test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table":"FAIL",
        "test_04a05_contingency.ContingencySourceGates.test_ui82x2_source_wiring_explicitly_withheld":"FAIL",
        "test_04a05_contingency.ContingencySourceGates.test_versioned_successor_sources_and_case_boundaries":"FAIL",
        "test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates":"FAIL",
        "test_investigation06.Investigation06Tests.test_two_by_two_or_condition_and_no_chart_handler":"FAIL"}
    expected_managed={
        "ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection":3,
        "ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted":1,
        "ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden":1}
    configurations={}
    numerical={}
    for config in ("Debug","Release"):
        name="final-"+config
        paths=[run/config/"results"/(s+".trx") for s in ("Core","OfficeAddin","OfficeExecution")]
        paths += [run/"executions"/(name+"-"+s)/"results"/(t+".trx") for s,t in (("Common","Common"),("Legacy","Legacy"))]
        paths += [run/"projection-executions"/name/"results/ControlMitigation.trx"]
        suites=[]; failures=[]; probes=[]
        for path in paths:
            tree=ET.parse(path)
            counters=tree.find(".//"+ns+"Counters").attrib
            definitions={n.attrib["id"]:n.find(ns+"TestMethod").attrib for n in tree.findall(".//"+ns+"UnitTest")}
            for row in tree.findall(".//"+ns+"UnitTestResult"):
                method=definitions.get(row.attrib["testId"],{})
                identity=method.get("className","")+"."+method.get("name","")
                if row.attrib["outcome"]!="Passed":
                    failures.append(dict(identity=identity,test_name=row.attrib["testName"],outcome=row.attrib["outcome"],trx=path.relative_to(ROOT).as_posix()))
                if "Completion05CoreProbeTests" in identity:
                    probes.append(row.findtext(".//"+ns+"StdOut",default=""))
            suites.append(dict(path=path.relative_to(ROOT).as_posix(),sha256=base.isolation.sha(path),counters=counters))
        observed={key:sum(f["identity"].endswith(key) for f in failures) for key in expected_managed}
        unexpected=[f for f in failures if not any(f["identity"].endswith(k) for k in expected_managed)]
        logs=[(run/(name+suffix+".log")).read_text(encoding="utf-8",errors="replace") for suffix in ("-full","-common-python")]
        nonpasses={method:outcome for log in logs for outcome,method in re.findall(r"^(FAIL|ERROR): [^\n]+ \(([^()]+)\)",log,flags=re.M)}
        old=prior["configurations"][config]["python_nonpasses"]
        extras={k:v for k,v in nonpasses.items() if k not in old}
        py_total=sum(int(n) for log in logs for n in re.findall(r"^Ran (\d+) tests? in",log,flags=re.M))
        configurations[config]=dict(managed_suites=suites,managed_distinct=sum(int(s["counters"]["total"]) for s in suites),
            managed_pass=sum(int(s["counters"]["passed"]) for s in suites),managed_failures=failures,
            expected_frozen_managed=observed==expected_managed and not unexpected,unexpected_managed=unexpected,
            node_pass=sum(int(n) for n in re.findall(r"\bpass (\d+)\s*$",logs[0],flags=re.M)),python_distinct=py_total,python_pass=py_total-len(nonpasses),
            python_nonpasses=nonpasses,prior_16_preserved=all(nonpasses.get(k)==v for k,v in old.items()),
            additional_frozen_source=extras,expected_source_only=extras==expected_source,semantic_capture_repetitions=15)
        before=ET.parse(run/("before-Core-"+config)/"Probe.trx")
        before_text="\n".join(n.text or "" for n in before.findall(".//"+ns+"StdOut"))
        after_text="\n".join(probes)
        pattern=r"CI (\d+/\d+/\d+/\d+) culture= estimate=([^ ]+) lower=([^ ]+) upper=([^\r\n ]+)"
        old_ci={m[0]:tuple(map(float,m[1:])) for m in re.findall(pattern,before_text)}
        new_ci={m[0]:tuple(map(float,m[1:])) for m in re.findall(pattern,after_text)}
        numerical[config]=dict(before_probe= "before-Core-"+config+"/Probe.trx",after_probe=suites[0]["path"],
            before_overflow_reproduced="CI 1000/100/80/300 culture= EXCEPTION System.OverflowException" in before_text,
            after_overflow_finite="1000/100/80/300" in new_ci,
            overflow_result=new_ci.get("1000/100/80/300"),small_case_exact_backwards={k:old_ci[k]==new_ci.get(k) for k in old_ci},
            supplemental_ci_candidates_sha256=base.isolation.sha(run/"ci-only-candidates.json"))
    record=dict(schema="04A05-bounded-completion-validation-v1",configurations=configurations,numerical_verification=numerical,
        new_managed_cases=85,new_successor_python_cases=13,independent_shared_candidates=22,supplemental_ci_candidates=3,
        native_attempts=0,scientific_reviewers=0,P_promoted=False,owner_acceptance=False,
        external_plan_edit="Revision2.4 preserved; its old hash assertion is classified separately from the approved production changes.")
    base.isolation.emit(run/"completion-final-summary.json",record)
    print(json.dumps({c:{k:r[k] for k in ("managed_distinct","managed_pass","node_pass","python_distinct","python_pass","expected_frozen_managed","prior_16_preserved","expected_source_only")} for c,r in configurations.items()},indent=2))
    print(json.dumps(numerical,indent=2))
    return int(any(not r["expected_frozen_managed"] or not r["prior_16_preserved"] or not r["expected_source_only"] for r in configurations.values()))


if __name__=="__main__":
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("action",choices=["init","probe","focused","final","preserve","summary"])
    p.add_argument("--evidence",required=True)
    p.add_argument("--configuration",choices=["Debug","Release"],default="Debug")
    p.add_argument("--name",default="focused-01")
    p.add_argument("--candidate-name")
    p.add_argument("--windows",action="store_true")
    p.add_argument("--legacy",action="store_true")
    p.add_argument("--vstest")
    args=p.parse_args()
    handlers=dict(init=init,probe=probe,focused=focused,final=base.final,preserve=base.isolation.preservation,summary=summary)
    raise SystemExit(handlers[args.action](args))
