"""Independent root-helper whole IL/dependency/source proof; no new oracle."""
import json,hashlib,re
from pathlib import Path
from verify_names import ROOT,HERE,need,tokens,member,body
def inspect(root=ROOT,overrides=None):
 root=Path(root);overrides=overrides or {};base=HERE.relative_to(ROOT).as_posix()+'/'
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 def load(p):return json.loads(raw(p))
 def text(p):return raw(p).decode('utf-8-sig').replace('\r\n','\n')
 matrix=load('evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members'];originals=load('evidence/04A38/preedit/entry-source-snapshot.json')['files']
 def original(p):
  r=next(r for r in originals if r['source']['path']==p)['snapshot'];b=raw(r['path']);need(hashlib.sha256(b).hexdigest()==r['sha256'],'Original source changed');return b.decode('utf-8-sig').replace('\r\n','\n')
 # Exact leaf bodies plus an explicit one-edge relocation in NormalizeKey.
 candidates=[('Regression/UdfRegressionOutput.vb','CategoryHeaders','BESHStatNG.ExcelCommon.UdfRegressionOutputReports38','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb'),('UdfOutputTables.vb','SafeCiLabel','BESHStatNG.ExcelCommon.UdfOutputTablesReports38','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb'),('UdfOutputTables.vb','SafeCiText','BESHStatNG.ExcelCommon.UdfOutputTablesReports38','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb'),('UdfScalarUtilities.vb','NormalizeMatchToken','BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38','BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb'),('UdfScalarUtilities.vb','NormalizeKey','BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38','BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb'),('UdfScalarUtilities.vb','CloneStringArray','BESHStatNG.ExcelCommon.UdfScalarUtilitiesMetadata38','BESHStat.Excel.Common/src/Analysis/UdfMetadataPreparation38.vb')]
 data={c:[load(base+'metadata-v5/names-v3/'+r+'-'+c+'.json') for r in ['original','current']] for c in ['Debug','Release']};proof=[]
 for suffix,n,target,path in candidates:
  p='BESHstatNG/src/ExcelUDFs/'+suffix;r=next(r for r in matrix if r['path']==p and r['name']==n);owner=r['canonical_original_id'].split('|')[1]
  portable=text(path)
  if path.endswith('UdfTypedReportPreparation38.vb'):portable=portable[portable.index('Class '+target.rsplit('.',1)[1]):]
  need(tokens(body(member(original(p),n)))==tokens(body(member(portable,n))),'Portable source operations differ '+n)
  before=body(member(text(p),n));need('Return Global.'+target+'.'+n+'(' in before and len([l for l in before.splitlines() if l.strip()])==1,'Not a pure actual native forwarding wrapper '+n)
  configs=[]
  for c,(o,now) in data.items():
   a=next(m for m in o['methods'] if m['declaring_type']==owner and m['member_name']==n);b=next(m for m in now['methods'] if m['declaring_type']==target and m['member_name']==n);native=next(m for m in now['methods'] if m['declaring_type']==owner and m['member_name']==n)
   need(any(isinstance(i['operand'],str) and 'METHOD:'+b['method']==i['operand'] for i in native['instructions']),'Native compiled target not consumed '+n)
   def projection(m):
    value={k:v for k,v in m.items() if k in ['instructions','locals','init_locals','exceptions','parameter_types']}
    if n=='NormalizeKey':
     value=json.loads(json.dumps(value));
     for i in value['instructions']:
      if isinstance(i['operand'],str):i['operand']=i['operand'].replace('BESHStatNG.UdfScalarUtilities @ BESHStatNG,','BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38 @ BESHStat.Excel.Common,')
    return value
   need(projection(a)==projection(b),'Whole original→portable CIL/local/EH/parameter behavior differs '+n)
   need(not b['type_initializer'] and b['nonliteral_static_fields']==0,'Portable hidden state '+n)
   configs.append(dict(configuration=c,original_method=a['method'],portable_method=b['method'],entire_resolved_instruction_local_EH_exact=True,exception_normalization='NONE',allowed_relocation='NormalizeKey calls reviewed NormalizeMatchToken target only' if n=='NormalizeKey' else 'NONE'))
  proof.append(dict(original_id=r['canonical_original_id'],path=p,name=n,target=target+'.'+n,source_operations_exact=True,configurations=configs))
 # Underlying CI getter/cache, enum values, constructors and field context are reused unchanged.
 for c,(o,n) in data.items():
  for owner in ['BESHStatNG.ConfidenceIntervalResult','BESHStatNG.CIformat']:
   ot=next(t for t in o['types'] if t['type']==owner);nt=next(t for t in n['types'] if t['type']==owner);need(ot==nt,'CI fields/base/enum contract changed')
   oms=[m for m in o['methods'] if m['declaring_type']==owner];nms={m['method']:m for m in n['methods'] if m['declaring_type']==owner}
   for m in oms:need(m['method'] in nms and {k:v for k,v in m.items() if k!='raw_IL_sha256'}=={k:v for k,v in nms[m['method']].items() if k!='raw_IL_sha256'},'CI getter/constructor/mutation/formatter changed')
 # Composition proofs for host text acquisition followed by exact portable normalization.
 scalar='BESHstatNG/src/ExcelUDFs/UdfScalarUtilities.vb';opt='BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb'
 for n,arg in [('NormalizeText','v'),('NormalizeToken','arg')]:
  a=body(member(original(scalar),n)).replace('AsString('+arg+')',arg)
  need(tokens(a)==tokens(body(member(text(opt),n))),'Typed normalizer source changed '+n)
  need(tokens(body(member(text(scalar),n)))==tokens('Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.'+n+'(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+arg+'))'),'Native conversion/exception ordering changed '+n)
  r=next(r for r in matrix if r['path']==scalar and r['name']==n)
  for c,(o,now) in data.items():
   native=next(m for m in now['methods'] if m['declaring_type']=='BESHStatNG.UdfScalarUtilities' and m['member_name']==n);calls=[i['operand'] for i in native['instructions'] if i['op']=='call'];need(len(calls)==2 and '.AsString(' in calls[0] and '.'+n+'(' in calls[1],'Actual acquisition-before-normalization compiled order changed')
  proof.append(dict(original_id=r['canonical_original_id'],path=scalar,name=n,target='UdfScalarUtilitiesOptions38.'+n,source_operations_exact=True,reason='Same native AsString then exact NormalizeMatchToken pure chain, preserves conversion exceptions and null/text sequencing; source and current compiled call order checked'))
 # Symmetry has no preconversion reset; all branch comparisons use the independently domain-proven text policy.
 p='BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb';expected=body(member(original(p),'TryParseSymmetryMethod'));expected=re.sub(r'\bs = ("[^"]*")',r'LegacyUdfText38.EqualText(s, \1)',expected).replace('Select Case s','Select Case LegacyUdfText38.CaseToken(s, New String() {"CM", "CABILIO-MASARO", "CABILIO", "MASARO"})')
 need(tokens(expected)==tokens(body(member(text(opt),'TryParseSymmetryMethod'))),'Symmetry decision/write/alias source changed')
 need(tokens(body(member(text(p),'TryParseSymmetryMethod')))==tokens('Return Global.BESHStatNG.ExcelCommon.AssumptionsUDFsOptions38.TryParseSymmetryMethod(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), methodInternal, title)'),'Symmetry native exception sequence changed')
 r=next(r for r in matrix if r['path']==p and r['name']=='TryParseSymmetryMethod');proof.append(dict(original_id=r['canonical_original_id'],path=p,name=r['name'],target='AssumptionsUDFsOptions38.TryParseSymmetryMethod',reason='Whole decision/write/alias body; host conversion before any ByRef write; NormalizeText/MatchToken dependencies proved here and AsString in earlier full host proof. Final current Framework alias/throw/mutation tests and fixed-RHS text-domain proof required separately.'))
 return dict(status='ROOT_HELPER_AND_SYMMETRY_COMPOSITION_PROOFS_PASS_NOT_READY',proofs=proof,still_OPEN='TryInvertMatrix: original VB ProjectData versus Core embedded shim exception/Err-state context not discharged',original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
