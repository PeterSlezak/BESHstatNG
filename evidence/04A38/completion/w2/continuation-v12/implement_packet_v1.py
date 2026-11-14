"""Move exact prepared body segments; never execute archived original code."""
from common import *
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
for row in plan['source_before']:assert pin(ROOT/row['live']['path'])==row['live']
hostpath='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb'
importpath='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMultivariate.vb'
reportpath='BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb'
preparepath='BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb'
def text(path):return (ROOT/path).read_bytes().decode('utf-8-sig').replace('\r\n','\n')
host=text(hostpath);imports=text(importpath);report=text(reportpath);prepared=text(preparepath)
phases=[]
def get_segment(source,parent,start,end):
    p=source.index(parent);a=source.index(start,p);b=source.index(end,a)
    return source[a:b]
segment=get_segment(host,'Public Function MCA_SUMMARY(', '                Dim out(', '                Return PrepareResultTableForUdf(out)')
replacements={'h.ObservationCount':'observationCount','h.VariableNames':'variableNames','h.Model':'model'}
portable=segment
for a,b in replacements.items():portable=portable.replace(a,b)
portable+='                Return out\n'
method='        Public Shared Function BuildMcaSummary(model As Multivariate.CA, observationCount As Integer, variableNames() As String, eigen() As Double, hdr As Boolean) As Object(,)\n'+portable+'        End Function\n'
host=host.replace(segment,'                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.BuildMcaSummary(h.Model, h.ObservationCount, h.VariableNames, eigen, hdr)\n',1)
report=report.replace('    End Class',method+'\n    End Class',1)
phases.append(dict(parent='MCA_SUMMARY',portable='BuildMcaSummary',original_segment=segment,portable_segment=portable,bindings=replacements))
segment=get_segment(host,'Public Function DA_SUMMARY(', '                Dim out(', '                Select Case ParseDiscriminantValidationMode')
replacements={'h.Method':'method','h.GroupVariableName':'groupVariableName','h.VariableNames':'variableNames','h.Standardization':'standardization','h.MissingValuePolicy':'missingValuePolicy','h.PriorMode':'priorMode','h.ValidationMode':'validationMode','h.CovarianceRegularization':'covarianceRegularization'}
portable=segment
for a,b in replacements.items():portable=portable.replace(a,b)
portable+='                Return out\n'
method='        Public Shared Function BuildDiscriminantSummary(model As Multivariate.DiscriminantAnalysis, method As String, groupVariableName As String, variableNames() As String, standardization As String, missingValuePolicy As String, priorMode As String, validationMode As String, covarianceRegularization As Double) As Object(,)\n'+portable+'        End Function\n'
host=host.replace(segment,'                Dim out(,) As Object = Global.BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38.BuildDiscriminantSummary(model, h.Method, h.GroupVariableName, h.VariableNames, h.Standardization, h.MissingValuePolicy, h.PriorMode, h.ValidationMode, h.CovarianceRegularization)\n',1)
report=report.replace('    End Class',method+'\n    End Class',1)
phases.append(dict(parent='DA_SUMMARY',portable='BuildDiscriminantSummary',original_segment=segment,portable_segment=portable,bindings=replacements))
segment=get_segment(imports,'Friend Function TryGetCategoricalMatrix(', '        Dim inferred(', '    End Function')
replacements={'Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell':'isBlank','Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText':'toText','ResolveImportLabelNames':'resolveNames'}
portable=segment
for a,b in replacements.items():portable=portable.replace(a,b)
method='        Public Shared Function CompleteCategoricalMatrixPrepared(arr As Object(,), ByRef varNames As Object, cols As Integer, assumeHeader As Boolean, startRow As Integer, rows As Integer, isBlank As Func(Of Object, Boolean), toText As Func(Of Object, String), resolveNames As Func(Of Object, Integer, String, String(), String()), ByRef data(,) As String, ByRef resolvedNames() As String) As Boolean\n'+portable+'        End Function\n'
call='        Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.CompleteCategoricalMatrixPrepared(\n            arr, varNames, cols, assumeHeader, startRow, rows,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText,\n            AddressOf ResolveImportLabelNames, data, resolvedNames)\n'
imports=imports.replace(segment,call,1);prepared=prepared.replace('    End Class',method+'\n    End Class',1)
phases.append(dict(parent='TryGetCategoricalMatrix',portable='CompleteCategoricalMatrixPrepared',original_segment=segment,portable_segment=portable,bindings=replacements))
for name,kind,policy in [('TryGetStringVector','String','Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText'),('TryGetDoubleVector','Double','Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble')]:
    segment=get_segment(imports,'Friend Function '+name+'(', '        Dim rows As Integer = arr.GetLength(0)', '    End Function')
    replacements={policy:'convertCell'};portable=segment.replace(policy,'convertCell')
    method='        Public Shared Function '+name+'Prepared(arr As Object(,), convertCell As Func(Of Object, '+('String' if kind=='String' else 'Double?')+'), ByRef values() As '+kind+') As Boolean\n'+portable+'        End Function\n'
    call='        Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.'+name+'Prepared(arr, AddressOf '+policy+', values)\n'
    imports=imports.replace(segment,call,1);prepared=prepared.replace('    End Class',method+'\n    End Class',1)
    phases.append(dict(parent=name,portable=name+'Prepared',original_segment=segment,portable_segment=portable,bindings=replacements))
prepared=prepared.replace('Imports System\n','Imports System\nImports System.Collections.Generic\nImports System.Globalization\n',1)
for path,value in [(hostpath,host),(importpath,imports),(reportpath,report),(preparepath,prepared)]:write_source(path,value)
save('implementation-source-composition-v1.json',dict(phases=phases,source_after=[pin(ROOT/p) for p in [hostpath,importpath,reportpath,preparepath]],
    qualification='Exact extracted source segments and substitutions, subject to independent full body/fragment/CIL/behavior verification before any closure.',original_queries=0))
print('Five exact prepared segments moved; native FIT/acquisition/cache/error/validation-order boundaries retained')
