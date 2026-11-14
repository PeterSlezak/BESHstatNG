from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';source=ROOT/path;target=HERE/'source-provisional/adapter-tests-precompile-v1.vb.txt';target.parent.mkdir(exist_ok=True)
with target.open('xb') as f:f.write(source.read_bytes())
text=source.read_text(encoding='utf-8-sig');old='''            Assert.IsTrue(guess(New Object(,) {{"no"}, {"yes"}}, ExcelError.ExcelErrorValue, Continuation14Label))
            Assert.AreEqual("yes", Continuation14Label)''';new=old.replace('Assert.AreEqual("yes", Continuation14Label)','Assert.AreEqual("ExcelErrorValue", Continuation14Label)')
assert text.count(old)==1;write_source(path,text.replace(old,new,1))
save('precompile-test-expectation-correction-v1.json',dict(before=pin(target),after=pin(source),reason='Before first test compilation: authenticated native CellToTrimmedText expressly converts ExcelError to its enum text; the explicit positive token is ExcelErrorValue, rather than inferred yes. Preserve this legacy policy instead of correcting it. No production assertion or historical test changed.',original_queries=0))
