Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    Public Enum UdfReportFault38
        None = 0
        NotAvailable = 1
        InvalidValue = 2
        InvalidNumber = 3
    End Enum

    Public Enum UdfReportPreparation38
        Raw = 0
        NormalizeBlanks = 1
        PvalueRows = 2
    End Enum

    ''' <summary>
    ''' A zero-based reference to a host-owned scalar, never a worksheet row or
    ''' an Excel object. Native cell identity remains at the Windows boundary.
    ''' </summary>
    Public NotInheritable Class UdfSourceCellReference38
        Public Sub New(sourceOrdinal As Integer)
            Me.SourceOrdinal = sourceOrdinal
        End Sub
        Public ReadOnly Property SourceOrdinal As Integer
    End Class

    ''' <summary>
    ''' Detached legacy report data using the accepted Core output model. Values
    ''' are report cells, with exact CLR scalar kinds and IEEE payloads retained.
    ''' Excel errors are represented by existing typed OutputCell error intent.
    ''' A fresh deep array copy is returned on every access. No native cell value
    ''' or model cache is retained; source references are ordinal intent only.
    ''' </summary>
    Public NotInheritable Class UdfReportProjection38
        Private ReadOnly _model As Global.BESHStatNG.ResultTableOutputModel

        Public Sub New(model As Global.BESHStatNG.ResultTableOutputModel,
                       Optional preparation As UdfReportPreparation38 = UdfReportPreparation38.Raw)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            _model = CopyModel(model)
            Me.Preparation = preparation
            Fault = UdfReportFault38.None
        End Sub

        Private Sub New(fault As UdfReportFault38)
            Me.Fault = fault
        End Sub

        Public ReadOnly Property Fault As UdfReportFault38
        Public ReadOnly Property Preparation As UdfReportPreparation38

        Public Shared Function FromFault(fault As UdfReportFault38) As UdfReportProjection38
            Return New UdfReportProjection38(fault)
        End Function

        Public Function ToOutputModel() As Global.BESHStatNG.ResultTableOutputModel
            If _model Is Nothing Then Return Nothing
            Return CopyModel(_model)
        End Function

        Public Shared Function SafeNumber(value As Double) As OutputCell
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return OutputCell.SpreadsheetError("#NUM!")
            Return OutputCell.NumberCell(value)
        End Function

        Private Shared Function CopyModel(model As Global.BESHStatNG.ResultTableOutputModel) As Global.BESHStatNG.ResultTableOutputModel
            Dim values As Object(,) = Nothing
            If model.Values IsNot Nothing Then values = DirectCast(CopyArray(model.Values), Object(,))
            Return New Global.BESHStatNG.ResultTableOutputModel(values, model.HeaderTopRows, model.HeaderLeftColumns,
                model.FooterRows, model.PvalueColumns, model.TitleRows, model.IsResultTable, model.PvalueCells)
        End Function

        Private Shared Function CopyArray(values As Array) As Array
            Dim copy As Array = DirectCast(values.Clone(), Array)
            Dim indices(values.Rank - 1) As Integer
            For dimension As Integer = 0 To indices.Length - 1
                indices(dimension) = values.GetLowerBound(dimension)
            Next
            If values.Length = 0 Then Return copy
            Do
                Dim value As Object = values.GetValue(indices)
                If TypeOf value Is Array Then
                    copy.SetValue(CopyArray(DirectCast(value, Array)), indices)
                Else
                    ValidateScalar(value)
                End If
                Dim dimension As Integer = indices.Length - 1
                Do While dimension >= 0
                    indices(dimension) += 1
                    If indices(dimension) <= values.GetUpperBound(dimension) Then Exit Do
                    indices(dimension) = values.GetLowerBound(dimension)
                    dimension -= 1
                Loop
                If dimension < 0 Then Exit Do
            Loop
            Return copy
        End Function

        Private Shared Sub ValidateScalar(value As Object)
            If value Is Nothing OrElse TypeOf value Is DBNull OrElse TypeOf value Is UdfSourceCellReference38 Then Return
            If TypeOf value Is OutputCell Then
                Dim kind As OutputCellKind = DirectCast(value, OutputCell).Kind
                If kind = OutputCellKind.Error OrElse kind = OutputCellKind.Number Then Return
                Throw New ArgumentException("Only typed numeric/error intent is used by legacy report projections.")
            End If
            Dim valueType As Type = value.GetType()
            If Not valueType.IsEnum Then
                Select Case Type.GetTypeCode(valueType)
                    Case TypeCode.String, TypeCode.Boolean, TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                         TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double,
                         TypeCode.Decimal, TypeCode.DateTime, TypeCode.Char
                        Return
                End Select
            End If
            Throw New ArgumentException("A legacy portable report cannot contain a native or mutable scalar payload.")
        End Sub
    End Class
End Namespace
