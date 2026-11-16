from common47 import *
lock();version=sys.argv[1] if len(sys.argv)>1 else 'v1'
r=command('publication-'+version,[sys.executable,'-I','-B',str(HERE/'run47.py'),'publication47.py',version])
assert not r['exit_code'],'PUBLICATION_FAILED_DIAGNOSTICS_PRESERVED'
folder=PRIVATE/('publication-'+version)
for name in ['BESHStatNG_04A47_SAFE_LONGRUN_GUI_WIRING_REVIEW_'+version+'.zip','external-delivery-receipt-'+version+'.json']:
 p=pin(folder/name);print(name,p['bytes'],p['sha256'],flush=True)
