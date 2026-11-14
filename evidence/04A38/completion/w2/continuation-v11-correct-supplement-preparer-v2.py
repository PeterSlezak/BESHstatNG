"""Additively distinguish optional excluded owner docs from required entry context."""
from pathlib import Path
base=Path(__file__).resolve().parent
p=base/'continuation-v11-supplement-publication-v1.py'
s=p.read_text()
old="assert set(missing)=={r['path'] for r in context},missing"
new="owners={r['path'] for r in json.loads((ROOT/'evidence/04A38/preservation-validation-v2.json').read_bytes())['owner_documents_excluded']}\n    assert set(missing)-owners=={r['path'] for r in context},missing"
assert old in s;s=s.replace(old,new)
s=s.replace("actual_missing_paths=missing,live_preservation=proof,","actual_missing_paths=missing,optional_excluded_owner_paths=sorted(set(missing)&owners),live_preservation=proof,")
s=s.replace("actual_preservation_reads=len(reads),","actual_preservation_reads=len(reads),failed_preparer_source=pin(W2/'continuation-v11-supplement-publication-v1.py'),failed_preparer_reproduction_command=pin(RUNROOT/'commands/w2c11-supplement-preparation-failure-reproduction-v1/command.json'),")
s=s.replace("READER,Path(__file__),note,","READER,Path(__file__),W2/'continuation-v11-supplement-publication-v1.py',W2/'continuation-v11-correct-supplement-preparer-v2.py',note,")
with (base/'continuation-v11-supplement-publication-v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(s)
