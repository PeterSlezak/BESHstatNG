"""Original candidate scope, unchanged scanners, protected values never published."""
from common40_v3 import *
import collections
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'));import physical
lock();assert physical.tokens();reports=[];union={}
for phase in ['13','14']:
    m=physical.scanner(phase);candidates=[p for p in m.paths() if (ROOT/p).is_file()]
    inputs=[pin(p) for p in candidates];found,reviewed=m.scan(candidates)
    assert inputs==[pin(p) for p in candidates]
    rows=[]
    for occurrence,row in enumerate(found):
        src=pin(row['path']);identity=sha(json.dumps([src['path'],src['sha256'],row.get('line'),row['origin'],row['category']],sort_keys=True).encode())
        r=dict(id=identity,source=src,origin=row['origin'],category=row['category'],line=row.get('line'),
          disposition='RULE_FINDING_UNWAIVED;SOURCE_CLASSIFICATION_REQUIRED;REVIEW_DERIVATIVE_MUST_PASS',scanner_outcome='FAIL',command_exit=0)
        rows.append(r);union[identity]=r
    save(PRIVATE/('repository-privacy-'+phase+'-v1.json'),dict(scanner=pin(m.__file__),candidate_inputs=inputs,findings=found,reviewed=reviewed,owner_context_applied=True))
    reports.append(dict(scanner=phase,source=pin(m.__file__),candidates=len(candidates),findings=len(found),unique_occurrence_keys=len({r['id'] for r in rows}),
       scanner_outcome='FAIL' if found else 'NO_RULE_FINDINGS',scanner_command_exit=0,rows=rows,raw=pin(PRIVATE/('repository-privacy-'+phase+'-v1.json'))))
assert reports[0]['rows']==reports[1]['rows']
save('python/repository-privacy-witness-v1.json',dict(reports=reports,distinct_rule_occurrences=len(union),findings=list(union.values()),
    same_findings_are_not_additive=True,original_scope_unchanged=True,rules_changed=False,waivers=0,publication_separate=True,
    historical39=dict(parents=2,each=1941,earlier_each=1599,added_each=342,W3='TWO_ERROR_PARENTS_UNWAIVED'),
    finding_values_and_owner_token_fingerprints='PROTECTED_LOCAL_ONLY',scope_reason='Accepted39 raw artifacts became committed main. New40 candidate paths are actual diff/untracked plus original scanner metadata. A lower current count is not resolution or waiver of old findings.'))
print('Original repository scanner rule occurrences',len(union),'unique;',[(r['source']['path'],r['category'],r['origin'],r['line']) for r in union.values()],flush=True)
