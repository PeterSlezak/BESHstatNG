// Genuine authenticated Excel Online only. Requires an already-open disposable workbook.
// Run from the repository root: node BESHStat.OfficeAddin/tests/browser/live-edge-workflow.mjs run-label
import { McpClient, sanitizeConsoleFiles } from './live-edge-mcp.mjs';
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { resolve, join } from 'node:path';
const output = resolve('TestResults/04-04/live-edge');
mkdirSync(output, {recursive:true});
const label = process.argv[2] ?? 'workflow';
if (!/^[a-zA-Z0-9-]+$/.test(label)) throw Error('live.invalid-evidence-label');
const devtools = await new McpClient().start('devtools');
const playwright = await new McpClient().start('playwright');
try {
  const listing = await devtools.call('list_pages');
  const pages = listing.content?.filter(c => c.type === 'text').map(c => c.text).join('\n') ?? '';
  const ids = [...pages.matchAll(/^(\d+): .*\(https:\/\/excel\.cloud\.microsoft\//gm)].map(m => Number(m[1]));
  if (ids.length !== 1) throw Error('live.one-disposable-excel-page-required');
  const identity = await devtools.call('evaluate_script', { pageId: ids[0], function: '() => ({edge:navigator.userAgent.includes("Edg/"),brands:navigator.userAgentData?.brands})', waitForStableDom: false });
  const code = readFileSync(new URL('./live-edge-workflow-code.js', import.meta.url), 'utf8').replace('__EVIDENCE_DIRECTORY__', JSON.stringify(output.replaceAll('\\', '/') + '/' + label));
  const run = await playwright.call('browser_run_code_unsafe', { code });
  const text = run.content?.filter(c => c.type === 'text').map(c => c.text).join('\n') ?? '';
  const json = text.match(/### Result\s*\n([^\n]+)/)?.[1];
  const summary = json ? JSON.parse(json) : { passed: false, failure: 'mcp.workflow-return-missing', toolError: run.isError === true, reason: text.split('\n').filter(l => l.startsWith('Error:')).map(l => l.replace(/https?:\/\/\S+/g, '[url-redacted]')) };
  summary.devtoolsIdentity = identity.content?.filter(c => c.type === 'text').map(c => c.text).join('\n');
  writeFileSync(join(output, label + '.json'), JSON.stringify(summary, null, 2) + '\n');
  console.log(JSON.stringify({passed:summary.passed,requestedWorkflowPassed:summary.requestedWorkflowPassed,stage:summary.stage,failure:summary.failure,reason:summary.reason,finalStatus:summary.finalStatus,firstWriteCompleted:summary.firstWriteCompleted,secondWriteCompleted:summary.secondWriteCompleted,freshConsentRequired:summary.freshConsentRequired,transitions:summary.evidence?.transitions,events:summary.evidence?.lifecycle?.length,trace:summary.evidence?.writer?.filter(e=>!e.stage.startsWith('lifecycle')).slice(-22)}, null, 2));
  process.exitCode = summary.passed ? 0 : 1;
} finally { await playwright.close(); await devtools.close(); sanitizeConsoleFiles(join(output, 'tool-console')); }
