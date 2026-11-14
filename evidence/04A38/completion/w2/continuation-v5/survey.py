from common import *
import re
h = json.loads((PREVIOUS / 'historical-ownership.json').read_bytes())
families = ['ExcelArgModules', 'UdfDataImportRangeReaders', 'UdfOutputTables', 'ParametricUDFs']
rows = [r for r in h['members'] if any(x in r['path'] for x in families)]
print('\n'.join(('CLOSED ' if r['proof'] else 'OPEN ') + r['name'] for r in rows))
originals = json.loads((ROOT / 'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']
names = ['IsMissingArg', 'IsBlankCell', 'AsString', 'CellToTrimmedText', 'TryGetDouble', 'TryGetFiniteDouble',
         'GetOptionalBool', 'GetOptionalInt', 'GetOptionalDouble', 'TryGetBinary01', 'TryGetWholeNumber',
         'TryGetFiniteDoubleFlexible', 'TryGetStatus01Flexible', 'TryGetOptionalPositiveInteger',
         'Get2D', 'LooksLikeHeaderRow', 'LooksLikeSingleColumnHeader', 'TryExtractNumericGroupsFromMatrix',
         'TryReadGroupedNumericColumns', 'TryGetGroupedNumericColumns', 'TryGetGroupedCompleteMatrix',
         'TryGetNestedThreeColumnData', 'TryParseAlpha', 'PrepareResultTableForUdf', 'IsNumericValue',
         'FormatPValueForUdf', 'ANOVA1', 'ANOVA1_WELCH', 'ANOVA1_MCP', 'ResolveNames', 'LoggedUdfError']
out = []
for item in originals:
    path = item['source']['path']
    for role, file in [('original', ROOT / item['snapshot']['path']), ('current', ROOT / path)]:
        source = file.read_text(encoding='utf-8-sig')
        for match in re.finditer(r'^([ \t]*)(?:Public|Private|Friend) (?:Shared )?Function (\w+)\b', source, re.M):
            if match[2] not in names:
                continue
            end = source.index('\n' + match[1] + 'End Function', match.end()) + len('\n' + match[1] + 'End Function')
            out.append(dict(path=path, role=role, name=match[2], source=source[match.start():end], source_pin=pin(file)))
save('review-input-bodies.json', dict(rows=out, originals='Existing immutable snapshots only; no original execution'))
for row in out:
    if row['role'] == 'original' and row['name'] in ['Get2D', 'LooksLikeHeaderRow', 'TryExtractNumericGroupsFromMatrix', 'PrepareResultTableForUdf', 'TryParseAlpha']:
        print(row['path'], row['name'], row['source'])
