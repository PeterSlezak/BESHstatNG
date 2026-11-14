"""Publish fresh entry identity accounting, without changing historical nonpasses."""
from pathlib import Path
from collections import Counter,defaultdict
import csv
import hashlib
import json
import os
import re
import sys
from preflight import ROOT,HERE,BASE,pin,save
from run import PRIVATE,m

def clean(value):
    if isinstance(value,list):return [clean(v) for v in value]
    if isinstance(value,dict):return {k:clean(v) for k,v in value.items()}
    if not isinstance(value,str):return value
    for a,b in [(str(PRIVATE),'<ISOLATED_RUN>'),(PRIVATE.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:value=value.replace(a,b)
    value=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',value,flags=re.I)
    value=re.sub(r'[A-Za-z]:[\\/]+Program Files(?: \(x86\))?[\\/]+','<PROGRAM_FILES>/',value,flags=re.I)
    value=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',value,flags=re.I)
    return re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',value)
def sha(b):return hashlib.sha256(b).hexdigest()
def index(records):
    groups=defaultdict(list)
    for r in records:groups[sha(json.dumps((r['class_name'],r['method'],r['test_name'])).encode())].append(r)
    return {(k,i):r for k,rows in groups.items() for i,r in enumerate(sorted(rows,key=lambda r:r['outcome']))}
def python_receipt(name):
    p=PRIVATE/'commands'/name;raw=(p/'stderr.log').read_bytes()
    try:text=raw.decode('utf-8-sig');encoding='utf-8-sig'
    except UnicodeDecodeError:text=raw.decode('cp1252');encoding='cp1252 console only'
    total=int(re.search(r'Ran (\d+) tests?',text).group(1))
    failures=int(re.search(r'failures=(\d+)',text).group(1)) if re.search(r'failures=(\d+)',text) else 0
    errors=int(re.search(r'errors=(\d+)',text).group(1)) if re.search(r'errors=(\d+)',text) else 0
    return dict(label=name,total=total,passed=total-failures-errors,failed=failures,errors=errors,stderr_sha256=sha(raw),console_decoding=encoding,
        exit_code=json.loads((p/'command.json').read_bytes())['exit_code'],identities=re.findall(r'^(test_\S+ \([^)]*\))',text,re.M),
        nonpasses=[dict(outcome=k,identity=i,subtest=clean(s.strip())) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',text,re.M)])

def main():
    with (ROOT/'evidence/04A37/managed-test-identities.csv').open(encoding='utf-8-sig',newline='') as f:old=list(csv.DictReader(f))
    identities=[];nonpasses=[];totals={};results=[];all_index={}
    for config in ['Debug','Release']:
        total=Counter()
        for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
            data=json.loads((PRIVATE/f'executions/entry-{config}-serial-{suite}/cases.json').read_bytes())
            slots=index(data['records']);all_index[config,suite]=slots;total.update(data['outcomes'])
            expected={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r['final_'+config.lower()] for r in old if r['suite']==suite}
            assert {k:r['outcome'] for k,r in slots.items()}==expected,('Fresh managed delta',config,suite)
            results.append(dict(configuration=config,suite=suite,outcomes=data['outcomes'],exit_code=data['command']['exit_code'],trx_sha256=data['sha256'],accepted37_identity_outcome_delta=0))
            for k,r in slots.items():
                if r['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,raw_identity_sha256=k[0],occurrence=k[1],class_name=r['class_name'],method=r['method'],test_name=clean(r['test_name']),outcome=r['outcome'],message_sha256=sha(r['message'].encode()),classification='INHERITED_ACCEPTED37_IDENTITY_OCCURRENCE_OUTCOME'))
        assert total==Counter(Passed=20507,Failed=14);totals[config]=dict(total)
    for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
        for k,r in sorted(all_index['Debug',suite].items()):
            assert r['outcome']==all_index['Release',suite][k]['outcome']
            identities.append(dict(suite=suite,class_name=r['class_name'],method=r['method'],test_name=clean(r['test_name']),raw_identity_sha256=k[0],raw_occurrence_ordinal=k[1],entry_debug=r['outcome'],entry_release=all_index['Release',suite][k]['outcome']))
    with (HERE/'managed-test-identities-entry.csv').open('x',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(identities[0]),lineterminator='\n');w.writeheader();w.writerows(identities)
    python=[python_receipt('entry-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]
    node=[]
    for suffix in ['office-node','resolver','wasm-node']:
        p=PRIVATE/'commands'/('entry-'+suffix);raw=(p/'stdout.log').read_bytes();text=raw.decode('utf-8-sig')
        def count(k):return int(re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',text)[-1])
        ids=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)',text,re.M)
        node.append(dict(suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((p/'command.json').read_bytes())['exit_code'],identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode()),stdout_sha256=sha(raw)))
    serial={s:json.loads((PRIVATE/'commands'/('entry-'+s)/'command.json').read_bytes())['exit_code'] for s in ['strict','profile','docs']}
    packet=[python_receipt('entry-accepted'+b+'-audits') for b in ['34','35','36','37']]+[python_receipt('entry-accepted37-supplement')]
    save('nonpasses-entry.json',dict(managed=nonpasses,python=[dict(label=r['label'],nonpasses=r['nonpasses']) for r in python],historical_raw_packet_audits=packet,
        raw_packet_qualification='Historical exact-byte readers retain FAIL/ERROR after legitimate Git normalization. No old source/test/evidence changed. Independent exact original37 view re-executes42+45 audits and reader successfully.',
        new_tooling_attempts='preedit/attempts/; invalid inventory/inspection attempts preserved; no product edits or execution freeze yet.'))
    save('entry-baseline.json',dict(status='FRESH_MERGED_MAIN_BASELINE_EXECUTED; IMPLEMENTATION_GATE_STILL_CLOSED',base=BASE,managed=totals,suites=results,managed_identity_count=len(identities),accepted37_identity_multiset_delta=0,
        python={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']},python_suites=python,node=node,node_totals={k:sum(r[k] for r in node) for k in ['total','passed','failed']},strict_profile_mkdocs=serial,
        accepted37_exact_view=pin('evidence/04A38/accepted37-exact-view.json'),
        canonical_wrappers=dict(status='NOT_RUN_UNMODIFIED_SHARED_OUTPUT_AND_RESTORE_PACKAGING_PATHS',
            portable='Original script writes shared TestResults/Codex and invokes dotnet test implicit restore; owner requires isolated output and prohibits package restore.',
            legacy='Original -Legacy script invokes full host Build including XLL packaging; packaging is explicitly unauthorized. Accepted isolated compile-only/offline runner executes equivalent managed suites serially.'),
        no_full_suite_green_claim=True,native='NOT_RUN',scientific='NOT_GOLDEN',next_batch='04A39_NOT_AUTHORIZED',product_edits='NONE; baseline precedes implementation and compatibility freeze'))
    save('command-receipts-entry.json',dict(commands=[dict(name=p.parent.name,**clean(json.loads(p.read_bytes()))) for p in sorted((PRIVATE/'commands').glob('*/command.json'))],scope='Actual commands/exits/timings/raw-private-output hashes; no inferred successful lane. Absolute paths sanitized.'))
    print('Fresh managed20507 PASS/14 FAIL each; exact old identity multiset retained; Python', {k:sum(r[k] for r in python) for k in ['passed','failed','errors']},'; Node',sum(r['passed'] for r in node),'; strict/profile/docs',serial)

if __name__=='__main__':main()
