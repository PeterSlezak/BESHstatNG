from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
before=pin(ROOT/path);entry=next(r for r in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before'] if r['live']['path']==path)
assert before==entry['live']
source=(ROOT/path).read_bytes().decode('utf-8-sig').replace('\r\n','\n');tail='End Class\n'
assert source.endswith(tail)
addition=(HERE/'test-additions/adapter-v1.vb.txt').read_text(encoding='utf-8')
write_source(path,source[:-len(tail)]+addition+'\n'+tail)
save('adapter-test-addition-v1.json',dict(before=before,preimage=entry['preimage'],addition=pin(HERE/'test-additions/adapter-v1.vb.txt'),after=pin(ROOT/path),old_assertions='EXACT_PREFIX_UNCHANGED',original_queries=0))
