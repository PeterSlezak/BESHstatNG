"""Safety boundaries for the 04A.02 execution helper; no historical paths written."""
import importlib.util
import json
from pathlib import Path
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location('isolation04a02', Path(__file__).resolve().parents[1] / 'verify-04a02-fixtures.py')
isolation = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(isolation)


class MigrationFixtureIsolationTests(unittest.TestCase):
    def test_historical_and_nested_roots_are_rejected(self):
        with tempfile.TemporaryDirectory() as folder, patch.object(isolation, 'ROOT', Path(folder)):
            for path in ['TestResults/Codex', 'TestResults/04A-01-old', 'TestResults/04A-02-initial-fixtures-x/nested']:
                with self.subTest(path=path), self.assertRaises(ValueError):
                    isolation.evidence(Path(folder) / path)

    def test_protection_snapshot_is_required(self):
        with tempfile.TemporaryDirectory() as folder, patch.object(isolation, 'ROOT', Path(folder)):
            path = Path(folder) / 'TestResults/04A-02-initial-fixtures-new'
            path.mkdir(parents=True)
            with self.assertRaises(ValueError):
                isolation.evidence(path)
            isolation.emit(path / 'start.json', {})
            isolation.emit(path / 'historical-metadata.json', [])
            self.assertEqual(path, isolation.evidence(path))

    def test_evidence_cannot_overwrite(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'evidence.json'
            isolation.emit(path, {'status': 'candidate'})
            before = path.read_bytes()
            with self.assertRaises(FileExistsError):
                isolation.emit(path, {'status': 'approved'})
            self.assertEqual(before, path.read_bytes())

    def test_four_parent_writers_stay_within_sandbox(self):
        root = Path('unique-execution/sandbox').resolve()
        base = root / 'Common/bin/Debug/net10.0'
        for relative in ['../../../../TestResults/Codex/Batch0205/Wire', '../../../../TestResults/Codex/Batch0207']:
            self.assertTrue((base / relative).resolve().is_relative_to(root))

    def template(self):
        return (Path(__file__).resolve().parents[1] / 'verify-beshstatng.ps1').read_text(encoding='utf-8')

    def test_prepare_uses_unique_anchors_and_disables_every_implicit_restore(self):
        original = self.template()
        with tempfile.TemporaryDirectory() as folder, patch.object(isolation, 'ROOT', Path(folder)):
            run = Path(folder) / 'TestResults/04A-02-initial-fixtures-scratch'
            prepared = isolation.transform_script(original, run, 'Debug', 'scratch-Debug')
            self.assertIn("$root = (Get-Location).Path", prepared)
            self.assertIn("TestResults/04A-02-initial-fixtures-scratch/Debug/scratch-Debug/results", prepared)
            self.assertNotIn("$results = Join-Path $root 'TestResults\\Codex'", prepared)
            self.assertIn("'scratch-Debug-Common'", prepared)
            self.assertIn("'scratch-Debug-Legacy'", prepared)
            commands = [line for line in prepared.splitlines() if line.lstrip().startswith("Invoke-Checked 'dotnet'")]
            self.assertEqual(6, len(commands))
            self.assertTrue(all("'--no-restore'" in line for line in commands))
            self.assertEqual(original, self.template())

    def test_prepare_rejects_every_missing_anchor(self):
        original = self.template()
        run = isolation.ROOT / 'TestResults/04A-02-initial-fixtures-scratch'
        for key, anchor in isolation.SCRIPT_ANCHORS.items():
            with self.subTest(anchor=key), self.assertRaisesRegex(ValueError, 'exactly one unchanged'):
                isolation.transform_script(original.replace(anchor, ''), run, 'Debug', 'scratch')

    def test_prepare_rejects_every_duplicate_anchor(self):
        original = self.template()
        run = isolation.ROOT / 'TestResults/04A-02-initial-fixtures-scratch'
        for key, anchor in isolation.SCRIPT_ANCHORS.items():
            with self.subTest(anchor=key), self.assertRaisesRegex(ValueError, 'exactly one unchanged'):
                isolation.transform_script(original + '\n' + anchor, run, 'Debug', 'scratch')

    def test_prepare_rejects_every_altered_anchor(self):
        original = self.template()
        run = isolation.ROOT / 'TestResults/04A-02-initial-fixtures-scratch'
        for key, anchor in isolation.SCRIPT_ANCHORS.items():
            with self.subTest(anchor=key), self.assertRaisesRegex(ValueError, 'exactly one unchanged'):
                isolation.transform_script(original.replace(anchor, anchor + ' # altered scratch'), run, 'Debug', 'scratch')

    def test_prepare_rejects_added_restore_command(self):
        with self.assertRaisesRegex(ValueError, 'Unexpected dotnet command'):
            isolation.transform_script(self.template() + "\nInvoke-Checked 'dotnet' @('restore', 'unexpected.csproj')",
                                       isolation.ROOT / 'TestResults/04A-02-initial-fixtures-scratch', 'Debug', 'scratch')

    def test_prepare_rejects_path_and_configuration_injection(self):
        original = self.template()
        run = isolation.ROOT / 'TestResults/04A-02-initial-fixtures-scratch'
        for name in ['../old', "quote'name", 'with space', 'x/y', '', 'x;restore']:
            with self.subTest(name=name), self.assertRaises(ValueError):
                isolation.transform_script(original, run, 'Debug', name)
        with self.assertRaises(ValueError):
            isolation.transform_script(original, run, 'PreRelease', 'scratch')
        with self.assertRaises(ValueError):
            isolation.transform_script(original, isolation.ROOT / 'TestResults/Codex', 'Debug', 'scratch')

    def scratch_run(self, root):
        run = root / 'TestResults/04A-02-initial-fixtures-scratch'
        isolation.emit(run / 'start.json', {'source_pins': {}})
        isolation.emit(run / 'historical-metadata.json', [])
        return run

    def test_prepare_rejects_source_hash_drift_before_writing(self):
        with tempfile.TemporaryDirectory() as folder, patch.object(isolation, 'ROOT', Path(folder)):
            root = Path(folder)
            source = root / 'scripts/verify-beshstatng.ps1'
            source.parent.mkdir()
            source.write_text(self.template(), encoding='utf-8')
            run = self.scratch_run(root)
            args = SimpleNamespace(evidence=run, configuration='Debug', name='new')
            with self.assertRaisesRegex(ValueError, 'source hash differs'):
                isolation.prepare(args)
            self.assertFalse((run / 'Debug').exists())

    def test_assembly_dependencies_and_testdata_are_hash_verified(self):
        self.check_clone(corrupt=False)

    def test_corrupt_dependency_clone_refuses_test_execution(self):
        self.check_clone(corrupt=True)

    def check_clone(self, corrupt):
        with tempfile.TemporaryDirectory() as folder, patch.object(isolation, 'ROOT', Path(folder)):
            root = Path(folder)
            run = self.scratch_run(root)
            source = root / 'BESHStat.Excel.Common.Tests/bin/Debug/net10.0'
            source.mkdir(parents=True)
            (source / 'BESHStat.Excel.Common.Tests.dll').write_bytes(b'scratch assembly')
            (source / 'dependency.dll').write_bytes(b'scratch dependency')
            (source / 'TestData').mkdir()
            (source / 'TestData/input.json').write_text('{}')
            args = SimpleNamespace(action='common', evidence=run, configuration='Debug', name='new', filter=None, vstest=None)
            copytree = isolation.shutil.copytree
            def copied_tree(src, dst, *positional, **kwargs):
                result = copytree(src, dst, *positional, **kwargs)
                if corrupt and Path(src) == source:
                    (Path(dst) / 'dependency.dll').write_bytes(b'non-newline scratch drift')
                return result
            with patch.object(isolation.shutil, 'copytree', side_effect=copied_tree), patch.object(isolation.subprocess, 'run', return_value=SimpleNamespace(returncode=0)) as execute:
                if corrupt:
                    with self.assertRaisesRegex(ValueError, 'copy mismatch'):
                        isolation.common(args)
                    execute.assert_not_called()
                else:
                    self.assertEqual(0, isolation.common(args))
                    execute.assert_called_once()
                    record = json.loads((run / 'executions/new/execution.json').read_bytes())
                    self.assertEqual({'BESHStat.Excel.Common.Tests.dll', 'dependency.dll', 'TestData/input.json'}, {p['path'] for p in record['copied']})


if __name__ == '__main__':
    unittest.main(verbosity=2)
