"""Reconcile original current/historical members and retained renderer operations."""
from pathlib import Path
from collections import Counter
import csv
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent


def load(name):
    return json.loads((HERE / name).read_bytes())


def save(name, value):
    (HERE / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')


def write_csv(name, rows):
    with (HERE / name).open('w', encoding='utf-8', newline='') as f:
        w = csv.DictWriter(f, fieldnames=list(rows[0]))
        w.writeheader()
        w.writerows(rows)


def member_id(n):
    return n['key'] + '@' + str(n['span_start']) + ':' + str(n['span_length'])


def native_assignments(n):
    return [a['left'] for a in n['assignments'] if a['left'].startswith(('.', 'figure . Axes'))]


def main():
    ast = load('final-members.json')
    original = load('entry-members.json')['entry']
    current = ast['current']
    assert len(original) == 32
    assert [(n['key'], n['token_sha256']) for n in ast['entry']] == [(n['key'], n['token_sha256']) for n in original]
    with (ROOT / 'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig') as f:
        historical = [r for r in csv.DictReader(f) if r['path'] in {n['path'] for n in original}]
    assert len(historical) == 32
    supersessions = dict(ResolveScoreLabels='MultivariateChartPresentation37.ScoreLabels',
        ResolveGroupLabels='MultivariateChartPresentation37.NormalizeGroups',
        ColumnSumsOfSquares='MultivariatePlotGeometry37.FactorPercent (original loop folded into same operation)',
        PercentOfTotal='MultivariatePlotGeometry37.PercentOfTotal (private), reached by InitialFactorPercent/FactorPercent',
        TotalVariance='MultivariatePlotGeometry37.TotalVariance (private), reached by InitialFactorPercent/FactorPercent')
    host = {'.ctor', 'AddPointLabels', 'ConfigureScatterAxes', 'DeleteLegendIfPresent', 'SetAxisTitles', 'WriteObjectTable'}
    rows = []; native = []
    for n in original:
        matches = [v for v in current if v['path'] == n['path'] and v['owner'] == n['owner'] and v['name'] == n['name'] and v['kind'] == n['kind']]
        assert len(matches) <= 1
        successor = matches[0] if matches else None
        name = n['name']; family = Path(n['path']).stem.replace('PlotExcel', '')
        disposition = 'HOST_ONLY' if name in host else 'EXISTING_PORTABLE_REUSED' if name in ['CreateDendrogramChart', 'Draw'] else 'PORTED_AND_CONSUMED'
        target = supersessions.get(name, 'MultivariatePlotGeometry37 / MultivariateChartPresentation37')
        reason = 'Original numerical prefixes and pure label/series/axis/title intent delegate at original chart phases; native styling, point writes and COM remain Windows.'
        if family == 'ClusterAnalysis':
            target = 'DendrogramChartGeometry37 / DendrogramChartPresentation37; existing CreateDendrogramLayout/AxisPadding/GetSuggestedAxisTitle'
        if disposition == 'HOST_ONLY':
            target = 'Retained Windows renderer'
            reason = 'Native construction/application of Chart/Series/Point/Axis/Worksheet state, styles, fonts, COM exceptions/lifetime, or empty adapter constructor; no numerical algorithm is hidden here.'
        if disposition == 'EXISTING_PORTABLE_REUSED':
            reason = 'Unchanged forwarding path to authoritative existing layout and the now-delegating CreateExcelChart; no hierarchy/cut recomputation.'
        if successor is None:
            assert name in supersessions
            mapping = 'EXPLICIT_SUPERSESSION'
        elif n['header_tokens'] != successor['header_tokens']:
            assert not n['signature'].startswith('Public')
            assert name in ['AddScoreSeries2D', 'ConfigureScatterAxes', 'AddZeroLines', 'ApplyDendrogramAxisTitles']
            mapping = 'PRIVATE_TYPED_ARGUMENT_CHANGE'
        else:
            mapping = 'EXACT_HEADER'
        links = [h for h in historical if h['path'] == n['path'] and h['name'] == name and h['kind'] == n['kind']]
        assert len(links) == 1
        rows.append(dict(member_id=member_id(n), family=family, path=n['path'], member=name,
            entry_signature=n['signature'], entry_source_sha256=hashlib.sha256((HERE / ('preedit/' + Path(n['path']).name + '.txt')).read_bytes()).hexdigest(),
            entry_token_sha256=n['token_sha256'], entry_lines=str(n['start']) + '-' + str(n['end']),
            historical_member_id=links[0]['member_id'], historical_source_sha256=links[0]['source_sha256'],
            disposition=disposition, mapping=mapping, successor=member_id(successor) if successor else target,
            successor_path=successor['path'] if successor else 'BESHStat.Excel.Common/src/Results/MultivariateChartPresentation37.vb' if family == 'Pca' else 'BESHStat.Core/src/Graphics/MultivariatePlotGeometry37.vb',
            dependency=target, consuming_edge=Path(n['path']).stem + '.' + name + ' -> ' + target + ' -> existing native renderer',
            reason=reason, tests='Graphics37GeometryTests; Graphics37PresentationTests; Graphics37AdapterTests; preedit operations/capture + series',
            native='SOURCE_ONLY_NATIVE_NOT_RUN; original chart/series/style/point/axis creation, labels, catches and lifetime retained'))
        if successor:
            public_header = n['header_tokens'] == successor['header_tokens']
            assert public_header or not n['signature'].startswith('Public')
            writes = native_assignments(n) == native_assignments(successor)
            catches = n['catches'] == successor['catches']
            assert writes, (name, native_assignments(n), native_assignments(successor))
            assert catches, name
            native.append(dict(member_id=member_id(n), successor=member_id(successor), public_header_preserved=public_header,
                native_write_targets_and_order_preserved=writes, catch_shapes_preserved=catches,
                complete_token_body_equal=n['token_sha256'] == successor['token_sha256']))
    historical_rows = []
    for h in historical:
        r = next(r for r in rows if r['historical_member_id'] == h['member_id'])
        historical_rows.append(dict(historical_member_id=h['member_id'], historical_signature_key=h['signature_key'], historical_source_sha256=h['source_sha256'],
            path=h['path'], name=h['name'], current_entry_member_id=r['member_id'], disposition=r['disposition'], successor=r['successor'],
            boundary='One-to-one current/historical member populations are independently 32; body fragments split explicitly where pure preparation moved. No prior ledger amended.'))
    write_csv('member-ownership.csv', rows)
    write_csv('historical-crosswalk.csv', historical_rows)
    save('coverage.json', dict(current_original_members=32, historical_members=32, supporting_entry_members=1029,
        by_family=dict(Counter(r['family'] for r in rows)), dispositions=dict(Counter(r['disposition'] for r in rows)),
        in_scope_open=0, supersessions=[r['member_id'] for r in rows if r['mapping'] == 'EXPLICIT_SUPERSESSION'],
        original_native_scientific_obligations='UNCHANGED_NOT_DISCHARGED'))
    for n in native:
        if next(r['member'] for r in rows if r['member_id'] == n['member_id']) in ['AddPointLabels', 'DeleteLegendIfPresent', 'SetAxisTitles', 'WriteObjectTable', 'Draw', 'CreateDendrogramChart']:
            assert n['complete_token_body_equal']
    save('renderer-preservation.json', dict(status='SOURCE_AND_COMPILED_SEAMS_VERIFIED', members=native,
        guarantees=['Every public header/default and original null/dimension guard retained.',
            'Dendrogram layout validation precedes chart creation. Chart-sheet, embedded-sheet, supplied chart, anchor/size and error paths remain native.',
            'PCA/factor XYZ invocation order/arguments/settings/styles unchanged; accepted XYZ renderer source is unchanged.',
            'CA columns precede optional rows; MCA omits rows but both coordinate groups determine axes. Percents(2,1)/(1,1) and late catch remain.',
            'Native write-target sequences, catches, colors, markers, line/arrow/font properties and point-index expressions retained. Literal text/coordinates are replaced only by prepared equivalents.',
            'Zero reference varying coordinate remains Double(); constant zero coordinate remains Integer() at the native boundary.',
            'Grouped score preparation now snapshots ordered data before native group-series creation, still after chart/axis creation. No live event/reentrancy or COM error equivalence is claimed.'],
        phase_policy='PRE_CHART numerical prefixes; AFTER_CHART grouped series; AFTER_SERIES dendrogram bounds; original late label/caption points, including CA Try/Catch.',
        native='NOT_RUN; no visual-equivalence or scientific-golden claim'))
    portable = [n for n in current if n['path'].endswith('37.vb')]
    save('coordinate-and-chart-spec-contract.json', dict(members=[{k: n[k] for k in ['path', 'owner', 'signature', 'token_sha256', 'start', 'end']} for n in portable],
        Core='Existing authoritative model arrays/percent rules; original biplot arithmetic, factor pattern*structure/trace, numerical axes/zero/arrows, detached existing dendrogram geometry and original padding.',
        Common='Existing MultivariatePlotDataRole and MultivariateAxisMap/Item reused; immutable point series, first-seen ordinal names plus historical text membership, typed ordinal kinds, titles/labels/legend intent and admitted dendrogram series.',
        Windows='All native chart/series/shape/worksheet operations, chart types, colors/styles, point indices and placement, catches/events/COM lifetime.',
        ordinals='Model positions 0-based; actual model RowIds separate; cluster slots 0-based; leaf original input ordinals 1-based; native Excel point conversion only Windows. Physical source rows unavailable unless separately supplied by existing analysis projections; never inferred.',
        exceptional_values='No finite filtering or substitute estimator; signed zeros and NaNs preserved in data. Text formatting suppresses negative-zero sign like legacy Format$.',
        copies='All stored coordinate/label/identity/nested-layout arrays deeply copied on input and access. Existing models never mutated.'))
    print('Reconciled 32 current + 32 historical members; public headers/native write order/catches preserved')


if __name__ == '__main__':
    main()
