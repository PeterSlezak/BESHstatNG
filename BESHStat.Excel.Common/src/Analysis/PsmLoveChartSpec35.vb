Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon
Public NotInheritable Class PsmLoveChartSpec35
Private ReadOnly beforeData As Double(),matchingData As Double(),weightingData As Double(),yData As Double(),labelData As String(),sourceData As Integer()
Private ReadOnly namesData As String(),channelsData As Integer()
Public Sub New(projection As PsmLovePlotProjection)
yData=projection.YValues:beforeData=projection.Before:matchingData=projection.AfterMatching:weightingData=projection.AfterWeighting:labelData=projection.Labels:sourceData=projection.SourceOrdinals
Threshold=projection.Threshold:MaximumX=projection.MaximumX:MaximumY=projection.Count+0.5R
Dim names As New List(Of String),channels As New List(Of Integer)
If PsmLoveSeriesPolicy.HasAnyFinite(beforeData) Then
names.Add("Before"):channels.Add(0)
End If
If PsmLoveSeriesPolicy.HasAnyFinite(matchingData) Then
names.Add("After matching"):channels.Add(1)
End If
If PsmLoveSeriesPolicy.HasAnyFinite(weightingData) Then
names.Add("After weighting"):channels.Add(2)
End If
namesData=names.ToArray():channelsData=channels.ToArray():LabelSeriesOrdinal=names.Count-1
End Sub
Public ReadOnly Property Threshold As Double
Public ReadOnly Property MaximumX As Double
Public ReadOnly Property MaximumY As Double
Public ReadOnly Property MinimumX As Double=0.0R
Public ReadOnly Property MinimumY As Double=0.5R
Public ReadOnly Property ReverseY As Boolean=True
Public ReadOnly Property Title As String="Love plot: absolute standardized mean differences"
Public ReadOnly Property XAxisTitle As String="Absolute standardized mean difference"
Public Const ThresholdSeriesName As String="Threshold"
Public ReadOnly Property LabelSeriesOrdinal As Integer
Public Function IncludesChannel(channel As Integer) As Boolean
Return channelsData.Contains(channel)
End Function
Public Function NameForChannel(channel As Integer) As String
Select Case channel
Case 0:Return "Before"
Case 1:Return "After matching"
Case 2:Return "After weighting"
Case Else:Throw New ArgumentOutOfRangeException(NameOf(channel))
End Select
End Function
Public ReadOnly Property Names As String()
Get
Return DirectCast(namesData.Clone(),String())
End Get
End Property
Public ReadOnly Property Channels As Integer()
Get
Return DirectCast(channelsData.Clone(),Integer())
End Get
End Property
Public ReadOnly Property SourceOrdinals As Integer()
Get
Return DirectCast(sourceData.Clone(),Integer())
End Get
End Property
Public ReadOnly Property Labels As String()
Get
Return DirectCast(labelData.Clone(),String())
End Get
End Property
Public ReadOnly Property Before As Double()
Get
Return DirectCast(beforeData.Clone(),Double())
End Get
End Property
Public ReadOnly Property AfterMatching As Double()
Get
Return DirectCast(matchingData.Clone(),Double())
End Get
End Property
Public ReadOnly Property AfterWeighting As Double()
Get
Return DirectCast(weightingData.Clone(),Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(yData.Clone(),Double())
End Get
End Property
End Class
End Namespace
