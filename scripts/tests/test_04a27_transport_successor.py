"""04A.27 successor contracts; historical source expectations stay untouched."""
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("transport27_contract", ROOT / "scripts/verify-04a27-contract.py")
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


class Transport27SuccessorTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data = json.loads(contract.DEST.read_bytes())
        cls.baseline = json.loads((contract.RUN / "baseline.json").read_bytes())

    def test_exact_successor_and_unapplied_patch(self):
        contract.check()
        self.assertEqual(contract.PATCH.read_text(encoding="utf-8"), contract.proposed_patch())
        contract.git("apply", "--check", str(contract.PATCH))

    def test_shared_populations_are_unions_and_restrictions_remain_open(self):
        amendment = json.loads((ROOT / contract.AMENDMENT).read_bytes())
        cases = amendment["case_transfers"]
        self.assertEqual(218, len({c for row in cases for c in row["blocked_control_ids"]}))
        self.assertEqual(239, len({c for row in cases for c in row["deferred_obligation_ids"]}))
        shared = [r for r in cases if r["case_id"] in ("04A02-ANOVA-NATIVE-01", "04A02-NORMALITY-NATIVE-01")]
        self.assertEqual(141, len({c for r in shared for c in r["blocked_control_ids"]}))
        self.assertEqual(160, len({c for r in shared for c in r["deferred_obligation_ids"]}))
        self.assertEqual(0, self.data["original_restrictions"]["released_controls"])
        self.assertEqual(dict(P="NO", N1="NO GO", N2="NO", Gate_C2="UNMET", native_attempts=0), self.data["restrictions"])

    def test_source_mapping_rows_are_real_original_rows(self):
        with (ROOT / "docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv").open(encoding="utf-8-sig", newline="") as stream:
            original = list(csv.DictReader(stream))
        source = {r["control_id"]: r for r in original}
        rows = self.data["blocked_switch"]["affected_refedit_obligations"]
        self.assertTrue(rows)
        self.assertEqual(len(rows), len({r["obligation_id"] for r in rows}))
        for r in rows:
            key = r["obligation_id"].removeprefix("control:")
            digest = hashlib.sha256(json.dumps(source[key], sort_keys=True, ensure_ascii=False).encode("utf-8")).hexdigest()
            self.assertEqual(digest, r["original_row_sha256"], key)
            self.assertEqual("OPEN", r["disposition"])
            self.assertFalse(r["owner_released"])
            self.assertFalse(r["native_executed"])

    def test_two_by_two_help_does_not_become_a_third_blocked_control(self):
        rows = self.data["two_by_two_still_open"]
        self.assertEqual(3, len(rows))
        self.assertEqual(2, sum(r["blocked_control"] for r in rows))
        self.assertFalse(next(r for r in rows if r["obligation_id"].endswith(".btnHelp"))["blocked_control"])

    def test_original_member_ids_and_body_pins_not_fabricated(self):
        self.assertEqual(self.data["current_member_anchors"], contract.current_members(self.baseline["main"]))
        with (ROOT / "docs/architecture/WindowsMigrationLedger/symbols.csv").open(encoding="utf-8-sig", newline="") as stream:
            symbols = list(csv.DictReader(stream))
        by_id = {r["symbol_id"]: r for r in symbols}
        self.assertTrue(self.data["original_member_anchors"])
        for row in self.data["original_member_anchors"]:
            self.assertTrue(all(by_id[row["symbol_id"]][k] == v for k, v in row.items()))

    def test_historical_source_checks_and_authorities_unchanged(self):
        self.assertEqual(self.data["preexisting_publication_byte_conflicts"], contract.publication_proof())
        for p, h in {**self.data["frozen_script_tests"], **self.data["authority_hashes"]}.items():
            self.assertEqual(h, contract.sha(p), p)
        for folder in ("BESHStat.Excel.Common.Tests/src", "BESHStatNG.Test/src"):
            # Every preexisting tracked test remains byte-for-byte committed; new tests are additive.
            for p in contract.git("ls-tree", "-r", "--name-only", "main", "--", folder).decode().splitlines():
                if p.endswith((".py", ".vb")):
                    # Git checkout CRLF conversion predates this batch; compare the
                    # committed logical text, without rewriting any source bytes.
                    self.assertEqual(contract.git("show", "main:" + p).replace(b"\r\n", b"\n"), (ROOT / p).read_bytes().replace(b"\r\n", b"\n"), p)

    def test_core_designer_and_refedit_sources_are_unmodified(self):
        for p, h in self.baseline["source_pins"].items():
            if p.startswith("BESHStat.Core/") or p.endswith("Designer.vb") or "/Refedit/" in p:
                self.assertEqual(h, contract.sha(p), p)

    def test_udf_exports_signatures_and_packing_are_preserved(self):
        files = list((ROOT / "BESHstatNG/src/ExcelUDFs").rglob("*.vb"))
        exported = [p for p in files if re.search(r"<ExcelFunction\b", p.read_text(encoding="utf-8-sig"), re.I)]
        self.assertEqual(268, sum(len(re.findall(r"<ExcelFunction\b", p.read_text(encoding="utf-8-sig"), re.I)) for p in exported))
        for p in exported:
            name = p.relative_to(ROOT).as_posix()
            self.assertEqual(self.baseline["source_pins"][name], contract.sha(name))
        for name in ("BESHstatNG/BESHstatNG-AddIn.dna", ".gitignore"):
            if (ROOT / name).exists():
                self.assertFalse(contract.git("diff", "--", name).strip())

    def test_real_gui_and_udf_routes_use_canonical_contract(self):
        paired = (ROOT / "BESHstatNG/src/DM/WindowsPairedInputAdapter.vb").read_text(encoding="utf-8-sig")
        reader = (ROOT / "BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb").read_text(encoding="utf-8-sig")
        self.assertIn("SpreadsheetSelectionTransport.Capture(snapshot, table.RowIds)", paired)
        for call in ("ReadUdfMatrix", "NumericMatrix", "ExtractNumericPairs", "PairedArgumentsAligned"):
            self.assertIn("WindowsSpreadsheetTransport." + call, reader)
        self.assertNotIn("BuildConcreteRangeAddress", (ROOT / "BESHStat.Excel.Common/src/Input/SpreadsheetSelectionTransport.vb").read_text())

    def test_portable_targets_and_host_boundaries_stay_intact(self):
        code = (ROOT / "BESHStat.Excel.Common/src/Input/SpreadsheetSelectionTransport.vb").read_text()
        self.assertIn("Option Strict On", code)
        self.assertIsNone(re.search(r"ExcelDna|Microsoft\.(Office|VisualBasic)|System\.Windows\.Forms|System\.IO|Marshal\.", code))
        for p in ("BESHStat.Core/BESHStat.Core.vbproj", "BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"):
            self.assertIn("netstandard2.0", (ROOT / p).read_text())
        self.assertIn(r'<Compile Include="src\DM\WindowsSpreadsheetTransport.vb"', (ROOT / "BESHstatNG/BESHStatNG.vbproj").read_text())

    def test_native_identity_and_risk_review_are_not_self_attested(self):
        risk = json.loads((ROOT / "docs/architecture/04A02/04A02-C14-N1-risk-review.pending.json").read_bytes())
        self.assertEqual("NO_GO_SOURCE_REVIEW_AND_REAL_CONTEXT_MISSING", risk["status"])
        self.assertIsNone(risk["independent_risk_reviewer"]["signature"])
        for source in risk["source_pins"]:
            if source["path"].startswith("scripts/"):
                self.assertEqual(source["sha256"], contract.sha(source["path"]))
        code = (ROOT / "BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb").read_text()
        self.assertNotIn("ReleaseComObject", code)
        self.assertNotIn("Process.Start", code)

    def test_staged_index_empty_and_geometry_exception_not_inferred(self):
        self.assertFalse(contract.git("diff", "--cached", "--name-only").strip())
        switch = self.data["blocked_switch"]
        self.assertFalse(switch["applied"])
        self.assertTrue(switch["unchanged"])
        self.assertIn("C05 exception does not apply", switch["missing"])


if __name__ == "__main__":
    unittest.main()
