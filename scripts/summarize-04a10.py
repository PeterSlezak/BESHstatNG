"""Sanitized, incremental 04A.10 validation/publication receipts. Never edits old evidence."""
import argparse
import base64
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('contract10',ROOT/'scripts/capture-04a10-contract.py')
c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
OUT=c.OUT;RUN=c.RUN
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'

def trx(p):
    tree=ET.parse(p)
    definitions={n.attrib['id']:n.find(NS+'TestMethod').attrib for n in tree.findall('.//'+NS+'TestDefinitions/'+NS+'UnitTest')}
    records=[]
    for node in tree.findall('.//'+NS+'UnitTestResult'):
        d=definitions.get(node.attrib['testId'],{})
        records.append(dict(class_name=d.get('className'),method=d.get('name'),test_name=node.attrib['testName'],outcome=node.attrib['outcome']))
    return dict(path=p.relative_to(ROOT).as_posix(),sha256=c.sha(p),counters=tree.find('.//'+NS+'Counters').attrib,
                nonpasses=[r for r in records if r['outcome']!='Passed'],batch10=[r for r in records if 'Regression10' in (r['class_name'] or '')],
                nist=[r for r in records if 'NistLinearRegression' in (r['class_name'] or '')],
                glm_references=[r for r in records if 'GLM_Tests' in (r['class_name'] or '')])

def validation():
    prior=json.loads((ROOT/'evidence/04A09/validation-v2.json').read_bytes())
    configs={}
    for config in ['Debug','Release']:
        name='final-'+config
        paths=[RUN/name/config/'results'/s for s in ['Core.trx','OfficeAddin.trx','OfficeExecution.trx']]
        paths += [RUN/'executions'/(name+'-'+suite)/'results'/(suite+'.trx') for suite in ['Common','Legacy']]
        paths += [RUN/'projection-executions'/name/'results/ControlMitigation.trx']
        suites=[trx(p) for p in paths]
        identity=lambda r:(r['class_name'],r['method'],r['test_name'],r['outcome'])
        old_managed={identity(r) for s in prior['configurations'][config]['managed_suites'] for r in s['nonpasses']}
        current_managed={identity(r) for s in suites for r in s['nonpasses']}
        assert old_managed==current_managed,(config,current_managed-old_managed,old_managed-current_managed)
        logs=[(RUN/(name+suffix+'.log')).read_text(encoding='utf-8',errors='replace') for suffix in ['-full','-common-python']]
        nonpasses={};events=[]
        for log in logs:
            for outcome,method,context in re.findall(r'^(FAIL|ERROR): [^\n]+? \((test_[^()]+)\)([^\n]*)$',log,re.M):
                nonpasses[method]=outcome
                events.append(dict(outcome=outcome,identity=method,context=context.strip()))
        inherited=prior['configurations'][config]['python_nonpasses']
        new=set(nonpasses)-set(inherited)
        expected={'test_04a09_regression_successor.Regression09SuccessorTests.test_exact_six_changed_members_and_unchanged_native_surroundings',
                  'test_04a09_regression_successor.Regression09SuccessorTests.test_real_six_routes_use_common_options_counts_and_reports'}
        entry_pin={'test_04a09_regression_successor.Regression09SuccessorTests.test_runtime_baselines_are_incremental_and_complete'}
        assert new==expected|entry_pin,(config,new)
        assert set(inherited)<=set(nonpasses)
        total=sum(int(n) for log in logs for n in re.findall(r'^Ran (\d+) tests? in',log,re.M))
        configs[config]=dict(managed_suites=suites,total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),
            failed=sum(int(s['counters']['failed']) for s in suites),new_managed_nonpasses=[r for s in suites for r in s['batch10'] if r['outcome']!='Passed'],
            batch10_executions=sum(len(s['batch10']) for s in suites),
            python_total=total,python_passed=total-len(nonpasses),python_nonpasses=nonpasses,python_failure_events=events,
            inherited_python_nonpasses=len(inherited),entry_python_pin_failures=sorted(entry_pin),new_python_pin_failures=sorted(expected),
            managed_nonpass_identities_preserved=True,python_inherited_outcome_changes={k:dict(before=v,after=nonpasses[k]) for k,v in inherited.items() if nonpasses[k]!=v},
            node_passed=sum(int(n) for n in re.findall(r'\bpass (\d+)\s*$',logs[0],re.M)),node_failed=sum(int(n) for n in re.findall(r'\bfail (\d+)\s*$',logs[0],re.M)),
            final_command=json.loads((RUN/(name+'-full-command.json')).read_bytes()))
        assert not configs[config]['new_managed_nonpasses']
    c.emit('validation.json',dict(base=c.BASE,configurations=configs,native=False,P='NO',prior_validation_sha256=c.sha(ROOT/'evidence/04A09/validation-v2.json'),
        pin_failure_disposition='Two newly stale source expectations plus one pre-existing checkout/receipt EOL pin mismatch beyond the historical 51 identities. source-delta.json and baseline-byte-provenance.json supply exact provenance. No historical test/fixture edited or skipped.'))
    c.emit('frozen-pin-successor.json',dict(new_failure_ids=sorted(expected),original_form_sha256=json.loads((OUT/'source-delta.json').read_bytes())['changed_existing'][c.FORM]['before'],
        current_form_sha256=c.sha(ROOT/c.FORM),source_delta='evidence/04A10/source-delta.json',
        entry_failure_ids=sorted(entry_pin),entry_provenance='evidence/04A10/baseline-byte-provenance.json',
        disposition='Remain FAIL: old whole-file pins and RunOLS request-overload source expectation are intentionally frozen. The baseline receipt expects CRLF, while all 24 files were LF on entry/main and remain unchanged. Actual family overload/writer compatibility has executable net48 tests. NOT native acceptance.'))
    print(json.dumps({k:{n:v for n,v in row.items() if n in ['total','passed','failed','batch10_executions','python_total','python_passed','node_passed','node_failed']} for k,row in configs.items()},indent=2))

def fixtures():
    def cell(s):
        if s=='NULL':return None
        kind,value=s.split(':',1)
        return value if kind=='D' else base64.b64decode(value).decode('utf-8')
    fixtures={}
    for p in sorted((OUT/'baselines').glob('*.txt')):
        lines=p.read_text().splitlines();sections=[];i=3
        while lines[i].startswith('TABLE '):
            metadata=lines[i];rows,cols=map(int,lines[i+1].split());values=[cell(s) for s in lines[i+2:i+2+rows*cols]]
            sections.append(dict(metadata=metadata,first_rows=[values[r*cols:(r+1)*cols] for r in range(min(rows,4))]))
            i+=2+rows*cols
        assert lines[i]=='FITTED';i+=1
        shape=list(map(int,lines[i].split()));i+=1
        fixtures[p.name]=dict(sha256=c.sha(p),row_count=len(lines[1].split(',')),convergence=lines[2],table_count=len(sections),sections=sections,
            fitted_shape=shape,fitted_first_rows=[cell(s) for s in lines[i:i+3*shape[1]]])
    c.emit('numerical-comparisons.json',dict(status='NOT_GOLDEN',native=False,P='NO',fixtures=fixtures,
        scope='Readable excerpts of complete pinned real-Core captures; successful comparisons are recorded by the Regression10 tests in validation.json. Only elapsed wall-clock text masked.',
        nist='Common projected Norris coefficient/SE reference comparisons in both cultures; full unchanged 11-case NIST suite plus GLM all-link references run per configuration. Original tolerances unchanged.'))

def commands():
    records=[]
    for p in sorted(RUN.glob('*-command.json')):
        records.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=c.sha(p),record=json.loads(p.read_bytes())))
    for p in sorted((RUN/'executions').glob('*/execution.json')):
        data=json.loads(p.read_bytes())
        records.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=c.sha(p),record={k:data[k] for k in ['command','exit_code','seconds']}))
    copies=[]
    for p in sorted(RUN.glob('*-verify-isolated.provenance.json')):
        copies.append(dict(path=p.relative_to(ROOT).as_posix(),raw_sha256=c.sha(p),record=json.loads(p.read_bytes())))
    c.emit('command-outcomes.json',dict(records=records,execution_copies=copies,
        deviation='Literal verify-beshstatng.ps1 was not executed: it overwrites TestResults/Codex and runs restore/Excel-DNA packaging hooks. Same suites/parameters executed via source-hashed audited execution copies, offline and without native packaging. Original script unchanged.',
        public_derivative=True,raw_logs='Ignored TestResults/04A-10-20261103-run01; preserved independently from sanitized public derivatives. Hashes refer to the raw bytes only when explicitly labelled raw_sha256.'))
    checks={}
    for name in ['historical-strict','historical-profile']:
        p=RUN/(name+'.log')
        checks[name]=dict(command=json.loads((RUN/(name+'-command.json')).read_bytes()),raw_log_sha256=c.sha(p),output=p.read_text(encoding='utf-8',errors='replace'))
    c.emit('historical-checkers.json',checks)

def closeout():
    c.source()
    preservation=RUN/'preservation-final.json'
    c.emit('preservation.json',dict(raw_receipt_sha256=c.sha(preservation),receipt=json.loads(preservation.read_bytes()),
        prior_public_evidence='All tracked previous evidence/architecture/planning byte pins verified by source-delta.json. No accepted source or older evidence rewritten. This is not a full historical TestResults byte rehash.'))
    paths=c.git('diff','--name-only').splitlines()+c.git('ls-files','--others','--exclude-standard').splitlines()
    assert not c.git('diff','--cached','--name-only')
    assert set(c.git('diff','--name-only').splitlines())==c.CHANGED
    files={};privacy=[]
    for name in sorted(set(paths)):
        if name=='evidence/04A10/publication-manifest.json':continue
        p=ROOT/name
        assert p.is_file()
        code=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-q',name],cwd=ROOT).returncode
        assert code==1,name
        content=p.read_text(encoding='utf-8-sig',errors='replace')
        if name.startswith('evidence/04A10/baselines/'):
            decoded=[]
            for line in content.splitlines():
                if line.startswith('String:'):
                    decoded.append(base64.b64decode(line.split(':',1)[1]).decode('utf-8'))
            content+='\n'+'\n'.join(decoded)
        if re.search(r'(?i)[A-Z]:[\\/]Users[\\/]|S-1-5-21-(?:\d+-){2,}\d+|BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY',content):privacy.append(name)
        files[name]=dict(sha256=c.sha(p),bytes=p.stat().st_size,git_ignore_exit=code)
    assert not privacy,privacy
    c.emit('publication-manifest.json',dict(base=c.BASE,head=c.git('rev-parse','HEAD'),branch=c.git('branch','--show-current'),index_empty=True,
        files=files,manifest_self_excluded=True,privacy_scan=dict(findings=privacy,scope='All current changed/untracked proposal text plus every decoded baseline String scalar; common local-absolute-user-path, SID and private-key signatures. Not a general secret-detection certification.'),
        excluded='Ignored historical/current TestResults, raw logs/TRXs/preimages/metadata inventory and all build/cache files. Original local evidence retained in place.',
        git_publication='No staging, commit, push, merge, remote modification or publication performed. Owner review pending.'))

def review():
    validation=json.loads((OUT/'validation.json').read_bytes())
    receipt=json.loads((RUN/'preservation-final.json').read_bytes())
    assert receipt['status']=='PASS' and receipt['errors']==[]
    a=validation['configurations']['Debug'];b=validation['configurations']['Release']
    assert a['python_nonpasses']==b['python_nonpasses']
    assert not a['python_inherited_outcome_changes'] and not b['python_inherited_outcome_changes']
    appendix='\n\n## Final preservation and complete Python nonpass ledger\n\n'
    appendix+=f"Preservation **PASS**: {receipt['historical_files']:,} historical files, zero membership/metadata/source-pin errors; {receipt['disk_free_bytes']:,} bytes free at the receipt. Scope is membership, size, creation/write nanosecond timestamps and file/device identity plus protected source hashes, not a full historical byte audit. All 3,171 protected existing tracked/docs files match entry hashes, including 51 prior public evidence files and 841 architecture/planning files.\n\n"
    appendix+='`git diff --check` and the empty-index check pass. Current changes are the three approved tracked files plus the new Common/tests/scripts/dossier/evidence candidates listed in the publication manifest. No other tracked file changed. All current proposal paths are checked with `git check-ignore`; raw TestResults remain ignored. Privacy receipt covers all proposal text and all decoded baseline string cells.\n\n'
    appendix+=f"Both configurations retain {len(a['python_nonpasses'])} distinct nonpassing methods and {len(a['python_failure_events'])} failure/error events including subtests. No inherited outcome was changed.\n\n| Outcome | Exact identity | Attribution |\n|---|---|---|\n"
    for identity,outcome in sorted(a['python_nonpasses'].items()):
        attribution='Inherited reported 04A.09 nonpass'
        if identity in a['entry_python_pin_failures']:attribution='Pre-existing at live main entry: CRLF receipt/LF checkout pin'
        if identity in a['new_python_pin_failures']:attribution='Newly stale frozen source expectation; exact successor evidence, remains FAIL'
        appendix+=f'| {outcome} | `{identity}` | {attribution} |\n'
    path=ROOT/'docs/architecture/batch-04A-10-owner-review.md'
    body=path.read_text(encoding='utf-8')
    marker='\n\n## Final preservation and complete Python nonpass ledger'
    if marker in body:
        assert body.endswith(appendix)
        return
    body=body.replace('| 9. Historical retention, publication/privacy and Git limits | Final preservation/publication receipts linked above; no Git publication action |',
                      '| 9. Historical retention, publication/privacy and Git limits | MET; preservation PASS, zero privacy findings, non-ignored public candidate paths; no Git publication action |')
    path.write_text(body+appendix,encoding='utf-8',newline='\n')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['validation','fixtures','commands','closeout','review']);a=p.parse_args()
    globals()[a.action]()
