"""Standalone physical review archive reader; no extraction, Git mutation or host work."""
import collections,hashlib,json,re,sys,zipfile
from pathlib import PurePosixPath

def digest(data):return hashlib.sha256(data).hexdigest()
def verify(path):
    with zipfile.ZipFile(path) as z:
        names=z.namelist();assert len(names)==len(set(names))==len({n.casefold() for n in names}),'DUPLICATE_OR_COLLIDING_PATH'
        for n in names:
            assert n and not n.startswith('/') and ':' not in n and '\\' not in n and all(p not in ('','.','..') and not p.endswith((' ','.')) for p in n.split('/')),'UNSAFE_PATH'
        index=json.loads(z.read('.review-index.json'));expected={r['path']:r for r in index['files']}
        assert len(expected)==len(index['files']) and set(names)==set(expected)|{'.review-index.json'},'MISSING_OR_EXTRA_MEMBER'
        for n,r in expected.items():
            data=z.read(n);info=z.getinfo(n)
            assert len(data)==r['bytes'] and digest(data)==r['sha256'] and f'{info.CRC:08x}'==r['crc32'],'MEMBER_HASH_SIZE_CRC'
        assert z.testzip() is None
        prefix='evidence/04A40/'
        read=lambda name:json.loads(z.read(prefix+name))
        d=read('build-and-tests/Debug-identities-v2.json');r=read('build-and-tests/Release-identities-v2.json')
        assert len(d['tests'])==d['summary']['identities']==21139 and d['summary']['outcomes']==dict(Passed=21125,Failed=14)
        assert len(r['tests'])==r['summary']['identities']==3110 and r['summary']['outcomes']==dict(Passed=3108,Failed=2)
        assert not d['summary']['missing'] and not r['summary']['missing']
        for config in ['Debug','Release']:
            a=read('abi-cil/'+config+'-proof-v2.json');assert a['exports']==268 and not a['unexpected']
            c=read('compatibility/'+config+'-current-only-v4.json');rows=c['case_identities']
            assert len(rows)==len({(x['corpus'],x['case_id']) for x in rows})==29424
            assert collections.Counter(x['provenance'] for x in rows)==dict(PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458)
            assert len(c['results'])==12 and all(x['exact'] and x['current']['sha256']==x['original']['sha256'] for x in c['results']) and c['original_queries']==0
            assert read('fixtures/'+config+'-39-retained-inputs-fresh-results-v2.json')['cases']==77
        p=read('python/parent-event-reconciliation-v2.json');assert len(p['parents'])==len({x['id'] for x in p['parents']})==956
        assert read('python/Node-fresh-identities-v3.json')['passed']==916
        reg=json.loads(z.read('evidence/04A39/register/defects-v7.json'));assert len(reg['issues'])==1169 and len(reg['crosswalk'])==2967
        assert len({o for x in reg['issues'] for o in x['original_obligations']})==1514
        assert len({r['original_id'] for r in reg['crosswalk'] if r['kind']=='RESTRICTED_CONTROL'})==802
        delta=read('register/issue-delta-v2.json');assert all(not x['behavior_fix_activated'] and x.get('exit_disposition_04A48') is None for x in delta['entries'])
        return dict(physical_members=len(names),indexed=len(expected),CRC_SHA_size='PASS',Debug=d['summary']['outcomes'],Release_targeted=r['summary']['outcomes'],
                    Python=p['summary']['parent_outcomes'],review_status=read('final/readiness-v1.json')['status'],privacy='External physical-original-scanner receipt required; this reader is structural verification only.')
if __name__=='__main__':print(json.dumps(verify(sys.argv[1]),sort_keys=True))
