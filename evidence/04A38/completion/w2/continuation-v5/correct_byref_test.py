from common import *
path='BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb'
dst=HERE/'attempts/common-tests-before-byref-fixture-fix.vb.txt';dst.write_bytes((ROOT/path).read_bytes())
text=(ROOT/path).read_text(encoding='utf-8-sig')
old='UdfCellConversion38.CellToTrimmedText(input))'
assert old in text
write_source(path,text.replace(old,'UdfCellConversion38.CellToTrimmedText(CType(input, Object)))'))
save('attempts/byref-test-compile.json',dict(before=pin(dst),after=pin(ROOT/path),reason='A typed rectangular array passed ByRef Object requires explicit widening to suppress an Option Strict copy-back conversion in this new direct portable test. Native consumers already use local Object. No production change.',
    receipt='focused-cells-v4-Debug.json',production_change=False))
text=(HERE/'focused.py').read_text(encoding='utf-8').replace("('tests-'+suite)","('tests-v2-'+suite)").replace("('results-'+suite)","('results-v2-'+suite)")
text=text.replace("command(stage+", "command(stage+'-tests-v2'+").replace("save('focused-'+stage+'-'+config", "save('focused-'+stage+'-v2-'+config")
with (HERE/'focused_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE/'run_full_validation_v2.py').read_text(encoding='utf-8').replace('w2c5-validation-v2','w2c5-validation-v3').replace('full-managed-Debug-v2.json','full-managed-Debug-v3.json').replace('common-python-validation-v2.json','common-python-validation-v3.json')
with (HERE/'run_full_validation_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
