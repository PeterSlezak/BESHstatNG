// Attach the already-installed MCP server packages to the user's existing Edge.
// No package installation, browser launch, fixture injection, or profile copying.
import { spawn } from 'node:child_process';
import { readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
export function edgeEndpoint() {
  const [port, path] = readFileSync(join(process.env.LOCALAPPDATA, 'Microsoft/Edge/User Data/DevToolsActivePort'), 'utf8').trim().split(/\r?\n/);
  if (!/^\d+$/.test(port) || !path?.startsWith('/devtools/browser/')) throw Error('edge.debugging-unavailable');
  return `ws://127.0.0.1:${port}${path}`;
}
export class McpClient {
  pending = new Map(); serial = 0; buffer = '';
  async start(kind) {
    const packagePath = kind === 'devtools' ? 'chrome-devtools-mcp/build/src/bin/chrome-devtools-mcp.js' : '@playwright/mcp/cli.js';
    const cache = join(process.env.LOCALAPPDATA, 'npm-cache/_npx');
    const entry = readdirSync(cache).map(d => join(cache, d, 'node_modules', packagePath)).find(p => { try { readFileSync(p); return true; } catch { return false; } });
    if (!entry) throw Error('mcp.cached-package-unavailable');
    const args = kind === 'devtools' ? [`--wsEndpoint=${edgeEndpoint()}`, '--no-usage-statistics', '--no-performance-crux'] : [`--cdp-endpoint=${edgeEndpoint()}`, '--console-level=error', '--output-dir=' + join(process.cwd(), 'TestResults/04-04/live-edge/tool-console')];
    this.child = spawn(process.execPath, [entry, ...args], { cwd: process.cwd(), windowsHide: true, env: { ...process.env, CHROME_DEVTOOLS_MCP_NO_UPDATE_CHECKS: '1' }, stdio: ['pipe', 'pipe', 'pipe'] });
    this.child.stderr.on('data', () => {}); // Never log arbitrary host/browser diagnostics.
    this.child.stdout.on('data', b => { this.buffer += b; let i; while ((i = this.buffer.indexOf('\n')) >= 0) { const line = this.buffer.slice(0, i); this.buffer = this.buffer.slice(i + 1); try { this.receive(JSON.parse(line)); } catch {} } });
    this.child.on('exit', () => { for (const p of this.pending.values()) p.reject(Error('mcp.server-exited')); this.pending.clear(); });
    await this.request('initialize', { protocolVersion: '2025-03-26', capabilities: { roots: { listChanged: false } }, clientInfo: { name: 'beshstat-live-edge', version: '1' } });
    this.send({ jsonrpc: '2.0', method: 'notifications/initialized' });
    return this;
  }
  send(message) { this.child.stdin.write(JSON.stringify(message) + '\n'); }
  receive(m) {
    if (m.method === 'roots/list') { this.send({ jsonrpc: '2.0', id: m.id, result: { roots: [{ uri: pathToFileURL(process.cwd()).href, name: 'BESHStatNG' }] } }); return; }
    const p = this.pending.get(m.id); if (!p) return; this.pending.delete(m.id); clearTimeout(p.timer);
    m.error ? p.reject(Error('mcp.request-failed:' + m.error.message)) : p.resolve(m.result);
  }
  request(method, params) { return new Promise((resolve, reject) => { const id = ++this.serial; const timer = setTimeout(() => { this.pending.delete(id); reject(Error('mcp.request-timeout:' + method)); }, 60000); this.pending.set(id, { resolve, reject, timer }); this.send({ jsonrpc: '2.0', id, method, params }); }); }
  call(name, args = {}) { return this.request('tools/call', { name, arguments: args }); }
  async close() {
    if (!this.child || this.child.exitCode !== null) return;
    await new Promise(resolve => { this.child.once('exit', resolve); this.child.stdin.end(); const timer = setTimeout(resolve, 6000); timer.unref(); });
  }
}
export function sanitizeConsoleFiles(folder) {
  let files; try { files = readdirSync(folder); } catch { return; }
  for (const name of files.filter(n => /^console-.*\.log$/.test(n))) {
    const path = join(folder, name), raw = readFileSync(path, 'utf8');
    try { if (JSON.parse(raw).sanitized === true) continue; } catch {}
    const messages = raw.split(/\r?\n/).filter(l => /\[(ERROR|WARNING|WARN|INFO|LOG|DEBUG)\]/i.test(l)).map(l => ({
      level: l.match(/\[(ERROR|WARNING|WARN|INFO|LOG|DEBUG)\]/i)[1].toLowerCase(),
      local: l.includes('https://localhost:7141'),
      exceptionType: l.match(/\b(TypeError|ReferenceError|SyntaxError|RangeError)\b/)?.[1] ?? null
    }));
    writeFileSync(path, JSON.stringify({ sanitized: true, rawMessagesDiscarded: true, originalLineCount: raw.split(/\r?\n/).length, messages }, null, 2) + '\n');
  }
}
if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  const client = await new McpClient().start(process.argv[2]);
  try {
    const args = process.argv[4] ? JSON.parse(readFileSync(process.argv[4], 'utf8')) : {};
    const result = await client.call(process.argv[3], args);
    for (const c of result.content ?? []) if (c.type === 'text') console.log(c.text.replace(/https?:\/\/[^\s"<>]+/g, s => { try { const u = new URL(s.replace(/[)`]+$/, '')); return u.origin + u.pathname; } catch { return '[url-redacted]'; } }));
  } finally { client.close(); }
}
