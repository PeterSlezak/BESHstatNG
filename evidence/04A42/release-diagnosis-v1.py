from common42 import *
sys.path.insert(0,str(ROOT/'evidence/04A38'));from baseline_report import index
old=load(ROOT/'evidence/04A41/build-and-tests/Debug-identities-v1.json');before={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in old['tests']};changes=[]
for part in load(HERE/'build-and-tests/Release-executed-v1.json')['parts']:
    for (identity,occurrence),r in index(load(ROOT/part['cases']['path'])['records']).items():
        b=before.get((part['suite'],identity,occurrence))
        if b and b['outcome']!=r['outcome']:
            changes.append(dict(before=b,current=r,part=part))
            print(r['class_name'],r['method'],b['outcome'],'->',r['outcome'],sha(r['message'].encode()),flush=True)
save(PRIVATE/'Release-changed-outcome-diagnosis-v1.json',dict(changes=changes))
