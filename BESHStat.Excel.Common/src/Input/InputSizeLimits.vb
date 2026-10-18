Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.Threading

Namespace ExcelCommon
    ''' <summary>Host-neutral bounded acquisition policy. Chunk cells bound a response;
    ''' total cells/estimated bytes bound the retained snapshot, not just a response.
    ''' Byte estimates include transient buffers but cannot guarantee actual heap use.</summary>
    Public NotInheritable Class InputSizeLimits
        Public Sub New(Optional maxRangeCells As Long = 10000, Optional maxTotalCells As Long = 10000,
                       Optional maxRanges As Integer = 64, Optional maxChunkCells As Integer = 2000,
                       Optional maxEstimatedBytes As Double = 67108864,
                       Optional estimatedBytesPerCell As Double = 1024,
                       Optional timeBudgetMilliseconds As Double = 30000)
            If maxRangeCells < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxRangeCells))
            If maxTotalCells < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxTotalCells))
            If maxRanges < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxRanges))
            If maxChunkCells < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxChunkCells))
            ValidatePositive(maxEstimatedBytes, 9007199254740991R, NameOf(maxEstimatedBytes))
            ValidatePositive(estimatedBytesPerCell, 9007199254740991R, NameOf(estimatedBytesPerCell))
            ValidatePositive(timeBudgetMilliseconds, Integer.MaxValue, NameOf(timeBudgetMilliseconds))
            Me.MaxRangeCells = maxRangeCells
            Me.MaxTotalCells = maxTotalCells
            Me.MaxRanges = maxRanges
            Me.MaxChunkCells = maxChunkCells
            Me.MaxEstimatedBytes = maxEstimatedBytes
            Me.EstimatedBytesPerCell = estimatedBytesPerCell
            Me.TimeBudgetMilliseconds = timeBudgetMilliseconds
        End Sub

        Private Shared Sub ValidatePositive(value As Double, maximum As Double, name As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0 OrElse value > maximum Then
                Throw New ArgumentOutOfRangeException(name, "A finite positive bounded budget is required.")
            End If
        End Sub

        Public ReadOnly Property MaxRangeCells As Long
        Public ReadOnly Property MaxTotalCells As Long
        Public ReadOnly Property MaxRanges As Integer
        Public ReadOnly Property MaxChunkCells As Integer
        Public ReadOnly Property MaxEstimatedBytes As Double
        Public ReadOnly Property EstimatedBytesPerCell As Double
        Public ReadOnly Property TimeBudgetMilliseconds As Double
    End Class

    ''' <summary>One operation's metadata preflight and cooperative checkpoints. Hosts must
    ''' add ALL rectangles before loading cells and call Checkpoint between payloads.
    ''' No spreadsheet address dialect, host dimensions or host objects are assumed.</summary>
    Public NotInheritable Class InputAcquisitionBudget
        Private ReadOnly _limits As InputSizeLimits
        Private ReadOnly _cancellation As CancellationToken
        Private ReadOnly _elapsedMilliseconds As Func(Of Double)
        Private _lastElapsed As Double

        Public Sub New(Optional limits As InputSizeLimits = Nothing,
                       Optional cancellation As CancellationToken = Nothing,
                       Optional elapsedMilliseconds As Func(Of Double) = Nothing)
            _limits = If(limits, New InputSizeLimits())
            _cancellation = cancellation
            Dim timer As Stopwatch = Stopwatch.StartNew()
            _elapsedMilliseconds = If(elapsedMilliseconds, Function() timer.Elapsed.TotalMilliseconds)
            Checkpoint()
        End Sub

        Public ReadOnly Property TotalCells As Long
        Public ReadOnly Property RangeCount As Integer

        Public Sub Checkpoint()
            _cancellation.ThrowIfCancellationRequested()
            Dim elapsed As Double = _elapsedMilliseconds()
            If Double.IsNaN(elapsed) OrElse Double.IsInfinity(elapsed) OrElse elapsed < _lastElapsed Then
                Throw New ArgumentException("Elapsed time must be finite, nonnegative and monotonic.")
            End If
            _lastElapsed = elapsed
            If elapsed > _limits.TimeBudgetMilliseconds Then
                Reject("input.time-budget", "timeBudgetMilliseconds", elapsed, _limits.TimeBudgetMilliseconds)
            End If
        End Sub

        Public Sub AddRange(rows As Long, columns As Long)
            Checkpoint()
            If rows < 1 OrElse columns < 1 Then
                Reject("input.dimensions", "ranges", rows.ToString(CultureInfo.InvariantCulture) & " x " & columns.ToString(CultureInfo.InvariantCulture), "positive dimensions")
            End If
            If rows > Long.MaxValue \ columns Then
                Reject("input.cell-overflow", "ranges", rows.ToString(CultureInfo.InvariantCulture) & " x " & columns.ToString(CultureInfo.InvariantCulture), Long.MaxValue)
            End If
            Dim cells As Long = rows * columns
            If cells > _limits.MaxRangeCells Then Reject("input.range-cells", "maxRangeCells", cells, _limits.MaxRangeCells)
            If RangeCount >= _limits.MaxRanges Then Reject("input.range-count", "maxRanges", CLng(RangeCount) + 1, _limits.MaxRanges)
            If cells > _limits.MaxTotalCells - TotalCells Then
                Reject("input.total-cells", "maxTotalCells", CDec(TotalCells) + CDec(cells), _limits.MaxTotalCells)
            End If
            Dim total As Long = TotalCells + cells
            Dim estimate As Double = CDbl(total) * _limits.EstimatedBytesPerCell
            If estimate > _limits.MaxEstimatedBytes Then Reject("input.estimated-bytes", "maxEstimatedBytes", estimate, _limits.MaxEstimatedBytes)
            _TotalCells = total
            _RangeCount += 1
        End Sub

        Private Shared Sub Reject(code As String, field As String, requested As Object, allowed As Object)
            Dim message As String = String.Format(CultureInfo.InvariantCulture,
                "Acquisition limit {0}: requested {1}; allowed {2}. Select less data or supply an explicit bounded policy.", field, requested, allowed)
            Throw New SpreadsheetInputValidationException(New SpreadsheetValidationReport(
                New SpreadsheetValidationIssue() {New SpreadsheetValidationIssue(code, SpreadsheetValidationSeverity.Error, message, field)}))
        End Sub
    End Class
End Namespace
