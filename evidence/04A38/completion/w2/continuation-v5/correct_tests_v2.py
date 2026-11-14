from common import *
path = 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
dst = HERE / 'attempts/adapter-tests-v2.vb.txt'
dst.write_bytes((ROOT / path).read_bytes())
text = (ROOT / path).read_text(encoding='utf-8-sig')
text = text.replace('CollectionAssert.AreEqual(New String() {"A", "C"}, names)',
                    '''' An empty source column prevents automatic header detection in the original policy.
            CollectionAssert.AreEqual(New String() {"Group 1", "Group 2"}, names)
            Assert.IsTrue(UdfDataImport.TryExtractNumericGroupsFromMatrix(source, 1, True, "", groups, names))
            CollectionAssert.AreEqual(New String() {"A", "C"}, names)''')
write_source(path, text)
save('attempts/fixture-v2.json', dict(status='FAILED_FIXTURE_RETAINED', before=pin(dst), after=pin(ROOT / path),
    reason='Header detection requires numeric values below every selected column, including a filtered empty column. Corrected inferred-name expectation and separately exercise explicit hasHeader=True.',
    receipt='focused-cells-v2-Debug.json', production_change=False, original_queries=0))
