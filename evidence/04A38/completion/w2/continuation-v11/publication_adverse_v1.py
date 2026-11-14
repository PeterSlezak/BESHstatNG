"""Real bounded ZIP controls, including duplicate/casefold and path/type attacks."""
from common import *
import copy
import zipfile
import zlib
import warnings
from verify_c11_publication_v1 import inspect_part

folder=PRIVATE/'publication-adverse-v1';folder.mkdir(exist_ok=False)
payload=b'public control'
controls=[];artifacts=[]
def archive(name,members,symlink=False):
    path=folder/(name+'.zip')
    with warnings.catch_warnings():
        warnings.simplefilter('ignore',UserWarning)
        with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED) as out:
            for member in members:
                item=zipfile.ZipInfo(member,date_time=(1980,1,1,0,0,0));item.compress_type=zipfile.ZIP_DEFLATED
                item.create_system=3;item.external_attr=(0o120777 if symlink else 0o100644)<<16
                out.writestr(item,payload)
    artifacts.append(pin(path))
    doc=dict(parts=[dict(name=path.name,**pin(path))],files=[dict(path='payload.txt',bytes=len(payload),sha256=sha(payload),
        stored_CRC32=format(zlib.crc32(payload)&0xffffffff,'08x'),part=path.name,logical_root_prefix='')])
    return path,doc

positive,index=archive('positive',['payload.txt'])
result=inspect_part(positive,index);controls.append(dict(name='Actual valid bounded part',status='PASS',result=result))
for label,members,symlink in [('duplicate',['payload.txt','payload.txt'],False),
                               ('casefold-collision',['PAYLOAD.TXT','payload.txt'],False),
                               ('unsafe-parent',['../escape.txt'],False),
                               ('reserved-name',['CON.txt'],False),
                               ('symlink',['payload.txt'],True),
                               ('unmanifested-extra',['payload.txt','unmanifested.txt'],False)]:
    path,doc=archive(label,members,symlink)
    try:inspect_part(path,doc)
    except (ValueError,AssertionError,KeyError,OSError,zipfile.BadZipFile) as error:
        controls.append(dict(name='Actual ZIP '+label,status='REJECTED',reason=clean(str(error))))
    else:raise AssertionError('Malicious physical ZIP accepted '+label)
for label,edit in [('wrong file SHA',lambda d:d['files'][0].update(sha256='0'*64)),
                  ('wrong stored CRC',lambda d:d['files'][0].update(stored_CRC32='00000000')),
                  ('wrong physical part SHA',lambda d:d['parts'][0].update(sha256='0'*64)),
                  ('wrong logical prefix',lambda d:d['files'][0].update(logical_root_prefix='wrong/'))]:
    doc=copy.deepcopy(index);edit(doc)
    try:inspect_part(positive,doc)
    except (ValueError,AssertionError,KeyError,OSError,zipfile.BadZipFile) as error:
        controls.append(dict(name=label,status='REJECTED',reason=clean(str(error))))
    else:raise AssertionError('Invalid part authority accepted '+label)
save('publication-adversarial-controls-v1.json',dict(reader=pin(HERE/'verify_c11_publication_v1.py'),
    controls=controls,physical_artifacts=artifacts,original_queries=0,
    scope='Real positive/negative ZIP files retained separately. No source/reference bytes altered; no negative archive extracted.'))
print('Physical publication controls',len(controls),'PASS_NOT_READY',flush=True)
