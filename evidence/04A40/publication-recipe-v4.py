from common40_v3 import *
lock();s=(HERE/'publish-v3.py').read_text(encoding='utf-8')
s=s.replace("folder=PRIVATE/'publication-v2'","folder=PRIVATE/'publication-v3'").replace('BESHStatNG_04A40_REVIEW_v2.zip','BESHStatNG_04A40_REVIEW_v3.zip')
s=s.replace("'publication-'+label","'publication-final2-'+label")
needle="    if raw!=data:renderings.append"
insert="""    if name=='docs/architecture/WindowsMigrationLedger/symbols.csv':data=(PRIVATE/'review-symbols-v1.csv').read_bytes()
    if name=='evidence/04A40/entry/tool-runtime-final-v1.json':data=(PRIVATE/'review-tool-runtime-v2.json').read_bytes()
"""
assert s.count(needle)==1;s=s.replace(needle,insert+needle)
(HERE/'publish-v4.py').open('x',encoding='utf-8').write(s)
