"""Entry authority can also be an owner document: required context takes precedence."""
from pathlib import Path
base=Path(__file__).resolve().parent
s=(base/'continuation-v11-supplement-publication-v2.py').read_text()
s=s.replace("assert set(missing)-owners=={r['path'] for r in context},missing", "required={r['path'] for r in context}\n    assert set(missing)-owners<=required and required<=set(missing),missing")
s=s.replace("optional_excluded_owner_paths=sorted(set(missing)&owners)","optional_excluded_owner_paths=sorted((set(missing)&owners)-required)")
s=s.replace("failed_preparer_source=pin(W2/'continuation-v11-supplement-publication-v1.py'),", "failed_preparer_sources=[pin(W2/name) for name in ['continuation-v11-supplement-publication-v1.py','continuation-v11-supplement-publication-v2.py']],second_failure_command=pin(RUNROOT/'commands/w2c11-supplement-preparation-v2/command.json'),")
s=s.replace("W2/'continuation-v11-supplement-publication-v1.py',W2/'continuation-v11-correct-supplement-preparer-v2.py',note,", "W2/'continuation-v11-supplement-publication-v1.py',W2/'continuation-v11-supplement-publication-v2.py',W2/'continuation-v11-correct-supplement-preparer-v2.py',W2/'continuation-v11-correct-supplement-preparer-v3.py',note,")
s=s.replace('The failed first supplement preparer', 'The failed first supplement preparer')
s=s.replace('optional excluded owner README documents remain omitted.', 'the15 optional excluded owner README documents remain omitted; the one required master-plan owner document is included byte-exact under its entry pin.')
with (base/'continuation-v11-supplement-publication-v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(s)
