from common import *
from baseline_report import python_receipt
from verify_publication import inspect
manifest=(HERE/'publication-manifest-v2.json').relative_to(ROOT).as_posix();records=[]
for label,script,args in [('public-local-v2','verify_publication.py',[manifest]),('public-copy-v2','check_public_copy.py',[manifest])]:
 r=command(label,[sys.executable,'-B',str(HERE/script),*args]);records.append(dict(label=label,command=clean(r)));assert r['exit_code']==0
r=command('public-adverse-v2',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_publication_v2.py','-v']);records.append(dict(label='public-adverse-v2',command=clean(r),tests=python_receipt('w2c2-public-adverse-v2')))
save('publication-verification-v2.json',dict(results=records,proof=inspect(manifest_path=manifest),raw_diagnostic='Retained locally/excluded only from public derivative; explicit relationship in diagnostic-public-derivative.json'))
assert r['exit_code']==0
