from common47 import *
s=(HERE/'disposition-python-v2.py').read_text()
before="search='evidence/04A47/preservation/missing-fixture-search-v1.json')))"
after="search='evidence/04A47/preservation/missing-fixture-search-v1.json'))"
assert before in s
exec(compile(s.replace(before,after),str(HERE/'disposition-python-v2.py'),'exec'))
