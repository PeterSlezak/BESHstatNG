import sys
from pathlib import Path
sys.path.insert(0,str(Path.cwd()/'evidence/04A38/completion/w2/continuation-v13'))
from common import *
source=PRIVATE/'bookkeeping-code-v1/probe_current_byref_alias_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace('current-byref-alias-probe-v1','current-byref-alias-probe-v2').replace('probe-compile-v1','probe-compile-v2').replace('probe-run-v1','probe-run-v2')
value=value.replace('public static string Label;','public static string Label; public static int Conversions;')
value=value.replace('public override string ToString() { Label=', 'public override string ToString() { Conversions++; Label=')
value=value.replace('conversionSideEffect="converted",','conversionSideEffect="converted", conversionCalls=Conversions,')
value=value.replace('(!result && Label==null)','(!result && Label==null && Conversions==1)')
target=PRIVATE/'bookkeeping-code-v1/probe_current_byref_alias_v2.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
result=command('current-byref-explicit-observer-driver-v2',[sys.executable,'-B',str(target)]);assert result['exit_code']==0
with (PRIVATE/'current-byref-alias-probe-v2/observer-source-transition.json').open('x',encoding='utf-8') as stream:json.dump(dict(before=pin(source),after=pin(target),scope='Add explicit conversion-call counter to the first current-only probe; retain v1 bytes and outcomes. No original/reference execution or new scientific authority.'),stream,indent=2)
