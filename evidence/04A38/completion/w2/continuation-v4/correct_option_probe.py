from common import *
s=(HERE/'OptionBoundaryProbe.cs').read_text(encoding='utf-8').replace('(object[])packet["rows"]','(System.Collections.IEnumerable)packet["rows"]').replace('((object[])spec["choices"]).Cast<string>()','((System.Collections.IEnumerable)spec["choices"]).Cast<string>()')
s=s.replace('static void Main(string[] args){','static void Main(string[] args){try{Execute(args);}catch(Exception e){Console.Error.WriteLine(e.GetType().FullName+": "+e.Message);Environment.Exit(2);}}\n static void Execute(string[] args){')
with (HERE/'OptionBoundaryProbe-v2.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'run_option_probe.py').read_text(encoding='utf-8').replace("'option-probe'","'option-probe-v2'").replace('OptionBoundaryProbe.cs','OptionBoundaryProbe-v2.cs').replace("'option-probe-","'option-probe-v2-")
with (HERE/'run_option_probe_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'report_managed.py').read_text(encoding='utf-8').replace('parametric-v1','parametric-v4').replace('NEW_CONTINUATION_V3','NEW_CONTINUATION_V4').replace('INHERITED_C2','INHERITED_C3')
with (HERE/'report_managed_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/option-probe-v1.json',dict(exit_code=3762504530,error='System.InvalidCastException: JavaScriptSerializer materialized ArrayList, not Object[]',correction='V2 accepts IEnumerable and has a top-level diagnostic catch to avoid unhandled-process reporting delays. No option method or original semantic query was reached in failed setup.',source=pin(HERE/'OptionBoundaryProbe.cs')))
