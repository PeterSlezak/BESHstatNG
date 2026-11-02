"""Versioned owner-disposition/source successor. Never amend a historical C05 matrix or release native IDs."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/"TestResults/04A-05-completion-20261102-run01"
DEST=ROOT/"docs/architecture/04A05/owner-completion-dispositions-v1.json"
PRIOR="docs/architecture/04A05/source-compatibility-restrictions.json"
PATCH="docs/architecture/04A05/ui82x2-wiring-PROPOSED.patch"
PRODUCTION=["BESHstatNG/src/UI/Ui82x2.vb","BESHStat.Excel.Common/src/Input/ContingencyInputPolicy.vb",
    "BESHStat.Excel.Common/src/Results/ProportionsReportPolicy.vb","BESHstatNG/src/DM/WindowsContingencyInputAdapter.vb",
    "BESHStat.Core/src/StatTests/ContingencyTable.vb"]
OWNER_QUOTE="""I approve 04A.05's completed implementation as partially qualified.

I authorize applying the reviewed `Ui82x2` patch, with a narrowly scoped exception to the missing pre-change native capture for `04A02-2X2-NATIVE-01`. All three obligations remain OPEN.

I approve mutually exclusive paired-response counts as the authoritative scientific convention and authorize correction of the McNemar mapping, zero-discordance handling and report labels. The paired Core interval-overflow issue must receive separate numerical verification.

All historical, P/N and Gate C2 restrictions remain unchanged. Codex may complete these corrections and ordinary regression tests within 04A.05, returning one consolidated final owner-review dossier. No Git publication, native Excel execution or start of 04A.27 is authorized."""


def sha(path):
    return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git","-c","safe.directory="+ROOT.as_posix(),*args],cwd=ROOT)


def member(source,name):
    m=re.search(r"(?ms)^\s*(?:Public|Private|Friend) (?:Shared )?(?:Function|Sub) "+re.escape(name)+r"\([^\n]*[\s\S]*?^\s*End (?:Function|Sub)",source)
    if not m:raise ValueError("Member not found "+name)
    return m.group(0).strip()


def build():
    initial=json.loads((RUN/"completion-baseline.json").read_bytes())
    prior=json.loads((ROOT/PRIOR).read_bytes())
    rows=prior["native_case"]["obligations"]
    patch_sha=sha(PATCH)
    assert patch_sha=="95328f1417731c433ef39fe1e6a6777e637d2ecf822a9541ffed4779e59e52f4"
    assert initial["source_sha256"][PRODUCTION[0]]=="8df9f73fd01e5677f671b8f30ff423d6d7f038a17361bc2c1c4c9f9c9ff41d85"
    # The unmodified reviewed patch independently reconstructs the intended form text.
    old=(RUN/"preimages"/PRODUCTION[0]).read_text(encoding="utf-8-sig")
    current=(ROOT/PRODUCTION[0]).read_text(encoding="utf-8-sig")
    for name in ("GetResultWriter","optWorksheetData_Click","optScreenData_Click","optOutputRange_Click","optWorksheet_Click","optWorkbook_Click"):
        assert member(old,name)==member(current,name),"Unapproved form member delta "+name
    before=(RUN/"preimages"/PRODUCTION[4]).read_text(encoding="utf-8-sig")
    after=(ROOT/PRODUCTION[4]).read_text(encoding="utf-8-sig")
    replaced=after
    for name in ("PairedProportions","Liddell_McNemar"):
        replaced=replaced.replace(member(after,name),member(before,name))
    assert replaced==before,"Core changes extend beyond the two approved methods"
    frozen={p:digest for p,digest in initial["source_sha256"].items() if p.startswith("scripts/") and p!="scripts/verify-04a05-completion.py" or p.startswith("BESHStat.Excel.Common.Tests/src/") or p=="BESHStatNG.Test/src/ContingencyAdapterTests.vb" or p.startswith("docs/")}
    assert all(sha(p)==digest for p,digest in frozen.items()),"Frozen source/authority changed"
    return dict(schema="04A05-owner-bounded-completion-v1",owner_authority=dict(source="Current explicit project-owner completion instruction, 2026-11-02; no external download or cryptographic signature asserted",quote=OWNER_QUOTE,
        final_batch_acceptance=False,independent_scientific_reviewers=0),
        git=dict(branch="codex/04A-05",head=initial["head"],main=initial["main"],publication_authorized=False),
        native_timing_exception=dict(case_id="04A02-2X2-NATIVE-01",criterion="Apply reviewed strict-count/Common report wiring only; timing of missing pre-change native capture deferred",
            source_preimage_sha256=initial["source_sha256"][PRODUCTION[0]],reviewed_patch=PATCH,reviewed_patch_sha256=patch_sha,
            amended_04A02_sha256=prior["authority_sha256"]["docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json"],
            obligations=[dict(obligation_id=r["obligation_id"],risk_tiers=r["risk_tiers"],blocked_control=r["blocked_control"],
                original_row_sha256=r["original_row_sha256"],source_record_sha256=r["source_record_sha256"],disposition="OPEN",owner_released=False,native_executed=False) for r in rows],
            native_execution_authorized=False,release_authorized=False),
        science=dict(counts="N=total,b=first-only,c=second-only,a=both,d=N-a-b-c; validated nonnegative integral complete accounting",
            natural_layout=[["a","b"],["c","d"]],core_liddell_layout=[["d","b"],["c","a"]],
            canonical=dict(n=100,b=20,c=15,a=40,d=25,responders_first=60,responders_second=55,difference=0.05,exact_p="0.4995598332025110721588134765625"),
            liddell_defects="Zero discordants p=1 with undefined odds/limits; equal positive discordants p=1; reciprocal F CDF replaces cancellation-prone 1-CDF; one-zero odds and upper bound are positive infinity.",
            paired_ci_overflow="Exactly two product assignments widen before multiplication; existing formula, continuity rule, critical-value method and interpretation retained.",
            report="Nine rows retained; total responders and proportions honestly labeled; p-value body cell (9,2) unchanged.",
            candidate_reference= "docs/architecture/04A05/paired-candidate-reference-v1.json",candidate_reference_sha256=sha("docs/architecture/04A05/paired-candidate-reference-v1.json"),
            reference_generator_sha256=sha("scripts/reference-04a05-completion.py"),golden_promoted=False,
            separate_open_label_finding="2x2 Liddell row still says Risk ratio for matched-pairs odds; no unrelated label change authorized."),
        production={p:dict(before_sha256=initial["source_sha256"][p],after_sha256=sha(p)) for p in PRODUCTION},frozen_source_pins=frozen,
        retained_populations=dict(shared_controls=141,shared_obligations=160,transferred_controls=218,transfer_obligations=239,wider_controls=802,inherited_obligations=1514),
        restrictions=dict(P="NO",N1="NO GO",N2="NO",Gate_C2="UNMET",released_controls=0,discharged_obligations=0),
        unrelated_external_edit=dict(path="planning/BESHStatNG_CrossPlatform_MasterPlan.md",observed_revision="2.4",observed_sha256=sha("planning/BESHStatNG_CrossPlatform_MasterPlan.md"),
            note="Not edited by this completion; appeared after initial baseline. Compatible owner-progress reconciliation, preserved separately."))


def check():
    data=json.loads(DEST.read_bytes())
    assert data==build(),"Completion successor drift"
    assert not git("diff","--cached","--name-only").strip(),"Staging unauthorized"
    source=(ROOT/PRODUCTION[0]).read_text(encoding="utf-8-sig")
    compute=member(source,"btCompute_Click")
    assert "ContingencyReportPolicy.TwoByTwo" in compute and "WindowsContingencyInputAdapter.AnalyzeDialog" in compute
    assert compute.index("If Not report.Success Then")<compute.index("GetResultWriter()")
    assert "contingencytable." not in compute and "Array2intArray" not in compute
    for p in PRODUCTION[1:3]:
        code=(ROOT/p).read_text(encoding="utf-8-sig")
        assert "Option Strict On" in code
        assert not re.search(r"Microsoft\.(Office|VisualBasic)|ExcelDna|System\.Windows\.Forms|AppGlobals|System\.IO",code)
    adapter=(ROOT/PRODUCTION[3]).read_text(encoding="utf-8-sig")
    assert "LegacyPairedEvidence" not in adapter and "Responders1 - request.Both" not in adapter
    assert "ProportionsReportPolicy.Analyze(input.Data)" in adapter
    assert len(data["native_timing_exception"]["obligations"])==3
    assert sum(r["blocked_control"] for r in data["native_timing_exception"]["obligations"])==2
    print("Owner-bound completion successor PASS; three obligations OPEN; no native/P/N release")


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action",choices=["create","check"])
    args=parser.parse_args()
    if args.action=="create":
        with DEST.open("x",encoding="utf-8",newline="\n") as stream:
            json.dump(build(),stream,indent=2,ensure_ascii=False)
            stream.write("\n")
    else:check()
