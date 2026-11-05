"""Generate new source-pinned inert derivatives from private entry preimages only."""
import importlib.util
import re
from pathlib import Path
s=importlib.util.spec_from_file_location('c25',Path(__file__).with_name('capture-04a25-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def generate():
    form=(c.RUN/'preimages/Ui20PropensityScoreMatching.vb').read_text(encoding='utf-8-sig')
    source=(c.RUN/'preimages/PsmLovePlotExcel.vb').read_text(encoding='utf-8-sig')
    # Renderer members have eight spaces, Ui20 four.
    members={m.group(2):m.group() for m in re.finditer(r'^        (?:Public|Private) Shared (Sub|Function) (\w+)\b.*?^        End \1',source,re.M|re.S)}
    normalize=c.methods(form)['NormalizeFitOptionsForRunMethod'].replace('Private Sub','Public Shared Sub')
    table=members['BuildPlotDataTable']
    prelude=members['AddChart'];prelude=prelude[prelude.index('            Dim cleanRows'):prelude.index('            Dim shp')]
    prelude=prelude.replace('If cleanRows.Count = 0 Then Return','If cleanRows.Count = 0 Then Return EmptyPlan()')
    planner='''        Public Shared Function Plan(rows As IList(Of PsmLovePlotRow)) As Object()
            If rows Is Nothing OrElse rows.Count = 0 Then Return EmptyPlan()
'''+prelude+'''            Return New Object() {yValues, xBefore, xAfterMatching, xAfterWeighting, labels, threshold, maxX}
        End Function
        Private Shared Function EmptyPlan() As Object()
            Return New Object() {New Double() {}, New Double() {}, New Double() {}, New Double() {}, New String() {}, 0.1R, 0.25R}
        End Function
'''
    helpers='\n'.join(members[n] for n in ['AbsForChart','AbsOrMissing','MaxFiniteAbs','MaxFinite'])
    output='''Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.CausalInference
Public NotInheritable Class Psm25Legacy
'''+normalize+'\n'+table+'\n'+planner+'\n'+helpers+'\nEnd Class\n'
    output=output.replace('AppInfrastructure.IsFinite(', 'Finite(').replace('End Class\n','''        Private Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
End Class
''')
    path=c.ROOT/'BESHStat.Excel.Common.Tests/src/Psm25Legacy.vb';path.write_text(output,encoding='utf-8',newline='\n')
    c.emit('legacy-projection-provenance.json',dict(source_files={p:c.sha(c.RUN/'preimages'/Path(p).name) for p in [c.FORM,'BESHstatNG/src/Graphics/PsmLovePlotExcel.vb']},derivative=dict(path=path.relative_to(c.ROOT).as_posix(),sha256=c.sha(path)),transformations=['Original Ui20 normalize access changed to Public Shared; body unchanged.','Original pure renderer table and four numeric helpers; inaccessible Friend finite qualifier replaced by identical NaN/Infinity predicate from NumericGuards.vb.','Original AddChart prelude returned as seven channels; empty plan added; no chart/COM calls.'],status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY'))
if __name__=='__main__':generate()
