"""04A.03 source-bound integration/ownership checks; no Excel or mutable baseline.

Managed policy/adapter tests execute production code. These additional checks
bind that tested seam to the real form and show the restricted host parts retain
the exact approved preimage; they are not claims of native runtime qualification.
"""
import json
from pathlib import Path
import re
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
HOST = "BESHstatNG/src/UI/UibyID.vb"
BASELINE = "9e2326d4b562cc94660745f1453b231df938bff0"


def method(source, name):
    hit = re.search(r"(?im)^    (?:Private |Public |Friend )?(?:Shared )?(?:Async )?(Sub|Function) " + re.escape(name) + r"\b.*?$", source)
    if hit is None:
        raise AssertionError("Missing method " + name)
    end = re.search(r"(?im)^    End " + hit[1] + "$", source[hit.end():])
    if end is None:
        raise AssertionError("Missing end " + name)
    return source[hit.start():hit.end() + end.end()]


class UnivariateSelectionSourceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.current = (ROOT / HOST).read_text(encoding="utf-8-sig")
        cls.before = subprocess.check_output(["git", "-c", "safe.directory=" + ROOT.as_posix(),
            "show", BASELINE + ":" + HOST], cwd=ROOT, text=True, encoding="utf-8-sig")

    def test_only_two_form_methods_change(self):
        pattern = r"(?im)^    (?:Private |Public |Friend )?(?:Shared )?(?:Async )?(?:Sub|Function) (\w+)\b"
        before_names = re.findall(pattern, self.before)
        self.assertEqual(before_names, re.findall(pattern, self.current))
        changed = {name for name in before_names if method(self.before, name) != method(self.current, name)}
        self.assertEqual({"checkInputs", "getDataMultipleGroups"}, changed)

    def test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged(self):
        for name in ["BindPilotLifecycle", "EnsurePilotAcquisition", "btCompute_Click", "ExecutionFormClosing",
                     "ExecutionFormDisposed", "optByColumn_Click", "optByID_Click", "getData2Groups",
                     "getGroupedContinuousDataByColumn", "getGroupedContinuousDataById", "RunNormality",
                     "RunDescStat", "RunOneWayANOVA", "RunOneWayANOVAThroughSharedLayer", "RunKruskallWalis"]:
            with self.subTest(method=name):
                self.assertEqual(method(self.before, name), method(self.current, name))

    def test_actual_form_calls_typed_common_roles_and_adapter(self):
        checks = method(self.current, "checkInputs")
        self.assertIn("UnivariateSelectionPolicy.InputRoles(", checks)
        self.assertEqual(2, checks.count("roles.InputsMustBeSingleColumn"))
        select = method(self.current, "getDataMultipleGroups")
        self.assertIn("WindowsUnivariateInputAdapter.CaptureColumn(columData)", select)
        self.assertIn("WindowsUnivariateInputAdapter.SelectCapturedColumns(importedColumns, strErr)", select)
        self.assertIn("WindowsUnivariateInputAdapter.SelectById(byIdData, strErr)", select)
        self.assertNotIn("DataByID2ByColumn", select)
        self.assertNotIn("DataDbl", select)

    def test_every_target_uses_shared_selector_and_error_guard_precedes_dispatch(self):
        compute = method(self.current, "btCompute_Click")
        start = compute.index("data = Me.getDataMultipleGroups(errText)")
        guard = compute.index("If errText <> String.Empty Then", start)
        exit_point = compute.index("Exit Sub", guard)
        for name in ["RunNormality", "RunDescStat", "RunOneWayANOVA", "RunKruskallWalis"]:
            self.assertLess(exit_point, compute.index("Me." + name + "(data", start))

    def test_windows_import_options_and_checkpoint_order_are_preserved(self):
        current = method(self.current, "getDataMultipleGroups")
        before = method(self.before, "getDataMultipleGroups")
        for line in ["EnsurePilotAcquisition()", "executionRun?.Checkpoint(ExecutionStage.Acquisition)",
                     "ExcelDnaDataImporter.ImportInto(columData, ref1, True)",
                     "ExcelDnaDataImporter.ImportInto(byIdData, refFinal, True, 0)"]:
            self.assertEqual(before.count(line), current.count(line))
        self.assertLess(current.index("ImportInto(columData"), current.index("CaptureColumn(columData)"))
        self.assertLess(current.index("CaptureColumn(columData)"), current.index("\n            Next"))

    def test_common_has_no_host_or_new_parser_dependency(self):
        policy = (ROOT / "BESHStat.Excel.Common/src/Input/UnivariateSelectionPolicy.vb").read_text()
        self.assertTrue(policy.startswith("Option Explicit On\nOption Strict On"))
        for forbidden in ["ExcelDna", "Microsoft.Office", "System.Windows.Forms", "Microsoft.VisualBasic", "System.IO", "AppGlobals"]:
            self.assertNotIn(forbidden, policy)
        self.assertIn("ConfigurableGroupedNumericParser.Parse(input, options)", policy)
        self.assertIn("imported.DataByID2ByColumn()", policy)

    def test_bridge_is_pure_and_has_no_write_or_acquisition_capability(self):
        bridge = (ROOT / "BESHstatNG/src/DM/WindowsUnivariateInputAdapter.vb").read_text()
        for forbidden in ["AppGlobals", "Interop", "ExcelDna", "System.Windows.Forms", "ImportInto(", "MsgBox("]:
            self.assertNotIn(forbidden, bridge)
        for call in ["CaptureImportedColumn", "SelectCapturedColumns", "SelectImportedById"]:
            self.assertIn("UnivariateSelectionPolicy." + call, bridge)

    def test_native_gate_populations_and_exact_touched_ids_are_retained(self):
        proposal = json.loads((ROOT / "docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json").read_bytes())
        cases = {case["case_id"]: case for case in proposal["case_transfers"]}
        first = cases["04A02-ANOVA-NATIVE-01"]
        second = cases["04A02-NORMALITY-NATIVE-01"]
        self.assertEqual(141, len(set(first["blocked_control_ids"])))
        self.assertEqual(160, len(set(first["deferred_obligation_ids"])))
        self.assertEqual(set(first["blocked_control_ids"]), set(second["blocked_control_ids"]))
        obligations = {row["obligation_id"]: row for row in proposal["obligation_transfers"]}
        for suffix in ["btCompute", "RefEdit1", "RefEdit2", "optByID", "optByColumn"]:
            row = obligations["control:BESHStatNG.UibyID." + suffix]
            self.assertFalse(row["capture_authorized"])
            self.assertEqual("OPEN", row["closure_status"])
            self.assertIn("exact case/criterion/ID-bound owner deferral", row["pre_change_gate"])


if __name__ == "__main__":
    unittest.main(verbosity=2)
