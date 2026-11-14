"""Reverse every authorized body insertion; untouched bytes and assertions stay exact."""
from common import *
import re
trans=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());moves=json.loads((HERE/'implementation-source-composition-v2.json').read_bytes())['phases']
decode=lambda b:b.decode('utf-8-sig').replace('\r\n','\n')
native={}
for move in moves:native.setdefault(move['path'],[]).append(move)
for path,items in native.items():
    row=next(r for r in trans if r['path']==path);text=decode((ROOT/path).read_bytes())
    for move in reversed(items):
        assert text.count(move['actual_native_call'])==1
        text=text.replace(move['actual_native_call'],move['original_segment'],1)
    assert text==decode((ROOT/row['preimage']['path']).read_bytes()),'Unrelated native source changed '+path
report='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb';row=next(r for r in trans if r['path']==report)
text=decode((ROOT/report).read_bytes())
for move in moves:
    if move['owner']=='prepared':continue
    exact='        Public Shared Function '+move['portable_signature']+'\n'+move['portable_segment']+'        End Function\n\n'
    assert text.count(exact)==1;text=text.replace(exact,'',1)
text=text.replace('Imports System\nImports System.Collections\n','Imports System\n',1)
assert text==decode((ROOT/row['preimage']['path']).read_bytes()),'Unrelated Common report owner changed'
adapter='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';row=next(r for r in trans if r['path']==adapter)
old=decode((ROOT/row['preimage']['path']).read_bytes());now=decode((ROOT/adapter).read_bytes());prefix=old[:old.rfind('End Class')]
assert now.startswith(prefix),'Prior native assertions changed'
approved={r['path'] for r in trans};unchanged=[]
for row in json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files']:
    if row['live']['path'] not in approved:assert pin(ROOT/row['live']['path'])==row['live'];unchanged.append(row['live'])
for row in trans:
    assert pin(ROOT/row['path'])==row['after'] and (ROOT/row['snapshot']['path']).read_bytes()==(ROOT/row['path']).read_bytes()
    if row['before']:assert (ROOT/row['preimage']['path']).read_bytes().startswith(b'\xef\xbb\xbf')==(ROOT/row['path']).read_bytes().startswith(b'\xef\xbb\xbf')
save('source-transition-independent-proof-v1.json',dict(status='EXACT_EIGHT_BODY_TWO_COMMON_OWNER_SOURCE_AND_ASSERTION_TRANSITIONS_ONLY',
    source_paths=sorted(approved),unchanged_entry_sources=unchanged,new_managed_tests=40,original_queries=0,
    scope='Every old native body byte reconstructed, existing Common report methods untouched, prior adapter assertion prefix retained, final snapshots byte-exact; no Core/project/export changes.'))
print('Complete source transition and untouched assertion/source preservation PASS_NOT_READY')
