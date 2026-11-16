from common45 import *
# Fresh syntax census only: old source-specific semantic decisions must not be replayed.
source=(HERE/'inventory-v1.py').read_text().replace('syntax-current-v1','syntax-current-v2').replace('roslyn-source-inventory-v1','roslyn-source-inventory-v2').replace("'--write','--ledger='+str(target)","'--write','--ledger='+str(target),'--reviews='+str(PRIVATE/'no-reviews45.json'),'--wiring-reviews='+str(PRIVATE/'no-wiring-reviews45.json')").replace('syntax-inputs-v1','syntax-inputs-v2').replace('syntax-schema-v1','syntax-schema-v2')
exec(compile(source,str(HERE/'inventory-v1.py'),'exec'))
