"""C11 pinned successor: only bundled R 4.5.1 compiler admission is added.

Frozen v4/v3/v2 ACL, owner identity, locking, command and publication primitives
are reused unchanged. Imports execute no R. Old failed attempts are immutable.
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
FROZEN_V4_SHA='2fbc57aa49b0fbe6a5229be831a4fb3fc42a0b5bba4f174509c65bff5e20884b'
FROZEN_MANIFEST_SHA='6c7995b70590f84e55235f980209544e59a6d963db4b197e64a374bc39c0393f'
FROZEN_MANIFEST='docs/architecture/04A02/oracle-proposals/manifest-v4-qualified-v3.json'
POLICY='docs/architecture/04A02/04A02-C11-namespace-policy-v1.json'
v4_path=ROOT/'scripts/04a02/oracle_operator_v4.py'
if hashlib.sha256(v4_path.read_bytes()).hexdigest()!=FROZEN_V4_SHA:raise ValueError('Frozen v4 drift before import')
spec=importlib.util.spec_from_file_location('c11_frozen_v4',v4_path)
v4=importlib.util.module_from_spec(spec);spec.loader.exec_module(v4)
v3,v2=v4.v3,v4.v2
require,Refused,digest,sha,canonical=v4.require,v4.Refused,v4.digest,v4.sha,v4.canonical
R_HOME,OWNER_SID,OWNER_RECEIPT_SHA=v4.R_HOME,v4.OWNER_SID,v4.OWNER_RECEIPT_SHA
assess_acl,collect_acl,locked_directory,lock_chain_complete=v4.assess_acl,v4.collect_acl,v4.locked_directory,v4.lock_chain_complete
STOCK_NAMESPACES={'base','utils','stats','tools','methods','graphics','grDevices','datasets'}
DEPENDENCIES=v4.DEPENDENCIES|{'scripts/04a02/oracle_operator_v5.py','scripts/04a02/r_startup_v2.R',POLICY,'scripts/tests/test_04a02_checkpoint11.py','scripts/tests/fixtures/04a02-c11-retained-startup-stdout.json'}


def validate_namespace_policy(policy):
    require(policy.get('schema')=='04A02-C11-R-namespace-policy-v1','Namespace policy schema')
    require(policy.get('r')==policy.get('stats')=='4.5.1' and policy.get('arch')=='x86_64','Namespace policy runtime')
    require(policy.get('r_home')==R_HOME.as_posix(),'Namespace policy home')
    require(policy.get('allowed_namespaces')==sorted(STOCK_NAMESPACES|{'compiler'}),'Only compiler policy extension allowed')
    compiler=policy.get('compiler',{})
    require(compiler.get('version')=='4.5.1' and compiler.get('priority')=='base' and
            compiler.get('package_path')==(R_HOME/'library/compiler').as_posix(),'Bundled compiler policy identity')
    files=compiler.get('files',[]);seen=set()
    require(bool(files),'Compiler hashes required')
    for entry in files:
        relative=v2.relative_component_path(entry['relative_path']).as_posix()
        require(relative.startswith('library/compiler/') and relative not in seen,'Compiler scope/duplicate')
        v2.hex_sha(entry['sha256']);seen.add(relative)
    require({'library/compiler/DESCRIPTION','library/compiler/NAMESPACE','library/compiler/Meta/nsInfo.rds',
             'library/compiler/Meta/package.rds','library/compiler/R/compiler','library/compiler/R/compiler.rdb',
             'library/compiler/R/compiler.rdx'}<=seen,'Compiler executable/namespace metadata pins incomplete')


def verify_compiler_files(policy):
    validate_namespace_policy(policy)
    package=R_HOME/'library/compiler';v2.no_reparse(package)
    expected={e['relative_path']:e['sha256'] for e in policy['compiler']['files']}
    actual=set()
    for path in package.rglob('*'):
        v2.no_reparse(path)
        if path.is_file():actual.add(path.relative_to(R_HOME).as_posix())
    require(actual==set(expected),'Compiler package membership drift')
    for relative,expected_sha in expected.items():require(digest(R_HOME/relative)==expected_sha,'Compiler package hash drift: '+relative)
    fields=dict(line.split(': ',1) for line in (package/'DESCRIPTION').read_text(encoding='utf-8').splitlines() if ': ' in line and not line.startswith(' '))
    require(fields.get('Package')=='compiler' and fields.get('Version')=='4.5.1' and fields.get('Priority')=='base','Compiler DESCRIPTION provenance')


def pins(manifest_path,manifest_sha,case_id):
    manifest=v3.pinned(manifest_path,manifest_sha)
    require(manifest.get('schema')=='04A02-oracle-execution-manifest-v5','Versioned v5 manifest required')
    require(manifest.get('source_manifest')==dict(path=FROZEN_MANIFEST,sha256=FROZEN_MANIFEST_SHA),'Frozen v4 manifest link')
    old,decision_id,case,fixture,frozen_policy=v4.pins(ROOT/FROZEN_MANIFEST,FROZEN_MANIFEST_SHA,case_id)
    require(manifest.get('cases')==old['cases'] and manifest.get('P')=='NO' and manifest.get('R_owner_approved') is True,'Scientific scope/authority/promotion drift')
    require(manifest.get('owner_receipt')==old['owner_receipt'] and manifest.get('r_home')==str(R_HOME),'Owner/runtime drift')
    require(set(manifest.get('operator_files',{}))==DEPENDENCIES,'Missing/extra v5 source dependencies')
    for relative,expected in manifest['operator_files'].items():require(digest(ROOT/relative)==expected,'V5 source pin drift: '+relative)
    require(manifest['operator_files']['scripts/04a02/oracle_operator_v5.py']==digest(Path(__file__)),'V5 self pin')
    ref=manifest.get('namespace_policy',{})
    require(ref==dict(path=POLICY,sha256=manifest['operator_files'][POLICY]),'Namespace policy pin')
    namespace_policy=v3.pinned(ROOT/POLICY,ref['sha256']);verify_compiler_files(namespace_policy)
    # Runtime extension is a new execution-local object; frozen scientific proposal
    # bytes, algorithm/tolerances and the original base runtime pins do not change.
    policy=copy.deepcopy(frozen_policy)
    policy['runtime_proposal']['runtime_components']+=copy.deepcopy(namespace_policy['compiler']['files'])
    policy['namespace_validation']=namespace_policy
    return manifest,decision_id,case,fixture,policy


def validate_probe(value,job,namespace_policy):
    validate_namespace_policy(namespace_policy)
    require(value.get('schema')=='04A02-C11-R-startup-v2','C11 startup schema; old receipt cannot be replayed')
    names=value.get('namespaces')
    require(isinstance(names,list) and all(isinstance(n,str) for n in names) and len(set(names))==len(names),'Namespace list shape')
    # Every original v4 runtime/path/profile/library check remains active. Remove
    # only compiler from this in-memory projection; never mutate retained stdout.
    original=dict(value,schema='04A02-C09-R-startup-v1',namespaces=[n for n in names if n!='compiler'])
    v4.validate_probe(original,job)
    if 'compiler' in names:
        package=(R_HOME/'library/compiler').as_posix()
        require(value.get('compiler')==dict(namespace_path=package,namespace_version='4.5.1',
                package_path=package,package_version='4.5.1'),'Compiler namespace/package provenance mismatch')
    else:require(value.get('compiler') is None,'Unloaded compiler provenance mismatch')
    empty=(job/'empty-startup').as_posix()
    environment=dict(HOME=(job/'home').as_posix(),R_HOME=R_HOME.as_posix(),R_PROFILE_USER=empty,R_ENVIRON_USER=empty,
        R_LIBS='',R_LIBS_USER=(job/'user-library').as_posix(),R_LIBS_SITE=(job/'site-library').as_posix(),R_DEFAULT_PACKAGES='utils,stats')
    require(value.get('environment')==environment,'Private environment escape')


def new_attempt_path(root,name):
    v2.component(name)
    require(name!='c10-startup-20261101-v1','Preserved C10 attempt must never be reused')
    job=root.absolute()/name
    require(not job.exists(),'Attempt already exists; no retry/overwrite')
    return job


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
            validate_probe(v2.strict_json((job/'raw-stdout.bin').read_bytes()),job,policy['namespace_validation'])
            v2.publish_complete(job,'startup-receipt.json',canonical(dict(schema='04A02-C11-startup-receipt-v1',status='R_RUNTIME_PROBED',job=str(job),namespace_policy=manifest['namespace_policy'],
                manifest_sha256=manifest_sha,stdout_sha256=digest(job/'raw-stdout.bin'),runtime=runtime,
                loader_bound='Exact bundled runtime/base/stats pins plus observed startup; OS API-set/SxS/system maintenance remain trusted Windows boundaries. No third-party packages or arbitrary DLL closure claimed.')))
            return job
        except BaseException as error:
            v2.exclusive_bytes(job/('failure-'+uuid.uuid4().hex+'.json'),canonical(dict(status='FAILED_RETAINED_NO_RETRY',error=str(error))))
            raise


def qualification(go_path,expected,manifest_sha,case_id,root,observed,namespace_policy):
    go=v3.pinned(go_path,expected)
    require(go.get('schema')=='04A02-C11-R-technical-v1' and go.get('status')=='R_TECHNICAL_GO','No current technical GO')
    require(go.get('case_id')==case_id and go.get('manifest_sha256')==manifest_sha and
            go.get('output_root')==str(root.absolute()),'Wrong case/manifest/output root')
    require(not assess_acl(observed,root),'Owner-context ACL gate failed')
    require(go.get('acl_sha256')==sha(canonical(observed)),'ACL drift/stale review')
    review=v3.pinned(Path(go['acl_review']['path']),go['acl_review']['sha256'])
    require(review.get('execution_sid')==OWNER_SID and review.get('reviewer') and review.get('source') and
            review.get('observed_sha256')==go['acl_sha256'] and review.get('decision')=='ACCEPT_EXACT_ACL_AND_THREAT_MODEL_V4',
            'Actual attributable owner ACL review required')
    probe=v3.pinned(Path(go['startup_receipt']['path']),go['startup_receipt']['sha256'])
    require(probe.get('schema')=='04A02-C11-startup-receipt-v1' and probe.get('status')=='R_RUNTIME_PROBED' and probe.get('manifest_sha256')==manifest_sha,'Missing executed startup qualification')
    policy_ref=dict(path=POLICY,sha256=digest(ROOT/POLICY))
    require(probe.get('namespace_policy')==policy_ref and go.get('namespace_policy')==policy_ref,'Startup/GO namespace policy drift')
    raw=Path(probe['job'])/'raw-stdout.bin'
    require(digest(raw)==probe['stdout_sha256'] and (raw.parent/'raw-stderr.bin').read_bytes()==b'','Probe raw drift/stderr')
    validate_probe(v2.strict_json(raw.read_bytes()),raw.parent,namespace_policy)
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
        go=qualification(go_path,go_sha,manifest_sha,case_id,root,observed,policy['namespace_validation'])
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
            v2.exclusive_bytes(job/'preflight-v5.json',canonical(dict(status='R_TECHNICALLY_QUALIFIED_NOT_GOLDEN',
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
