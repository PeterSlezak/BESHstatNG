from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';source=ROOT/path;target=HERE/'source-provisional/adapter-tests-failed-compile-v1.vb.txt'
with target.open('xb') as f:f.write(source.read_bytes())
text=source.read_text(encoding='utf-8-sig');start=text.index('    Private Delegate Function Continuation14Guess');prefix=text[:start];tail=text[start:];assert tail.count('OpCodes.')==5;tail=tail.replace('OpCodes.','Global.System.Reflection.Emit.OpCodes.');write_source(path,prefix+tail)
s=HERE/'focused_v1.py';v=s.read_text().replace("(stage+'-focus-bc-v1')","(stage+'-focus-legacy-v2')").replace("('Common','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj',False,'FullyQualifiedName~UdfContinuation14Tests'),",'').replace("stage+'-focus-bc-v1-'","stage+'-focus-legacy-v2-'").replace("'focused-bc-v1-'+stage","'focused-legacy-v2-'+stage").replace('len(results)==4','len(results)==2')
with (HERE/'focused_legacy_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('adapter-compile-correction-v1.json',dict(before=pin(target),after=pin(source),
 reason='The test class already has an Opcodes dictionary. VB is case-insensitive; qualify the five new Reflection.Emit.OpCodes references. No production or existing assertion changed.',first_Common_PASS=35,first_Windows_test_build='FAIL_BC30456_PRESERVED',corrective_scope='Windows focused lane only, distinct paths; retain Common first-pass results without repeating that lane.',original_queries=0))
