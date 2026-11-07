Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon.Chart34
Public Module SpecialtyLegendPolicy34
Public Function SankeyNodeOrder(nodes As SankeyNode()) As SankeyNode()
Return nodes.OrderBy(Function(n) n.FirstAppearanceIndex).ToArray()
End Function
Public Function SankeyLegendOrder(firstIndices As Integer()) As Integer()
Return Enumerable.Range(0,firstIndices.Length).OrderBy(Function(i) firstIndices(i)).ToArray()
End Function
Public Function CdfLegendChoice(showLegend As Boolean, allCurves As Boolean, hasEmpirical As Boolean, hasFitted As Boolean) As CdfLegendChoice34
If Not showLegend Then Return New CdfLegendChoice34(False,False,False)
If allCurves Then Return New CdfLegendChoice34(hasEmpirical,hasFitted,False)
If hasFitted Then Return New CdfLegendChoice34(False,True,True)
If hasEmpirical Then Return New CdfLegendChoice34(True,False,True)
Return New CdfLegendChoice34(False,False,False)
End Function
Public Function LadderLegendChoice(showLegend As Boolean, mode As Integer, observationIndex As Integer) As LadderLegendChoice34
If Not showLegend Then Return New LadderLegendChoice34(False,False)
Select Case mode
Case 1
Return New LadderLegendChoice34(True,False)
Case 2
Return New LadderLegendChoice34(False,True)
Case Else
Return New LadderLegendChoice34(observationIndex = 0,False)
End Select
End Function
End Module
Public NotInheritable Class CdfLegendChoice34
    Public ReadOnly Property KeepEmpirical As Boolean
    Public ReadOnly Property KeepFitted As Boolean
    Public ReadOnly Property RenameRepresentative As Boolean
    Public Sub New(inputKeepEmpirical As Boolean, inputKeepFitted As Boolean, inputRenameRepresentative As Boolean)
        Me.KeepEmpirical = inputKeepEmpirical
        Me.KeepFitted = inputKeepFitted
        Me.RenameRepresentative = inputRenameRepresentative
    End Sub
End Class
Public NotInheritable Class LadderLegendChoice34
    Public ReadOnly Property Keep As Boolean
    Public ReadOnly Property RecordGroup As Boolean
    Public Sub New(inputKeep As Boolean, inputRecordGroup As Boolean)
        Me.Keep = inputKeep
        Me.RecordGroup = inputRecordGroup
    End Sub
End Class
End Namespace
