Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Gate B compatibility assertions; no new production serializer or wire format.</summary>
<TestClass>
Public Class GateBOutputContractCompatibilityTests
    <TestMethod>
    Public Sub PublishedEnumMeanings_RemainInt32AndKeepExistingValues()
        CheckEnum(GetType(SpreadsheetCellKind), "Blank=0|Number=1|Text=2|Bool=3|CellError=4")
        CheckEnum(GetType(SpreadsheetOutputDestination), "CurrentWorksheet=0|NewWorksheet=1|NewWorkbook=2")
        CheckEnum(GetType(SpreadsheetAddressDialect), "Unknown=0|ExcelA1=1|HostNative=2")
        CheckEnum(GetType(SpreadsheetValidationSeverity), "Information=0|Warning=1|Error=2")
        CheckEnum(GetType(AnalysisDocumentStatus), "Succeeded=0|Failed=1")
        CheckEnum(GetType(AnalysisArtifactKind), "Table=1|Metric=2|TypedDetail=3")
        CheckEnum(GetType(AnalysisMetricRole), "Statistic=0|PValue=1")
        CheckEnum(GetType(OutputArtifactKind), "Table=1|Diagnostic=2|Plot=3")
        CheckEnum(GetType(OutputDiagnosticSeverity), "Warning=1|Fault=2")
        CheckEnum(GetType(OutputCellKind), "Blank=0|Number=1|Text=2|BooleanValue=3|Error=4|Formula=5")
        CheckEnum(GetType(OutputNumberKind), "Binary64=1|ExactDecimal=2")
        CheckEnum(GetType(OutputErrorKind), "Unknown=0|NullIntersection=1|DivisionByZero=2|InvalidValue=3|InvalidReference=4|UnknownName=5|InvalidNumber=6|NotAvailable=7|GettingData=8|NonFinite=9")
        CheckEnum(GetType(OutputStyleRole), "Body=0|Title=1|TopColumnHeader=2|LeftRowHeader=3|Footer=4|DiagnosticWarning=5|DiagnosticFault=6")
        CheckEnum(GetType(OutputNumberFormatKind), "General=0|Numeric=1|Percentage=2|Scientific=3|PValue=4")
        CheckEnum(GetType(CompositeOutputKind), "Table=1|Diagnostic=2|PlotPlaceholder=3|Note=4|Link=5")
        CheckEnum(GetType(OutputOccupancy), "Empty=0|Occupied=1|Unknown=2|Rejected=3")
        CheckEnum(GetType(OutputWriteState), "Completed=0|CancelledByUser=1|RejectedBeforeWrite=2|FailedBeforeWrite=3|PartiallyWritten=4")
    End Sub

    Private Shared Sub CheckEnum(enumType As Type, published As String)
        Assert.AreEqual(GetType(Integer), [Enum].GetUnderlyingType(enumType), enumType.Name)
        ' Existing meanings are locked; this does not automatically approve new members.
        For Each entry As String In published.Split("|"c)
            Dim parts = entry.Split("="c)
            Assert.IsTrue([Enum].IsDefined(enumType, parts(0)), enumType.Name & "." & parts(0))
            Assert.AreEqual(Integer.Parse(parts(1), Globalization.CultureInfo.InvariantCulture),
                            Convert.ToInt32([Enum].Parse(enumType, parts(0))), enumType.Name & "." & parts(0))
        Next
    End Sub

    <TestMethod>
    Public Sub SafeWriteAndCapabilityCodes_KeepPublishedSpellings()
        CheckCodes(GetType(WriteCodes), "Completed=output.completed|Cancelled=output.cancelled|Empty=output.occupancy-empty|Occupied=output.occupancy-occupied|Unverified=output.occupancy-unverified|Rejected=output.rejected|Changed=output.consent-stale|ConfirmationRequired=output.confirmation-required|Failed=output.host-failed|MutationUncertain=output.mutation-uncertain")
        CheckCodes(GetType(HostCapabilityIds), "ReadSelectedRange=input.read-selected-range|WriteResultTables=output.write-result-tables|CurrentWorksheet=output.current-worksheet|NewWorksheet=output.new-worksheet|NewWorkbook=output.new-workbook")
        CheckCodes(GetType(CapabilityPreflightCodes), "HostUnavailable=host.unavailable|UnknownAnalysis=analysis.unknown|UnsupportedAnalysis=analysis.unsupported|MissingCapability=host.capability-missing|UnsupportedDestination=output.destination-unsupported|InvalidTarget=output.target-invalid|SheetNameCollision=output.sheet-name-collision|UnwritableTarget=output.target-unwritable|MergedTarget=output.target-merged")
    End Sub

    Private Shared Sub CheckCodes(owner As Type, published As String)
        For Each entry As String In published.Split("|"c)
            Dim parts = entry.Split("="c)
            Dim field = owner.GetField(parts(0))
            Assert.IsNotNull(field, owner.Name & "." & parts(0))
            Assert.AreEqual(GetType(String), field.FieldType)
            Assert.IsTrue(field.IsLiteral)
            Assert.AreEqual(parts(1), DirectCast(field.GetRawConstantValue(), String))
        Next
    End Sub

    <TestMethod>
    Public Sub RequiredSafeWriteAndScalarMembers_KeepTypesAndReadOnlyBoundary()
        CheckProperty(GetType(OutputCell), "Kind", GetType(OutputCellKind))
        CheckProperty(GetType(OutputCell), "NumberKind", GetType(OutputNumberKind?))
        CheckProperty(GetType(OutputCell), "BinaryValue", GetType(Double?))
        CheckProperty(GetType(OutputCell), "DecimalValue", GetType(Decimal?))
        CheckProperty(GetType(OutputCell), "TextValue", GetType(String))
        CheckProperty(GetType(OutputCell), "BooleanValue", GetType(Boolean?))
        CheckProperty(GetType(OutputCell), "NativeErrorCode", GetType(Integer?))
        CheckProperty(GetType(OutputCell), "FormulaExpression", GetType(String))
        CheckProperty(GetType(OutputCell), "FormulaLanguage", GetType(String))
        CheckProperty(GetType(WriteIntent), "Regions", GetType(IReadOnlyList(Of WriteRegion)))
        CheckProperty(GetType(WriteRegion), "Bounds", GetType(OutputRectangle))
        CheckProperty(GetType(WriteInspection), "OccupiedRegionIds", GetType(IReadOnlyList(Of String)))
        CheckProperty(GetType(WriteResult), "SchemaVersion", GetType(Integer))
        CheckProperty(GetType(WriteResult), "State", GetType(OutputWriteState))
        CheckProperty(GetType(WriteResult), "ConfirmedWritten", GetType(IReadOnlyList(Of WriteRegion)))
        CheckProperty(GetType(WriteResult), "MutationMayHaveOccurred", GetType(Boolean))
        CheckProperty(GetType(WriteResult), "ConfirmationRequired", GetType(Boolean?))
        CheckProperty(GetType(WriteResult), "OverwriteApproved", GetType(Boolean?))
        CheckProperty(GetType(WriteResult), "UndoSupported", GetType(Boolean))
        CheckProperty(GetType(WriteResult), "RollbackAttempted", GetType(Boolean))
        CheckProperty(GetType(WriteResult), "RollbackSucceeded", GetType(Boolean))
    End Sub

    Private Shared Sub CheckProperty(owner As Type, name As String, expectedType As Type)
        Dim member = owner.GetProperty(name)
        Assert.IsNotNull(member, owner.Name & "." & name)
        Assert.AreEqual(expectedType, member.PropertyType, owner.Name & "." & name)
        Assert.IsTrue(member.CanRead)
        Assert.IsFalse(member.CanWrite, owner.Name & "." & name)
    End Sub

    <TestMethod>
    Public Sub AnalysisSnapshot_ExistingReaderIgnoresOptionalFieldsWithoutRetainingThem()
        Dim document = AnalysisResultDocument.Failure(AnalysisIds.OneWayAnova,
            New AnalysisFault(AnalysisFaultCodes.InvalidRequest, "Invalid request.", "input"))
        Dim original = JsonSerializer.Serialize(document.ToContractSnapshot())
        Dim extended = JsonNode.Parse(original).AsObject()
        extended.Add("OptionalFutureDiagnostic", JsonValue.Create("optional"))
        extended("Faults").AsArray()(0).AsObject().Add("OptionalFutureHint", JsonValue.Create(17))
        ' This is the existing mutable diagnostic snapshot DTO with the existing default
        ' System.Text.Json reader. It says nothing about strict Office plan readers.
        Dim restored = JsonSerializer.Deserialize(Of AnalysisResultDocumentSnapshot)(extended.ToJsonString())
        Assert.AreEqual(1, restored.SchemaVersion)
        Assert.AreEqual(AnalysisDocumentStatus.Failed, restored.Status)
        Assert.AreEqual(AnalysisFaultCodes.InvalidRequest, restored.Faults(0).Code)
        Assert.AreEqual("input", restored.Faults(0).FieldPath)
        Assert.AreEqual(original, JsonSerializer.Serialize(restored))
    End Sub

    <DataTestMethod>
    <DataRow("reordered")>
    <DataRow("subset")>
    Public Sub Consent_RejectsChangedOccupiedRegionSequenceEvenWithSameToken(change As String)
        Dim intent As New WriteIntent("payload", "target", New SpreadsheetOutputTarget(),
            {New WriteRegion("first", New OutputRectangle(1, 1, 1, 1)),
             New WriteRegion("second", New OutputRectangle(3, 1, 3, 1))})
        Dim original As New WriteInspection(intent, OutputOccupancy.Occupied, "observation",
                                           {"first", "second"}, hasValues:=True)
        Dim consent As New WriteConsent(original, True)
        Assert.IsTrue(consent.Authorizes(original))
        Dim changedIds As String() = If(change = "reordered", {"second", "first"}, {"second"})
        Dim current As New WriteInspection(intent, OutputOccupancy.Occupied, "observation",
                                          changedIds, hasValues:=True)
        Assert.IsFalse(consent.Authorizes(current))
    End Sub
End Class
