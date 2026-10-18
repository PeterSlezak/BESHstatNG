"""Batch 00.02A read-only source reconciliation regression tests.

Run from the repository root:
    python -m unittest discover -s BESHstatNG/tools/tests -p 'test_*.py' -v
"""
from __future__ import annotations

import copy
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[3]
TOOLS = ROOT / "BESHstatNG/tools"
sys.path.insert(0, str(TOOLS))
from validate_methods_catalog import _load_yaml, validate_catalog  # noqa: E402

YAML_PATH = ROOT / "BESHStatNG_Help_MkDocs/docs/_meta/methods.yml"


class MethodsCatalogValidationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.original = _load_yaml(YAML_PATH)

    def validate_changed(self, updater):
        doc = copy.deepcopy(self.original)
        updater(doc)
        with tempfile.TemporaryDirectory() as directory:
            candidate = Path(directory) / "methods.yml"
            candidate.write_text(yaml.safe_dump(doc, allow_unicode=True, sort_keys=False), encoding="utf-8")
            return validate_catalog(ROOT, candidate)[0]

    def test_current_manifest_reconciles_to_all_deployed_buttons(self):
        errors, counts = validate_catalog(ROOT)
        self.assertEqual([], errors, "\n".join(errors))
        # Counts freeze the 14 Oct checkpoint here, not in the reusable validator.
        self.assertEqual(76, counts["methods"])
        self.assertEqual(2, counts["pages"])
        self.assertEqual(12, counts["categories"])
        self.assertEqual(79, counts["method_button_mappings"])
        self.assertEqual(84, counts["ribbon_buttons"])
        self.assertEqual(5, counts["utility_commands"])
        self.assertEqual(79, counts["curated_navigation_entries"])

    def test_registered_ids_are_stable_and_only_two_are_executable(self):
        ids = {m["key"]: m["analysisId"] for m in self.original["methods"] if "analysisId" in m}
        self.assertEqual({"OneWayANOVA": "anova.oneway",
                          "KruskalWallisTest": "nonparametric.kruskal-wallis"}, ids)

    def test_regression_bullet_agreement_and_multiple_category_links(self):
        methods = {m["key"]: m for m in self.original["methods"]}
        expected = {
            "BulletChart": ("btm2_16Bullet", "OnbtmBulletPressed"),
            "BlandAltman": ("btn10_3Bland", "OnbtmBlandAltman"),
            "LinsCCC": ("btn10_4LCCC", "OnbtmLCCC"),
            "CohensKappa": ("btn10_5Cohen", "OnbtmCohenKappa"),
            "IntraclassCorrelationCoefficients": ("btn10_6icc", "OnbtmIcc"),
        }
        for key, (rid, action) in expected.items():
            with self.subTest(key=key):
                self.assertEqual([rid], methods[key]["ribbonIds"])
                self.assertEqual([action], methods[key]["onActions"])
                self.assertEqual(action, methods[key]["forms"][0]["handler"])
        nav = [item["ref"] for group in self.original["categories"] for item in group["items"]]
        self.assertIn("BulletChart", nav)
        for ref in ["KaplanMeierPlot", "CorrespondenceAnalysis", "CoxRegression"]:
            self.assertEqual(2, nav.count(ref))

    def test_validator_rejects_wrong_ribbon_callback(self):
        errors = self.validate_changed(lambda m: next(x for x in m["methods"] if x["key"] == "BulletChart")["onActions"].__setitem__(0, "Ui01BulletChart"))
        self.assertTrue(any("callback" in e for e in errors), errors)

    def test_validator_rejects_duplicate_ribbon_mapping(self):
        errors = self.validate_changed(lambda m: next(x for x in m["methods"] if x["key"] == "BlandAltman")["ribbonIds"].__setitem__(0, "btn10_4LCCC"))
        self.assertTrue(any("assigned twice" in e for e in errors), errors)

    def test_validator_rejects_bad_navigation_slug(self):
        errors = self.validate_changed(lambda m: next(x for x in m["categories"] if x["name"] == "Graphics")["items"][-1].__setitem__("slug", "wrong-slug"))
        self.assertTrue(any("stale slug" in e for e in errors), errors)

    def test_validator_rejects_unknown_form(self):
        errors = self.validate_changed(lambda m: next(x for x in m["methods"] if x["key"] == "OneWayANOVA")["forms"][0].__setitem__("form", "NonexistentTestForm"))
        self.assertTrue(any("unknown WinForms class" in e for e in errors), errors)

    def test_validator_rejects_ambiguous_form_title(self):
        def mutate(m):
            next(x for x in m["methods"] if x["key"] == "OneWayANOVA")["forms"][0]["analysisTitle"] = "Kruskal-Wallis Test"
        errors = self.validate_changed(mutate)
        self.assertTrue(any("Ambiguous HelpContext" in e for e in errors), errors)

    def test_validator_rejects_unknown_analysis_id(self):
        errors = self.validate_changed(lambda m: next(x for x in m["methods"] if x["key"] == "OneWayANOVA").__setitem__("analysisId", "anova.unregistered"))
        self.assertTrue(any("not present in compiled AnalysisIds" in e for e in errors), errors)

    def test_validator_rejects_missing_utility_command(self):
        errors = self.validate_changed(lambda m: m["commands"].pop())
        self.assertTrue(any("Utility commands mismatch" in e for e in errors), errors)

    def test_validator_rejects_duplicate_yaml_keys(self):
        with tempfile.TemporaryDirectory() as directory:
            candidate = Path(directory) / "methods.yml"
            candidate.write_text("site:\n  baseUrl: first\n  baseUrl: second\n", encoding="utf-8")
            errors, _ = validate_catalog(ROOT, candidate)
        self.assertTrue(any("Duplicate YAML mapping key" in e for e in errors), errors)

    def test_help_generator_produces_same_checked_in_content(self):
        # Only Ribbon IDs/actions/curated navigation and optional IDs changed;
        # HelpTopic enum and form/title map must remain byte-equivalent except
        # for the existing generator's non-deterministic Generated timestamp.
        with tempfile.TemporaryDirectory() as directory:
            result = subprocess.run([
                sys.executable, str(TOOLS / "generate_help_links.py"),
                "--methods-yml", str(YAML_PATH), "--project-dir", directory,
            ], capture_output=True, text=True)
            self.assertEqual(0, result.returncode, result.stdout + "\n" + result.stderr)
            for name in ("HelpLinks.vb", "HelpContext.vb"):
                with self.subTest(name=name):
                    checked_in = (ROOT / "BESHstatNG/src/Help" / name).read_text(encoding="utf-8").splitlines()
                    generated = (Path(directory) / "src/Help" / name).read_text(encoding="utf-8").splitlines()
                    clean = lambda lines: [line for line in lines if not line.startswith("' Generated:")]
                    self.assertEqual(clean(checked_in), clean(generated))


if __name__ == "__main__":
    unittest.main()
