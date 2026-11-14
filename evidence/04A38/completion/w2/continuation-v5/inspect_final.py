"""Sequential final inspection after both completed builds; passive metadata only."""
from common import *
stage = 'cells-v3'
for config in ['Debug', 'Release']:
    assert json.loads((HERE / ('build-' + stage + '-' + config + '.json')).read_bytes())['all_exit_zero']
folder = PRIVATE / 'final-inspection'
folder.mkdir()
records = []
facade = Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
for name, src in [('Inspect38', ROOT / 'evidence/04A38/helpers/Inspect38.cs'),
                  ('Closure38', ROOT / 'evidence/04A38/helpers/PortableConsumerClosure38.cs'),
                  ('MethodBodies38', W2 / 'MethodBodies-v2.cs'), ('MethodMetadata38', HERE / 'MethodMetadata-v6.cs')]:
    exe = folder / (name + '.exe')
    r = command('final-' + name + '-compile', [compiler('csc.exe'), '/nologo', '/target:exe', '/reference:System.Core.dll',
        '/reference:System.Web.Extensions.dll', '/reference:' + str(facade), '/out:' + str(exe), str(src)])
    records.append(clean(r))
    assert r['exit_code'] == 0
    for config in ['Debug', 'Release']:
        lib = PRIVATE / stage / config / 'lib'
        dst = HERE / 'inspection/final' / (name + '-' + config + '.json')
        dst.parent.mkdir(parents=True, exist_ok=True)
        if name == 'Inspect38':
            args = [str(lib), str(dst), str(ROOT / 'evidence/04A38/entry-members.json')]
        elif name == 'Closure38':
            args = [str(lib), str(ROOT / 'evidence/04A38/entry-members.json'), str(dst)]
        elif name == 'MethodMetadata38':
            args = [str(lib), str(dst), str(HERE / 'native-metadata-owners-v6.txt')]
        else:
            args = [str(lib), str(dst)]
        r = command('final-' + name + '-' + config, [str(exe), *args])
        records.append(clean(r))
        assert r['exit_code'] == 0
    if name == 'MethodMetadata38':
        for config in ['Debug', 'Release']:
            lib = RUNROOT / 'consolidated38-v7/original-build' / config
            dst = HERE / 'inspection/final' / ('original-' + config + '.json')
            r = command('final-original-metadata-' + config, [str(exe), str(lib), str(dst), str(HERE / 'native-metadata-owners-v6.txt')])
            records.append(clean(r))
            assert r['exit_code'] == 0
            assert json.loads(dst.read_bytes()) == json.loads((PREVIOUS / 'metadata-v6/parametric-v4' / ('original-' + config + '.json')).read_bytes())
census = RUNROOT / 'mmrm38-v6/w1-census-tool-v2/bin/Census.dll'
r = command('final-current-syntax', ['dotnet', str(census), str(ROOT), str(HERE / 'current-universe-final.json')])
records.append(clean(r))
assert r['exit_code'] == 0
rows = []
entry = json.loads((HERE / 'entry.json').read_bytes())
for before in entry['source_before']:
    path = before['live']['path']
    dst = HERE / 'source-after/final' / (path + '.txt')
    dst.parent.mkdir(parents=True, exist_ok=True)
    dst.write_bytes((ROOT / path).read_bytes())
    rows.append(dict(path=path, before=before['live'], preimage=before['preimage'], after=pin(ROOT / path), snapshot=pin(dst)))
for path in entry['new_sources']:
    dst = HERE / 'source-after/final' / (path + '.txt')
    dst.parent.mkdir(parents=True, exist_ok=True)
    dst.write_bytes((ROOT / path).read_bytes())
    rows.append(dict(path=path, before=None, preimage=None, after=pin(ROOT / path), snapshot=pin(dst)))
save('source-transitions.json', dict(files=rows, original_queries=0, engine_changes='NONE', registration_changes='NONE'))
exports = []
for c in ['Debug', 'Release']:
    old = json.loads((ROOT / 'evidence/04A38/preedit' / ('compiled-' + c + '.json')).read_bytes())['exports']
    new = json.loads((HERE / 'inspection/final' / ('Inspect38-' + c + '.json')).read_bytes())['exports']
    key = lambda row: row['effective_excel_name']
    assert len(new) == 268 and sorted(old, key=key) == sorted(new, key=key)
    exports.append(dict(configuration=c, exact=268))
save('inspection-final.json', dict(commands=records, exports=exports, original_queries=0, original_activity='PASSIVE_METADATA_ONLY'))
save('attempts/inspection-race.json', dict(status='FAILED_ATTEMPTS_RETAINED',
    commands=['w2c5-cells-v3-Inspect38-Release', 'w2c5-cells-v3-metadata-v6-current-Release'],
    cause='Inspection was incorrectly launched before the independent Release build completed; libraries not yet all available',
    correction='inspect_final.py requires both completed build receipts and uses fresh append-only outputs', production_defect=False))
print('Final CIL/source/268 exports verified each configuration')
