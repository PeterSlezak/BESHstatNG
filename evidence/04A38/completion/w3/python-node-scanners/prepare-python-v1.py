"""Path/scheduling adaptation of authenticated W2 assertion-preserving workers."""
from common import *

target=HERE/'python-node-scanners/campaign-v1'
target.mkdir(exist_ok=False)
changes=[]
def adapt(name,replacements):
    src=ACCEPTED/name
    text=src.read_text(encoding='utf-8')
    for before,after in replacements:
        assert text.count(before)==1,(name,before,text.count(before))
        text=text.replace(before,after)
    out=target/name
    with out.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    changes.append(dict(authority=pin(src),adapted=pin(out),substitutions=[dict(before=a,after=b) for a,b in replacements]))
adapt('common.py',[
    ('ROOT=Path(__file__).resolve().parents[5]','ROOT=Path(__file__).resolve().parents[6]'),
    ('W2=HERE.parent',"W2=ROOT/'evidence/04A38/completion/w2'"),
    ("PRIVATE=RUNROOT/'completion-w2-cont17-postcorrection-v1'","PRIVATE=RUNROOT/'w3pyv1'"),
    ("'w2c17pc1-'+label","'w3pyv1-'+label")])
adapt('python_workers_v4.py',[
    ("PRIVATE=RUNROOT/'c17pc1-py'","PRIVATE=RUNROOT/'w3pyv1'"),
    ("RUNROOT/'executions/w2c17pc1-CS/observations'","RUNROOT/'executions/w3D-Common-semantic/observations'")])
for name in ['retained_scan_cache_v17_v1.py','scanner_retained_preflight_v2.py','scanner_cache_controls_v2.py']:
    adapt(name,[])
plan=load(ACCEPTED/'implementation-plan-v2.json')
with (target/'implementation-plan-v2.json').open('x',encoding='utf-8') as f:json.dump(plan,f,indent=2)
save('python-node-scanners/python-driver-crosswalk-v1.json',dict(status='PATH_ONLY_ADAPTATION_BEFORE_ANY_PYTHON_PARENT',
    changes=changes,assertions_changed=0,expected_parents=956,workers_at_peak=3,scanner_helpers=0,
    shared_scanners='Serial after all ordinary/disposable parts; no mutable evidence writers during execution',
    decision='Fresh full W3 campaign required: new W3 Git candidates affect scanner domain, so whole-campaign carry-forward cannot be asserted.',
    retained_success_cache='Only exact per-file bytes/policy/kernel/environment matches, with existing adverse controls; failures/errors never cached as PASS.'))
print('Python campaign drivers prepared; zero parents executed',flush=True)
