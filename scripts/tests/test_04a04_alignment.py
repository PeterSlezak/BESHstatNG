"""Successor gates; these execute no native controls and amend no historical assertion."""
import copy
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("contract04", ROOT / "scripts/verify-04a04-contract.py")
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


class PairedBlockSourceGates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.crosswalk = json.loads((ROOT / contract.DESTINATION).read_bytes())

    def test_exact_crosswalk_and_authorities(self):
        contract.check()

    def test_populations_are_distinct_and_not_released(self):
        _, ledger, counts = contract.populations()
        self.assertEqual((141, 160, 218, 239, 802, 1514), tuple(counts[k] for k in (
            "shared_controls", "shared_obligations", "transferred_controls", "transferred_obligations", "original_restricted_controls", "original_obligations")))
        self.assertEqual(1514, len({r["obligation_id"] for r in ledger}))
        self.assertEqual(0, self.crosswalk["restrictions"]["released_controls"])
        self.assertEqual(0, self.crosswalk["restrictions"]["discharged_obligations"])

    def test_exact_original_ids_not_fabricated(self):
        _, ledger, _ = contract.populations()
        original = {r["obligation_id"]: r for r in ledger}
        for row in self.crosswalk["controls"]:
            if row["obligation_id"]:
                self.assertIn(row["obligation_id"], original)
                self.assertEqual(original[row["obligation_id"]]["source_record_sha256"], row["source_record_sha256"])
                self.assertEqual("OPEN", row["disposition"])
            self.assertFalse(row["native_executed"])
            self.assertFalse(row["owner_released"])

    def test_no_invented_four_case_transfer(self):
        self.assertTrue(all(not r["transfer_cases"] for r in self.crosswalk["controls"]))

    def test_real_three_form_callchains(self):
        expected = {"UiTwoInputRefedits": ["SelectPairs(columData, strErr)", "AlignHotelling(Gr1Data, Gr2Data"],
            "Ui0OneRefeditMulticol": ["SelectBlocks(columData, strErr)"],
            "Ui9ANOVA2nested": ["SelectNested(byIdData, strErr)", "PairedOptionPolicy.ProjectBland", "PairedOptionPolicy.ValidateLimits"]}
        for form, calls in expected.items():
            source = (ROOT / f"BESHstatNG/src/UI/{form}.vb").read_text(encoding="utf-8-sig")
            for call in calls:
                self.assertIn(call, source)
            self.assertIn("If errText <> String.Empty Then", source)

    def test_shared_nontarget_routes_remain_guarded(self):
        two = (ROOT / "BESHstatNG/src/UI/UiTwoInputRefedits.vb").read_text(encoding="utf-8-sig")
        self.assertIn("Me.Tag = HelpTopic.PairedSingleSampleTTests OrElse Me.Tag = HelpTopic.WilcoxonSignedRankTest Then", two)
        blocks = (ROOT / "BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb").read_text(encoding="utf-8-sig")
        self.assertIn("Me.Tag = HelpTopic.FriedmanTest OrElse Me.Tag = HelpTopic.OneWayRepeatedMeasuresANOVA Then", blocks)
        self.assertIn('Me.Text <> "Skillings-Mack Test"', blocks)

    def test_adapter_is_real_producer_not_passing_projection(self):
        source = (ROOT / contract.PRODUCTION[2]).read_text()
        for value in ["imported.CoreTable", "table.NumericMatrix", "imported.CoreTable.RowIds", "PairedBlockInputPolicy.ParsePairs",
                      "PairedBlockInputPolicy.ParseBlocks", "PairedBlockInputPolicy.ParseNested", "PairedBlockInputPolicy.AlignRows"]:
            self.assertIn(value, source)
        self.assertNotRegex(source, r"AppGlobals|ExcelDnaResultWriter|ExcelConnector|GetActiveObject|Workbooks\.Open")

    def test_existing_pair_parser_is_reused(self):
        source = (ROOT / contract.PRODUCTION[0]).read_text()
        self.assertIn("TypedNumericRangeParsers.ParsePaired(input, options)", source)
        self.assertNotIn("Double.TryParse", source)

    def test_portable_dependency_and_targets(self):
        for path in contract.PRODUCTION[:2]:
            source = (ROOT / path).read_text()
            self.assertIn("Option Strict On", source)
            self.assertNotRegex(source, r"Microsoft\.(Office|VisualBasic)|ExcelDna|System\.Windows\.Forms|\bDataObj\b|AppGlobals|System\.IO")
        for path in ["BESHStat.Core/BESHStat.Core.vbproj", "BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"]:
            self.assertIn("<TargetFramework>netstandard2.0</TargetFramework>", (ROOT / path).read_text())

    def test_udf_exports_and_packaging_unchanged(self):
        changed = contract.git("diff", "--name-only").decode().splitlines()
        self.assertFalse(any("UDF" in p.upper() or p.endswith(".dna") for p in changed))
        self.assertEqual(contract.git("show", contract.BASE + ":BESHstatNG/BESHStatNG.vbproj").decode().count("ExcelDna"),
                         (ROOT / "BESHstatNG/BESHStatNG.vbproj").read_text().count("ExcelDna"))
        for form in contract.TARGETS:
            path = f"BESHstatNG/src/UI/{form}.Designer.vb"
            self.assertEqual(contract.git("show", contract.BASE + ":" + path).decode("utf-8-sig").splitlines(),
                             (ROOT / path).read_text(encoding="utf-8-sig").splitlines())

    def test_native_and_publication_remain_unreleased(self):
        self.assertEqual(dict(P="NO", N1="NO GO", N2="NO", Gate_C2="UNMET", released_controls=0, discharged_obligations=0), self.crosswalk["restrictions"])
        self.assertFalse(contract.git("diff", "--cached", "--name-only").strip())

    def test_frozen_receipts_and_provenance_are_additive(self):
        for path, expected in self.crosswalk["authorities"].items():
            self.assertEqual(expected, contract.sha(path))
        self.assertEqual("8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307", self.crosswalk["authorities"][contract.AMENDMENT])
        self.assertEqual("363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10", self.crosswalk["authorities"][contract.LEDGER])

    def test_legacy_geometry_ambiguity_is_not_hidden(self):
        self.assertIn("first area's geometry", self.crosswalk["unresolved"][0])
        importer = "BESHstatNG/src/DM/ExcelDnaDataImporter.vb"
        self.assertEqual(contract.git("show", contract.BASE + ":" + importer), (ROOT / importer).read_bytes())

    def test_isolation_disables_incompatible_optional_historical_writers(self):
        source = (ROOT / "scripts/verify-04a04.py").read_text()
        self.assertIn('env.pop("BESHSTAT_04A03_OBSERVATIONS", None)', source)
        self.assertIn('env.pop("BESHSTAT_04A02_OBSERVATIONS", None)', source)
        self.assertIn("--no-restore", source)


if __name__ == "__main__":
    unittest.main()
