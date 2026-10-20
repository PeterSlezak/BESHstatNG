"""Batch 00.05 regression tests for retiring unused Windows host placeholders."""
from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / 'BESHstatNG/tools'))
from audit_transitional_officejs import (  # noqa: E402
    REQUIRED_PATHS, RETIRED_PATHS, audit_solution,
)


class TransitionalOfficeJsAuditTests(unittest.TestCase):
    def test_supplied_solution_uses_current_host_contracts(self):
        self.assertEqual([], audit_solution(ROOT))

    def test_each_planned_office_caller_contract_is_required(self):
        caller = Path('BESHStat.WasmSmoke/Pages/Home.razor')
        for missing_token in REQUIRED_PATHS[caller]:
            with self.subTest(missing_token=missing_token), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                for relative, tokens in REQUIRED_PATHS.items():
                    target = root / relative
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_text('\n'.join(token for token in tokens
                                                if relative != caller or token != missing_token), encoding='utf-8')
                issues = audit_solution(root)
                self.assertEqual(1, len(issues), issues)
                self.assertIn(repr(missing_token), issues[0])

    def test_retired_source_file_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / RETIRED_PATHS[0]
            source.parent.mkdir(parents=True, exist_ok=True)
            source.write_text('Public Class OfficeJsDataImporter\nEnd Class\n', encoding='utf-8')
            issues = audit_solution(root, require_host_paths=False)
            self.assertTrue(any('Retired file' in item for item in issues), issues)

    def test_project_reference_to_retired_writer_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            proj = root / 'BESHstatNG/BESHStatNG.vbproj'
            proj.parent.mkdir(parents=True, exist_ok=True)
            proj.write_text('<Compile Include="src\\DM\\ResultWriter\\OfficeJsResultWriter.vb" />', encoding='utf-8')
            issues = audit_solution(root, require_host_paths=False)
            self.assertTrue(any('OfficeJsResultWriter' in item for item in issues), issues)

    def test_referencing_removed_public_types_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / 'BESHstatNG/src/UI/Example.vb'
            source.parent.mkdir(parents=True, exist_ok=True)
            source.write_text('Dim writer As New GoogleSheetsResultWriter()\n', encoding='utf-8')
            issues = audit_solution(root, require_host_paths=False)
            self.assertTrue(any('GoogleSheetsResultWriter' in item for item in issues), issues)

    def test_missing_canonical_host_component_is_reported(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            issues = audit_solution(root)
            self.assertTrue(any('Current host contract file missing' in item for item in issues), issues)

    def test_historical_markdown_is_not_treated_as_live_reference(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            doc = root / 'docs/architecture/history.md'
            doc.parent.mkdir(parents=True, exist_ok=True)
            doc.write_text('OfficeJsDataImporter was retired.', encoding='utf-8')
            self.assertEqual([], audit_solution(root, require_host_paths=False))


if __name__ == '__main__':
    unittest.main()
