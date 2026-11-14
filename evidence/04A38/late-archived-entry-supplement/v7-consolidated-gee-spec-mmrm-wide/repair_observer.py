"""Append-only recorder repair; approved inputs and original libraries unchanged."""
from datetime import datetime,timezone
from common import *
from verify_inputs import inspect
inspect();assert not (HERE/'original-output-seal.json').exists() and not (HERE/'current-results').exists()
source=HERE/'MmrmCapture38.cs';text=source.read_text(encoding='utf-8');assert text.count('if(property("Result")==null')==1
text=text.replace('if(property("Result")==null','if(property(ActiveFamily=="GEE"?"Model":"Result")==null')
with (HERE/'MmrmCapture38-v2.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
save('attempt-original-recorder-v1.json',dict(status='FAILED_ORIGINAL_OBSERVATION_NO_OUTPUT_SEAL_NO_CURRENT',
 reason='Recorder asked for nonexistent GEE cache Result property; original declares Model. Actual fit allocated a real cached handle and public query reached intended guard, but recorder-fit operation was EXCEPTION and GEE session receipt missing. These files cannot count as accepted reference.',
 failed_files=[pin(p) for p in sorted((HERE/'original-results').rglob('*')) if p.is_file()],source=pin(source),original_Dlls_changed=False,typed_inputs_changed=False,current_executions=0,original_attempt_executions_per_configuration=12))
receipts=[]
facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
for config in ['Debug','Release']:
 for mode in ['plain','trace']:
  folder=PRIVATE/('original-'+config+'-'+mode+'-v2');exe=folder/'MmrmCapture38-v2.exe'
  result=command('recorder-corrected-'+config+'-'+mode,[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'MmrmCapture38-v2.cs')]);assert result['exit_code']==0
  receipts.append(dict(configuration=config,mode=mode,executable=library_pin(exe),command=clean(result)))
save('recorder-correction-seal.json',dict(status='OBSERVATION_ONLY_CORRECTION_SEALED',recorded_utc=datetime.now(timezone.utc).isoformat(),input_seal=pin(HERE/'input-seal.json'),corrected_source=pin(HERE/'MmrmCapture38-v2.cs'),recorders=receipts,
 original_libraries='Unchanged exact libraries compiled before input seal',expected_outputs='UNKNOWN_NOT_CAPTURED until accepted original attempt',new_semantic_cases=0,current_executions=0))
text=(HERE/'execute_original.py').read_text(encoding='utf-8').replace("exe=folder/'MmrmCapture38.exe'","exe=folder/'MmrmCapture38-v2.exe'").replace("out=HERE/'original-results'","out=HERE/'original-results-v2'").replace("command('original-'","command('original-corrected-'").replace("save('original-execution-'","save('original-execution-v2-'")
with (HERE/'execute_original_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
text=(HERE/'seal_original_outputs.py').read_text(encoding='utf-8').replace("inspect();save('original-source-verification.json',source());", "inspect();assert source()==json.loads((HERE/'original-source-verification.json').read_bytes());")
text=text.replace("HERE/'original-results'","HERE/'original-results-v2'").replace("original_executions_per_configuration=12","original_executions_per_configuration=24,accepted_original_executions_per_configuration=12")
with (HERE/'seal_original_outputs_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
print('Observer repaired/sealed, prior failed records intact; same4 cases/configuration, original libraries/inputs unchanged')
