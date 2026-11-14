from common import *
for path, snippet in [('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb', 'common-tests.vb.txt'),
                      ('BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb', 'adapter-tests.vb.txt')]:
    source = (ROOT / path).read_text(encoding='utf-8-sig')
    assert source.rstrip().endswith('End Class')
    index = source.rindex('End Class')
    write_source(path, source[:index] + (HERE / snippet).read_text(encoding='utf-8') + '\n' + source[index:])
save('tests-added.json', dict(ordinary_edge_compatibility=['Invariant/sk-SK', 'legacy type whitelist and rounding overflow',
     'native sentinels and Framework Err', 'noncontiguous retained columns, copied names/data, rectangular lower bounds',
     'callback order/exception/ByRef reset', 'alpha invariant-first parsing and failed-parse ByRef'], new_original_queries=0))
