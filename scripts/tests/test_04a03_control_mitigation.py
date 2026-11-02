"""Source/ledger adversaries and offline native gate tests; no Excel/native executor."""
import copy
import importlib.util
import json
from pathlib import Path
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("mitigation03", ROOT / "scripts/04a03_control_mitigation.py")
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)


class ControlMitigationSourceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.value = json.loads((m.DOC / "successor-candidates.json").read_bytes())
        amendment, _, _, cls.population = m.load_population()
        cls.originals = {row["obligation_id"]: row for row in amendment["obligation_transfers"]}
        cls.source = (ROOT / m.HOST).read_text(encoding="utf-8-sig")

    def test_exact_shared_denominators_and_targets(self):
        self.assertEqual([141, 160, 218, 239, 802, 1514], list(self.population.values()))
        m.validate_successor(self.value, self.originals)
        self.assertEqual(42, self.value["newly_checked_source_fields"])

    def test_help_is_only_an_obligation_not_a_blocked_control(self):
        help_row = next(row for row in self.value["targets"] if row["obligation_id"].endswith(".btnHelp"))
        self.assertFalse(help_row["blocked_control"])
        self.assertEqual(["P2"], help_row["risk_tiers"])
        self.assertEqual(20, sum(row["blocked_control"] for row in self.value["targets"]))

    def test_shared_double_count_rejected(self):
        value = copy.deepcopy(self.value)
        value["targets"] *= 2
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_missing_deferred_id_rejected(self):
        value = copy.deepcopy(self.value)
        value["targets"].pop()
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_case_coverage_change_rejected(self):
        value = copy.deepcopy(self.value)
        value["targets"][0]["case_ids"].pop()
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_nonblocked_help_inflation_rejected(self):
        value = copy.deepcopy(self.value)
        value["selected_blocked_controls"] = 21
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_open_to_released_without_owner_rejected(self):
        value = copy.deepcopy(self.value)
        value["targets"][0]["complete_obligation_status"] = "OWNER_RELEASED"
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_host_free_cannot_be_labeled_native(self):
        value = copy.deepcopy(self.value)
        value["targets"][0]["runtime_scope"] = "NATIVE_VERIFIED"
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)
        with self.assertRaisesRegex(ValueError, "No qualifying"):
            m.qualified_receipt(ROOT / "TestResults" / "nonexistent-controls-test-only-receipts")

    def test_fabricated_source_row_rejected(self):
        value = copy.deepcopy(self.value)
        value["targets"][0]["original_row_sha256"] = "0" * 64
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_population_reduction_requires_actual_owner_release(self):
        value = copy.deepcopy(self.value)
        value["populations_after"]["shared_blocked_controls"] -= 1
        with self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_p_or_native_gate_promotion_rejected(self):
        for key, state in [("P", "YES"), ("N1", "GO"), ("N2", "YES"), ("Gate_C2", "MET")]:
            value = copy.deepcopy(self.value)
            value[key] = state
            with self.subTest(key=key), self.assertRaises(ValueError): m.validate_successor(value, self.originals)

    def test_sealed_sources_are_reconciled_never_rewritten(self):
        for row in self.value["targets"]:
            self.assertEqual(row["accepted_pre_source_sha256"], row["post_source_sha256"])
            self.assertEqual(row["accepted_pre_source_sha256"], m.sha(ROOT / m.HOST))
            self.assertNotEqual(row["historical_source_pins"][1]["sha256"], row["post_source_sha256"])
        with patch.object(m, "sha", return_value="0" * 64), self.assertRaisesRegex(ValueError, "amendment drift"):
            m.load_population()

    def test_per_mode_consumers_and_branch_order(self):
        normality = m.method(self.source, "RunNormality")
        kw = m.method(self.source, "RunKruskallWalis")
        anova = m.method(self.source, "RunOneWayANOVAThroughSharedLayer")
        for name in ("ckWelch", "ckLSD", "ckBonferroni", "ckTukey", "ckGamesHowell"):
            self.assertIn("Me." + name + ".Checked", anova)
            self.assertNotIn("Me." + name, normality)
            self.assertNotIn("Me." + name, kw)
        for name in ("ckLevene", "ckBartlett", "ckFlignerKilleen", "ckSquaredRanks"):
            self.assertIn("Me." + name + ".Checked", anova)
            self.assertIn("Me." + name + ".Checked", kw)
            self.assertNotIn("Me." + name, normality)
        for body in (normality, kw, anova):
            self.assertLess(body.index("If Me.ckDescriptiveStatistics.Checked"), body.index("If Me.ckBoxPlot.Checked"))
        variance = m.method(self.source, "ComputeVarianceHomogeneity")
        positions = [variance.index("assumptions." + name) for name in
                     ("FlignerKilleenTEST", "LeveneTEST", "SquaredRanksTestVARIANCE", "BartlettTEST")]
        self.assertEqual(sorted(positions), positions)

    def test_constructor_native_boundary_and_help_route_are_explicit(self):
        constructor = m.method(self.source, "New")
        self.assertEqual(3, constructor.count(".ExcelConnector = AppGlobals.app"))
        self.assertEqual(1, constructor.count("Me.WireHelp(Me.btnHelp)"))
        context = (ROOT / "BESHstatNG/src/Help/HelpContext.vb").read_text()
        for title in ("one way anova", "kruskal wallis test", "normality"):
            self.assertIn('"UibyID|' + title + '"', context)
        # Actual shipping Tag is Integer; the resolver's typed-enum branch alone
        # is not proof of shipping title-map/browser delivery.
        self.assertIn("Me.Tag = tagn", constructor)
        self.assertIn("Dim formName = frm.GetType().Name", context)

    def test_projection_has_no_global_mutation_or_uninitialized_controls(self):
        tests = (ROOT / "BESHStatNG.Test/ControlMitigation/ControlMitigationProjectionTests.vb").read_text()
        for forbidden in ("FormatterServices", "GetUninitializedObject", "AppGlobals.app =", ".Show(", "Process.Start", "GetActiveObject"):
            self.assertNotIn(forbidden, tests)
        self.assertIn("New UibyIDControlProjection", tests)


class NativeGateNegativeTests(unittest.TestCase):
    def sample(self):
        context = dict(pid=101, start_utc_ticks=123, session_id=1, owner_sid="MOCK_TEST_SID", hwnd=202)
        return dict(reviewed_context=context), dict(context, foreground_hwnd=202, loaded_build_attribution_verified=True)

    def test_reviewed_hwnd_mismatch_rejected_before_any_native_action(self):
        plan, observed = self.sample()
        observed["hwnd"] = 203
        with patch.object(subprocess_placeholder, "run", side_effect=AssertionError("No launch")), self.assertRaisesRegex(ValueError, "hwnd"):
            m.reviewed_identity_gate(plan, observed)

    def test_supplied_source_hash_is_not_loaded_build_attribution(self):
        plan, observed = self.sample()
        observed["loaded_build_attribution_verified"] = False
        observed["source_sha256"] = "0" * 64
        with self.assertRaisesRegex(ValueError, "loaded XLL/source"):
            m.reviewed_identity_gate(plan, observed)

    def test_foreground_mismatch_rejected(self):
        plan, observed = self.sample()
        observed["foreground_hwnd"] = 999
        with self.assertRaisesRegex(ValueError, "Foreground"):
            m.reviewed_identity_gate(plan, observed)

    def test_boolean_numeric_identity_alias_rejected(self):
        plan, observed = self.sample()
        plan["reviewed_context"]["pid"] = 1
        observed["pid"] = True
        with self.assertRaisesRegex(ValueError, "type/value"):
            m.reviewed_identity_gate(plan, observed)

    def test_complete_synthetic_context_never_grants_N1(self):
        plan, observed = self.sample()
        with self.assertRaisesRegex(ValueError, "NO_GO"):
            m.reviewed_identity_gate(plan, observed)

    def test_native_packet_has_no_authority_or_attempts(self):
        packet = json.loads((m.DOC / "native-readonly-plan.json").read_bytes())
        self.assertEqual("NO_GO", packet["N1"])
        self.assertEqual(0, packet["native_attempts"])
        self.assertIsNone(packet["exact_owner_N1_decision"])
        self.assertIsNone(packet["independent_windows_com_review"]["signature"])
        self.assertEqual(3, len(packet["retained_C14_findings"]))

    def test_review_packet_source_pins_match_current_sources(self):
        packet = json.loads((m.DOC / "native-readonly-plan.json").read_bytes())
        for path, expected in packet["review_source_pins"].items():
            with self.subTest(path=path):
                self.assertEqual(expected, m.sha(ROOT / path))


# Real subprocess module is mocked only around the offline predicate. It is
# never called by that predicate; no native supervisor execute() is imported.
import subprocess as subprocess_placeholder

if __name__ == "__main__":
    unittest.main(verbosity=2)
