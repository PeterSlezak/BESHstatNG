async (page) => {
  const evidenceDirectory = __EVIDENCE_DIRECTORY__;
  const pages = page.context().pages().filter(p => p.url().startsWith('https://excel.cloud.microsoft'));
  if (pages.length !== 1) throw Error('live.one-disposable-excel-page-required');
  const p = pages[0];
  let f; const origins = [];
  const attachDeadline = Date.now() + 15000;
  while (!f && Date.now() < attachDeadline) {
    for (const candidate of p.frames()) {
      const origin = await candidate.evaluate(() => location.origin).catch(() => 'unavailable');
      if (!origins.includes(origin)) origins.push(origin);
      if (origin === 'https://localhost:7141') { f = candidate; break; }
    }
    if (!f) await p.waitForTimeout(100);
  }
  if (!f) return {passed:false,failure:'live.addin-frame-missing',origins};
  const result = { genuineExcel: true, simulated: false, passed: false, stage: 'readiness', screenshots: [] };
  let screenshotSession;
  const status = () => f.locator('#pilot-status code').textContent();
  const settle = () => f.waitForFunction(() => document.querySelector('.analysis-pilot')?.getAttribute('aria-busy') === 'false', null, { timeout: 35000 });
  const requireCode = async expected => { const actual = await status(); if (actual !== expected) throw Error(`live.expected-${expected}:actual-${actual}`); };
  const click = name => f.getByRole('button', { name, exact: true }).click();
  const refs = () => f.locator('#input-reference,#output-start').evaluateAll(es => es.map(e => e.value));
  const shot = async name => {
    await f.locator(await f.locator('#overwrite-heading').count() ? '#overwrite-heading' : '#pilot-status').scrollIntoViewIfNeeded();
    screenshotSession ??= await p.context().newCDPSession(p);
    const metrics = await screenshotSession.send('Page.getLayoutMetrics');
    const zoom = metrics.cssVisualViewport.zoom;
    const box = await (await f.frameElement()).boundingBox();
    if (!box || !(zoom > 0)) throw Error('live.screenshot-geometry-unavailable');
    // Edge remote debugging reports layout CSS bounds; screenshot clips use the
    // page's zoomed coordinates. Capture only the add-in, including at 80% zoom.
    const clip = { x: box.x * zoom, y: box.y * zoom, width: box.width * zoom, height: box.height * zoom };
    const host = f.parentFrame();
    await p.screenshot({ path: evidenceDirectory + '-' + name + '.png', clip,
      mask: [f.locator('dd'), host.getByRole('main'), host.getByRole('banner')] });
    result.screenshots.push(name + '.png');
  };
  const phase = async value => { result.stage = value; await f.evaluate(value => { globalThis.__livePhase = value; }, value); };
  try {
    result.edge = await p.evaluate(() => navigator.userAgent.includes('Edg/'));
    result.platform = await f.evaluate(() => Office.context.platform);
    if (!result.edge || result.platform !== 'OfficeOnline' || await f.locator('#host-code').textContent() !== 'office.excel-ready') throw Error('live.host-not-genuine-edge-excel');
    if ((await f.locator('#pilot-status code').count() ? await status() : '') === 'output.mutation-uncertain' ||
        (await f.locator('main').innerText()).includes('Do not retry this write.')) throw Error('live.uncertain-write-requires-review-and-reopen');
    screenshotSession = await p.context().newCDPSession(p);
    await f.evaluate(async () => {
      if (!globalThis.__liveEvidence) {
        globalThis.__liveEvidence = { transitions: [], errors: [], lifecycle: [] };
        new MutationObserver(() => {
          const code = document.querySelector('#pilot-status code')?.textContent, a = globalThis.__liveEvidence.transitions;
          if (code && a.at(-1)?.code !== code) a.push({ ms: Math.round(performance.now()), code });
        }).observe(document.body, { childList: true, subtree: true, characterData: true });
        window.addEventListener('error', e => globalThis.__liveEvidence.errors.push({ kind: 'error', local: e.filename?.startsWith('https://localhost:7141'), line: e.lineno }));
        window.addEventListener('unhandledrejection', () => globalThis.__liveEvidence.errors.push({ kind: 'unhandledrejection' }));
        globalThis.__liveEventOwners = [];
        await Excel.run(async c => {
          const s = c.workbook.worksheets;
          for (const [name, event] of [['changed',s.onChanged],['selection',s.onSelectionChanged],['activated',s.onActivated],['format',s.onFormatChanged],['name',s.onNameChanged],['deleted',s.onDeleted],['added',s.onAdded],['protection',s.onProtectionChanged],['workbookActivated',c.workbook.onActivated]])
            globalThis.__liveEventOwners.push(event.add(() => globalThis.__liveEvidence.lifecycle.push({ ms: Math.round(performance.now()), event: name, phase: globalThis.__livePhase })));
          await c.sync();
        });
      }
      globalThis.__liveEvidence = { transitions: [], errors: [], lifecycle: [] };
      (await import('/js/productionWriter.mjs')).enableWriterTrace(true);
    });
    await phase('first-run');
    await f.locator('#analysis-group').selectOption('Parametric');
    await f.locator('#analysis-method').selectOption('anova.oneway');
    await f.locator('#anova-welch').uncheck(); await f.locator('#anova-fisher').uncheck();
    await f.locator('#anova-bonferroni').uncheck(); await f.locator('#anova-games').uncheck();
    await f.locator('#anova-tukey').check();
    await f.locator('#group-arrangement').selectOption('0');
    await f.locator('#group-headers').selectOption('0');
    await f.locator('#input-reference').fill("'Hárok 1'!A1:C4");
    result.occupiedBeforeFirstWrite = await f.evaluate(async () => Excel.run(async c => {
      const r = c.workbook.worksheets.getItem('Hárok 1').getRange('J1'); r.load('valueTypes'); await c.sync(); return r.valueTypes[0][0] !== 'Empty';
    }));
    if (!result.occupiedBeforeFirstWrite) throw Error('live.fixture-J1-must-already-be-occupied');
    const sourceDigest = () => f.evaluate(async () => Excel.run(async c => {
      const r = c.workbook.worksheets.getItem('Hárok 1').getRange('A1:C4');r.load('values,numberFormat');await c.sync();
      const hash = await crypto.subtle.digest('SHA-256',new TextEncoder().encode(JSON.stringify([r.values,r.numberFormat])));
      return [...new Uint8Array(hash)].map(b=>b.toString(16).padStart(2,'0')).join('');
    }));
    const originalSource = await sourceDigest();
    await click('Run analysis'); await settle(); await requireCode('analysis.completed');
    const beforeTables = await f.locator('.analysis-pilot li').count();
    await f.locator('#output-destination').selectOption('0');
    await f.locator('#output-start').fill("'Hárok 1'!J1");
    await phase('first-inspection');
    await click('Write results'); await settle(); await requireCode('output.occupancy-occupied');
    await shot('workflow-first-consent');
    await phase('first-overwrite');
    await click('Yes, overwrite inspected output'); await settle(); await requireCode('output.completed');
    result.firstWriteCompleted = true;
    const boundReferences = await refs();
    await phase('option-edit');
    await f.locator('#anova-welch').check(); await f.locator('#anova-fisher').check();
    await phase('second-run');
    await click('Run analysis'); await settle(); await requireCode('analysis.completed');
    result.unchangedReferences = JSON.stringify(boundReferences) === JSON.stringify(await refs());
    result.additionalTables = await f.locator('.analysis-pilot li').count() > beforeTables;
    if (!result.unchangedReferences || !result.additionalTables) throw Error('live.options-not-recalculated-with-same-references');
    await shot('workflow-recalculated');
    await phase('second-inspection');
    await click('Write results'); await settle(); await requireCode('output.occupancy-occupied');
    result.freshConsentRequired = await f.getByRole('button', { name: 'Yes, overwrite inspected output', exact: true }).isEnabled();
    if (!result.freshConsentRequired) throw Error('live.fresh-consent-not-enabled');
    await shot('workflow-fresh-consent');
    result.requestedWorkflowPassed = true;
    await phase('second-overwrite');
    await click('Yes, overwrite inspected output'); await settle(); await requireCode('output.completed');
    result.secondWriteCompleted = true; result.sourceUnchanged = await sourceDigest() === originalSource;
    if (!result.sourceUnchanged) throw Error('live.source-changed');
    result.passed = true;
  } catch (e) {
    result.failure = e.message.startsWith('live.') ? e.message : 'live.automation-failed';
    await shot('workflow-failure').catch(() => {});
  }
  result.finalStatus = await status().catch(() => null);
  result.evidence = await f.evaluate(async () => ({ ...globalThis.__liveEvidence, writer: (await import('/js/productionWriter.mjs')).getWriterTrace() }));
  await screenshotSession?.detach();
  return result;
}
