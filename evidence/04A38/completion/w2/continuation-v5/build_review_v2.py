"""Record reviewed complete bodies, precise dependencies and non-generic ownership."""
from common import *
import re

ARG = 'BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb'
RANGE = 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'
DATA = 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb'
OPTIONS = 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportOptions.vb'
OUTPUT = 'BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb'
PAR = 'BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb'
history = json.loads((PREVIOUS / 'historical-ownership.json').read_bytes())['members']
originals = {r['source']['path']: r['snapshot'] for r in json.loads((ROOT / 'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}


def source_member(text, name, signature=None):
    found = []
    for match in re.finditer(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?(Function|Sub) ' + name + r'\b', text, re.M):
        end = text.index('\n' + match[1] + 'End ' + match[2], match.end()) + len('\n' + match[1] + 'End ' + match[2])
        body = text[match.start():end]
        if signature is None or signature in body.splitlines()[0]:
            found.append(body)
    assert len(found) == 1, (name, signature, len(found))
    return found[0]


contracts = []
def add(path, name, dependencies, portable, review, signature=None):
    orig = (ROOT / originals[path]['path']).read_text(encoding='utf-8-sig')
    native = (ROOT / path).read_text(encoding='utf-8-sig')
    a = source_member(orig, name, signature)
    b = source_member(native, name, signature)
    candidates = [h for h in history if h['path'] == path and h['name'] == name]
    if len(candidates) != 1:
        # The Object overload was already closed; only the ResultTable overload is new.
        assert name == 'PrepareResultTableForUdf' and signature == 'table As ResultTable'
        candidates = [h for h in candidates if not h['proof']]
    assert len(candidates) == 1, name
    h = candidates[0]
    depids = []
    for dep in dependencies:
        matches = [r for r in history if r['name'] == dep]
        if dep == 'PrepareResultTableForUdf':
            matches = [r for r in matches if r['proof']]  # Object overload
        assert len(matches) == 1, dep
        depids.append(matches[0]['canonical_original_id'])
    portables = []
    for p, method in portable:
        text = (ROOT / p).read_text(encoding='utf-8-sig')
        portables.append(dict(path=p, name=method, whole_body=source_member(text, method), source_pin=pin(ROOT / p)))
    contracts.append(dict(original_id=h['canonical_original_id'], new_support=not bool(h['proof']), path=path, name=name, owner=h['canonical_original_id'].split('|')[1],
        original_snapshot=originals[path], original_whole_body=a, current_whole_body=b,
        original_body_sha256=sha(a.encode()), current_body_sha256=sha(b.encode()),
        dependency_ids=depids, portable_bodies=portables, review=review,
        disposition='REVIEWED_COMPLETE_SOURCE_COMPOSITION_WITH_ACTUAL_CIL_AND_RESOLVED_DEPENDENCIES',
        branch_proof='Every statement/branch/short-circuit/inline under the exact reviewed source body is bound in the refreshed syntax crosswalk; no child closed by a declaration header',
        exception_byref='All original native conversion/catch boundaries, assignment order, ByRef writes and returned array ownership reviewed as stated in the contract; no new original execution'))

CELL = 'BESHStat.Excel.Common/src/Analysis/UdfCellConversion38.vb'
SCALAR = 'BESHStat.Excel.Common/src/Analysis/UdfScalarChoice38.vb'
NUMERIC = 'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'
RANGEP = 'BESHStat.Excel.Common/src/Analysis/UdfRangePreparation38.vb'
SPILL = 'BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb'
add(ARG, 'CellToTrimmedText', ['IsMissingArg'], [(CELL, 'CellToTrimmedText')],
    'Missing guard stays native. ExcelError takes the original invariant fallback entirely in Windows. Other values execute the complete original string/type-specific numeric/fallback body in Common: six signed/float/Decimal types only, invariant Convert.ToDouble before finite/round checks, strict 1e-12 threshold, checked CLng overflow uncaught. Unsigned/Boolean/DBNull/custom values use invariant Convert.ToString then null/Trim. The portable ByRef argument aliases only the host local Object and is never assigned, preserving the original single CLR boxed-value copy before conversion. DirectCast on the TypeOf String branch replaces the VB conversion helper on its proven domain. No array indexing/copy/mutation. Current-only probe verifies provider/call/Err/exception/input observations.')
add(ARG, 'TryGetDouble', [], [(CELL, 'ConvertFiniteNumber')],
    'Original ordered missing/sentinel/error/Boolean/String rejection remains native before conversion. Convert.ToDouble(v) uses current culture and executes exactly once before finite rejection. Only those operations moved into an uncaught Common method inside the same native Try. Actual Framework SetProjectError/ClearProjectError remain in the native EH region. Invalid casts return nullable Nothing; no input or ByRef state exists; seeded Err clears only on caught paths.')
add(ARG, 'TryGetFiniteDouble', ['TryGetDouble'], [],
    'Whole original/current body and CIL identical. Call parser once; failure writes NaN before False, success writes parsed value before redundant finite check. No catch or exception suppression; parser dependency now supported.')
for name, portable, review in [
    ('GetOptionalBool','OptionalBoolean','Missing/sentinel and Boolean return precede current-culture Convert.ToString.Trim; blank/default and all aliases retain original fixed-RHS text domain. Invalid conversion remains uncaught. Entire Common alias/default body reviewed; v4 EqualText/CaseToken domain proof is unchanged.'),
    ('GetOptionalInt','OptionalInteger','Missing returns caller default; parser runs before nullable selection; Math.Truncate then checked CInt is unchanged, including overflow and lack of local catch.'),
    ('GetOptionalDouble','OptionalDouble','Missing returns supplied default; parser once then HasValue chooses value/default. No new finite or normalization policy.'),
    ('TryGetBinary01','TryBinary','Write -1 before missing guard; numeric parser rejects Boolean without conversion, so eager type-test/CBool before portable dispatch has no callback/throw on a numeric success. Truncate then checked CInt, compare 0/1 and update only on success; Boolean maps to 1/0; numeric overflow remains uncaught.'),
    ('TryGetWholeNumber','TryWholeNumber','Write zero before parsing and again at Common entry, with no intervening externally visible operation. Reject missing/nonfinite, midpoint-to-even rounding, 1e-7 tolerance, Int32 range and final checked CInt preserve all original branches and ByRef values.'),
    ('TryGetFiniteDoubleFlexible','TryFiniteText','Set x=0, numeric-first success updates x; reject missing/error/Boolean before CellToTrimmedText. Blank text retains x; invariant NumberStyles.Any precedes current culture, each successful parse rejects nonfinite; failed parse ByRef zeroing is preserved.'),
    ('TryGetOptionalPositiveInteger','TryPositiveInteger','Assign supplied default before missing check; finite-double conversion before rounding validation. Common repeats default assignment without an intervening callback, receives a present finite nullable and missing=False, so added nullable/finite guards cannot alter the original path. 1e-7 tolerance, minimum then Int32 maximum and final checked assignment remain in order.')]:
    deps = ['TryGetDouble']
    if name == 'TryGetFiniteDoubleFlexible': deps += ['IsMissingArg','CellToTrimmedText']
    if name == 'TryGetOptionalPositiveInteger': deps = ['IsMissingArg','TryGetFiniteDouble']
    add(ARG, name, deps, [(SCALAR, portable)], review)
add(ARG, 'TryGetStatus01Flexible', ['TryGetBinary01','TryGetFiniteDoubleFlexible'], [],
    'Whole original/current source and resolved CIL identical. Initial zero may become -1 in binary helper even on failure; flexible helper does not overwrite integer output until exact x=0/1. Short-circuit success and failed ByRef side effects retained; no truncation-policy correction.')
add(RANGE, 'TryExtractNumericGroupsFromMatrix', ['TryGetDouble','CellToTrimmedText'], [(NUMERIC,'TryExtractNumericGroupsFromMatrix')],
    'Native resets both ByRefs and checks Nothing before dimensions; Common repeats resets, then original dimensions/column-major scan, per-column capacity, numeric callbacks, omission of empty columns and lazy text header only for retained groups. Default numbering uses retained group count and invariant culture. Groups assigned before names only after full scan. Direct nonzero lower bounds retain original index errors; callers through Get2D get detached zero-based values. No alias to input or detected-name arrays.')
add(RANGE, 'LooksLikeSingleColumnHeader', ['TryGetDouble'], [(RANGEP,'LooksLikeSingleColumnHeader')],
    'Null and one-column guards stay native. Exact original row-count/first-numeric/first-below-numeric loop moved with callback HasValue at identical cells and short-circuit points. No conversion of header text or allocation of data arrays.')
add(RANGE, 'TryReadGroupedNumericColumns', ['Get2D','LooksLikeHeaderRow','TryExtractNumericGroupsFromMatrix'], [],
    'Whole original/current source and CIL identical: reset outputs before acquisition, validate dimensions, create ordered Enumerable.Range columns, header detection requires numeric below every column, then derive startRow and Group prefix. Original host acquisition handles scalar rejection, reference order, lower-bound normalization and copied values. Every child contract now supported; no row identity fabricated for opaque arrays.')
add(DATA, 'TryGetGroupedNumericColumns', ['TryReadGroupedNumericColumns'], [],
    'Entire original/current facade is one compiled forwarding call with identical ByRefs and Boolean return. Full grouped reader, header, acquisition and extraction dependencies are now supported; no edge-only parent claim.')
for name in ['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData']:
    deps = ['Get2D','TryGetDouble'] + (['LooksLikeHeaderRow','RowsToMatrix'] if name == 'TryGetGroupedCompleteMatrix' else [])
    add(DATA, name, deps, [(NUMERIC,name)],
        ('Reset outputs before acquiring data then groups in original order. Exact original dimension, header, group-row offset validation; case-insensitive buckets with first-seen order; labels use current-culture native Convert.ToString.Trim, blanks skipped before numeric row conversion; first invalid numeric ends row; allocate fresh row/matrix/name lists; partial output assignments only at original positions. '
         if name == 'TryGetGroupedCompleteMatrix' else
         'Reset data/names before host acquisition. Exact three-column/nonempty guards, first response header probe plus first valid below, lazy default/header names, row-order label conversions before numeric, legacy text comparisons to empty, skip invalid rows, names may be assigned on False, data assigned only after nonempty row list; fresh rectangular result. ') +
        'Common receives original typed arrays from native Get2D and native primitive callbacks; callback bodies have no catch or model/cache state. Full Common body, callback CIL and accepted Core matrix dependency are retained; text-domain evidence is unchanged.')
add(OPTIONS, 'TryParseAlpha', ['IsMissingArg'], [(SCALAR,'TryAlphaText'),(SCALAR,'IsOpenProbability')],
    'Initial alpha=.05 regardless of global default, then native missing guard. String conversion/Trim and numeric Convert.ToDouble stay within original real Framework Try; invariant NumberStyles.Any TryParse followed only on failure by current-culture TryParse writes directly to caller alpha. Portable early IsOpenProbability replaces only post-Try finite/open-interval comparisons; it cannot throw or observe state. Numeric conversion failure leaves .05; invalid text leaves parser output; no default unification.')
add(OUTPUT, 'IsNumericValue', [], [(CELL,'IsNumericTypeCode')],
    'Native null/DBNull/Boolean rejection and runtime Type.GetTypeCode(value.GetType()) remain before portable exact eleven-code switch. Underlying numeric enum types remain eligible, including legacy ExcelError behavior; no conversion occurs here. Custom IConvertible GetTypeCode is never called. Raw Excel values never pass to the typed portable whitelist.')
add(OUTPUT, 'FormatPValueForUdf', ['IsNumericValue'], [],
    'Whole original/current body/CIL exact: nonnumeric returns original object by reference; invariant numeric conversion within real native catch returns same object on exception; only after success read AppGlobals.PValuePresentation afresh and call accepted Core FormatForDisplay. No cached settings, thresholds, finite rejection or precision change. Full settings/services/formatter type/field/initializer/method context pinned to original passive metadata.')
add(OUTPUT, 'PrepareResultTableForUdf', ['PrepareResultTableForUdf','FormatPValueForUdf'], [(SPILL,'PvalueCells')],
    'ResultTable null guard, ToOutputModel then original Object normalization; fresh model owns nonnull copied metadata lists. Original column-first then explicit-cell loops use identical coordinate formulas, range tests and duplicate order. Address-list preparation has no user callbacks or host settings reads; formatting sees only CLR primitive/enum numeric cells, so no caller/model mutation can interleave. Keep per-cell AppGlobals read, blank/error preservation and output shape. Accepted ResultTable/OutputModel constructors and ToOutputModel full source/IL unchanged.', 'table As ResultTable')
add(OUTPUT, 'IsPValueLabel', [], [(SPILL,'IsPValueLabel')],
    'Native null/DBNull guards precede invariant Convert.ToString; Common preserves blank rejection, Unicode letter/digit lower-invariant filtering and Contains(pvalue). No Excel interpretation or label normalization change.')
for name, deps, review in [
    ('PreparePValueTableForUdf',['PrepareResultTableForUdf','FormatPValueForUdf'],'Normalize first, return on null output/column list; one-based column subtraction and bounds before row-major formatting within each supplied column; duplicate columns, integer overflow, blank/string/error identity and per-cell setting reads unchanged.'),
    ('PreparePValueRowsForUdf',['PrepareResultTableForUdf','IsPValueLabel','FormatPValueForUdf'],'Normalize first; null/<2-column short circuit; inspect first-column label before formatting remaining columns in row order; no new header skips or p-value eligibility changes.'),
    ('MarkPValueRows',['IsPValueLabel','IsNumericValue'],'Null/table/body/width guards before row-label recognition; only eligible cells get one-based body addresses in original row/column order; duplicate metadata semantics remain Core-owned.'),
    ('StackResultTables',['PrepareResultTableForUdf','StackWithBlankRow'],'Null/empty list returns Nothing; process table list in order, prepare each metadata-bearing table, stack with existing exact Common blank-row layout, then normalize after every append. Local partial result not returned on exception.'),
    ('BuildResultTable',['MarkPValueRows','PrepareResultTableForUdf'],'Construct accepted ResultTable, SetBody, mark numeric p-value rows, add title header, then metadata-bearing preparation; preserve sequence, metadata coordinates, blank propagation and original body ownership.')]:
    add(OUTPUT, name, deps, [], 'Whole original/current resolved CIL and source exact. ' + review)
add(OUTPUT, 'ClampProb', [], [('BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb','ClampProbabilityValue')],
    'Native NaN/infinity returns ExcelErrorNum before portable min/max clamp. Original negative-to-zero/greater-than-one-to-one/otherwise identity with signed zero unchanged; no scientific p-value change.')
for name, deps, report in [
    ('ANOVA1',['TryGetGroupedNumericColumns','ResolveNames','PrepareResultTableForUdf','LoggedUdfError'],'BuildOneWayAnovaTable'),
    ('ANOVA1_WELCH',['TryGetGroupedNumericColumns','ResolveNames','PrepareResultTableForUdf','LoggedUdfError'],'BuildWelchAnovaTable'),
    ('ANOVA1_MCP',['TryGetGroupedNumericColumns','ResolveNames','TryParseAlpha','NormalizeText','PrepareResultTableForUdf','StackWithBlankRow','LoggedUdfError'],'BuildOneWayMcpTables')]:
    add(PAR, name, deps, [('BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb',report)],
        'Full C4 independently checked original/current source substitution and actual CIL rebound unchanged. Import then group-count rejection then lazy names; MCP alpha before selector conversion, existing engine construction/compute before selector rejection. Classical compute/wrap/table0; Welch compute then original WelshANOVA, same cell types/labels/SetBody/pvalue-column/header order; MCP exact aliases/actions/ALL order, selected table1 or sequential rendering/stack with final normalization once. Entire OneWayANOVA engine and ResultTable source/IL/context unchanged; native real catch/logging/error fallback preserved. All surrounding named dependencies now supported rather than merely the report call edge.')
for name, deps in [('FindLastNonBlankRow',['IsBlankCell']),('HasNumericColumnHeader',['IsBlankCell','TryGetDouble']),
                   ('HasBinaryColumnHeader',['TryGetBinary01']),('HasNumericMatrixHeader',['IsBlankCell','TryGetDouble'])]:
    add(RANGE, name, deps, [(RANGEP,name)],
        'Complete original null/dimension guards stay in native adapter, followed by exact original portable scanning/validation operations. Reverse row/ascending column order for last-row search. Header probes preserve blank-before-number and first/second cell conversion order, including eager second probe, all-column checks and short circuits. Numeric and binary callbacks are native primitives with reviewed state/exception behavior; no array normalization or data/name mutation occurs here.')
save('body-contracts-v2.json', dict(contracts=contracts, original_queries=0,
    review_scope='Complete selected original/current bodies and all portable sub-bodies, native host state, callbacks and named dependencies; no generic HOST_ONLY'))
print('Reviewed complete source contracts', len(contracts))
