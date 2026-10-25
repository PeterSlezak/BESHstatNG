import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
import xml.etree.ElementTree as ET
import struct

TOOL=Path(__file__).resolve().parents[1]/'generate_ribbon.py'
spec=importlib.util.spec_from_file_location('ribbon',TOOL);ribbon=importlib.util.module_from_spec(spec);spec.loader.exec_module(ribbon)

class RibbonTests(unittest.TestCase):
    def test_generated_outputs_are_reproducible(self):
        subprocess.run([sys.executable,'-B',str(TOOL),'--check'],check=True,capture_output=True)
    def test_every_occurrence_and_catalog_key_is_reconciled(self):
        result=ribbon.inventory();self.assertEqual(84,result['summary']['buttons']);self.assertEqual(79,result['summary']['methodOccurrences'])
        self.assertEqual(76,result['summary']['methods']);self.assertEqual(12,result['summary']['categories']);self.assertEqual(5,result['summary']['utilities'])
        self.assertEqual(3,len(result['repeatedMethods']));self.assertEqual([],result['unknownIds']);self.assertEqual([],result['omissions'])
    def test_unknown_ribbon_button_is_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'ribbon.xml';path.write_text(ribbon.RIBBON.read_text(encoding='utf-8-sig').replace('btm1_1Normaility','unknown-ribbon'),encoding='utf-8')
            with self.assertRaisesRegex(ValueError,'Unmapped'):ribbon.inventory(path)
    def test_callback_mismatch_is_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'ribbon.xml';path.write_text(ribbon.RIBBON.read_text(encoding='utf-8-sig').replace('OnbtmNormalityTestsPressed','UnknownCallback'),encoding='utf-8')
            with self.assertRaisesRegex(ValueError,'Callback mismatch'):ribbon.inventory(path)
    def test_authored_omission_is_rejected(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'ribbon.xml';tree=ET.parse(ribbon.RIBBON);ns='{http://schemas.microsoft.com/office/2009/07/customui}'
            for node in tree.iter():
                for child in list(node):
                    if child.get('id')=='btn00_1About':node.remove(child)
            tree.write(path,encoding='utf-8')
            with self.assertRaisesRegex(ValueError,'absent in XML'):ribbon.inventory(path)
    def test_icons_are_valid_png_at_manifest_dimensions(self):
        for size in (16,32,80):
            raw=(ribbon.ROOT/f'BESHStat.OfficeAddin/wwwroot/icons/ribbon-{size}.png').read_bytes()
            self.assertEqual(b'\x89PNG\r\n\x1a\n',raw[:8]);self.assertEqual((size,size),struct.unpack('>II',raw[16:24]));self.assertTrue(raw.endswith(b'IEND\xaeB`\x82'))
    def test_only_supported_methods_and_working_utilities_enabled(self):
        rows=ribbon.inventory()['occurrences'];active=[r for r in rows if r['available']]
        self.assertEqual(6,len(active));self.assertFalse(next(r for r in rows if r['catalogKey']=='ExportChart')['available'])
        self.assertEqual({'anova.oneway','nonparametric.kruskal-wallis'},{r['analysisId'] for r in active if r['kind']=='method'})
    def test_checked_in_ledger_matches_actual_source_hashes(self):
        expected=json.loads((ribbon.ROOT/'docs/architecture/batch-04-07-ribbon-parity.json').read_text(encoding='utf-8'))
        self.assertEqual(ribbon.inventory(),expected)

if __name__=='__main__':unittest.main()
