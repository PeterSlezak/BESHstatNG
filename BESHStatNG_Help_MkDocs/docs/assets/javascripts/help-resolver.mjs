// Runs only in the documentation-origin tab. No workbook data, telemetry or cache.
import { root, routes } from './help-routes.mjs';

export const home = root + 'latest/';
export const resolver = home + 'help-resolver/';
const maxBytes = 512 * 1024;

export function validVersion(value) {
    return typeof value === 'string' && /^(0|[1-9][0-9]{0,4})(\.(0|[1-9][0-9]{0,4})){3}$/.test(value) &&
        value.split('.').every(part => Number(part) <= 65535) && value !== '0.0.0.0';
}

export function requestTargets(search) {
    // Do not decode percent escapes or accept duplicate/reordered/extra parameters.
    // Route identifiers, never paths/domains, must match the generated catalog exactly.
    const match = typeof search === 'string' && /^\?version=([0-9.]{0,32})&route=([A-Za-z0-9_]{1,128})$/.exec(search);
    if (!match || !Object.hasOwn(routes, match[2])) return Object.freeze({ latest: home, version: null, valid: false });
    const relative = routes[match[2]];
    return Object.freeze({ latest: home + relative,
        version: validVersion(match[1]) ? root + 'v' + match[1] + '/' + relative : null, valid: true });
}

export function usableDocument(html, expectedUrl) {
    if (typeof html !== 'string') return false;
    // Existing MkDocs Material pages (including older published builds) provide
    // canonical identity. A root page, custom 404 or unrelated landing is not proof.
    const links = html.match(/<link\b[^>]*>/gi) || [];
    const canonical = links.filter(tag => /\brel\s*=\s*["']canonical["']/i.test(tag));
    if (canonical.length !== 1) return false;
    const href = /\bhref\s*=\s*["']([^"']*)["']/i.exec(canonical[0]);
    if (!href || href[1] !== expectedUrl) return false;
    const article = /<article\b[^>]*\bclass\s*=\s*["'][^"']*\bmd-content__inner\b[^"']*["'][^>]*>([\s\S]*?)<\/article>/i.exec(html);
    const heading = article && /<h1\b[^>]*>([\s\S]*?)<\/h1>/i.exec(article[1]);
    const title = heading && heading[1].replace(/<[^>]*>/g, '').trim();
    return !!title && !/404|410|not found|page cannot|page can.?t|oops!/i.test(title);
}

function expectedResponse(response, url) {
    return response && response.status === 200 && response.url === url && !response.redirected &&
        response.type !== 'opaque' && response.type !== 'opaqueredirect' &&
        /^text\/html(?:\s*;|$)/i.test(response.headers?.get('content-type') || '');
}

async function readBounded(response) {
    if (!response.body?.getReader) return null;
    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let bytes = 0, html = '';
    try {
        for (;;) {
            const { done, value } = await reader.read();
            if (done) return html + decoder.decode();
            bytes += value.byteLength;
            if (bytes > maxBytes) return null;
            html += decoder.decode(value, { stream: true });
        }
    } finally {
        // Cancellation itself must not keep the bounded result waiting.
        Promise.resolve(reader.cancel()).catch(() => {});
    }
}

export async function pageAvailable(url, fetchPage, timeoutMs = 1500) {
    const controller = new AbortController();
    let timer;
    const options = { signal: controller.signal, mode: 'same-origin', redirect: 'error',
        credentials: 'omit', cache: 'no-store', referrerPolicy: 'no-referrer' };
    const attempt = async () => {
        try {
            const head = await fetchPage(url, { ...options, method: 'HEAD' });
            if (!head || ![200, 405, 501].includes(head.status) || head.url !== url || head.redirected ||
                head.type === 'opaque' || head.type === 'opaqueredirect') return false;
            // HEAD alone cannot distinguish a custom HTTP-200 404. Confirm actual
            // HTML identity with a bounded GET, also used when HEAD is unsupported.
            const response = await fetchPage(url, { ...options, method: 'GET' });
            if (!expectedResponse(response, url)) return false;
            return usableDocument(await readBounded(response), url);
        } catch { return false; }
    };
    try {
        return await Promise.race([attempt(), new Promise(resolve => {
            timer = setTimeout(() => { controller.abort(); resolve(false); }, timeoutMs);
        })]);
    } finally { clearTimeout(timer); controller.abort(); }
}

export async function resolveHelp(search, { fetchPage = globalThis.fetch, timeoutMs = 1500 } = {}) {
    const targets = requestTargets(search);
    if (!targets.valid) return Object.freeze({ url: home, state: 'invalid' });
    if (!targets.version) return Object.freeze({ url: targets.latest, state: 'latest' });
    if (await pageAvailable(targets.version, fetchPage, timeoutMs)) return Object.freeze({ url: targets.version, state: 'version' });
    if (await pageAvailable(targets.latest, fetchPage, timeoutMs)) return Object.freeze({ url: targets.latest, state: 'latest' });
    // Stay on the accessible resolver with a home link rather than redirecting in
    // a loop or pretending that an unverified page is available while offline.
    return Object.freeze({ url: home, state: 'unavailable' });
}

export async function openHelp(search, { navigate, ...options }) {
    const result = await resolveHelp(search, options);
    if (result.state !== 'unavailable') navigate(result.url);
    return result;
}

export async function startResolver(document, location, fetchPage) {
    const status = document.getElementById('help-resolver-status');
    const latest = document.getElementById('help-resolver-latest');
    if (!status || !latest) return;
    if (location.origin + location.pathname !== resolver) {
        status.textContent = 'Version checking is available on the published Help site. Use latest Help below.';
        return;
    }
    latest.href = requestTargets(location.search).latest;
    try {
        const result = await openHelp(location.search, { fetchPage, navigate: url => location.replace(url) });
        if (result.state === 'unavailable') {
            latest.href = home;
            status.textContent = 'Help could not be confirmed. You may be offline. Try latest Help below.';
        }
    } catch {
        latest.href = home;
        status.textContent = 'Help is unavailable. Try latest Help below.';
    }
}

if (typeof document !== 'undefined' && typeof location !== 'undefined') {
    void startResolver(document, location, globalThis.fetch);
}
