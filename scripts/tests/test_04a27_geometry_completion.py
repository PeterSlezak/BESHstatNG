"""Owner-approved active geometry contracts; prior unapplied tests remain executable."""
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("geometry27_contract", ROOT / "scripts/verify-04a27-geometry-contract.py")
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


class Geometry27CompletionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data = json.loads(contract.DEST.read_bytes())
        cls.baseline = json.loads((contract.RUN / "baseline.json").read_bytes())
        cls.source = (ROOT / contract.IMPORTER).read_text(encoding="utf-8-sig")

    def test_source_owner_pins_and_all_44_original_rows(self):
        contract.check()
        rows = self.data["original_44_obligations"]
        self.assertEqual(44, len({r["obligation_id"] for r in rows}))
        self.assertTrue(all(r["disposition"] == "OPEN" and not r["owner_released"] and not r["native_executed"] for r in rows))

    def test_reviewed_patch_and_exact_applied_copy_are_immutable(self):
        self.assertEqual("96a6dcfe84b630f8f8451a2da02acbb7108dbe26986c960ae3eebe4d8c8433b1", contract.sha(contract.PATCH))
        applied = json.loads((contract.RUN / "reviewed-application.json").read_bytes())
        self.assertTrue(applied["reverse_apply_check"])
        self.assertEqual(applied["exact_applied_sha256"], self.data["pins"]["exact_applied_source_sha256"])
        self.assertIn("PROPOSED ONLY", (ROOT / contract.PATCH).read_text())
        self.assertNotIn("PROPOSED ONLY", self.source)

    def test_real_importer_true_dispatch_precedes_every_legacy_geometry_operation(self):
        body = contract.member(self.source, "Import")
        self.assertLess(body.index("If bStartRow Then Return ImportPhysicalAreas(ws, ref, fullString)"), body.index("Dim nCols"))
        physical = contract.member(self.source, "ImportPhysicalAreas")
        self.assertIn("ImportPhysicalSelection(original, addresses", physical)
        self.assertIn("For Each address As String In selected", physical)
        self.assertIn("ws.Range(address)", physical)
        self.assertIn("ValidatePhysicalMergeState(area.MergeCells)", physical)
        self.assertIn("AcquireRangeSetData(areas)", physical)
        self.assertNotIn("BuildConcreteRangeAddress", physical)
        self.assertNotIn("ResolveStartRow", physical)

    def test_false_mode_and_materialization_members_remain_exact(self):
        proof = self.data["member_proofs"]
        self.assertEqual(proof["False_mode_original_sha256"], proof["False_mode_after_removing_only_True_switch_sha256"])
        before = (contract.RUN / "preimages" / contract.IMPORTER).read_text(encoding="utf-8-sig")
        for name in proof["unchanged_members"]:
            self.assertEqual(contract.member(before, name), contract.member(self.source, name))
        body = contract.member(self.source, "ImportInto")
        self.assertIn("Import(ref, bStartRow)", body)
        self.assertIn("CharCols:=CharCols, SkipRow:=SkipRow, cloneTable:=False", body)

    def test_shipping_join_header_and_policy_match_reviewed_projection(self):
        reviewed = (contract.RUN / "importer-exact-reviewed-patch.vb").read_text(encoding="utf-8-sig")
        old = contract.member(reviewed, "ImportPhysicalAreas").split("        Dim joined =", 1)[1].split("        result.SourceInfo.Address = original", 1)[0]
        new = contract.member(self.source, "ImportPhysicalSelection").split("        Dim joined =", 1)[1].split("        result.SourceInfo.Address = original", 1)[0]
        substitution = self.data["member_proofs"]["sole_projection_substitution"]
        self.assertEqual(old.replace(substitution["before"], substitution["after"]), new)
        self.assertNotIn("ColNumber2Letter", new)
        self.assertIn("PhysicalIntersection", new)
        self.assertNotIn("Array2intArray", new)

    def test_provenance_sidecar_has_weak_table_keys_and_no_excel_host_override(self):
        self.assertIn("ConditionalWeakTable(Of CoreDataTable", self.source)
        self.assertIn("physicalEvidence.Add(result, joined)", self.source)
        self.assertIn("physicalEvidence.TryGetValue(table, evidence)", self.source)
        body = contract.member(self.source, "ImportPhysicalSelection")
        self.assertIn("acquire(Array.AsReadOnly(selected))", body)
        self.assertIsNone(re.search(r"AppGlobals|GetResultWriter|New Excel.Application|ReleaseComObject|MsgBox", body))
        self.assertNotRegex(self.source, r"Shared\s+(?:acquire|resolver)\s+As\s+Func")

    def test_initial_implementation_and_historical_tests_not_rewritten(self):
        for path, digest in self.baseline["source_pins"].items():
            if path.startswith(("BESHStat.Core/", "BESHStat.Excel.Common/", "scripts/tests/", "BESHStatNG.Test/src/")) and path != contract.COLUMN_FORMATTER:
                self.assertEqual(digest, contract.sha(path), path)
        original_formatter = contract.git("show", self.baseline["main"] + ":" + contract.COLUMN_FORMATTER).decode("utf-8-sig").replace("\r\n", "\n")
        self.assertEqual(original_formatter, (ROOT / contract.COLUMN_FORMATTER).read_text(encoding="utf-8-sig").replace(contract.FORMATTER_ADDITION, ""))
        self.assertEqual(self.baseline["source_pins"][contract.PRIOR], contract.sha(contract.PRIOR))
        partial = "docs/architecture/batch-04A-27-owner-review.md"
        self.assertEqual(self.baseline["source_pins"][partial], contract.sha(partial))

    def test_wider_restrictions_and_two_by_two_help_not_released(self):
        expected = {"inherited_obligations": 1514, "restricted_live_controls": 802, "transferred_controls": 218,
                    "transferred_obligations": 239, "shared_controls": 141, "shared_obligations": 160,
                    "released_controls": 0, "discharged_obligations": 0}
        self.assertEqual(expected, self.data["retained_populations"])
        rows = self.data["original_two_by_two_open"]
        self.assertEqual(3, len(rows))
        self.assertEqual(2, sum(r["blocked_control"] for r in rows))
        self.assertTrue(all(r["disposition"] == "OPEN" for r in rows))
        self.assertFalse(self.data["authority"]["native_authorized"])
        self.assertEqual(dict(P="NO", N1="NO GO", N2="NO", Gate_C2="UNMET", native_attempts=0), self.data["restrictions"])

    def test_real_consumer_routes_and_udf_exports_preserved(self):
        calls = {"UiTwoInputRefedits": ["WindowsPairedInputAdapter.SelectPairs", "ExcelDnaDataImporter.ImportInto"],
                 "Ui0OneRefeditMulticol": ["WindowsPairedInputAdapter.SelectBlocks", "ExcelDnaDataImporter.ImportInto"],
                 "UibyID": ["WindowsUnivariateInputAdapter.SelectById", "ExcelDnaDataImporter.ImportInto"],
                 "Ui82x2": ["WindowsContingencyInputAdapter.ParseWorksheet", "ExcelDnaDataImporter.ImportInto"]}
        for form, targets in calls.items():
            source = (ROOT / f"BESHstatNG/src/UI/{form}.vb").read_text(encoding="utf-8-sig")
            for target in targets:
                self.assertIn(target, source)
        files = list((ROOT / "BESHstatNG/src/ExcelUDFs").rglob("*.vb"))
        self.assertEqual(268, sum(len(re.findall(r"<ExcelFunction\b", p.read_text(encoding="utf-8-sig"), re.I)) for p in files))
        for p in files:
            name = p.relative_to(ROOT).as_posix()
            self.assertEqual(self.baseline["source_pins"][name], contract.sha(name))

    def test_compile_targets_and_native_helpers_unchanged(self):
        project = (ROOT / "BESHStatNG.Test/BESHStatNG.Tests.vbproj").read_text()
        self.assertIn(r'<Compile Include="src\PhysicalImporter27Tests.vb"', project)
        for name in ("BESHStat.Core/BESHStat.Core.vbproj", "BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj", "BESHstatNG/BESHStatNG.vbproj"):
            self.assertEqual(self.baseline["source_pins"][name], contract.sha(name))
        for path, digest in self.baseline["source_pins"].items():
            if "/Refedit/" in path or path.startswith("scripts/04a02/"):
                self.assertEqual(digest, contract.sha(path))
        self.assertFalse(contract.git("diff", "--cached", "--name-only").strip())


if __name__ == "__main__":
    unittest.main()
