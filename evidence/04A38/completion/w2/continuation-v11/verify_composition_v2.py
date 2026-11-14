"""Independent full source reconstruction, never executing original code."""
import hashlib
import json
import re
from pathlib import Path
from source_views import body

ROOT=Path(__file__).resolve().parents[5]
PREFIX=Path(__file__).resolve().parent.relative_to(ROOT).as_posix()+'/'
OLD='evidence/04A38/completion/w2/continuation-v10/'
PHASES={
 'FA_COMMUNALITIES':('BuildFactorCommunalities','h.Model, h.VariableNames, factorNames, contrib, p, k, hdr',{'h.Model':'model','h.VariableNames':'variableNames'}),
 'FA_FACTORABILITY':('BuildFactorability','h.Model, hdr',{'h.Model':'model'}),
 'KMEANS_SUMMARY':('BuildKMeansSummary','h.Model, h.VariableNames, h.InitializationMethod, h.DistanceMetric, h.Standardization, h.MissingValuePolicy, h.EmptyClusterHandling, h.MaxIterations, h.Tolerance, hdr',
  {'h.Model':'model','h.VariableNames':'variableNames','h.InitializationMethod':'initializationMethod','h.DistanceMetric':'distanceMetric','h.Standardization':'standardization','h.MissingValuePolicy':'missingValuePolicy','h.EmptyClusterHandling':'emptyClusterHandling','h.MaxIterations':'maxIterations','h.Tolerance':'tolerance'}),
 'HCLUST_SUMMARY':('BuildHierarchicalSummary','h.Model, h.VariableNames, h.Linkage, h.DistanceMetric, h.MinkowskiPower, h.Standardization, h.MissingValuePolicy, mergeSteps, finalHeight, hdr',
  {'h.Model':'model','h.VariableNames':'variableNames','h.Linkage':'linkage','h.DistanceMetric':'distanceMetric','h.MinkowskiPower':'minkowskiPower','h.Standardization':'standardization','h.MissingValuePolicy':'missingValuePolicy'}),
 'CA_SUMMARY':('BuildCaSummary','h.Model, eigen, hdr',{'h.Model':'model'})}
HELPERS={'BuildClusterPreprocessingTable','BuildRemovedRowsOutput','BuildHierarchicalLeafOrderTable',
         'BuildCaEigenOutput','BuildCaOverviewOutput','BuildCaAxisMetricOutput'}

def need(value,message):
    if not value:raise ValueError(message)

def tokens(text):
    result=[]
    for match in re.finditer(r'"(?:[^"\n]|"")*"|\x27[^\n]*|[^\s]',text):
        value=match[0]
        if not value.startswith("'"):result.append(value if value.startswith('"') else value.lower())
    return ''.join(result)

def phase(text):
    text=text.replace('\r\n','\n')
    begin=text.index('(');depth=0;end=None
    for index in range(begin,len(text)):
        if text[index]=='(':depth+=1
        elif text[index]==')':
            depth-=1
            if depth==0:end=index;break
    need(end is not None,'Missing actual typed source header')
    end=text.index('\n',end)+1
    return text[end:text.rfind('End Function')].strip()

def unwrap(text):
    for value in ['note','out']:
        text=text.replace('New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel('+value+'), UdfReportPreparation38.Raw)',value)
    text=text.replace('New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.NormalizeBlanks)','PrepareResultTableForUdf(out)')
    return text.replace('UdfReportProjection38.FromFault(UdfReportFault38.NotAvailable)','ExcelError.ExcelErrorNA')

def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {}
    raw=lambda path:overrides[path] if path in overrides else (root/path).read_bytes()
    load=lambda path:json.loads(raw(path))
    contracts=load(PREFIX+'whole-body-contracts-v1.json')['contracts']
    portable=load(PREFIX+'portable-universe-v1.json')
    apis={(n['owner'].split('.')[-1],n['name']):n for n in portable['members'] if n['parent_member'] is None}
    old=load(OLD+'current-universe-packet-v2.json')
    oldby={(n['owner'],n['name']):n for n in old['members'] if n['parent_member'] is None}
    trans={r['path']:r for r in load(PREFIX+'source-transitions-final-v1.json')['files']}
    rows=[]
    for contract in contracts:
        if not contract['new_support']:continue
        n=contract['current_record'];name=n['name']
        now=body(raw(n['path']),n)[0]
        authentic=body(raw(contract['original_source']['path']),contract['original_record'])[0]
        if name in PHASES:
            api,args,replacements=PHASES[name]
            before=body(raw(trans[n['path']]['preimage']['path']),oldby[n['owner'],name])[0].replace('\r\n','\n')
            need(tokens(before)==tokens(authentic),'New phase entry differed from authentic original parent')
            begin=before.index('                Dim out(');end=before.index('                Return Prepare',begin)
            original_phase=before[begin:end]
            call='                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.'+api+'('+args+')\n'
            need(tokens(now)==tokens(before[:begin]+call+before[end:]),'Native parent changed outside exact layout '+name)
            expected=original_phase
            for a,b in replacements.items():expected=expected.replace(a,b)
            expected+='\nReturn out'
            p=apis['UdfMultivariateReportPreparation38',api]
            actual=phase(body(raw(p['path']),p)[0])
            need(tokens(actual)==tokens(expected),'Portable complete layout mismatch '+name)
            rows.append(dict(original_id=contract['original_id'],current_id=n['symbol_id'],portable_api=p['symbol_id'],
                mode='EXACT_NATIVE_PREFIX_LAYOUT_SUFFIX_COMPOSITE',consumer_arguments=args,
                original_phase_sha256=hashlib.sha256(original_phase.encode()).hexdigest()))
        elif name in HELPERS:
            p=apis['MultivariateAnalysisUDFsReports38',name]
            expected=phase(authentic);actual=unwrap(phase(body(raw(p['path']),p)[0]))
            need(tokens(actual)==tokens(expected),'Original full helper does not reconstruct '+name)
            need('MultivariateAnalysisUDFsReports38.'+name+'(' in now and 'MaterializePortableReport38(' in now,'Wrong native helper chain')
            rows.append(dict(original_id=contract['original_id'],current_id=n['symbol_id'],portable_api=p['symbol_id'],
                mode='EXACT_ORIGINAL_HELPER_TYPED_PROJECTION_NATIVE_MATERIALIZER',
                qualification='Raw and NormalizeBlanks policies and typed NA fault independently map to the complete native materializer retained in the graph.'))
        else:
            need(tokens(now)==tokens(authentic),'Unreviewed original/current parent transition '+name)
            rows.append(dict(original_id=contract['original_id'],current_id=n['symbol_id'],mode='EXACT_COMPLETE_ORIGINAL_CURRENT_PARENT'))
    need(len(rows)==31 and len([r for r in rows if r['mode'].startswith('EXACT_NATIVE')])==5 and len([r for r in rows if 'HELPER' in r['mode']])==6,'Source reconstruction scope changed')
    return dict(status='FULL_SOURCE_COMPOSITION_PASS_NOT_READY',rows=rows,original_queries=0,
        qualification='Complete source/branch/inline reconstruction; dynamic/native arm execution is not inferred.')

if __name__=='__main__':print(json.dumps(inspect()))
