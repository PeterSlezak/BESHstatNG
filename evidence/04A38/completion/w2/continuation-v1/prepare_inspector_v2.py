from common import *
s=(HERE/'inspect_stage.py').read_text()
s=s.replace("before=json.loads((HERE/'entry.json').read_bytes())['source_before'];after=[]",
 "initial=json.loads((HERE/'entry.json').read_bytes())['source_before']; extra=json.loads((HERE/'source-before-p1.json').read_bytes())['files']+json.loads((HERE/'source-before-p1-reports.json').read_bytes())['files']; before=list({r['live']['path']:r for r in extra+initial}.values());after=[]")
with (HERE/'inspect_stage_v2.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/inspector-setup-shell-quoting.json',dict(exit_code=1,stage='P1 inspection setup',observed='PowerShell parsed nested double quotes in Python -c; no source/evidence payload changed and inspector file was not produced. Subsequent missing-file invocation exit2 preserved as tool attempt.',correction='Versioned Python setup script; no shell interpolation',affected_paths=[]))
