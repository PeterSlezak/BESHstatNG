"""Standalone physical readback. External SHA required; optional original ZIP integrity/key proof."""
import argparse,ast,collections,hashlib,json,re,unicodedata,zipfile
from pathlib import Path

def sha(data):return hashlib.sha256(data).hexdigest()
def file_sha(path):
    with Path(path).open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def readback(path,expected,main=None,postseal=None):
    assert file_sha(path)==expected,'EXTERNAL_WITNESS_ZIP_SHA_MISMATCH'
    with zipfile.ZipFile(path) as z:
        names=z.namelist();assert len(names)==len(set(names))==len({n.casefold() for n in names})
        for name in names:assert not name.startswith('/') and '\\' not in name and ':' not in name and all(p not in ('','.','..') for p in name.split('/'))
        index=json.loads(z.read('.witness-inventory.json'));files={r['path']:r for r in index['files']}
        assert set(names)==set(files)|{'.witness-inventory.json'} and len(files)==len(index['files'])
        for name,row in files.items():
            data=z.read(name);assert sha(data)==row['sha256'] and len(data)==row['bytes'] and f'{z.getinfo(name).CRC:08x}'==row['crc32']
        prefix='evidence/04A39/physical-witness-v1/'
        ready=json.loads(z.read(prefix+'readiness.json'))
        assert ready['status']=='READY_FOR_OWNER_04A39_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
        assert ready['publication']=='PHYSICAL_DERIVATIVE_PUBLICATION_PASS' and ready['repository_privacy']=='REPOSITORY_CANDIDATE_PRIVACY_FAIL_UNWAIVED'
        assert not ready['owner39_acceptance'] and not ready['accepted_or_merged']
        assert sha(z.read(prefix+'dependencies/privacy_regex_guard.py'))=='ad17f82adca850afb94fa60d3c0ca5866b446860a7724dc77a5e5b3111c9ce5f'
        assert sha(z.read('scripts/verify-04a13-publication.py'))=='b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03'
        assert sha(z.read('scripts/verify-04a14-publication.py'))=='b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25'
        proof=json.loads(z.read(prefix+'exact-25-fixtures.json'));assert len(proof['fixtures'])==25
        sdk=json.loads(z.read(prefix+'sdk-HEAD-proof.json'));head=z.read('BESHstatNG/BESHStatNG.vbproj')
        assert sha(head)==sdk['HEAD_source']['sha256'] and len(head)==sdk['HEAD_source']['bytes']
        physical=json.loads(z.read(prefix+'physical-inputs.json'))
        guard_controls=json.loads(z.read(prefix+'guard-controls.json'));assert guard_controls['positive_email_samples']>0
        checked_inputs=0;checked_keys=0
        for argument,label in [(main,'main'),(postseal,'postseal')]:
            if argument is None:continue
            packet=next(p for p in physical['archives'] if p['label']==label)
            assert file_sha(argument)==packet['archive']['sha256'],'PINNED_ORIGINAL_ZIP_CHANGED'
            with zipfile.ZipFile(argument) as source:
                expected_files={r['path']:r for r in packet['files']}
                assert len(source.namelist())==len(expected_files) and set(source.namelist())==set(expected_files)
                for name,row in expected_files.items():
                    raw=source.read(name);assert len(raw)==row['bytes'] and sha(raw)==row['sha256'] and f'{source.getinfo(name).CRC:08x}'==row['crc32']
                if label=='main':
                    # Extract the exact historical rules from the authenticated scanner AST.
                    module=ast.parse(z.read('scripts/verify-04a13-publication.py').decode('utf-8-sig'))
                    scan=next(n for n in module.body if isinstance(n,ast.FunctionDef) and n.name=='scan')
                    patterns=ast.literal_eval(next(n.value for n in ast.walk(scan) if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='patterns' for t in n.targets)))
                    for row in proof['fixtures']:
                        key=row['key'];name,expected_file_sha,origin,line,category,offset,match_sha,codec=key
                        raw=source.read(name);assert sha(raw)==expected_file_sha and origin=='content'
                        value=unicodedata.normalize('NFKC',raw.decode(codec,errors='strict'))
                        inspected=value if category=='unc_share' else value.replace('\\\\','\\')
                        matches=[m for m in re.finditer(patterns[category],inspected) if m.start()==offset]
                        assert len(matches)==1 and inspected.count('\n',0,offset)+1==line
                        assert sha(matches[0].group().replace('\\\\','\\').casefold().encode())==match_sha
                        assert sha(matches[0].group().encode())==row['raw_match_sha256']
                        checked_keys+=1
                    body=source.read('BESHstatNG/BESHStatNG.vbproj').decode('utf-8-sig')
                    sdkpattern=next(n.args[0].value for n in ast.walk(scan) if isinstance(n,ast.Call) and isinstance(n.func,ast.Attribute) and n.func.attr=='match' and n.args and isinstance(n.args[0],ast.Constant) and isinstance(n.args[0].value,str) and 'signtool' in n.args[0].value)
                    for occurrence in sdk['occurrences']:
                        literal=re.search(sdkpattern,body.splitlines()[occurrence['line']-1],re.I)
                        assert literal and literal[0] in head.decode('utf-8-sig') and sha(literal[0].encode())==occurrence['full_SDK_literal_sha256']
            checked_inputs+=1
        return dict(status='PASS_PHYSICAL_DOSSIER_READBACK',witness_members=len(names),checked_original_ZIPs=checked_inputs,checked_synthetic_keys=checked_keys,
            owner_local_execution='Fresh owner-machine witness attested; private token values intentionally not exported.',overall39=ready['status'],accepted=False)
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('archive');p.add_argument('--sha256',required=True);p.add_argument('--main');p.add_argument('--postseal');args=p.parse_args()
    print(json.dumps(readback(args.archive,args.sha256,args.main,args.postseal),sort_keys=True))
