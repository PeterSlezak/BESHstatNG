"""Preserve failed setup; reuse the exact existing no-package SDK assets."""
import hashlib,json
from pathlib import Path
HERE=Path(__file__).resolve().parent
source=HERE/'run_census.py';data=source.read_bytes()
with (HERE/'census-setup-attempt-v1.json').open('x',encoding='utf-8') as stream:json.dump(dict(status='FAILED_BEFORE_COMPILATION',exit_code=1,source_file='run_census.py',source_sha256=hashlib.sha256(data).hexdigest(),exception='AssertionError: assumed inventory-v1/obj/project.assets.json does not exist',source_or_model_execution=False,retained_empty_output='mmrm38-v6/w1-census-tool',correction='Original inventory command receipt points to existing04A37/helpers/obj/project.assets.json; read/copy without restore or alteration'),stream,indent=2)
text=data.decode().replace("folder=PRIVATE/'w1-census-tool'", "folder=PRIVATE/'w1-census-tool-v2'").replace("assets=RUNROOT/'inventory-v1/obj/project.assets.json'", "assets=ROOT/'evidence/04A37/helpers/obj/project.assets.json'")
text=text.replace("'/p:Configuration=Release',", "'/p:Configuration=Release','/p:ProjectAssetsFile='+str(obj/'project.assets.json'),")
with (HERE/'run_census_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
