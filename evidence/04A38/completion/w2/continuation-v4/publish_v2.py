"""Final C4 additive manifest; retains the first manifest and producer unchanged."""
from common import *
source=(HERE/'publish.py').read_text(encoding='utf-8')
assert source.count('PARTIAL_W2_CONTINUATION_V3;NO_W3_READY_OR_OWNER_ACCEPTANCE')==1
assert source.count('protected C2 predecessor publication')==1
source=source.replace('PARTIAL_W2_CONTINUATION_V3;NO_W3_READY_OR_OWNER_ACCEPTANCE','PARTIAL_W2_CONTINUATION_V4;NO_W3_READY_OR_OWNER_ACCEPTANCE').replace('protected C2 predecessor publication','protected C3 predecessor publication')
exec(compile(source,str(HERE/'publish.py'),'exec'))
