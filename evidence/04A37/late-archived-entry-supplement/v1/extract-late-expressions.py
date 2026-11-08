"""Bind exact original late text expressions without invoking any native object."""
from pathlib import Path
import re,json,hashlib
HERE=Path(__file__).resolve().parent
sources={n:(HERE/'archived-source'/(n+'PlotExcel.vb.txt')).read_text(encoding='utf-8-sig') for n in ['Pca','FactorAnalysis','CorrespondenceAnalysis']}
bindings=[]
def remember(family,expression,role):
    assert expression in sources[family]
    bindings.append(dict(family=family,role=role,expression=expression,sha256=hashlib.sha256(expression.encode()).hexdigest()))
    return expression
def interpolations(family):return re.findall(r'\$"(?:[^"\n]|"#0\.0#")*"',sources[family])
pc=interpolations('Pca');fa=interpolations('FactorAnalysis');ca=interpolations('CorrespondenceAnalysis')
out=['Option Explicit On','Option Strict On','Imports System','Namespace Frozen37','Public Class ArchivedLateExpressions37',
 'Public Shared Function Component(pct As Double(), ordinal As Integer, score As Boolean) As String']
for i,prefix in enumerate(['1st','2nd','3rd']):
    expr=next(e for e in pc if e.startswith('$"'+prefix+' Component'))
    expr=remember('Pca',expr,'loading/biplot-component-'+str(i))
    if i in [0,2]:
        cap=prefix[0]+prefix[1].upper()+prefix[2:]
        score_expr=remember('Pca',next(e for e in pc if e.startswith('$"'+cap+' Component')),'score-component-'+str(i))
        out+=['If ordinal = '+str(i)+' Then','If score Then Return '+score_expr,'Return '+expr,'End If']
    else:out+=['If ordinal = 1 Then Return '+expr]
out+=['Throw New ArgumentOutOfRangeException(NameOf(ordinal))','End Function',
 'Public Shared Function Factor(factorNames As String(), factorPct As Double(), ordinal As Integer) As String']
for i in range(3):out+=['If ordinal = '+str(i)+' Then Return '+remember('FactorAnalysis',next(e for e in fa if e.startswith('$"{factorNames('+str(i)+')}')),'factor-caption-'+str(i))]
out+=['Throw New ArgumentOutOfRangeException(NameOf(ordinal))','End Function',
 'Public Shared Function Correspondence(pct As Double(,), ordinal As Integer) As String']
for i in [1,2]:out+=['If ordinal = '+str(i)+' Then Return '+remember('CorrespondenceAnalysis',next(e for e in ca if e.startswith('$"Factor '+str(i)+' ')),'CA-percent-index-'+str(i))]
out+=['Throw New ArgumentOutOfRangeException(NameOf(ordinal))','End Function',
 'Public Shared Function Percent(pct As Double(), i As Integer) As String', 'Return '+remember('Pca','Format$(pct(i), "#0.0#")','scree-number-expression'),'End Function',
 'Public Shared Function BiplotName(c As Double) As String','Return '+remember('Pca','"Biplot scale=" & CStr(c)','biplot-chart-name'),'End Function',
 'Public Shared Function LoadingName(id As Integer) As String','Return '+remember('Pca','"Loading_" & CStr(id)','loading-series-name'),'End Function',
 'Public Shared Function VariableLabel(varNames As String(), id As Integer) As String','Return '+remember('Pca','CStr(varNames(id))','variable-label'),'End Function',
 'Public Shared Function ContributionName(lAxis As Integer) As String','Return '+remember('CorrespondenceAnalysis','"Contribution" & CStr(lAxis + 1)','contribution-chart-name'),'End Function',
 'Public Shared Function ContributionTitle(lAxis As Integer) As String','Return '+remember('CorrespondenceAnalysis','"Contribution Plot: Axis " & CStr(lAxis + 1)','contribution-title'),'End Function',
 'Public Shared Function GroupIndices(groupLabels As String(), expectedCount As Integer) As Object()',
 'If groupLabels Is Nothing Then Return New Object(){System.Linq.Enumerable.Range(0,expectedCount).ToArray()}',
 'Dim groupIds As New Collections.Generic.List(Of String)', 'Dim seen As New Collections.Generic.HashSet(Of String)(StringComparer.Ordinal)',
 'For Each groupId As String In groupLabels','If seen.Add(groupId) Then groupIds.Add(groupId)','Next',
 'Dim result As New Collections.Generic.List(Of Object)',
 'For g As Integer=0 To groupIds.Count-1','Dim indices As New Collections.Generic.List(Of Integer)',
 'For i As Integer=0 To groupLabels.Length-1','If groupLabels(i) = groupIds(g) Then indices.Add(i)','Next','result.Add(indices.ToArray())','Next','Return result.ToArray()','End Function','End Class','End Namespace']
# /optioncompare:text matches shipping source. The source predicate and ordinal
# discovery loops above are pinned independently, not borrowed from new Common.
remember('Pca','If groupLabels(i) = groupIds(g) Then','group-membership-predicate')
remember('Pca','Dim seen As New HashSet(Of String)(StringComparer.Ordinal)','group-name-discovery-comparer')
text='\n'.join(out)+'\n';p=HERE/'ArchivedLateExpressions37.vb'
with p.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
with (HERE/'late-expression-bindings.json').open('x',encoding='utf-8',newline='\n') as f:json.dump(dict(bindings=bindings,qualification='Expression bytes are extracted verbatim. Named parameters bind the same original pct/factorNames/varNames/lAxis variables. Group ordinal metadata repeats exact original discovery and text-membership predicates; native point mapping remains source-only.'),f,indent=2);f.write('\n')
print('Bound',len(bindings),'authentic original late expressions/predicates')
