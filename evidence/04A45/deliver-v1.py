from common45 import *
lock();r=command('publication-v1',[sys.executable,'-I','-B',str(HERE/'run45.py'),'publication45.py'])
assert not r['exit_code'],'PUBLICATION_FAILED_EXACT_DIAGNOSTICS_PRESERVED'
folder=PRIVATE/'publication'
for name in ['BESHStatNG_04A45_SAFE_HARNESS_REVIEW_v1.zip','external-delivery-receipt-v1.json']:
 p=pin(folder/name);print(name,p['bytes'],p['sha256'],flush=True)
