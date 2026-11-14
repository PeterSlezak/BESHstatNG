"""Additive corrected derivative; failed helper remains protected locally."""
from common import *
source=(HERE/'publish.py').read_text(encoding='utf-8')
source=source.replace('PARTIAL_W2_CONTINUATION_V3;NO_W3_READY_OR_OWNER_ACCEPTANCE','PARTIAL_W2_CONTINUATION_V4;NO_W3_READY_OR_OWNER_ACCEPTANCE').replace('protected C2 predecessor publication','protected C3 predecessor publication')
source=source.replace('paths=sorted(p for p in paths if not forbidden(p))','paths=sorted(p for p in paths if not forbidden(p) and p != (HERE/"final_live.py").relative_to(ROOT).as_posix())')
source=source.replace("excluded=['C2 and C4", "excluded=['C4 final_live.py: failed new closeout script, unchanged local preimage in failed-attempt receipt; v2 manifest references this omitted payload and cannot be fully revalidated from derivative','C2 and C4")
assert 'failed new closeout script' in source
exec(compile(source,str(HERE/'publish.py'),'exec'))
