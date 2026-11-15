"""Expand the small transport to include exact synthetic compatibility evidence."""
from pathlib import Path
root=Path(__file__).resolve().parents[2];here=root/'evidence/04A40'
s=(here/'publish-v1.py').read_text(encoding='utf-8')
s=s.replace("folder=PRIVATE/'publication-v1'","folder=PRIVATE/'publication-v2'").replace('BESHStatNG_04A40_REVIEW_v1.zip','BESHStatNG_04A40_REVIEW_v2.zip')
s=s.replace("PRIVATE/'pending/review_readback.py'","HERE/'review_readback-v2.py'").replace("HERE/'review_readback.py'","HERE/'review_readback-v2.py'")
s=s.replace("module('standalone40',PRIVATE/'pending/review_readback.py')","module('standalone40',HERE/'review_readback-v2.py')")
s=s.replace("for r in load(HERE/'scope/source-freeze-v1.json')['paths']", "for r in load(HERE/'scope/source-freeze-v1.json')['paths']")
needle='renderings=[]'
extra="""selected.update(['evidence/04A39/fixtures/frozen-input-manifest-v2.json','evidence/04A39/fixtures/Observe39v2.cs','evidence/04A39/fixtures/before-v2.json',
 'evidence/04A13/R-candidate-reference-v2.csv','evidence/04A13/numerical-comparisons.json','evidence/04A13/issues.json','scripts/reference-04a13.R'])
for config in ['Debug','Release']:
    selected.add('evidence/04A39/abi-cil/'+config+'/exports-v1.json')
    for row in load(HERE/('compatibility/'+config+'-current-only-v4.json'))['results']:
        for field in ['original','current','inputs','recorder']:selected.add(row[field]['path'])
"""
assert s.count(needle)==1;s=s.replace(needle,extra+needle)
needle="    put(name,data)"
replacement="""    if name=='evidence/04A40/baseline/Debug-build-v1.json':
        assert raw!=data and data==(PRIVATE/'baseline-Debug-build-review-v1.json').read_bytes(),'PROTECTED_RAW_ENTERED_PAYLOAD'
    put(name,data)"""
assert s.count(needle)==1;s=s.replace(needle,replacement)
s=s.replace("reader=module('standalone40',PRIVATE/'pending/review_readback.py')", "reader=module('standalone40',HERE/'review_readback-v2.py')")
(here/'publish-v2.py').open('x',encoding='utf-8').write(s)
s=(here/'review_readback.py').read_text(encoding='utf-8')
s=s.replace('import collections,hashlib,json,re,sys,zipfile','import collections,hashlib,json,re,sys,zipfile,csv')
needle="            assert read('fixtures/'+config+'-39-retained-inputs-fresh-results-v2.json')['cases']==77"
extra="""            for row in c['results']:
                before=z.read(row['original']['path']);current=z.read(row['current']['path'])
                assert before==current and digest(before)==row['original']['sha256']==row['current']['sha256']
                assert len(list(csv.DictReader(current.decode('utf-8-sig').splitlines(),delimiter='\\t')))==row['cases']
                for field in ['inputs','recorder']:assert digest(z.read(row[field]['path']))==row[field]['sha256']
            original_exports=json.loads(z.read('evidence/04A39/abi-cil/'+config+'/exports-v1.json'))['exports']
            actual_exports=json.loads(z.read(prefix+'abi-cil/'+config+'-actual-exports.json'))['exports']
            assert len(actual_exports)==268 and actual_exports==original_exports
"""
assert s.count(needle)==1;s=s.replace(needle,extra+needle)
(here/'review_readback-v2.py').open('x',encoding='utf-8').write(s)
