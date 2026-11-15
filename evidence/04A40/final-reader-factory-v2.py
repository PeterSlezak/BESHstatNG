from pathlib import Path
root=Path(__file__).resolve().parents[2];here=root/'evidence/04A40'
s=(here/'verify40.py').read_text(encoding='utf-8')
old="        for config in ['Debug','Release']:\n            current=load"
new="""        for config in ['Debug','Release']:
            proof=d['ABI'][config]
            for item in proof['assembly_hashes']+[proof['current_metadata'],proof['current_exports']]:assert pin(item['path'])==item,'SHIPPING_OR_METADATA_BYTES_CHANGED'
            current=load"""
assert s.count(old)==1;s=s.replace(old,new)
old="    assert len(d['source']['source_delta'])==16 and len(d['source']['new_tests'])==2"
new=old+"\n    assert d['source']['source_pins']==load(HERE/'matrix/source-freeze-v2.json')['source_pins'],'FROZEN_SOURCE_MANIFEST_CHANGED'"
s=s.replace(old,new)
s=s.replace("('branch',lambda x:","('changed-source-pin',lambda x:x['source']['source_pins'].__setitem__(next(iter(x['source']['source_pins'])),'0'*64)),('branch',lambda x:")
s=s.replace('readiness-v1.json','readiness-v1.json')
(here/'verify40-v2.py').open('x',encoding='utf-8').write(s)
