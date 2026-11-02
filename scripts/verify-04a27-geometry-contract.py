"""Owner-bound successor for the activated importer; original C27 records stay frozen."""
import argparse
import csv
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / "TestResults/04A-27-20261102-completion-01"
DEST = ROOT / "docs/architecture/04A27/geometry-owner-disposition-2026-11-02.json"
IMPORTER = "BESHstatNG/src/DM/ExcelDnaDataImporter.vb"
PATCH = "docs/architecture/04A27/import-physical-geometry-PROPOSED.patch"
PRIOR = "docs/architecture/04A27/transport-restrictions-v1.json"
LEDGER = "docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"
COLUMN_FORMATTER = "BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb"
FORMATTER_ADDITION = """        ''' <summary>Format a known Excel column without querying active host state.</summary>
        Public Shared Function ColumnLetters(index As Integer) As String
            If index < 1 OrElse index > ExcelMaxColumns Then Throw New ArgumentOutOfRangeException(NameOf(index))
            Return ToColumn(index)
        End Function

"""
OWNER_QUOTE = """The owner explicitly approves the existing 04A.27 shared transport and Windows integration as **partially qualified** and **authorizes applying the exact reviewed patch**:

- Patch file: `docs/architecture/04A27/import-physical-geometry-PROPOSED.patch`
- Exact patch SHA-256: `96a6dcfe84b630f8f8451a2da02acbb7108dbe26986c960ae3eebe4d8c8433b1`
- Shipping target: `BESHstatNG/src/DM/ExcelDnaDataImporter.vb`
- Previously unchanged target SHA-256: `5aa0578f771675c072e40206f165d458a693f8087a97b405abf6da19531fa9ae`
- Exact native-restriction crosswalk: `docs/architecture/04A27/transport-restrictions-v1.json`, containing **44 distinct original RefEdit obligations**. They **all remain OPEN**.

This is a **narrow timing exception for missing native pre-change capture** on this importer switch. It is NOT a finding that native Excel behavior is validated, not a release or discharge of an obligation, and not a general exception for other source changes. The prior 04A.05 2×2 timing exception does not broaden this one."""


def sha(path):
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def git(*args):
    return subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(), *args], cwd=ROOT)


def member(source, name):
    pattern = r"^    (?:Public|Private|Friend)\s+(?:Shared\s+)?(?:Sub|Function)\s+" + re.escape(name) + r"\b.*?^    End (?:Sub|Function)"
    found = re.search(pattern, source, re.M | re.S)
    assert found, "Missing actual member: " + name
    return found[0]


def validate_originals():
    baseline = json.loads((RUN / "baseline.json").read_bytes())
    assert sha(PATCH) == "96a6dcfe84b630f8f8451a2da02acbb7108dbe26986c960ae3eebe4d8c8433b1"
    assert sha(PRIOR) == baseline["source_pins"][PRIOR]
    assert sha(LEDGER) == "363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10"
    prior = json.loads((ROOT / PRIOR).read_bytes())
    rows = prior["blocked_switch"]["affected_refedit_obligations"]
    assert len(rows) == len({r["obligation_id"] for r in rows}) == 44
    original = {r["obligation_id"]: r for r in json.loads((ROOT / LEDGER).read_bytes())}
    with (ROOT / "docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv").open(encoding="utf-8-sig", newline="") as stream:
        source = {r["control_id"]: r for r in csv.DictReader(stream)}
    for row in rows:
        assert row["disposition"] == "OPEN" and row["owner_released"] is False and row["native_executed"] is False
        frozen = original[row["obligation_id"]]
        assert row["original_row_sha256"] == frozen["source_record_sha256"]
        assert row["original_source_sha256"] == frozen["source_evidence"]["codebehind_source_sha256"]
        assert row["original_designer_sha256"] == frozen["source_evidence"]["designer_source_sha256"]
        raw = source[row["obligation_id"].removeprefix("control:")]
        assert hashlib.sha256(json.dumps(raw, sort_keys=True, ensure_ascii=False).encode()).hexdigest() == row["original_row_sha256"]
    return baseline, prior, rows


def build():
    baseline, prior, rows = validate_originals()
    before = (RUN / "preimages" / IMPORTER).read_text(encoding="utf-8-sig")
    exact = (RUN / "importer-exact-reviewed-patch.vb").read_text(encoding="utf-8-sig")
    after = (ROOT / IMPORTER).read_text(encoding="utf-8-sig")
    original_import = member(before, "Import")
    current_import = member(after, "Import")
    switch = "        ' Owner-authorized 04A.27 physical-area switch; native obligations remain OPEN.\n        If bStartRow Then Return ImportPhysicalAreas(ws, ref, fullString)\n"
    assert current_import.replace(switch, "") == original_import, "False path changed beyond the reviewed True dispatch"
    unchanged = ["ImportInto", "NormalizeExcelValue", "ResolveStartRow", "BuildConcreteRangeAddress"]
    assert all(member(before, name) == member(after, name) for name in unchanged)
    start, finish = "        Dim joined =", "        result.SourceInfo.Address = original"
    reviewed_projection = member(exact, "ImportPhysicalAreas").split(start, 1)[1].split(finish, 1)[0]
    shipping_projection = member(after, "ImportPhysicalSelection").split(start, 1)[1].split(finish, 1)[0]
    old_fallback = 'ColNumber2Letter(frame.SourceAt(0, c).WorksheetColumn)'
    new_fallback = 'Global.BESHStatNG.ExcelCommon.SpreadsheetRangeAddress.ColumnLetters(frame.SourceAt(0, c).WorksheetColumn)'
    assert reviewed_projection.replace(old_fallback, new_fallback) == shipping_projection, "Reviewed join/header/cell policy changed beyond pure blank-header formatter"
    formatter_before = git("show", baseline["main"] + ":" + COLUMN_FORMATTER).decode("utf-8-sig").replace("\r\n", "\n")
    formatter_after = (ROOT / COLUMN_FORMATTER).read_text(encoding="utf-8-sig")
    assert formatter_after.count(FORMATTER_ADDITION) == 1 and formatter_after.replace(FORMATTER_ADDITION, "") == formatter_before
    return dict(schema="04A27-owner-geometry-completion-v1", date="2026-11-02", authority=dict(quote=OWNER_QUOTE,
        source="Explicit current project-owner instruction; no external signature, independent risk review or final acceptance asserted",
        final_acceptance=False, native_authorized=False),
        branch="codex/04A-27", base=baseline["main"],
        pins=dict(importer_before_sha256=baseline["source_pins"][IMPORTER], original_patch_sha256=sha(PATCH),
            exact_applied_source_sha256=hashlib.sha256((RUN / "importer-exact-reviewed-patch.vb").read_bytes()).hexdigest(),
            final_importer_sha256=sha(IMPORTER), column_formatter_before_sha256=baseline["source_pins"][COLUMN_FORMATTER], column_formatter_after_sha256=sha(COLUMN_FORMATTER),
            original_crosswalk_sha256=sha(PRIOR), original_ledger_sha256=sha(LEDGER),
            accepted_C02_amendment_sha256=sha("docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json")),
        active_shipping_switch=True, original_patch_preserved=True,
        original_44_obligations=rows,
        retained_populations=prior["original_restrictions"], restrictions=prior["restrictions"],
        original_two_by_two_open=prior["two_by_two_still_open"],
        extra_edits=["Replace obsolete PROPOSED ONLY/source-summary comments", "Extract operation-local ImportPhysicalSelection callback seam; no global host override",
            "Extract exact merge-state predicate for executable tests", "Weak-key CoreDataTable sidecar preserves immutable original frame and warnings; ImportInto retains table identity",
            "Failing all-blank header test exposed ColNumber2Letter active-worksheet dependency; new True path uses additive pure ColumnLetters wrapper around the existing Common ToColumn formatter. Intended VarB/VarE labels preserved without active Excel; False helper unchanged"],
        member_proofs=dict(False_mode_original_sha256=hashlib.sha256(original_import.encode()).hexdigest(),
            False_mode_after_removing_only_True_switch_sha256=hashlib.sha256(current_import.replace(switch, "").encode()).hexdigest(),
            unchanged_members={n: hashlib.sha256(member(after, n).encode()).hexdigest() for n in unchanged},
            reviewed_projection_sha256=hashlib.sha256(reviewed_projection.encode()).hexdigest(),
            shipping_projection_sha256=hashlib.sha256(shipping_projection.encode()).hexdigest(),
            sole_projection_substitution=dict(before=old_fallback, after=new_fallback)),
        semantics=dict(true_mode="Own selected areas; bounded existing snapshot acquisition; PhysicalIntersection; existing cell/header projection; whole-row downstream policies",
            false_mode="Unchanged legacy code including ordinal origin and singleton behavior; never labeled physical by this successor",
            sidecar="Available on returned CoreDataTable and ImportInto's un-cloned CoreTable through GetPhysicalImportEvidence; arbitrary Core clones have no attached Windows sidecar",
            diagnostic="Join warnings and raw original cells retained in sidecar; typed errors/size/identity failures throw before a table is returned; existing UI error presentation retained",
            native="COM range resolution/merged getters/connection points not executed or qualified"))


def check():
    data = json.loads(DEST.read_bytes())
    assert data == build(), "Geometry successor drift"
    assert data["pins"]["importer_before_sha256"] == "5aa0578f771675c072e40206f165d458a693f8087a97b405abf6da19531fa9ae"
    assert data["pins"]["exact_applied_source_sha256"] == "36b70018df3dab98569e2c4ba4580c240fca4bdb52fe5e24db6bf51b32efb326"
    assert data["pins"]["accepted_C02_amendment_sha256"] == "8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307"
    assert not git("diff", "--cached", "--name-only").strip()
    baseline = json.loads((RUN / "baseline.json").read_bytes())
    permitted = {IMPORTER, COLUMN_FORMATTER, "BESHStatNG.Test/BESHStatNG.Tests.vbproj", "scripts/verify-04a27-completion.py"}
    for p, h in baseline["source_pins"].items():
        if p not in permitted:
            assert sha(p) == h, "Inherited source/evidence changed: " + p
    source = (ROOT / IMPORTER).read_text(encoding="utf-8-sig")
    dispatch = member(source, "Import")
    assert dispatch.index("If bStartRow Then Return ImportPhysicalAreas") < dispatch.index("Dim nCols")
    native = member(source, "ImportPhysicalAreas")
    assert "ws.Range(address)" in native and "ValidatePhysicalMergeState(area.MergeCells)" in native
    assert "ImportPhysicalSelection(original, addresses" in native and "AcquireRangeSetData(areas)" in native
    conversion = member(source, "ImportPhysicalSelection")
    assert "PhysicalIntersection" in conversion and "frame.ToCoreDataTable(names, policy)" in conversion
    assert "BuildConcreteRangeAddress" not in native + conversion
    assert not re.search(r"GetResultWriter|WriteOutput|MsgBox|AppGlobals|New Excel.Application", conversion)
    print("04A.27 geometry successor PASS: shipping True switch active; False unchanged;44 obligations OPEN; no native release")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=["create", "check"])
    args = parser.parse_args()
    if args.action == "create":
        with DEST.open("x", encoding="utf-8", newline="\n") as stream:
            json.dump(build(), stream, indent=2, ensure_ascii=False)
            stream.write("\n")
    else:
        check()
