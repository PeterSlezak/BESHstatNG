from common import *
entry=json.loads((HERE/'entry.json').read_bytes());stage=json.loads((HERE/'source-transitions-parametric-v4.json').read_bytes());files=[];unchanged=[]
for r in stage['files']:
 b=next(x for x in entry['source_before'] if x['live']['path']==r['after']['path']);assert pin(ROOT/r['after']['path'])==r['after']
 if r['before']['sha256']==r['after']['sha256']:unchanged.append(r);continue
 files.append(dict(path=r['after']['path'],before=r['before'],preimage=b['preimage'],after=r['after'],snapshot=r['snapshot']))
assert len(files)==7
save('source-transitions.json',dict(files=files,planned_unmodified=unchanged,original_queries=0,engine_changes='NONE;only existing Core worksheet preparation split for host exception boundary',registration_changes='NONE'))
pins=[]
for path in git('ls-files','BESHStat.Core','-z').decode().split('\0'):
 if not path or not path.endswith(('.vb','.vbproj','.props','.targets')):continue
 p=ROOT/path;raw=git('cat-file','blob',BASE+':'+path);b=p.read_bytes();assert raw.replace(b'\r\n',b'\n')==b.replace(b'\r\n',b'\n'),'Existing Core dependency changed '+path
 pins.append(dict(current=pin(p),Git_bytes=len(raw),Git_sha256=sha(raw),identity='RAW' if raw==b else 'CRLF_LF_ONLY'))
save('accepted-Core-dependency-pins.json',dict(files=pins,status='ALL_TRACKED_CORE_SOURCE_PROJECT_DEPENDENCIES_PINNED',new_worksheet_preparation='Untracked already-authorized04A38 class governed separately by source-transitions.json',scientific_accuracy='NOT_GOLDEN'))
