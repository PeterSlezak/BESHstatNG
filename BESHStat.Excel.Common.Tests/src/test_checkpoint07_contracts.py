import ast
import copy
import json
from pathlib import Path
import struct
import tempfile
import unittest
import checkpoint07_contracts as c

DOC=c.ROOT/'docs/architecture/04A02'


def load(name):
    return json.loads((DOC/name).read_text(encoding='utf-8'))


class C07Contracts(unittest.TestCase):
    def test_all_239_field_dispositions_preserve_cases_and_restrictions(self):
        self.assertTrue(c.validate_dispositions(load('04A02-C07-exact-ID-disposition.json'),load('04A02-G4-native-capture-ready-v1.json')))

    def test_two_scientific_forms_have_complete_field_specific_policies(self):
        form=load('04A02-C07-scientific-decision-form.json')
        manifest=load('oracle-proposals/manifest-v2.json')
        self.assertEqual(2,len(form['cases']))
        for row in form['cases']:
            self.assertIsNone(row['explicit_domain_owner_answer'])
            self.assertEqual('PENDING',row['S'])
            self.assertFalse(row['R'] or row['P'] or row['N'])
            self.assertIsNone(row['numeric_expected_values'])
            fields=row['field_policies'];self.assertEqual(len(fields),len({f['field'] for f in fields}))
            actual_names={f['field'].split('[')[0] for f in fields}
            expected=set(next(x for x in manifest['cases'] if x['case_id']==row['case_id'])['metric_fields'])
            self.assertEqual(expected,actual_names)
            self.assertTrue(all(f['rationale'] and f['subject_mapping'] and f['proposed_atol']>=0 and f['proposed_rtol']>=0 for f in fields))
            self.assertLess(row['ordinary_p_range_proposal']['min'],row['ordinary_p_range_proposal']['max'])

    def test_private_environment_gap_is_real_and_patch_is_review_only(self):
        path=c.ROOT/'scripts/04a02/oracle_operator.py';before=path.read_bytes()
        source=before.decode('utf-8');patched=c.proposed_private_environment_patch(source)
        self.assertNotEqual(source,patched)
        for token in ['HOME=str(private_home)','R_USER=str(private_home)','R_LIBS_USER=str(private_library)',"private_home.mkdir()"]:
            self.assertIn(token,patched)
        ast.parse(patched)  # syntax only; do not import/execute proposed operator
        self.assertEqual(before,path.read_bytes())

    def test_changed_or_duplicate_private_env_anchor_fails_closed(self):
        source=(c.ROOT/'scripts/04a02/oracle_operator.py').read_text()
        with self.assertRaises(ValueError):c.proposed_private_environment_patch(source.replace('env.update(R_HOME=', 'env.update(CHANGED='))
        with self.assertRaises(ValueError):c.proposed_private_environment_patch(source+'\n'+source)

    def test_readiness_is_not_an_acl_or_scientific_release(self):
        ready=load('04A02-C07-runtime-output-readiness.json')
        self.assertEqual('NOT_READY_FOR_OFFICIAL_R',ready['status'])
        self.assertFalse(ready['R_process_executed_in_C07'])
        self.assertTrue(any(x['status'].startswith('FAIL_SOURCE') for x in ready['checklist']))
        self.assertTrue(all(x['status']=='PASS' for x in ready['binary_package_pins']))

    def test_missing_n0_inputs_are_precise_and_no_live_boundary_exists(self):
        record=load('04A02-C07-N0-dry-run-contract.json')
        result=c.assess_n0(record['request'])
        self.assertEqual(12,len(result['missing']))
        self.assertFalse(result['native_calls_allowed'])
        self.assertFalse(result['live_execution_authorized'])

    def test_boolean_attestation_is_not_an_authentic_n1_release(self):
        request=load('04A02-C07-N0-dry-run-contract.json')['request']
        keys=c.assess_n0(request)['missing']
        for key in keys:request[key]=True
        result=c.assess_n0(request)
        self.assertEqual([],result['missing'])
        self.assertFalse(result['live_execution_authorized'])
        self.assertIn('AUTHENTICITY_AND_RUNTIME_NOT_QUALIFIED',result['status'])

    def test_n1_or_n2_is_not_a_dry_run(self):
        for stage in ['N1','N2','CAPTURED']:
            with self.subTest(stage=stage),self.assertRaises(ValueError):c.assess_n0(dict(stage=stage,case_id='04A02-2X2-NATIVE-01'))

    def test_wrong_rot_pid_race_or_provider_policy_is_always_no_go(self):
        request=load('04A02-C07-N0-dry-run-contract.json')['request'];keys=c.assess_n0(request)['missing']
        for key in keys:request[key]=True
        for key in ['owned_pid_session_start_proof','exclusive_disposable_context_proof','provider_read_side_effect_policy','rot_or_exact_instance_binding_review','external_timeout_supervisor']:
            mutated=dict(request);mutated[key]=False
            with self.subTest(key=key):self.assertEqual('NO_GO_PENDING_EVIDENCE',c.assess_n0(mutated)['status'])

    def test_native_v1_source_cannot_claim_measured_no_state_change(self):
        source=(DOC/'04A02-GUI-readonly-pilot-proposal-v1.ps1').read_text()
        self.assertIn("::GetActiveObject('Excel.Application')",source)
        self.assertLess(source.index('::GetActiveObject('),source.index('$appRoot.Current.ProcessId'))
        self.assertIn('NoWorkbookWrite=$true',source)
        changes=load('04A02-C07-proposed-changes.json')
        self.assertTrue(any(x['id']=='CR-N1-ROT-READ-07' for x in changes['requests']))

    def test_source_shared_risk_receipt_is_not_direct_refedit_or_owner_review(self):
        record=load('04A02-C07-RefEdit-mitigation-receipt.json')
        self.assertEqual('PENDING_EXTERNAL_REVIEW',record['risk_owner_review']['status'])
        self.assertFalse(record['N_released'] or record['obligation_discharged'])
        self.assertEqual(9,record['test_count'])
        self.assertTrue(any('private RefEdit' in x for x in record['not_covered']))

    def test_screen_fixture_is_exact_oriented_input_not_numeric_reference(self):
        record=load('04A02-C07-N0-dry-run-contract.json')['screen_fixture']
        self.assertTrue(c.validate_screen_fixture(record))
        record['counts'][0][0]=0
        self.assertTrue(c.validate_screen_fixture(record)) # admission, not numeric zero-cell policy

    def test_delay_import_parser_on_synthetic_pe_not_dll_load(self):
        raw=bytearray(1024);raw[:2]=b'MZ';struct.pack_into('<I',raw,0x3c,0x80);raw[0x80:0x84]=b'PE\0\0'
        struct.pack_into('<HH',raw,0x84,0x8664,1);struct.pack_into('<H',raw,0x94,0xf0)
        opt=0x98;struct.pack_into('<H',raw,opt,0x20b);struct.pack_into('<Q',raw,opt+24,0x140000000);struct.pack_into('<I',raw,opt+108,16)
        struct.pack_into('<II',raw,opt+112+13*8,0x1000,64);struct.pack_into('<IIII',raw,opt+0xf0+8,0x200,0x1000,0x200,0x200)
        struct.pack_into('<IIIIIIII',raw,0x200,1,0x1080,0,0,0,0,0,0);raw[0x280:0x28c]=b'example.dll\0'
        with tempfile.TemporaryDirectory() as directory:
            p=Path(directory)/'mock.exe';p.write_bytes(raw)
            self.assertEqual(['example.dll'],c.delay_imports(p))
            p.write_bytes(b'not PE')
            with self.assertRaises(ValueError):c.delay_imports(p)


def disposition_negative(name):
    def test(self):
        value=load('04A02-C07-exact-ID-disposition.json');row=value['per_id'][0]
        if name=='missing':value['per_id'].pop()
        elif name=='duplicate':value['per_id'].append(copy.deepcopy(row))
        elif name=='case':row['cases']=[]
        elif name=='owner':row['owner_roles']=[]
        elif name=='released':row['N_released']=True
        elif name=='discharged':row['obligation_discharged']=True
        elif name=='native':row['native_evidence']['status']='CAPTURED'
        elif name=='risk_review':row['risk_owner_review']['status']='APPROVED'
        elif name=='definition':row['source_fields']['definition']['status']='UNKNOWN'
        with self.assertRaises(ValueError):c.validate_dispositions(value,load('04A02-G4-native-capture-ready-v1.json'))
    setattr(C07Contracts,'test_disposition_reject_'+name,test)


def screen_negative(name):
    def test(self):
        record=load('04A02-C07-N0-dry-run-contract.json')['screen_fixture']
        if name=='orientation':record['orientation'].reverse()
        elif name=='dimensions':record['counts'].pop()
        elif name=='negative':record['counts'][0][0]=-1
        elif name=='overflow':record['counts'][0][0]=2**31
        elif name=='float':record['counts'][0][0]=1.0
        elif name=='nan':record['counts'][0][0]=float('nan')
        elif name=='boolean':record['counts'][0][0]=True
        elif name=='alpha':record['alpha']=0
        elif name=='options':record['options']['Chi2']=True
        elif name=='reference':record['numeric_reference']=1
        elif name=='native':record['native_executed']=True
        with self.assertRaises(ValueError):c.validate_screen_fixture(record)
    setattr(C07Contracts,'test_screen_reject_'+name,test)


for name in ['missing','duplicate','case','owner','released','discharged','native','risk_review','definition']:disposition_negative(name)
for name in ['orientation','dimensions','negative','overflow','float','nan','boolean','alpha','options','reference','native']:screen_negative(name)


if __name__=='__main__':unittest.main(verbosity=2)
