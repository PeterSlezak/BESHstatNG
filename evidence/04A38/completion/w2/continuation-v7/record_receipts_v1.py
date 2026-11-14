"""Authenticate all raw C7 command streams and retain every known first attempt."""
from common import *
rows=[];artifacts={}
for folder in sorted((RUNROOT/'commands').iterdir()):
    if not folder.is_dir() or not folder.name.startswith('w2c7-') or not (folder/'command.json').is_file():continue
    record=json.loads((folder/'command.json').read_bytes())
    for name in ['stdout','stderr']:assert sha((folder/(name+'.log')).read_bytes())==record[name+'_sha256']
    inputs=[]
    for arg in record['command']:
        if not isinstance(arg,str) or arg.startswith('/'):continue
        path=Path(arg)
        if not path.is_absolute():path=ROOT/path
        if path.is_file():
            key=str(path.resolve())
            if key not in artifacts:
                data=path.read_bytes();artifacts[key]=dict(name=path.name,bytes=len(data),sha256=sha(data))
            inputs.append(artifacts[key])
    rows.append(dict(name=folder.name,**clean(record),command_receipt=pin(folder/'command.json'),raw_stdout=pin(folder/'stdout.log'),raw_stderr=pin(folder/'stderr.log'),direct_file_arguments=inputs))
assert rows and not any(('vstest' in ' '.join(r['command']).lower() and 'Release' in ' '.join(r['command'])) for r in rows),'Unexpected Release unit suite'
save('command-receipts-v1.json',dict(commands=rows,scope='Machine-path-cleaned commands with exact unaltered local command/stdout/stderr pins. Full source pins and compiled/input/recorder artifacts are additionally in build/metadata/compatibility receipts. No raw diagnostic stream is altered or published.'))
save('attempt-accounting-v1.json',dict(status='FIRST_ATTEMPTS_RETAINED_NOT_WAIVED',attempts=[
    dict(name='Initial Git ownership rejection',classification='READ_ONLY_TOOLING',outcome='Rejected before -c safe.directory correction; no global Git configuration changed',raw='Initial conversation tool receipt; no standalone filesystem transcript is fabricated'),
    dict(name='Initial PowerShell Python -c quoting',classification='READ_ONLY_TOOLING',outcome='SyntaxError; corrected stdin script',raw='Conversation tool receipt'),
    dict(name='Read-only server interruption',classification='INCOMPLETE_ENTRY_PRIVACY_ATTEMPT',outcome='No completed receipt; live5639 payload hashes and standalone ZIP SHA/CRC independently verified before source edits',evidence='entry-v1.json'),
    dict(name='Initial focused Core target filename',classification='TOOLING_NO_TESTS_EXECUTED',exit_code=1,logs='commands/w2c7-packet-v1-Debug-test-Core',source='focused_v1.py',corrected='focused_v2.py'),
    dict(name='Initial expanded dependency scanner',classification='REJECTED_INCOMPLETE_METADATA_SCOPE',source='build_proof_inputs_v1.py',outcome='Required preference store, actual constructed/interface/embedded handles; corrected passive metadata-v3 and concrete554-method scope'),
    dict(name='Initial partial reader current-only support',classification='READER_DEFECT_CORRECTED',source='verify_checkpoint_template_v1.py',outcome='Rejected inherited current-only MaterializePortableReport38; corrected without historical ID invention in template_v2 and later sealed reader'),
    dict(name='Unbound signature-only lambda diagnostic',classification='REJECTED_DIAGNOSTIC_NOT_AUTHORITY',outcome='An ad-hoc key-only lookup reported16 false byte drifts. Exact parent/signature/body-token mapping proves0; the sealed reader enforces exact inherited whole-body byte hashes and concrete captured contexts.'),
    dict(name='Full Debug observation environment',classification='RUNNER_CONFIGURATION_DEFECT_CORRECTED_FOCUSED',first=dict(Passed=20700,Failed=18),correction=dict(Passed=4),effective=dict(Passed=20704,Failed=14),evidence='managed-environment-correction-v1.json',source='validate_managed_v1.py',no_second_full_run=True),
    dict(name='Four Python evidence-root scratch writers',classification='WRITE_GUARD_ERRORS_CORRECTED_IN_DISPOSABLE_COPY',first_extra_ERROR=4,corrected_PASS=4,evidence='python-environment-corrections-v1.json',assertions_changed=0),
    dict(name='Fixture crosswalk unused open constructed handles',classification='REJECTED_BINDER_SCOPE_ASSUMPTION',source='finish_inputs_v1.py',outcome='TypeError for unused None FullName; no claimed554-method dependency lacks a concrete type/field owner. Corrected explicit scope/crosswalk in finish_fixture_crosswalk_v2.py; no generic alias or original query.'),
    dict(name='First managed report',classification='REPORT_FAIL_CLOSED_ON_FIRST_RUN_DELTA',source='report_managed_v1.py',corrected='report_managed_v2.py',outcome='First four environment-induced failures preserved; only actual corrected executions qualify final outcomes')],original_queries=0,retained_historical_attempts='All C6 and prior manifests/readers/raw traces/failed builds remain immutable;562 C6 raw command streams independently rehashed.'))
print('C7 command streams authenticated',len(rows),flush=True)
