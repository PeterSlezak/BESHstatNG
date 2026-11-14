"""Pre-production fit/query input matrix and recorder extension.

Only synthetic inputs, original public-call sequencing and stored identity
observation are added. No estimate, preparation policy or expected value is
implemented in the recorder. Original source/dependencies remain entry-v6.
"""
import hashlib
import itertools
import json
from preflight import HERE,ROOT,BASE,git,pin,save


TRANSACTION = r'''
     if(T(c,"workflow")=="fit_then_queries"){
      object fit=result;string fitKind=kind;var records=new List<object>();records.Add(new object[]{"fit",fitKind,fit});
      if(fitKind=="HANDLE_CREATED"){
       records.Add(new object[]{"stored_identity",StoredIdentity((string)fit)});
       foreach(var specValue in (IEnumerable)c["query_exports"]){
        var spec=(Dictionary<string,object>)specValue;string queryName=T(spec,"name");var query=exports[queryName];var qp=query.GetParameters();
        var qa=qp.Select(p=>p.HasDefaultValue&&p.DefaultValue!=DBNull.Value?p.DefaultValue:Default(p)).ToArray();int slot=Array.FindIndex(qp,p=>p.Name=="handle");
        if(slot<0)throw new Exception("QUERY_REQUIRES_HANDLE");qa[slot]=fit;
        foreach(var pair in (Dictionary<string,object>)spec["arguments"]){int qslot=Array.FindIndex(qp,p=>p.Name==pair.Key);if(qslot<0)throw new Exception("Unknown query parameter");qa[qslot]=Decode(Convert.ToString(pair.Value,Inv));}
        ControlCachedFixtureClocks();phase="ORIGINAL_QUERY_AFTER_FIT";
        try{var value=query.Invoke(null,qa);records.Add(new object[]{queryName,Kind(value),value,qa});}
        catch(Exception ex){records.Add(new object[]{queryName,"EXCEPTION",Error(ex),qa});}
       }
       phase="ORIGINAL_FIT_ACCEPTED_AND_QUERY_RECORDS_CAPTURED";kind="TRANSACTION_FIT_ACCEPTED";
      }else{phase="ORIGINAL_FIT_REJECTED_BEFORE_QUERY";kind="TRANSACTION_FIT_REJECTED";}
      result=records.ToArray();
     }
'''

IDENTITY = r'''
 static object StoredIdentity(string handle){
  var output=new List<object>();
  string[] allowed={"SourceRowsUsed","SourceRowsDropped","RowIds","LeafOriginalRowOrder","VarNames","RawVarNames","RawPredictorKeys","ExpandedPredictorNames","FixedEffectNames","FittedTimeValues","VisitValues","FittedDesign"};
  foreach(var type in Library.GetTypes())foreach(var field in type.GetFields(BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)){
   if(!field.Name.EndsWith("Cache",StringComparison.Ordinal)&&!field.Name.EndsWith("cache",StringComparison.Ordinal))continue;
   object cache=field.GetValue(null);if(cache==null)continue;var lookup=cache.GetType().GetMethod("TryGetValue");if(lookup==null)continue;
   var lookupArgs=new object[]{handle,null};if(!(bool)lookup.Invoke(cache,lookupArgs))continue;
   Action<object,string> observe=(value,prefix)=>{if(value==null)return;foreach(string name in allowed){var p=value.GetType().GetProperty(name,BindingFlags.Instance|BindingFlags.Public);if(p!=null&&p.GetIndexParameters().Length==0)output.Add(new object[]{prefix+name,p.GetValue(value,null)});}};
   observe(lookupArgs[1],"handle.");foreach(string name in new[]{"Model","Result"}){var p=lookupArgs[1].GetType().GetProperty(name,BindingFlags.Public|BindingFlags.Instance);if(p!=null)observe(p.GetValue(lookupArgs[1],null),name+".");}
  }
  output.Add(new object[]{"identity_scope","INPUT_ARRAY_ORDINAL_AND_MODEL_REPORTED_IDS_AS_STORED; PHYSICAL_WORKSHEET_ROWS_UNKNOWN"});return output.ToArray();
 }
'''


def main():
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert git('rev-parse','HEAD').decode().strip()==BASE
    folder=HERE/'preedit/attempts/model-inputs-v2';folder.mkdir(exist_ok=False)
    census=json.loads((HERE/'entry-members.json').read_bytes());exports={e['effective_excel_name']:e for e in census['exports']}
    recorder=(HERE/'preedit/frozen-v2/Capture38.cs').read_text(encoding='utf-8')
    old='args=PublicArgs(T(c,"target"),target,false);'
    assert recorder.count(old)==1
    recorder=recorder.replace(old,'args=PublicArgs(T(c,"target"),target,T(c,"workflow")=="fit_then_queries");')
    old='input=Snap(args);ControlCachedFixtureClocks();phase="INVOKE_ORIGINAL_SOURCE_BODY";result=target.Invoke(null,args);kind=Kind(result);'
    assert recorder.count(old)==1
    recorder=recorder.replace(old,old+TRANSACTION).replace(' static int Execute(string[] argv){',IDENTITY+' static int Execute(string[] argv){')
    (folder/'Capture38.cs').write_text(recorder,encoding='utf-8',newline='\n')
    cases=[]
    def queries(fit):
        prefix=fit.rsplit('_FIT',1)[0] if fit.endswith('_FIT') else 'BESH.PS'
        ordered=[]
        for suffix in ['_SUMMARY','_COEF','_FITTED','_SCORES','_CENTERS','_CASEWISE','_LEAFORDER','_MEMBERSHIP','_REMOVED','_RESID']:
            name=prefix+suffix
            if name in exports and all(a['name']=='handle' or a['default_value'] is not None for a in exports[name]['arguments']):ordered.append(dict(name=name,arguments={}))
            if len(ordered)>=4:break
        if fit=='BESH.PS.FIT':ordered=[dict(name='BESH.PS.'+s,arguments={}) for s in ['SUMMARY','SCORES','WEIGHTS','EFFECT']]
        assert ordered,fit
        return ordered
    def add(fit,arguments,descriptor,extra_queries=()):
        for culture in ['','sk-SK']:
            c=dict(target_kind='export',target=fit,culture=culture,cohort='fit_query_input_identity',workflow='fit_then_queries',
                arguments=arguments,query_exports=queries(fit)+list(extra_queries),input_factory_version='model-inputs-v2',
                parameters=descriptor,expected_stage='ORIGINAL_FIT_THEN_QUERIES_IF_HANDLE_CREATED; source fit rejections are negative cases and never query coverage')
            c['case_id']=fit+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24];cases.append(c)
    def vector(n,kind):
        if kind=='none':return 'null'
        length=n-1 if kind=='short' else n
        return 'vector:'+','.join('null' if kind=='gap' and i==3 else '0' if kind=='zero' else str(0.5+0.1*(i%4)) for i in range(length))
    fits=[n for n in exports if n.endswith('_FIT') and n.startswith(('BESH.REGR.','BESH.SURV.COX_'))]
    for fit in fits:
        parameters={a['name'] for a in exports[fit]['arguments']};n=54 if fit in ['BESH.REGR.LMM_FIT','BESH.REGR.MMRM_FIT'] else 36
        weight_states=['none','positive','gap','short'] if 'weights' in parameters else ['not_applicable']
        offset_states=['none','positive','gap','short'] if 'offset' in parameters else ['not_applicable']
        formula_name='formula' if 'formula' in parameters else 'fixedFormula' if 'fixedFormula' in parameters else 'countFormula'
        for weight,offset,formula in itertools.product(weight_states,offset_states,['none','A + B','B','invalid(']):
            args={}
            if weight!='not_applicable':args['weights']=vector(n,weight)
            if offset!='not_applicable':args['offset']=vector(n,offset)
            args[formula_name]='null' if formula=='none' else 'text:'+formula
            add(fit,args,dict(rows=n,weight=weight,offset=offset,formula=formula,formula_parameter=formula_name))
        xname='xCount' if 'xCount' in parameters else 'x';columns=5 if n==54 else 3
        add(fit,{formula_name:'text:B',xname:f'matrix_profile:{n}:{columns}:none:blank'},dict(rows=n,unused_first_column_gap=True,selected_formula='B',scope='Original family-specific missing/formula behavior, not reconciled'))
    add('BESH.SURV.COX_FIT',{'strata':'vector:'+','.join(str(i%3+1) for i in range(36)),'formula':'text:A+B'},dict(strata='three repeated numeric strata',formula='A+B'))
    for reference in ['first','last']:
        for fit in ['BESH.REGR.MNLOGIT_FIT','BESH.REGR.ORDLOGIT_FIT']:
            add(fit,{'reference':'text:'+reference},dict(reference=reference,source_category_order='1/2/3 repeated; original mapping'))
    for labels,probabilities in itertools.product(['text:G3,G1,G2','text:G1,G1,G2','text:G1,G2'],['text:0.2,0.5,0.3','text:0.2,NaN,0.8']):
        add('BESH.MULTI.DA_FIT',{'priorMode':'text:user','priorLabels':labels,'priorProbabilities':probabilities},dict(prior_labels=labels,prior_probabilities=probabilities))
    for centers in ['matrix_profile:2:4:none:clean','matrix_profile:2:3:none:clean','matrix_profile:3:4:none:clean']:
        add('BESH.MULTI.KMEANS_FIT',{'initializationMethod':'text:userspecified','startingCenters':centers,'numberOfClusters':'number:2'},dict(starting_centers=centers,clusters=2))
    add('BESH.MULTI.HCLUST_FIT',{},dict(cut_matrix='count and height'),[
        dict(name='BESH.MULTI.HCLUST_MEMBERSHIP',arguments={'mode':'text:count','value':'number:3'}),
        dict(name='BESH.MULTI.HCLUST_MEMBERSHIP',arguments={'mode':'text:height','value':'number:0.5'}),
    ])
    for formula in ['null','text:A+B','text:B','text:invalid(']:
        add('BESH.PS.FIT',{'formula':formula},dict(formula=formula,score_method='existing deterministic fixture'))
    for case in cases:
        assert set(case['arguments']) <= {a['name'] for a in exports[case['target']]['arguments']}
        for q in case['query_exports']:
            assert set(q['arguments']) <= {a['name'] for a in exports[q['name']]['arguments']}
    assert len(cases)==len({c['case_id'] for c in cases})
    with (folder/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as stream:
        for c in cases:stream.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    save('preedit/attempts/model-inputs-v2/specification.json',dict(status='PRE_EDIT_FIT_QUERY_CANDIDATE_NOT_SEALED',source_commit=BASE,rows=len(cases),
        cases=pin((folder/'cases.jsonl').relative_to(ROOT).as_posix()),recorder=pin((folder/'Capture38.cs').relative_to(ROOT).as_posix()),generator=pin('evidence/04A38/model_input_cases_v2.py'),
        source_authority='Same original entry-v6 independently compiled source/dependencies; no new projection used as oracle.',
        input_dimensions='culture x actual fit family x independently named weight x offset x formula, plus missing unused predictor, strata, category reference, supplied priors/centers/cuts and PSM formula paths',
        observations='Original public fit result; original public query outputs; narrowly whitelisted stored model/handle identity properties. No inferred worksheet rows.',
        chronology='BEFORE_FIRST_04A38_PRODUCT_EDIT; earlier frozen corpus preserved',native='NOT_RUN',scientific='NOT_GOLDEN',
        remaining_gate='Run original/repeated/instrumented Debug/Release; audit valid fits/queries separately from source guards and fixture errors.'))
    print('Prepared',len(cases),'fit/query transactions; before product edits')


if __name__=='__main__':main()
