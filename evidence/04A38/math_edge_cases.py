"""Additional original numerical-helper edge references, before product edits."""
import hashlib
import itertools
import json
from preflight import HERE,ROOT,BASE,git,pin,save


def main():
    assert not git('diff','--name-only') and git('rev-parse','HEAD').decode().strip()==BASE
    folder=HERE/'preedit/attempts/math-edges-v1';folder.mkdir(exist_ok=False)
    recorder=(HERE/'preedit/frozen-v2/Capture38.cs').read_text(encoding='utf-8')
    marker='  if(state.StartsWith("double_matrix:"))'
    assert recorder.count(marker)==1
    decoder='''  if(state.StartsWith("integer_matrix:")){var s=state.Split(':');int nr=Int32.Parse(s[1],Inv),nc=Int32.Parse(s[2],Inv);var a=new int[nr,nc];for(int r=0;r<nr;r++)for(int c=0;c<nc;c++)a[r,c]=s[3]=="max"?Int32.MaxValue:s[3]=="zero"?0:s[3]=="negative"?-1:r+c+1;return a;}\n'''
    recorder=recorder.replace(marker,decoder+marker)
    (folder/'Capture38.cs').write_text(recorder,encoding='utf-8',newline='\n')
    cases=[]
    def add(owner,target,arguments,**parameters):
        for culture in ['','sk-SK']:
            c=dict(target_kind='helper',type='BESHStatNG.'+owner,target=target,culture=culture,cohort='numeric_helper_edges',arguments=arguments,parameters=parameters,input_factory_version='math-edges-v1',
                expected_stage='DIRECT_ORIGINAL_NUMERICAL_HELPER; exact return/exception and IEEE bits, never a new estimator')
            c['case_id']=target+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24];cases.append(c)
    for values,total in itertools.product(['null','doubles:','doubles:1,2,3','doubles:-0,-0','doubles:NaN,+Inf,-Inf'],['number:-Inf','number:-1','number:-0','number:0','number:1','number:10','number:NaN','number:+Inf']):
        add('WorksheetFunctions.MultivariateAnalysisUDFs','PercentOfTotal',{'values':values,'total':total},values=values,total=total)
    for method,n,p,profile in itertools.product(['Is2x2','TableTotal','EffectiveDf','TransposeIntMatrix'],[0,1,2,3],[0,1,2,3],['ordinary','zero','negative','max']):
        add('WorksheetFunctions.ContingencyTableUDFs',method,{'table':f'integer_matrix:{n}:{p}:{profile}'},rows=n,columns=p,profile=profile)
    for method in ['Is2x2','TableTotal','EffectiveDf','TransposeIntMatrix']:
        add('WorksheetFunctions.ContingencyTableUDFs',method,{'table':'null'},profile='missing')
    for state in ['number:-Inf','number:-1000','number:-0','number:0','number:1','number:700','number:1000','number:NaN','number:+Inf']:
        add('UdfRegressionOutput','ExpForDisplay',{'exponent':state},exponent=state)
    assert len(cases)==len({c['case_id'] for c in cases})
    with (folder/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as stream:
        for c in cases:stream.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    save('preedit/attempts/math-edges-v1/specification.json',dict(status='PRE_EDIT_NUMERIC_EDGE_CANDIDATE_NOT_SEALED',source_commit=BASE,rows=len(cases),
        cases=pin((folder/'cases.jsonl').relative_to(ROOT).as_posix()),recorder=pin((folder/'Capture38.cs').relative_to(ROOT).as_posix()),generator=pin('evidence/04A38/math_edge_cases.py'),
        chronology='BEFORE_FIRST_04A38_PRODUCT_EDIT; first valid original-v1 freeze preserved',
        purpose='Cover distinct UDF percent guards, integer overflow/shape behavior and exponential display transformation. No assumption of equivalence to existing graphics-percent helpers.',
        native='NOT_RUN',scientific='NOT_GOLDEN'))
    print('Prepared',len(cases),'numerical helper edge cases; before product edits')


if __name__=='__main__':main()
