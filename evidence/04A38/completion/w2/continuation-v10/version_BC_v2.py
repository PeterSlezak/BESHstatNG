"""Correct the extraction assertion: Math.Max is not a reference to host variable h."""
from common import *
source=(HERE/'implement_packet_BC_v1.py').read_text(encoding='utf-8');old="assert 'h.' not in code and 'GetOptionalBool(' not in code";assert old in source
source=source.replace('import textwrap','import textwrap,re').replace(old,"assert not re.search(r'\bh\.',code) and 'GetOptionalBool(' not in code")
with (HERE/'implement_packet_BC_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());row=next(r for r in plan['source_before'] if r['live']['path'].endswith('MultivariateAnalysisUDFs.vb'));assert pin(ROOT/row['live']['path'])==row['live']
save('BC-generator-first-attempt-v1.json',dict(before=pin(HERE/'implement_packet_BC_v1.py'),after=pin(HERE/'implement_packet_BC_v2.py'),production_unchanged_on_rejection=True,reason='Naive substring h-dot matched Math.Max. Version2 checks the complete host-variable token with a word boundary. The first tool traceback and private-draft PowerShell quoting rejection remain in conversation receipts; no production/test source correction occurred.'))
