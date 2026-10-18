"""Batch 00.02B: reproducible, metadata-only VB source projection."""
from __future__ import annotations
import importlib.util
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[3]
TOOLS = ROOT / 'BESHstatNG/tools'
sys.path.insert(0, str(TOOLS))
import generate_analysis_metadata as generator  # noqa: E402


class GenerateAnalysisMetadataTests(unittest.TestCase):
    def test_generated_metadata_is_current_and_stable(self):
        expected = (ROOT / generator.OUTPUT_REL).read_text(encoding='utf-8')
        self.assertEqual(expected, generator.render_catalog(ROOT))
        self.assertEqual(expected, generator.render_catalog(ROOT))
        self.assertIn('New AnalysisMethodMetadata("OneWayANOVA", "anova.oneway"', expected)
        self.assertIn('New AnalysisMethodMetadata("KruskalWallisTest", "nonparametric.kruskal-wallis"', expected)
        self.assertNotIn('GetType(OneWayAnovaAnalysisOptions)', expected)
        self.assertEqual(76, expected.count('New AnalysisMethodMetadata('))

    def test_generator_check_mode_succeeds_then_detects_stale_output(self):
        proc = subprocess.run([sys.executable, str(TOOLS / 'generate_analysis_metadata.py'), '--check'],
                              capture_output=True, text=True, check=False)
        self.assertEqual(0, proc.returncode, proc.stdout + proc.stderr)
        # Pure check: a nonmatching file must fail without changing that file.
        with tempfile.TemporaryDirectory() as temp:
            wrong = Path(temp) / 'GeneratedMethodMetadata.vb'
            wrong.write_text('stale', encoding='utf-8')
            proc = subprocess.run([sys.executable, str(TOOLS / 'generate_analysis_metadata.py'),
                                   '--check', '--output', str(wrong)],
                                  capture_output=True, text=True, check=False)
            self.assertNotEqual(0, proc.returncode)
            self.assertEqual('stale', wrong.read_text(encoding='utf-8'))

    def test_vb_escaping_and_plain_text_description(self):
        self.assertEqual('"Title ""quoted"""', generator._vb_literal('Title "quoted"'))
        self.assertEqual('Nothing', generator._vb_literal(None))
        self.assertEqual('Includes: A B', generator._plain_description('**Includes:** A  \nB'))


if __name__ == '__main__':
    unittest.main()
