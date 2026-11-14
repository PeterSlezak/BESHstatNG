"""Per-site source-domain classification; does not close whole option bodies."""
from common import *
t=json.loads((HERE/'text-call-sites.json').read_bytes());print('keys',list(t));sites=t['sites'];print('example',json.dumps(sites[0],ensure_ascii=False)[:5000])
parents=sorted({r['parent_name'] for r in sites});print('parents',parents)
