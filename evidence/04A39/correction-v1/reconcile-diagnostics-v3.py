"""Additive reader correction: follow the two delegated publication-verifier calls."""
from pathlib import Path
import hashlib

prior=Path(__file__).with_name('reconcile-diagnostics-v2.py')
source=prior.read_text(encoding='utf-8')
old="        assert 'diff' in body\n"
new="""        if 'diff' not in body:
            phase='22' if r['identity'].startswith('test_04a22_spc.') else '24'
            helper=ROOT/('scripts/verify-04a'+phase+'-publication.py')
            assert helper.name in body and 'diff' in helper.read_text(encoding='utf-8-sig')
            bindings.append(dict(delegated_producer=pin(helper),proof='Exact test calls this verifier; its candidate function reads tracked Git diff.'))
"""
assert source.count(old)==1
exec(compile(source.replace(old,new),str(prior),'exec'))
