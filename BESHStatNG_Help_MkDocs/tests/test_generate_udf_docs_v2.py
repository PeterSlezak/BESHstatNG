"""Batch 00.02B: source exports are exhaustively represented in generated UDF docs."""
from __future__ import annotations
from collections import Counter
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
DOC_ROOT = ROOT / 'BESHStatNG_Help_MkDocs'
VB_ROOT = ROOT / 'BESHstatNG/src/ExcelUDFs'
METHODS = DOC_ROOT / 'docs/_meta/methods.yml'
LEGACY_LINKS = DOC_ROOT / 'udf_method_links.yml'
sys.path.insert(0, str(DOC_ROOT))
import generate_udf_docs_v2 as udf  # noqa: E402


def export_names_from_source():
    """An independent attribute-token scan, rather than reusing parse_udfs_from_vb."""
    names = []
    attrs = 0
    for path in sorted(VB_ROOT.rglob('*.vb')):
        source = path.read_text(encoding='utf-8-sig')
        for match in re.finditer(r'<ExcelFunction\((.*?)\)\s*>', source, flags=re.I | re.S):
            attrs += 1
            name = re.search(r'\bName\s*:=\s*"([^"]+)"', match.group(1))
            if not name:
                raise AssertionError(f'Missing exported name: {path}')
            names.append(name.group(1))
    return attrs, names


class GenerateUdfDocsTests(unittest.TestCase):
    def test_current_source_has_268_unique_exports_and_no_missing_udf_docs(self):
        count, names = export_names_from_source()
        self.assertEqual(268, count)
        self.assertEqual(count, len(set(names)))
        parsed = [doc for path in sorted(VB_ROOT.rglob('*.vb')) for doc in udf.parse_udfs_from_vb(path)]
        self.assertCountEqual(names, [doc.excel_name for doc in parsed])
        self.assertEqual(12, len(set(doc.category for doc in parsed)))
        expected = Counter(names)
        headings = Counter()
        for file in (DOC_ROOT / 'docs/udf').glob('*.md'):
            # The index and curated cookbook pages are not category output.
            if file.name in ('index.md', 'udf-cookbook.md', 'regression-formula-syntax.md'):
                continue
            headings.update(re.findall(r'^## (BESH\.[A-Za-z0-9_.]+)\s*$', file.read_text(encoding='utf-8'), re.M))
        self.assertEqual(expected, headings)
        for psm in ('BESH.PS.SCORES', 'BESH.PS.WEIGHTS', 'BESH.PS.EFFECT', 'BESH.PS.TABLE'):
            self.assertEqual(1, headings[psm])

    def test_parser_handles_two_adjacent_single_line_attributes(self):
        with tempfile.TemporaryDirectory() as folder:
            file = Path(folder) / 'Sample.vb'
            file.write_text('''''' + '''\n'''
                "''' <summary>First</summary>\n"
                '    <ExcelFunction(Name:="BESH.A", Category:="Test", Description:="A")>\n'
                '    Public Function First() As Object\n    End Function\n'
                "''' <summary>Second</summary>\n"
                '    <ExcelFunction(Name:="BESH.B", Category:="Test", Description:="B")>\n'
                '    Public Function Second() As Object\n    End Function\n', encoding='utf-8')
            docs = udf.parse_udfs_from_vb(file)
            self.assertEqual(['BESH.A', 'BESH.B'], [d.excel_name for d in docs])
            self.assertEqual(['First', 'Second'], [d.summary for d in docs])

    def test_methods_yml_has_six_explicit_valid_associations_and_rejects_unknown_udf(self):
        _, names = export_names_from_source()
        links = udf._load_methods_yml(METHODS, DOC_ROOT / 'docs', set(names))
        self.assertEqual(6, len(links))
        self.assertEqual(['methods/one-way-anova.md'], links['BESH.PAR.ANOVA1'])
        self.assertEqual(['methods/kruskal-wallis-test.md'], links['BESH.NP.KW_MCP'])
        with tempfile.TemporaryDirectory() as folder:
            broken = Path(folder) / 'bad.yml'
            broken.write_text('methods:\n- key: OneWayANOVA\n  slug: one-way-anova\n  udfNames:\n  - BESH.INVALID\n', encoding='utf-8')
            with self.assertRaisesRegex(ValueError, 'unknown exported'):
                udf._load_methods_yml(broken, DOC_ROOT / 'docs', set(names))
            broken.write_text('methods:\n- key: OneWayANOVA\n  slug: ../secret\n  udfNames:\n  - BESH.PAR.ANOVA1\n', encoding='utf-8')
            with self.assertRaisesRegex(ValueError, 'invalid slug'):
                udf._load_methods_yml(broken, DOC_ROOT / 'docs', set(names))

    def test_generated_markdown_is_reproducible_and_all_links_exist(self):
        script = DOC_ROOT / 'generate_udf_docs_v2.py'
        with tempfile.TemporaryDirectory() as folder:
            dest = Path(folder) / 'udf'
            base = [sys.executable, str(script), '--udfs-dir', str(VB_ROOT),
                    '--output-dir', str(dest), '--docs-root', str(DOC_ROOT / 'docs')]
            subprocess.run(base + ['--method-links-yaml', str(LEGACY_LINKS),
                                   '--methods-yml', str(METHODS)], check=True, capture_output=True)
            first = {p.name: p.read_bytes() for p in dest.glob('*.md')}
            self.assertEqual(13, len(first))  # 12 generated group pages + the overview
            subprocess.run(base + ['--method-links-yaml', str(LEGACY_LINKS),
                                   '--methods-yml', str(METHODS)], check=True, capture_output=True)
            self.assertEqual(first, {p.name: p.read_bytes() for p in dest.glob('*.md')})
            self.assertIn('## BESH.PS.SCORES', first['causal-inference.md'].decode('utf-8'))
            self.assertIn('One Way Anova', first['parametric.md'].decode('utf-8'))
            self.assertIn('Two Way Nested Anova', first['parametric.md'].decode('utf-8'))
            for file in dest.glob('*.md'):
                for relative in re.findall(r'\]\(([^)#]+\.md)\)', file.read_text(encoding='utf-8')):
                    self.assertTrue((file.parent / relative).is_file(), f'{file}: broken link {relative}')

    def test_legacy_and_canonical_flags_work_independently(self):
        script = DOC_ROOT / 'generate_udf_docs_v2.py'
        with tempfile.TemporaryDirectory() as folder:
            dest = Path(folder) / 'udf'
            base = [sys.executable, str(script), '--udfs-dir', str(VB_ROOT),
                    '--output-dir', str(dest), '--docs-root', str(DOC_ROOT / 'docs')]
            subprocess.run(base + ['--method-links-yaml', str(LEGACY_LINKS)], check=True, capture_output=True)
            old = (dest / 'parametric.md').read_text(encoding='utf-8')
            self.assertIn('Two Way Nested Anova', old)
            subprocess.run(base + ['--methods-yml', str(METHODS)], check=True, capture_output=True)
            new = (dest / 'parametric.md').read_text(encoding='utf-8')
            self.assertIn('One Way Anova', new)
            self.assertNotIn('Two Way Nested Anova', new)


if __name__ == '__main__':
    unittest.main()
