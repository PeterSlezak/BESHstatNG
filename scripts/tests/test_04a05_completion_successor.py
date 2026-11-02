"""Owner-approved successor contracts. Original partial/frozen expectations remain untouched and executable."""
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location("completion_contract",ROOT/"scripts/verify-04a05-completion-contract.py")
contract=importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


class Completion05SuccessorTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data=json.loads(contract.DEST.read_bytes())
        cls.initial=json.loads((contract.RUN/"completion-baseline.json").read_bytes())

    def test_owner_bound_successor_and_source_hashes(self):
        contract.check()

    def test_exact_native_timing_exception_never_releases_ids(self):
        record=self.data["native_timing_exception"]
        self.assertEqual("04A02-2X2-NATIVE-01",record["case_id"])
        expected={"control:BESHStatNG.Ui82x2.RefEdit1_WorksheetData","control:BESHStatNG.Ui82x2.RefEditOutput","control:BESHStatNG.Ui82x2.btnHelp"}
        self.assertEqual(expected,{r["obligation_id"] for r in record["obligations"]})
        self.assertEqual(2,sum(r["blocked_control"] for r in record["obligations"]))
        for row in record["obligations"]:
            self.assertEqual("OPEN",row["disposition"])
            self.assertFalse(row["native_executed"])
            self.assertFalse(row["owner_released"])
        self.assertFalse(record["native_execution_authorized"])
        self.assertFalse(record["release_authorized"])

    def test_original_source_row_hashes_are_preserved(self):
        prior=json.loads((ROOT/contract.PRIOR).read_bytes())
        by_id={r["obligation_id"]:r for r in prior["native_case"]["obligations"]}
        for row in self.data["native_timing_exception"]["obligations"]:
            old=by_id[row["obligation_id"]]
            self.assertEqual(old["original_row_sha256"],row["original_row_sha256"])
            self.assertEqual(old["source_record_sha256"],row["source_record_sha256"])

    def test_all_populations_remain_distinct_and_open(self):
        amendment=json.loads((ROOT/"docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json").read_bytes())
        ledger=json.loads((ROOT/"docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json").read_bytes())
        self.assertEqual(218,len(set(amendment["blocked_control_ids"])))
        self.assertEqual(239,len(set(amendment["distinct_deferred_obligation_ids"])))
        self.assertEqual(1514,len(ledger))
        self.assertEqual(802,len({r["original_id"] for r in ledger if r["id_kind"]=="control" and r["affected_live_golden_blocked"]}))
        cases=[r for r in amendment["case_transfers"] if r["case_id"] in ("04A02-ANOVA-NATIVE-01","04A02-NORMALITY-NATIVE-01")]
        self.assertEqual(141,len(set().union(*(set(r["blocked_control_ids"]) for r in cases))))
        self.assertEqual(160,len(set().union(*(set(r["deferred_obligation_ids"]) for r in cases))))

    def test_three_shipping_forms_and_no_write_guards(self):
        forms={"Ui82x2":["ContingencyReportPolicy.TwoByTwo","WindowsContingencyInputAdapter.AnalyzeDialog","ParseWorksheet"],
               "Ui0OneRefeditMulticol":["WindowsContingencyInputAdapter.AnalyzeRxC"],
               "Ui8Proportions":["WindowsContingencyInputAdapter.AnalyzeProportions"]}
        for form,calls in forms.items():
            source=(ROOT/f"BESHstatNG/src/UI/{form}.vb").read_text(encoding="utf-8-sig")
            method=contract.member(source,"RunRxC" if form=="Ui0OneRefeditMulticol" else "btCompute_Click")
            for call in calls:self.assertIn(call,method)
            self.assertLess(method.index("If Not report.Success Then"),method.index("GetResultWriter()"))
            self.assertNotRegex(method,r"contingencytable\.|distributions\.|Array2intArray|strConfidenceInterval")

    def test_applied_form_constructor_events_writes_unchanged(self):
        path="BESHstatNG/src/UI/Ui82x2.vb"
        before=(contract.RUN/"preimages"/path).read_text(encoding="utf-8-sig")
        after=(ROOT/path).read_text(encoding="utf-8-sig")
        self.assertEqual(before.split("    Private Sub btCompute_Click",1)[0],after.split("    Private Sub btCompute_Click",1)[0])
        self.assertEqual(contract.member(before,"GetResultWriter"),contract.member(after,"GetResultWriter"))
        self.assertEqual(contract.member(before,"btCompute_Click").split("'Dump outputs",1)[1],contract.member(after,"btCompute_Click").split("'Dump outputs",1)[1])

    def test_paired_legacy_invocation_removed_from_production(self):
        adapter=(ROOT/contract.PRODUCTION[3]).read_text()
        self.assertNotIn("LegacyPairedEvidence",adapter)
        self.assertNotIn("Responders1 - request.Both",adapter)
        common=(ROOT/contract.PRODUCTION[2]).read_text()
        self.assertIn("contingencytable.Liddell_McNemar(request.CoreLiddellTable(), alpha)",common)
        inputs=(ROOT/contract.PRODUCTION[1]).read_text()
        self.assertIn("{{Both, Responders1}, {Responders2, Total1 - Responders1 - Responders2 - Both}}",inputs)
        self.assertIn("{{Total1 - Both - Responders1 - Responders2, Responders1}, {Responders2, Both}}",inputs)

    def test_corrected_labels_and_stable_pvalue_cell(self):
        source=(ROOT/contract.PRODUCTION[2]).read_text()
        paired=contract.member(source,"ProjectPaired")
        for value in ("Total Responders in the 1st Category","Total Responders in the 2nd Category","Proportion in the 2nd Category"):
            self.assertIn(value,paired)
        self.assertNotIn("Total Number of Subjects in the 2nd Category",paired)
        self.assertIn('{"Two-sided P-value", test.Pvalue}}, 9)',paired)
        self.assertIn("proportions.paired-nonfinite",paired)

    def test_core_only_two_functions_changed_and_no_duplicate_algorithm(self):
        before=(contract.RUN/"preimages"/contract.PRODUCTION[4]).read_text(encoding="utf-8-sig")
        after=(ROOT/contract.PRODUCTION[4]).read_text(encoding="utf-8-sig")
        restored=after
        for name in ("PairedProportions","Liddell_McNemar"):
            restored=restored.replace(contract.member(after,name),contract.member(before,name))
        self.assertEqual(before,restored)
        interval=contract.member(after,"PairedProportions")
        self.assertIn("CDbl(NoResp1 + RespBoth) *",interval)
        self.assertIn("CDbl(RespBoth) *",interval)
        exact=contract.member(after,"Liddell_McNemar")
        self.assertIn("Not estimable (no discordant pairs)",exact)
        self.assertIn("Double.PositiveInfinity",exact)
        self.assertNotIn("distributions.F_RT(",exact)
        self.assertIn("distributions.F_CDF(",exact)
        self.assertNotRegex((ROOT/contract.PRODUCTION[2]).read_text(),r"Math\.(Sqrt|Log|Exp)|NormSInv|BinomialTail")

    def test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial(self):
        path=contract.PRODUCTION[2]
        before=(contract.RUN/"preimages"/path).read_text(encoding="utf-8-sig")
        after=(ROOT/path).read_text(encoding="utf-8-sig")
        self.assertEqual(before.split("                Else\n                    issues.Add(ContingencyInputPolicy.ErrorIssue(\"proportions.paired-evidence-required\"",1)[0],
                         after.split("                ElseIf request.Mode = ProportionsMode.Paired Then",1)[0])
        for path in ("BESHstatNG/src/UI/Ui8Proportions.vb","BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb","BESHStat.Excel.Common/src/Results/ContingencyReportPolicy.vb"):
            self.assertEqual(self.initial["source_sha256"][path],contract.sha(path))

    def test_all_inherited_tests_and_partial_records_remain_immutable(self):
        for path,digest in self.data["frozen_source_pins"].items():
            self.assertEqual(digest,contract.sha(path),path)
        self.assertEqual(self.initial["source_sha256"][contract.PRIOR],contract.sha(contract.PRIOR))
        self.assertEqual("95328f1417731c433ef39fe1e6a6777e637d2ecf822a9541ffed4779e59e52f4",contract.sha(contract.PATCH))

    def test_independent_candidate_provenance_not_core_or_golden(self):
        meta=json.loads((ROOT/"docs/architecture/04A05/paired-candidate-reference-v1.json").read_bytes())
        self.assertEqual("CANDIDATE_NOT_P_GOLDEN",meta["status"])
        self.assertEqual(0,meta["independent_scientific_reviewers"])
        self.assertEqual(meta["fixture_sha256"],contract.sha(meta["fixture"]))
        self.assertEqual(meta["generator_sha256"],contract.sha("scripts/reference-04a05-completion.py"))

    def test_no_designer_importer_targets_exports_or_signing_change(self):
        for path in ("BESHstatNG/src/UI/Ui82x2.Designer.vb","BESHstatNG/src/DM/ExcelDnaDataImporter.vb","BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb"):
            self.assertFalse(contract.git("diff","--name-only","--",path).strip())
        self.assertFalse(contract.git("diff","--name-only","--","BESHstatNG/src/ExcelUDFs").strip())
        self.assertFalse(contract.git("diff","--name-only","--",".gitignore").strip())
        for path in ("BESHStat.Core/BESHStat.Core.vbproj","BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"):
            self.assertIn("<TargetFramework>netstandard2.0</TargetFramework>",(ROOT/path).read_text())
        self.assertEqual(dict(P="NO",N1="NO GO",N2="NO",Gate_C2="UNMET",released_controls=0,discharged_obligations=0),self.data["restrictions"])


if __name__=="__main__":unittest.main()
