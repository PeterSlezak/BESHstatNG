"""Versioned 04A.27 source/gate crosswalk; no historical writes or native calls."""
import argparse
import csv
import difflib
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / "TestResults/04A-27-20261102-run01"
DIRECTORY = ROOT / "docs/architecture/04A27"
DEST = DIRECTORY / "transport-restrictions-v1.json"
PATCH = DIRECTORY / "import-physical-geometry-PROPOSED.patch"
IMPORTER = "BESHstatNG/src/DM/ExcelDnaDataImporter.vb"
AMENDMENT = "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json"
LEDGER = "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"
CHANGED = ["BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb", "BESHstatNG/src/DM/WindowsPairedInputAdapter.vb",
           "BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb"]
NEW = ["BESHStat.Excel.Common/src/Input/SpreadsheetSelectionTransport.vb", "BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb"]


def sha(path):
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT)


def proposed_patch():
    old = (ROOT / IMPORTER).read_text(encoding="utf-8-sig")
    anchor = "        Dim fullString() As String = ColumListFromRefAdress(ref)\n"
    assert old.count(anchor) == 1
    new = old.replace(anchor, anchor + '''        ' PROPOSED ONLY: activate after exact-ID native timing disposition.
        If bStartRow Then Return ImportPhysicalAreas(ws, ref, fullString)
''')
    helper = '''    Private Shared Function ImportPhysicalAreas(ws As Worksheet, original As String, addresses As String()) As CoreDataTable
        Dim areas As New List(Of Range)
        For Each address As String In addresses
            ' Resolve each actual selected rectangle independently; do not rewrite its rows.
            Dim area As Range = ws.Range(address)
            Dim merged As Object = area.MergeCells
            If Not TypeOf merged Is Boolean OrElse CBool(merged) Then Throw New ArgumentException("Merged or unverifiable merge regions are not supported by this transport.")
            areas.Add(area)
        Next
        Dim snapshots = AcquireRangeSetData(areas)
        Dim joined = Global.BESHStatNG.ExcelCommon.SpreadsheetSelectionTransport.JoinColumns(snapshots,
            Global.BESHStatNG.ExcelCommon.SpreadsheetAreaAlignment.PhysicalIntersection)
        If Not joined.Success Then Throw New Global.BESHStatNG.ExcelCommon.SpreadsheetInputValidationException(joined.Validation)
        Dim frame = joined.Data
        Dim first = frame.Data
        Dim names(first.ColumnCount - 1) As String
        For c As Integer = 0 To names.Length - 1
            Dim cell = first.Rows(0)(c)
            Select Case cell.Kind
                Case Global.BESHStatNG.ExcelCommon.SpreadsheetCellKind.Text
                    names(c) = cell.TextValue
                Case Global.BESHStatNG.ExcelCommon.SpreadsheetCellKind.Number
                    names(c) = cell.NumberValue.ToString(Globalization.CultureInfo.CurrentCulture)
                Case Global.BESHStatNG.ExcelCommon.SpreadsheetCellKind.Bool
                    names(c) = cell.BooleanValue.ToString()
                Case Else
                    names(c) = "Var" & ColNumber2Letter(frame.SourceAt(0, c).WorksheetColumn)
            End Select
        Next
        Dim policy As New Global.BESHStatNG.ExcelCommon.SpreadsheetCellPolicyOptions With {
            .Errors = Global.BESHStatNG.ExcelCommon.SpreadsheetErrorPolicy.TreatAsMissing,
            .MissingText = Global.BESHStatNG.ExcelCommon.SpreadsheetMissingTextPolicy.BlankOrWhitespaceText,
            .NumericText = If(Globalization.CultureInfo.CurrentCulture.Equals(Globalization.CultureInfo.InvariantCulture),
                Global.BESHStatNG.ExcelCommon.SpreadsheetNumericTextPolicy.Invariant, Global.BESHStatNG.ExcelCommon.SpreadsheetNumericTextPolicy.ExplicitCulture),
            .NumericCultureName = Globalization.CultureInfo.CurrentCulture.Name}
        Dim result = frame.ToCoreDataTable(names, policy)
        result.SourceInfo.Address = original
        Return result
    End Function

'''
    new = new.replace("    Public Shared Sub ImportInto(", helper + "    Public Shared Sub ImportInto(")
    return "".join(difflib.unified_diff(old.splitlines(True), new.splitlines(True), fromfile="a/" + IMPORTER, tofile="b/" + IMPORTER))


def publication_proof():
    prior = json.loads((ROOT / "docs/architecture/04A05/owner-completion-dispositions-v1.json").read_bytes())
    core = "BESHStat.Core/src/StatTests/ContingencyTable.vb"
    old = (ROOT / "TestResults/04A-05-completion-20261102-run01/preimages" / core).read_bytes().splitlines(True)
    new = (ROOT / core).read_bytes().splitlines(True)
    strip = lambda value: value.rstrip(b"\r\n")
    matcher = difflib.SequenceMatcher(a=[strip(x) for x in old], b=[strip(x) for x in new], autojunk=False)
    # Original file had mixed endings; apply the accepted new lines with CRLF
    # while retaining old endings for unchanged lines. This exactly reconstructs
    # the accepted post-change SHA without altering any source/evidence bytes.
    restored = b"".join(b"".join(old[i:j]) if op == "equal" else b"".join(strip(x) + b"\r\n" for x in new[k:l])
                        for op, i, j, k, l in matcher.get_opcodes())
    reconstructed = hashlib.sha256(restored).hexdigest()
    assert reconstructed == prior["production"][core]["after_sha256"]
    fixture = "BESHStat.Core.Tests/TestData/04A05Completion/paired-candidates-v1.csv"
    captured = next((ROOT / "TestResults/04A-05-completion-20261102-run01/executions/final-Debug-Common").rglob("paired-candidates-v1.csv"))
    original, current = captured.read_bytes(), (ROOT / fixture).read_bytes()
    assert original.replace(b"\r", b"") == current.replace(b"\r", b"")
    return dict(Core=dict(current_sha256=sha(core), accepted_sha256=reconstructed,
                          proof="Exact accepted hash reconstructed using original unchanged-line endings and CRLF on accepted changed lines; logical source identical"),
                candidate_csv=dict(current_sha256=sha(fixture), accepted_sha256=hashlib.sha256(original).hexdigest(),
                                   proof="23 CRLF rows became LF during publication; removing CR gives byte-identical data; no fixture rewrite"),
                owner_record_rewritten=False, golden_promoted=False)


def current_members(base):
    names = {CHANGED[0]: ["SnapshotSelection", "SnapshotValues", "ToSnapshotCell"],
             CHANGED[1]: ["Capture"], CHANGED[2]: ["Get2D", "TryReadNumericMatrix", "TryReadPairedNumericColumns",
                                                  "ExtractCompletePairedNumericRows", "ExtractPairedNumericColumnsIgnoringNonNumeric"]}
    result = []
    for path, methods in names.items():
        before = git("show", base + ":" + path).decode("utf-8-sig").replace("\r\n", "\n")
        after = (ROOT / path).read_text(encoding="utf-8-sig")
        for name in methods:
            pattern = r"^    (?:Public|Private|Friend)\s+(?:Shared\s+)?Function\s+" + re.escape(name) + r"\b.*?^    End Function"
            old, new = re.search(pattern, before, re.M | re.S), re.search(pattern, after, re.M | re.S)
            assert old and new, "Missing actual method " + path + ":" + name
            result.append(dict(path=path, member=name, before_sha256=hashlib.sha256(old[0].encode()).hexdigest(),
                               after_sha256=hashlib.sha256(new[0].encode()).hexdigest(),
                               convention="UTF-8 logical member text, LF, from declaration through End Function; original inventory IDs are separate"))
    return result


def build():
    baseline = json.loads((RUN / "baseline.json").read_bytes())
    amendment = json.loads((ROOT / AMENDMENT).read_bytes())
    ledger = json.loads((ROOT / LEDGER).read_bytes())
    assert sha(AMENDMENT) == "8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307"
    assert sha(LEDGER) == "363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10"
    assert len(ledger) == 1514
    assert len(set(amendment["blocked_control_ids"])) == 218
    assert len(set(amendment["distinct_deferred_obligation_ids"])) == 239
    # Source-derived conservative dependency closure: actual GUI import callers,
    # then exact original RefEdit obligations for those files. No invented IDs.
    callers = [p.relative_to(ROOT).as_posix() for p in (ROOT / "BESHstatNG/src/UI").glob("*.vb")
               if "ExcelDnaDataImporter.Import" in p.read_text(encoding="utf-8-sig")]
    transferred = {r["obligation_id"]: r for r in amendment["obligation_transfers"]}
    rows = []
    for r in ledger:
        path = r.get("source_evidence", {}).get("codebehind_path")
        if path in callers and "RefEdit" in r["obligation_id"]:
            transfer = transferred.get(r["obligation_id"])
            rows.append(dict(obligation_id=r["obligation_id"], source_file=path,
                source_record_path=r["source_record_path"], original_row_sha256=r["source_record_sha256"],
                original_source_sha256=r["source_evidence"]["codebehind_source_sha256"],
                original_designer_sha256=r["source_evidence"]["designer_source_sha256"],
                earliest_approved_batches=r["earliest_approved_batches"], affected_live_golden_blocked=r["affected_live_golden_blocked"],
                case_ids=[] if transfer is None else transfer["case_ids"],
                pre_change_gate=r["safe_capture_preconditions"] if transfer is None else transfer["pre_change_gate"],
                relationship="Conservative importer caller dependency; not a claim every option/control reaches bStartRow=True",
                disposition="OPEN", owner_released=False, native_executed=False))
    with (ROOT / "docs/architecture/WindowsMigrationLedger/symbols.csv").open(encoding="utf-8-sig", newline="") as stream:
        symbols = list(csv.DictReader(stream))
    members = [{k: r[k] for k in ("symbol_id", "path", "name", "kind", "body_sha256", "source_sha256")} for r in symbols
               if r["path"] in [IMPORTER, *CHANGED] and r["kind"] in ("SubBlock", "FunctionBlock")]
    original_tests = {p: h for p, h in baseline["source_pins"].items() if p.startswith("scripts/tests/")}
    controls = {r["original_id"] for r in ledger if r.get("affected_live_golden_blocked") and r.get("id_kind") == "control"}
    two_by_two = [r for r in amendment["obligation_transfers"] if "04A02-2X2-NATIVE-01" in r["case_ids"]]
    return dict(schema="04A27-transport-restriction-successor-v1", branch="codex/04A-27", base=baseline["main"],
        authority="Current owner's 04A.27 instruction confirms final 04A.05 acceptance/publication; no blanket geometry timing exception granted",
        authority_hashes={p: sha(p) for p in [AMENDMENT, LEDGER, "docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json",
            "docs/architecture/04A03/pre-change-crosswalk.json", "docs/architecture/04A04/compatibility-crosswalk.json", "docs/architecture/04A05/owner-completion-dispositions-v1.json"]},
        production={p: dict(before_sha256=baseline["source_pins"][p], after_sha256=sha(p)) for p in CHANGED},
        new_production={p: sha(p) for p in NEW},
        blocked_switch=dict(path=IMPORTER, sha256=sha(IMPORTER), unchanged=sha(IMPORTER) == baseline["source_pins"][IMPORTER],
            patch=PATCH.relative_to(ROOT).as_posix(), patch_sha256=sha(PATCH.relative_to(ROOT)), applied=False,
            original_example=dict(selected=["B3:B5", "E8:E10"], actual_reads=["B3:B5", "E3:E5"]),
            corrected_example=dict(actual_reads=["B3:B5", "E8:E10"], outcome="transport.no-common-rows; no accepted table/write"),
            proposed_policy="bStartRow=True: per-area Value2 snapshots, explicit physical-row intersection; original areas/row map sidecar; False remains unchanged",
            missing="Exact native pre-change baseline and independent risk/N authority OR fresh exact-ID/criterion-bound owner timing disposition; C05 exception does not apply",
            intentional_deltas=["Later selected areas never shifted or extended", "Disjoint rows reject; overlaps retain intersection with warnings", "Names from first common row", "Value2 serials stay numbers, formats remain hints", "Unchunked >10000/entire-column snapshot requests rejected before Value2; no silent used-range extension", "Nonfinite/unsupported cells fail closed"],
            affected_refedit_obligations=rows, source_caller_files=callers),
        original_member_anchors=members, current_member_anchors=current_members(baseline["main"]),
        frozen_script_tests=original_tests, preexisting_publication_byte_conflicts=publication_proof(),
        original_restrictions=dict(inherited_obligations=len(ledger), restricted_live_controls=len(controls), transferred_controls=218,
            transferred_obligations=239, shared_controls=141, shared_obligations=160, released_controls=0, discharged_obligations=0),
        two_by_two_still_open=[dict(obligation_id=r["obligation_id"], blocked_control=r["in_blocked_control_set"], disposition="OPEN") for r in two_by_two],
        restrictions=dict(P="NO", N1="NO GO", N2="NO", Gate_C2="UNMET", native_attempts=0),
        udf=dict(exports=268, signatures_changed=False, array_coordinates="unknown/0", reference_identity="SheetId only operation-local; no inferred workbook name/ID",
            numeric="Existing finite numeric-only eligibility; text/logical/date/error excluded; normalizers share Common transport", paired="Two ExcelReferences require same sheet token and physical rows before xlCoerce; arrays retain explicit ordinal semantics",
            budget="Default at most1048576 range/total/response cells and128MiB estimated; explicit smaller MaxChunkCells honored before callback; full-column metadata permitted, full-sheet rejected; shape verified after coercion; no chunked/native-read claim"))


def check():
    data = json.loads(DEST.read_bytes())
    assert data == build(), "Successor drift"
    assert not git("diff", "--cached", "--name-only").strip(), "Unauthorized staging"
    assert data["blocked_switch"]["unchanged"]
    assert data["original_restrictions"]["restricted_live_controls"] == 802
    assert all(sha(p) == h for p, h in data["frozen_script_tests"].items()), "Historical test rewritten"
    for p, h in json.loads((RUN / "baseline.json").read_bytes())["source_pins"].items():
        if p.startswith("BESHStat.Core/") or "Designer.vb" in p or "/Refedit/" in p:
            assert sha(p) == h, "Unauthorized math/designer/RefEdit change: " + p
    assert PATCH.read_text(encoding="utf-8") == proposed_patch()
    print("04A.27 successor PASS; geometry activation unapplied; exact original obligations retained")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["create", "check"])
    args = parser.parse_args()
    if args.action == "create":
        DIRECTORY.mkdir(exist_ok=True)
        with PATCH.open("x", encoding="utf-8", newline="\n") as stream:
            stream.write(proposed_patch())
        with DEST.open("x", encoding="utf-8", newline="\n") as stream:
            json.dump(build(), stream, indent=2)
            stream.write("\n")
    else:
        check()
