from common47 import *
import collections
lock();results=load(HERE/'fixtures/helpers-observer-v2.json')['results'];s=(HERE/'helpers-observer-v1.py').read_text();s=s[s.index("d=load(HERE/'scope/family-review-v2.json')"):];s=s.replace('fixtures/helpers-observer-v1.json','fixtures/helpers-observer-v2.json');exec(compile(s,str(HERE/'helpers-observer-v1.py'),'exec'))
