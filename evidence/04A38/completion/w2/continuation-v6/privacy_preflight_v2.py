from common import *
sys.path.insert(0,str(PREVIOUS))
from publication_policy import privacy,forbidden
records=[];failures=[]
for path in sorted(HERE.rglob('*')):
 if not path.is_file():continue
 name=path.relative_to(ROOT).as_posix()
 if forbidden(name):continue
 data=path.read_bytes()
 try:findings=privacy(name,data)
 except ValueError as error:failures.append(dict(file=pin(path),error=str(error)));continue
 records.append(dict(file=pin(path),findings=findings))
save('privacy-preflight-v2.json',dict(status='PASS' if not failures else 'ADVERSE_PRIVACY_FINDING_RETAINED',files=records,failures=failures,
    scope='Preliminary C6-only explicit evidence scan; final sealed publication has an independent complete inventory scan',TestResults='NOT_TRAVERSED'))
print(json.dumps(dict(passed=not failures,files=len(records),failures=failures)))
