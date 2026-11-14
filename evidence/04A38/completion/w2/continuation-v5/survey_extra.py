from common import *
data = json.loads((HERE / 'review-input-bodies.json').read_bytes())['rows']
names = {'TryGetFiniteDouble','GetOptionalBool','GetOptionalInt','GetOptionalDouble','TryGetBinary01','TryGetWholeNumber',
         'TryGetFiniteDoubleFlexible','TryGetStatus01Flexible','TryGetOptionalPositiveInteger',
         'TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData','LooksLikeSingleColumnHeader'}
for r in data:
    if r['name'] in names:
        print(r['role'], r['name'], r['source'])
for h in json.loads((PREVIOUS / 'historical-ownership.json').read_bytes())['members']:
    if h['name'] in ['NormalizeText', 'TryParseAlpha', 'LoggedUdfError', 'RowsToMatrix']:
        print('DEPENDENCY', h['name'], bool(h['proof']), h['canonical_original_id'])
