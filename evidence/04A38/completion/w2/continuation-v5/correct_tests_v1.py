from common import *
path = 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
source = ROOT / path
before = source.read_bytes()
dst = HERE / 'attempts/adapter-tests-v1.vb.txt'
dst.parent.mkdir(exist_ok=True)
dst.write_bytes(before)
text = source.read_text(encoding='utf-8-sig')
text = text.replace('For Each value As Object In {Nothing, ExcelEmpty.Value, ExcelMissing.Value}',
                    'For Each value As Object In New Object() {Nothing, ExcelEmpty.Value, ExcelMissing.Value}')
text = text.replace('For Each value As Object In {ExcelError.ExcelErrorNA, True, "1.5", "", DBNull.Value, New Object(,) {{1.0R}}}',
                    'For Each value As Object In New Object() {ExcelError.ExcelErrorNA, True, "1.5", "", DBNull.Value, New Object(,) {{1.0R}}}')
assert text != before.decode('utf-8-sig')
write_source(path, text)
save('attempts/compile-v1.json', dict(status='FAILED_RETAINED', reason='Two new mixed Object array literals need explicit New Object() under the Framework compiler; Common tests passed',
    retained_test=pin(dst), correction=pin(source), production_change=False,
    receipt='focused-cells-v1-Debug.json'))
