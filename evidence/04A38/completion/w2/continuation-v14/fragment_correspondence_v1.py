"""Real original fragments to actual native/portable fragments with explicit composites."""
from common import *
from verify_c14_composition_v2 import tokens,unwrap_cases
from source_views import body
import re
draft='--draft' in sys.argv;out=PRIVATE/'draft-proof-v1' if draft else HERE
def load(name):return json.loads(((out/name) if (out/name).exists() else HERE/name).read_bytes())
contracts=load('whole-body-contracts-v1.json')['contracts']+load('property-body-contracts-v1.json')['contracts'];u=load('current-universe-packet-v1.json');original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());pc=load('portable-api-contracts-v1.json')['contracts'];new={c['original_record']['symbol_id']:c for c in contracts if c['new_support']};moves=load('implementation-source-composition-v1.json')['phases'];full=load('dependency-full-common-v1.json')
def canonical(name,text):
 text=unwrap_cases(text).replace('ChrW(10)','Convert.ToChar(10)').replace('ChrW(13)','Convert.ToChar(13)')
 if name=='GetFormulaAddressingMode':text=text.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)','arg')
 if name=='ResolveNamedTable':
  move=next(m for m in moves if m['parent']==name)
  text=text.replace('Dim name As Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38 = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.ResolveTableKindPrepared(requested)','Dim name As String = NormalizeOptionKey(requested)')
  for b in move['case_bindings']:text=text.replace('Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.'+b['portable_case']+'\n','Case '+b['original_case']+'\n')
 if name=='GuessPositiveClass':
  text=text.replace('statusCol.GetLength(0)','rowCount()').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(i, 0))','text(i)')
  text=re.sub(r'Dim iv As Integer\s+If Not Global\.BESHStatNG\.WorksheetFunctions\.ExcelArgNumeric\.TryGetStatus01Flexible\(statusCol\(i, 0\), iv\) Then\s+allBinary01 = False\s+End If','If Not binary(i) Then allBinary01 = False',text)
  text=re.sub(r'If Not Global\.BESHStatNG\.WorksheetFunctions\.ExcelArgNumeric\.TryGetStatus01Flexible\(statusCol\(i, 0\), iv\) Then\s+allBinary01 = False\s+End If','If Not binary(i) Then allBinary01 = False',text)
  text=re.sub(r'\bs\b','token',text);text=re.sub(r'\bnorm\b','normalized',text);text=text.replace('Dim norms =','Dim norms As String() =').replace('Function(x) NormalizeOptionKey(x)','Function(token) UdfDataImportOptions38.NormalizeOptionKey(token)').replace('NormalizeOptionKey(token)','UdfDataImportOptions38.NormalizeOptionKey(token)').replace('UdfDataImportOptions38.UdfDataImportOptions38.','UdfDataImportOptions38.')
 return tokens(text)
rows=[]
for f in original['fragments']:
 if f['member_id'] not in new:continue
 c=new[f['member_id']];name=c['current_record']['name'];pool=[v for v in u['fragments'] if v['member_id']==c['current_id']]
 if c['current_record']['kind']!='PropertyStatement':
  reached=set(c['compiled'][0]['transitive']['methods'])
  for p in pc:
   if p['compiled'][0]['method']['method'] in reached:pool.extend(p['fragments'])
 if name=='GetFormulaAddressingMode':
  p=next(n for n in full['members'] if n['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and n['name']==name);pool.extend(v for v in full['fragments'] if v['member_id']==p['symbol_id'])
 matches=[]
 for v in pool:
  cross_kind=name=='GuessPositiveClass' and f['kind']=='MultiLineIfBlock' and v['kind']=='SingleLineIfStatement' and 'TryGetStatus01Flexible' in f['code'] and 'allBinary01 = False' in f['code']
  if (v['kind']==f['kind'] or cross_kind) and canonical(name,v['code'])==canonical(name,f['code']):matches.append(dict(current_fragment_id=v['fragment_id'],current_member_id=v['member_id'],current_kind=v['kind'],composite='PSM_TYPED_DISPATCH_LITERAL_HEADER_AND_UNCHANGED_NATIVE_BRANCH_BODY' if name=='ResolveNamedTable' and 'PsmUdfTableKind38' in v['code'] else 'EXPLICIT_NATIVE_STATUS_PREDICATE_AND_EQUIVALENT_SINGLE_LINE_FALSE_FLAG' if cross_kind else 'EXACT_FRAGMENT_UNDER_COMPLETE_PARENT_ARGUMENT_AND_LEGACY_POLICY_BINDINGS'))
 rows.append(dict(fragment_id=f['fragment_id'],original_source_record=f,original_parent=c['original_id'],current_parent=c['current_id'],matches=matches,bound_code=canonical(name,f['code']),parent_composition='source-composition-proof-v1.json',dynamic_execution='NOT_INFERRED'))
unmatched=[r for r in rows if not r['matches'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
value=dict(rows=rows,original_fragments=len(rows),exact_or_composite_matches=sum(bool(r['matches']) for r in rows),whole_parent_only=sum(not r['matches'] for r in rows),unmatched_branch_inline=unmatched,original_queries=0)
with (out/'new-parent-fragment-crosswalk-v1.json').open('x',encoding='utf-8') as stream:json.dump(value,stream,indent=2,ensure_ascii=False)
for r in unmatched:print(new[r['original_source_record']['member_id']]['current_record']['name'],r['original_source_record']['kind'],r['original_source_record']['code'][:180].replace('\n',' '))
print('Original fragments',len(rows),'unmatched branch/inline',len(unmatched))
assert not unmatched,'Preserve real unmatched arms; do not infer a match from parent presence'
