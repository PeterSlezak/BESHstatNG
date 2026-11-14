"""Include the actual reporting module so preserved uncalled helpers stay measurable."""
from common_v2 import *
before=HERE/'native-metadata-owners-v2.txt';text=before.read_text(encoding='utf-8-sig');owner='BESHStatNG.regression.BinaryClassificationReporting';assert owner not in text.splitlines()
with (HERE/'native-metadata-owners-v3.txt').open('x',encoding='utf-8',newline='\n') as f:f.write(text.rstrip()+'\n'+owner+'\n')
source=(HERE/'inspect_metadata_v4.py').read_text(encoding='utf-8').replace('metadata-all-v4','metadata-all-v5').replace('metadata-v4','metadata-v5').replace('context-v4','context-v5').replace('native-metadata-owners-v2.txt','native-metadata-owners-v3.txt')
with (HERE/'inspect_metadata_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('metadata-scope-expansion-v1.json',dict(before=pin(before),after=pin(HERE/'native-metadata-owners-v3.txt'),added_owner=owner,reason='Four report methods now delegate to Common; two unchanged native private helpers no longer lie on those call paths. Explicit real-module roots preserve their method/field/ctor measurements and expose real GUI/Core forwarding edges. This is passive current-only metadata, not GUI/native execution or deletion.',retained_first_measurement=pin(HERE/'cohort-diagnostic-v1.json'),original_queries=0))
