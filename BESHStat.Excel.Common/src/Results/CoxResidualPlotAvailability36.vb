Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Namespace ExcelCommon
''' <summary>Existing residual block metadata, with no inferred join from dictionary iteration to input time. Native residual control remains disabled.</summary>
Public NotInheritable Class CoxResidualBlock36
    Private ReadOnly _raw As Object(,)
    Public ReadOnly Property BlockOrdinal As Integer
    Public ReadOnly Property Series As IReadOnlyList(Of CoxResidualSeries36)
    Friend Sub New(ordinal As Integer, raw As Object(,), rows As Integer(), time As Double())
        BlockOrdinal=ordinal : _raw=DirectCast(CopyCell(raw),Object(,))
        Dim seriesValues As New List(Of CoxResidualSeries36)
        'Only the Score block preserves explicit original record.Index keys.
        'Schoenfeld overwrites its key header and scalar blocks omit keys.
        If raw.GetLength(0)>=2 AndAlso raw.GetLength(1)>1 AndAlso
           Object.Equals(raw(0,0),"Score Residuals") AndAlso Object.Equals(raw(1,0),"Row ID") AndAlso
           rows.Length=time.Length AndAlso rows.Distinct().Count()=rows.Length Then
            Dim positions As New Dictionary(Of Integer,Integer)
            For i=0 To rows.Length-1 : positions.Add(rows(i),i) : Next
            Dim count=raw.GetLength(0)-2, keys(count-1), ordinals(count-1) As Integer, times(count-1) As Double
            Dim valid=True
            For i=0 To count-1
                Dim key=raw(i+2,0)
                If Not TypeOf key Is Integer OrElse Not positions.ContainsKey(CInt(key)) Then
                    valid=False : Exit For
                End If
                keys(i)=CInt(key) : ordinals(i)=positions(keys(i)) : times(i)=time(ordinals(i))
            Next
            If valid Then
                For c=1 To raw.GetLength(1)-1
                    Dim values(count-1) As Double
                    Dim numeric=True
                    For i=0 To count-1
                        If Not TypeOf raw(i+2,c) Is Double Then
                            numeric=False : Exit For
                        End If
                        values(i)=CDbl(raw(i+2,c))
                    Next
                    If numeric Then seriesValues.Add(New CoxResidualSeries36(ordinal,c,TryCast(raw(1,c),String),keys,ordinals,times,values))
                Next
            End If
        End If
        Series=seriesValues.AsReadOnly()
    End Sub
    Public ReadOnly Property PlotAvailability As String
        Get
            Return If(Series.Count>0,"AVAILABLE_DATA_NATIVE_CONTROL_DISABLED","NOT_SUPPORTED_BY_EXISTING_SOURCE")
        End Get
    End Property
    Public ReadOnly Property Reason As String
        Get
            Return If(Series.Count>0,"Score block uses explicit original record.Index keys joined to existing input RowIds; source order retained. Native control remains disabled.","Legacy block has no proven common time/observation join; no positional join or worksheet rows inferred. Native control remains disabled.")
        End Get
    End Property
    Public ReadOnly Property LegacyValues As Object(,)
        Get
            Return DirectCast(CopyCell(_raw),Object(,))
        End Get
    End Property
    'Existing numerical blocks contain scalar cells. Preserve detached nested
    'arrays too when an unsupported caller-supplied block carries metadata.
    Private Shared Function CopyCell(value As Object) As Object
        Dim source=TryCast(value,Array)
        If source Is Nothing Then Return value
        Dim copy=DirectCast(source.Clone(),Array), indices(source.Rank-1) As Integer
        CopyDimension(source,copy,indices,0)
        Return copy
    End Function
    Private Shared Sub CopyDimension(source As Array, copy As Array, indices As Integer(), dimension As Integer)
        For i=source.GetLowerBound(dimension) To source.GetUpperBound(dimension)
            indices(dimension)=i
            If dimension+1=source.Rank Then
                copy.SetValue(CopyCell(source.GetValue(indices)),indices)
            Else
                CopyDimension(source,copy,indices,dimension+1)
            End If
        Next
    End Sub
    'Two original header rows are excluded; nonnumeric/blank cells remain absent.
    Public ReadOnly Property NumericBody As Double?(,)
        Get
            Dim values(Math.Max(0,_raw.GetLength(0)-2)-1,_raw.GetLength(1)-1) As Double?
            For r=0 To values.GetLength(0)-1
                For c=0 To values.GetLength(1)-1
                    Dim v=_raw(r+2,c)
                    If TypeOf v Is Double Then values(r,c)=CDbl(v)
                    If TypeOf v Is Integer Then values(r,c)=CDbl(CInt(v))
                Next
            Next
            Return values
        End Get
    End Property
End Class
Public NotInheritable Class CoxResidualSeries36
    Private ReadOnly _keys As Integer(), _ordinals As Integer(), _time As Double(), _values As Double()
    Public ReadOnly Property BlockOrdinal As Integer
    Public ReadOnly Property ValueColumnIndex As Integer
    Public ReadOnly Property Name As String
    Friend Sub New(block As Integer, column As Integer, name As String, keys As Integer(), ordinals As Integer(), time As Double(), values As Double())
        BlockOrdinal=block : ValueColumnIndex=column : Me.Name=name
        _keys=DirectCast(keys.Clone(),Integer()) : _ordinals=DirectCast(ordinals.Clone(),Integer())
        _time=DirectCast(time.Clone(),Double()) : _values=DirectCast(values.Clone(),Double())
    End Sub
    Public ReadOnly Property RowIds As Integer()
        Get
            Return DirectCast(_keys.Clone(),Integer())
        End Get
    End Property
    Public ReadOnly Property ObservationOrdinals As Integer()
        Get
            Return DirectCast(_ordinals.Clone(),Integer())
        End Get
    End Property
    Public ReadOnly Property XValues As Double()
        Get
            Return DirectCast(_time.Clone(),Double())
        End Get
    End Property
    Public ReadOnly Property YValues As Double()
        Get
            Return DirectCast(_values.Clone(),Double())
        End Get
    End Property
End Class
Public NotInheritable Class CoxResidualPlotAvailability36
    Private ReadOnly _rows As Integer(), _time As Double(), _status As Integer(), _predictors As Double(,), _names As String()
    Public ReadOnly Property Blocks As IReadOnlyList(Of CoxResidualBlock36)
    Public Sub New(data As CoxResidualData)
        If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
        _rows=data.RowIds : _time=data.Time : _status=data.Status : _predictors=data.Predictors : _names=data.Names
        Blocks=data.LegacyBlocks().Select(Function(b,i) New CoxResidualBlock36(i,b,_rows,_time)).ToList().AsReadOnly()
    End Sub
    Public ReadOnly Property NativeControlAvailable As Boolean
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property RowIds As Integer()
        Get
            Return DirectCast(_rows.Clone(),Integer())
        End Get
    End Property
    Public ReadOnly Property Time As Double()
        Get
            Return DirectCast(_time.Clone(),Double())
        End Get
    End Property
    Public ReadOnly Property Status As Integer()
        Get
            Return DirectCast(_status.Clone(),Integer())
        End Get
    End Property
    Public ReadOnly Property Predictors As Double(,)
        Get
            Return DirectCast(_predictors.Clone(),Double(,))
        End Get
    End Property
    Public ReadOnly Property Names As String()
        Get
            Return DirectCast(_names.Clone(),String())
        End Get
    End Property
End Class
End Namespace
