Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Optional local compatibility receipts. Expected values come from the
''' frozen 04A30 source fixture; IEEE text never serializes nonfinite JSON numbers.</summary>
Public Module Graphics31Evidence
    Public Function Bits(values() As Double) As String
        If values Is Nothing Then Return "Nothing"
        Return String.Join("|", values.Select(Function(x) BitConverter.DoubleToInt64Bits(x).ToString("X16", CultureInfo.InvariantCulture)))
    End Function

    Public Sub EqualBits(expected() As Double, actual() As Double)
        If expected Is Nothing Then
            Assert.IsNull(actual) : Return
        End If
        Assert.IsNotNull(actual)
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            'NaNs are compared by classification in legacy numerical operations;
            'provenance and coordinate copies additionally use exact Bits strings.
            If Double.IsNaN(expected(i)) Then
                Assert.IsTrue(Double.IsNaN(actual(i)), "NaN slot " & i)
            Else
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected(i)), BitConverter.DoubleToInt64Bits(actual(i)), "IEEE slot " & i)
            End If
        Next
    End Sub

    Private Function Csv(value As String) As String
        Return """" & value.Replace("""", """""") & """"
    End Function

    Public Sub Record(id As String, input As String, options As String, shape As String, expected() As Double, actual() As Double, Optional expectedError As Type = Nothing, Optional actualError As Type = Nothing)
        Dim folder = Environment.GetEnvironmentVariable("BESHSTAT_04A31_CASES")
        If String.IsNullOrEmpty(folder) Then Return
        Directory.CreateDirectory(folder)
        Dim fields = {id, input, options, shape, Bits(expected), Bits(actual), If(expectedError Is Nothing, "None", expectedError.FullName), If(actualError Is Nothing, "None", actualError.FullName), CultureInfo.CurrentCulture.Name}
        Dim key As String
        Using algorithm = System.Security.Cryptography.SHA256.Create()
            key = BitConverter.ToString(algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(id))).Replace("-", "")
        End Using
        File.WriteAllLines(Path.Combine(folder, key & ".csv"), {"id,input_ieee,options,shape,expected_ieee,actual_ieee,expected_exception,actual_exception,culture", String.Join(",", fields.Select(Function(x) Csv(x)))})
    End Sub
End Module
