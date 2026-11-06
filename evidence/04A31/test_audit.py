"""Adversarial publication/lineage checks; no file mutation and no native/oracle."""
import copy,json,unittest
from unittest.mock import patch
from audit import ROOT,HERE,sha,pin,NEW_CORE
import finalize

class Graphics31AuditTests(unittest.TestCase):
    def fixture(self):
        return [pin(ROOT/NEW_CORE[0]),dict(path='evidence/04A31/publication-manifest.json',bytes=None,sha256=None)]
    def test_exact_payload_and_self_entry(self):
        rows=self.fixture();finalize.check_manifest(rows,{r['path'] for r in rows})
    def test_duplicate_rejected(self):
        rows=self.fixture();rows.append(copy.deepcopy(rows[0]))
        with self.assertRaises(AssertionError):finalize.check_manifest(rows,{r['path'] for r in rows})
    def test_missing_rejected(self):
        rows=self.fixture()
        with self.assertRaises(AssertionError):finalize.check_manifest(rows[:-1],{r['path'] for r in rows})
    def test_wrong_hash_rejected(self):
        rows=self.fixture();rows[0]['sha256']='0'*64
        with self.assertRaises(AssertionError):finalize.check_manifest(rows,{r['path'] for r in rows})
    def test_wrong_size_rejected(self):
        rows=self.fixture();rows[0]['bytes']+=1
        with self.assertRaises(AssertionError):finalize.check_manifest(rows,{r['path'] for r in rows})
    def test_recursive_self_hash_rejected(self):
        rows=self.fixture();rows[1]['sha256']='0'*64
        with self.assertRaises(AssertionError):finalize.check_manifest(rows,{r['path'] for r in rows})
    def test_null_payload_hash_rejected(self):
        rows=self.fixture();rows[0]['sha256']=None;rows[0]['bytes']=None
        with self.assertRaises(AssertionError):finalize.check_manifest(rows,{r['path'] for r in rows})
    def test_path_escape_rejected_before_payload_read(self):
        for bad in ['../outside','/absolute','drive:payload','evidence//double','evidence/./dot','evidence\\backslash']:
            rows=[dict(path=bad,bytes=1,sha256='0'*64)]
            with patch.object(finalize,'pin',side_effect=RuntimeError('Must not read')):
                with self.assertRaises(AssertionError):finalize.check_manifest(rows,{bad})
    def test_accepted_engines_and_historical_authorities_preserved(self):
        records=json.loads((HERE/'source-pins.json').read_bytes())['files'];by={r['path']:r for r in records}
        files=[r for r in records if r['path'].startswith('BESHStat.Core/src/Graphics/')]
        self.assertEqual(18,len(files))
        files += [by[p] for p in ['AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','BESHStat.Excel.Common/src/Analysis/OrderedPlotGroups.vb','BESHStat.Core.Tests/src/Graphics/LegacyGraphics30.vb','BESHStat.Core.Tests/src/Graphics/Graphics30Tests.vb','BESHStat.Excel.Common.Tests/src/OrderedPlotGroups30Tests.vb','BESHStatNG.Test/src/Charts/Graphics30AdapterTests.vb','evidence/04A30/04A29-eol-qualification-v2.json','evidence/04A30/04A28-source-transition-successor-v2.json']]
        for row in files:self.assertEqual(row,pin(ROOT/row['path']))
    def test_successor_preserves_original_278_pins(self):
        s=json.loads((HERE/'source-transition-successor.json').read_bytes())
        self.assertEqual(278,len(s['pins']));self.assertEqual(272,s['unchanged_original'])
        self.assertEqual(4,s['retained_04A30_transitions']);self.assertEqual(2,s['further_04A31_transitions'])
        for row in s['pins']:
            if row['relation']=='UNCHANGED_ORIGINAL':self.assertEqual(row['original_04A28_sha256'],sha(ROOT/row['path']))
            else:self.assertEqual(row['final_04A31_sha256'],sha(ROOT/row['path']))
    def test_roslyn_renderer_and_portable_dependency_guard(self):
        guard=json.loads((HERE/'source-ownership.json').read_bytes())
        self.assertEqual('PASS',guard['status']);self.assertTrue(guard['renderers_only_coordinate_substitutions'])
        self.assertTrue(guard['existing_headers_preserved']);self.assertEqual(38,guard['compiled_graphics'])
        self.assertTrue(all(r['header_preserved'] for r in guard['members']))
    def test_full_publication_if_present(self):
        manifest=HERE/'publication-manifest.json'
        if not manifest.exists():self.skipTest('Pre-seal run;final post-seal execution required')
        rows=json.loads(manifest.read_bytes())['files']
        finalize.check_manifest(rows,set(finalize.publication_paths()))

if __name__=='__main__':unittest.main()
