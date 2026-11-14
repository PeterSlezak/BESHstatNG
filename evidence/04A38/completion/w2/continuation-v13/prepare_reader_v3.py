from common import *
source=HERE/'verify_c13_checkpoint_template_v2.py';v=source.read_text()
a=v.index(' saved=list(sys.path)');b=v.index(" u=load(PREFIX+'current-universe",a);prior=v[a:b];v=v[:a]+v[b:]
v=v.replace(" graph=closure(meta,[bind(members[k],meta,shapes)['method'] for k in inputs['roots']]);", " need(set(inputs['roots'])=={bind(c['current_record'],meta,shapes)['method'] for c in cs},'Compiled root/complete source parent binding changed')\n  graph=closure(meta,inputs['roots']);")
v=v.replace(' return summary\n',prior+' return summary\n')
compile(v,'verify_c13_checkpoint_template_v3.py','exec')
with (HERE/'verify_c13_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('reader-root-schema-correction-v1.json',dict(before=pin(source),after=pin(HERE/'verify_c13_checkpoint_template_v3.py'),
    reason='C13 diagnostic roots are actual complete compiled handles, not source IDs. Independently bind every one of96 source parents and require exact root-set equality before executable graph reconstruction. Actual predecessor replays in rejected attempts completed before the current schema lookup failed; final version still replays the predecessor, after current checks to fail quickly.',original_queries=0))
