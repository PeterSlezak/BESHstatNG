from common import *
from baseline_report import python_receipt
manifest=sys.argv[1];records=[]
r=command('publication-local-'+Path(manifest).stem,[sys.executable,'-B',str(HERE/'verify_publication.py'),manifest]);records.append(dict(reader=clean(r)));assert r['exit_code']==0
r=command('publication-adverse-v1',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_publication.py','-v']);tests=python_receipt('w2c3-publication-adverse-v1');records.append(dict(command=clean(r),tests=tests))
save('publication-verification-v1.json',dict(results=records,manifest=pin(ROOT/manifest),independent=True,whole_batch_READY=False))
