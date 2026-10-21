using BESHStat.WasmSmoke.Infrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class OfficeFaultTests
{
    private const string Payload = "PRIVATE Sheet!A1 C:\\people\\secret.xlsx JS stack at write";
    private sealed class BrokenSink : IDiagnosticSink
    {
        public Task EmitAsync(DiagnosticRecord record) => throw new InvalidOperationException(Payload);
    }

    [TestMethod]
    public async Task UnknownInteropLikeFailureStaysGenericDespiteBrokenDiagnosticSink()
    {
        using var run = new OfficeExecutionRun();
        var original = new Exception(Payload);
        var caught = await Assert.ThrowsExceptionAsync<Exception>(() => run.RunAsync(
            new DiagnosticEmitter(new BrokenSink(), null, null), AnalysisIds.OneWayAnova, false,
            session => throw original));
        Assert.AreSame(original, caught);
        Assert.AreEqual(ExecutionState.Failed, run.Execution.Snapshot.State);
        var fault = OfficeFaultPresentation.Failure(run, caught, false);
        Assert.AreEqual(UserFaultCodes.Unexpected, fault!.Code);
        var display = UserFaultMapper.FormatDisplay(fault, run.Diagnostics!.RunId);
        AssertSafe(display);
        StringAssert.Contains(display, run.Diagnostics.RunId);
        Assert.IsFalse(run.IsBusy);
    }

    [TestMethod]
    public async Task ValidationShowsCorrectionAndSelectedOffsetsWithoutPayload()
    {
        using var run = new OfficeExecutionRun();
        var original = new SpreadsheetInputValidationException(new SpreadsheetValidationReport(new[] {
            new SpreadsheetValidationIssue("cell.not-numeric", SpreadsheetValidationSeverity.Error,
                Payload, "observations", 4, 2, 104, 12) }));
        await Assert.ThrowsExceptionAsync<SpreadsheetInputValidationException>(() => run.RunAsync(
            new DiagnosticEmitter(null, null, null), AnalysisIds.KruskalWallis, false, session => throw original));
        var fault = OfficeFaultPresentation.Failure(run, original, false)!;
        Assert.AreEqual(UserFaultCodes.InvalidInput, fault.Code);
        StringAssert.Contains(fault.Action, "numeric observation");
        Assert.AreEqual(4, fault.SelectedRow);
        AssertSafe(UserFaultMapper.FormatDisplay(fault));
    }

    [DataTestMethod]
    [DataRow("Completed", "", UserFaultSeverity.Information)]
    [DataRow("CancelledByUser", "output.cancelled", UserFaultSeverity.Information)]
    [DataRow("RejectedBeforeWrite", "output.consent-stale", UserFaultSeverity.Warning)]
    [DataRow("FailedBeforeWrite", "output.host-failed", UserFaultSeverity.Error)]
    [DataRow("PartiallyWritten", "output.mutation-uncertain", UserFaultSeverity.Warning)]
    [DataRow("AwaitingConfirmation", "output.confirmation-required", UserFaultSeverity.Information)]
    [DataRow("PRIVATE", "user.unexpected", UserFaultSeverity.Error)]
    [DataRow("0", "user.unexpected", UserFaultSeverity.Error)]
    public void WriterPresentationUsesClosedStateVocabulary(string state, string code, UserFaultSeverity severity)
    {
        var fault = OfficeFaultPresentation.WriteFault(state, WriteCodes.Changed, false);
        if (state == "Completed") { Assert.IsNull(fault); return; }
        Assert.AreEqual(code, fault!.Code);
        Assert.AreEqual(severity, fault.Severity);
        AssertSafe(UserFaultMapper.FormatDisplay(fault));
    }

    [TestMethod]
    public async Task UnknownWriteFailureWarnsAboutRemainingOutputWithoutGuessingPermission()
    {
        using var run = new OfficeExecutionRun();
        var original = new Exception(Payload);
        await Assert.ThrowsExceptionAsync<Exception>(() => run.RunAsync(new DiagnosticEmitter(null, null, null),
            AnalysisIds.OneWayAnova, false, async session => await session.WriteAsync<int>(() => throw original)));
        var fault = OfficeFaultPresentation.Failure(run, original, true)!;
        Assert.AreEqual(WriteCodes.MutationUncertain, fault.Code);
        StringAssert.Contains(fault.Summary, "No rollback");
        AssertSafe(UserFaultMapper.FormatDisplay(fault));
        Assert.AreEqual(WriteCodes.MutationUncertain, OfficeFaultPresentation.WriteFault(Payload, Payload, true)!.Code);
    }

    [TestMethod]
    public async Task CompletedWriteAndLateRequestCannotBecomePresentationFailure()
    {
        using var run = new OfficeExecutionRun();
        await run.RunAsync(new DiagnosticEmitter(null, null, null), AnalysisIds.OneWayAnova, false, async session => {
            var state = await session.WriteAsync(() => {
                session.Execution.RequestCancellation();
                return Task.FromResult("Completed");
            });
            session.FinishOutput(state);
        });
        Assert.IsNull(OfficeFaultPresentation.Failure(run, new Exception(Payload), true));
        Assert.AreEqual(ExecutionState.Completed, run.Execution.Snapshot.State);
    }

    [TestMethod]
    public async Task ConsentCancellationUsesConfirmedStateAndNeverStartsWrite()
    {
        using var run = new OfficeExecutionRun();
        var writes = 0;
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => run.RunAsync(
            new DiagnosticEmitter(null, null, null), AnalysisIds.OneWayAnova, false, async session => {
                await session.ConfirmAsync(session.Cancel);
                writes++;
            }));
        Assert.AreEqual(0, writes);
        Assert.AreEqual(ExecutionState.Cancelled, run.Execution.Snapshot.State);
        Assert.AreEqual(UserFaultCodes.Cancelled, OfficeFaultPresentation.Failure(run, new Exception(Payload), false)!.Code);
    }

    private static void AssertSafe(string message)
    {
        foreach (var secret in new[] { "PRIVATE", "Sheet!A1", "secret.xlsx", "JS stack" })
            Assert.IsFalse(message.Contains(secret), message);
    }
}
