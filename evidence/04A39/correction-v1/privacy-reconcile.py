from common import *
from scanner_adapter import *
import collections

lock()
prior=load(PRIVATE/'privacy-13-replay-v2.json')
m13,m14=module('13'),module('14')
assert function_ast(m13)==function_ast(m14)
paths13=prior['candidates']
paths14=sorted((set(paths13)-{m13.MANIFEST,m13.PRIVACY})|{m14.MANIFEST,m14.PRIVACY})
findings,reviewed,details=detailed(m13,paths13)
assert collections.Counter(json.dumps(r,sort_keys=True) for r in findings)==collections.Counter(json.dumps(r,sort_keys=True) for r in prior['findings'])
f14,r14=m14.scan(paths14)
assert findings==f14 and len(findings)==1599
save(PRIVATE/'privacy-14-replay-v3.json',dict(candidates=paths14,findings=f14,reviewed=r14,source=pin(m14.__file__),unstarted_interruption='v2 interrupted during second scanner read; no test identity executed. Exact AST proof and unchanged-source guarded replay here.'))
filepins={p:pin(p) for p in sorted({r['path'] for r in findings})}
for p in filepins:assert p.startswith('evidence/04A39/')
canonical={};public=[];private=[]
for i,row in enumerate(details):
    # Same matched bytes can be repeated on many lines and in JSON views.
    # Keep scanner occurrences and exposure groups as separate populations.
    key=(row['path'],row['category'],row['match_sha256'])
    group=canonical.setdefault(key,'EXPOSURE-'+str(len(canonical)+1).zfill(4))
    identity='A39-PRIVACY-'+str(i+1).zfill(4)
    raw=dict(finding_id=identity,**row,source=filepins[row['path']],exposure_group=group,parents=['04A13','04A14'])
    private.append(raw)
    public.append(dict(finding_id=identity,source_id='SOURCE-'+sha(row['path'].encode())[:16],source_sha256=filepins[row['path']]['sha256'],category=row['category'],origin=row['origin'],line=row.get('line'),
        exposure_group=group,scanner_occurrences=2,classification='NEW_04A39_DIAGNOSTIC_EXPOSURE',area='NEW_04A39_EVIDENCE',
        concern='ABSOLUTE_PATH_OR_MACHINE_IDENTIFIER',severity='RESTRICTED_DIAGNOSTIC',disposition='PROTECTED_LOCAL_ONLY_EXCLUDE_RAW_UPLOAD_INCLUDE_SANITIZED_DERIVATIVE',
        authorization='PUBLICATION_SEPARATION_AUTHORIZED;IMMUTABLE_ORIGINAL_CHANGE_NOT_AUTHORIZED',required_action='Preserve exact original; gate staged derivative; original Git candidate parent remains FAIL.',
        resolution='DERIVATIVE_REPAIR_REQUIRED;ROOT_CANDIDATE_NONPASS_UNWAIVED'))
save(PRIVATE/'privacy-findings-restricted.json',dict(files=filepins,findings=private,scanner13=pin(m13.__file__),scanner14=pin(m14.__file__)))
save('privacy/finding-reconciliation.json',dict(findings=public,scanner_findings_per_parent=1599,scanner_occurrences=3198,identical_cross_parent_occurrences=1599,
    within_parent_exposure_groups=len(canonical),distinct_finding_source_files=len(filepins),categories=dict(collections.Counter(r['category'] for r in findings)),
    no_secret_pattern_findings_in_this_domain=True,privacy_clean=False,source_mapping=pin(PRIVATE/'privacy-findings-restricted.json'),
    historical_errors='Both W3 UnicodeDecodeError events retained, not waived. Different candidate domain; no assertion change. Replayed domain from frozen Git publication paths, not a new original reference.',
    candidates13=len(paths13),candidates14=len(paths14),sources=dict(scanner13=pin(m13.__file__),scanner14=pin(m14.__file__),guard=pin(GUARD))))
print('Exact replay: 1599 per parent; overlap1599; exposure groups',len(canonical),'files',len(filepins),flush=True)
