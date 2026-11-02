"""C09 ACL/qualification successor. No R execution until pinned owner-context GO.

Reuses frozen v3 protocol and v2 no-replace publication; neither is modified.
All ancestors remain checked/locked. Ancestor sibling-creation rights do not imply
replacement of the selected existing child. Administrators/SYSTEM/TrustedInstaller
are trusted OS maintenance principals, not arbitrary third-party writers.
"""
from contextlib import ExitStack, contextmanager
import argparse
import ctypes
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import uuid

ROOT=Path(__file__).resolve().parents[2]
for _path,_expected in {
    'scripts/04a02/oracle_operator.py':'5aa59d822a824808d3fd42bef9b1c1bc8d789dfc92d80dc4a6f108202aa7d30b',
    'scripts/04a02/oracle_operator_v3.py':'9a98e7e16a28844eae7915ddf03e8a30d8ec72d05b15f6ca848705368a07daf0',
}.items():
    if hashlib.sha256((ROOT/_path).read_bytes()).hexdigest()!=_expected:
        raise ValueError('Frozen dependency drift before import: '+_path)
spec=importlib.util.spec_from_file_location('c08_frozen',ROOT/'scripts/04a02/oracle_operator_v3.py')
v3=importlib.util.module_from_spec(spec)
spec.loader.exec_module(v3)
v2=v3.v2
require,Refused,digest,sha,canonical=v3.require,v3.Refused,v3.digest,v3.sha,v3.canonical
OS_TRUST={'S-1-5-18','S-1-5-32-544','S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464'}
OWNER_SID='S-1-5-21-142429299-24905182-1790040237-1002'
R_HOME=Path('C:/Program Files/R/R-4.5.1')
OWNER_RECEIPT_SHA='8a4c8e0973e7b24f66795559ac1841d1144ba1edc27dc1fa685ff7fa274b4baf'
DEPENDENCIES={'scripts/04a02/'+name for name in ('oracle_operator_v4.py','oracle_operator_v3.py','oracle_operator.py',
    'inspect_output_acl_v4.ps1','windows_acl_v4.cs','r_startup_v1.R')}


def expand(mask):
    mask &= 0xffffffff
    for generic,mapped in [(0x10000000,0x1f01ff),(0x80000000,0x120089),
                           (0x40000000,0x120116),(0x20000000,0x1200a0)]:
        if mask & generic:
            mask=(mask & ~generic)|mapped
    return mask


def assess_acl(observed,root,owner_sid=OWNER_SID):
    """No blanket same-owner requirement on C:\\ or Users. No unknown-writer waiver.

    Parent: DeleteChild/WriteDACL/WriteOwner may replace its child. Each child:
    Delete on the child can also permit replacement. Selected root: additionally
    reject unknown file/subdirectory creators and data/attribute writers.
    Deny ACEs are not subtracted to guess effective access for another identity.
    Such apparent grants need a separately supplied token-aware review, not a pass.
    """
    require(Path(root).is_absolute(),'Literal absolute root required')
    for part in Path(root).parts[1:]:v2.component(part)
    root=Path(os.path.abspath(root))
    require(root.is_absolute() and not str(root).startswith('\\\\'),'Local absolute root required')
    for part in root.parts[1:]:v2.component(part)
    expected=[str(p).casefold() for p in reversed([root,*root.parents])]
    rows=observed.get('ancestors',[])
    problems=[]
    if observed.get('execution_sid')!=owner_sid:problems.append('EXECUTION_NOT_NORMAL_OWNER')
    if [r.get('path','').casefold() for r in rows]!=expected:problems.append('INCOMPLETE_ANCESTOR_CHAIN')
    trusted=OS_TRUST|{owner_sid}
    for i,row in enumerate(rows):
        path=row.get('path','?')
        if row.get('error'):
            problems.append('UNREADABLE:'+path);continue
        if row.get('reparse') is not False:problems.append('REPARSE_OR_UNKNOWN:'+path)
        if not row.get('sddl') or row.get('canonical') is not True:problems.append('UNSUPPORTED_DACL:'+path)
        if row.get('owner_sid') not in trusted:problems.append('UNTRUSTED_OWNER:'+path)
        if not isinstance(row.get('current_token_effective'),dict):problems.append('NO_CURRENT_TOKEN_ACCESSCHECK:'+path)
        is_root=i==len(rows)-1
        forbidden=0x40|0x10000|0x40000|0x80000
        if is_root:forbidden|=2|4|0x10|0x100
        for ace in row.get('access',[]):
            if ace.get('type')!='Allow' or 'InheritOnly' in ace.get('propagation',''):continue
            if ace.get('sid') not in trusted and expand(ace['rights'])&forbidden:
                problems.append('UNRESOLVED_MODIFY:'+path+':'+str(ace.get('sid')))
    if rows:
        last=rows[-1]
        if last.get('owner_sid')!=owner_sid:problems.append('REFERENCE_ROOT_NOT_OWNER_OWNED')
        rights=last.get('current_token_effective',{})
        if not all(rights.get(k) is True for k in ('create_file','create_directory','read_control')):
            problems.append('OWNER_LACKS_REFERENCE_ROOT_ACCESS')
    return problems


def collect_acl(root):
    command=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File',str(ROOT/'scripts/04a02/inspect_output_acl_v4.ps1'),
             '-OutputRoot',str(root.absolute())]
    result=subprocess.run(command,cwd=ROOT,capture_output=True,timeout=30)
    require(result.returncode==0 and not result.stderr,'ACL collection failed: '+result.stderr.decode(errors='replace')[:1000])
    return v2.strict_json(result.stdout)


@contextmanager
def locked_directory(path):
    """A data-access directory handle: attribute-only handles do not block rename.

    FILE_LIST_DIRECTORY|FILE_READ_ATTRIBUTES, share READ|WRITE but not DELETE.
    OPEN_REPARSE_POINT prevents silently following a replaced junction. Parent
    chain is already held by lock_chain_complete before its child is opened.
    """
    from ctypes import wintypes
    require(os.name=='nt','Windows directory locking required')
    before=v2.no_reparse(path)
    require(path.is_dir(),'Directory required')
    kernel=ctypes.WinDLL('kernel32',use_last_error=True)
    create=kernel.CreateFileW
    create.argtypes=[wintypes.LPCWSTR,wintypes.DWORD,wintypes.DWORD,wintypes.LPVOID,wintypes.DWORD,wintypes.DWORD,wintypes.HANDLE]
    create.restype=wintypes.HANDLE
    close=kernel.CloseHandle;close.argtypes=[wintypes.HANDLE];close.restype=wintypes.BOOL
    handle=create(str(path),0x81,3,None,3,0x02000000|0x00200000,None)
    if handle==wintypes.HANDLE(-1).value:raise ctypes.WinError(ctypes.get_last_error())
    try:
        after=v2.no_reparse(path)
        require((before.st_dev,before.st_ino)==(after.st_dev,after.st_ino),'Directory identity changed during lock acquisition')
        yield path
    finally:close(handle)


@contextmanager
def lock_chain_complete(path):
    """Every ancestor, including drive/profile. Hold deny-delete handles throughout."""
    path=Path(os.path.abspath(path))
    require(not str(path).startswith('\\\\'),'UNC refused')
    with ExitStack() as stack:
        for current in reversed([path,*path.parents]):
            for part in current.parts[1:]:v2.component(part)
            try:
                stack.enter_context(locked_directory(current))
            except PermissionError as error:
                raise PermissionError('Cannot hold required ancestor lock: '+str(current)) from error
        yield path


def pins(manifest_path,manifest_sha,case_id):
    manifest=v3.pinned(manifest_path,manifest_sha)
    require(manifest.get('schema')=='04A02-oracle-execution-manifest-v4','Versioned v4 manifest required')
    require(set(manifest.get('operator_files',{}))==DEPENDENCIES,'Missing/extra executable dependencies')
    for relative,expected in manifest['operator_files'].items():
        require(digest(ROOT/relative)==expected,'Operator dependency drift: '+relative)
    require(manifest['operator_files'].get('scripts/04a02/oracle_operator_v4.py')==digest(Path(__file__)),'Self pin missing')
    owner=v3.pinned(ROOT/manifest['owner_receipt']['path'],OWNER_RECEIPT_SHA)
    require(manifest['owner_receipt']['sha256']==OWNER_RECEIPT_SHA,'Owner receipt pin drift')
    decision_id=v3.owner_admission(owner,case_id)
    for relative,expected in v3.POLICIES.items():require(digest(ROOT/relative)==expected,'Policy drift')
    old=v3.pinned(ROOT/'docs/architecture/04A02/oracle-proposals/manifest-v2.json',v3.FROZEN_MANIFEST)
    require(manifest['cases']==old['cases'] and manifest['P']=='NO','Altered case scope/promotion')
    case,fixture=v2.load_case(old,case_id)
    require(digest(ROOT/case['script'])==case['script_sha256'],'Script drift')
    policy=next(c for c in v2.strict_json((ROOT/next(iter(v3.POLICIES))).read_bytes())['cases'] if c['case_id']==case_id)
    require(str(R_HOME).casefold()==manifest['r_home'].casefold(),'R installation drift')
    runtime=policy['runtime_proposal']
    require(digest(R_HOME/'bin/x64/Rscript.exe')==runtime['rscript_sha256'],'Rscript drift')
    for entry in runtime['runtime_components']:
        require(digest(R_HOME/v2.relative_component_path(entry['relative_path']))==entry['sha256'],'Runtime/package drift')
    return manifest,decision_id,case,fixture,policy


def validate_probe(value,job):
    require(value.get('schema')=='04A02-C09-R-startup-v1','Startup schema')
    require(value.get('r')==value.get('stats')=='4.5.1' and value.get('arch')=='x86_64','R/stats/architecture identity')
    require(value.get('locale')=='C' and value.get('home')==str(job/'home').replace('\\','/'),'Private R_USER/HOME proof')
    wanted={str(job/'user-library').replace('\\','/'),str(job/'site-library').replace('\\','/'),str(R_HOME/'library').replace('\\','/')}
    require(set(value.get('libraries',[]))<=wanted and str(R_HOME/'library').replace('\\','/') in value['libraries'],'Package search escape')
    require(set(value.get('namespaces',[]))<= {'base','utils','stats','tools','methods','graphics','grDevices','datasets'},'Unapproved loaded package')
    require(value.get('r_profile')==value.get('r_environ')==str(job/'empty-startup').replace('\\','/'),'Private startup settings')


def probe(manifest_path,manifest_sha,root,name,review_path,review_sha):
    """Nonstatistical R startup only after actual-owner ACL review; never a case GO."""
    manifest,_,_,_,policy=pins(manifest_path,manifest_sha,'C30-SYN-ANOVA')
    root=root.absolute();v2.component(name)
    with ExitStack() as stack:
        stack.enter_context(lock_chain_complete(root))
        observed=collect_acl(root)
        require(not assess_acl(observed,root),'Actual owner/root ACL qualification required before startup')
        review=v3.pinned(review_path,review_sha)
        require(review.get('decision')=='ACCEPT_EXACT_ACL_AND_THREAT_MODEL_V4' and review.get('reviewer') and
                review.get('source') and review.get('execution_sid')==OWNER_SID and
                review.get('observed_sha256')==sha(canonical(observed)),'Attributable exact current ACL review required')
        runtime=policy['runtime_proposal']
        inputs=[(ROOT/p,h) for p,h in manifest['operator_files'].items()]
        inputs += [(manifest_path,manifest_sha),(review_path,review_sha),(R_HOME/'bin/x64/Rscript.exe',runtime['rscript_sha256'])]
        inputs += [(R_HOME/v2.relative_component_path(p['relative_path']),p['sha256']) for p in runtime['runtime_components']]
        for path,expected in inputs:
            stack.enter_context(lock_chain_complete(path.parent));stream=stack.enter_context(v2.locked_path(path))
            require(hashlib.file_digest(stream,'sha256').hexdigest()==expected,'Startup input drift')
        job=root/name;job.mkdir()
        stack.enter_context(locked_directory(job))
        try:
            env=v3.private_environment(job,R_HOME)
            source=ROOT/'scripts/04a02/r_startup_v1.R'
            require(digest(source)==manifest['operator_files'].get(source.relative_to(ROOT).as_posix()),'Startup script not pinned')
            v2.exclusive_bytes(job/'startup.R',source.read_bytes())
            command=[str(R_HOME/'bin/x64/Rscript.exe'),'--vanilla',str(job/'startup.R')]
            v2.exclusive_bytes(job/'preflight.json',canonical(dict(command=command,env=env,manifest_sha256=manifest_sha,
                runtime=runtime,cwd=str(job),timeout=30,status='STARTUP_ONLY_NOT_STATISTICAL_GO')))
            with v2.locked_path(job/'startup.R'),(job/'raw-stdout.bin').open('xb') as out,(job/'raw-stderr.bin').open('xb') as err:
                result=subprocess.run(command,cwd=job,env=env,stdin=subprocess.DEVNULL,stdout=out,stderr=err,
                    timeout=30,shell=False,creationflags=getattr(subprocess,'CREATE_NO_WINDOW',0))
            v2.exclusive_bytes(job/'exit.json',canonical(dict(exit_code=result.returncode)))
            require(result.returncode==0 and (job/'raw-stderr.bin').read_bytes()==b'','Startup failed/stderr')
            validate_probe(v2.strict_json((job/'raw-stdout.bin').read_bytes()),job)
            v2.publish_complete(job,'startup-receipt.json',canonical(dict(status='R_RUNTIME_PROBED',job=str(job),
                manifest_sha256=manifest_sha,stdout_sha256=digest(job/'raw-stdout.bin'),runtime=runtime,
                loader_bound='Exact bundled runtime/base/stats pins plus observed startup; OS API-set/SxS/system maintenance remain trusted Windows boundaries. No third-party packages or arbitrary DLL closure claimed.')))
            return job
        except BaseException as error:
            v2.exclusive_bytes(job/('failure-'+uuid.uuid4().hex+'.json'),canonical(dict(status='FAILED_RETAINED_NO_RETRY',error=str(error))))
            raise


def qualification(go_path,expected,manifest_sha,case_id,root,observed):
    go=v3.pinned(go_path,expected)
    require(go.get('schema')=='04A02-C09-R-technical-v1' and go.get('status')=='R_TECHNICAL_GO','No current technical GO')
    require(go.get('case_id')==case_id and go.get('manifest_sha256')==manifest_sha and
            go.get('output_root')==str(root.absolute()),'Wrong case/manifest/output root')
    require(not assess_acl(observed,root),'Owner-context ACL gate failed')
    require(go.get('acl_sha256')==sha(canonical(observed)),'ACL drift/stale review')
    review=v3.pinned(Path(go['acl_review']['path']),go['acl_review']['sha256'])
    require(review.get('execution_sid')==OWNER_SID and review.get('reviewer') and review.get('source') and
            review.get('observed_sha256')==go['acl_sha256'] and review.get('decision')=='ACCEPT_EXACT_ACL_AND_THREAT_MODEL_V4',
            'Actual attributable owner ACL review required')
    probe=v3.pinned(Path(go['startup_receipt']['path']),go['startup_receipt']['sha256'])
    require(probe.get('status')=='R_RUNTIME_PROBED' and probe.get('manifest_sha256')==manifest_sha,'Missing executed startup qualification')
    raw=Path(probe['job'])/'raw-stdout.bin'
    require(digest(raw)==probe['stdout_sha256'] and (raw.parent/'raw-stderr.bin').read_bytes()==b'','Probe raw drift/stderr')
    validate_probe(v2.strict_json(raw.read_bytes()),raw.parent)
    tests=v3.pinned(Path(go['test_receipt']['path']),go['test_receipt']['sha256'])
    require(tests.get('status')=='PASS' and tests.get('operator_sha256')==digest(Path(__file__)) and
            tests.get('failed')==0 and tests.get('tests',0)>0,'Current qualification tests required')
    return go


def run(manifest_path,manifest_sha,case_id,root,name,go_path,go_sha):
    manifest,decision_id,case,fixture,policy=pins(manifest_path,manifest_sha,case_id)
    v2.component(name)
    root=root.absolute()
    # Preserve current ACL drift check inside held ancestor locks.
    with ExitStack() as stack:
        stack.enter_context(lock_chain_complete(root))
        observed=collect_acl(root)
        go=qualification(go_path,go_sha,manifest_sha,case_id,root,observed)
        inputs=[(manifest_path,manifest_sha),(go_path,go_sha),(ROOT/case['script'],case['script_sha256']),
                (ROOT/'BESHStat.Excel.Common.Tests/TestData/04A02'/(case_id+'.json'),case['fixture_file_sha256'])]
        inputs += [(ROOT/p,h) for p,h in manifest['operator_files'].items()]
        inputs += [(ROOT/p,h) for p,h in v3.POLICIES.items()]
        inputs += [(ROOT/p['path'],p['sha256']) for p in fixture['source_pins']]
        inputs += [(ROOT/manifest['owner_receipt']['path'],OWNER_RECEIPT_SHA)]
        inputs += [(ROOT/'docs/architecture/04A02/oracle-proposals/manifest-v2.json',v3.FROZEN_MANIFEST)]
        runtime=policy['runtime_proposal']
        inputs += [(R_HOME/'bin/x64/Rscript.exe',runtime['rscript_sha256'])]
        inputs += [(R_HOME/v2.relative_component_path(p['relative_path']),p['sha256']) for p in runtime['runtime_components']]
        inputs += [(Path(go[k]['path']),go[k]['sha256']) for k in ('acl_review','startup_receipt','test_receipt')]
        startup=v3.pinned(Path(go['startup_receipt']['path']),go['startup_receipt']['sha256'])
        inputs += [(Path(startup['job'])/'raw-stdout.bin',startup['stdout_sha256']),
                   (Path(startup['job'])/'raw-stderr.bin',sha(b''))]
        for path,expected in inputs:
            stack.enter_context(lock_chain_complete(path.parent))
            handle=stack.enter_context(v2.locked_path(path))
            require(hashlib.file_digest(handle,'sha256').hexdigest()==expected,'Locked input drift: '+str(path))
        job=root/name
        job.mkdir(exist_ok=False)
        stack.enter_context(locked_directory(job))
        try:
            env=v3.private_environment(job,R_HOME)
            runtime_set=sha(canonical({p['relative_path']:p['sha256'] for p in runtime['runtime_components']}))
            dcf=dict(zip(v2.DCF_FIELDS,[decision_id,case_id,'4.5.1','4.5.1',case['script_sha256'],runtime['rscript_sha256'],runtime_set,
                                      case['method_id'],case['tail_policy'],case['finite_policy']]))
            v2.exclusive_bytes(job/'approved-input.dcf',v2.make_dcf(dcf))
            v2.exclusive_bytes(job/'proposal-source.R',(ROOT/case['script']).read_bytes())
            command=[str(R_HOME/'bin/x64/Rscript.exe'),'--vanilla',str(job/'proposal-source.R'),str(job/'approved-input.dcf')]
            v2.exclusive_bytes(job/'preflight-v4.json',canonical(dict(status='R_TECHNICALLY_QUALIFIED_NOT_GOLDEN',
                go=go,manifest=manifest,command=command,env=env,cwd=str(job),timeout=30)))
            with v2.locked_path(job/'proposal-source.R'),v2.locked_path(job/'approved-input.dcf'):
                v3.execute_protocol(command,job,env,decision_id,case,fixture,policy,runtime_set)
            return job
        except BaseException as error:
            v2.exclusive_bytes(job/('failure-'+uuid.uuid4().hex+'.json'),canonical(dict(status='FAILED_RETAINED_NO_RETRY',
                type=type(error).__name__,message=str(error))))
            raise


if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--manifest',type=Path,required=True);p.add_argument('--manifest-sha256',required=True)
    p.add_argument('--case',choices=tuple(v3.CASES));p.add_argument('--root',type=Path,required=True)
    p.add_argument('--name',required=True);p.add_argument('--go',type=Path);p.add_argument('--go-sha256')
    p.add_argument('--startup-only',action='store_true');p.add_argument('--acl-review',type=Path);p.add_argument('--acl-review-sha256')
    a=p.parse_args()
    if a.startup_only:
        require(a.acl_review and a.acl_review_sha256,'Exact ACL review pin required')
        probe(a.manifest,a.manifest_sha256,a.root,a.name,a.acl_review,a.acl_review_sha256)
    else:
        require(a.case and a.go and a.go_sha256,'Per-case technical GO required')
        run(a.manifest,a.manifest_sha256,a.case,a.root,a.name,a.go,a.go_sha256)
