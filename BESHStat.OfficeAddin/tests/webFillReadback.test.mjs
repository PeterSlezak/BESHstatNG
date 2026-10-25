// Office boundary simulation of the representation observed in genuine Edge/Excel Online.
import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { createProductionWriter } from '../wwwroot/js/productionWriter.mjs';
import { presentationMatches } from '../wwwroot/js/tablePresentation.mjs';
import { pilotWorkbook } from './helpers/pilotWorkbook.mjs';
const fixture = JSON.parse(readFileSync(new URL('../../BESHStat.WasmSmoke/tests/fixtures/output-plans/mixed-zero.json', import.meta.url), 'utf8'));

for (const defect of ['none', 'header-color', 'uncleared-fill', 'unknown-pattern', 'missing-pattern', 'desktop-null'])
test(`single-cell web fill readback: ${defect}`, async () => {
    const x = pilotWorkbook(); x.scope.Office.context.platform = defect === 'desktop-null' ? 'PC' : 'OfficeOnline';
    const writer = createProductionWriter(x.scope), pin = await writer.pinSource(x.state.snapshot());
    const plan = structuredClone(fixture); plan.target.destination = 1; plan.target.worksheetName = 'Results';
    for (const t of plan.tables) t.autoFit = false;
    for (const c of plan.tables[0].cells.filter(c => c.row === 0)) c.role = 'TopColumnHeader';
    const intent = { planIdentity: 'web-fill', targetIdentity: 'target', workbookIdentity: pin.sessionIdentity, worksheetIdentity: null,
        worksheetName: 'Results', destination: 1, startRow: plan.target.startRow, startColumn: plan.target.startColumn,
        regions: plan.tables.map(t => ({ artifactId: t.artifactId, bounds: t.bounds, mayMutate: true })), guards: [], reservations: [] };
    await writer.prepareOutput(plan, intent, pin.sourceToken);
    const readback = x.state.syncs + 4;
    let observedBothFills = false;
    x.state.hook = async n => {
        if (n !== readback) return;
        for (const fill of x.fills.values()) { fill.pattern = null; if (fill.color === null) fill.color = ''; }
        const header = [...x.fills.values()].find(f => f.color === '#DDDDDD');
        const empty = [...x.fills.values()].find(f => f.color === '');
        observedBothFills = !!header && !!empty;
        if (defect === 'header-color') header.color = '#FFFFFF';
        if (defect === 'uncleared-fill') empty.color = '#FFFFFF';
        if (defect === 'unknown-pattern') header.pattern = 'Gray25';
        if (defect === 'missing-pattern') header.pattern = undefined;
    };
    const result = await writer.execute(plan, intent, null);
    assert.equal(observedBothFills, true, 'The fixture must exercise both solid and cleared cells.');
    assert.equal(result.code, defect === 'none' ? 'output.completed' : 'output.mutation-uncertain');
    if (defect !== 'none') {
        assert.equal(result.mutationMayHaveOccurred, true);
        assert.equal((await writer.captureSelection(false)).code, 'output.mutation-uncertain');
    }
    await writer.dispose();
});

test('web null pattern normalization requires explicit web host and exact color, never unknown data', () => {
    for (const [fill, color] of [['#DDDDDD', '#dddddd'], [null, '']]) {
        const item = { format: { fill: { pattern: null, color } } }, style = { fill };
        assert.equal(presentationMatches(item, style, 'fill', true), true);
        assert.equal(presentationMatches(item, style, 'fill'), false);
        for (const bad of [null, undefined, 'white']) {
            item.format.fill.color = bad;
            assert.equal(presentationMatches(item, style, 'fill', true), false);
        }
    }
});
