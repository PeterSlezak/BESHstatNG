"""Authenticate C8 raw commands; retain failed attempts without relabelling them."""
from common import *
rows=[];artifacts={}
for folder in sorted((RUNROOT/'commands').iterdir()):
    if not folder.is_dir() or not folder.name.startswith('w2c8-') or not (folder/'command.json').is_file():continue
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
assert rows and not any(('vstest' in ' '.join(r['command']).lower() and 'Release' in ' '.join(r['command'])) for r in rows)
save('command-receipts-v1.json',dict(commands=rows,scope='Exact unaltered raw streams retained privately; cleaned review copies of command descriptions. Publication/copied-reader/post-seal commands occur later and have independent external receipts. No raw diagnostic stream altered.'))
save('attempt-accounting-v1.json',dict(status='FIRST_ATTEMPTS_RETAINED_NOT_WAIVED',attempts=[
dict(name='Git safe-directory and shell quoting rejections',classification='READ_ONLY_TOOLING',raw='Conversation tool receipts; no fabricated standalone transcript'),
dict(name='Entry v1 old-before pin routing',classification='READER_DEFECT_CORRECTED',source='entry_v1.py',corrected='entry_v2.py',outcome='Historical before pins verified against preserved preimages, current after pins against live source; no protected conflict'),
dict(name='Initial nested-class test insertion',classification='GENERATOR_REJECTED_BEFORE_WRITE',source='append_native_tests_v1.py',outcome='Corrected final module terminator targeting; old attempt retained'),
dict(name='Focused new native import assertions',classification='NEW_TEST_EXPECTATION_DEFECT_CORRECTED_FROM_SOURCE',outcome='Original GEE sorting/RowIds and original headered ordinary Cox text-strata rejection retained; only new assertions corrected twice; all initial TRX retained',evidence='focused-v3-packet-v1-Debug.json'),
dict(name='Source reader versions1-6',classification='FAIL_CLOSED_READER_DEVELOPMENT',outcome='Nested callback omission, CRLF reconstruction, field-list ordering, draft authority pointers and portable declaration coverage caught; precise corrections and all drafts retained'),
dict(name='Parallel scanner v1 then v2 proof',classification='BOUNDED_OPTIMIZATION_PROOF_EXPANDED',outcome='Added compound finding order and wrong-seed controls before full run; historical assertions/source unchanged'),
dict(name='Initial publication prototype',classification='INVENTORY_REVIEW_CORRECTED_BEFORE_SEAL',source='publish_v1.py',corrected='publish_v2.py',outcome='Explicitly include all three new root source/test files; no physical prototype publication claimed')],original_queries=0,retained_prior_attempts='All C7 and earlier source/evidence/raw history remains immutable, including four corrected C7 fixture cases and the C6-introduced 04A27 pin FAIL.'))
print('C8 raw commands authenticated',len(rows))
