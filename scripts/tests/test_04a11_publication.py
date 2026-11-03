"""Successor checks: public aliases are not historical on-disk filenames."""
import importlib.util
import json
from pathlib import Path
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location('publication11', Path(__file__).parents[1] / 'verify-04a11-publication.py')
p = importlib.util.module_from_spec(spec)
spec.loader.exec_module(p)


class Publication11Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.v = p.vault()
        cls.receipt = p.load(cls.v / 'private-receipt.json')

    def test_four_expected_private_raw_hashes(self):
        for name, expected in p.EXPECTED.items():
            raw = (self.v / 'originals' / p.OUT / name).read_bytes()
            self.assertEqual(p.sha(raw), expected)

    def test_exact_public_bytes_and_all_values_preserved(self):
        for path, record in self.receipt['raw_files'].items():
            raw = (self.v / 'originals' / path).read_bytes()
            public, mapping, _ = p.project(raw)
            self.assertEqual((p.ROOT / path).read_bytes(), public)
            self.assertEqual(mapping, record['mapping'])
            self.assertNotEqual(p.sha(public), p.sha(raw))

    def test_seventy_unique_aliases_per_file_and_shared_policy(self):
        mappings = [r['mapping'] for r in self.receipt['raw_files'].values()]
        self.assertTrue(all(m == mappings[0] for m in mappings))
        for mapping in mappings:
            self.assertEqual(len(mapping), 70)
            self.assertEqual(len(set(mapping.values())), 70)
            self.assertTrue(all(p.PLACEHOLDER in alias for alias in mapping.values()))

    def test_projection_is_deterministic(self):
        raw = (self.v / 'originals' / p.OUT / 'prechange-manifest.json').read_bytes()
        self.assertEqual(p.project(raw), p.project(raw))

    def test_collision_is_rejected(self):
        pins = {'tests/TestResults/SYNTHETIC_HOST_2026-11-03_{:02d}/capture.trx'.format(i): str(i) for i in range(70)}
        pins['tests/TestResults/REDACTED_OWNER_HOST_2026-11-03_00/capture.trx'] = 'collision'
        with self.assertRaisesRegex(AssertionError, 'collision'):
            p.project(p.encoded(dict(source_pins=pins)))

    def test_companion_marks_display_aliases_and_preserved_values(self):
        note = p.load(p.ROOT / p.OUT / 'public-sanitization.json')
        self.assertEqual(set(note['files']), set(self.receipt['raw_files']))
        self.assertTrue(all(r['redacted_keys'] == 70 for r in note['files'].values()))
        self.assertIn('not real filenames', note['policy'])

    def test_all_manifests_and_implementation_freeze(self):
        p.verify()
        with patch.object(p, 'vault', side_effect=AssertionError('Public mode must not read private originals')):
            p.public()

    def test_complete_candidate_privacy_scan(self):
        self.assertEqual(p.scan(), [])

    def test_accepted_dossier_and_dormant_patches_unchanged(self):
        for path, expected in self.receipt['initial_hashes'].items():
            if path.endswith(('.vb', '.cs', '.vbproj', '.patch')) or path == 'docs/architecture/batch-04A-11-owner-review.md':
                self.assertEqual(p.sha((p.ROOT / path).read_bytes()), expected, path)

    def test_raw_local_source_pins_and_native_writer_tails(self):
        raw_spec = importlib.util.spec_from_file_location('raw_contract11', p.ROOT / 'scripts/capture-04a11-contract.py')
        raw = importlib.util.module_from_spec(raw_spec)
        raw_spec.loader.exec_module(raw)
        class CollectedWithoutWriting(Exception):
            pass
        def compare_private(name, value):
            self.assertEqual(name, 'source-delta-final.json')
            self.assertEqual(raw.prior.sanitize(value), p.load(self.v / 'originals' / p.OUT / name))
            raise CollectedWithoutWriting()
        # Original source() reads actual raw local baseline pins. Intercept its
        # first output to compare against the private raw receipt, never aliases.
        raw.emit = compare_private
        with self.assertRaises(CollectedWithoutWriting):
            raw.source()


if __name__ == '__main__':
    unittest.main()
