from common import *
s=(HERE/'verify_slice.py').read_text(encoding='utf-8').replace("shapeCall['offset']<eh['try_start']<=kernelCall['offset']<eh['try_end']","now['instructions'].index(shapeCall)<eh['try_start']<=now['instructions'].index(kernelCall)<eh['try_end']")
with (HERE/'verify_slice_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/slice-reader-v1.json',dict(status='FAILED_READER_SCHEMA_ASSUMPTION',error='KeyError:offset',correction='Passive metadata canonicalizes branch and EH ranges to semantic instruction indices, not raw byte offsets. V2 checks actual instruction indices against the same canonical EH ranges; no method/branch is dropped.',source=pin(HERE/'verify_slice.py')))
