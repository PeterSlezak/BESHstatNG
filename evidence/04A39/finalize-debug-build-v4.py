"""Recompile only the changed new Legacy test; reuse identical already-built production/portable graphs."""
from common39 import *
import shutil
source_lock()
run=runner();old=load(HERE/'build-and-tests/Debug-build-v2.json')
now=run.__class__.build.__globals__['source_pins']()
changes={k for k in old['source_pins'].keys()|now.keys() if old['source_pins'].get(k)!=now.get(k)}
assert changes=={'BESHStatNG.Test/src/UDFs/UdfInputBoundary39Tests.vb'}
assert not any(old['exit_codes']) and old['source_unchanged']
before=PRIVATE/'builds/a39v2/Debug';after=PRIVATE/'builds/a39v4/Debug'
shutil.copytree(before,after)
authority=load(PRIVATE/'commands/a39v2-Debug-build-Legacy/command.json')
args=[a.replace(str(before),str(after)) for a in authority['command']]
receipt=command('Debug-final-new-test-compile-v4',args)
save('build-and-tests/Debug-test-source-correction-v4.json',dict(previous=pin(HERE/'build-and-tests/Debug-build-v2.json'),
    changed_paths=sorted(changes),command=receipt,
    reason='New test wrongly expected custom alpha fallback for invalid text. Actual accepted ParseAlphaOrDefault throws; correction asserts exact exception. No inherited test or production change.',
    source_unchanged_during_compile=now==run.__class__.build.__globals__['source_pins']()))
assert receipt['exit_code']==0
result=dict(old,label='a39v4',source_pins=now,source_unchanged=True,output=str(after/'portable'),
    graph_provenance='Eight unchanged graph steps retained from verified a39v2; only changed Legacy test assembly recompiled with exact prior CLI/path adaptation.',
    final_test_compile=receipt)
save('build-and-tests/Debug-build-v4.json',result)
save(after/'build.json',result) if False else None
# The copied raw build.json is preserved; final authoritative version is the additive public report.
folder=PRIVATE/'shipping-final/Debug';folder.mkdir(parents=True,exist_ok=False)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
    shutil.copy2(after/'legacy'/name,folder/name)
save('abi-cil/Debug-shipping-v4.json',dict(assemblies=[pin(p) for p in sorted(folder.glob('*.dll'))]))
