"""Individual authentic original fragment mapping to actual current/portable call dependencies."""
from common import *
from verify_c10_composition_v1 import tokens
import re
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes());pu=json.loads((HERE/'portable-universe-v1.json').read_bytes());original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());meta=json.loads((HERE/'metadata-all/packet-v2-context-v5/current-Debug.json').read_bytes());defs={m['method']:m for m in meta['methods']};portable=json.loads((HERE/'portable-api-contracts-v1.json').read_bytes())['contracts'];new={c['original_record']['symbol_id']:c for c in contracts if c['new_support'] and c['original_id']};rows=[]
for f in original['fragments']:
    if f['member_id'] not in new:continue
    c=new[f['member_id']];pool=[v for v in u['fragments'] if v['member_id']==c['current_id']];graph=set(c['compiled'][0]['transitive']['methods'])
    for p in portable:
        if p['compiled'][0]['method']['method'] in graph:pool.extend(p['fragments'])
    code=f['code'];name=c['current_record']['name'];bound=code
    if name.startswith('CT_'):
        bound=bound.replace('Return ExcelError.ExcelErrorNum','Return Nothing').replace('TransposeIntMatrix(tab)','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TransposeIntMatrix(tab)').replace('SafeCiLabel(','UdfOutputTablesReports38.SafeCiLabel(').replace('SafeCiText(','UdfOutputTablesReports38.SafeCiText(')
    if name in ['PCA_SUMMARY','PCA_EIGEN','FA_SUMMARY','FA_EIGEN']:
        bound=bound.replace('h.Model','model')
        if name=='PCA_SUMMARY':
            for a,b in [('h.RowIds','rowIds'),('h.VariableNames','variableNames'),('h.MatrixType','matrixLabel'),('h.RetentionMethod','retentionLabel'),('h.RetentionValue','retentionValue')]:bound=bound.replace(a,b)
        if name=='FA_SUMMARY':
            for a,b in [('h.VariableNames','variableNames'),('h.MatrixType','matrixLabel'),('h.ExtractionMethod','extractionLabel'),('h.RotationMethod','rotationLabel'),('h.ScoreMethod','scoreLabel')]:bound=bound.replace(a,b)
        if name=='FA_EIGEN':
            for a,b in [('Global.BESHStatNG.Matrix.MatrixArithmeticCore.Trace(','matrixTrace('),('GetOptionalBool(includeHeader, True)','readHeader(includeHeader, True)'),('ColumnSumsOfSquares(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ColumnSumsOfSquares('),('PercentOfTotal(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.PercentOfTotal('),('Cumulative(','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.Cumulative(')]:bound=bound.replace(a,b)
    matches=[v['fragment_id'] for v in pool if v['kind']==f['kind'] and any(tokens(v['code'])==tokens(candidate) for candidate in [code,bound])]
    rows.append(dict(fragment_id=f['fragment_id'],original_parent=c['original_id'],original_source_record=f,current_parent=c['current_id'],matched_current_fragments=matches,bound_source=bound,proof='EXACT_SOURCE_UNDER_ACTUAL_ARGUMENT_BINDINGS' if matches else 'COMPOSITE_STATEMENT_AUTHENTIC_FULL_PARENT_AND_PHASE_PROOF',whole_parent_authority='whole-body-contracts-v1.json',composition='source-composition-v1.json',dynamic_coverage='NOT_INFERRED_FROM_STATIC_CORRESPONDENCE'))
unmatched=[r for r in rows if not r['matched_current_fragments'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
save('new-parent-fragment-crosswalk-v1.json',dict(rows=rows,original_fragments=len(rows),exact_matches=sum(bool(r['matched_current_fragments']) for r in rows),composite=sum(not r['matched_current_fragments'] for r in rows),unmatched_branch_inline=unmatched,original_queries=0));print('Original fragments',len(rows),'unmatched branch/inline',len(unmatched))
for r in unmatched:print(r['original_source_record']['kind'],r['original_source_record']['code'][:200])
