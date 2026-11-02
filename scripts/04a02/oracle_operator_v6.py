"""C12 evidence-justified Windows path identity successor. No R on import.

Frozen v5/v4/v3/v2 and R startup v2 remain unchanged. Only authenticated
compiler and R_HOME path representations are projected to their trusted names.
"""
from contextlib import ExitStack
import argparse
import copy
import hashlib
import importlib.util
import os
from pathlib import Path
import subprocess
import uuid

ROOT=Path(__file__).resolve().parents[2]
FROZEN_V5_SHA='9fe7b00eeab346e5c3964497cf91d98101f4cbc220cacafa5bdc64163ceebe1b'
FROZEN_MANIFEST='docs/architecture/04A02/oracle-proposals/manifest-v5-c11-v1.json'
FROZEN_MANIFEST_SHA='a25243a3baa84d6950a473641c7a2fca53d729608a70361b6b14a2e97d4ced29'
PATH_POLICY='docs/architecture/04A02/04A02-C12-path-identity-policy-v1.json'
for relative,expected in [('scripts/04a02/oracle_operator_v5.py',FROZEN_V5_SHA),
                          ('scripts/04a02/windows_r_path_identity_v1.py','623227969878b298606cd519ed8e0318feb8d96d6dbbffe8e57b3b0180a26b4b')]:
    if hashlib.sha256((ROOT/relative).read_bytes()).hexdigest()!=expected:raise ValueError('Frozen/input drift before import: '+relative)
def _module(name,relative):
    spec=importlib.util.spec_from_file_location(name,ROOT/relative)
    result=importlib.util.module_from_spec(spec);spec.loader.exec_module(result);return result
v5=_module('c12_frozen_v5','scripts/04a02/oracle_operator_v5.py')
identity=_module('c12_windows_identity','scripts/04a02/windows_r_path_identity_v1.py')
v4,v3,v2=v5.v4,v5.v3,v5.v2
require,Refused,digest,sha,canonical=v5.require,v5.Refused,v5.digest,v5.sha,v5.canonical
R_HOME,OWNER_SID,OWNER_RECEIPT_SHA=v5.R_HOME,v5.OWNER_SID,v5.OWNER_RECEIPT_SHA
POLICY=v5.POLICY
assess_acl,collect_acl,locked_directory,lock_chain_complete=v5.assess_acl,v5.collect_acl,v5.locked_directory,v5.lock_chain_complete
DEPENDENCIES=v5.DEPENDENCIES|{'scripts/04a02/oracle_operator_v6.py','scripts/04a02/windows_r_path_identity_v1.py',PATH_POLICY,'scripts/tests/test_04a02_checkpoint12.py','scripts/tests/fixtures/04a02-c12-retained-c11-stdout.json'}


def validate_path_policy(policy):
    require(policy.get('schema')=='04A02-C12-Windows-path-policy-v1' and policy.get('drive')=='C:' and
            policy.get('filesystem')=='NTFS' and policy.get('mechanism')=='HELD_NONREPARSE_DIRECTORY_CHAINS_VOLUME_AND_FILE_ID','Unqualified Windows identity policy')
    require(policy.get('representations')==['absolute_DOS_drive_C','slash_direction','actual_Windows_case_equivalence','actual_8dot3_alias_verified_by_file_identity'],'Path policy representation drift')
    trusted=policy.get('trusted',{})
    require(set(trusted)=={'r_home','compiler'},'Trusted directory scope')
    for key,path in [('r_home',R_HOME.as_posix()),('compiler',(R_HOME/'library/compiler').as_posix())]:
        entry=trusted[key];require(entry.get('path')==path,'Trusted directory root drift: '+key)
        token=entry.get('identity',{})
        require(set(token)=={'filesystem','volume_serial','file_index'} and token['filesystem']=='NTFS' and
                type(token['volume_serial']) is int and 0<token['volume_serial']<=0xffffffff and
                type(token['file_index']) is int and 0<token['file_index']<=0xffffffffffffffff,'Invalid pinned directory identity')
    require(trusted['r_home']['identity']['volume_serial']==trusted['compiler']['identity']['volume_serial'],'Trusted roots cross volumes')


def pins(manifest_path,manifest_sha,case_id):
    manifest=v3.pinned(manifest_path,manifest_sha)
    require(manifest.get('schema')=='04A02-oracle-execution-manifest-v6','Versioned v6 manifest required')
    require(manifest.get('source_manifest')==dict(path=FROZEN_MANIFEST,sha256=FROZEN_MANIFEST_SHA),'Frozen v5 manifest link')
    old,decision_id,case,fixture,policy=v5.pins(ROOT/FROZEN_MANIFEST,FROZEN_MANIFEST_SHA,case_id)
    require(manifest.get('cases')==old['cases'] and manifest.get('P')=='NO' and manifest.get('R_owner_approved') is True,'Scientific scope/authority/promotion drift')
    require(manifest.get('owner_receipt')==old['owner_receipt'] and manifest.get('r_home')==old['r_home'] and manifest.get('namespace_policy')==old['namespace_policy'],'Owner/runtime/compiler namespace policy drift')
    require(set(manifest.get('operator_files',{}))==DEPENDENCIES,'Missing/extra v6 dependencies')
    for relative,expected in manifest['operator_files'].items():require(digest(ROOT/relative)==expected,'V6 source pin drift: '+relative)
    require(manifest['operator_files']['scripts/04a02/oracle_operator_v6.py']==digest(Path(__file__)),'V6 self pin')
    reference=dict(path=PATH_POLICY,sha256=manifest['operator_files'][PATH_POLICY])
    require(manifest.get('path_identity_policy')==reference,'Path policy pin drift')
    path_policy=v3.pinned(ROOT/PATH_POLICY,reference['sha256']);validate_path_policy(path_policy)
    require(path_policy.get('namespace_policy')==old['namespace_policy'],'Compiler policy is immutable')
    policy['path_identity_validation']=path_policy
    return manifest,decision_id,case,fixture,policy


def validate_probe(value,job,namespace_policy,path_policy):
    v5.validate_namespace_policy(namespace_policy);validate_path_policy(path_policy)
    require(isinstance(value,dict) and value.get('schema')=='04A02-C11-R-startup-v2','Pinned R startup v2 schema required')
    names=value.get('namespaces')
    require(isinstance(names,list) and all(isinstance(n,str) for n in names) and len(set(names))==len(names),'Namespace list shape')
    v4.validate_probe(dict(value,schema='04A02-C09-R-startup-v1',namespaces=[n for n in names if n!='compiler']),job)
    projected=copy.deepcopy(value);targets=[]
    if 'compiler' in names:
        compiler=value.get('compiler')
        require(isinstance(compiler,dict) and set(compiler)=={'namespace_path','namespace_version','package_path','package_version'},'compiler metadata missing/extra fields')
        for field in ['namespace_version','package_version']:
            require(compiler[field]=='4.5.1',f'compiler.{field} mismatch: expected 4.5.1; observed {compiler[field]!r}')
        for field in ['namespace_path','package_path']:
            targets.append(('compiler.'+field,compiler[field],'compiler'))
    environment=value.get('environment')
    require(isinstance(environment,dict) and 'R_HOME' in environment,'environment.R_HOME metadata missing')
    targets.append(('environment.R_HOME',environment['R_HOME'],'r_home'))
    evidence=[]
    with ExitStack() as stack:
        for field,observed,key in targets:
            entry=path_policy['trusted'][key]
            try:proof=stack.enter_context(identity.same_trusted_directory(observed,entry['path'],entry['identity']))
            except identity.PathIdentityError as error:raise Refused(f'{field}: {error}; observed={observed!r}; expected={entry["path"]!r}') from error
            evidence.append(dict(field=field,**proof))
            section,name=field.split('.')
            projected[section][name]=entry['path']
        # All immutable v5 version/package/namespace/environment/profile/library
        # checks remain active. Only three proven equivalent path spellings differ.
        v5.validate_probe(projected,job,namespace_policy)
    return evidence


def new_attempt_path(root,name):
    require(name not in {'c10-startup-20261101-v1','c11-startup-20261101-v1'},'Preserved failed attempt name must never be reused')
    return v5.new_attempt_path(root,name)


def probe(manifest_path,manifest_sha,root,name,review_path,review_sha):
    """Nonstatistical R startup only after actual-owner ACL review; never a case GO."""
    new_attempt_path(root,name)
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
        job=root/name;job.mkdir(exist_ok=False)
        stack.enter_context(locked_directory(job))
        try:
            env=v3.private_environment(job,R_HOME)
            source=ROOT/'scripts/04a02/r_startup_v2.R'
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
            path_evidence=validate_probe(v2.strict_json((job/'raw-stdout.bin').read_bytes()),job,policy['namespace_validation'],policy['path_identity_validation'])
            v2.publish_complete(job,'startup-receipt.json',canonical(dict(schema='04A02-C12-startup-receipt-v1',status='R_RUNTIME_PROBED',job=str(job),namespace_policy=manifest['namespace_policy'],path_identity_policy=manifest['path_identity_policy'],path_identity_evidence=path_evidence,
                manifest_sha256=manifest_sha,stdout_sha256=digest(job/'raw-stdout.bin'),runtime=runtime,
                loader_bound='Exact bundled runtime/base/stats pins plus observed startup; OS API-set/SxS/system maintenance remain trusted Windows boundaries. No third-party packages or arbitrary DLL closure claimed.')))
            return job
        except BaseException as error:
            v2.exclusive_bytes(job/('failure-'+uuid.uuid4().hex+'.json'),canonical(dict(status='FAILED_RETAINED_NO_RETRY',error=str(error))))
            raise


def qualification(go_path,expected,manifest_sha,case_id,root,observed,namespace_policy,path_policy):
    go=v3.pinned(go_path,expected)
    require(go.get('schema')=='04A02-C12-R-technical-v1' and go.get('status')=='R_TECHNICAL_GO','No current technical GO')
    require(go.get('case_id')==case_id and go.get('manifest_sha256')==manifest_sha and
            go.get('output_root')==str(root.absolute()),'Wrong case/manifest/output root')
    require(not assess_acl(observed,root),'Owner-context ACL gate failed')
    require(go.get('acl_sha256')==sha(canonical(observed)),'ACL drift/stale review')
    review=v3.pinned(Path(go['acl_review']['path']),go['acl_review']['sha256'])
    require(review.get('execution_sid')==OWNER_SID and review.get('reviewer') and review.get('source') and
            review.get('observed_sha256')==go['acl_sha256'] and review.get('decision')=='ACCEPT_EXACT_ACL_AND_THREAT_MODEL_V4',
            'Actual attributable owner ACL review required')
    probe=v3.pinned(Path(go['startup_receipt']['path']),go['startup_receipt']['sha256'])
    require(probe.get('schema')=='04A02-C12-startup-receipt-v1' and probe.get('status')=='R_RUNTIME_PROBED' and probe.get('manifest_sha256')==manifest_sha,'Missing executed startup qualification')
    policy_ref=dict(path=POLICY,sha256=digest(ROOT/POLICY))
    require(probe.get('namespace_policy')==policy_ref and go.get('namespace_policy')==policy_ref,'Startup/GO namespace policy drift')
    raw=Path(probe['job'])/'raw-stdout.bin'
    require(digest(raw)==probe['stdout_sha256'] and (raw.parent/'raw-stderr.bin').read_bytes()==b'','Probe raw drift/stderr')
    path_ref=dict(path=PATH_POLICY,sha256=digest(ROOT/PATH_POLICY))
    require(probe.get('path_identity_policy')==path_ref and go.get('path_identity_policy')==path_ref,'Startup/GO path identity policy drift')
    path_evidence=validate_probe(v2.strict_json(raw.read_bytes()),raw.parent,namespace_policy,path_policy)
    require(probe.get('path_identity_evidence')==path_evidence,'Startup directory identity evidence drift')
    tests=v3.pinned(Path(go['test_receipt']['path']),go['test_receipt']['sha256'])
    require(tests.get('status')=='PASS' and tests.get('operator_sha256')==digest(Path(__file__)) and
            tests.get('failed')==0 and tests.get('tests',0)>0,'Current qualification tests required')
    return go


def run(manifest_path,manifest_sha,case_id,root,name,go_path,go_sha):
    new_attempt_path(root,name)
    manifest,decision_id,case,fixture,policy=pins(manifest_path,manifest_sha,case_id)
    v2.component(name)
    root=root.absolute()
    # Preserve current ACL drift check inside held ancestor locks.
    with ExitStack() as stack:
        stack.enter_context(lock_chain_complete(root))
        observed=collect_acl(root)
        go=qualification(go_path,go_sha,manifest_sha,case_id,root,observed,policy['namespace_validation'],policy['path_identity_validation'])
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
            v2.exclusive_bytes(job/'preflight-v6.json',canonical(dict(status='R_TECHNICALLY_QUALIFIED_NOT_GOLDEN',
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
