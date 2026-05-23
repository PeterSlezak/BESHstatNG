Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.CausalInference
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Worksheet-facing import specification for propensity score analyses.
''' This object deliberately contains only worksheet variable keys and model-effect metadata;
''' UI controls should translate their state into this neutral specification.
''' </summary>
Public Class PsmDataImportSpec
    Public Property Worksheet As Worksheet
    Public Property VariableColumnsInfo As Dictionary(Of String, VarColumnInfo)
    Public Property TreatmentKey As String
    Public Property FormulaText As String = String.Empty
    Public Property FormulaAddressing As String = "relative"
    Public Property AbsoluteColumnLetters As String()
    Public Property AllowRelativeColumnLetters As Boolean = True
    Public Property AllowAbsoluteColumnLetters As Boolean = False
    Public Property AllowQuotedVariableNames As Boolean = True
    Public Property OutcomeKey As String
    Public Property SelectedCovariateKeys As List(Of String)
    Public Property EffectItems As IEnumerable
    Public Property TermSpecs As Dictionary(Of String, TermSpec)
    Public Property OmitCategoricalReference As Boolean = True
    Public Property ScoreMethod As PsmScoreMethod = PsmScoreMethod.LogisticRegression
    Public Property SuppliedScoreKey As String = String.Empty
    Public Property IdKey As String = String.Empty
    Public Property ExactGroupKeys As List(Of String)
End Class

''' <summary>
''' Raw-matrix import specification for propensity score analyses.
''' This is intended for future UDFs and tests where inputs are already available as matrices
''' rather than worksheet column references.
''' </summary>
Public Class PsmDataRawMatrixSpec
    Public Property ModelRawInput As Object(,)
    Public Property ModelVariableNames As String()
    Public Property TreatmentKey As String
    Public Property FormulaText As String = String.Empty
    Public Property FormulaAddressing As String = "relative"
    Public Property AbsoluteColumnLetters As String()
    Public Property AllowRelativeColumnLetters As Boolean = True
    Public Property AllowAbsoluteColumnLetters As Boolean = False
    Public Property AllowQuotedVariableNames As Boolean = True
    Public Property OutcomeRawInput As Object(,)
    Public Property OutcomeVariableNames As String()
    Public Property SelectedCovariateKeys As List(Of String)
    Public Property EffectItems As IEnumerable
    Public Property TermSpecs As Dictionary(Of String, TermSpec)
    Public Property OmitCategoricalReference As Boolean = True
    Public Property ScoreMethod As PsmScoreMethod = PsmScoreMethod.LogisticRegression
    Public Property SuppliedScoreRawInput As Object(,)
    Public Property SuppliedScoreVariableNames As String()
    Public Property IdRawInput As Object(,)
    Public Property IdVariableNames As String()
    Public Property ExactGroupRawInput As Object(,)
    Public Property ExactGroupVariableNames As String()
    Public Property FirstSourceRow As Integer = 1
    Public Property SourceWorksheet As Worksheet = Nothing
End Class

''' <summary>
''' DataObj-based composite importer for propensity score methods.
''' The object itself stores the treatment/raw-covariate model data by inheriting DataObj.
''' Outcome, supplied propensity score, ID, and exact-matching variables are imported through
''' child DataObj instances and then aligned by DataObj.RowIds. This keeps GUI and future UDF
''' front ends on the same import/cleaning path used by the regression dialogs.
''' </summary>
Public Class psmData
    Inherits DataObj

    Public Property OutcomeData As DataObj
    Public Property SuppliedScoreData As DataObj
    Public Property IdData As DataObj
    Public Property ExactGroupData As DataObj

    Public Property Input As PsmInputData
    Public Property ExpandedData As Double(,)
    Public Property ExpandedVarNames As String()
    Public Property RawCovariateKeys As List(Of String)
    Public Property EffectiveEffectItems As List(Of Object)
    Public Property PropensityTermSpecs As Dictionary(Of String, TermSpec)
    Public Property DroppedRowsDuringAlignment As Integer

    Public Property ModelReference As String = String.Empty
    Public Property OutcomeReference As String = String.Empty
    Public Property SuppliedScoreReference As String = String.Empty
    Public Property IdReference As String = String.Empty
    Public Property ExactGroupReference As String = String.Empty

    Public ReadOnly Property ImportedReferenceSummary As String
        Get
            Dim parts As New List(Of String)()
            If Not String.IsNullOrWhiteSpace(ModelReference) Then parts.Add("Model: " & ModelReference)
            If Not String.IsNullOrWhiteSpace(OutcomeReference) Then parts.Add("Outcome: " & OutcomeReference)
            If Not String.IsNullOrWhiteSpace(SuppliedScoreReference) Then parts.Add("Score: " & SuppliedScoreReference)
            If Not String.IsNullOrWhiteSpace(IdReference) Then parts.Add("ID: " & IdReference)
            If Not String.IsNullOrWhiteSpace(ExactGroupReference) Then parts.Add("Exact: " & ExactGroupReference)
            Return String.Join("; ", parts.ToArray())
        End Get
    End Property

    Public Sub DataImportFromWorksheet(spec As PsmDataImportSpec)
        ValidateWorksheetSpec(spec)
        ResetPsmImportState()

        RawCovariateKeys = BuildRequiredRawCovariateKeys(spec.SelectedCovariateKeys, spec.EffectItems, spec.TermSpecs)
        EffectiveEffectItems = BuildEffectiveEffectItems(spec.EffectItems, RawCovariateKeys)
        PropensityTermSpecs = spec.TermSpecs

        Dim modelKeys As New List(Of String)()
        modelKeys.Add(spec.TreatmentKey)
        modelKeys.AddRange(RawCovariateKeys)
        ModelReference = BuildExcelRefList(spec.Worksheet, modelKeys, spec.VariableColumnsInfo)
        Dim modelReferenceForImport As String = ModelReference
        ExcelDnaDataImporter.ImportInto(Me, modelReferenceForImport)
        If Me.bZeroValid Then Return

        OutcomeReference = BuildExcelRefList(spec.Worksheet, New List(Of String) From {spec.OutcomeKey}, spec.VariableColumnsInfo)
        OutcomeData = ImportWorksheetDataObject(OutcomeReference, charCols:=-1)

        If Not String.IsNullOrWhiteSpace(spec.SuppliedScoreKey) Then
            SuppliedScoreReference = BuildExcelRefList(spec.Worksheet, New List(Of String) From {spec.SuppliedScoreKey}, spec.VariableColumnsInfo)
            SuppliedScoreData = ImportWorksheetDataObject(SuppliedScoreReference, charCols:=-1)
        End If

        If Not String.IsNullOrWhiteSpace(spec.IdKey) Then
            IdReference = BuildExcelRefList(spec.Worksheet, New List(Of String) From {spec.IdKey}, spec.VariableColumnsInfo)
            IdData = ImportWorksheetDataObject(IdReference, charCols:=0)
        End If

        Dim exactKeys As List(Of String) = NormalizeStringList(spec.ExactGroupKeys)
        If exactKeys.Count > 0 Then
            ExactGroupReference = BuildExcelRefList(spec.Worksheet, exactKeys, spec.VariableColumnsInfo)
            ExactGroupData = ImportWorksheetDataObject(ExactGroupReference, charCols:=exactKeys.Count - 1)
        End If

        AlignChildDataObjects()
        BuildPsmInput(spec.TreatmentKey,
                      spec.OmitCategoricalReference,
                      spec.ScoreMethod,
                      formulaText:=Nothing,
                      absoluteColumnLetters:=Nothing,
                      allowRelativeColumnLetters:=True,
                      allowAbsoluteColumnLetters:=False,
                      allowQuotedVariableNames:=True)
    End Sub

    Public Sub DataImportFromRawMatrices(spec As PsmDataRawMatrixSpec)
        ValidateRawMatrixSpec(spec)
        ResetPsmImportState()

        RawCovariateKeys = BuildRequiredRawCovariateKeys(spec.SelectedCovariateKeys, spec.EffectItems, spec.TermSpecs)
        EffectiveEffectItems = BuildEffectiveEffectItems(spec.EffectItems, RawCovariateKeys)
        PropensityTermSpecs = spec.TermSpecs

        MyBase.DataImportRawMatrix(spec.ModelRawInput,
                                   spec.ModelVariableNames,
                                   firstSourceRow:=spec.FirstSourceRow,
                                   sourceWorksheet:=spec.SourceWorksheet,
                                   CharCols:=-1,
                                   SkipRow:=0)
        ModelReference = "RawMatrix: model"
        If Me.bZeroValid Then Return

        OutcomeData = ImportRawDataObject(spec.OutcomeRawInput,
                                          spec.OutcomeVariableNames,
                                          spec.FirstSourceRow,
                                          spec.SourceWorksheet,
                                          charCols:=-1)
        OutcomeReference = "RawMatrix: outcome"

        If spec.SuppliedScoreRawInput IsNot Nothing Then
            SuppliedScoreData = ImportRawDataObject(spec.SuppliedScoreRawInput,
                                                    spec.SuppliedScoreVariableNames,
                                                    spec.FirstSourceRow,
                                                    spec.SourceWorksheet,
                                                    charCols:=-1)
            SuppliedScoreReference = "RawMatrix: supplied score"
        End If

        If spec.IdRawInput IsNot Nothing Then
            IdData = ImportRawDataObject(spec.IdRawInput,
                                         spec.IdVariableNames,
                                         spec.FirstSourceRow,
                                         spec.SourceWorksheet,
                                         charCols:=0)
            IdReference = "RawMatrix: ID"
        End If

        If spec.ExactGroupRawInput IsNot Nothing Then
            Dim exactCols As Integer = spec.ExactGroupRawInput.GetLength(1)
            ExactGroupData = ImportRawDataObject(spec.ExactGroupRawInput,
                                                 spec.ExactGroupVariableNames,
                                                 spec.FirstSourceRow,
                                                 spec.SourceWorksheet,
                                                 charCols:=exactCols - 1)
            ExactGroupReference = "RawMatrix: exact groups"
        End If

        AlignChildDataObjects()
        BuildPsmInput(spec.TreatmentKey,
                      spec.OmitCategoricalReference,
                      spec.ScoreMethod,
                      formulaText:=spec.FormulaText,
                      absoluteColumnLetters:=spec.AbsoluteColumnLetters,
                      allowRelativeColumnLetters:=spec.AllowRelativeColumnLetters,
                      allowAbsoluteColumnLetters:=spec.AllowAbsoluteColumnLetters,
                      allowQuotedVariableNames:=spec.AllowQuotedVariableNames)
    End Sub

    Private Sub ResetPsmImportState()
        OutcomeData = Nothing
        SuppliedScoreData = Nothing
        IdData = Nothing
        ExactGroupData = Nothing
        Input = Nothing
        ExpandedData = Nothing
        ExpandedVarNames = Nothing
        RawCovariateKeys = New List(Of String)()
        EffectiveEffectItems = New List(Of Object)()
        PropensityTermSpecs = Nothing
        DroppedRowsDuringAlignment = 0
        ModelReference = String.Empty
        OutcomeReference = String.Empty
        SuppliedScoreReference = String.Empty
        IdReference = String.Empty
        ExactGroupReference = String.Empty
    End Sub

    Private Shared Sub ValidateWorksheetSpec(spec As PsmDataImportSpec)
        If spec Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec)))
        If spec.Worksheet Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec.Worksheet)))
        If spec.VariableColumnsInfo Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec.VariableColumnsInfo)))
        If String.IsNullOrWhiteSpace(spec.TreatmentKey) Then CoreServices.Errors.LogAndThrow(New ArgumentException("Treatment variable is required."))
        If String.IsNullOrWhiteSpace(spec.OutcomeKey) Then CoreServices.Errors.LogAndThrow(New ArgumentException("Outcome variable is required."))
        If spec.ScoreMethod = PsmScoreMethod.Supplied AndAlso String.IsNullOrWhiteSpace(spec.SuppliedScoreKey) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Supplied propensity score variable is required when score method is Supplied."))
        End If
    End Sub

    Private Shared Sub ValidateRawMatrixSpec(spec As PsmDataRawMatrixSpec)
        If spec Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec)))
        If spec.ModelRawInput Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec.ModelRawInput)))
        If spec.ModelVariableNames Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec.ModelVariableNames)))
        If String.IsNullOrWhiteSpace(spec.TreatmentKey) Then CoreServices.Errors.LogAndThrow(New ArgumentException("Treatment variable is required."))
        If spec.OutcomeRawInput Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec.OutcomeRawInput)))
        If spec.OutcomeVariableNames Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(spec.OutcomeVariableNames)))
        If spec.ScoreMethod = PsmScoreMethod.Supplied AndAlso spec.SuppliedScoreRawInput Is Nothing Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Supplied propensity score data is required when score method is Supplied."))
        End If
    End Sub

    Private Shared Function BuildRequiredRawCovariateKeys(selectedCovariateKeys As IEnumerable(Of String),
                                                          effectItems As IEnumerable,
                                                          termSpecs As Dictionary(Of String, TermSpec)) As List(Of String)
        Dim keys As List(Of String) = RegressionDesignCore.GetRequiredRawVarKeys(effectItems, termSpecs)
        If keys Is Nothing Then keys = New List(Of String)()

        If keys.Count = 0 Then
            For Each k As String In NormalizeStringList(selectedCovariateKeys)
                If Not keys.Contains(k) Then keys.Add(k)
            Next
        End If

        If keys.Count = 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("At least one covariate or propensity-model effect is required."))
        Return keys
    End Function

    Private Shared Function BuildEffectiveEffectItems(effectItems As IEnumerable,
                                                      rawCovariateKeys As List(Of String)) As List(Of Object)
        Dim items As New List(Of Object)()
        If effectItems IsNot Nothing Then
            For Each obj As Object In effectItems
                If obj IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(CStr(obj)) Then items.Add(obj)
            Next
        End If

        If items.Count = 0 AndAlso rawCovariateKeys IsNot Nothing Then
            For Each key As String In rawCovariateKeys
                items.Add(key)
            Next
        End If

        Return items
    End Function

    Private Shared Function NormalizeStringList(values As IEnumerable(Of String)) As List(Of String)
        Dim out As New List(Of String)()
        If values Is Nothing Then Return out
        For Each v As String In values
            Dim s As String = If(v, String.Empty).Trim()
            If s <> String.Empty AndAlso Not out.Contains(s) Then out.Add(s)
        Next
        Return out
    End Function

    Private Shared Function ImportWorksheetDataObject(ref As String, charCols As Integer) As DataObj
        Dim d As New DataObj()
        ExcelDnaDataImporter.ImportInto(d, ref, CharCols:=charCols)
        Return d
    End Function

    Private Shared Function ImportRawDataObject(rawInput(,) As Object,
                                                variableNames() As String,
                                                firstSourceRow As Integer,
                                                sourceWorksheet As Worksheet,
                                                charCols As Integer) As DataObj
        Dim d As New DataObj()
        d.DataImportRawMatrix(rawInput,
                              variableNames,
                              firstSourceRow:=firstSourceRow,
                              sourceWorksheet:=sourceWorksheet,
                              CharCols:=charCols,
                              SkipRow:=0)
        Return d
    End Function

    Private Sub AlignChildDataObjects()
        If Me.RowIds Is Nothing OrElse Me.RowIds.Length = 0 Then
            Me.bZeroValid = True
            Return
        End If

        Dim originalRows As Integer = Me.RowIds.Length
        Dim commonRows As Integer() = CType(Me.RowIds.Clone(), Integer())

        IntersectRowIds(commonRows, OutcomeData)
        IntersectRowIds(commonRows, SuppliedScoreData)
        IntersectRowIds(commonRows, IdData)
        IntersectRowIds(commonRows, ExactGroupData)

        If commonRows.Length = 0 Then
            Me.bZeroValid = True
            Me.nRows = 0
            Me.RowIds = New Integer() {}
            Me.FinalData = Nothing
            Return
        End If

        Me.SubsetByRowIdValues(CommonItems(Me.RowIds, commonRows))
        SubsetChildDataObject(OutcomeData, commonRows)
        SubsetChildDataObject(SuppliedScoreData, commonRows)
        SubsetChildDataObject(IdData, commonRows)
        SubsetChildDataObject(ExactGroupData, commonRows)

        DroppedRowsDuringAlignment = Math.Max(0, originalRows - Me.RowIds.Length)
    End Sub

    Private Shared Sub IntersectRowIds(ByRef commonRows As Integer(), data As DataObj)
        If data Is Nothing Then Return
        If data.RowIds Is Nothing OrElse data.RowIds.Length = 0 Then
            commonRows = New Integer() {}
            Return
        End If
        commonRows = commonRows.Intersect(data.RowIds).ToArray()
    End Sub

    Private Shared Sub SubsetChildDataObject(data As DataObj, commonRows As Integer())
        If data Is Nothing Then Return
        data.SubsetByRowIdValues(CommonItems(data.RowIds, commonRows))
    End Sub

    Private Sub BuildPsmInput(treatmentKey As String,
                              omitCategoricalReference As Boolean,
                              scoreMethod As PsmScoreMethod,
                              Optional formulaText As String = Nothing,
                              Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                              Optional allowRelativeColumnLetters As Boolean = True,
                              Optional allowAbsoluteColumnLetters As Boolean = False,
                              Optional allowQuotedVariableNames As Boolean = True)
        If Me.bZeroValid Then Return
        If OutcomeData Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentException("Outcome data was not imported."))

        If Not String.IsNullOrWhiteSpace(formulaText) Then
            Dim designBuild As RegressionFormulaRegressionDataBuildResult = Nothing
            Dim designErr As String = Nothing
            If Not RegressionFormulaDesignService.TryBuildExpandedRegressionDataMatrixFromFormula(raw:=Me,
                                                                                                  yKey:=treatmentKey,
                                                                                                  result:=designBuild,
                                                                                                  errorMessage:=designErr,
                                                                                                  formulaText:=formulaText,
                                                                                                  absoluteColumnLetters:=absoluteColumnLetters,
                                                                                                  allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                                  allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                                  allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                                  omitCategoricalReference:=omitCategoricalReference) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Propensity-score formula could not be expanded. " & If(designErr, String.Empty)))
            End If
            ExpandedData = designBuild.RegressionDataMatrix
            ExpandedVarNames = designBuild.RegressionDataVarNames
        Else
            RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw:=Me,
                                                                   yKey:=treatmentKey,
                                                                   effectItems:=EffectiveEffectItems,
                                                                   termSpecs:=PropensityTermSpecs,
                                                                   omitCategoricalReference:=omitCategoricalReference,
                                                                   outData:=ExpandedData,
                                                                   outVarNames:=ExpandedVarNames)
        End If

        If ExpandedData Is Nothing OrElse ExpandedData.GetLength(1) < 2 Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("The propensity-score model matrix contains no covariate columns."))
        End If

        Dim n As Integer = ExpandedData.GetLength(0)
        Dim p As Integer = ExpandedData.GetLength(1) - 1
        Dim treatment(n - 1) As Double
        Dim outcome(n - 1) As Double
        Dim covariates(n - 1, p - 1) As Double
        Dim covariateNames(p - 1) As String

        For j As Integer = 0 To p - 1
            covariateNames(j) = ExpandedVarNames(j + 1)
        Next

        For i As Integer = 0 To n - 1
            treatment(i) = ExpandedData(i, 0)
            outcome(i) = CDbl(OutcomeData.FinalData(i, 0))
            For j As Integer = 0 To p - 1
                covariates(i, j) = ExpandedData(i, j + 1)
            Next
        Next

        Dim scores As Double() = Nothing
        If SuppliedScoreData IsNot Nothing Then
            ReDim scores(n - 1)
            For i As Integer = 0 To n - 1
                scores(i) = CDbl(SuppliedScoreData.FinalData(i, 0))
            Next
        ElseIf scoreMethod = PsmScoreMethod.Supplied Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Supplied propensity-score data was not imported."))
        End If

        Dim ids(n - 1) As String
        If IdData IsNot Nothing Then
            For i As Integer = 0 To n - 1
                ids(i) = CStr(IdData.FinalData(i, 0))
            Next
        Else
            For i As Integer = 0 To n - 1
                ids(i) = CStr(Me.RowIds(i))
            Next
        End If

        Dim exactLabels As String() = Nothing
        If ExactGroupData IsNot Nothing Then
            ReDim exactLabels(n - 1)
            For i As Integer = 0 To n - 1
                exactLabels(i) = BuildExactLabel(ExactGroupData.FinalData, i)
            Next
        End If

        Input = New PsmInputData With {
            .Ids = ids,
            .Treatment = treatment,
            .Outcome = outcome,
            .Covariates = covariates,
            .CovariateNames = covariateNames,
            .SuppliedPropensityScores = scores,
            .ExactGroupLabels = exactLabels
        }
    End Sub

    Private Shared Function BuildExactLabel(data(,) As Object, rowIndex As Integer) As String
        Dim parts As New List(Of String)()
        For j As Integer = 0 To data.GetLength(1) - 1
            parts.Add(If(data(rowIndex, j) Is Nothing, "", CStr(data(rowIndex, j))))
        Next
        Return String.Join(" | ", parts.ToArray())
    End Function
End Class
