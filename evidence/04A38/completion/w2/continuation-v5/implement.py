"""C5 narrow conversion extraction; exact native error/catch boundaries retained."""
from common import *
import re

def replace_method(source, name, replacement):
    match = re.search(r'^([ \t]*)(?:Public|Private|Friend) Function ' + name + r'\b', source, re.M)
    assert match
    end = source.index('\n' + match[1] + 'End Function', match.end()) + len('\n' + match[1] + 'End Function')
    return source[:match.start()] + replacement.rstrip() + source[end:]

path = 'BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb'
source = (ROOT / path).read_text(encoding='utf-8-sig')
source = replace_method(source, 'CellToTrimmedText', '''        Friend Function CellToTrimmedText(v As Object) As String
            If ExcelArgPredicates.IsMissingArg(v) Then Return ""
            If TypeOf v Is ExcelError Then
                Dim s As String = Convert.ToString(v, CultureInfo.InvariantCulture)
                If s Is Nothing Then Return ""
                Return s.Trim()
            End If
            Return Global.BESHStatNG.ExcelCommon.UdfCellConversion38.CellToTrimmedText(v)
        End Function''')
source = source.replace('''                Dim d As Double = Convert.ToDouble(v)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
                Return d''', '''                Return Global.BESHStatNG.ExcelCommon.UdfCellConversion38.ConvertFiniteNumber(v)''')
write_source(path, source)

path = 'BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb'
source = (ROOT / path).read_text(encoding='utf-8-sig')
start = source.index('        Select Case Type.GetTypeCode(value.GetType())')
end = source.index('        End Select', start) + len('        End Select')
source = source[:start] + '        Return Global.BESHStatNG.ExcelCommon.UdfCellConversion38.IsNumericTypeCode(Type.GetTypeCode(value.GetType()))' + source[end:]
write_source(path, source)

new = ROOT / 'BESHStat.Excel.Common/src/Analysis/UdfCellConversion38.vb'
assert not new.exists()
new.write_text('''Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    ''' + "''' <summary>Legacy UDF conversion operations after host sentinel screening. Callers own exception handling.</summary>" + '''
    Public NotInheritable Class UdfCellConversion38
        Private Sub New()
        End Sub

        Public Shared Function CellToTrimmedText(v As Object) As String
            If TypeOf v Is String Then
                Return CStr(v).Trim()
            End If

            If TypeOf v Is Double OrElse
               TypeOf v Is Single OrElse
               TypeOf v Is Decimal OrElse
               TypeOf v Is Integer OrElse
               TypeOf v Is Long OrElse
               TypeOf v Is Short Then

                Dim d As Double = Convert.ToDouble(v, CultureInfo.InvariantCulture)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return ""

                If Math.Abs(d - Math.Round(d)) < 0.000000000001R Then
                    Return CLng(Math.Round(d)).ToString(CultureInfo.InvariantCulture)
                End If

                Return d.ToString(CultureInfo.InvariantCulture)
            End If

            Dim s As String = Convert.ToString(v, CultureInfo.InvariantCulture)
            If s Is Nothing Then Return ""
            Return s.Trim()
        End Function

        Public Shared Function ConvertFiniteNumber(v As Object) As Double?
            Dim d As Double = Convert.ToDouble(v)
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
            Return d
        End Function

        Public Shared Function IsNumericTypeCode(code As TypeCode) As Boolean
            Select Case code
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                     TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
                Case Else
                    Return False
            End Select
        End Function
    End Class
End Namespace
''', encoding='utf-8', newline='\n')
save('implementation.json', dict(changed=[pin(ROOT / p) for p in ['BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb',
     'BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb', 'BESHStat.Excel.Common/src/Analysis/UdfCellConversion38.vb']],
     new_original_queries=0, semantics='Original conversions, finite checks, type-code whitelist and uncaught rounding retained; native sentinels/ExcelError/catch/Framework Err remain host-owned'))
