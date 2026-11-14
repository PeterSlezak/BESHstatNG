"""Append-only compiler correction. No typed case or sealed source is changed."""
from datetime import datetime,timezone
from inspect_entry import ROOT,HERE,pin,save

def main():
 old=(HERE/'GeeCapture38.cs').read_text(encoding='utf-8')
 a='var parameters=(string[])gee.GetMethod("BuildParameterNames"'
 b='if(!parameters.Contains("X1"))'
 c='parameter_names=parameters,'
 assert old.count(a)==old.count(b)==old.count(c)==1
 new=old.replace(a,a.replace('parameters=','fittedParameters=')).replace(b,'if(!fittedParameters.Contains("X1"))').replace(c,'parameter_names=fittedParameters,')
 with (HERE/'GeeCapture38-v2.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(new)
 log=ROOT/'.test-rationalization-04A38.tmp/commands/gee38-v2-original-Debug-plain-recorder/stdout.log'
 text=log.read_text(encoding='utf-8').replace(str(ROOT),'<REPO_ROOT>')
 with (HERE/'attempt-recorder-compile-v1.txt').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
 save('recorder-tooling-seal.json',dict(status='RECORDER_TOOLING_SEALED_BEFORE_FIRST_REFERENCE',sequence='1.1',sealed_utc=datetime.now(timezone.utc).isoformat(),
      input_seal=pin(HERE/'input-seal.json'),files=[pin(HERE/'GeeCapture38-v2.cs'),pin(HERE/'correct_recorder_compile.py'),pin(HERE/'execute_original.py')],
      correction='CS0136 local name shadowing; rename fitted parameter local in recorder only. Original sealed recorder retained byte-for-byte.',
      old_recorder=pin(HERE/'GeeCapture38.cs'),failed_compile_log=pin(HERE/'attempt-recorder-compile-v1.txt'),
      typed_input_changes=0,baseline_source_changes=0,model_executions=0,query_executions=0,current_executions=0))
if __name__=='__main__':main()
