"""Bounded 04A.40 reporting correction. No staging, source changes or scanner waivers."""
from pathlib import Path
import ast, collections, contextlib, copy, datetime, hashlib, importlib.util
import io, json, os, re, shutil, stat, subprocess, sys, traceback, unittest, zipfile, zlib

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
PRIVATE = ROOT / '.test-rationalization-04A40.tmp/owner-authorized-candidate-v3'
MAIN = 'bba848305c2c46ff3fc835a1132522ebfec59514'
INDEX = '1ba1e0ac644200d867cb5b171cbc9c883193780f0be5dae687a681c30208d700'
GATES = dict(P='NO', N1='NO GO', N2='NO', C2='UNMET', C3='UNAPPROVED', scientific='NOT_GOLDEN', next='04A41_NOT_AUTHORIZED')
STATUS = 'READY_FOR_OWNER_04A40_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
PAIRS = [
    dict(raw='evidence/04A40/baseline/Debug-build-v1.json', size=172325,
         sha='c2fe841a71b4b859358a5aced18ec71aedc03c07d6dc23aced051f9e8e75835f',
         safe='evidence/04A40/baseline/Debug-build-review-v2.json', safe_size=172293,
         safe_sha='4f04ce1e02d3e0241f15460c88ec4c4f41d67b102ddd744efef1212ae83eacea', field='/output'),
    dict(raw='evidence/04A40/entry/tool-runtime-final-v1.json', size=6401,
         sha='4ef9143dc7241ce08da3322607ba30e6c0b4581eb643194ec0b7c2d065d67f54',
         safe='evidence/04A40/entry/tool-runtime-review-v2.json', safe_size=6364,
         safe_sha='84dc88b1a9fdb13a7f87a49226a5d7fa779027b12aaf45ba6840f09e8842478c', field='/commands/0/stdout')]
os.environ.update(PYTHONDONTWRITEBYTECODE='1', GIT_OPTIONAL_LOCKS='0')
sys.dont_write_bytecode = True
sys.path.insert(0, str(ROOT/'evidence/04A40'))
sys.path.insert(0, str(ROOT/'evidence/04A39/physical-witness-v1'))
import physical

def utc(): return datetime.datetime.now(datetime.timezone.utc).isoformat()
def sha(b): return hashlib.sha256(b).hexdigest()
def load(p): return json.loads(Path(p).read_bytes())
def pin(p):
    p = Path(p)
    if not p.is_absolute(): p = ROOT/p
    with p.open('rb') as f: h = hashlib.file_digest(f, 'sha256').hexdigest()
    return dict(path=p.relative_to(ROOT).as_posix(), bytes=p.stat().st_size, sha256=h)
def exact(r):
    assert pin(r['path']) == {k:r[k] for k in ('path','bytes','sha256')}, 'PIN_MISMATCH:'+r['path']
def save(p, d):
    p = Path(p)
    if not p.is_absolute(): p = HERE/p
    p.parent.mkdir(parents=True, exist_ok=True)
    with p.open('x', encoding='utf-8', newline='\n') as f:
        json.dump(d, f, ensure_ascii=False, indent=2, allow_nan=False); f.write('\n')
def git(*args):
    result = subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args], cwd=ROOT, capture_output=True)
    assert result.returncode == 0, 'READ_ONLY_GIT_FAILED'
    return result.stdout
def imported(name, path):
    spec=importlib.util.spec_from_file_location(name,path)
    m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def lock():
    assert Path.cwd().resolve()==ROOT
    assert git('branch','--show-current').decode().strip()=='codex/04A-40'
    assert all(git('rev-parse',r).decode().strip()==MAIN for r in ['HEAD','main','codex/04A-40'])
    assert git('merge-base','main','codex/04A-40').decode().strip()==MAIN
    assert git('rev-parse','HEAD^').decode().strip()=='f2eb060c9c422c1fec4e82989d07907dbd4b9337'
    assert pin('.git/index')['sha256']==INDEX and not git('diff','--cached','--name-only')
    assert not git('diff','--check')
def scanner(phase):
    m=physical.scanner(phase)
    assert m.ROOT==ROOT and physical.tokens()
    return m
def fields(a,b,p=''):
    assert type(a)==type(b), 'JSON_TYPE_CHANGED'
    if isinstance(a,dict):
        assert a.keys()==b.keys()
        return sum((fields(a[k],b[k],p+'/'+k) for k in a),[])
    if isinstance(a,list):
        assert len(a)==len(b)
        return sum((fields(x,y,p+'/'+str(i)) for i,(x,y) in enumerate(zip(a,b))),[])
    return [] if a==b else [p]
def rawcopy(pair): return PRIVATE/'protected-originals'/pair['raw']
def authenticate_pair(pair, original):
    r=pin(original);assert (r['bytes'],r['sha256'])==(pair['size'],pair['sha'])
    s=pin(pair['safe']);assert (s['bytes'],s['sha256'])==(pair['safe_size'],pair['safe_sha'])
    assert fields(load(original),load(ROOT/pair['safe']))==[pair['field']]
    return r,s
def source_lock():
    freeze=load(ROOT/'evidence/04A40/matrix/source-freeze-v2.json')
    for p,h in freeze['source_pins'].items(): assert pin(p)['sha256']==h, 'SOURCE_CHANGED:'+p
    for r in freeze['new_tests']:exact(r)
    assert len(freeze['source_delta'])==16 and len(freeze['new_tests'])==2
    return freeze
def clean_findings(found):
    rows={}
    for row in found:
        src=pin(row['path'])
        item=dict(source=src,origin=row['origin'],category=row['category'],line=row.get('line'))
        item['occurrence_key_sha256']=sha(json.dumps(item,sort_keys=True).encode())
        rows[item['occurrence_key_sha256']]=item
    return list(rows.values())

def prepare():
    lock();source_lock()
    old=load(ROOT/'evidence/04A40/continuation-owner-v2-boundary-v1/git-source-readback-v1.json')
    for r in old['historical_pins']:exact(r)
    assert git('diff','--name-only').decode().splitlines()==old['modified']
    for pair in PAIRS:authenticate_pair(pair,ROOT/pair['raw'])
    pre=[]
    for phase in ['13','14']:
        m=scanner(phase);paths=m.paths();inputs=[pin(p) for p in paths]
        found,reviewed=m.scan(paths);assert inputs==[pin(p) for p in paths]
        unique=clean_findings(found)
        assert len(unique)==8 and {r['source']['path'] for r in unique}=={p['raw'] for p in PAIRS}
        save(PRIVATE/('before-scanner-'+phase+'.json'),dict(scanner=pin(m.__file__),candidate_inputs=inputs,findings=found,reviewed=reviewed))
        pre.append(dict(scanner=phase,source=pin(m.__file__),members=inputs,detections=len(found),unique_occurrences=len(unique),findings=unique,status='FAIL',scope_unchanged=True))
        print('Fresh pre-removal original scanner',phase,len(paths),'candidates;',len(unique),'unique findings',flush=True)
    snapshot_paths=set()
    for pattern in ['repository-privacy-*.json','publication-*physical*.json','publication-stop*.json']:
        snapshot_paths.update((ROOT/'.test-rationalization-04A40.tmp').glob(pattern))
    snapshot_paths.update((ROOT/'evidence/04A40').rglob('*'))
    snapshots=[pin(p) for p in sorted(snapshot_paths) if p.is_file() and '__pycache__' not in p.parts and HERE not in p.parents]
    for p in (ROOT/'.test-rationalization-04A40.tmp').glob('publication-v*'):
        if p.is_dir(): snapshots.extend(pin(f) for f in sorted(p.rglob('*')) if f.is_file())
    originals=[]
    for pair in PAIRS:
        source=ROOT/pair['raw']; target=rawcopy(pair)
        target.parent.mkdir(parents=True,exist_ok=True)
        st=source.stat()
        with source.open('rb') as src,target.open('xb') as dst:shutil.copyfileobj(src,dst)
        os.utime(target,ns=(st.st_atime_ns,st.st_mtime_ns))
        target.chmod(stat.S_IREAD)
        assert target.read_bytes()==source.read_bytes()
        r,s=authenticate_pair(pair,target)
        originals.append(dict(original=pin(source),protected_copy=r,original_mtime_ns=st.st_mtime_ns,original_mtime_utc=datetime.datetime.fromtimestamp(st.st_mtime,datetime.timezone.utc).isoformat(),copy_read_only=not bool(target.stat().st_mode & stat.S_IWRITE),immutability='READ_ONLY_ATTRIBUTE_AND_SEPARATE_SHA_PIN;NOT_WORM_STORAGE'))
    save(PRIVATE/'preservation-before-removal.json',dict(utc=utc(),originals=originals,prior_snapshots=snapshots,prior_failures_retained=True))
    (PRIVATE/'preservation-before-removal.json').chmod(stat.S_IREAD)
    save('pre-removal-scans-v1.json',dict(status='FAIL_RETAINED',reports=pre,same_occurrences_not_additive=True,owner_context=dict(username_present=True,hostname_present=True,raw_values_and_identifier_hashes_published=False),protected_manifest=pin(PRIVATE/'preservation-before-removal.json'),authorizations=dict(representation='I authorize versioned, sanitized **Git-candidate representations** of the two newly created reporting artifacts.',removal_response='I authorize the reuquest',removal_scope='Only the two named raw reports may be removed from repository working-tree paths after independently verified immutable separately pinned copies; original-path provenance/all failed snapshots retained; no scanner/ignore changes.')))
    print('Copies sealed; independent readback required before exact authorized removal',flush=True)

def crosswalk():
    lock();preserved=load(PRIVATE/'preservation-before-removal.json');artifacts=[]
    for pair,old in zip(PAIRS,preserved['originals']):
        assert not (ROOT/pair['raw']).exists()
        r,s=authenticate_pair(pair,rawcopy(pair));exact(old['protected_copy'])
        assert not bool(rawcopy(pair).stat().st_mode & stat.S_IWRITE)
        categories=sorted({f['category'] for report in load(HERE/'pre-removal-scans-v1.json')['reports'] for f in report['findings'] if f['source']['path']==pair['raw']})
        artifacts.append(dict(original=old['original'],original_mtime_utc=old['original_mtime_utc'],original_mtime_ns=old['original_mtime_ns'],protected_copy=r,representation=s,kind='SANITIZED_REVIEW_AND_GIT_CANDIDATE_DOCUMENT;NOT_AN_ORIGINAL_EXECUTION_TRANSCRIPT',changed_fields=[pair['field']],categories=categories,raw_failure='FAIL_RETAINED',repository_original_path='ABSENT_BY_EXACT_ADDITIONAL_OWNER_AUTHORIZATION',copy_read_only=True,commands_arguments_exit_codes_and_other_JSON_values_exact=True,transformation=('Only build output-directory metadata is sanitized. Raw build output/arguments and outcome data are unchanged.' if pair['field']=='/output' else 'Only dotnet-info stdout path rendering differs. Paths use aliases/forward separators. Embedded stdout_sha256 identifies protected raw stdout, not rendered text.'),authority='pre-removal-scans-v1.json authorizations; current owner approved exact removal request.'))
    save('raw-presentation-crosswalk-v3.json',dict(artifacts=artifacts,preservation_manifest=pin(PRIVATE/'preservation-before-removal.json'),raw_bytes_preserved=True,original_repository_paths_removed=2,other_protected_files_mutated=0,scanner_ignore_rules_changed=0,earlier_snapshots_retained=True))

class Capture(unittest.TestResult):
    def __init__(self):super().__init__();self.actual=[]
    def addFailure(self,test,err):
        super().addFailure(test,err)
        message=str(err[1]);trace=''.join(traceback.format_exception(*err))
        self.actual.append(dict(identity=test.id(),outcome='FAIL',message=message,traceback=trace,message_sha256=sha(message.encode()),traceback_sha256=sha(trace.encode())))

def guards():
    lock();sys.path.insert(0,str(ROOT/'scripts/tests'))
    diagnostics=load(ROOT/'evidence/04A40/python/diagnostic-dispositions-v3.json')
    rows=[];raw=[];changed=git('diff','--name-only').decode().splitlines()
    for old in load(ROOT/'evidence/04A40/python/parent-event-reconciliation-v2.json')['summary']['parent_changes']:
        exact(old['source']['source'])
        suite=unittest.defaultTestLoader.loadTestsFromName(old['id']);result=Capture()
        with contextlib.redirect_stdout(io.StringIO()),contextlib.redirect_stderr(io.StringIO()):suite.run(result)
        assert result.testsRun==1 and len(result.actual)==1 and not result.errors and result.actual[0]['outcome']=='FAIL','GUARD_DID_NOT_RETAIN_FAIL'
        actual=result.actual[0];raw.append(actual)
        event=next(r for r in diagnostics['rows'] if r['identity']==old['id'])
        if old['id'].startswith('test_04a04_alignment.'):
            triggering=[p for p in changed if 'UDF' in p.upper() or p.endswith('.dna')]
            assertion='assertFalse(any("UDF" in p.upper() or p.endswith(".dna") for p in git diff --name-only))'
        else:
            triggering=[p for p in changed if p.startswith('BESHstatNG/src/ExcelUDFs/')]
            assertion=('assertFalse(git diff --name-only -- BESHstatNG/src/ExcelUDFs)' if 'completion_successor' in old['id'] else 'assertFalse(any(Designer.vb / ExcelUDFs/ / BESHStat.Core/ / .dna changed))')
        rows.append(dict(identity=old['id'],status='FAIL',source=pin(old['source']['source']['path']),lines=old['source']['lines'],actual_assertion=assertion,triggering_paths=triggering,earlier_passing_snapshot_diagnostics=event['before'],retained_original_current_diagnostics=event['current'],fresh_diagnostics={k:actual[k] for k in ['message_sha256','traceback_sha256']},runtime=sys.version.split()[0],traceback_equality_not_required='Fresh runner/Python 3.13.18 stack differs from retained Python 3.14 campaign; both hashes retained. No result replaced.',applicability='Historical batch source-diff prohibition remains violated under separately authorized 04A.40 edits. Its current FAIL is applicable to its literal unchanged assertion; it is not evidence of an ABI or scientific defect. Separate retained ABI pins substantiate exports, without waiving this FAIL.',waived=False))
    save(PRIVATE/'fresh-three-guard-diagnostics.json',raw)
    save('scope-guard-dispositions-v3.json',dict(status='THREE_ACTUAL_FAILS_RETAINED',tests_run=3,FAIL=3,PASS=0,ERROR=0,guards=rows,protected_raw=pin(PRIVATE/'fresh-three-guard-diagnostics.json'),all32_current_differences_retained=True,all54_original_differences_retained=True,full_Python_campaign_not_rerun=True,assertions_modified=False))
    print('Three actual guards: 3 FAIL / 0 ERROR; no assertion changes',flush=True)

def preserve():
    lock();source=source_lock();prior=load(ROOT/'evidence/04A40/preservation/historical-readback-v4.json')
    cp=load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files'];assert len(cp)==468
    transitions={r['original']['path']:r for r in prior['owner_line_ending_transitions']};assert len(transitions)==4
    drift=0
    for r in cp:
        exact(dict(r,path='.test-rationalization-04A39-correction-v1.tmp/checkpoint/'+r['path']))
        if r['path'] in transitions:
            t=transitions[r['path']];exact(t['committed']);exact(t['exact_original_copy'])
            original=(ROOT/t['exact_original_copy']['path']).read_bytes();current=(ROOT/r['path']).read_bytes()
            assert original.replace(b'\r\n',b'\n')==current.replace(b'\r\n',b'\n') and current==git('show',MAIN+':'+r['path'])
        else:exact(r)
        st=(ROOT/r['path']).stat();drift+=((st.st_dev,st.st_ino)!=(r['device'],r['file_id']))
    records={};authorities=[]
    for path,field in [('evidence/04A39/preservation/new-pins-v1.json','files'),('evidence/04A39/preservation/post-publication-additions-v1.json','added'),('.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
        authorities.append(pin(path))
        for r in load(ROOT/path)[field]:
            if r['path'] in records:assert all(records[r['path']][k]==r[k] for k in ['bytes','sha256'])
            records[r['path']]=r
    assert len(records)==8984
    for i,r in enumerate(records.values()):
        if r['path'] in transitions:exact(transitions[r['path']]['committed'])
        else:exact(r)
        if 'device' in r:
            st=(ROOT/r['path']).stat();drift+=((st.st_dev,st.st_ino)!=(r['device'],r['file_id']))
        if (i+1)%2000==0:print('Protected exact readback',i+1,'/',len(records),flush=True)
    before=load(ROOT/'evidence/04A40/entry/source-preimages-v3.json')['files'];assert len(before)==1353
    delta={r['before']['path']:r for r in prior['authorized_source_transitions']}
    assert len(delta)==16
    for r in before:
        if r['path'] in delta:
            t=delta[r['path']];assert t['before']==r;exact(t['after']);exact(t['preserved_preimage'])
        else:exact(r)
    snapshots=load(PRIVATE/'preservation-before-removal.json')['prior_snapshots']
    removed={p['raw']:p for p in PAIRS}
    for r in snapshots:
        if r['path'] in removed:
            p=removed[r['path']];assert pin(rawcopy(p))['sha256']==r['sha256']
        else:exact(r)
    reader=imported('authenticated_reader40',ROOT/'evidence/04A40/verify40-v3.py')
    docs=reader.documents();retained_result=reader.verify(docs,live=True);adverse=reader.adverse(docs)
    register=load(ROOT/'evidence/04A39/register/defects-v7.json')
    register_reader=imported('register_reader40v3',ROOT/'evidence/04A39/verify-register-v5.py')
    counts=register_reader.verify(ROOT,register,check_files=False)
    assert (counts['canonical_entries'],counts['original_obligations'],counts['restricted_controls'],len(register['crosswalk']))==(1169,1514,802,2967)
    assert len(load(ROOT/'evidence/04A40/register/issue-delta-v4.json')['implemented_issue_decisions'])==20
    assert len(load(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json')['changes'])==54
    assert len(load(ROOT/'evidence/04A40/python/diagnostic-dispositions-v3.json')['rows'])==32
    for r in prior['archives']:exact(r)
    save('preservation-readback-v3.json',dict(status='BYTE_AND_SOURCE_LOCK_READBACK_PASS;CONTINUOUS_FILE_IDENTITY_NOT_VERIFIED',checkpoint_files=468,live_byte_exact=464,previous_owner_authorized_newline_renderings=4,exact_checkpoint_copies=468,protected_unique_paths=8984,protected_listed_bytes=sum(r['bytes'] for r in records.values()),protected_authorities=authorities,prior_snapshots_checked=len(snapshots),source_entry_identities=1353,unchanged_source_identities=1337,authorized_production_transitions=15,authorized_plan_transition=1,new_tests=source['new_tests'],all_current_source_pins_verified=len(source['source_pins']),register=counts,mappings=2967,investigations=20,observed_file_id_drift_records=drift,retained_previous_drift_count=788,continuous_precommit_identity='NOT_VERIFIED;NO_REBASELINE',W3_union=dict(files=37484,archives=154,inherited_qualified=True,fresh_audit=False),retained_reader_result=retained_result['status'],reader_note='Unchanged historical reader returns historical NOT_READY policy. New additive readiness is evaluated separately; no historical assertions changed.',adverse_controls=adverse,shipping_exports_compatibility_live_pins_verified=True,compatibility=dict(per_configuration=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458),exports_per_configuration=268,gates=GATES))
    print('468/8984/1353 preservation and shipping/compatibility locks PASS; reader controls',len(adverse),flush=True)

def main():
    action=sys.argv[1]
    {'prepare':prepare,'crosswalk':crosswalk,'guards':guards,'preserve':preserve}[action]()

if __name__=='__main__':main()
