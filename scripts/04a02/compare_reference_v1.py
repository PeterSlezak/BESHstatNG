"""Independent reference comparison, never a golden promoter or numerical algorithm.

Consumes operator raw R JSON only when a separately hashed successful publication
exists. Candidate structure uses the existing strict comparator. Reference-only
KW auxiliaries stay distinct from unavailable Common fields.
"""
import argparse
import importlib.util
import math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]


def module(name,path):
    spec=importlib.util.spec_from_file_location(name,ROOT/path)
    value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value


r=module('r09_comparison','scripts/04a02/oracle_operator_v4.py')
c=module('strict_candidate09','BESHStat.Excel.Common.Tests/src/capture_contract04a02.py')
ANOVA_CELLS={'SSBetween':(1,1),'SSWithin':(2,1),'SSTotal':(3,1),'MSBetween':(1,3),'MSWithin':(2,3),
             'DFBetween':(1,2),'DFWithin':(2,2),'DFTotal':(3,2)}


def compare(candidate,reference,policy,case,fixture,decision_id,runtime_set):
    candidate=c.validate_candidate(candidate)
    pins=dict(script_sha256=case['script_sha256'],rscript_sha256=policy['runtime_proposal']['rscript_sha256'],runtime_set_sha256=runtime_set)
    decision=dict(decision_id=decision_id,r_version='4.5.1',stats_version='4.5.1',
                  ordinary_p_range={k:policy['ordinary_p_range_proposal'][k] for k in ('min','max')})
    r.v2.validate_candidate(r.canonical(reference),b'',decision,case,fixture,pins)
    r.require(candidate['case_id']==case['case_id'],'Candidate case mismatch')
    subjects=dict(candidate['metrics'])
    if case['case_id'].endswith('ANOVA'):
        subjects.update({k:candidate['tables'][0]['cells'][row][col]['value'] for k,(row,col) in ANOVA_CELLS.items()})
        for field,coord in [('FStatistic',(1,4)),('PValue',(1,5))]:
            r.require(subjects[field]==candidate['tables'][0]['cells'][coord[0]][coord[1]]['value'],'Service/table mismatch')
    else:
        for field,row in [('HStatistic',3),('PValue',4),('TieCorrectedHStatistic',5),('TieCorrectedPValue',6)]:
            r.require(subjects[field]==candidate['tables'][0]['cells'][row][1]['value'],'KW service/table mismatch')
    fields=[]
    for rule in policy['field_policies']:
        field=rule['field'].split('[')[0];ref=reference['comparison']['metrics'][field]
        atol,rtol=rule['proposed_atol'],rule['proposed_rtol']
        if field not in subjects:
            fields.append(dict(field=field,reference=ref,subject=None,status='REFERENCE_ONLY_NO_COMMON_MEMBER',
                               atol=atol,rtol=rtol,mapping=rule['subject_mapping']))
            continue
        actual=subjects[field]
        delta=abs(actual-ref);limit=max(atol,rtol*abs(ref))
        exact=rule['rule']=='EXACT'
        ok=(type(actual) is type(ref) and actual==ref) if exact else delta<=limit
        fields.append(dict(field=field,reference=ref,subject=actual,subject_type=type(actual).__name__,
            reference_type=type(ref).__name__,delta=delta,atol=atol,rtol=rtol,limit=limit,
            calculation='abs(subject-reference) <= max(atol, rtol*abs(reference))' if not exact else 'EXACT_VALUE_AND_TYPE',
            status='PASS' if ok else 'FAIL'))
    auxiliary=[]
    if case['case_id'].endswith('KW'):
        m=reference['comparison']['metrics'];rule=next(x for x in policy['field_policies'] if x['field']=='formula_corrected_H_for_separate_review')
        delta=abs(m['formula_corrected_H_for_separate_review']-m['TieCorrectedHStatistic'])
        limit=max(rule['proposed_atol'],rule['proposed_rtol']*abs(m['TieCorrectedHStatistic']))
        auxiliary.append(dict(field='R_stats_corrected_H_vs_R_separate_formula',delta=delta,limit=limit,status='PASS' if delta<=limit else 'FAIL'))
    failed=[f['field'] for f in fields+auxiliary if f['status']=='FAIL']
    return dict(status='REFERENCE_EXECUTED_COMPARISON_PENDING_INDEPENDENT_REVIEWS' if not failed else 'REFERENCE_COMPARISON_FAILED',
        fields=fields,auxiliary=auxiliary,failed=failed,missing=[],extra=[],structure='EXACT_CANDIDATE_SCHEMA_TABLE_ORDER_CLR_TYPES_AND_P_METADATA_PASS',
        rendered_excel_format='NOT_CAPTURED_NOT_COMPARED',P='NO',reviewer_1=None,reviewer_2=None)


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--candidate',type=Path,required=True)
    p.add_argument('--reference-job',type=Path,required=True);p.add_argument('--output',type=Path,required=True)
    a=p.parse_args();job=a.reference_job
    publication=r.v2.strict_json((job/'publication-v3.json').read_bytes())
    r.require(publication.get('status')=='REFERENCE_EXECUTED_NOT_COMPARED_NOT_GOLDEN','Operator publication required')
    for name,expected in publication['hashes'].items():r.require(r.digest(job/name)==expected,'Reference artifact drift: '+name)
    r.require((job/'raw-stderr.bin').read_bytes()==b'','Reference stderr')
    ref=r.v2.strict_json((job/'raw-stdout.bin').read_bytes())
    old=r.v2.strict_json((ROOT/'docs/architecture/04A02/oracle-proposals/manifest-v2.json').read_bytes())
    cid=ref['comparison']['case_id'];case,fixture=r.v2.load_case(old,cid)
    for path,expected in r.v3.POLICIES.items():r.require(r.digest(ROOT/path)==expected,'Policy drift')
    policy=next(v for v in r.v2.strict_json((ROOT/next(iter(r.v3.POLICIES))).read_bytes())['cases'] if v['case_id']==cid)
    result=compare(r.v2.strict_json(a.candidate.read_bytes()),ref,policy,case,fixture,ref['provenance']['decision_id'],ref['provenance']['runtime_set_sha256'])
    result['hashes']=dict(candidate=r.digest(a.candidate),raw_reference=r.digest(job/'raw-stdout.bin'),publication=r.digest(job/'publication-v3.json'),
        comparator=r.digest(Path(__file__)),policy=r.digest(ROOT/next(iter(r.v3.POLICIES))))
    r.v2.exclusive_bytes(a.output,r.canonical(result))


if __name__=='__main__':main()
