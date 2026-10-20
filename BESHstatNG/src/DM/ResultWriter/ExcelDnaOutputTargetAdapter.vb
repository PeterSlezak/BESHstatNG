Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Runtime.InteropServices
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Friend NotInheritable Class ExcelDnaChartSafetySnapshot
    Friend Sub New(existingShapeCount As Integer, observationIdentity As String)
        If existingShapeCount < 0 OrElse existingShapeCount > 1000 OrElse String.IsNullOrEmpty(observationIdentity) Then Throw New ArgumentException("Invalid shape inspection.")
        Me.ExistingShapeCount = existingShapeCount : Me.ObservationIdentity = observationIdentity
    End Sub
    Friend ReadOnly Property ExistingShapeCount As Integer
    Friend ReadOnly Property ObservationIdentity As String
End Class

Friend NotInheritable Class ExcelDnaOccupancySnapshot
    Friend Sub New(values As Object, formulas As Object)
        Me.Values = values : Me.Formulas = formulas
    End Sub
    Friend ReadOnly Property Values As Object
    Friend ReadOnly Property Formulas As Object
End Class

''' <summary>Read-only Windows observations and a pure pre-creation validation seam.</summary>
Friend NotInheritable Class ExcelDnaOutputTargetAdapter
    Private Shared ReadOnly IdentitySalt As Byte() = Guid.NewGuid().ToByteArray()
    Private Sub New()
    End Sub

    ' Worksheet.CodeName can legitimately be empty before Excel initializes its VBA
    ' project. Names are not physical identity. Hash the apartment-local COM identity
    ' with a process nonce; never expose native pointer bytes or initialize VBA to probe it.
    Friend Shared Function TargetIdentity(target As Object) As String
        If target Is Nothing OrElse Not Marshal.IsComObject(target) Then Throw New ArgumentException("A resolved native target is required.")
        Dim pointer As IntPtr = Marshal.GetIUnknownForObject(target)
        Try
            Using hash As New HMACSHA256(IdentitySalt)
                Return "native-" & BitConverter.ToString(hash.ComputeHash(BitConverter.GetBytes(pointer.ToInt64()))).Replace("-", String.Empty)
            End Using
        Finally
            Marshal.Release(pointer)
        End Try
    End Function

    ' Bounded bulk Value2 + Formula reads distinguish blank constants from blank-result
    ' formulas. Fingerprint stays private; result diagnostics never contain cell contents.
    Friend Shared Function InspectWrite(intent As WriteIntent, workbook As Workbook, worksheet As Worksheet) As WriteInspection
        If workbook Is Nothing OrElse worksheet Is Nothing OrElse TargetIdentity(workbook) <> intent.WorkbookIdentity OrElse
            TargetIdentity(worksheet) <> intent.WorksheetIdentity OrElse worksheet.Name <> intent.WorksheetName Then Throw New InvalidOperationException("Write target changed.")
        Dim target As New SpreadsheetOutputTarget With {.Destination = intent.Destination, .WorksheetName = intent.WorksheetName,
            .StartRow = intent.StartRow, .StartColumn = intent.StartColumn}
        Return InspectWriteCore(intent,
            Sub(region) TargetPlanner.Create(target, ExcelDnaHostCapabilities.GetSnapshot(), Inspect(workbook, worksheet, intent.Destination, region.Bounds)).EnsureSupported(),
            Function(bounds)
                Dim range As Range = worksheet.Range(worksheet.Cells(bounds.StartRow, bounds.StartColumn), worksheet.Cells(bounds.EndRow, bounds.EndColumn))
                Return New ExcelDnaOccupancySnapshot(range.Value2, range.Formula)
            End Function)
    End Function

    ' AddChart appends a shape; it does not overwrite cells or existing shapes. BoxPlot
    ' leaves placement to Excel, so no exact collision footprint is observable in advance.
    ' Existing shapes require a separate visual-overlap decision, never cell overwrite consent.
    Friend Shared Function InspectChartSafety(intent As WriteIntent, workbook As Workbook, worksheet As Worksheet) As ExcelDnaChartSafetySnapshot
        If workbook Is Nothing OrElse worksheet Is Nothing OrElse TargetIdentity(workbook) <> intent.WorkbookIdentity OrElse
            TargetIdentity(worksheet) <> intent.WorksheetIdentity OrElse worksheet.Name <> intent.WorksheetName Then Throw New InvalidOperationException("Chart target changed.")
        If workbook.ReadOnly OrElse worksheet.ProtectContents OrElse worksheet.ProtectDrawingObjects Then Throw New InvalidOperationException("Chart destination is protected or read-only.")
        Dim count As Integer = worksheet.Shapes.Count
        If count > 1000 Then Throw New InvalidOperationException("Shape inspection exceeds the 1,000-object safety quota.")
        Using stream As New MemoryStream(), writer As New BinaryWriter(stream, Encoding.UTF8, True)
            writer.Write(count)
            For index As Integer = 1 To count
                Dim shape = worksheet.Shapes.Item(index)
                writer.Write(TargetIdentity(shape))
                For Each coordinate As Single In {shape.Left, shape.Top, shape.Width, shape.Height}
                    If Single.IsNaN(coordinate) OrElse Single.IsInfinity(coordinate) Then Throw New InvalidOperationException("Unverified shape geometry.")
                    writer.Write(coordinate)
                Next
            Next
            If worksheet.Shapes.Count <> count Then Throw New InvalidOperationException("Shape inventory changed during inspection.")
            writer.Flush()
            Using sha = SHA256.Create()
                Return New ExcelDnaChartSafetySnapshot(count, BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", String.Empty))
            End Using
        End Using
    End Function

    ' This recording seam runs exactly the same inspection, hashing, quotas and scalar
    ' checks as production. Only target facts and bulk native reads are replaced in tests.
    Friend Shared Function InspectWriteCore(intent As WriteIntent, validate As Action(Of WriteRegion),
                                            read As Func(Of OutputRectangle, ExcelDnaOccupancySnapshot)) As WriteInspection
        If intent Is Nothing OrElse validate Is Nothing OrElse read Is Nothing Then Throw New ArgumentNullException("inspection seam")
        If intent.Guards.Count > 0 Then Throw New InvalidOperationException("Windows cell guards are unsupported. Inspect chart objects separately.")
        Dim occupied As New List(Of String)()
        Dim hasValues As Boolean = False, hasFormulas As Boolean = False
        Dim cells As Long = 0
        Dim plannedCells = intent.Regions.Where(Function(r) r.Bounds IsNot Nothing).Select(Function(r) r.Bounds).
            Sum(Function(b) CLng(b.EndRow - b.StartRow + 1) * (b.EndColumn - b.StartColumn + 1))
        If plannedCells > 100000 Then Return New WriteInspection(intent, OutputOccupancy.Unknown, "quota", Nothing,
            explanation:="Occupancy inspection exceeds the Windows 100,000-cell safety quota. Choose a smaller or new-sheet destination.")
        Using stream As New MemoryStream(), writer As New BinaryWriter(stream, Encoding.UTF8, True)
            writer.Write(If(intent.WorkbookIdentity, String.Empty)) : writer.Write(If(intent.WorksheetIdentity, String.Empty)) : writer.Write(intent.WorksheetName)
            For Each region In intent.Regions.Where(Function(r) r.Bounds IsNot Nothing)
                validate(region)
                Dim regionOccupied = SnapshotRegion(read(region.Bounds), region.Bounds, writer, cells, hasValues, hasFormulas)
                If regionOccupied Then occupied.Add(region.ArtifactId)
                CoreServices.Logger.Debug("Shared Windows output stage=cell-occupancy; role=cell-write; artifact=" & region.ArtifactId &
                    "; bounds=" & region.Bounds.ToString() & "; occupied=" & regionOccupied.ToString())
            Next
            writer.Flush()
            Using sha = SHA256.Create()
                Dim token = BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", String.Empty)
                Return New WriteInspection(intent, If(occupied.Count > 0, OutputOccupancy.Occupied, OutputOccupancy.Empty), token, occupied, hasValues, hasFormulas)
            End Using
        End Using
    End Function

    Private Shared Function SnapshotRegion(snapshot As ExcelDnaOccupancySnapshot, bounds As OutputRectangle, writer As BinaryWriter,
                                           ByRef totalCells As Long, ByRef hasValues As Boolean, ByRef hasFormulas As Boolean) As Boolean
        Dim rows As Integer = bounds.EndRow - bounds.StartRow + 1, columns As Integer = bounds.EndColumn - bounds.StartColumn + 1
        totalCells += CLng(rows) * columns
        If totalCells > 100000 Then Throw New InvalidOperationException("Windows occupancy read exceeds safety quota.")
        If snapshot Is Nothing Then Throw New InvalidOperationException("Missing occupancy read.")
        Dim values As Object = snapshot.Values, formulas As Object = snapshot.Formulas
        For Each nativeMatrix As Object In {values, formulas}
            Dim matrix = TryCast(nativeMatrix, Array)
            If matrix Is Nothing Then
                If rows * CLng(columns) > 1 Then Throw New InvalidOperationException("Incomplete occupancy read.")
            ElseIf matrix.Rank <> 2 OrElse matrix.GetLowerBound(0) <> 1 OrElse matrix.GetLowerBound(1) <> 1 OrElse matrix.GetLength(0) <> rows OrElse matrix.GetLength(1) <> columns Then
                Throw New InvalidOperationException("Invalid native occupancy matrix shape.")
            End If
        Next
        Dim occupied As Boolean = False
        writer.Write(bounds.ToString())
        For row As Integer = 1 To rows
            For column As Integer = 1 To columns
                Dim value = MatrixValue(values, row, column), formula = MatrixValue(formulas, row, column)
                If Convert.IsDBNull(value) OrElse Convert.IsDBNull(formula) Then Throw New InvalidOperationException("Unverified native cell value.")
                Dim isFormula As Boolean = TypeOf formula Is String AndAlso DirectCast(formula, String).StartsWith("=", StringComparison.Ordinal)
                Dim isValue As Boolean = value IsNot Nothing AndAlso Not Convert.IsDBNull(value)
                occupied = occupied OrElse isFormula OrElse isValue
                hasFormulas = hasFormulas OrElse isFormula : hasValues = hasValues OrElse (isValue AndAlso Not isFormula)
                For Each scalar In {value, formula}
                    writer.Write(If(scalar Is Nothing, "null", scalar.GetType().FullName))
                    Dim nativeError = TryCast(scalar, System.Runtime.InteropServices.ErrorWrapper)
                    writer.Write(If(nativeError IsNot Nothing, nativeError.ErrorCode.ToString(CultureInfo.InvariantCulture),
                        If(scalar Is Nothing, String.Empty, Convert.ToString(scalar, CultureInfo.InvariantCulture))))
                    If writer.BaseStream.Length > 67108864 Then Throw New InvalidOperationException("Windows occupancy byte quota exceeded.")
                Next
            Next
        Next
        Return occupied
    End Function

    Private Shared Function MatrixValue(value As Object, row As Integer, column As Integer) As Object
        Dim matrix = TryCast(value, Array)
        Return If(matrix Is Nothing, value, matrix.GetValue(row, column))
    End Function

    Friend Shared Function Inspect(workbook As Workbook, worksheet As Worksheet,
                                   destination As SpreadsheetOutputDestination,
                                   Optional bounds As OutputRectangle = Nothing) As OutputTargetFacts
        If destination = SpreadsheetOutputDestination.NewWorkbook AndAlso worksheet Is Nothing Then
            Return New OutputTargetFacts() ' Workbooks.Add does not depend on the active workbook.
        End If
        If workbook Is Nothing Then Return New OutputTargetFacts(workbookAvailable:=False)
        If destination = SpreadsheetOutputDestination.NewWorksheet AndAlso worksheet Is Nothing Then
            Return New OutputTargetFacts(workbookAvailable:=True, writable:=Not workbook.ReadOnly,
                                         canCreateWorksheet:=Not workbook.ProtectStructure)
        End If
        If worksheet Is Nothing Then Return New OutputTargetFacts(workbookAvailable:=True, sheetExists:=False)
        Dim merged As Boolean? = Nothing
        If bounds IsNot Nothing Then
            Dim region As Range = worksheet.Range(worksheet.Cells(bounds.StartRow, bounds.StartColumn),
                                                  worksheet.Cells(bounds.EndRow, bounds.EndColumn))
            Dim value As Object = region.MergeCells
            ' Mixed merged/unmerged ranges return Null; any mixed range is also a conflict.
            merged = If(TypeOf value Is Boolean, CBool(value), True)
        End If
        Return New OutputTargetFacts(workbookAvailable:=True, sheetExists:=True, identityUnambiguous:=True,
            writable:=Not workbook.ReadOnly, protectedSheet:=worksheet.ProtectContents,
            mergedConflict:=merged, resolvedWorksheetName:=worksheet.Name,
            maxRows:=worksheet.Rows.Count, maxColumns:=worksheet.Columns.Count)
    End Function

    Friend Shared Function Preflight(target As SpreadsheetOutputTarget, facts As OutputTargetFacts,
                                     document As OutputDocument,
                                     supplementary As IEnumerable(Of ResultTableOutputModel),
                                     Optional host As HostCapabilitySnapshot = Nothing) As OutputRectangle
        Dim decision = TargetPlanner.Create(target, If(host, ExcelDnaHostCapabilities.GetSnapshot()), facts)
        decision.EnsureSupported()
        Dim limits As New OutputLayoutOptions(maxRows:=decision.Facts.MaxRows, maxColumns:=decision.Facts.MaxColumns)
        Dim layout = OutputLayoutPlanner.Create(document, decision.ToTarget(), limits)
        Return ExcelDnaSharedOutputPlanAdapter.PreflightBounds(layout, supplementary)
    End Function
End Class
