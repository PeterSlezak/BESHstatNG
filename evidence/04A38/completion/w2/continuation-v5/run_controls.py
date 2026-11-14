from common import *
records=[]
r=command('independent-c5-controls',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_checkpoint.py','-v'])
records.append(clean(r))
save('independent-controls.json',dict(commands=records,passed=r['exit_code']==0,scope='Positive whole-body/ownership and23 meaningful mutation controls; no original execution'))
assert r['exit_code']==0
