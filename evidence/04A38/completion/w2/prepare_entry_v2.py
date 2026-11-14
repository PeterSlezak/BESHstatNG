"""Retain failed path-length attempt and prepare a distinct preservation copy."""
import json
from pathlib import Path
HERE=Path(__file__).resolve().parent
s=(HERE/'begin.py').read_text(encoding='utf-8').replace("'w2-pre-change-view'","'w2-before-v2'")
s=s.replace('shutil.copyfile(source,target)',"shutil.copyfile('\\\\\\\\?\\\\'+str(source.resolve()),'\\\\\\\\?\\\\'+str(target.resolve()))")
s=s.replace('target.read_bytes()==source.read_bytes()',"Path('\\\\\\\\?\\\\'+str(target.resolve())).read_bytes()==source.read_bytes()")
with (HERE/'begin_v2.py').open('x',encoding='utf-8') as f:f.write(s)
with (HERE/'entry-copy-attempt-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(status='FAILED_WINDOWS_COPY_PATH_LENGTH',stage='Historical pre-W2 view copy',production_changes=0,partial_view='Retained privately; never used as preservation proof',repair='New view with extended-length destination/source paths; no deletion or global environment change',TestResults='Only known tracked paths were considered through git ls-files; ignored history was not traversed'),f,indent=2)
