"""Derive independent reader structure; fixed observations come from sealed original."""
from common import *
s=(V6/'verify_supplement.py').read_text(encoding='utf-8')
s=s.replace('BASE,KIND,','BASE,KINDS,').replace('262229291122a20199982d8278f96e5d37a0531f4fdecf204c33c9db7e0748c0',sha((HERE/'original-output-seal.json').read_bytes()))
start=s.index("PREVIOUS=");end=s.index('\ndef time',start)
constants="PREVIOUS='"+PREVIOUS+"'\n"
errors={
 'GEE_SPEC_INTERCEPT_ROLE':'the intercept column should not be used as a profile column; it is supplied automatically by the fitted design.',
 'MMRM_WIDE_MULTIROW_REJECTION':'wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row.'}
traces=json.loads((HERE/'original-results-v2/Debug/trace/results.tsv.branches.json').read_bytes())
constants+='ERRORS='+repr(errors)+'\nEVENTS='+repr({r['case_id'].split('/')[-1]:r['operations'][1]['events'] for r in traces[:2]})+'\n'
s=s[:start]+constants+s[end:]
s=s.replace("here/'original-results'","here/'original-results-v2'")
s=s.replace("runtime['cases']==2","runtime['cases']==4").replace('all(len(s)==2','all(len(s)==4').replace('len(restored)==2','len(restored)==4')
s=s.replace("  detail=[]","  detail=[]")
s=s.replace("   need(row['input_sha256']", "   kind=case['case_kind'];family='GEE' if kind==KINDS[0] else 'MMRM';error=ERRORS[kind];message='BESH.REGR.'+family+'_LSMESTIMATE error: '+error\n   need(row['input_sha256']")
s=s.replace("['events']==EVENTS,","['events']==EVENTS[kind],")
s=s.replace("fit['target']=='BESH.REGR.MMRM_FIT'","fit['target']=='BESH.REGR.'+family+'_FIT'").replace("fit['result']=='HANDLE:MMRM:SESSION1'","fit['result']=='HANDLE:'+family+':SESSION1'")
s=s.replace("query['target']=='BESH.REGR.MMRM_LSMEANS'","query['target']=='BESH.REGR.'+family+'_LSMESTIMATE'").replace('MESSAGE.encode()','message.encode()')
s=s.replace("session['symbol']=='HANDLE:MMRM:SESSION1'","session['symbol']=='HANDLE:'+family+':SESSION1'").replace("re.fullmatch('MMRM:[0-9a-f]{32}'","re.fullmatch(family+':[0-9a-f]{32}'")
start=s.index("    need(session['intercept_indices']");end=s.index('\n   comparable=',start)
s=s[:start]+'''    need(session['family']==family and session['cached_result_nonnull'] is True,'Failed/foreign original fitted model')
    need(session['fitted_design'].startswith('System.Double[0:36,0:3]{' if family=='GEE' else 'System.Double[0:54,0:6]{'),'Wrong fitted design shape')
    if family=='MMRM':need(session['fitted_time']=='System.Double[0:54]{'+'|'.join(['D'+['3FF0000000000000','4000000000000000','4008000000000000'][j%3] for j in range(54)])+'}','Actual saved visits differ')
    else:need(session['fitted_time']=='N','GEE factory time changed')'''+s[end:]
start=s.index('   detail.append(');end=s.index('\n if (here/',start)
s=s[:start]+'''   detail.append(dict(case_id=case['case_id'],culture=case['culture'],kind=kind,exact=True,true_guard_original_lines=([1506,1507,1508,1509] if family=='GEE' else [592,593,594]),outcome='RETURN_TEXT',returned_report=False,input_sha256=row['input_sha256'],original_query_sha256=digest(qb),current_query_sha256=digest(qb),decoded_result=message,events=EVENTS[kind],private_ByRef=(dict(components_nonnull=True,components_count=0,errorMessage=error) if family=='GEE' else dict(atProfile=None,errorMessage=error))))
  results[config]=dict(unique_cases=4,exact=4,G1_true_guard_cases=2,G2_true_guard_cases=2,returned_reports=0,total_original_executions=24,accepted_original_executions=12,current_executions=4,result_bytes=len(raw),original_sha256=digest(raw),current_sha256=digest((current/'results.tsv').read_bytes()),cases=detail)
 report=dict(status='APPROVAL_G_EVIDENCE_PASS_NOT_BATCH_READY',configurations=results,reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',distinct_components_per_configuration=dict(genuine_preedit=28966,A=432,B=12,C=2,D=4,E=2,F=2,G=4),native='NOT_RUN',scientific='NOT_GOLDEN')'''+s[end:]
with (HERE/'verify_supplement.py').open('x',encoding='utf-8') as f:f.write(s)
print('Independent reader prepared; no producer import')
