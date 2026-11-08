Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Globalization
Imports BESHStatNG
Imports BESHStatNG.Multivariate
Namespace Frozen37
Public Module Cases37
    Private Sub Put(value As Object, name As String, data As Object)
        value.GetType().GetField(name, BindingFlags.Instance Or BindingFlags.NonPublic).SetValue(value, data)
    End Sub
    Public Function Matrix(rows As Integer, columns As Integer, id As Integer) As Double(,)
        Dim result(rows - 1, columns - 1) As Double
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To columns - 1
                result(i, j) = If(id Mod 11 = 0, 0.0, (i - 2.0) * (j + 0.25) / 7.0)
                If id Mod 13 = 0 Then result(i, j) = BitConverter.Int64BitsToDouble(Long.MinValue)
            Next
        Next
        Return result
    End Function
    Public Function PcaModel(id As Integer) As PCA
        If id = 0 Then Return Nothing
        Dim result As New PCA, n As Integer = id Mod 7, p As Integer = 4, k As Integer = id Mod 4
        Dim eig() As Double = {4, 2, 1, 0.5}, scores = Matrix(n, k, id), loadings = Matrix(p, k, id + 1)
        Select Case id Mod 32
            Case 4 : eig = Nothing
            Case 5 : eig = {1}
            Case 6 : eig = {0, 0, 0, 0}
            Case 7 : eig = {-4, -2, 1, 0.5}
            Case 8 : eig(0) = Double.NaN
            Case 9 : eig(0) = Double.PositiveInfinity
            Case 10 : loadings = Nothing
            Case 11 : scores = Nothing
            Case 18 : scores = Matrix(n, 1, id)
        End Select
        Put(result, "n", n) : Put(result, "p", p) : Put(result, "pNoExtractComponents", k)
        Put(result, "pEigenval", eig) : Put(result, "pFinalDataset", scores) : Put(result, "pLoadings", loadings)
        Put(result, "pRowNums", If(id Mod 32 = 13, Nothing, Enumerable.Range(0, n).Select(Function(i) 501 - i * 17).ToArray()))
        Put(result, "pVarNames", If(id Mod 32 = 12, Nothing, New String(){"A", "A", "", "μ"}))
        Return result
    End Function
    Public Function Labels(id As Integer) As String()
        If id Mod 4 = 0 Then Return Nothing
        Return Enumerable.Range(0, If(id Mod 32 = 14, id Mod 7 + 1, id Mod 7)).Select(Function(i) New String(){Nothing, " ", " dup ", "μ"}(i Mod 4)).ToArray()
    End Function
    Public Function Groups(id As Integer) As String()
        If id Mod 3 = 0 Then Return Nothing
        Return Enumerable.Range(0, If(id Mod 32 = 15, id Mod 7 + 1, id Mod 7)).Select(Function(i) New String(){" B ", "a", "A", Nothing, "", "Ｂ", "b"}(i Mod 7)).ToArray()
    End Function
    Public Function Scale(id As Integer) As Double
        Return New Double(){0, 0.5, 1, 0.25, 0.75, -0.1, 1.1, Double.NaN}(id Mod 8)
    End Function
    Public Function FactorModel(id As Integer) As FactorAnalysis
        If id = 0 Then Return Nothing
        Dim result As New FactorAnalysis, k As Integer = id Mod 4, p As Integer = 4
        Dim pattern = Matrix(p, k, id), structureMatrix = Matrix(p, k, id + 2), trace = Matrix(p, p, id + 1)
        Dim eig() As Double = {4, 2, 1, 0.5}
        Select Case id Mod 24
            Case 4 : eig = Nothing
            Case 5 : eig = {}
            Case 6 : trace = New Double(p - 1, p - 1) {}
            Case 7 : For i = 0 To p - 1 : trace(i, i) = -1 : Next
            Case 8 : trace(0, 0) = Double.NaN
            Case 9 : trace(0, 0) = Double.PositiveInfinity
            Case 10 : pattern = Nothing
            Case 11 : structureMatrix = Nothing
            Case 12 : structureMatrix = Matrix(1, 1, id)
            Case 13 : trace = Nothing
        End Select
        Put(result, "pNoFactors", k) : Put(result, "pp", p) : Put(result, "pInitialEigenvalues", eig)
        Put(result, "pPatternMatrix", pattern) : Put(result, "pStructureMatrix", structureMatrix) : Put(result, "pWorkingMatrix", trace)
        Put(result, "pVarNames", New String(){"A", "A", "", "μ"})
        Return result
    End Function
    Public Function CaModel(id As Integer) As CA
        If id = 0 Then Return Nothing
        Dim result As New CA, k As Integer = id Mod 4, rows As Integer = 3, cols As Integer = 4
        Put(result, "pRowFactors", Matrix(rows, k, id)) : Put(result, "pColFactors", Matrix(cols, k, id + 1))
        Put(result, "pRowContribution", Matrix(rows, k, id + 2)) : Put(result, "pColContribution", Matrix(cols, k, id + 3))
        Put(result, "pRowNames", New String(){" row ", "row", "μ"}) : Put(result, "pColNames", New String(){"col", "col", "", "ž"})
        Put(result, "pVarNamesToPresent", New String(){"var1", "var1", "var2", "var2"})
        Put(result, "pbMultiple", id Mod 2 = 0) : Put(result, "pDim", k - 1)
        Put(result, "pEigenvalues", New Double(){4, 2, 1, 0.5})
        Return result
    End Function
    Public Function Layout(id As Integer) As DendrogramLayout
        Dim n As Integer = 1 + id Mod 5
        Dim x() As Double = If(id Mod 17 = 0, New Double(){}, If(id Mod 13 = 0, New Double(){Double.NaN, -0.0, 0}, New Double(){0.5, 1.5, 2.5}))
        Dim result As New DendrogramLayout With {.PolylineX=x, .PolylineY=New Double(){0, 1.75, 2}, .Orientation=CType(id Mod 4,DendrogramOrientation), .HeightMode=CType((id \ 4) Mod 2,DendrogramHeightMode), .MaximumHeight=If(id Mod 3=0,0.0,2.0), .LeafCount=n, .LeafX=Enumerable.Range(1,n).Select(Function(i) CDbl(i)).ToArray(), .LeafY=New Double(n-1){}, .LeafLabels=Enumerable.Range(0,n).Select(Function(i) New String(){"dup", "dup", "", Nothing, "long μ label"}(i Mod 5)).ToArray(), .LeafOriginalRowOrder=Enumerable.Range(0,n).Select(Function(i) n-i+7).ToArray()}
        If id Mod 3 <> 0 Then result.CutLineX=New Double(){-1,3}:result.CutLineY=New Double(){1.5,1.5}
        If id Mod 7=0 Then result.CutLineX=New Double(){99}
        result.ClusterPolylineX=New Collections.Generic.List(Of Double()) From {New Double(){0,1},Nothing,New Double(){},New Double(){2,3}}
        result.ClusterPolylineY=New Collections.Generic.List(Of Double()) From {New Double(){1,2},Nothing,New Double(){},New Double(){3,4}}
        Return result
    End Function
    Public Function Snapshot(value As Object) As String
        If value Is Nothing Then Return "N"
        If TypeOf value Is Double Then Return "D" & BitConverter.DoubleToInt64Bits(CDbl(value)).ToString("X16",CultureInfo.InvariantCulture)
        If TypeOf value Is Array Then
            Dim a = DirectCast(value, Array)
            Return a.GetType().GetElementType().FullName & "[" & String.Join(",",Enumerable.Range(0,a.Rank).Select(Function(i) a.GetLowerBound(i).ToString(CultureInfo.InvariantCulture) & ":" & a.GetLength(i).ToString(CultureInfo.InvariantCulture))) & "]{" & String.Join("|",a.Cast(Of Object).Select(Function(v) Snapshot(v))) & "}"
        End If
        If TypeOf value Is String Then Return "S" & Convert.ToBase64String(Text.Encoding.UTF8.GetBytes(CStr(value)))
        Return value.GetType().FullName & ":" & Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function
End Module
End Namespace
