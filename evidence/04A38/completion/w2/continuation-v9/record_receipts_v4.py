"""Exact current command/stream pins; distinguish raw failures and later receipts."""
from common_v2 import *
rows=[];artifacts={}
for folder in sorted((RUNROOT/'commands').iterdir()):
    if not folder.is_dir() or not folder.name.startswith('w2c9-') or not (folder/'command.json').is_file():continue
    record=json.loads((folder/'command.json').read_bytes())
    for name in ['stdout','stderr']:assert sha((folder/(name+'.log')).read_bytes())==record[name+'_sha256']
    inputs=[]
    for arg in record['command']:
        if not isinstance(arg,str) or arg.startswith('/'):continue
        p=Path(arg)
        if not p.is_absolute():p=ROOT/p
        if p.is_file():
            key=str(p.resolve())
            if key not in artifacts:artifacts[key]=library_pin(p)
            inputs.append(artifacts[key])
    rows.append(dict(name=folder.name,**clean(record),command_receipt=pin(folder/'command.json'),raw_stdout=pin(folder/'stdout.log'),raw_stderr=pin(folder/'stderr.log'),direct_file_arguments=inputs))
assert rows and not any('vstest' in ' '.join(r['command']).lower() and 'Release' in ' '.join(r['command']) for r in rows)
save('command-receipts-v1.json',dict(commands=rows,scope='Raw streams retained unchanged privately. Only command-description machine paths are cleaned in this review copy. Publication/copied-reader/post-seal commands occur later with independent external receipts.'))
save('attempt-accounting-v1.json',dict(status='FIRST_ATTEMPTS_RETAINED_NOT_WAIVED',attempts=[dict(name='Missing earlier reference rejection harness',classification='INHERITED_READER_ALREADY_FAILED_CLOSED',receipt='control-harness-correction-v1.json',outcome='Missing None bytes raised inherited TypeError; the first harness did not catch that rejection. Only this directed control accepts that error in v4; other TypeErrors remain failures.'),dict(name='Interrupted parent-keyed nonpass history aggregation',classification='BOOKKEEPING_SIZE_DEFECT_CORRECTED_WITH_EXACT_EVENT_LINKS',receipt='failed-aggregation-attempt-v1.json',correction='aggregation-correction-crosswalk-v1.json',outcome='Interrupted partial retained unchanged; raw test artifacts complete. No test rerun or assertion change.'),dict(name='First nonpass message investigator',classification='ADDITIONAL_HISTORICAL_SCOPE_AND_FIRST_SOURCE_PIN_DELTAS_REQUIRE_PROOF',receipt='python-additional-message-source-proof-v1.json',outcome='Initial fail-closed investigator rejected missing delta categories. Revised investigator proves exact projected source bytes and existing assertion path sets; all10 changes retained as failures.'),
dict(name='Read-only source display',classification='TOOLING_DISPLAY_ONLY',outcome='One oversized read was truncated, then owner Python cp1252 could not print Greek. Complete bodies were reread with UTF8 and authenticated spans. One private-draft inspection expression was rejected by PowerShell quoting before execution; structured patching followed. No source execution or change.',raw='Conversation tool receipts'),
dict(name='New lower-bound adapter assertion',classification='NEW_ASSERTION_CORRECTED_FROM_UNCHANGED_ACCEPTED_TRANSPORT',first='focused-v3-packet-v1-Debug.json',correction='focused-corrective-v1.json',source='new-lower-bound-test-correction-v1.json',outcome='Original transport CopyValues normalizes bounds; only the new test assumption changed. First5 PASS/1 FAIL and corrected1 PASS remain distinct.'),
dict(name='Source mapper diagnostics v1/v2',classification='SIGNATURE_ONLY_CALLBACK_LOOKUP_REJECTED',outcome='Identical lambda headers under a parent cannot identify a body. Final mapper preserves exact existing IDs first and otherwise requires unique full body/parent/signature matching; no fabricated alias.'),
dict(name='Metadata current scope v4',classification='SCOPE_RECONCILED_NOT_SOURCE_DELETION',outcome='Two unchanged local report helpers became uncalled by moved phases. v5 adds their real reporting module as a measurement root; no original method removed. Both old measurements retained.'),
dict(name='Historical callback v1',classification='OVERBROAD_FIELD_IDENTITY_ASSUMPTION_REJECTED',outcome='Category callback actually uses only singleton I and cacheI43-0. Their source/CIL/ctor/caller identities are exact. Other older32/33 versus81/90 cache differences stay raw, outside this path and unchanged since C8; UdfDataImport stays OPEN.',corrected='historical_callback_v2.py'),
dict(name='Ownership first candidate',classification='UNREVIEWED_CONTAINER_CLOSURE_REJECTED',outcome='A declaration with closed methods is insufficient to close unrelated Distribution containers. Only the three fully proved packet containers are eligible; root namespace source owner spelling is explicitly bound to the real compiled type.',corrected='build_ownership_v3.py'),
dict(name='Reader template v1',classification='COUNTER_IMPLEMENTATION_DEFECT_CORRECTED',outcome='Counter(dict) interpreted record values as counts. Corrected keys comparison in v2; no underlying identity drift. All drafts retained.'),
dict(name='Closure terminology',classification='DRAFT_FIELD_WORDING_QUALIFIED',outcome='Five existing callbacks newly supported, zero new callback declarations. Authoritative new-closures-v3 keeps exact same body/ID sets and explicitly separates those quantities.')],full_Debug_campaigns=1,full_Python_campaigns=1,Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',historical_assertions_changed=0,original_queries=0))
print('Current command raw streams authenticated',len(rows))
