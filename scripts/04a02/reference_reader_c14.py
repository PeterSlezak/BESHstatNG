"""Read-only C14 consolidation of actual frozen owner R outcomes; never runs R.

No owner-token or signature claim is inferred from readable JSON. Copies, when
requested by the caller, belong only to new exclusive evidence destinations.
"""
import hashlib
import importlib.util
import math
from pathlib import Path
import re
import stat

ROOT=Path(__file__).resolve().parents[2]
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path)
    value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
h=module('c14_c13_handoff','scripts/04a02/reference_handoff_c13.py');r=h.r
cmp=module('c14_comparison','scripts/04a02/compare_reference_v1.py')
PUBLICATIONS={'C30-SYN-ANOVA':'27a2594c2b52ebf5b1fad3abbf0ce5cab0afc5b1dbc6af08c7f5aa02f2221c89',
              'C30-SYN-KW':'ec88aebe4f2ea8182f65a0c43fe9b854804973e7879eded89f9473c52dbdb4fa'}
def digest_bytes(value):return hashlib.sha256(value).hexdigest()
def read(path,expected=None):
    path=Path(path)
    try:r.identity.literal_path(str(path))
    except r.identity.PathIdentityError as error:raise r.Refused('Local evidence path required: '+str(error)) from error
    for node in [path,*path.parents]:
        r.require(not node.lstat().st_file_attributes & stat.FILE_ATTRIBUTE_REPARSE_POINT,'Reparse evidence path refused')
    with r.v2.locked_path(path) as stream:
        value=stream.read()
    if expected is not None:r.require(digest_bytes(value)==expected,'Evidence hash mismatch: '+str(path))
    return value
def relative_name(name):
    r.require(isinstance(name,str) and re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9._-]{0,180}',name) and '..' not in name,'Unsafe publication member')
    return name
def verify_publication(job,expected):
    raw=read(job/'publication-v3.json',expected);publication=r.v2.strict_json(raw)
    r.require(set(publication)=={'status','hashes'} and publication['status']=='REFERENCE_EXECUTED_NOT_COMPARED_NOT_GOLDEN','No executed NOT_GOLDEN publication')
    hashes=publication['hashes'];required={'approved-input.dcf','preflight-v6.json','process-exit.json','proposal-source.R','raw-stderr.bin','raw-stdout.bin','reference-candidate.json','empty-startup'}
    r.require(isinstance(hashes,dict) and required<=set(hashes),'Missing reference evidence')
    for name,expected in hashes.items():read(job/relative_name(name),expected)
    r.require(read(job/'raw-stderr.bin')==b'','Nonempty reference stderr')
    r.require(r.v2.strict_json(read(job/'process-exit.json'))=={'exit_code':0},'Reference process failed')
    r.require(not list(job.glob('failure-*.json')),'Retained failed outcome cannot pass')
    return publication
def compare_retained(candidate,reference,policy,case,fixture,decision,runtime_set,retained,hashes):
    result=cmp.compare(candidate,reference,policy,case,fixture,decision,runtime_set)
    result['hashes']=hashes
    r.require(r.canonical(result)==r.canonical(retained),'Retained comparison differs from unchanged comparator reproduction')
    r.require(not result['failed'] and not result['missing'] and not result['extra'],'Numerical comparison failed')
    for row in result['fields']:
        if row['status']=='REFERENCE_ONLY_NO_COMMON_MEMBER':
            r.require(row['subject'] is None,'Reference-only field became Common match');continue
        r.require(row['status']=='PASS','Unsuccessful mapped Common field')
        actual,reference=row['subject'],row['reference']
        r.require(type(actual) in (int,float) and type(reference) in (int,float) and math.isfinite(actual) and math.isfinite(reference),'Nonfinite/non-numeric comparison')
        r.require(row['subject_type']==type(actual).__name__ and row['reference_type']==type(reference).__name__,'Numeric type mismatch')
        r.require(row['delta']==abs(actual-reference) and row['limit']==max(row['atol'],row['rtol']*abs(reference)),'Comparison arithmetic mismatch')
    return result
def load_case(case_id):
    r.require(case_id in PUBLICATIONS,'Unsupported reference case')
    manifest,decision,case,fixture,policy=h.verified_startup(case_id)
    job=h.OWNER_ROOT/h.CASES[case_id]
    publication=verify_publication(job,PUBLICATIONS[case_id])
    reference=r.v2.strict_json(read(job/'raw-stdout.bin'))
    runtime=policy['runtime_proposal'];runtime_set=r.sha(r.canonical({p['relative_path']:p['sha256'] for p in runtime['runtime_components']}))
    r.require(reference['provenance']['runtime_set_sha256']==runtime_set,'Runtime set mismatch')
    r.require(r.digest(r.R_HOME/'bin/x64/Rscript.exe')==runtime['rscript_sha256'],'Runtime executable drift')
    for component in runtime['runtime_components']:r.require(r.digest(r.R_HOME/component['relative_path'])==component['sha256'],'Runtime component drift')
    r.require(read(job/'proposal-source.R',case['script_sha256'])==read(ROOT/case['script'],case['script_sha256']),'Executed source differs from frozen script')
    for pin in fixture['source_pins']:read(ROOT/pin['path'],pin['sha256'])
    short='anova' if case_id.endswith('ANOVA') else 'kw'
    go_path=ROOT/('docs/architecture/04A02/04A02-C13-'+short+'-technical-go-v1.json')
    go=r.v2.strict_json(read(go_path));go_sha=r.digest(go_path)
    preflight=r.v2.strict_json(read(job/'preflight-v6.json'))
    # Preserve historical proposal false/null fields. Actual case GO and later
    # C08 authority bind current execution, never mutate that proposal object.
    r.require(preflight.get('manifest')==manifest,'Preflight manifest mismatch')
    r.require(preflight.get('go')==go,'Preflight case GO differs from reviewed immutable source')
    r.require(preflight.get('cwd')==str(job) and preflight.get('timeout')==30,'Preflight command isolation drift')
    r.require(preflight.get('command')==[str(r.R_HOME/'bin/x64/Rscript.exe'),'--vanilla',str(job/'proposal-source.R'),str(job/'approved-input.dcf')],'Executed command mismatch')
    env=preflight.get('env',{})
    expected_env=dict(SystemRoot='C:\\WINDOWS',WINDIR='C:\\WINDOWS',R_HOME=str(r.R_HOME),PATH=str(r.R_HOME/'bin/x64'),HOME=str(job/'home'),R_USER=str(job/'home'),R_LIBS_USER=str(job/'user-library'),R_LIBS_SITE=str(job/'site-library'),R_LIBS='',R_DEFAULT_PACKAGES='utils,stats',TEMP=str(job/'temp'),TMP=str(job/'temp'),TMPDIR=str(job/'temp'),LC_ALL='C',LANG='C',LANGUAGE='C',TZ='UTC')
    expected_env.update({key:str(job/'empty-startup') for key in ('R_ENVIRON','R_ENVIRON_USER','R_PROFILE','R_PROFILE_USER')})
    r.require(env==expected_env,'Private environment/profile/library drift')
    r.require(preflight.get('status')=='R_TECHNICALLY_QUALIFIED_NOT_GOLDEN','No executed case preflight')
    receipt_path=ROOT/('TestResults/04A-02-c13-owner-review-'+short+'-20261101-v1.json')
    receipt=r.v2.strict_json(read(receipt_path))
    r.require(receipt.get('case_id')==case_id and receipt.get('go_sha256')==go_sha and receipt.get('sid')==r.OWNER_SID and receipt.get('reviewer') and receipt.get('utc') and receipt.get('source'),'Wrong owner technical receipt')
    r.require(receipt.get('decision')=='REVIEWED_EXACT_GO_FOR_ONE_CASE_ONLY' and receipt.get('P')=='NO' and receipt.get('N2')=='NO' and receipt.get('scientific_review')=='NOT_A_SCIENTIFIC_REVIEW','Owner receipt scope drift')
    comparisons=[]
    for culture in ['invariant','sk-SK']:
        candidate_path=ROOT/'TestResults/04A-02-initial-fixtures-c13-20261101T190032Z/executions/c13D-Common/observations'/(case_id+'-'+culture+'.candidate.json')
        result_path=job/('comparison-c13-'+culture+'.json');retained=r.v2.strict_json(read(result_path))
        hashes=dict(candidate=r.digest(candidate_path),raw_reference=r.digest(job/'raw-stdout.bin'),publication=PUBLICATIONS[case_id],comparator=r.digest(ROOT/'scripts/04a02/compare_reference_v1.py'),policy=r.digest(ROOT/'docs/architecture/04A02/04A02-C07-scientific-decision-form.json'))
        result=compare_retained(r.v2.strict_json(read(candidate_path)),reference,policy,case,fixture,decision,runtime_set,retained,hashes)
        comparisons.append(dict(culture=culture,path=str(result_path),sha256=r.digest(result_path),candidate=dict(path=str(candidate_path),sha256=r.digest(candidate_path)),result=result,mapped_pass=sum(f['status']=='PASS' for f in result['fields']),reference_only=sum(f['status']=='REFERENCE_ONLY_NO_COMMON_MEMBER' for f in result['fields']),auxiliary_pass=sum(f['status']=='PASS' for f in result['auxiliary'])))
    r.require(r.v2.strict_json(read(job/'reference-candidate.json')).get('status')=='UNREVIEWED_REFERENCE_CANDIDATE_NOT_GOLDEN','Candidate promotion/scope mismatch')
    return dict(case_id=case_id,job=str(job),publication=dict(path=str(job/'publication-v3.json'),sha256=PUBLICATIONS[case_id]),reference=reference,comparisons=comparisons,source_case=case,fixture=fixture,approved_policy=policy,owner_technical_receipt=dict(path=str(receipt_path),sha256=r.digest(receipt_path),record=receipt,signature='NO_CRYPTOGRAPHIC_SIGNATURE; owner-attributed technical JSON plus direct owner handoff, not independent scientific signature'),preflight_sha256=r.digest(job/'preflight-v6.json'),go=dict(path=str(go_path),sha256=go_sha),runtime_components_verified=len(runtime['runtime_components']),rendered_excel_format='NOT_CAPTURED_NOT_COMPARED',P='NO')
