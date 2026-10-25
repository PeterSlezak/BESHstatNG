import { test } from 'node:test';
import assert from 'node:assert/strict';
import { preferenceKey, maximumPreferenceBytes, loadPreferences, savePreferences, inspectPreferenceJson } from '../wwwroot/js/preferences.mjs';
const data = () => ({ schemaVersion: 1, defaultAlpha: 0.01, defaultRandomSeed: 12345, traceEnabled: false,
    pValues: { decimalPlaces: 4, smallValueDisplay: 1, useUpperBound: false } });
function environment() {
    const values = new Map([['other.application', 'private']]);
    return { values, localStorage: { getItem: k => values.get(k) ?? null, setItem: (k, v) => values.set(k, v),
        clear() { throw new Error('clear is prohibited'); }, removeItem() { throw new Error('deletion is prohibited'); } } };
}
test('Save → independent pane/reload → Load exactly restores Common DTO and touches one key', () => {
    const env = environment(); assert.equal(loadPreferences(env).status, 'missing');
    assert.equal(savePreferences(data(), undefined, env).status, 'saved');
    const reopened = { localStorage: env.localStorage };
    assert.deepEqual(loadPreferences(reopened), { status: 'loaded', data: data(), forceEnglish: true });
    assert.equal(env.values.size, 2); assert.equal(env.values.get('other.application'), 'private');
});
for (const [name, raw, status] of [
    ['malformed', '{', 'invalid'], ['future', '{"schemaVersion":2}', 'unsupported'], ['old', '{"schemaVersion":0}', 'unsupported'],
    ['unknown root field', '{"schemaVersion":1,"worksheet":"PRIVATE"}', 'invalid'],
    ['unknown display field', '{"schemaVersion":1,"pValues":{"extra":1}}', 'invalid'],
    ['missing version', '{}', 'invalid'], ['array', '[]', 'invalid'], ['null', 'null', 'invalid'],
    ['string alpha', '{"schemaVersion":1,"defaultAlpha":"0.05"}', 'invalid'],
    ['infinite alpha', '{"schemaVersion":1,"defaultAlpha":1e999}', 'invalid'],
    ['fraction seed', '{"schemaVersion":1,"defaultRandomSeed":1.5}', 'invalid'],
    ['large seed', '{"schemaVersion":1,"defaultRandomSeed":2147483648}', 'invalid'],
    ['string trace', '{"schemaVersion":1,"traceEnabled":"false"}', 'invalid'],
    ['display array', '{"schemaVersion":1,"pValues":[]}', 'invalid'],
    ['huge data', ' '.repeat(maximumPreferenceBytes + 1), 'invalid'],
    ['string forceEnglish', '{"schemaVersion":1,"forceEnglish":"false"}', 'invalid'],
    ['null forceEnglish', '{"schemaVersion":1,"forceEnglish":null}', 'invalid'],
    ['numeric forceEnglish', '{"schemaVersion":1,"forceEnglish":0}', 'invalid']
]) test(`${name}: load and save preserve existing bytes`, () => {
    const env = environment(); env.values.set(preferenceKey, raw);
    assert.equal(loadPreferences(env).status, status); assert.equal(savePreferences(data(), false, env).status, status);
    assert.equal(env.values.get(preferenceKey), raw);
});
for (const candidate of [{ schemaVersion: 1 }, { schemaVersion: 1, defaultAlpha: 0 }, { schemaVersion: 1, defaultRandomSeed: 0 },
    { schemaVersion: 1, pValues: { decimalPlaces: 99, smallValueDisplay: 99 } }, { schemaVersion: 1, traceEnabled: null, pValues: null }])
    test(`Optional/bounded numeric values are left for Common normalization: ${JSON.stringify(candidate)}`, () => {
        assert.deepEqual(inspectPreferenceJson(JSON.stringify(candidate)), { status: 'loaded', data: candidate, forceEnglish: true });
    });
test('security exception on storage getter is safe', () => {
    const env = { get localStorage() { throw new Error('PRIVATE token'); } };
    assert.deepEqual(loadPreferences(env), { status: 'unavailable' }); assert.deepEqual(savePreferences(data(), false, env), { status: 'unavailable' });
});
test('quota error retains old data and no fake success', () => {
    const env = environment(); env.values.set(preferenceKey, JSON.stringify(data()));
    env.localStorage.setItem = () => { throw new Error('QuotaExceededError PRIVATE'); };
    assert.equal(savePreferences({ ...data(), defaultAlpha: .02 }, false, env).status, 'unavailable');
    assert.equal(loadPreferences(env).data.defaultAlpha, .01);
});
test('silent write discard fails verification', () => {
    const env = environment(); env.localStorage.setItem = () => {};
    assert.equal(savePreferences(data(), false, env).status, 'unavailable');
});
test('absent storage is unavailable without throwing', () => {
    assert.equal(loadPreferences({}).status, 'unavailable'); assert.equal(savePreferences(data(), false, {}).status, 'unavailable');
});
test('old v1 defaults ON without modifying any existing Common preferences or bytes', () => {
    const env = environment(), raw = JSON.stringify(data()); env.values.set(preferenceKey, raw);
    assert.deepEqual(loadPreferences(env), { status: 'loaded', data: data(), forceEnglish: true });
    assert.equal(env.values.get(preferenceKey), raw);
});
for (const flag of [false, true]) test(`forceEnglish=${flag} saves and reloads atomically with Common values`, () => {
    const env = environment(); assert.equal(savePreferences(data(), flag, env).status, 'saved');
    assert.deepEqual(loadPreferences({ localStorage: env.localStorage }), { status: 'loaded', data: data(), forceEnglish: flag });
    assert.deepEqual(JSON.parse(env.values.get(preferenceKey)), { ...data(), forceEnglish: flag });
});
test('Common-only save preserves previously accepted host override', () => {
    const env = environment(); savePreferences(data(), false, env);
    assert.equal(savePreferences({ ...data(), defaultAlpha: .02 }, undefined, env).status, 'saved');
    assert.equal(loadPreferences(env).forceEnglish, false); assert.equal(loadPreferences(env).data.defaultAlpha, .02);
});
test('verification exception restores exact prior bytes rather than leaking a failed language Save', () => {
    const env = environment(), raw = JSON.stringify(data()); env.values.set(preferenceKey, raw);
    const original = env.localStorage.getItem; let reads = 0;
    env.localStorage.getItem = key => { if (++reads === 2) throw Error('security'); return original(key); };
    assert.equal(savePreferences(data(), false, env).status, 'unavailable'); assert.equal(env.values.get(preferenceKey), raw);
});
