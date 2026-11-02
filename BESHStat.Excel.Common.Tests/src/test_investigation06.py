"""Host-free source contracts and nonstatistical R qualification receipts. No process launch."""
import copy
import json
from pathlib import Path
import tempfile
import unittest
import investigation06 as m

DOC = m.ROOT / 'docs/architecture/04A02'


def read(name):
    return json.loads((DOC / name).read_text(encoding='utf-8'))


def runtime_admit(value):
    if (value['status'] != 'QUALIFIED_FOR_READ_ONLY_PROBING' or
            value['actual_r_version'] != '4.5.1' or value['actual_stats_version'] != '4.5.1' or
            value['bitness'] != 64 or value['locale_qualified'] != 'C' or
            value['qualified_for_official_reference_execution'] is not False):
        raise ValueError('probe qualification mismatch')
    if value['reference_gate_state'] != {'S': 'PENDING', 'R': False, 'P': False, 'N': False}:
        raise ValueError('probe is not scientific approval')
    for run in value['clean_nonstatistical_probes']['commands']:
        if run['exit_code'] != 0 or run['stderr']: raise ValueError('not a clean probe')
        code = ' '.join(run['command'])
        if any(token in code for token in ['oneway.test', 'kruskal.test', 'anova(', 'lm(', 'pchisq(']):
            raise ValueError('statistical process outside probe scope')
    return True


class Investigation06Tests(unittest.TestCase):
    def setUp(self):
        self.trace = read('04A02-control-root-cause-matrix-v1.json')
        ready = read('04A02-G4-native-capture-ready-v1.json')
        self.ids = {i for case in ready['cases'] for i in case['deferred_obligation_ids']}

    def test_all_239_exact_ids_have_reversible_root_cause_source_maps(self):
        self.assertTrue(m.validate_trace(self.trace, self.ids))
        self.assertEqual(218, sum(x['is_in_blocked_set'] for x in self.trace['per_id']))
        self.assertEqual(239, len(self.trace['per_id']))
        self.assertEqual(11, len(self.trace['clusters']))

    def test_case_counts_deduplicate_shared_univariate_controls(self):
        cases = {x['case_id']: x for x in self.trace['case_cost_counts']}
        self.assertEqual(160, cases['04A02-ANOVA-NATIVE-01']['deferred_obligations'])
        self.assertEqual(160, cases['04A02-NORMALITY-NATIVE-01']['deferred_obligations'])
        self.assertEqual(3, cases['04A02-2X2-NATIVE-01']['deferred_obligations'])
        self.assertEqual(76, cases['04A02-PCA-NATIVE-01']['deferred_obligations'])
        self.assertTrue(all(x['actually_resolved_original_obligations'] == 0 for x in cases.values()))

    def test_commented_handles_are_not_executable_wiring(self):
        self.assertEqual('', m.active_vb("'Private Sub Old() Handles xlApp.WorkbookActivate"))
        self.assertIn('"owner\'s text"', m.active_vb('Dim x = "owner\'s text" \' comment'))
        members = m.member_inventory(m.ROOT / 'BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb')
        self.assertNotIn('xlApp_WorkbookActivate', [x['member'] for x in members])
        self.assertIn('xlSheetChangeHandle', [x['member'] for x in members])

    def test_known_compute_handler_and_passive_label_are_distinct(self):
        compute = m.control_inventory('UibyID', 'btCompute')
        self.assertTrue(any('Handles btCompute.Click' in x['text'] for x in compute['event_wiring']))
        label = m.control_inventory('UibyID', 'lblRefedit1')
        self.assertTrue(label['declarations'])
        self.assertEqual([], label['event_wiring'], 'Passive label is not an unwired command')

    def test_bulk_disable_and_pinned_addresses_are_existing_source_contracts(self):
        source = (m.ROOT/'BESHstatNG/src/UI/UibyID.vb').read_text(encoding='utf-8-sig')
        self.assertIn('For Each control As Control In Controls', source)
        self.assertIn('control.Enabled = False', source)
        execution = (m.ROOT/'BESHstatNG/src/AppInfrastructure/WindowsExecutionRun.vb').read_text()
        self.assertIn('BindInputAddresses', execution)
        self.assertIn('Lifecycle?.EnsureCurrent()', execution)

    def test_normality_5000_disagreement_preserved_as_review_defect(self):
        source = (m.ROOT/'BESHstatNG/src/UI/UibyID.vb').read_text(encoding='utf-8-sig')
        self.assertIn('data.X(i).Length < 5000', source)
        self.assertIn('NA n<4 or n>5000', source)

    def test_anova_defaults_mapping_remain_explicit_source_facts(self):
        for name in ['ckWelch', 'ckBoxPlot', 'ckDescriptiveStatistics']:
            row = m.control_inventory('UibyID', name)
            self.assertTrue(any('.Checked = True' in x['text'] for x in row['designer_defaults']))
        source = (m.ROOT/'BESHstatNG/src/UI/UibyID.vb').read_text()
        self.assertIn('.IncludeWelch = Me.ckWelch.Checked', source)

    def test_pca_has_three_biplot_scales_not_the_old_two_scale_proposal(self):
        source = (m.ROOT/'BESHstatNG/src/UI/Ui11PCA.vb').read_text()
        for value in ['0.0', '0.5', '1.0']:
            self.assertIn('PcaPlotExcel.Biplot(objPCA, '+value, source)

    def test_two_by_two_or_condition_and_no_chart_handler(self):
        members = m.member_inventory(m.ROOT/'BESHstatNG/src/UI/Ui82x2.vb')
        body = '\n'.join(x['active'] for x in next(x for x in members if x['member']=='btCompute_Click')['lines'])
        self.assertIn('Me.ckChi2.Checked Or Me.ckAssociation.Checked', body)
        self.assertNotIn('Charts.Add', body)
        self.assertNotIn('AddChart', body)

    def test_refedit_disposal_and_sheetactivate_guards_are_source_not_native_proof(self):
        source = (m.ROOT/'BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb').read_text()
        self.assertIn('Handles xlBook.SheetActivate', source)
        self.assertIn('If Me.IsDisposed OrElse Not Me.pbInTextAdressHandler Then Return', source)
        self.assertIn('Handles Me.Disposed', source)
        self.assertIn('RemoveHandler xlSheet.SelectionChange', source)
        self.assertFalse(self.trace['global_edges'][0]['native_0x800401A8_reproduced'])

    def test_unknown_high_and_progress_p0_not_removed(self):
        edge, progress = self.trace['global_edges']
        self.assertEqual('UNKNOWN_HIGH', edge['risk'])
        self.assertEqual('P0', progress['risk'])
        self.assertEqual(['04A.46'], progress['owners'])

    def test_runtime_receipt_is_actual_451_probe_only(self):
        value = read('04A02-R-runtime-qualification-v1.json')
        self.assertTrue(runtime_admit(value))
        for file in value['binary_file_metadata']:
            self.assertEqual(file['SHA256'].lower(), m.sha(file['Path']))
        self.assertFalse(value['historical_443_executed'])

    def test_pe_inspection_reads_real_64bit_rscript_without_loading_it(self):
        value = read('04A02-R-runtime-qualification-v1.json')
        path = Path(value['canonical_r_home'])/'bin/x64/Rscript.exe'
        self.assertEqual(64, m.pe_imports(path)['bitness'])
        self.assertTrue(m.pe_imports(path)['imports'])

    def test_pe_parser_rejects_malformed_bytes(self):
        with tempfile.TemporaryDirectory() as folder:
            p = Path(folder)/'bad.exe'
            for raw in [b'', b'not-a-pe', b'MZ', b'MZ'+b'\0'*200]:
                p.write_bytes(raw)
                with self.subTest(raw=raw[:4]), self.assertRaises((ValueError, UnicodeDecodeError)):
                    m.pe_imports(p)

    def test_missing_actual_compute_handler_inventory_is_rejected(self):
        value = copy.deepcopy(self.trace)
        row = next(x for x in value['per_id'] if x['control_id'] == 'BESHStatNG.UibyID.btCompute')
        self.assertTrue(row['source']['event_wiring'])
        row['source']['event_wiring'] = []
        with self.assertRaises(ValueError): m.validate_trace(value, self.ids)

    def test_reversed_authored_welch_default_is_rejected(self):
        value = copy.deepcopy(self.trace)
        row = next(x for x in value['per_id'] if x['control_id'] == 'BESHStatNG.UibyID.ckWelch')
        entry = next(x for x in row['source']['designer_defaults'] if '.Checked = True' in x['text'])
        entry['text'] = entry['text'].replace('True', 'False')
        with self.assertRaises(ValueError): m.validate_trace(value, self.ids)

    def test_pilot_source_gates_precede_every_native_boundary(self):
        source = (DOC/'04A02-GUI-readonly-pilot-proposal-v1.ps1').read_text()
        gate = source.index('if (-not $ExecuteApprovedReadOnlyPilot)')
        for boundary in ['Add-Type -AssemblyName UIAutomationClient', "::GetActiveObject('Excel.Application')", '::FromHandle(']:
            self.assertLess(gate, source.index(boundary))
        self.assertIn('readonly_com_context_check_authorized -ne $true', source)
        self.assertIn('Compare-Object $expected $actual', source)

    def test_pilot_is_readonly_and_does_not_launch_or_quit_excel(self):
        source = '\n'.join(l for l in (DOC/'04A02-GUI-readonly-pilot-proposal-v1.ps1').read_text().splitlines() if not l.lstrip().startswith('#'))
        for forbidden in ['New-Object -ComObject', 'Start-Process', 'taskkill', '.Quit(', '.Kill(', '.Invoke(', '.SetValue(']:
            self.assertNotIn(forbidden, source)
        self.assertIn('ActiveWorkbook', source)
        self.assertIn('$app.Workbooks.Count -ne 1', source)

    def test_all_four_gui_pilots_remain_proposed_with_complete_ids(self):
        plan = read('04A02-GUI-automation-feasibility-v1.json')
        ready = read('04A02-G4-native-capture-ready-v1.json')
        self.assertFalse(plan['native_permission'])
        self.assertEqual(4, len(plan['pilots']))
        for pilot in plan['pilots']:
            case = next(x for x in ready['cases'] if x['case_id'] == pilot['case_id'])
            self.assertEqual(case['blocked_control_ids'], pilot['blocked_control_ids'])
            self.assertEqual(case['deferred_obligation_ids'], pilot['deferred_obligation_ids'])
            self.assertFalse(pilot['native_executed'])
            self.assertFalse(pilot['owner_released'])

    def test_actual_r_package_and_loaded_component_pins(self):
        value = read('04A02-R-runtime-qualification-v1.json')
        root = Path(value['canonical_r_home'])
        for pin in value['runtime_components_proposed_for_owner_review']:
            self.assertEqual(pin['sha256'], m.sha(root/pin['relative_path']))
        self.assertFalse(value['qualified_for_official_reference_execution'])


def add_trace_negative(name):
    def test(self):
        value = copy.deepcopy(self.trace); row = value['per_id'][0]
        if name == 'missing_id': value['per_id'].pop()
        elif name == 'duplicate_id': value['per_id'].append(copy.deepcopy(row))
        elif name == 'owner': row['owner_roles'] = []
        elif name == 'group': row['clusters'] = ['invented']
        elif name == 'released': row['owner_released'] = True
        elif name == 'native': row['native_executed'] = True
        elif name == 'reviewed': row['mitigation_status'] = 'MITIGATION_REVIEWED'
        elif name == 'definition': row['source']['declarations'] = []
        elif name == 'hash': row['source']['sources'][0]['sha256'] = '0'*64
        elif name == 'defaults': row['source']['designer_defaults'].append(dict(line=1,text='invented default'))
        elif name == 'events': row['source']['event_wiring'].append(dict(line=1,text='invented event'))
        with self.assertRaises(ValueError): m.validate_trace(value,self.ids)
    setattr(Investigation06Tests,'test_trace_reject_'+name,test)


def add_runtime_negative(name):
    def test(self):
        value = read('04A02-R-runtime-qualification-v1.json')
        if name == '443': value['actual_r_version']='4.4.3'
        elif name == 'stats': value['actual_stats_version']='4.4.3'
        elif name == 'bitness': value['bitness']=32
        elif name == 'locale': value['locale_qualified']='C.UTF-8'
        elif name == 'stderr': value['clean_nonstatistical_probes']['commands'][0]['stderr']='warning'
        elif name == 'statistical': value['clean_nonstatistical_probes']['commands'][0]['command'].append('stats::oneway.test(x)')
        elif name == 'reference': value['qualified_for_official_reference_execution']=True
        elif name == 'gate': value['reference_gate_state']['R']=True
        with self.assertRaises(ValueError): runtime_admit(value)
    setattr(Investigation06Tests,'test_runtime_reject_'+name,test)


for name in ['missing_id','duplicate_id','owner','group','released','native','reviewed','definition','hash','defaults','events']:
    add_trace_negative(name)
for name in ['443','stats','bitness','locale','stderr','statistical','reference','gate']:
    add_runtime_negative(name)


if __name__ == '__main__':
    unittest.main(verbosity=2)
