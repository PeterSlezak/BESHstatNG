"""Additive 04A.05 source, semantics and exact native restriction ledger. No native execution."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
BASE = "849025214223b58a500cd7cceed237a666868ce8"
RUN = ROOT / "TestResults/04A-05-20261102-run01"
DEST = ROOT / "docs/architecture/04A05/source-compatibility-restrictions.json"
AMENDMENT = "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json"
LEDGER = "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"
PRODUCTION = ["BESHStat.Excel.Common/src/Input/ContingencyInputPolicy.vb",
    "BESHStat.Excel.Common/src/Results/ContingencyReportPolicy.vb", "BESHStat.Excel.Common/src/Results/ProportionsReportPolicy.vb",
    "BESHstatNG/src/DM/WindowsContingencyInputAdapter.vb", "BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb", "BESHstatNG/src/UI/Ui8Proportions.vb"]
ORACLE = "BESHStat.Excel.Common.Tests/src/ContingencyLegacyCharacterization.vb"
AUTHORITY = [AMENDMENT, LEDGER, "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
    "docs/architecture/04A02/native-restriction-crosswalk-v1.json", "docs/architecture/04A02/04A02-C14-N1-risk-review.pending.json",
    "docs/architecture/04A03/pre-change-crosswalk.json", "docs/architecture/04A03/control-mitigation/successor-candidates.json",
    "docs/architecture/04A03/control-mitigation/native-readonly-plan.json", "docs/architecture/04A04/compatibility-crosswalk.json",
    "docs/architecture/batch-04A-03-owner-review.md", "docs/architecture/batch-04A-03-control-mitigation-owner-review.md",
    "docs/architecture/batch-04A-04-owner-review.md", "BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb",
    "BESHstatNG/src/DM/ExcelDnaDataImporter.vb", "BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb"]


def sha(path):
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT)


def members(source):
    return {m.group(2): m.group(0) for m in re.finditer(r"(?ms)^    Private (Sub|Function) (\w+)[\s\S]*?^    End \1", source)}


def counts():
    amendment=json.loads((ROOT/AMENDMENT).read_bytes())
    ledger=json.loads((ROOT/LEDGER).read_bytes())
    return dict(transferred_controls=len(set(amendment["blocked_control_ids"])), transferred_obligations=len(set(amendment["distinct_deferred_obligation_ids"])),
        broader_controls=len({r["original_id"] for r in ledger if r["id_kind"]=="control" and r["affected_live_golden_blocked"]}), original_obligations=len(ledger))


def build():
    amendment=json.loads((ROOT/AMENDMENT).read_bytes())
    ledger={r["obligation_id"]:r for r in json.loads((ROOT/LEDGER).read_bytes())}
    case=next(c for c in amendment["case_transfers"] if c["case_id"]=="04A02-2X2-NATIVE-01")
    obligations=[]
    for oid in case["deferred_obligation_ids"]:
        original=ledger[oid]
        obligations.append(dict(obligation_id=oid, blocked_control=original["original_id"] in case["blocked_control_ids"],
            disposition="OPEN",owner_released=False,native_executed=False,
            original_row_sha256=hashlib.sha256(json.dumps(original,sort_keys=True,ensure_ascii=False).encode()).hexdigest(),
            source_record_path=original["source_record_path"],source_record_sha256=original["source_record_sha256"],
            risk_tiers=original["risk_tiers"],owner_roles=original["owner_roles"],blocked_gate=original["blocked_gate"],
            safe_capture_preconditions=original["safe_capture_preconditions"],acceptance_condition=original["acceptance_condition"],
            original_codebehind_sha256=original["source_evidence"]["codebehind_source_sha256"],
            original_designer_sha256=original["source_evidence"]["designer_source_sha256"]))
    changed_members={}
    for form,allowed in {"Ui0OneRefeditMulticol":{"btCompute_Click","RunRxC"},"Ui8Proportions":{"btCompute_Click","ValidateIndependentMargin"}}.items():
        path=f"BESHstatNG/src/UI/{form}.vb"
        old=members(git("show",BASE+":"+path).decode("utf-8-sig"))
        new=members((ROOT/path).read_text(encoding="utf-8-sig"))
        changed={name for name in old.keys()|new.keys() if old.get(name)!=new.get(name)}
        assert changed==allowed,(form,changed)
        changed_members[form]={name:dict(before_body_sha256=hashlib.sha256(old[name].encode()).hexdigest(),after_body_sha256=hashlib.sha256(new[name].encode()).hexdigest()) for name in sorted(changed)}
    sources={p:dict(after_sha256=sha(p),before_git_blob_sha256=hashlib.sha256(git("show",BASE+":"+p)).hexdigest() if p in ("BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb","BESHstatNG/src/UI/Ui8Proportions.vb") else None) for p in PRODUCTION}
    return dict(schema="04A05-source-semantics-restrictions-v1",base=BASE,branch="codex/04A-05",populations=counts(),
        authority_sha256={p:sha(p) for p in AUTHORITY},source=sources,changed_members=changed_members,
        withheld_ui82x2_sha256=sha("BESHstatNG/src/UI/Ui82x2.vb"),characterization=dict(test_source=ORACLE,sha256=sha(ORACLE),
            source_preimages=json.loads((RUN/"characterization-provenance.json").read_bytes())["source_preimages"],
            qualification="Mechanically source-derived host-free compatibility oracle; Matrix transpose replaced by equivalent test-only loop after compile repair; no native constructor/handler executed; NOT_P_GOLDEN"),
        native_case=dict(case_id=case["case_id"],primary_owner=case["primary_integration_owner"],pre_change_gate=case["pre_change_gate"],
            obligations=obligations,N1="NO GO",N2="NO",P="NO",Gate_C2="UNMET",controls_released=0,obligations_discharged=0),
        contracts=[
            dict(id="count-policy",status="IMPLEMENTED_HOST_FREE_QUALIFIED",api="ContingencyInputPolicy.Parse/FromDialog; ParseContingencyCounts reused; checked Int32 cells+total; immutable ordered categories and cell sources"),
            dict(id="2x2-semantic-report",status="IMPLEMENTED_HOST_FREE_QUALIFIED_NATIVE_WIRING_WITHHELD",api="ContingencyReportPolicy.TwoByTwo; WindowsContingencyInputAdapter.AnalyzeDialog/ParseWorksheet",
                blocker="04A02-2X2-NATIVE-01: strict raw-count rejection changes prior filtered/CInt-rounded behavior. Ui82x2 code remains exactly unchanged; Common is not claimed reachable from this form."),
            dict(id="rxc",status="WIRED_HOST_FREE_QUALIFIED",call="Ui0OneRefeditMulticol.btCompute_Click -> RunRxC -> AnalyzeRxC -> Common report",
                invalid_input_delta="Raw missing/text/error/date/Boolean/fractional counts reject rather than DataObj silently dropping rows or CInt rounding. GUI explicit no-header/no-row-label; portable caller may supply label choices."),
            dict(id="single-independent-ni-equivalence",status="WIRED_HOST_FREE_QUALIFIED",call="Ui8Proportions.btCompute_Click -> adapter -> Common validated request/report",
                sign="Superiority control-minus-experimental as current Core/UI; NI/TOST experimental-minus-control, one-sided alpha, symmetric margins, CI 1-2alpha"),
            dict(id="paired",status="INPUT_AND_PROJECTION_WIRED_LEGACY_SCIENTIFIC_MAPPING_UNRESOLVED",call="Common disjoint validation -> Windows LegacyPairedEvidence unchanged Core invocation -> Common ProjectPaired",
                candidate="SC-PROPORTIONS-PAIRED-01; N100, firstOnly20, secondOnly15, both40. Proper disjoint table [[40,20],[15,25]], old Liddell input [[-20,40],[105,-25]]. Different p-values. No correction or new portable wrong-table builder."),
        ],
        candidates=[dict(id="SC-PROPORTIONS-PAIRED-01",owner="04A.05 owner/scientific reviewer",decision="Approve exclusive-category Liddell table and corrected category labels, or specify intended paired count meaning. Existing invocation retained as temporary explicit host compatibility exception, never an approved golden."),
            dict(id="SC-CORE-PAIRED-OVERFLOW-02",owner="Core contingency maintainer / scientific review",fixture="PairedProportions(1000,100,80,300) Integer intermediate overflow; portable counts valid, adapter returns structured failure with no tables; Core unchanged."),
            dict(id="CORE-EQUIVALENCE-DOMAIN",owner="Core equivalence maintainer",fixture="100/45 vs80/52, margin .15: observed difference .2 outside margins; existing Core rejects. Valid comparison fixture uses .3. No change to this Core precondition."),
        ],
        prior_04A04_byte_provenance=dict(frozen_assertion="test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities",
            untouched_published_sources=["BESHstatNG/src/UI/UiTwoInputRefedits.vb","BESHstatNG/src/UI/Ui9ANOVA2nested.vb"],
            note="Their current checkout bytes differ from the frozen prepublication after_sha256 despite no Git diff. Ui0OneRefeditMulticol additionally changes in 04A.05. Preserve the old assertion as a nonpass; successor checks bind current sources and exact changed members."),
        owner_decisions=["Exact 04A02-2X2-NATIVE-01 case/criterion/three-obligation disposition for switching Ui82x2 computation to the strict shared input/report path; no N execution authorized.",
            "SC-PROPORTIONS-PAIRED-01 independent scientific mapping/label decision before correcting the legacy test; do not promote compatibility output."],
        later_work=["04A.27 combined-selection geometry/importer/RefEdit/UDF reconciliation remains untouched","04A.28 layout/formatting","04A.45-48 native validation remains restricted"],
        verdict="PARTIALLY QUALIFIED PENDING EXACT OWNER DISPOSITION")


def check():
    value=json.loads(DEST.read_bytes())
    assert value==build(),"04A.05 successor source/contract drift"
    assert value["populations"]==dict(transferred_controls=218,transferred_obligations=239,broader_controls=802,original_obligations=1514)
    assert value["withheld_ui82x2_sha256"]=="8df9f73fd01e5677f671b8f30ff423d6d7f038a17361bc2c1c4c9f9c9ff41d85"
    assert value["authority_sha256"][AMENDMENT]=="8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307"
    for p in PRODUCTION[:3]:
        source=(ROOT/p).read_text(encoding="utf-8-sig")
        assert "Option Strict On" in source
        assert not re.search(r"Microsoft\.(Office|VisualBasic)|ExcelDna|System\.Windows\.Forms|\bDataObj\b|AppGlobals|System\.IO",source)
    assert not git("diff","--cached","--name-only").strip()
    print("04A.05 successor contracts PASS; exact Ui82x2 native wiring withheld; zero releases")


if __name__=="__main__":
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action",choices=["create","refresh-working-draft","check"])
    args=parser.parse_args()
    if args.action in ("create","refresh-working-draft"):
        DEST.parent.mkdir(parents=True,exist_ok=True)
        if args.action=="refresh-working-draft":
            old=json.loads(DEST.read_bytes())
            assert old["schema"]=="04A05-source-semantics-restrictions-v1" and old["base"]==BASE
            assert not git("ls-files","--",DEST.relative_to(ROOT).as_posix()).strip()
        with DEST.open("x" if args.action=="create" else "w",encoding="utf-8",newline="\n") as stream:
            json.dump(build(),stream,indent=2,ensure_ascii=False)
            stream.write("\n")
    else:
        check()
