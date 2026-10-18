# Batch 01.06 — survival and longitudinal input contracts

This batch adds **only** parsing/validation in `BESHStat.Excel.Common` (`netstandard2.0`).
No Core algorithm, Windows Ribbon, Office.js bridge, handler registry, or runtime UI
is changed. It builds on batches 01.01–01.05A.

## Public entry points

| Parser | Typed result | Roles / rules |
|---|---|---|
| `LongitudinalParsers.ParseSurvival(range, options)` | `SpreadsheetSurvivalInput` | Numeric survival endpoint; 0/1 censor/event (or explicitly enabled Boolean/text); optional nonnegative delayed entry and type-tagged stratum. Entry must be strictly before exit. |
| `LongitudinalParsers.ParseRepeatedSubjects(range, options)` | `SpreadsheetRepeatedSubjectInput` | Type-tagged subject ID, visit ID, numeric response, optional treatment/group. Optional numeric strictly increasing visits, duplicate (subject, visit) rejection, explicit date serial interpretation. |
| `LongitudinalParsers.ParseMixedModel(range, options)` | `SpreadsheetMixedModelInput` | Repeated base rows plus fixed-effect covariate cell snapshots and one or more random grouping identifiers; optional `VisitColumnIndex=-1` for cross-sectional LMM. |

All indices are **zero-based relative to the selected rectangular range**;
`-1` means *absent* for optional columns. The default heading mode is
`SpreadsheetHeaderMode.Required`. `Auto` is intentionally rejected for
longitudinal/categorical headings because text data can look like headings.
Use `.Header = SpreadsheetHeaderMode.None` for data without a header row.

Parser return types use the pre-existing `SpreadsheetTypedParseResult(Of T)`.
Callers must check `.Success` and inspect `.Validation.Issues`; failed parses
return `Data = Nothing`. Diagnostics include a stable code, severity, field path,
selected-range coordinates and one-based worksheet coordinates (zero when the
acquisition adapter did not supply them). No individual column is independently
repacked: incomplete observations are either rejected or the **entire source row**
is omitted with a warning if `.Missing = Skip`.

## Deliberate semantics and limits

- Survival endpoints and delayed-entry fields are **numeric durations**, not
  unverified Excel dates; default survival `TimeColumnIndex=0`,
  `StatusColumnIndex=1`, `EntryColumnIndex=-1`, `StratumColumnIndex=-1`.
  `RequirePositiveTime=True` by default. An all-censored dataset is structurally
  valid; whether fitting/statistical inference is appropriate belongs to the
  eventual analysis handler.
- Event encoding is **exact numeric `1` event / `0` censored**. Boolean statuses
  require `AllowBooleanStatus=True`, while text coding requires specifying
  *both* distinct `EventText` and `CensorText`. Strings `"1"`/`"0"` are not
  automatically numeric status codes.
- Subject/stratum/group/visit identities preserve numeric versus text versus
  Boolean categories, using `SpreadsheetCategoryValue`. Names such as text
  `"001"` and number `1` are distinct; case folding is opt-in.
- Text visit labels are supported if numeric ordering is not requested.
  `RequireAscendingVisits=True` requires strictly increasing **numeric**
  visit coordinates within each subject in source-row order. Ordering/scheduling
  of textual visits is a later model-builder responsibility, not guessed here.
- `InterpretVisitAsDate=True` only applies to numeric visit serials and uses
  `SpreadsheetDateSystem.Excel1900/Excel1904` **or** explicitly verified
  `DateSerialOrigin`. It rejects unknown/conflicting origins, preventing a
  non-Excel host from silently inheriting Excel's 1900 base.
- Mixed covariates are held as typed `SpreadsheetFrameCell` snapshots, with
  source coordinates, formats, and native error codes intact. Formula/design
  expansion, factors/contrasts, covariance parameterization and model fitting
  are later batches (01.07, 06/07), not implemented here.
- Missingness and error policies are opt-in through existing
  `SpreadsheetCellPolicyOptions`, and pre-existing Excel behavior is unchanged.
- No Windows/Mac/LibreOffice host integration, Core data-model changes, or
  UNO/Python/.NET bridge is introduced.

## Illustrative calls

```vb
Dim survival = LongitudinalParsers.ParseSurvival(selectedRange,
    New SpreadsheetSurvivalParseOptions With {
        .TimeColumnIndex = 0, .StatusColumnIndex = 1,
        .EntryColumnIndex = 2, .StratumColumnIndex = 3})

Dim repeated = LongitudinalParsers.ParseRepeatedSubjects(selectedRange,
    New SpreadsheetRepeatedParseOptions With {
        .SubjectColumnIndex = 0, .VisitColumnIndex = 1,
        .ResponseColumnIndex = 2, .RequireAscendingVisits = True})

Dim mixed = LongitudinalParsers.ParseMixedModel(selectedRange,
    New SpreadsheetMixedModelParseOptions With {
        .SubjectColumnIndex = 0, .VisitColumnIndex = -1,
        .ResponseColumnIndex = 1,
        .FixedEffectColumnIndices = New Integer() {2},
        .RandomGroupColumnIndices = New Integer() {0, 3}})
```

New regression suite: `BESHStat.Excel.Common.Tests/src/LongitudinalParserTests.vb`.
See the package README for full build/test and manual smoke acceptance.
