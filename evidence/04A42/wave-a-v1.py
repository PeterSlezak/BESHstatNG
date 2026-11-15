from common42 import *
import re
lock();assert (HERE/'entry/Release-compiled-v1.json').exists() and (HERE/'fixtures/pre-edit-current-main-v1.json').exists()
def write(p,text):
    p=ROOT/p;p.write_bytes(text.replace('\r\n','\n').replace('\n','\r\n').encode('utf-8'))
def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
p='BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb';s=read(p)
addition='''
        '''+"''' <summary>Preserves factor naming, contribution calculation and deferred header-read order.</summary>"+'''
        Public Shared Function BuildFactorCommunalities(model As Multivariate.FactorAnalysis, variableNames() As String, readHeader As Func(Of Boolean)) As Object(,)
            Dim factorNames() As String = model.FactorNames("Factor ")
            Dim contrib(,) As Double = model.CommunalityContributionsByFactor()
            Dim p As Integer = variableNames.Length
            Dim k As Integer = model.NumberOfFactors
            Dim hdr As Boolean = readHeader()
            Return BuildFactorCommunalities(model, variableNames, factorNames, contrib, p, k, hdr)
        End Function

        Public Shared Function BuildHierarchicalMembership(result As Multivariate.HierarchicalClusterResult,
                                                          mode As Multivariate.HierarchicalMembershipDisplayMode,
                                                          readHeight As Func(Of Double), readCount As Func(Of Integer)) As Object(,)
            If mode = Multivariate.HierarchicalMembershipDisplayMode.ByHeight Then
                Return result.GetMembershipTableByHeight(readHeight())
            End If
            Return result.GetMembershipTable(readCount())
        End Function

        Public Shared Function SelectDiscriminantPrediction(model As Multivariate.DiscriminantAnalysis, normalizedSource As String) As Multivariate.DiscriminantPredictionResult
            If model Is Nothing Then Return Nothing
            Select Case normalizedSource
                Case "", "TRAINING", "APPARENT", "RESUBSTITUTION"
                    Return model.TrainingClassification
                Case "VALIDATION", "CV", "CROSSVALIDATION"
                    Return model.ValidationClassification
                Case Else
                    Throw New ArgumentException("source must be training or validation.")
            End Select
        End Function

        Public Shared Function FindDiscriminantTable(model As Multivariate.DiscriminantAnalysis, title As String,
                                                    startsWith As Boolean, toText As Func(Of Object, String)) As Object(,)
            If model Is Nothing Then Return Nothing
            Dim tables As List(Of ResultTable) = model.wrapResults()
            If tables Is Nothing Then Return Nothing
            Return MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(tables, title, startsWith, toText)
        End Function
'''
assert 'SelectDiscriminantPrediction' not in s;s=s.replace('    End Class',addition+'    End Class');write(p,s)
p='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb';s=read(p)
a=s.index('                Dim factorNames() As String =',s.index('Public Function FA_COMMUNALITIES('));b=s.index('                Return PrepareResultTableForUdf(out)',a)
s=s[:a]+'''                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.BuildFactorCommunalities(h.Model, h.VariableNames, Function() GetOptionalBool(includeHeader, True))
'''+s[b:]
a=s.index('                Dim table As Object(,)',s.index('Public Function HCLUST_MEMBERSHIP('));b=s.index('                Return PrepareExistingObjectTableForUdf',a)
s=s[:a]+'''                Dim table As Object(,) = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.BuildHierarchicalMembership(h.Model.Result, cutMode, Function() GetOptionalDouble(value, 0.0R), Function() GetOptionalInt(value, 3))

'''+s[b:]
a=s.index('            If model Is Nothing Then Return Nothing',s.index('Private Function FindWrappedDiscriminantTable('));b=s.index('        End Function',a)
s=s[:a]+'''            Return Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.FindDiscriminantTable(model, title, startsWith, AddressOf CellToTrimmedText)
'''+s[b:]
a=s.index('            Dim token As String = NormalizeToken(source)',s.index('Private Function GetDiscriminantPredictionResult('));b=s.index('        End Function',a)
s=s[:a]+'''            Return Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.SelectDiscriminantPrediction(model, NormalizeToken(source))
'''+s[b:];write(p,s)
print('Wave A: factor payload preparation,hierarchical cut dispatch,discriminant source/table selection;cache/FIT/DROP unchanged',flush=True)
