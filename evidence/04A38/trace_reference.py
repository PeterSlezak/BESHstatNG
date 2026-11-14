"""Build an explicitly instrumented, isolated coverage copy of entry source.

This is supplementary reachability instrumentation, never the output oracle.
The unchanged original-source v6 library remains the output authority.
"""
import json
from collections import defaultdict
from preflight import ROOT, HERE, pin, save
from run import PRIVATE, m


def main():
    folder = PRIVATE / 'trace-points-v1'
    folder.mkdir(exist_ok=False)
    runner = m.Runner(PRIVATE)
    build = runner.command('trace-points-build-v1', ['dotnet', 'msbuild',
        str(HERE / 'helpers/TracePoints38.csproj'), '/t:Build',
        '/p:ProjectAssetsFile=' + str(ROOT / 'evidence/04A37/helpers/obj/project.assets.json'),
        '/p:IntermediateOutputPath=' + str(folder / 'obj') + '/',
        '/p:OutputPath=' + str(folder / 'bin') + '/'])
    assert build['exit_code'] == 0
    positions = HERE / 'preedit/attempts/reachability-v1/positions.json'
    positions.parent.mkdir(exist_ok=False)
    result = runner.command('trace-points-read-v1', ['dotnet', str(folder / 'bin/TracePoints38.dll'),
        str(HERE / 'entry-members.json'), str(positions)])
    assert result['exit_code'] == 0
    grouped = defaultdict(list)
    for point in json.loads(positions.read_bytes()):
        grouped[point['path']].append(point)
    snapshots = json.loads((HERE / 'preedit/entry-source-snapshot.json').read_bytes())['files']
    records = []
    for row in snapshots:
        path = row['source']['path']
        original = (ROOT / row['snapshot']['path']).read_bytes()
        text = original.decode('utf-8-sig')
        inserted = []
        for point in sorted(grouped[path], key=lambda p: p['insertion_utf16'], reverse=True):
            offset = len(text.encode('utf-16-le')[:point['insertion_utf16'] * 2].decode('utf-16-le'))
            addition = 'Global.BESHStatNG.UdfTrace38.Hit("' + point['symbol_id'].replace('"', '""') + '")\n        '
            text = text[:offset] + addition + text[offset:]
            inserted.append(dict(offset=offset, addition=addition, symbol_id=point['symbol_id']))
        relative = path.removeprefix('BESHstatNG/src/ExcelUDFs/')
        dest = positions.parent / 'source' / relative
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_text(text, encoding='utf-8', newline='')
        restored = text
        for item in reversed(inserted):
            offset = item['offset']
            assert restored[offset:offset + len(item['addition'])] == item['addition']
            restored = restored[:offset] + restored[offset + len(item['addition']):]
        assert restored == original.decode('utf-8-sig')
        records.append(dict(original=row['snapshot'], instrumented=pin(dest.relative_to(ROOT).as_posix()), insertions=inserted))
    trace = positions.parent / 'UdfTrace38.vb'
    trace.write_text('''Option Strict On
Public Module UdfTrace38
    Private ReadOnly Seen As New System.Collections.Generic.HashSet(Of String)(System.StringComparer.Ordinal)
    Public Sub Hit(identity As String)
        Seen.Add(identity)
    End Sub
    Public Sub Clear()
        Seen.Clear()
    End Sub
    Public Function Snapshot() As String()
        Dim values(Seen.Count - 1) As String
        Seen.CopyTo(values)
        System.Array.Sort(values, System.StringComparer.Ordinal)
        Return values
    End Function
End Module
''', encoding='utf-8', newline='\n')
    save('preedit/attempts/reachability-v1/source-bindings.json', dict(
        purpose='ISOLATED_COVERAGE_ONLY; uninstrumented v6 library remains output oracle',
        files=records, trace=pin(trace.relative_to(ROOT).as_posix()),
        verification='Removing exactly the declared insertions restores every original UTF-8 source text; no expression/guard/catch/order changed.',
        native='NOT_RUN', product_edits=False))


if __name__ == '__main__':
    main()
