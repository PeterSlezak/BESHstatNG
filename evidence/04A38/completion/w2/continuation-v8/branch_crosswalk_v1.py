"""Individual original statement/branch/inline locations to actual source phases."""
from common import *
from verify_c8_composition_v2 import tokens
import re
paths=['BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb','BESHStat.Core/src/StatTests/SampleSize.vb','BESHStat.Core/src/DM/NumericGuards.vb','BESHStat.Core/src/DM/GlmData.vb','BESHStat.Core/src/DM/GeeData.vb','BESHStat.Core/src/DM/CoxPHData.vb']
exe=PRIVATE/'portable-census-v1/bin/Census.dll'
r=command('dependency-source-census-v1',['dotnet',str(exe),str(ROOT),str(HERE/'dependency-source-universe-v1.json'),*paths]);assert r['exit_code']==0
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());p=json.loads((HERE/'portable-universe-v1.json').read_bytes());d=json.loads((HERE/'dependency-source-universe-v1.json').read_bytes())
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];new=json.loads((HERE/'new-closures-v1.json').read_bytes())['rows'];planning={r['native']:r for r in json.loads((HERE/'sample-size-extraction-v1.json').read_bytes())['rows']};layouts={r['native']:r for r in json.loads((HERE/'import-layout-extraction-v1.json').read_bytes())['rows']}
def canonical(code,name):
    text=code.replace('\r\n','\n')
    if name in planning:
        for reader in planning[name]['readers']:text=re.sub(r'\b'+reader['prepared']+r'\(',reader['original']+'(',text)
        text=text.replace('Dim hypothesis As UdfSampleSizeHypothesis38','Dim hypothesis As String').replace('hypothesis = UdfSampleSizeHypothesis38.Invalid','hypothesis Is Nothing')
        for old,label in [('Superiority','HypothesisSuperiority'),('NonInferiority','HypothesisNonInferiority'),('Equivalence','HypothesisEquivalence')]:text=text.replace('UdfSampleSizeHypothesis38.'+old,label)
        text=text.replace('Return Failed(errorKind, UdfSampleSizeError38.Value)','Return ExcelError.ExcelErrorValue').replace('Return Failed(errorKind, UdfSampleSizeError38.Numeric)','Return ExcelError.ExcelErrorNum').replace('Return SampleSizeUDFsReports38.Make','Return Make')
    if name=='ResolveSampleSizeHypothesisType':
        text=re.sub(r'LegacyUdfText38\.CaseToken\(s\.Trim\(\)\.ToLowerInvariant\(\), New String\(\) \{[^}]*\}\)','s.Trim().ToLowerInvariant()',text)
    return tuple(tokens(text))
rows=[];unmatched=[]
for closure in new:
    c=closure['source_contract'];name=c['current_record']['name'] if 'current_record' in c else 'lambda'
    native_ids=set(closure['current_ids']);candidates=[f for f in u['fragments'] if f['member_id'] in native_ids]
    targets=[]
    if name in planning:targets=[planning[name]['portable']]
    if name in layouts:targets=[s['portable'].split('.')[-1] for s in layouts[name]['splices']]
    if name.startswith('Make') or name=='ResolveSampleSizeHypothesisType':targets.append(name)
    if targets:
        member_ids={n['symbol_id'] for collection in [p,d] for n in collection['members'] if n['name'] in targets}
        candidates.extend(f for collection in [p,d] for f in collection['fragments'] if f['member_id'] in member_ids)
    for f in c['original_fragments']:
        matches=[dict(fragment_id=g['fragment_id'],path=g['path'],member_id=g['member_id']) for g in candidates if canonical(g['code'],name)==canonical(f['code'],name)]
        row=dict(original_fragment=f['fragment_id'],original_parent=closure['original_id'],kind=f['kind'],is_branch=f['is_branch'],is_inline=f['is_inline'],actual_current_source_matches=matches,proof='EXACT_NORMALIZED_SOURCE_FRAGMENT' if matches else 'FULL_AUTHENTIC_PARENT_COMPOSITION',parent_contract='historical-callback-contracts-v1.json' if name=='lambda' else 'whole-body-contracts-v1.json',source_composition='source-composition-v1.json',qualification='Matching fragments are static source correspondences; full composite statements/declarations use the exact complete-parent reconstruction. No claim every branch dynamically executed, no new original oracle.')
        rows.append(row)
        if (f['is_branch'] or f['is_inline']) and not matches:unmatched.append(dict(row,original_code=f['code']))
save('new-parent-fragment-crosswalk-v1.json',dict(rows=rows,original_parents=24,exact_fragment_matches=sum(bool(r['actual_current_source_matches']) for r in rows),composite_fragment_proofs=sum(not r['actual_current_source_matches'] for r in rows),branch_inline_composite_details=unmatched,original_queries=0))
print(json.dumps(dict(rows=len(rows),branch_inline_composite=len(unmatched),exact=sum(bool(r['actual_current_source_matches']) for r in rows))))
