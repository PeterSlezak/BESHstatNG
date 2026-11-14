"""Authorized future reporting boundary: raw private receipt plus separate sanitized projection."""
from common import *
from scanner_adapter import guard
import importlib.util

spec=importlib.util.spec_from_file_location('authenticated39_redactor',PRIOR/'transport-privacy-v2.py')
redactor=importlib.util.module_from_spec(spec);spec.loader.exec_module(redactor)
redactor.actual.re=guard.RegexProxy()

def sanitized(data):
    # JSON identity/count/status structure is preserved. Collision is an error.
    return redactor.redact(data)

def emit(identity,data,raw_root,public_root):
    key=sha(identity.encode())
    raw_root=Path(raw_root).resolve();public_root=Path(public_root).resolve()
    assert raw_root.is_relative_to(PRIVATE.resolve()), 'RAW_LOCATION_NOT_PROTECTED'
    raw_root.mkdir(parents=True,exist_ok=True);public_root.mkdir(parents=True,exist_ok=True)
    raw_path=raw_root/(key+'.raw.json')
    save(raw_path,dict(identity=identity,data=data))
    projection=dict(identity=identity,raw_id=key,raw_sha256=digest(raw_path),data=sanitized(data),
        original_status=data.get('status') if isinstance(data,dict) else None,diagnostic_projection=True,test_behavior_changed=False)
    encoded=(json.dumps(projection,ensure_ascii=False,indent=2)+'\n').encode()
    assert not redactor.scan('diagnostics/'+key+'.json',encoded), 'SANITIZATION_INCOMPLETE'
    destination=public_root/(key+'.json')
    with destination.open('xb') as f:f.write(encoded)
    return projection,raw_path,destination
