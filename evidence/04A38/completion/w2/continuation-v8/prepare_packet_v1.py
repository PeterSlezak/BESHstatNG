"""Freeze genuine immediate preimages and offline tools before implementation."""
from common import *
assert (HERE/'entry-v2.json').is_file()
old=['BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
new=['BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb','BESHStat.Excel.Common.Tests/src/UdfContinuation8Tests.vb']
preimages=[]
for name in old:
    target=HERE/'source-before'/(name+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write((ROOT/name).read_bytes())
    preimages.append(dict(live=pin(ROOT/name),preimage=pin(target)))
assert all(not (ROOT/n).exists() for n in new)
save('implementation-plan-v1.json',dict(source_before=preimages,new_sources=[dict(path=n,entry_state='ABSENT') for n in new],source_paths=old+new,queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),scope='A complete eight-entry sample-size orchestration with host-owned conversions/logging and unchanged Core engines/reports; B prepared response/predictor/optional/Cox layout and exact aligned-column copy; no scientific correction',original_queries=0))
tools=[]
for name in ['source_views.py','build_v1.py','focused_v2.py','inspect_stage_v1.py','compatibility_v1.py','inspect_metadata_v3.py','MethodMetadata-v13.cs','native-metadata-owners-v1.txt']:
    p=PREVIOUS/name
    if not p.is_file():continue
    text=p.read_text(encoding='utf-8-sig')
    if name=='build_v1.py':
        text=text.replace('implementation-plan-v2.json','implementation-plan-v1.json').replace("for p in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before']","for p in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before']")
    if name=='inspect_stage_v1.py':text=text.replace('implementation-plan-v2.json','implementation-plan-v1.json')
    if name=='compatibility_v1.py':text=text.replace('W2 continuation-v7','W2 continuation-v8')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    tools.append(dict(before=pin(p),after=pin(HERE/name)))
save('versioned-tool-crosswalk-v1.json',dict(rows=tools,original_queries=0))
print('Frozen3 immediate source preimages and3 absent new paths; offline tools versioned')
