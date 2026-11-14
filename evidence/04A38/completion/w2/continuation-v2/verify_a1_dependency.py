"""Complete A1 parser/normalizer operation proof. Source inspection only for originals."""
import json,re
from pathlib import Path
from verify_slice_v2 import ROOT,HERE,member,body,tokens,need,digest
def inspect(root=ROOT):
 root=Path(root)
 def text(p):return (root/p).read_text(encoding='utf-8-sig').replace('\r\n','\n')
 original=text('evidence/04A38/preedit/source/AssumptionsUDFs.vb.txt');scalar=text('evidence/04A38/preedit/source/UdfScalarUtilities.vb.txt');current=text('BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb')
 for n in ['TryParseLeveneCenter','TryParseSymmetryMethod']:
  expected=body(member(original,n));expected=re.sub(r'\bs = ("[^"]*")',r'LegacyUdfText38.EqualText(s, \1)',expected)
  aliases=['MEDIAN','BROWN-FORSYTHE','BROWNFORSYTHE','BF'] if n=='TryParseLeveneCenter' else ['CM','CABILIO-MASARO','CABILIO','MASARO']
  expected=expected.replace('Select Case s','Select Case LegacyUdfText38.CaseToken(s, New String() {'+', '.join('"'+v+'"' for v in aliases)+'})')
  need(tokens(expected)==tokens(body(member(current,n))),'Original parser branch/alias/output mutation changed '+n)
 need(tokens(body(member(scalar,'NormalizeMatchToken')))==tokens(body(member(current,'NormalizeMatchToken'))),'Normalizer trim/case/separator/null policy changed')
 need(tokens(body(member(scalar,'NormalizeText')).replace('AsString(v)','v'))==tokens(body(member(current,'NormalizeText'))),'Host acquisition to typed normalization split changed')
 proof=json.loads((root/HERE.relative_to(ROOT)/'host-cache-proof.json').read_bytes());need(any(r['name']=='AsString' and r['source_body_exact'] for r in proof['methods']),'Original/current host acquisition full proof missing')
 scenarios=[]
 for c in ['Debug','Release']:
  focused=json.loads((root/HERE.relative_to(ROOT)/('focused-b1-v1-'+c+'.json')).read_bytes());r=next(r for r in focused['results'] if r['kind']=='TEST' and r['suite']=='Legacy')
  tests=[r for r in r['tests']['records'] if r['method'].startswith(('ShippingLevene','ShippingSymmetry'))]
  need(len(tests)==14 and all(r['outcome']=='Passed' for r in tests),'Actual production parser exception/mutation tests missing')
  full=json.loads((root/HERE.relative_to(ROOT)/('metadata/b1-v1/current-'+c+'.json')).read_bytes())
  host=next(r for r in full['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.AssumptionsUDFs' and r['member_name']=='TryParseLeveneCenter')
  il=host['instructions'];call=next(i for i,r in enumerate(il) if isinstance(r['operand'],str) and '.TryParseLeveneCenterFromReader(' in r['operand']);need(any(r['op']=='stind.i1' for r in il[:call]) and any(r['op']=='stind.ref' for r in il[:call]),'Actual native ByRef reset missing')
  callback=[r for r in full['methods'] if r['declaring_type'].startswith('BESHStatNG.WorksheetFunctions.AssumptionsUDFs+') and any(isinstance(i['operand'],str) and '.AsString(' in i['operand'] for i in r['instructions'])]
  need(len(callback)==1 and sum(isinstance(i['operand'],str) and '.AsString(' in i['operand'] for i in callback[0]['instructions'])==1,'Wrong generated host callback')
  portable=next(r for r in full['methods'] if r['declaring_type']=='BESHStatNG.ExcelCommon.AssumptionsUDFsOptions38' and r['member_name']=='TryParseLeveneCenterFromReader');il2=portable['instructions'];invoke=next(i for i,r in enumerate(il2) if isinstance(r['operand'],str) and '.Invoke(' in r['operand']);need(any(r['op']=='stind.i1' for r in il2[:invoke]) and any(r['op']=='stind.ref' for r in il2[:invoke]),'Portable reset must precede exactly one reader evaluation')
  scenarios.append(dict(configuration=c,production_route_tests=14,host_argument_reader_callbacks=1,actual_reset_before_callback=True,actual_local_parse_then_copyback=True))
 return dict(status='A1_ORIGINAL_PARSER_NORMALIZER_OPERATIONS_AND_EXCEPTION_BOUNDARY_PASS',scenarios=scenarios,original_queries=0,Levene='Reset before reader allocation/invocation; one AsString evaluation; invalid/throw retain conversion mutations; accepted branch overwrites outputs in original order',Symmetry='No pre-conversion output initialization in original or current; conversion throw and invalid token preserve original/callback state; long normalized aliases retain their original rejection',text_domain='Fixed-literal Option Compare Text proof and A2 current-runtime observations; not arbitrary custom CultureInfo equivalence',whole_parent_closure='Levene counted by bounded slice; Symmetry exception boundary reviewed here but its ledger remains OPEN until formally incorporated into all-dependency ownership')
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
