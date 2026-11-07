"""Portable presentation extraction; run after frozen sources and baseline."""
from work import *
from freeze import method
from implement import HEADER
if __name__=='__main__':
    assert (ROOT/'.test-rationalization-04A36.tmp/entry-source.json').is_file()
    original=(HERE/'Legacy36.vb').read_text()
    bands=method(original,'Bands');start=bands.index('Dim categories(');end=bands.index('Return New Object()',start)
    body=bands[start:end].replace('As Object','As Double?').replace('MissingChartValue()','Nothing')
    core=HEADER+'Imports BESHStatNG.StatisticalProcessControl\n'+'''Public NotInheritable Class SpcZoneBands36
    Private ReadOnly _values As Double?()()
    Public ReadOnly Property ValidPointCount As Integer
    Public Sub New(points As SpcPointResult(), minimum As Double)
'''+body+'''ValidPointCount=validBandCount
_values=New Double?()(){baseValues,lowerOuter,lowerMiddle,lowerCenter,upperCenter,upperMiddle,upperOuter}
End Sub
Public ReadOnly Property Values As Double?()()
Get
Return _values.Select(Function(v) DirectCast(v.Clone(),Double?())).ToArray()
End Get
End Property
'''
    for name in ['HasValidZones','IsFinite']:core+=method(original,name).replace('Public Function','Private Shared Function')+'\n'
    core+='End Class\n'
    write('BESHStat.Core/src/Graphics/SpcZoneBands36.vb',core)
    write('BESHStat.Excel.Common/src/Results/SurvivalChartPresentation36.vb',HEADER+'''Imports BESHStatNG.survival
Namespace ExcelCommon
Public NotInheritable Class SurvivalChartPresentation36
    Public Const MaximumProbability As Double=1.0
    Public Const ProbabilityMajorUnit As Double=0.2
    Public Const ProbabilityTitle As String="Survival Probability"
    Public Const CoxTimeTitle As String="Time"
    Public Const CoxTitle As String="Cox - Survival plot"
    Public Const BaselineName As String="Baseline"
    Private Sub New()
    End Sub
    'Called at original name-assignment time, not while preparing earlier curves.
    Public Shared Function SeriesName(data As KaplanMeierPlotData, group As Integer, role As KaplanMeierSeriesRole36) As String
        If role=KaplanMeierSeriesRole36.Survival Then Return data.GroupLabels(group)
        If role=KaplanMeierSeriesRole36.Censor Then Return "Censored " + data.GroupLabels(group)
        Return $"{100.0 * (1.0 - data.Alpha):0.##}% CI " + data.GroupLabels(group)
    End Function
    Public Shared Function KmTimeTitle(unit As String) As String
        Return $"Time ({unit})"
    End Function
    Public Shared Function DeleteLegend(groups As Integer, requested As Boolean) As Boolean
        Return groups=1 OrElse Not requested
    End Function
End Class
End Namespace
''')
    write('BESHStat.Excel.Common/src/Analysis/SpcPanelPresentation36.vb',HEADER+'''Imports BESHStatNG.StatisticalProcessControl
Namespace ExcelCommon
Public Enum SpcSeriesRole36
    Statistic
    Center
    Control
    Zone
    Specification
    Target
    Excluded
    Signal
End Enum

Public NotInheritable Class SpcSeriesIntent36
    Private ReadOnly _values As Double?()
    Public ReadOnly Property Role As SpcSeriesRole36
    Public ReadOnly Property Name As String
    Public ReadOnly Property LastValueOrdinal As Integer
    Public ReadOnly Property ShowLimitLabel As Boolean
    Public ReadOnly Property HideLegend As Boolean
    Friend Sub New(role As SpcSeriesRole36, name As String, values As IEnumerable(Of Double?), showLabel As Boolean, hide As Boolean)
        Me.Role=role : Me.Name=name : _values=values.ToArray()
        LastValueOrdinal=LastFiniteOrdinal(_values) : ShowLimitLabel=showLabel : HideLegend=hide
    End Sub
    Public ReadOnly Property Values As Double?()
        Get
            Return DirectCast(_values.Clone(),Double?())
        End Get
    End Property
    Public Shared Function LastFiniteOrdinal(values As IReadOnlyList(Of Double?)) As Integer
        For i=values.Count-1 To 0 Step -1
            If values(i).HasValue AndAlso Not Double.IsNaN(values(i).Value) AndAlso Not Double.IsInfinity(values(i).Value) Then Return i
        Next
        Return -1
    End Function
End Class

'''+'\'\'\' <summary>Ordered rendering intent for an accepted aligned panel. Never fits or recomputes limits/signals.</summary>\n'+'''Public NotInheritable Class SpcPanelPresentation36
    Public ReadOnly Property Series As IReadOnlyList(Of SpcSeriesIntent36)
    Public ReadOnly Property IncludesBands As Boolean
    Public Sub New(panel As SpcPanelSeriesProjection, specifications As SpcSpecificationLimits, zoneMode As Integer, showSpecs As Boolean, showTarget As Boolean, showExcluded As Boolean, showLabels As Boolean, showZoneLegend As Boolean)
        Dim result As New List(Of SpcSeriesIntent36)
        IncludesBands=zoneMode=2 OrElse zoneMode=3
        Dim add As Action(Of SpcSeriesRole36,String,IEnumerable(Of Double?),Boolean,Boolean)=Sub(role,name,values,label,hide)
            Dim intent As New SpcSeriesIntent36(role,name,values,label,hide)
            If role=SpcSeriesRole36.Statistic OrElse intent.LastValueOrdinal>=0 Then result.Add(intent)
        End Sub
        Dim c=panel.Channels
        add(SpcSeriesRole36.Statistic,panel.Panel.DisplayName,c(CInt(SpcChartChannel.Statistic)),False,False)
        add(SpcSeriesRole36.Center,"CL",c(CInt(SpcChartChannel.CenterLine)),showLabels,False)
        add(SpcSeriesRole36.Control,"UCL",c(CInt(SpcChartChannel.UpperControlLimit)),showLabels,False)
        add(SpcSeriesRole36.Control,"LCL",c(CInt(SpcChartChannel.LowerControlLimit)),showLabels,False)
        If zoneMode=1 OrElse zoneMode=3 Then
            add(SpcSeriesRole36.Zone,"-2 sigma",c(CInt(SpcChartChannel.LowerTwoSigma)),False,Not showZoneLegend)
            add(SpcSeriesRole36.Zone,"-1 sigma",c(CInt(SpcChartChannel.LowerOneSigma)),False,Not showZoneLegend)
            add(SpcSeriesRole36.Zone,"+1 sigma",c(CInt(SpcChartChannel.UpperOneSigma)),False,Not showZoneLegend)
            add(SpcSeriesRole36.Zone,"+2 sigma",c(CInt(SpcChartChannel.UpperTwoSigma)),False,Not showZoneLegend)
        End If
        If Chart34.SpcChartPreparation34.PanelSupportsSpecificationLimits(panel.Panel.PanelType) Then
            Dim count=c(0).Count
            Dim constant As Func(Of Double,Double?())=Function(v) SpcAxisBoundsCore.SpcConstantReference(count,v).Select(Function(x) CType(x,Double?)).ToArray()
            If showSpecs Then
                If specifications.LowerSpecificationLimit.HasValue Then add(SpcSeriesRole36.Specification,"LSL",constant(specifications.LowerSpecificationLimit.Value),showLabels,False)
                If specifications.UpperSpecificationLimit.HasValue Then add(SpcSeriesRole36.Specification,"USL",constant(specifications.UpperSpecificationLimit.Value),showLabels,False)
            End If
            If showTarget AndAlso specifications.Target.HasValue Then add(SpcSeriesRole36.Target,"Target",constant(specifications.Target.Value),showLabels,False)
        End If
        If showExcluded Then add(SpcSeriesRole36.Excluded,"Excluded",c(CInt(SpcChartChannel.Excluded)),False,False)
        add(SpcSeriesRole36.Signal,"Signal",c(CInt(SpcChartChannel.Signal)),False,False)
        Series=result.AsReadOnly()
    End Sub
End Class
End Namespace
''')
    write('BESHStat.Excel.Common/src/Results/CoxResidualPlotAvailability36.vb',HEADER+'''Namespace ExcelCommon
'''+'\'\'\' <summary>Existing residual block metadata, with no inferred join from dictionary iteration to input time. Native residual control remains disabled.</summary>\n'+'''Public NotInheritable Class CoxResidualBlock36
    Private ReadOnly _raw As Object(,)
    Public ReadOnly Property BlockOrdinal As Integer
    Public ReadOnly Property Series As IReadOnlyList(Of CoxResidualSeries36)
    Friend Sub New(ordinal As Integer, raw As Object(,), rows As Integer(), time As Double())
        BlockOrdinal=ordinal : _raw=DirectCast(raw.Clone(),Object(,))
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
            Return DirectCast(_raw.Clone(),Object(,))
        End Get
    End Property
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
''')
    # Lazy access leaves existing report construction and table timing untouched.
    p='BESHStat.Excel.Common/src/Results/SurvivalReportPolicy.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    target='        Public Function LegacyBlocks() As List(Of Object(,))'
    s=s.replace(target,'''        Public Function ResidualPlotAvailability() As CoxResidualPlotAvailability36
            Return New CoxResidualPlotAvailability36(Me)
        End Function
'''+target)
    write(p,s)
    print('Applied portable captions, SPC intent/bands and bounded residual availability; no native activation')
