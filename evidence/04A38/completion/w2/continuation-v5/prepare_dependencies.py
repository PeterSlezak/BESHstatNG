from common import *
source = (HERE / 'MethodMetadata-v6.cs').read_text(encoding='utf-8')
source = source.replace('||RootType(t).FullName=="BESHStatNG.ResultTable"',
    '||RootType(t).FullName=="BESHStatNG.PValuePresentation"||RootType(t).FullName.StartsWith("BESHStatNG.AppInfrastructure.",StringComparison.Ordinal)||RootType(t).FullName=="BESHStatNG.ResultTable"')
with (HERE / 'MethodMetadata-v7.cs').open('x', encoding='utf-8', newline='\n') as stream:
    stream.write(source)
owners = (HERE / 'native-metadata-owners-v6.txt').read_text(encoding='utf-8')
with (HERE / 'native-metadata-owners-v7.txt').open('x', encoding='utf-8', newline='\n') as stream:
    stream.write(owners.rstrip() + '\nBESHStatNG.AppInfrastructure.AppGlobals\n')
folder = PRIVATE / 'dependency-metadata-v7'
folder.mkdir()
exe = folder / 'MethodMetadata38.exe'
r = command('dependency-metadata-v7-compile', [compiler('csc.exe'), '/nologo', '/target:exe',
    '/reference:System.Core.dll', '/reference:System.Web.Extensions.dll', '/out:' + str(exe), str(HERE / 'MethodMetadata-v7.cs')])
records = [clean(r)]
assert r['exit_code'] == 0
for role in ['original', 'current']:
    for config in ['Debug', 'Release']:
        lib = RUNROOT / 'consolidated38-v7/original-build' / config if role == 'original' else PRIVATE / 'cells-v3' / config / 'lib'
        dst = HERE / 'dependency-metadata-v7' / (role + '-' + config + '.json')
        dst.parent.mkdir(exist_ok=True)
        r = command('dependency-metadata-v7-' + role + '-' + config, [str(exe), str(lib), str(dst), str(HERE / 'native-metadata-owners-v7.txt')])
        records.append(clean(r))
        assert r['exit_code'] == 0
save('dependency-metadata-v7.json', dict(commands=records, source=pin(HERE / 'MethodMetadata-v7.cs'),
    original_calls=0, original_compilations=0, scope='Passive full AppGlobals, Core settings/services and PValuePresentation plus existing UDF/engine/result-table bodies'))
