import json
import unittest
from verify_slice import ROOT, PREFIX, inspect as slice_proof
from verify_ownership import inspect as ownership


class CheckpointControls(unittest.TestCase):
    def mutate(self, relative, change, message, reader=slice_proof):
        path = PREFIX + relative
        data = json.loads((ROOT/path).read_bytes())
        change(data)
        with self.assertRaisesRegex(ValueError, message):
            reader(overrides={path:json.dumps(data).encode()})

    def current(self, change, message):
        self.mutate('inspection/final-v2/MethodMetadata38-Debug.json',change,message)

    def test_positive_partial(self):
        self.assertEqual(ownership()['historical'],dict(total=704,closed=199,OPEN=505))

    def test_rewritten_review(self):
        self.mutate('body-contracts.json',lambda d:d['contracts'][0].update(review='FORGED'),'Unreviewed/rewritten')

    def test_missing_original_source(self):
        path=json.loads((ROOT/(PREFIX+'body-contracts.json')).read_bytes())['contracts'][0]['original_snapshot']['path']
        with self.assertRaisesRegex(ValueError,'Original source dependency'):
            slice_proof(overrides={path:b'changed protected original'})

    def test_portable_source_dependency_changed(self):
        path='BESHStat.Excel.Common/src/Analysis/UdfCellConversion38.vb'
        with self.assertRaisesRegex(ValueError,'Portable source dependency'):
            slice_proof(overrides={path:(ROOT/path).read_bytes()+b'\nchanged'})

    def test_missing_native_catch(self):
        self.current(lambda d:next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.ExcelArgNumeric' and m['member_name']=='TryGetDouble').update(exceptions=[]),'real conversion catch')

    def test_hidden_portable_catch(self):
        self.current(lambda d:next(m for m in d['methods'] if m['member_name']=='ConvertFiniteNumber').update(exceptions=[{}]),'hidden catch/static')

    def test_hidden_portable_static(self):
        self.current(lambda d:next(m for m in d['methods'] if m['member_name']=='ConvertFiniteNumber').update(nonliteral_static_fields=1),'hidden catch/static')

    def test_missing_actual_consumer(self):
        def change(d):
            m=next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.ExcelArgNumeric' and m['member_name']=='TryGetDouble')
            next(i for i in m['instructions'] if '.ConvertFiniteNumber(' in str(i['operand']))['operand']='METHOD:Fake.Target()'
        self.current(change,'actual Windows-to-Common')

    def test_conversion_moved_outside_catch(self):
        self.current(lambda d:next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.ExcelArgNumeric' and m['member_name']=='TryGetDouble')['exceptions'][0].update(try_start=10000),'outside native catch')

    def test_native_Err_helper_changed(self):
        def change(d):
            m=next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.ExcelArgNumeric' and m['member_name']=='TryGetDouble')
            next(i for i in m['instructions'] if 'ClearProjectError' in str(i['operand']))['operand']='METHOD:Fake.Clear()'
        self.current(change,'Framework Err helpers')

    def test_preserved_ANOVA_body_changed(self):
        self.current(lambda d:next(m for m in d['methods'] if m['member_name']=='ANOVA1')['instructions'].pop(),'Unreviewed compiled body')

    def test_settings_dependency_changed(self):
        def change(d):
            next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.AppInfrastructure.AppGlobals' and m['member_name']=='get_PValuePresentation')['instructions'].pop()
        self.mutate('dependency-metadata-v7/current-Debug.json',change,'Settings/formatter/ANOVA/result')

    def test_original_binary_changed(self):
        self.mutate('inspection/final-v2/original-Debug.json',lambda d:d['assemblies'][0].update(sha256='0'*64),'Original metadata/source/IL')

    def test_export_signature_changed(self):
        self.mutate('inspection/final-v2/Inspect38-Debug.json',lambda d:d['exports'][0].update(effective_excel_name='FORGED'),'Export signature')

    def test_probe_Err_changed(self):
        self.mutate('cell-probe-after-Debug.json',lambda d:d['cases'][0]['after'].update(number=999),'probe drift')

    def test_current_output_byte_changed(self):
        row=json.loads((ROOT/(PREFIX+'compatibility-cells-v3.json')).read_bytes())['results'][0]
        with self.assertRaisesRegex(ValueError,'output mismatch'):
            slice_proof(overrides={row['current']['path']:b'FORGED'})

    def test_open_identity_cannot_be_forged_closed(self):
        self.mutate('historical-ownership.json',lambda d:next(r for r in d['members'] if not r['proof']).update(proof={'kind':'HOST_ONLY'}),'unsupported historical',ownership)

    def test_reviewed_identity_cannot_be_opened_silently(self):
        self.mutate('historical-ownership.json',lambda d:next(r for r in d['members'] if r['proof']).update(proof=None),'historical whole-body',ownership)

    def test_missing_current_fragment(self):
        self.mutate('current-ownership.json',lambda d:d['fragments'].pop(),'Missing branch/statement/inline',ownership)

    def test_missing_original_branch(self):
        self.mutate('original-fragment-parent-crosswalk.json',lambda d:d['rows'].pop(),'Original fragment lost',ownership)

    def test_false_branch_coverage(self):
        self.mutate('current-ownership.json',lambda d:next(r for r in d['fragments'] if r['review_status'].startswith('OPEN')).update(review_status='COVERED_BY_REVIEWED_BODY'),'Unsupported branch/inline',ownership)

    def test_unsupported_support_closure(self):
        self.mutate('current-ownership.json',lambda d:next(r for r in d['support'] if not r['fully_reconciled']).update(fully_reconciled=True),'declaration/host-only',ownership)

    def test_false_ready(self):
        self.mutate('ownership-summary.json',lambda d:d.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW',W3='STARTED'),'Unsupported READY',ownership)

    def test_missing_remaining_queue(self):
        self.mutate('remaining-work-queue.json',lambda d:d['members'].pop(),'Incomplete remaining queue',ownership)


if __name__=='__main__':
    unittest.main()
