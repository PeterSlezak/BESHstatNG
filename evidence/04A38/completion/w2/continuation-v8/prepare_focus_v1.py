"""Version focused runner and corrected append command, retaining failed first attempt."""
from common import *
text=(HERE/'add_adapter_tests_v1.py').read_text(encoding='utf-8')
text=text.replace("assert source.count('End Class')==1\nwrite_source(path,source.replace('End Class',tests+'\\nEnd Class'))", "assert source.rstrip().endswith('End Class')\nhead,separator,tail=source.rpartition('End Class')\nwrite_source(path,head+tests+'\\nEnd Class'+tail)")
text=text.replace("save('adapter-test-addition-v1.json'", "save('adapter-test-addition-v2.json'")
with (HERE/'add_adapter_tests_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
text=(HERE/'focused_v2.py').read_text(encoding='utf-8')
text=text.replace('focus-v2','focus-v3').replace('focused-v2','focused-v3')
text=text.replace('FullyQualifiedName~UdfNumericPreparation38Tests','FullyQualifiedName~SampleSize')
text=text.replace('FullyQualifiedName~UdfModelProfilePreparation38Tests|FullyQualifiedName~UdfPreparation38Tests','FullyQualifiedName~UdfContinuation8Tests|FullyQualifiedName~SampleSize')
with (HERE/'focused_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
save('focus-runner-crosswalk-v1.json',dict(original=pin(HERE/'focused_v2.py'),current=pin(HERE/'focused_v3.py'),append_before=pin(HERE/'add_adapter_tests_v1.py'),append_corrected=pin(HERE/'add_adapter_tests_v2.py'),failed_append_exit=1,reason='Existing test file has nested classes; select the final class terminator. First attempt changed no source.',original_queries=0))
