"""Unique additive receipt names; preserve the stopped privacy ZIP and v2 prelaunch collision."""
from pathlib import Path
path=Path(__file__).with_name('publish-v2.py')
source=path.read_text(encoding='utf-8')
source=source.replace("save('publication-stop-v1.json'", "save('publication-stop-v2.json'")
source=source.replace("save('source-rendering-v2.json'", "save('source-rendering-v3.json'")
needle='for old,new in replacements.items():'
extra="""replacements['git-final-raw.json']='git-final-v3-raw.json'
replacements['from readback import readback']='from readback_v2 import readback'
replacements[\"index=dict(schema='04A39-final-physical-privacy-witness-v1'\"]=\"index=dict(active_readiness='evidence/04A39/physical-witness-v1/readiness-v3.json',schema='04A39-final-physical-privacy-witness-v1'\"
for k in list(replacements):
    replacements[k]=replacements[k].replace('Git-final-v2','Git-final-v3').replace('readiness-v2','readiness-v3').replace('owner-review-v2','owner-review-v3').replace('REVIEW-v2','REVIEW-v3').replace('WITNESS_v2','WITNESS_v3').replace('new-dossier-v2','new-dossier-v3').replace('receipt-v2','receipt-v3')
for old,new in replacements.items():"""
assert source.count(needle)==1
exec(compile(source.replace(needle,extra),str(path),'exec'))
