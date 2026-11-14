from common import *
t=json.loads((HERE/'text-call-sites.json').read_bytes());sites=t['calls'];print('example',json.dumps(sites[0],ensure_ascii=False)[:4000]);print('parents',sorted({r['parent_name'] for r in sites}))
