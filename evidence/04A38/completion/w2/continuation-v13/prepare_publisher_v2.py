from common import *
s=HERE/'publish_parts_v1.py';v=s.read_text()
v=v.replace("    return names","    names.add((W2/'continuation-v13-retained-private-reader-v2.py').relative_to(ROOT).as_posix())\n    return names",1)
v=v.replace("('changed SHA',lambda d:d['files'][0].update(sha256='0'*64))","('changed SHA',lambda d:d['files'][0].update(sha256='0'*64)),('stale source index',lambda d:next(r for r in d['files'] if r['path']==json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['path']).update(sha256=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['before']['sha256']))")
compile(v,'publish_parts_v2.py','exec')
with (HERE/'publish_parts_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('publisher-successor-v1.json',dict(before=pin(s),after=pin(HERE/'publish_parts_v2.py'),scope='Include actual private-reader v2 and reject an index bearing the real previous source SHA rather than the frozen current SHA. All payload/physical/union/policy checks remain.',original_queries=0))
