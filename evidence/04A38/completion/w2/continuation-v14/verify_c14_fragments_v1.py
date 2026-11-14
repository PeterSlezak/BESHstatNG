"""Recompute every actual fragment match from original/current source records."""
import json,re
from pathlib import Path
from verify_c14_composition_v2 import tokens,unwrap_cases

def inspect(load,prefix):
 contracts=load(prefix+'whole-body-contracts-v1.json')['contracts']+load(prefix+'property-body-contracts-v1.json')['contracts'];u=load(prefix+'current-universe-packet-v1.json');original=load('evidence/04A38/completion/w1/original-universe.json');pc=load(prefix+'portable-api-contracts-v1.json')['contracts'];new={c['original_record']['symbol_id']:c for c in contracts if c['new_support']};moves=load(prefix+'implementation-source-composition-v1.json')['phases'];full=load(prefix+'dependency-full-common-v1.json');legacy=next(m for m in load(prefix.replace('v14/','v13/')+'implementation-source-composition-v2.json')['phases'] if 'Function TryGetRocInput(' in m['parent'])
 def canon(name,text):
  text=unwrap_cases(text).replace('ChrW(10)','Convert.ToChar(10)').replace('ChrW(13)','Convert.ToChar(13)')
  if name=='GetFormulaAddressingMode':text=text.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)','arg')
  if name=='ResolveNamedTable':
   m=next(m for m in moves if m['parent']==name);text=text.replace('Dim name As Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38 = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.ResolveTableKindPrepared(requested)','Dim name As String = NormalizeOptionKey(requested)')
   for b in m['case_bindings']:text=text.replace('Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.'+b['portable_case']+'\n','Case '+b['original_case']+'\n')
  if name=='TryGetRocInput':
   for a,b in legacy['bindings'].items():text=text.replace(a,b)
  if name=='GuessPositiveClass':
   text=text.replace('statusCol.GetLength(0)','rowCount()').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(i, 0))','text(i)')
   for pattern in [r'Dim iv As Integer\s+If Not Global\.BESHStatNG\.WorksheetFunctions\.ExcelArgNumeric\.TryGetStatus01Flexible\(statusCol\(i, 0\), iv\) Then\s+allBinary01 = False\s+End If',r'If Not Global\.BESHStatNG\.WorksheetFunctions\.ExcelArgNumeric\.TryGetStatus01Flexible\(statusCol\(i, 0\), iv\) Then\s+allBinary01 = False\s+End If']:text=re.sub(pattern,'If Not binary(i) Then allBinary01 = False',text)
   text=text.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusCol(i, 0), iv)','binary(i)');text=re.sub(r'\bs\b','token',text);text=re.sub(r'\bnorm\b','normalized',text);text=text.replace('Dim norms =','Dim norms As String() =').replace('Function(x) NormalizeOptionKey(x)','Function(token) UdfDataImportOptions38.NormalizeOptionKey(token)').replace('NormalizeOptionKey(token)','UdfDataImportOptions38.NormalizeOptionKey(token)').replace('UdfDataImportOptions38.UdfDataImportOptions38.','UdfDataImportOptions38.')
  return tokens(text)
 rows=load(prefix+'new-parent-fragment-crosswalk-v2.json')['rows'];expected=[f for f in original['fragments'] if f['member_id'] in new];assert len(rows)==len(expected)==1080 and {r['fragment_id'] for r in rows}=={f['fragment_id'] for f in expected}
 by={r['fragment_id']:r for r in rows}
 for f in expected:
  r=by[f['fragment_id']];c=new[f['member_id']];name=c['current_record']['name'];assert r['original_source_record']==f and r['original_parent']==c['original_id'] and r['current_parent']==c['current_id'] and r['bound_code']==canon(name,f['code'])
  pool=[v for v in u['fragments'] if v['member_id']==c['current_id']]
  if c['current_record']['kind']!='PropertyStatement':
   reached=set(c['compiled'][0]['transitive']['methods'])
   for p in pc:
    if p['compiled'][0]['method']['method'] in reached:pool.extend(p['fragments'])
  if name=='GetFormulaAddressingMode':
   p=next(n for n in full['members'] if n['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and n['name']==name);pool.extend(v for v in full['fragments'] if v['member_id']==p['symbol_id'])
  matches=[]
  for v in pool:
   cross=name=='GuessPositiveClass' and f['kind']=='MultiLineIfBlock' and v['kind']=='SingleLineIfStatement' and 'TryGetStatus01Flexible' in f['code'] and 'allBinary01 = False' in f['code']
   header=name=='GuessPositiveClass' and f['kind']=='IfStatement' and v['kind']=='SingleLineIfStatement' and canon(name,f['code'])==tokens('If Not binary(i) Then') and canon(name,v['code'])==tokens('If Not binary(i) Then allBinary01 = False')
   if header or (v['kind']==f['kind'] or cross) and canon(name,v['code'])==canon(name,f['code']):matches.append((v['fragment_id'],v['member_id'],v['kind']))
  assert [(v['current_fragment_id'],v['current_member_id'],v['current_kind']) for v in r['matches']]==matches,'Fragment match/composite fabricated'
  assert matches or not (f['is_branch'] or f['is_inline']),'Unmatched actual branch/inline arm'
 assert not load(prefix+'new-parent-fragment-crosswalk-v2.json')['unmatched_branch_inline']
 return dict(original_new_fragments=1080,unmatched_branch_inline=0,dynamic_execution='NOT_INFERRED')
