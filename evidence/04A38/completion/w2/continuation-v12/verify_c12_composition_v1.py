"""Independent exact original/native/portable source reconstructions for C12."""
import hashlib,json,re,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent/'continuation-v11'))
from source_views import body
from verify_composition_v2 import tokens,phase,unwrap
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
OLD='evidence/04A38/completion/w2/continuation-v11/'
def need(v,m):
    if not v:raise ValueError(m)
PHASES={
'MCA_SUMMARY':dict(api='BuildMcaSummary',owner='UdfMultivariateReportPreparation38',start='                Dim out(',end='                Return PrepareResultTableForUdf(out)',
    call='                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.BuildMcaSummary(h.Model, h.ObservationCount, h.VariableNames, eigen, hdr)\n',
    bindings={'h.ObservationCount':'observationCount','h.VariableNames':'variableNames','h.Model':'model'},extra='Return out'),
'DA_SUMMARY':dict(api='BuildDiscriminantSummary',owner='UdfMultivariateReportPreparation38',start='                Dim out(',end='                Select Case ParseDiscriminantValidationMode',
    call='                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.BuildDiscriminantSummary(model, h.Method, h.GroupVariableName, h.VariableNames, h.Standardization, h.MissingValuePolicy, h.PriorMode, h.ValidationMode, h.CovarianceRegularization)\n',
    bindings={'h.Method':'method','h.GroupVariableName':'groupVariableName','h.VariableNames':'variableNames','h.Standardization':'standardization','h.MissingValuePolicy':'missingValuePolicy','h.PriorMode':'priorMode','h.ValidationMode':'validationMode','h.CovarianceRegularization':'covarianceRegularization'},extra='Return out'),
'TryGetCategoricalMatrix':dict(api='CompleteCategoricalMatrixPrepared',owner='UdfModelImportPreparation38',start='        Dim inferred(',end='    End Function',
    call='        Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CompleteCategoricalMatrixPrepared(\n            arr, varNames, cols, assumeHeader, startRow, rows,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText,\n            AddressOf ResolveImportLabelNames, data, resolvedNames)\n',
    bindings={'Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell':'isBlank','Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText':'toText','ResolveImportLabelNames':'resolveNames'},extra=''),
'TryGetStringVector':dict(api='TryGetStringVectorPrepared',owner='UdfModelImportPreparation38',start='        Dim rows As Integer = arr.GetLength(0)',end='    End Function',
    call='        Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.TryGetStringVectorPrepared(arr, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, values)\n',
    bindings={'Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText':'convertCell'},extra=''),
'TryGetDoubleVector':dict(api='TryGetDoubleVectorPrepared',owner='UdfModelImportPreparation38',start='        Dim rows As Integer = arr.GetLength(0)',end='    End Function',
    call='        Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.TryGetDoubleVectorPrepared(arr, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble, values)\n',
    bindings={'Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble':'convertCell'},extra='')}
HELPERS={'BuildMcaCategoryOverviewOutput','BuildMcaCategoryAxisMetricOutput','BuildMcaBurtOutput','BuildMcaIndicatorOutput','BuildDiscriminantGroupSummaryTable'}
def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {};raw=lambda p:overrides[p] if p in overrides else (root/p).read_bytes();load=lambda p:json.loads(raw(p))
    contracts=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];portable=load(PREFIX+'portable-api-contracts-v1.json')['contracts']
    apis={(c['source_record']['owner'].split('.')[-1],c['source_record']['name']):c for c in portable}
    previous=load(OLD+'current-universe-packet-v2.json');old={(n['owner'],n['name']):n for n in previous['members'] if n['parent_member'] is None}
    transitions={r['path']:r for r in load(PREFIX+'source-transitions-final-v1.json')['files']};rows=[]
    for c in contracts:
        if not c['new_support']:continue
        n=c['current_record'];name=n['name'];current=body(raw(n['path']),n)[0];authentic=body(raw(c['original_source']['path']),c['original_record'])[0]
        if name in PHASES:
            spec=PHASES[name];entry=body(raw(transitions[n['path']]['preimage']['path']),old[n['owner'],name])[0].replace('\r\n','\n')
            need(tokens(entry)==tokens(authentic),'Entry/original complete body differs '+name)
            a=entry.index(spec['start']);b=entry.index(spec['end'],a);segment=entry[a:b]
            need(tokens(current)==tokens(entry[:a]+spec['call']+entry[b:]),'Native change outside exact prepared segment '+name)
            expected=segment
            for left,right in spec['bindings'].items():expected=expected.replace(left,right)
            expected+='\n'+spec['extra'];api=apis[spec['owner'],spec['api']]
            actual=phase(body(raw(api['source']['path']),api['source_record'])[0])
            need(tokens(actual)==tokens(expected),'Portable complete prepared body differs '+name)
            rows.append(dict(original_id=c['original_id'],current_id=n['symbol_id'],portable_id=api['source_record']['symbol_id'],mode='EXACT_NATIVE_PREFIX_PREPARED_BODY_SUFFIX_COMPOSITE',
                original_segment=segment,bindings=spec['bindings'],actual_native_call=spec['call'],original_segment_sha256=hashlib.sha256(segment.encode()).hexdigest()))
        elif name in HELPERS:
            api=apis['MultivariateAnalysisUDFsReports38',name];actual=unwrap(phase(body(raw(api['source']['path']),api['source_record'])[0]));expected=phase(authentic)
            need(tokens(actual)==tokens(expected),'Original complete report helper differs '+name)
            need('MultivariateAnalysisUDFsReports38.'+name+'(' in current and 'MaterializePortableReport38(' in current,'Wrong report/materializer owner '+name)
            rows.append(dict(original_id=c['original_id'],current_id=n['symbol_id'],portable_id=api['source_record']['symbol_id'],mode='EXACT_ORIGINAL_REPORT_TYPED_PROJECTION_NATIVE_MATERIALIZER'))
        else:
            need(tokens(current)==tokens(authentic),'Unreconciled full body transition '+name)
            rows.append(dict(original_id=c['original_id'],current_id=n['symbol_id'],mode='EXACT_COMPLETE_ORIGINAL_CURRENT_PARENT'))
    need(len(rows)==26 and sum(r['mode'].startswith('EXACT_NATIVE') for r in rows)==5 and sum('TYPED_PROJECTION' in r['mode'] for r in rows)==5,'Wrong complete source composition cohort')
    return dict(status='C12_FULL_SOURCE_COMPOSITION_PASS_NOT_READY',rows=rows,original_queries=0,qualification='Full actual original/native/portable source; no inference that every dynamic/native arm executed.')
if __name__=='__main__':print(json.dumps(inspect()))
