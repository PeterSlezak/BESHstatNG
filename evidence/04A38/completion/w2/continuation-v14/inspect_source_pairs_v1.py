from common import *
import re
sys.path.append(str(W2/'continuation-v11'))
from verify_composition_v2 import tokens
root=PRIVATE/'draft-proof-v1';rows=json.loads((root/'whole-body-contracts-v1.json').read_bytes())['contracts']
for c in rows:
 if not c['new_support'] or tokens(c['current_whole_body'])==tokens(c['original_whole_body']):continue
 if c['current_record']['name'] in ['BuildFitOptions','ResolveNamedTable','DataImportSummaryTable','TryGetPsmData']:continue
 print('\nNAME',c['current_record']['name'],'ORIGINAL\n',c['original_whole_body'],'\nCURRENT\n',c['current_whole_body'])
