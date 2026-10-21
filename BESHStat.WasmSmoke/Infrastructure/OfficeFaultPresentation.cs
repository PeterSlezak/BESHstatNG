using BESHStatNG.ExcelCommon;

namespace BESHStat.WasmSmoke.Infrastructure;

// Presentation only; never changes the Office writer envelope or execution state.
public static class OfficeFaultPresentation
{
    public static UserFault? WriteFault(string? state, string? code, bool mutationMayHaveOccurred)
    {
        OutputWriteState? known = state switch {
            "Completed" => OutputWriteState.Completed,
            "CancelledByUser" => OutputWriteState.CancelledByUser,
            "RejectedBeforeWrite" => OutputWriteState.RejectedBeforeWrite,
            "FailedBeforeWrite" => OutputWriteState.FailedBeforeWrite,
            "PartiallyWritten" => OutputWriteState.PartiallyWritten,
            _ => null
        };
        if (known.HasValue) return UserFaultMapper.FromWriteState(known.Value, code, mutationMayHaveOccurred);
        if (mutationMayHaveOccurred)
            return UserFaultMapper.FromWriteState(OutputWriteState.PartiallyWritten, WriteCodes.MutationUncertain, true);
        if (state == "AwaitingConfirmation") return UserFaultMapper.AwaitingConfirmation();
        return UserFaultMapper.FromException(null);
    }

    public static UserFault? Failure(OfficeExecutionRun run, Exception error, bool writeInFlight)
    {
        if (run.Execution.Snapshot.State is ExecutionState.Completed or ExecutionState.Cancelled)
            return UserFaultMapper.FromOutcome(run.Execution.Snapshot);
        // A thrown interop failure after dispatch cannot establish whether mutation occurred.
        if (writeInFlight)
            return UserFaultMapper.FromWriteState(OutputWriteState.PartiallyWritten, WriteCodes.MutationUncertain, true);
        return UserFaultMapper.FromOutcome(run.Execution.Snapshot, failureException: error);
    }
}
