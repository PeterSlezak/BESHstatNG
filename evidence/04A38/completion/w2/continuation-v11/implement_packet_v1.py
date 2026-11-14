"""Move exact pure layout segments; retain all native acquisition and lifecycle code."""
from common import *
import re

plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
for row in plan['source_before']:
    assert pin(ROOT/row['live']['path'])==row['live']
host=plan['source_paths'][0];common=plan['source_paths'][1]
source=(ROOT/host).read_text(encoding='utf-8-sig')
portable=(ROOT/common).read_text(encoding='utf-8-sig')
specs=[
 ('FA_COMMUNALITIES','BuildFactorCommunalities',
  'model As Multivariate.FactorAnalysis, variableNames() As String, factorNames() As String, contrib(,) As Double, p As Integer, k As Integer, hdr As Boolean',
  'h.Model, h.VariableNames, factorNames, contrib, p, k, hdr',{'h.Model':'model','h.VariableNames':'variableNames'}),
 ('FA_FACTORABILITY','BuildFactorability',
  'model As Multivariate.FactorAnalysis, hdr As Boolean','h.Model, hdr',{'h.Model':'model'}),
 ('KMEANS_SUMMARY','BuildKMeansSummary',
  'model As Multivariate.KMeans, variableNames() As String, initializationMethod As String, distanceMetric As String, standardization As String, missingValuePolicy As String, emptyClusterHandling As String, maxIterations As Integer, tolerance As Double, hdr As Boolean',
  'h.Model, h.VariableNames, h.InitializationMethod, h.DistanceMetric, h.Standardization, h.MissingValuePolicy, h.EmptyClusterHandling, h.MaxIterations, h.Tolerance, hdr',
  {'h.Model':'model','h.VariableNames':'variableNames','h.InitializationMethod':'initializationMethod',
   'h.DistanceMetric':'distanceMetric','h.Standardization':'standardization','h.MissingValuePolicy':'missingValuePolicy',
   'h.EmptyClusterHandling':'emptyClusterHandling','h.MaxIterations':'maxIterations','h.Tolerance':'tolerance'}),
 ('HCLUST_SUMMARY','BuildHierarchicalSummary',
  'model As Multivariate.HierarchicalClustering, variableNames() As String, linkage As String, distanceMetric As String, minkowskiPower As Double, standardization As String, missingValuePolicy As String, mergeSteps As Integer, ByRef finalHeight As Object, hdr As Boolean',
  'h.Model, h.VariableNames, h.Linkage, h.DistanceMetric, h.MinkowskiPower, h.Standardization, h.MissingValuePolicy, mergeSteps, finalHeight, hdr',
  {'h.Model':'model','h.VariableNames':'variableNames','h.Linkage':'linkage','h.DistanceMetric':'distanceMetric',
   'h.MinkowskiPower':'minkowskiPower','h.Standardization':'standardization','h.MissingValuePolicy':'missingValuePolicy'}),
 ('CA_SUMMARY','BuildCaSummary','model As Multivariate.CA, eigen() As Double, hdr As Boolean',
  'h.Model, eigen, hdr',{'h.Model':'model'})]
rows=[];new_methods=[]
for name,api,parameters,arguments,replacements in specs:
    match=re.search(r'        Public Function '+name+r'\([\s\S]*?        End Function',source)
    assert match is not None
    before=match[0]
    start=before.index('                Dim out(')
    end=before.index('                Return Prepare',start)
    old_layout=before[start:end]
    materialization=before[end:before.index('\n',end)]
    assert materialization.endswith('(out)')
    new_call='                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.'+api+'('+arguments+')\n'
    after=before[:start]+new_call+before[end:]
    assert after.replace(new_call,old_layout)==before
    layout=old_layout
    for old,new in replacements.items():
        layout=layout.replace(old,new)
    layout='\n'.join(line[4:] if line.startswith('    ') else line for line in layout.split('\n')).rstrip()
    method='        Public Shared Function '+api+'('+parameters+') As Object(,)\n'+layout+'\n            Return out\n        End Function\n'
    new_methods.append(method)
    rows.append(dict(name=name,api=api,parameters=parameters,arguments=arguments,replacements=replacements,
        entry_whole_body=before,current_whole_body=after,native_prefix=before[:start],
        entry_layout=old_layout,current_call=new_call,native_suffix=before[end:],
        portable_whole_body=method,materialization=materialization,
        composite_reconstruction_sha256=sha(before.encode()),
        qualification='Exact full source reconstruction, including every branch/inline arm. The original native exception boundary and header/derived-data ordering remain. New API arguments read sealed handle auto-properties; no callback or model algorithm moves. No dynamic-arm coverage claim.'))
    source=source[:match.start()]+after+source[match.end():]
assert len(rows)==5
assert portable.count('    End Class')==1
portable=portable.replace('Imports System.Globalization\n','Imports System.Globalization\nImports System.Linq\n')
portable=portable.replace('    End Class','\n'+'\n'.join(new_methods)+'    End Class')
# Persist complete proposed source before applying it, with entry pins checked above.
save('source-composition-v1.json',dict(rows=rows,original_queries=0,
    retained_defects=['FA_FACTORABILITY original inclusive upper bounds If(hdr,5,4) cannot hold the final RMSR row. Preserved exactly; no scientific/output correction authorized.']))
write_source(host,source);write_source(common,portable)
save('packet-source-transition-v1.json',dict(files=[dict(before=r['live'],preimage=r['preimage'],
    after=pin(ROOT/r['live']['path'])) for r in plan['source_before'][:2]],original_queries=0))
print('C11 five exact layout phases extracted; original full parents reconstruct byte-exact after LF decoding')
