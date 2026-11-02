"""Current shipping/source gates; no old expectation, receipt or native release is modified."""
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location("contract05",ROOT/"scripts/verify-04a05-contract.py")
contract=importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


class ContingencySourceGates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data=json.loads(contract.DEST.read_bytes())

    def test_versioned_successor_sources_and_case_boundaries(self):
        contract.check()

    def test_exact_two_blocked_controls_and_three_original_obligations(self):
        native=self.data["native_case"]
        self.assertEqual("04A02-2X2-NATIVE-01",native["case_id"])
        self.assertEqual("04A.05",native["primary_owner"])
        expected={"control:BESHStatNG.Ui82x2.RefEdit1_WorksheetData","control:BESHStatNG.Ui82x2.RefEditOutput","control:BESHStatNG.Ui82x2.btnHelp"}
        rows=native["obligations"]
        self.assertEqual(expected,{r["obligation_id"] for r in rows})
        self.assertEqual(3,len(rows))
        self.assertEqual(2,sum(r["blocked_control"] for r in rows))
        self.assertFalse(next(r for r in rows if r["obligation_id"].endswith("btnHelp"))["blocked_control"])

    def test_no_releases_or_native_claim_from_host_free_tests(self):
        native=self.data["native_case"]
        self.assertEqual(("NO GO","NO","NO","UNMET"),(native["N1"],native["N2"],native["P"],native["Gate_C2"]))
        self.assertEqual((0,0),(native["controls_released"],native["obligations_discharged"]))
        for row in native["obligations"]:
            self.assertEqual("OPEN",row["disposition"])
            self.assertFalse(row["owner_released"])
            self.assertFalse(row["native_executed"])
        self.assertFalse(self.data["characterization"]["qualification"].find("NOT_P_GOLDEN")==-1)

    def test_no_shared_population_double_count(self):
        self.assertEqual(dict(transferred_controls=218,transferred_obligations=239,broader_controls=802,original_obligations=1514),contract.counts())

    def test_ui82x2_source_wiring_explicitly_withheld(self):
        self.assertEqual("8df9f73fd01e5677f671b8f30ff423d6d7f038a17361bc2c1c4c9f9c9ff41d85",contract.sha("BESHstatNG/src/UI/Ui82x2.vb"))
        status=next(c for c in self.data["contracts"] if c["id"]=="2x2-semantic-report")
        self.assertEqual("IMPLEMENTED_HOST_FREE_QUALIFIED_NATIVE_WIRING_WITHHELD",status["status"])
        self.assertEqual("PARTIALLY QUALIFIED PENDING EXACT OWNER DISPOSITION",self.data["verdict"])

    def test_other_multicol_members_including_04a04_selection_unchanged(self):
        path="BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb"
        before=contract.members(contract.git("show",contract.BASE+":"+path).decode("utf-8-sig"))
        after=contract.members((ROOT/path).read_text(encoding="utf-8-sig"))
        for name in before:
            if name not in ("btCompute_Click","RunRxC"):
                self.assertEqual(before[name],after[name],name)
        self.assertIn("WindowsPairedInputAdapter.SelectBlocks",after["getDataMultipleGroups"])

    def test_rxc_shipping_dispatch_guard_and_common_report(self):
        source=(ROOT/"BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb").read_text(encoding="utf-8-sig")
        methods=contract.members(source)
        self.assertIn("If Me.Tag = HelpTopic.RxCTable Then\n                Me.RunRxC()",methods["btCompute_Click"])
        body=methods["RunRxC"]
        self.assertIn("WindowsContingencyInputAdapter.AnalyzeRxC",body)
        self.assertLess(body.index("If Not report.Success Then"),body.index("GetResultWriter()"))
        self.assertNotRegex(body,r"contingencytable\.|strConfidenceInterval|Matrix\.Array2intArray")

    def test_proportions_shipping_all_modes_and_margin_focus(self):
        source=(ROOT/"BESHstatNG/src/UI/Ui8Proportions.vb").read_text(encoding="utf-8-sig")
        methods=contract.members(source)
        body=methods["btCompute_Click"]
        self.assertIn("ProportionsInputPolicy.ModeFromChoices",body)
        self.assertIn("Not Me.ValidateIndependentMargin()",body)
        self.assertIn("WindowsContingencyInputAdapter.AnalyzeProportions",body)
        self.assertIn("ProportionsInputPolicy.ValidateMargin",methods["ValidateIndependentMargin"])
        self.assertLess(body.index("If Not report.Success Then"),body.index("GetResultWriter()"))
        self.assertNotRegex(body,r"contingencytable\.|distributions\.|strConfidenceInterval|equivalencetests\.")

    def test_proportion_events_and_constructor_remain_windows_unchanged(self):
        path="BESHstatNG/src/UI/Ui8Proportions.vb"
        old=contract.git("show",contract.BASE+":"+path).decode("utf-8-sig")
        new=(ROOT/path).read_text(encoding="utf-8-sig")
        before,after=contract.members(old),contract.members(new)
        for name in before:
            if name not in ("btCompute_Click","ValidateIndependentMargin"):
                self.assertEqual(before[name],after[name],name)
        self.assertEqual(old[:old.index("    Private Sub btCompute_Click")],new[:new.index("    Private Sub btCompute_Click")])

    def test_output_destinations_and_overwrite_flow_preserved(self):
        for form in ("Ui0OneRefeditMulticol","Ui8Proportions"):
            path=f"BESHstatNG/src/UI/{form}.vb"
            before=contract.members(contract.git("show",contract.BASE+":"+path).decode("utf-8-sig"))
            after=contract.members((ROOT/path).read_text(encoding="utf-8-sig"))
            self.assertEqual(before["GetResultWriter"],after["GetResultWriter"])
            name="RunRxC" if form.startswith("Ui0") else "btCompute_Click"
            self.assertEqual(before[name].split("'Dump outputs",1)[1],after[name].split("'Dump outputs",1)[1])

    def test_one_generic_count_parser_and_no_ci_reparsing(self):
        input_source=(ROOT/contract.PRODUCTION[0]).read_text()
        self.assertIn("RectangularCategoricalParsers.ParseContingencyCounts",input_source)
        self.assertNotIn("Double.TryParse",input_source)
        for path in contract.PRODUCTION[1:3]:
            source=(ROOT/path).read_text()
            self.assertNotRegex(source,r"Double\.(Parse|TryParse)|Split\(|Regex|Math\.(Sqrt|Exp|Log)|NormSInv")
            self.assertIn("strConfidenceInterval",source)

    def test_new_windows_compile_and_real_producer_input(self):
        self.assertIn('Compile Include="src\\DM\\WindowsContingencyInputAdapter.vb"',(ROOT/"BESHstatNG/BESHStatNG.vbproj").read_text())
        self.assertIn('Compile Include="src\\ContingencyAdapterTests.vb"',(ROOT/"BESHStatNG.Test/BESHStatNG.Tests.vbproj").read_text())
        adapter=(ROOT/contract.PRODUCTION[3]).read_text()
        for token in ("table.ObjectMatrix","table.RowIds","ContingencyInputPolicy.Parse","ProportionsInputPolicy.Validate","ContingencyReportPolicy.RxC"):
            self.assertIn(token,adapter)
        self.assertNotRegex(adapter,r"AppGlobals|ExcelConnector|GetActiveObject|Workbooks\.Open|ExcelDnaResultWriter")

    def test_scientific_ambiguity_is_not_a_permanent_portable_wrong_table(self):
        common=(ROOT/contract.PRODUCTION[2]).read_text()
        self.assertIn("proportions.paired-evidence-required",common)
        self.assertIn("ProjectPaired",common)
        adapter=(ROOT/contract.PRODUCTION[3]).read_text()
        self.assertIn("SC-PROPORTIONS-PAIRED-01",adapter)
        self.assertIn("Private Shared Function LegacyPairedEvidence",adapter)
        self.assertEqual(3,len(self.data["candidates"]))

    def test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed(self):
        old=json.loads((ROOT/"docs/architecture/04A04/compatibility-crosswalk.json").read_bytes())
        drift=[p for p,v in old["production"].items() if contract.sha(p)!=v["after_sha256"]]
        self.assertEqual({"BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb","BESHstatNG/src/UI/UiTwoInputRefedits.vb","BESHstatNG/src/UI/Ui9ANOVA2nested.vb"},set(drift))
        # Two untouched source byte pins already drift after publication/checkout;
        # this batch changes only Ui0 semantics. Keep all old records/assertions visible.
        self.assertFalse(contract.git("diff","--name-only","--","BESHstatNG/src/UI/UiTwoInputRefedits.vb","BESHstatNG/src/UI/Ui9ANOVA2nested.vb").strip())
        for path in ("scripts/tests/test_04a04_alignment.py","scripts/verify-04a04-contract.py"):
            self.assertEqual(contract.git("show",contract.BASE+":"+path),(ROOT/path).read_bytes())

    def test_historical_ledgers_and_importer_unchanged(self):
        for path,digest in self.data["authority_sha256"].items():
            self.assertEqual(digest,contract.sha(path))
        self.assertEqual("363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10",self.data["authority_sha256"][contract.LEDGER])

    def test_no_designer_udf_numerical_packaging_or_framework_change(self):
        changed=contract.git("diff","--name-only").decode().splitlines()
        self.assertFalse(any(p.endswith("Designer.vb") or "ExcelUDFs/" in p or p.startswith("BESHStat.Core/") or p.endswith(".dna") for p in changed))
        for path in ("BESHStat.Core/BESHStat.Core.vbproj","BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"):
            self.assertIn("<TargetFramework>netstandard2.0</TargetFramework>",(ROOT/path).read_text())
        self.assertEqual(268,len(contract.git("grep","-n","<ExcelFunction",contract.BASE,"--","BESHstatNG/src/ExcelUDFs").splitlines()))
        self.assertFalse(contract.git("diff","--cached","--name-only").strip())


if __name__=="__main__":
    unittest.main()
