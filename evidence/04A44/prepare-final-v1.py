from common44 import *
import re
lock();plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';original=plan.read_bytes();entry=load(HERE/'entry/source-preimages-v1.json');assert pin(plan)==entry['plan']
snapshot=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.20.md';snapshot.write_bytes(original)
text=original.decode('utf-8-sig');lines=text.splitlines();assert 'Revision 2.20' in lines[2];lines[2]='**Revision 2.21 · 15 November 2026 · 04A.43 owner-accepted/merged; 04A.44 in progress**';text='\n'.join(lines)+'\n'
authority='**Current owner authority (04A.44 entry):** Owner accepted04A.43 with documented restrictions and authorized04A.44 only. Verified merged main `'+MAIN+'` is the direct child of `'+PRIOR+'`. **04A.44 IN_PROGRESS**, not accepted/merged; **04A45_NOT_AUTHORIZED**. Portable settings/number/fault/progress decisions preserve Windows store,UI,Help/updater actions and every scientific/native/cache restriction. Exact2.20 snapshot and all226 original IDs retained.\n\n**Historical Revision2.20 entry (retained):**'
text=text.replace('**Current owner authority (04A.43 entry):**',authority,1)
for a,b in [('Phase 04A: 42) | **82**','Phase 04A: 43) | **83**'),('including in-progress 04A.43) | **144**','including in-progress 04A.44) | **143**'),('**42 / 75**','**43 / 75**'),('| Phase 04A not yet accepted | **33**','| Phase 04A not yet accepted | **32**'),('| **04A.43** **IN PROGRESS** |','| **04A.43** **ACCEPTED / MERGED** |'),('| **04A.44** |','| **04A.44** **IN PROGRESS** |')]:assert text.count(a)==1,a;text=text.replace(a,b)
text+='\n**Revision2.21 change log (15 November2026):** Records accepted/merged43 and only44 portable infrastructure extraction. Current summary:04A.01–04A.43 accepted;44 in progress;45 unauthorized. All earlier summaries are historical. No native/scientific/cache behavior activation or original-control discharge;detailed2.12 and charter remain binding.\n'
ids=list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',text,re.M)));assert ids==entry['plan_ids'] and len(ids)==226
plan.write_text(text,encoding='utf-8',newline='\n');save('planning/reconciliation-v1.json',dict(before=entry['plan'],snapshot=pin(snapshot),after=pin(plan),revision='2.21',identities=ids,detailed_Rev2_12_authority=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md'),charter_changed=False,gates=GATES))
pins=runner().__class__.build.__globals__['source_pins']();build=load(PRIVATE/'wave-b-build-v3.json');assert pins==build['source_pins'] and not any(build['exit_codes'])
delta=[dict(before=r,after=pin(r['path'])) for r in entry['files'] if pin(r['path'])!=r];assert len(delta)==7
save('matrix/source-freeze-v1.json',dict(utc=utc(),source_pins=pins,files=[pin(p) for p in sorted(pins)],source_delta=delta,existing_runtime_files=6,test_compile_registration=1,new_production=[pin('BESHStat.Excel.Common/src/Infrastructure/NumberTextPolicy.vb')],new_tests=[pin('BESHStat.Excel.Common.Tests/src/InfrastructurePolicy44Tests.vb'),pin('BESHStatNG.Test/src/InfrastructurePolicy44AdapterTests.vb')],plan=pin(plan),plan_snapshot=pin(snapshot),Debug_build=pin(PRIVATE/'wave-b-build-v3.json'),no_runtime_or_test_changes_after_freeze=True))
source=ROOT/'evidence/04A43'
def recipe(name,edits=[]):
 s=(source/name).read_text(encoding='utf-8-sig').replace('from common43','from common44')
 for a,b in edits:s=s.replace(a,b)
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
recipe('final-managed-v1.py',[('wave-b2-build-v1','wave-b-build-v3'),('a43-wave-b2-v1','a44-wave-b-v3'),('a43-','a44-'),("['UdfHandle','UdfPreparation38','UdfBatch','UdfOutput','UdfCache','UdfNumeric','UdfInput']","['InfrastructurePolicy44','Preference','Settings','Help','Diagnostic','Fault','Execution','Culture','Progress','NumberText']")])
recipe('reconcile-managed-v1.py',[('evidence/04A42/build-and-tests/Debug-identities-v2','evidence/04A43/build-and-tests/Debug-identities-v1'),('ADDED43','ADDED44'),('UdfHandlePolicyTests','InfrastructurePolicy44Tests'),('UdfHandleSessionTests','InfrastructurePolicy44AdapterTests'),('baseline42','baseline43')])
recipe('serial-checks-v1.py',[('a43-','a44-')])
recipe('compatibility-v1.py',[('a43-','a44-')])
recipe('registration-flags-v1.py',[('Attributes43','Attributes44')])
with (HERE/'fixtures/Attributes44.cs').open('xb') as f:f.write((source/'fixtures/Attributes43.cs').read_bytes())
s=(source/'compiled-and-fixtures-v1.py').read_text().replace('from common43','from common44').replace("'a43-wave-b2-v1'","'a44-wave-b-v3'").replace("'a43-final-v1'","'a44-final-v1'").replace('evidence/04A42/abi-cil/','evidence/04A43/abi-cil/').replace('-proof-v3','-proof-v1')
start=s.index("owners=[");end=s.index('\nunexpected=',start)
s=s[:start]+"owners=['AppInfrastructure.AppGlobals','AppInfrastructure.WinFormsProgressReporter','AppInfrastructure.WinFormsUserMessageService','BESHStatNG.UIprocedures','ExcelCommon.PreferenceSnapshot','ExcelCommon.ExecutionSnapshot','ExcelCommon.UserFaultMapper','ExcelCommon.NumberTextPolicy']"+s[end:]
s=s.replace("HERE/'compare43_v3.py'","ROOT/'evidence/04A43/compare43_v3.py'").replace("PRIVATE/'fixtures-before-v3/","ROOT/'.test-rationalization-04A43.tmp/fixtures-before-v3/").replace("PRIVATE/'races-before-v1/","ROOT/'.test-rationalization-04A43.tmp/races-before-v1/").replace("HERE/'scope/cache-export-inventory-v1.json'","ROOT/'evidence/04A43/scope/cache-export-inventory-v1.json'").replace("HERE/'fixtures/volatile-fields-v1.json'","ROOT/'evidence/04A43/fixtures/volatile-fields-v1.json'").replace("HERE/'fixtures/reference-normalization-v3.json'","ROOT/'evidence/04A43/fixtures/reference-normalization-v3.json'")
s+='''\n# Actual44 Windows consumer calls,against immutable pre-edit capture;no runtime exclusions.\na,b,current=execute_fixture('44',PRIVATE/'fixtures-before-v1/Observe44.exe',[],PRIVATE/'fixtures-before-v1/observations.json')\nassert a==b and len(a['rows'])==591\nsave('fixtures/'+config+'-44-v1.json',dict(kind='POST_EDIT_04A44_SAME_ENGINE_COMPATIBILITY',cases=591,exact=True,baseline=pin(PRIVATE/'fixtures-before-v1/observations.json'),current=current,excluded_runtime_fields=[],scientific='NOT_GOLDEN'))\n'''
with (HERE/'compiled-and-fixtures-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Plan2.21/226 IDs/source frozen;one consolidated Debug/Release/compatibility campaign prepared',flush=True)
