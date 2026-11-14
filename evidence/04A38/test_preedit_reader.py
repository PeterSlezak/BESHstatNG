"""Adverse tests for the separate pre-edit reader; original files never rewritten.

The private test view uses hard links for unchanged files. Every mutation writes
a NEW inode and atomically replaces only its checked private-view destination.
Failed views remain in the ignored batch workspace; no recursive cleanup.
"""
import contextlib
import hashlib
import json
import os
from pathlib import Path
import unittest
import preedit_reader as reader

ROOT=Path(__file__).resolve().parents[2]
PRIVATE=ROOT/'.test-rationalization-04A38.tmp'


class PreeditReaderAdverseTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.view=PRIVATE/'adverse-preedit-reader-v1'
        cls.view.mkdir(exist_ok=False)
        cls.files=[]
        here=ROOT/'evidence/04A38'
        selected=[here/x for x in ['entry-authority.json','entry-members.json','preedit/entry-source-snapshot.json','preedit/compiled-Debug.json','preedit/compiled-Release.json']]
        receipt=json.loads((here/'preedit/entry-source-snapshot.json').read_bytes())
        selected += [ROOT/r['snapshot']['path'] for r in receipt['files']]
        selected += [p for p in (here/'preedit/attempts/capture-v10').iterdir() if p.is_file()]
        for source in selected:
            destination=cls.view/source.relative_to(ROOT)
            destination.parent.mkdir(parents=True,exist_ok=True)
            os.link(source,destination)
            cls.files.append((source,hashlib.sha256(source.read_bytes()).hexdigest()))

    @classmethod
    def tearDownClass(cls):
        for source,expected in cls.files:
            assert hashlib.sha256(source.read_bytes()).hexdigest()==expected,'Original artifact was mutated'

    @contextlib.contextmanager
    def changed(self,relative,transform):
        target=self.view/'evidence/04A38'/relative
        self.assertTrue(target.resolve().is_relative_to(self.view.resolve()))
        original=target.read_bytes()
        replacement=target.with_name(target.name+'.new-inode')
        replacement.write_bytes(transform(original))
        os.replace(replacement,target)
        try:yield
        finally:
            replacement.write_bytes(original)
            os.replace(replacement,target)

    def rejects(self,relative,transform):
        with self.changed(relative,transform):
            with self.assertRaises((reader.InvalidReference,OSError,KeyError,ValueError)):
                reader.inspect(self.view)

    def test_untouched_original_view_passes(self):
        result,_=reader.inspect(self.view)
        self.assertFalse(result['production_authorized_by_this_reader'])
        self.assertEqual(result['configurations']['Debug']['callable_entries_reached'],643)

    def test_source_corruption_rejected(self):
        receipt=json.loads((self.view/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())
        relative=receipt['files'][0]['snapshot']['path'].removeprefix('evidence/04A38/')
        self.rejects(relative,lambda b:b+b'\n')

    def test_rehashed_source_inventory_rejected(self):
        self.rejects('preedit/entry-source-snapshot.json',lambda b:b.replace(b'sha256',b'sha257',1))

    def test_false_registration_census_rejected(self):
        self.rejects('entry-members.json',lambda b:b.replace(b'BESH.',b'FAKE.',1))

    def test_compiled_metadata_change_rejected(self):
        self.rejects('preedit/compiled-Debug.json',lambda b:b+b' ')

    def test_wrong_entry_commit_rejected(self):
        self.rejects('entry-authority.json',lambda b:b.replace(reader.BASE.encode(),b'0'*40))

    def test_case_omission_rejected(self):
        self.rejects('preedit/attempts/capture-v10/cases.jsonl',lambda b:b.partition(b'\n')[2])

    def test_case_duplicate_rejected(self):
        self.rejects('preedit/attempts/capture-v10/cases.jsonl',lambda b:b+b.partition(b'\n')[0]+b'\n')

    def test_case_reorder_rejected(self):
        self.rejects('preedit/attempts/capture-v10/cases.jsonl',lambda b:b'\n'.join(reversed(b.rstrip(b'\n').split(b'\n')))+b'\n')

    def test_case_descriptor_edit_rejected(self):
        self.rejects('preedit/attempts/capture-v10/cases.jsonl',lambda b:b.replace(b'"culture":""',b'"culture":"en-US"',1))

    def test_case_recorder_change_rejected(self):
        self.rejects('preedit/attempts/capture-v10/Capture38.cs.txt',lambda b:b+b'// fake\n')

    def test_original_result_change_rejected(self):
        self.rejects('preedit/attempts/capture-v10/capture-Debug-original-v1.tsv',lambda b:b.replace(b'RETURN_ARRAY',b'RETURN_TEXT_',1))

    def test_release_result_change_rejected(self):
        self.rejects('preedit/attempts/capture-v10/capture-Release-original-v1.tsv',lambda b:b+b'\n')

    def test_repeat_result_change_rejected(self):
        self.rejects('preedit/attempts/capture-v10/capture-Debug-repeat-v2.tsv',lambda b:b+b'\n')

    def test_trace_result_change_rejected(self):
        self.rejects('preedit/attempts/capture-v10/capture-Debug-trace-v1.tsv',lambda b:b+b'\n')

    def test_original_and_repeats_cannot_refresh_expected_output(self):
        base='preedit/attempts/capture-v10/capture-Debug-'
        with contextlib.ExitStack() as stack:
            for suffix in ['original-v1','repeat-v2','trace-v1']:
                stack.enter_context(self.changed(base+suffix+'.tsv',lambda b:b.replace(b'RETURN_ARRAY',b'RETURN_TEXT_',1)))
            with self.assertRaises(reader.InvalidReference):reader.inspect(self.view)

    def test_native_attempt_receipt_rejected(self):
        self.rejects('preedit/attempts/capture-v10/execution-Debug-original-v1.json',lambda b:b.replace(b'"native_attempts":0',b'"native_attempts":1'))

    def test_false_golden_receipt_rejected(self):
        self.rejects('preedit/attempts/capture-v10/execution-Debug-original-v1.json',lambda b:b.replace(b'NOT_GOLDEN',b'GOLDEN'))

    def test_false_outcome_accounting_rejected(self):
        self.rejects('preedit/attempts/capture-v10/execution-Debug-original-v1.json',lambda b:b.replace(b'"RETURN_ARRAY":3308',b'"RETURN_ARRAY":3309'))

    def test_trace_case_omission_rejected(self):
        self.rejects('preedit/attempts/capture-v10/capture-Debug-trace-v1.tsv.reachability.jsonl',lambda b:b.partition(b'\n')[2])

    def test_false_trace_member_rejected(self):
        self.rejects('preedit/attempts/capture-v10/capture-Debug-trace-v1.tsv.reachability.jsonl',lambda b:b.replace(b'BESHstatNG/src/',b'FAKE_OTHER/src/'))

    def test_path_traversal_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.safe_path(self.view,'../outside')

    def test_drive_path_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.safe_path(self.view,'C:/private')

    def test_windows_backslash_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.safe_path(self.view,'a\\b')

    def test_reserved_windows_path_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.safe_path(self.view,'evidence/CON.txt')

    def test_windows_trailing_dot_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.safe_path(self.view,'evidence/fake.')

    def test_essential_grid_missing_cell_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.grid([dict(cohort='x',a=1)],'x',['a'],[[1,2]])

    def test_essential_grid_unexpected_cell_rejected(self):
        with self.assertRaises(reader.InvalidReference):reader.grid([dict(cohort='x',a=1),dict(cohort='x',a=3)],'x',['a'],[[1,2]])


if __name__=='__main__':unittest.main(verbosity=2)
