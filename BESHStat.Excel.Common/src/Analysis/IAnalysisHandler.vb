Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon

    ''' <summary>
    ''' Non-generic, compile-time registered boundary for a single supported input shape.
    ''' No reflection, dynamic invocation, Excel objects, or host-specific dependencies.
    ''' </summary>
    Public Interface IAnalysisHandler
        ReadOnly Property AnalysisId As String
        ReadOnly Property InputType As Type
        ReadOnly Property OptionsType As Type
        Function Execute(input As SpreadsheetAnalysisInput,
                         options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult
    End Interface

    ''' <summary>
    ''' Strongly typed operation exposed by each registered input/option/detail combination.
    ''' Multiple handlers may share one analysis ID when the analysis accepts multiple input shapes.
    ''' </summary>
    Public Interface IAnalysisHandler(Of TInput As SpreadsheetAnalysisInput,
                                         TOptions As SpreadsheetAnalysisOptions,
                                         TDetail As Class)
        Inherits IAnalysisHandler

        Function ExecuteTyped(input As TInput, options As TOptions) As TDetail
    End Interface

    ''' <summary>
    ''' Explicit typed adapter from a Core-backed application service to the current
    ''' SpreadsheetAnalysisResult envelope. The typed detail remains the actual service result.
    ''' </summary>
    Public NotInheritable Class TypedAnalysisHandler(Of TInput As SpreadsheetAnalysisInput,
                                                     TOptions As SpreadsheetAnalysisOptions,
                                                     TDetail As Class)
        Implements IAnalysisHandler(Of TInput, TOptions, TDetail)

        Private ReadOnly _execute As Func(Of TInput, TOptions, TDetail)
        Private ReadOnly _wrap As Func(Of TDetail, SpreadsheetAnalysisResult)

        Public Sub New(analysisId As String,
                       execute As Func(Of TInput, TOptions, TDetail),
                       wrap As Func(Of TDetail, SpreadsheetAnalysisResult))
            If String.IsNullOrWhiteSpace(analysisId) Then Throw New ArgumentException("Analysis ID is required.", NameOf(analysisId))
            If execute Is Nothing Then Throw New ArgumentNullException(NameOf(execute))
            If wrap Is Nothing Then Throw New ArgumentNullException(NameOf(wrap))
            Me.AnalysisId = analysisId
            _execute = execute
            _wrap = wrap
        End Sub

        Public ReadOnly Property AnalysisId As String Implements IAnalysisHandler.AnalysisId

        Public ReadOnly Property InputType As Type Implements IAnalysisHandler.InputType
            Get
                Return GetType(TInput)
            End Get
        End Property

        Public ReadOnly Property OptionsType As Type Implements IAnalysisHandler.OptionsType
            Get
                Return GetType(TOptions)
            End Get
        End Property

        Public Function ExecuteTyped(input As TInput,
                                     options As TOptions) As TDetail Implements IAnalysisHandler(Of TInput, TOptions, TDetail).ExecuteTyped
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            Return _execute(input, options)
        End Function

        Public Function Execute(input As SpreadsheetAnalysisInput,
                                options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult Implements IAnalysisHandler.Execute
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If Not GetType(TInput).IsInstanceOfType(input) Then
                Throw New ArgumentException("Expected input type " & GetType(TInput).Name & ".", NameOf(input))
            End If
            If options IsNot Nothing AndAlso Not GetType(TOptions).IsInstanceOfType(options) Then
                Throw New ArgumentException("Expected options type " & GetType(TOptions).Name & ".", NameOf(options))
            End If

            Dim typedOptions As TOptions = Nothing
            If options IsNot Nothing Then typedOptions = DirectCast(options, TOptions)
            Dim detail As TDetail = ExecuteTyped(DirectCast(input, TInput), typedOptions)
            If detail Is Nothing Then Throw New InvalidOperationException("Handler returned no detail for " & AnalysisId & ".")
            Dim result As SpreadsheetAnalysisResult = _wrap(detail)
            If result Is Nothing Then Throw New InvalidOperationException("Handler returned no result for " & AnalysisId & ".")
            Return result
        End Function
    End Class

End Namespace
