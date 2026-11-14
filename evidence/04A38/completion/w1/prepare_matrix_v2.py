"""Disambiguate identical lambda headers/bodies by proven normalized spans."""
from pathlib import Path
import json,hashlib
HERE=Path(__file__).resolve().parent
source=HERE/'build_matrix.py';raw=source.read_bytes()
with (HERE/'matrix-attempt-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(status='FAILED_CLOSED_BEFORE_LEDGER_PUBLICATION',exit_code=1,source_sha256=hashlib.sha256(raw).hexdigest(),reason='Two original lambda nodes share signature key and body tokens; exact archived normalized UTF16 span is required',new_reference_executions=0,new_production_changes=0),f,indent=2)
text=raw.decode();text=text.replace(" original_map={};crosswalk=[]", """ snapshots={r['source']['path']:(ROOT/r['snapshot']['path']).read_text(encoding='utf-8-sig',newline='') for r in []}
 snapshots={}
 for row in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']:
  with (ROOT/row['snapshot']['path']).open(encoding='utf-8-sig',newline='') as stream:snapshots[row['source']['path']]=stream.read()
 def normalized_span(text,start):return len(text.encode('utf-16-le')[:start*2].decode('utf-16-le').replace('\\r\\n','\\n').encode('utf-16-le'))//2
 original_map={};crosswalk=[]""")
text=text.replace("  assert len(matches)==1,('Ambiguous original identity'", "  if len(matches)>1:\n   matches=[e for e in matches if normalized_span(snapshots[e['path']],e['span_start'])==m['span_start']]\n  assert len(matches)==1,('Ambiguous original identity'")
text=text.replace(" snapshots={r['source']['path']:(ROOT/r['snapshot']['path']).read_text(encoding='utf-8-sig',newline='') for r in []}\n",'')
with (HERE/'build_matrix_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(text)
