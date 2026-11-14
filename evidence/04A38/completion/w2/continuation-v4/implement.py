from common import *
import re
def replace_member(path,name,new):
 s=(ROOT/path).read_text(encoding='utf-8-sig');m=re.search(r'^([ \t]*)(?:Friend|Public|Private) (?:Shared )?Function '+name+r'\b',s,re.M);assert m
 end=s.index('\n'+m[1]+'End Function',m.end())+len('\n'+m[1]+'End Function');s=s[:m.start()]+new.rstrip()+s[end:];write_source(path,s)
core='BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb'
old=(ROOT/core).read_text(encoding='utf-8-sig');start=old.index('        Public Shared Function TryInvertMatrix(');end=old.index('\n        End Function',start)+len('\n        End Function');prior=old[start:end]
prefix=prior[prior.index('            If a Is Nothing'):prior.index('\n            Try')].replace('Dim nRows As Integer =','nRows =').replace('Dim nCols As Integer =','nCols =')
inside=prior[prior.index('                Dim iErr'):prior.index('\n            Catch')];inside='\n'.join(line[4:] for line in inside.splitlines())
new='''        Public Shared Function TryInvertMatrix(a As Double(,), ByRef inv As Double(,)) As Boolean
            inv = Nothing
            Dim nRows As Integer = 0
            Dim nCols As Integer = 0
            If Not TryGetSquareMatrixDimensions(a, nRows, nCols) Then Return False
            Try
                Return TryInvertPreparedSquareMatrix(a, nRows, nCols, inv)
            Catch
                inv = Nothing
                Return False
            End Try
        End Function

        ' Shape acquisition runs before the caller's exception boundary.
        Public Shared Function TryGetSquareMatrixDimensions(a As Double(,), ByRef nRows As Integer, ByRef nCols As Integer) As Boolean
'''+prefix+'''
            Return True
        End Function

        ' Caller supplies the validated dimensions and owns the catch/runtime state.
        ' No new inverse algorithm or numerical policy; exceptions propagate intact.
        Public Shared Function TryInvertPreparedSquareMatrix(a As Double(,), nRows As Integer, nCols As Integer, ByRef inv As Double(,)) As Boolean
'''+inside+'''
        End Function'''
write_source(core,old[:start]+new+old[end:])
replace_member('BESHstatNG/src/ExcelUDFs/UdfLinearAlgebra.vb','TryInvertMatrix','''    Friend Function TryInvertMatrix(a As Double(,), ByRef inv As Double(,)) As Boolean
        inv = Nothing
        Dim nRows As Integer = 0
        Dim nCols As Integer = 0
        If Not Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryGetSquareMatrixDimensions(a, nRows, nCols) Then Return False
        Try
            Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryInvertPreparedSquareMatrix(a, nRows, nCols, inv)
        Catch
            inv = Nothing
            Return False
        End Try
    End Function''')
native='BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb';s=(ROOT/native).read_text(encoding='utf-8-sig')
classical='''                Dim mdl As New parametric.OneWayANOVA(data, names)
                mdl.compute()
                Dim tables = mdl.wrapResults()
                Return PrepareResultTableForUdf(tables(0))'''
assert s.count(classical)==1;s=s.replace(classical,'''                Return PrepareResultTableForUdf(Global.BESHStatNG.ExcelCommon.ParametricUdfReports38.BuildOneWayAnovaTable(data, names))''')
welchstart=s.index('                Dim mdl As New parametric.OneWayANOVA(data, names)',s.index('        Public Function ANOVA1_WELCH('));welchend=s.index('\n                Return PrepareResultTableForUdf(t)',welchstart)+len('\n                Return PrepareResultTableForUdf(t)');welch=s[welchstart:welchend]
s=s[:welchstart]+'''                Return PrepareResultTableForUdf(Global.BESHStatNG.ExcelCommon.ParametricUdfReports38.BuildWelchAnovaTable(data, names))'''+s[welchend:]
mcpstart=s.index('                Dim mdl As New parametric.OneWayANOVA(data, names)',s.index('        Public Function ANOVA1_MCP('));mcpend=s.index('\n            Catch ex As Exception',mcpstart);mcp=s[mcpstart:mcpend]
s=s[:mcpstart]+'''                Dim stackAll As Boolean = False
                Dim tables = Global.BESHStatNG.ExcelCommon.ParametricUdfReports38.BuildOneWayMcpTables(data, names, which, alphaValue, stackAll)
                If tables Is Nothing Then Return ExcelError.ExcelErrorValue
                If Not stackAll Then Return PrepareResultTableForUdf(tables(1))
                Dim stacked As Object(,) = PrepareResultTableForUdf(tables(1))
                For i As Integer = 2 To tables.Count - 1
                    stacked = StackWithBlankRow(stacked, PrepareResultTableForUdf(tables(i)))
                Next
                Return PrepareResultTableForUdf(stacked)'''+s[mcpend:]
write_source(native,s)
newclass='''    '''+'\n'+'''    '''+'<summary>Existing Core ANOVA orchestration and report metadata; host output adaptation remains at the caller.</summary>\n'+'''    Public NotInheritable Class ParametricUdfReports38
        Private Sub New()
        End Sub

        Public Shared Function BuildOneWayAnovaTable(data()() As Double, names() As String) As Global.BESHStatNG.ResultTable
            Dim mdl As New Global.BESHStatNG.parametric.OneWayANOVA(data, names)
            mdl.compute()
            Dim tables = mdl.wrapResults()
            Return tables(0)
        End Function

        Public Shared Function BuildWelchAnovaTable(data()() As Double, names() As String) As Global.BESHStatNG.ResultTable
'''
newclass+='\n'.join(line[4:].replace('parametric.OneWayANOVA','Global.BESHStatNG.parametric.OneWayANOVA').replace('New ResultTable','New Global.BESHStatNG.ResultTable') for line in welch.splitlines()[:-1])+'''
            Return t
        End Function

        ' Invalid selectors are resolved only AFTER construction/compute, as originally.
        ' wrapResults always supplies a non-null List in the pinned Core engine.
        Public Shared Function BuildOneWayMcpTables(data()() As Double, names() As String, which As String, alphaValue As Double, ByRef stackAll As Boolean) As List(Of Global.BESHStatNG.ResultTable)
            Dim mdl As New Global.BESHStatNG.parametric.OneWayANOVA(data, names)
            mdl.compute()
            stackAll = False
            Select Case LegacyUdfText38.CaseToken(which, New String() {"", "TUKEY", "TUKEYKRAMER", "TUKEY-KRAMER", "TK", "GAMESHOWELL", "GAMES-HOWELL", "GH", "GAMES", "LSD", "FISHER", "FISHERLSD", "FISHER-LSD", "BONF", "BONFERRONI", "ALL", "BOTH"})
                Case "", "TUKEY", "TUKEYKRAMER", "TUKEY-KRAMER", "TK"
                    mdl.TukeyKramer(alphaValue)
                Case "GAMESHOWELL", "GAMES-HOWELL", "GH", "GAMES"
                    mdl.GamesHowell(alphaValue)
                Case "LSD", "FISHER", "FISHERLSD", "FISHER-LSD"
                    mdl.FisherLSD(False, alphaValue)
                Case "BONF", "BONFERRONI"
                    mdl.FisherLSD(True, alphaValue)
                Case "ALL", "BOTH"
                    mdl.FisherLSD(False, alphaValue)
                    mdl.FisherLSD(True, alphaValue)
                    mdl.TukeyKramer(alphaValue)
                    mdl.GamesHowell(alphaValue)
                    stackAll = True
                Case Else
                    Return Nothing
            End Select
            Return mdl.wrapResults()
        End Function
    End Class
'''
p='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb';text=(ROOT/p).read_text(encoding='utf-8-sig');i=text.rindex('End Namespace');write_source(p,text[:i]+newclass+text[i:])
save('implementation.json',dict(changed=[core,'BESHstatNG/src/ExcelUDFs/UdfLinearAlgebra.vb',native,p],inversion='Source-backed Windows VB Err catch restoration; engine untouched',reports='Existing OneWayANOVA/ResultTable reused; native acquisition/errors/render/stack timing retained',original_calls=0,original_source_fragments=dict(classical=classical,welch=welch,mcp=mcp)))
