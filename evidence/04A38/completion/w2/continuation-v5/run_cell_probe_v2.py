from common import *
stage = sys.argv[1]
out = PRIVATE / stage / 'cell-probe'
out.mkdir()
exe = out / 'CellProbe.exe'
vb = Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Microsoft.VisualBasic.dll')
dna = ROOT / 'packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll'
r = command(stage + '-cell-probe-compile', [compiler('csc.exe'), '/nologo', '/target:exe', '/reference:System.Core.dll',
    '/reference:System.Web.Extensions.dll', '/reference:' + str(vb), '/reference:' + str(dna), '/out:' + str(exe), str(HERE / 'CellProbe-v2.cs')])
assert r['exit_code'] == 0
import shutil
shutil.copy2(dna, out / dna.name)
receipts = [clean(r)]
comparisons = []
for c in ['Debug', 'Release']:
    outputs = []
    for role, lib in [('before', RUNROOT / 'completion-w2-cont4/parametric-v4' / c / 'lib'), ('after', PRIVATE / stage / c / 'lib')]:
        dst = HERE / ('cell-probe-v2-' + role + '-' + c + '.json')
        r = command(stage + '-cell-probe-' + role + '-' + c, [str(exe), str(lib), str(dst)])
        receipts.append(clean(r))
        assert r['exit_code'] == 0
        outputs.append(json.loads(dst.read_bytes()))
    comparisons.append(dict(configuration=c, observations=len(outputs[0]['cases']), exact=outputs[0] == outputs[1]))
save('cell-probe-comparison-v2.json', dict(commands=receipts, comparisons=comparisons, original_calls=0,
    scope='Current C4 and current C5 libraries only, Invariant/sk-SK; conversion order/provider/calls/type/bits/Err/exceptions/array cells'))
assert all(r['exact'] for r in comparisons)
print('Current conversion before/after exact', comparisons)
