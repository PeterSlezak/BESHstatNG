"""Independent scratch-only falsification of the supplemental diagnostic."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

SPEC = importlib.util.spec_from_file_location('audit04a02', Path(__file__).resolve().parents[1] / '04a02/audit_compatibility.py')
audit = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(audit)


class AuditCompatibilityDiagnosticTests(unittest.TestCase):
    def test_mixed_newline_drift_is_demonstrated_in_both_directions(self):
        old = b'[\r\n {"symbol_id":"A","value":1},\n {"symbol_id":"B","value":2}\r\n]\n'
        new = old.replace(b'\r\n', b'\n')
        result = audit.compare_bytes(old, new)
        self.assertFalse(result['byte_equal'])
        self.assertEqual(1, result['first_difference_offset'])
        self.assertEqual('BYTE_MISMATCH_NEWLINE_ONLY', result['outcome'])
        self.assertEqual('EQUAL', result['json_meaning'])
        self.assertTrue(result['reconstruct_original_using_current_bodies'])
        self.assertTrue(result['reconstruct_current_using_original_bodies'])
        self.assertEqual('BYTE_MISMATCH_NEWLINE_ONLY', audit.compare_bytes(new, old)['outcome'])

    def test_scratch_non_newline_change_cannot_be_explained_away(self):
        with tempfile.TemporaryDirectory() as folder:
            original = Path(folder) / 'original.json'
            changed = Path(folder) / 'changed.json'
            original.write_bytes(b'{"value":123}\r\n')
            changed.write_bytes(b'{"value":124}\n')
            result = audit.compare_bytes(original.read_bytes(), changed.read_bytes())
            self.assertEqual('NON_NEWLINE_DRIFT_DETECTED', result['outcome'])
            self.assertEqual('DIFFERENT', result['json_meaning'])
            self.assertFalse(result['normalized_crlf_to_lf_equal'])

    def test_source_hash_drift_detects_same_length_and_newline_changes(self):
        expected = audit.digest(b'original source\r\n')
        self.assertEqual('SOURCE_HASH_MATCH', audit.source_status(b'original source\r\n', expected))
        self.assertEqual('SOURCE_HASH_DRIFT_DETECTED', audit.source_status(b'Original source\r\n', expected))
        self.assertEqual('SOURCE_HASH_DRIFT_DETECTED', audit.source_status(b'original source\n', expected))

    def test_json_meaning_uses_exact_numbers_without_binary64_aliasing(self):
        self.assertNotEqual(audit.semantic_hash(b'{"n":9007199254740992}'), audit.semantic_hash(b'{"n":9007199254740993}'))
        self.assertNotEqual(audit.semantic_hash(b'{"n":1.00000000000000000000000000001}'), audit.semantic_hash(b'{"n":1}'))
        self.assertEqual(audit.semantic_hash(b'{"n":1.0}'), audit.semantic_hash(b'{"n":1}'))
        with self.assertRaises(ValueError):
            audit.semantic_hash(b'{"n":NaN}')

    def test_prefix_newlines_do_not_hide_order_or_content_drift(self):
        old = b'[\r\n{"symbol_id":"A",\r\n"value":1},\r\n{"symbol_id":"B",\r\n"value":2}\r\n]'
        current = old.replace(b'\r\n', b'\n')
        self.assertEqual('BYTE_MISMATCH_NEWLINE_ONLY', audit.compare_prefix(old, current)['outcome'])
        self.assertEqual('NON_NEWLINE_DRIFT_DETECTED', audit.compare_prefix(old, current.replace(b'"value":2', b'"value":3'))['outcome'])
        swapped = b'[{"symbol_id":"B","value":2},{"symbol_id":"A","value":1}]'
        self.assertFalse(audit.compare_prefix(old, swapped)['original_order_preserved'])
        self.assertEqual('NON_NEWLINE_DRIFT_DETECTED', audit.compare_prefix(old, swapped)['outcome'])

    def test_missing_local_evidence_is_unavailable_not_a_pass(self):
        with tempfile.TemporaryDirectory() as folder, patch.object(audit, 'ROOT', Path(folder)):
            result = audit.reconcile(Path(folder) / 'new-run')
            self.assertEqual('EVIDENCE_UNAVAILABLE', result['outcome'])
            self.assertFalse(result['original_failures_reclassified_as_pass'])
            self.assertIn('scripts/windows-migration-audit/control-review-decisions.json', result['unavailable'])

    def test_diagnostic_output_cannot_replace_existing_file(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / 'diagnostic.json'
            audit.emit(path, {'outcome': 'EVIDENCE_UNAVAILABLE'})
            before = path.read_bytes()
            with self.assertRaises(FileExistsError):
                audit.emit(path, {'outcome': 'PASS'})
            self.assertEqual(before, path.read_bytes())


if __name__ == '__main__':
    unittest.main(verbosity=2)
