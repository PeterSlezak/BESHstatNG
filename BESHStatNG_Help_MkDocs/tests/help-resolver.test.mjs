import test from 'node:test';
import assert from 'node:assert/strict';
import { root, routes } from '../docs/assets/javascripts/help-routes.mjs';
import { home, resolver, requestTargets, validVersion, usableDocument, pageAvailable,
    resolveHelp, openHelp, startResolver } from '../docs/assets/javascripts/help-resolver.mjs';

const query = (version = '1.2.6.0', route = 'OneWayANOVA') => `?version=${version}&route=${route}`;
const html = url => `<link rel="canonical" href="${url}"><article class="md-content__inner md-typeset"><h1>One-Way ANOVA</h1></article>`;
function response(url, status = 200, content = html(url), extra = {}) {
    const result = new Response(content, { status, headers: { 'content-type': 'text/html; charset=utf-8' } });
    Object.defineProperty(result, 'url', { value: url });
    for (const [key, value] of Object.entries(extra)) Object.defineProperty(result, key, { value });
    return result;
}
function fake(handler = (url, options) => response(url, 200, options.method === 'HEAD' ? null : html(url))) {
    const calls = [];
    const fetchPage = async (url, options) => { calls.push({ url, ...options }); return handler(url, options); };
    return { fetchPage, calls };
}

test('79 generated routes are immutable and never include resolver/navigation input', () => {
    assert.equal(Object.keys(routes).length, 79);
    assert.equal(routes.OneWayANOVA, 'methods/one-way-anova/');
    assert.equal(routes.KruskalWallisTest, 'methods/kruskal-wallis-test/');
    assert.equal(routes.ExportChart, 'export-chart/');
    assert.equal(routes.GlobalSettings, 'global-settings/');
    assert.equal(routes.Home, '');
    assert.throws(() => { routes.Home = 'https://evil.test/'; }, TypeError);
    for (const path of Object.values(routes)) assert.match(path, /^(?:|(?:methods\/)?[a-z0-9]+(?:-[a-z0-9]+)*\/)$/);
});

for (const version of ['1.2.5.0', '1.2.6.0']) {
    for (const route of ['OneWayANOVA', 'KruskalWallisTest', 'GlobalSettings', 'ExportChart', 'Histogram', 'Home']) {
        test(`${version} ${route}: confirmed exact document opens that version`, async () => {
            const transport = fake();
            const targets = requestTargets(query(version, route));
            const navigated = [];
            const result = await openHelp(query(version, route), { ...transport, navigate: url => navigated.push(url) });
            assert.equal(result.state, 'version');
            assert.deepEqual(navigated, [root + 'v' + version + '/' + routes[route]]);
            assert.deepEqual(transport.calls.map(c => [c.url, c.method]), [[targets.version, 'HEAD'], [targets.version, 'GET']]);
            for (const call of transport.calls) {
                assert.equal(call.mode, 'same-origin');
                assert.equal(call.redirect, 'error');
                assert.equal(call.credentials, 'omit');
                assert.equal(call.referrerPolicy, 'no-referrer');
                assert.ok(call.signal instanceof AbortSignal);
            }
        });
    }
}

for (const status of [404, 410, 301, 302, 403, 500, 204]) {
    test(`version status ${status}: only corresponding latest document is considered`, async () => {
        const targets = requestTargets(query());
        const transport = fake((url, options) => response(url, url === targets.version ? status : 200,
            status === 204 && url === targets.version || options.method === 'HEAD' ? null : html(url)));
        const result = await resolveHelp(query(), transport);
        assert.deepEqual(result, { state: 'latest', url: targets.latest });
        assert.equal(transport.calls[0].url, targets.version);
        assert.ok(transport.calls.slice(1).every(c => c.url === targets.latest));
    });
}

test('version root 200 is never a substitute for a missing method page', async () => {
    const targets = requestTargets(query());
    const transport = fake((url, options) => response(url, url === targets.version ? 404 : 200, options.method === 'HEAD' ? null : html(url)));
    assert.equal((await resolveHelp(query(), transport)).url, targets.latest);
    assert.ok(!transport.calls.some(c => c.url === root + 'v1.2.6.0/'));
    assert.ok(!transport.calls.some(c => c.url.includes('v1.2.5.0')));
});

for (const final of ['latest', 'home', 'external', 'same-path-redirect']) {
    test(`version redirects to ${final} never confirm version identity`, async () => {
        const targets = requestTargets(query());
        const redirected = final === 'latest' ? targets.latest : final === 'home' ? home : final === 'external' ? 'https://evil.test/' : targets.version;
        const transport = fake((url, options) => url === targets.version ? response(redirected, 200, null, { redirected: true }) : response(url, 200, options.method === 'HEAD' ? null : html(url)));
        assert.equal((await resolveHelp(query(), transport)).state, 'latest');
    });
}

for (const status of [405, 501]) {
    test(`HEAD ${status} uses GET, final path and document checks`, async () => {
        const transport = fake((url, options) => response(url, options.method === 'HEAD' ? status : 200, options.method === 'HEAD' ? null : html(url)));
        assert.equal((await resolveHelp(query('1.2.5.0'), transport)).state, 'version');
        assert.deepEqual(transport.calls.map(c => c.method), ['HEAD', 'GET']);
    });
    test(`HEAD ${status} followed by GET error must fall back`, async () => {
        const targets = requestTargets(query());
        const transport = fake((url, options) => response(url, options.method === 'HEAD' ? status : url === targets.version ? 404 : 200,
            options.method === 'HEAD' ? null : html(url)));
        assert.equal((await resolveHelp(query(), transport)).state, 'latest');
    });
}

for (const failure of ['soft404', 'landing', 'json', 'wrong-final-url', 'opaque', 'empty', 'empty-heading', 'missing-status', 'duplicate-canonical', 'too-large']) {
    test(`unusable version document ${failure} cannot be treated as published`, async () => {
        const targets = requestTargets(query());
        const transport = fake((url, options) => {
            if (url !== targets.version || options.method === 'HEAD') return response(url, 200, options.method === 'HEAD' ? null : html(url));
            if (failure === 'soft404') return response(url, 200, html(url).replace('One-Way ANOVA', '404 - Not found'));
            if (failure === 'landing') return response(url, 200, html(home));
            if (failure === 'json') return new Response('{}', { headers: { 'content-type': 'application/json' } });
            if (failure === 'wrong-final-url') return response(home);
            if (failure === 'opaque') return response(url, 200, html(url), { type: 'opaque' });
            if (failure === 'empty') return response(url, 200, '');
            if (failure === 'empty-heading') return response(url, 200, html(url).replace('One-Way ANOVA', ''));
            if (failure === 'missing-status') return {};
            if (failure === 'duplicate-canonical') return response(url, 200, html(url) + `<link rel="canonical" href="${url}">`);
            return response(url, 200, html(url) + 'x'.repeat(512 * 1024));
        });
        assert.equal((await resolveHelp(query(), transport)).state, 'latest');
    });
}

test('network/CORS-like error falls back; same-origin request mode is preserved', async () => {
    const targets = requestTargets(query());
    const transport = fake((url, options) => {
        if (url === targets.version) throw new TypeError('Failed to fetch');
        return response(url, 200, options.method === 'HEAD' ? null : html(url));
    });
    assert.equal((await resolveHelp(query(), transport)).url, targets.latest);
    assert.ok(transport.calls.every(c => c.mode === 'same-origin'));
});

test('version deadline aborts a hung request even if injected fetch ignores cancellation', async () => {
    const targets = requestTargets(query());
    const transport = fake((url, options) => url === targets.version ? new Promise(() => {}) : response(url, 200, options.method === 'HEAD' ? null : html(url)));
    const result = await resolveHelp(query(), { ...transport, timeoutMs: 10 });
    assert.equal(result.url, targets.latest);
    assert.ok(transport.calls[0].signal.aborted);
});

test('streaming GET body has the same short deadline', async () => {
    const targets = requestTargets(query());
    const transport = fake((url, options) => {
        if (url === targets.version && options.method === 'GET') {
            const result = new Response(new ReadableStream({ pull() { return new Promise(() => {}); } }), { headers: { 'content-type': 'text/html' } });
            Object.defineProperty(result, 'url', { value: url });
            return result;
        }
        return response(url, 200, options.method === 'HEAD' ? null : html(url));
    });
    assert.equal((await resolveHelp(query(), { ...transport, timeoutMs: 10 })).state, 'latest');
});

for (const route of ['OneWayANOVA', 'Home']) {
    test(`${route}: both pages missing retains accessible resolver without redirect loop`, async () => {
        const transport = fake(url => response(url, 404));
        const navigated = [];
        const result = await openHelp(query('1.2.6.0', route), { ...transport, navigate: url => navigated.push(url) });
        assert.deepEqual(result, { url: home, state: 'unavailable' });
        assert.deepEqual(navigated, []);
        assert.equal(transport.calls.length, 2);
    });
}

test('both requests time out without navigation or leaked active timers', async () => {
    const transport = fake(() => new Promise(() => {}));
    const result = await resolveHelp(query(), { ...transport, timeoutMs: 10 });
    assert.equal(result.state, 'unavailable');
    assert.equal(transport.calls.length, 2);
    assert.ok(transport.calls.every(c => c.signal.aborted));
});

for (const input of [null, '', '?version=1.2.6.0&route=../', '?version=1.2.6.0&route=OneWayANOVA%2f',
    '?version=1.2.6.0&route=https://evil.test/', '?version=1.2.6.0&route=javascript:alert(1)',
    '?version=1.2.6.0&route=Home&next=https://evil.test', '?version=1.2.6.0&route=Home#evil',
    '?version=1.2.6.0&route=Home\\evil', '?version=1.2.6.0&route=constructor',
    '?version=1.2.6.0&route=__proto__', '?version=1.2.6.0&route=аnova',
    '?version=1.2.6.0&route=help-resolver', '?route=Home&version=1.2.6.0',
    '?version=1.2.6.0&route=Home&route=OneWayANOVA', '?version=%31.2.6.0&route=Home']) {
    test(`reject raw malicious/unknown resolver input ${JSON.stringify(input)}`, async () => {
        const transport = fake(() => assert.fail('Invalid input must not issue HTTP'));
        const navigated = [];
        const result = await openHelp(input, { ...transport, navigate: url => navigated.push(url) });
        assert.equal(result.url, home);
        assert.deepEqual(navigated, [home]);
        assert.equal(transport.calls.length, 0);
    });
}

for (const value of ['', '1.2.6', '0.0.0.0', '1.2.6.65536', '01.2.6.0', '1.2.6.0.1']) {
    test(`invalid version ${JSON.stringify(value)} never invents a version candidate`, async () => {
        assert.equal(validVersion(value), false);
        const transport = fake(() => assert.fail('No HTTP needed for invalid version'));
        const result = await resolveHelp(query(value), transport);
        assert.deepEqual(result, { state: 'latest', url: home + routes.OneWayANOVA });
    });
}

test('bounds and canonical versions', () => {
    for (const value of ['0.0.0.1', '65535.65535.65535.65535']) assert.equal(validVersion(value), true);
    for (const value of ['1.2.-1.0', '1.2.6.0 ', '１.2.6.0']) assert.equal(validVersion(value), false);
    assert.ok(Object.isFrozen(requestTargets(query())));
});

test('boot does not probe from a task pane, local preview or other origin', async () => {
    const status = {}, latest = { href: home };
    await startResolver({ getElementById: id => id.endsWith('status') ? status : latest },
        { origin: 'https://office-pilot.test', pathname: '/taskpane/', search: query() },
        () => assert.fail('No CORS-dependent task-pane fetch'));
    assert.match(status.textContent, /published Help site/);
    assert.equal(latest.href, home);
});

test('boot follows confirmed URL with replace and no popup API', async () => {
    const status = {}, latest = {};
    const navigated = [];
    await startResolver({ getElementById: id => id.endsWith('status') ? status : latest },
        { origin: 'https://www.beshstat.eu', pathname: '/beshstatng/help/latest/help-resolver/',
            search: query('1.2.5.0'), replace: url => navigated.push(url) }, fake().fetchPage);
    assert.deepEqual(navigated, [root + 'v1.2.5.0/methods/one-way-anova/']);
    assert.equal(latest.href, home + routes.OneWayANOVA);
});

test('usable HTML requires exact canonical and a real document heading', () => {
    const url = requestTargets(query()).version;
    assert.equal(usableDocument(html(url), url), true);
    assert.equal(usableDocument(html(home), url), false);
    assert.equal(usableDocument('<h1>OK</h1>', url), false);
    assert.equal(usableDocument(html(url).replace('<h1>', '<h2>').replace('</h1>', '</h2>'), url), false);
    assert.equal(resolver, home + 'help-resolver/');
});
