Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    ''' <summary>Existing Data-sheet layout, never a second matrix store. All positions
    ''' are one-based output positions; source identities are supplied by the host.</summary>
    Public NotInheritable Class MultivariateModelDataSection
        Friend Sub New(input As MultivariateInputIntent, factor As Boolean)
            Dim labels = input.RowLabels
            IncludesRowLabel = Not factor AndAlso labels IsNot Nothing AndAlso labels.Length = input.RowCount
            Shape = New MultivariateArrayShape(input.RowCount + 1, input.ColumnCount + If(factor OrElse IncludesRowLabel, 1, 0))
            Dim names As New List(Of String)
            If factor Then names.Add("OriginalRow")
            If IncludesRowLabel Then names.Add("Row label")
            If input.VariableNames IsNot Nothing Then names.AddRange(input.VariableNames)
            Headers = names.AsReadOnly()
        End Sub
        Public ReadOnly Property SheetName As String = "Data"
        Public ReadOnly Property HeaderRow As Integer = 1
        Public ReadOnly Property BodyRow As Integer = 2
        Public ReadOnly Property FirstColumn As Integer = 1
        Public ReadOnly Property IncludesRowLabel As Boolean
        Public ReadOnly Property Headers As IReadOnlyList(Of String)
        Public ReadOnly Property Shape As MultivariateArrayShape
    End Class

    Public NotInheritable Class MultivariateFactorRequest
        Private ReadOnly _options As MultivariateFactorOptions
        Public Sub New(input As MultivariateInputIntent, options As MultivariateFactorOptions)
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            Me.Input = input : _options = options.Copy()
            DataSection = New MultivariateModelDataSection(input, True)
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property DataSection As MultivariateModelDataSection
        Public ReadOnly Property ResultsSheetName As String = "Factor analysis results"
        Public ReadOnly Property Options As MultivariateFactorOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public Sub Configure(model As FactorAnalysis)
            _options.Configure(model)
        End Sub
        ''' <summary>Borrow original cells synchronously. No optional FA label column
        ''' is introduced; the established ClusterInputTable owns cells and column order.</summary>
        Public Function DataTable(original(,) As Object) As Object(,)
            Return MultivariateWorkflowPolicy.ClusterInputTable(original, Input.VariableNames, Input.SourceRowIds, Nothing)
        End Function
    End Class

    ''' <summary>Original ordered, unnormalised prior provenance after Core priorInputs.
    ''' Immutable scalar labels are detached; arbitrary mutable reference labels are not
    ''' retained. The Windows parser supplies strings. Core alone normalises/validates.</summary>
    Public NotInheritable Class MultivariateUserPriorEntry
        Private ReadOnly _value As Object
        Friend Sub New(index As Integer, label As Object, weight As Double)
            Me.Index = index : Me.Weight = weight
            SourceType = If(label Is Nothing, Nothing, label.GetType().FullName)
            SourceValueAvailable = label Is Nothing OrElse TypeOf label Is String OrElse label.GetType().IsPrimitive OrElse label.GetType().IsEnum OrElse TypeOf label Is Decimal OrElse TypeOf label Is DateTime
            _value = If(SourceValueAvailable, label, Nothing)
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Weight As Double
        Public ReadOnly Property SourceType As String
        Public ReadOnly Property SourceValueAvailable As Boolean
        Public ReadOnly Property SourceValue As Object
            Get
                Return _value
            End Get
        End Property
    End Class
    Public NotInheritable Class MultivariateUserPriorIntent
        Friend Sub New(labels() As Object, weights() As Double)
            Entries = labels.Select(Function(v, i) New MultivariateUserPriorEntry(i, v, weights(i))).ToList().AsReadOnly()
        End Sub
        Public ReadOnly Property Entries As IReadOnlyList(Of MultivariateUserPriorEntry)
    End Class

    ''' <summary>Model phase only. Host prior parsing and subsequent validation controls
    ''' retain their original separate failure boundaries. Never fits, seeds or partitions.</summary>
    Public NotInheritable Class MultivariateDiscriminantRequest
        Private ReadOnly _options As MultivariateDiscriminantOptions
        Public Sub New(input As MultivariateInputIntent, options As MultivariateDiscriminantOptions, Optional firstRowSkipped As Boolean = False)
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            Me.Input = input : _options = options.Copy() : Me.FirstRowSkipped = firstRowSkipped
            DataSection = New MultivariateModelDataSection(input, False)
            Predictors = input.Columns.Skip(1).Select(Function(c) New MultivariateAxisItem(c.Index - 1, c.Name, sourcePosition:=c.Index, sourceKey:=c.SourceKey)).ToList().AsReadOnly()
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property FirstRowSkipped As Boolean
        Public ReadOnly Property GroupingSourceColumn As Integer = 0
        Public ReadOnly Property FirstPredictorSourceColumn As Integer = 1
        Public ReadOnly Property Predictors As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property DataSection As MultivariateModelDataSection
        Public ReadOnly Property ResultsSheetName As String = "Discriminant results"
        Public ReadOnly Property Options As MultivariateDiscriminantOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public Sub ConfigureModel(model As DiscriminantAnalysis)
            _options.ConfigureModel(model)
        End Sub
        Public Function ApplyUserPriors(model As DiscriminantAnalysis, labels() As Object, weights() As Double) As MultivariateUserPriorIntent
            ' Core receives original host values before metadata construction, preserving
            ' null/length/normalisation exceptions and their timing.
            model.priorInputs(labels, weights)
            Return New MultivariateUserPriorIntent(labels, weights)
        End Function
        Public Function DataTable(original(,) As Object, groups() As Object) As Object(,)
            Return MultivariateWorkflowPolicy.DiscriminantInputTable(original, Input.VariableNames, groups, Input.RowLabels)
        End Function
    End Class

    Public NotInheritable Class MultivariateDiscriminantValidationRequest
        Private ReadOnly _options As MultivariateDiscriminantOptions
        Public Sub New(options As MultivariateDiscriminantOptions)
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            _options = options.Copy()
        End Sub
        Public ReadOnly Property Options As MultivariateDiscriminantOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property HasExplicitSeed As Boolean
            Get
                Return _options.Seed <> Integer.MinValue
            End Get
        End Property
        ''' <summary>The current public Core API does not expose its resolved validation
        ''' seed. Nothing means unavailable, never a fabricated effective seed.</summary>
        Public ReadOnly Property EffectiveSeed As Integer?
            Get
                Return Nothing
            End Get
        End Property
        Public Sub Configure(model As DiscriminantAnalysis)
            _options.ConfigureValidation(model)
        End Sub
    End Class
End Namespace
