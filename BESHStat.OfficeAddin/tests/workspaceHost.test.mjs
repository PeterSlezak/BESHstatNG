import { test } from 'node:test';
import assert from 'node:assert/strict';
import { configureHelp, safeHelp, displayLocale, openHelp, copyLog } from '../wwwroot/js/workspaceHost.mjs';
const home = 'https://beshstat.eu/help/latest/';
const resolver = home + 'help-resolver/';
configureHelp([home, home + 'global-settings/'], resolver, true, ['Home', 'GlobalSettings', 'OneWayANOVA', 'KruskalWallisTest']);
test('Office display language wins; browser and missing-API fallbacks are safe', () => {
    assert.equal(displayLocale({ Office: { context: { displayLanguage: 'sk-SK' } }, navigator: { language: 'en-US' } }), 'sk-SK');
    assert.equal(displayLocale({ Office: { context: { displayLanguage: 'en-US' } }, navigator: { language: 'sk-SK' } }), 'en-US');
    assert.equal(displayLocale({ navigator: { language: 'sk-SK' } }), 'sk-SK'); assert.equal(displayLocale({}), 'en');
    assert.equal(displayLocale({ get Office() { throw new Error('private'); } }), 'en');
});
test('Help prefers the user-activated Office bridge and reports only requested', () => {
    const calls = []; const env = { Office: { context: { ui: { openBrowserWindow: url => calls.push(url) } } }, open() { throw new Error('must use Office'); } };
    assert.equal(openHelp(home, env), 'requested'); assert.deepEqual(calls, [home]);
});
for (const route of ['Home', 'GlobalSettings', 'OneWayANOVA', 'KruskalWallisTest']) test(`Version-first published resolver allowed: ${route}`, () => {
    assert.equal(safeHelp(`${resolver}?version=1.2.6.0&route=${route}`), true);
});
for (const url of ['javascript:alert(1)', 'http://beshstat.eu/help/latest/', 'https://evil.example/', home + '../private',
    `${resolver}?version=1.2.6.0&route=unknown`, `${resolver}?version=01.2.6.0&route=Home`, `${resolver}?version=0.0.0.0&route=Home`,
    `${resolver}?version=65536.2.6.0&route=Home`, `${resolver}?version=1.2.6.0&route=Home&url=https://evil.example`,
    'https://beshstat.eu/help/v1.2.6.0/']) test(`arbitrary Help target rejected: ${url}`, () => {
    assert.equal(openHelp(url, { open() { throw new Error('must never launch'); } }), 'rejected');
});
test('blocked Office launch does not try hidden popup bypass', () => {
    assert.equal(openHelp(home, { Office: { context: { ui: { openBrowserWindow() { throw new Error('private'); } } } } }), 'blocked');
    assert.equal(openHelp(home, { Office: { context: {} }, open() { throw new Error('must not bypass'); } }), 'unavailable');
});
test('standalone fallback distinguishes allowed from blocked popup', () => {
    const opened = {}; assert.equal(openHelp(home, { open: () => opened }), 'requested'); assert.equal(opened.opener, null);
    assert.equal(openHelp(home, { open: () => null }), 'blocked');
});
test('allowlist cannot be replaced by later callers', () => {
    configureHelp(['https://evil.example/'], 'https://evil.example/', true, ['Home']);
    assert.equal(safeHelp('https://evil.example/'), false);
});
test('clipboard failure is fixed safe outcome; bounded export only', async () => {
    let copied; assert.equal(await copyLog('sanitized', { navigator: { clipboard: { writeText: text => { copied = text; } } } }), true);
    assert.equal(copied, 'sanitized'); assert.equal(await copyLog('sanitized', {}), false);
    assert.equal(await copyLog('x'.repeat(131073), {}), false);
});
