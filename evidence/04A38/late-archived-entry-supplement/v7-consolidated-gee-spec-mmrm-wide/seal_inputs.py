"""Materialize exactly4 authorized cases/configuration before any fit/query."""
import copy
from datetime import datetime,timezone
from common import *
def array(rows,cols,values):return dict(kind='Array',element_type='System.Object',rank=2,lengths=[rows,cols],lower_bounds=[0,0],upper_bounds=[rows-1,cols-1],values=values)
def string(s):return dict(kind='String',value=s)
def double(bits):return dict(kind='Double',bits=bits)
def main():
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll');folder=PRIVATE/'original-Debug-plain-v2';commands=[]
 for family,main,previous in [('GEE','MaterializeGee38',V2),('MMRM','MaterializeMmrm38',V3)]:
  exe=folder/(main+'.exe');result=command('factory-'+family+'-compile',[compiler('csc.exe'),'/nologo','/target:exe','/main:'+main,'/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/(main+'.cs')),str(ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs')]);commands.append(clean(result));assert result['exit_code']==0
  result=command('factory-'+family+'-materialize',[str(exe),str(folder),str(HERE/('factory-'+family+'.json'))]);commands.append(clean(result));assert result['exit_code']==0
  assert (HERE/('factory-'+family+'.json')).read_bytes()==(previous/'factory-inputs.json').read_bytes()
 factories={f:json.loads((HERE/('factory-'+f+'.json')).read_bytes()) for f in ['GEE','MMRM']}
 plan=dict(source_commit=BASE,source_map=json.loads((HERE/'compile-original-Debug-trace.json').read_bytes())['instrumentation'],kinds={
  'GEE_SPEC_INTERCEPT_ROLE':dict(public_entry='BESH.REGR.GEE_LSMESTIMATE',argument_order=['handle','spec','covarianceType','alpha','at','scale'],original_lines=[1506,1507,1508,1509],prospective=['real fitted names','spec helper entry','label/weight header recognition','successful Intercept lookup','intercept role true arm','error assignment and False propagated'],note='Source rejects during profile-header examination before numeric weight-row parsing; do not claim row-weight parsing occurred'),
  'MMRM_WIDE_MULTIROW_REJECTION':dict(public_entry='BESH.REGR.MMRM_LSMESTIMATE',argument_order=['handle','spec','alpha','at'],original_lines=[652,592,593,594],prospective=['successful spec','at helper entry','headerless nonnull preconditions','column1 differsfrom2 and probe declines','wide visit header','first data row accepted','second data row true guard','ByRef error/profile propagation'])},expected_public_result='UNKNOWN_NOT_CAPTURED',expected_private_state='UNKNOWN_NOT_CAPTURED',handle_normalization='Only real fitted GUID -> HANDLE:<GEE|MMRM>:SESSION1; raw GUID sidecars retained; no other normalization')
 save('trace-plan.json',plan)
 for config in ['Debug','Release']:
  rows=[];B=list(map(json.loads,(V2/('inputs-'+config+'.jsonl')).read_bytes().splitlines()));C=list(map(json.loads,(V3/('inputs-'+config+'.jsonl')).read_bytes().splitlines()))
  for culture in ['','sk-SK']:
   for family,kind,source in [('GEE','GEE_SPEC_INTERCEPT_ROLE',B),('MMRM','MMRM_WIDE_MULTIROW_REJECTION',C)]:
    c=copy.deepcopy(next(r for r in source if r['culture']==culture));c['case_id']='AUTHORIZED-G/'+(culture or 'Invariant')+'/'+kind;c['case_kind']=kind;c['configuration']=config;c['ui_culture']=culture;c['reference_class']=CLASS;c['expected_result']='UNKNOWN_NOT_CAPTURED'
    assert c['operations'][0]['arguments']==factories[family]['fit_arguments']
    args=c['operations'][1]['arguments']
    if family=='GEE':
     args[1]['value']=array(2,3,[string('Label'),string('Weight'),string('Intercept'),string('L'),double('3FF0000000000000'),double('3FF0000000000000')]);args[4]['value']=dict(kind='Nothing')
     defaults={d['name']:d['value'] for d in factories[family]['query_defaults']}
     for index,name in [(2,'covarianceType'),(3,'alpha'),(5,'scale')]:assert args[index]['value']==defaults[name]
    else:
     assert args[1]['value']==factories[family]['spec'] and args[2]['value']==dict(kind='Nothing')
     args[3]['value']=array(3,1,[string('visit'),double('3FF0000000000000'),double('4000000000000000')])
    c['session']['lifetime']='One genuinely successful '+family+'_FIT then one public query; fresh cache per case'
    c['session']['handle_normalization']=plan['handle_normalization'];c['branch_expectation']=plan['kinds'][kind];c.pop('expected_source_branch',None)
    rows.append(c)
  assert len(rows)==len({r['case_id'] for r in rows})==4
  with (HERE/('inputs-'+config+'.jsonl')).open('x',encoding='utf-8',newline='\n') as f:
   for row in rows:f.write(json.dumps(row,separators=(',',':'),ensure_ascii=False)+'\n')
 recorders=[]
 for config in ['Debug','Release']:
  for mode in ['plain','trace']:
   folder=PRIVATE/('original-'+config+'-'+mode+'-v2');exe=folder/'MmrmCapture38.exe'
   result=command('recorder-'+config+'-'+mode,[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MmrmCapture38.cs')]);commands.append(clean(result));assert result['exit_code']==0
   recorders.append(dict(configuration=config,mode=mode,executable=library_pin(exe)))
  result=command('typed-materialization-'+config,[str(PRIVATE/('original-'+config+'-plain-v2')/'MmrmCapture38.exe'),'--materialize',str(HERE/('inputs-'+config+'.jsonl')),str(HERE/('materialized-'+config+'.json'))]);commands.append(clean(result));assert result['exit_code']==0
 save('materialization-receipt.json',dict(commands=commands,recorders=recorders,factory_identity=[dict(family=f,fresh=pin(HERE/('factory-'+f+'.json')),predecessor=pin((V2 if f=='GEE' else V3)/'factory-inputs.json'),bytes_exact=True) for f in ['GEE','MMRM']],model_executions=0,query_executions=0,python_version=sys.version))
 approval=json.loads((HERE/'owner-authorization.json').read_bytes())
 with (HERE/'owner-authorization.md').open('x',encoding='utf-8') as f:f.write('# Approval G — 2026-11-09\n\n'+approval['owner_quote']+'\n\nExactly4 cases/configuration; not scientific/native/owner acceptance. A–F remain separate and exhausted.\n')
 save('input-seal.json',dict(status='INPUT_SEALED',sequence=1,sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,reference_class=CLASS,cases_per_configuration=4,files=[pin(p) for p in sorted(HERE.iterdir()) if p.is_file()],original_executions=0,current_executions=0,not_preedit_frozen=True))
 print('INPUT_SEALED:4 approved cases/configuration; complete .NET rectangular inputs; original/current fits/queries0')
if __name__=='__main__':main()
