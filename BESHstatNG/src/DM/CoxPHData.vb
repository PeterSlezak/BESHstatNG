Option Explicit On

''' <summary>
''' Specialized data container for Cox proportional hazards (CoxPH) survival models,
''' extending <see cref="DataObj"/>.
''' </summary>
''' <remarks>
''' <para>
''' The <c>CoxPHData</c> class inherits all functionality from <c>DataObj</c> (importing, cleaning, subsetting)
''' and adds support for CoxPH-specific features:
''' </para>
''' <list type="bullet">
'''   <item><description>Mandatory time-to-event variable (first column).</description></item>
'''   <item><description>Mandatory censoring indicator (second column, must be 0 or 1).</description></item>
'''   <item><description>Optional strata variable (third column) for stratified Cox models.</description></item>
'''   <item><description>Automatic separation of time, censoring, and strata columns from covariates.</description></item>
'''   <item><description>Creation of <c>SurvivalRecord</c> objects for each observation.</description></item>
''' </list>
''' </remarks>
''' <example>
''' ' Example: import CoxPH data with strata from a raw matrix
''' Dim cox As New CoxPHData()
''' cox.bStrata = True
''' cox.DataImportRawMatrix(raw, variableNames, CharCols:=2)
''' Console.WriteLine("Rows: " + cox.nRows + ", Cols: " + cox.nCols)
''' Console.WriteLine("Time variable: " + cox.TimeVarName)
''' Console.WriteLine("Censor variable: " + cox.CensorVarName)
''' Console.WriteLine("Strata variable: " + cox.StrataVarName)
''' </example>
Public Class CoxPHData
    Inherits DataObj

    ''' <summary>Flag indicating whether a strata variable is present.</summary>
    Public bStrata As Boolean

    ''' <summary>Array of imported strata values (optional).</summary>
    Public StrataData() As String

    ''' <summary>Array of censoring indicators (0 = event, 1 = censored).</summary>
    Public CensorData() As Integer

    ''' <summary>Array of time-to-event values.</summary>
    Public TimeData() As Double

    ''' <summary>Name of the strata variable (third column if present).</summary>
    Public StrataVarName As String

    ''' <summary>Name of the censoring variable (second column).</summary>
    Public CensorVarName As String

    ''' <summary>Name of the time variable (first column).</summary>
    Public TimeVarName As String

    ''' <summary>List of <see cref="survival.SurvivalRecord"/> objects representing each observation.</summary>
    Public SurvRecordsList = New List(Of survival.SurvivalRecord)

    ''' <summary>
    ''' Initializes a new instance of the <c>CoxPHData</c> class with strata disabled.
    ''' </summary>
    Sub New()
        MyBase.New()
        Me.bStrata = False
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                         variableNames() As String,
                                         Optional firstSourceRow As Integer = 1,
                                         Optional sourceInfo As DataSourceInfo = Nothing,
                                         Optional CharCols As Integer = -1,
                                         Optional SkipRow As Integer = 0)

        Dim effectiveCharCols As Integer = CharCols
        If Me.bStrata AndAlso effectiveCharCols < 2 Then
            effectiveCharCols = 2
        End If

        MyBase.DataImportRawMatrix(rawInput, variableNames, firstSourceRow, sourceInfo, effectiveCharCols, SkipRow)
    End Sub

    Protected Overrides Sub OnDataImported()
        If Me.bZeroValid OrElse Me.FinalData Is Nothing Then Return
        FinalizeCoxImport()
    End Sub

    Private Sub FinalizeCoxImport()
        ReDim TimeData(Me.nRows - 1), CensorData(Me.nRows - 1)

        For i = 0 To Me.nRows - 1
            Me.TimeData(i) = CDbl(Me.FinalData(i, 0))

            If Not (Me.FinalData(i, 1) = 0 Or Me.FinalData(i, 1) = 1) Then
                Throw New ArgumentException($"Censorting value is not 1/0. Value ={Me.FinalData(i, 1)}")
            End If

            Me.CensorData(i) = CInt(Me.FinalData(i, 1))
            Me.TimeVarName = Me.varNames(0)
            Me.CensorVarName = Me.varNames(1)
        Next

        If Me.bStrata Then
            ReDim Me.StrataData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.StrataData(i) = CStr(Me.FinalData(i, 2))
            Next
            Me.StrataVarName = Me.varNames(2)

            For i = 0 To Me.nRows - 1
                For j = 3 To Me.nCols - 1
                    Me.FinalData(i, j - 3) = Me.FinalData(i, j)
                    If i = 0 Then Me.varNames(j - 3) = Me.varNames(j)
                Next
            Next

            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 4)
            ReDim Preserve Me.varNames(Me.nCols - 4)
            Me.nCols -= 3
        Else
            For i = 0 To Me.nRows - 1
                For j = 2 To Me.nCols - 1
                    Me.FinalData(i, j - 2) = Me.FinalData(i, j)
                    If i = 0 Then Me.varNames(j - 2) = Me.varNames(j)
                Next
            Next

            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 3)
            ReDim Preserve Me.varNames(Me.nCols - 3)
            Me.nCols -= 2
        End If

        SurvRecordsList.Clear()
        For i = 0 To Me.nRows - 1
            Dim Xs(Me.nCols - 1) As Double
            For j = 0 To Me.nCols - 1
                Xs(j) = CDbl(Me.FinalData(i, j))
            Next

            Dim sr = New survival.SurvivalRecord
            sr.Censorship = Me.CensorData(i)
            sr.Stratum = If(Me.bStrata, Me.StrataData(i), "0")
            sr.Time = Me.TimeData(i)
            sr.Index = Me.RowIds(i)
            sr.Covariates = Xs
            SurvRecordsList.Add(sr)
        Next
    End Sub
End Class
