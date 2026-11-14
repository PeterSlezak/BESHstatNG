from common import *
source=(HERE/'investigate_python_delta.py').read_text(encoding='utf-8').replace("text=raw.decode('utf-8-sig')","text=raw.decode('utf-8-sig').replace('\\r\\n','\\n')")
p=HERE/'investigate_python_delta_v2.py'
with p.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/entry-trace-reader-v1.json',dict(status='FAILED_READER_ATTEMPT_RETAINED',reader=pin(HERE/'investigate_python_delta.py'),failure='AssertionError len(matches)==1: bytes.decode retained CRLF while the split delimiter assumed normalized LF',correction='v2 uses explicit CRLF-to-LF for trace splitting only; raw stderr bytes/hashes remain unchanged',source_changes=0))
exec(compile(source,str(p),'exec'))
