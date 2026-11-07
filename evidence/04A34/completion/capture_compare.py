"""Exact read-only comparison of final executed captures to immutable oracles."""
from work import *
rows=[]
for config in ['Debug','Release']:
    for name in ['geometry','extra-geometry','preparation','series','policies','spc-basis','compatibility']:
        old=HERE/('pre-edit-'+name+'.csv') if name!='compatibility' else HERE.parent/'pre-edit-compatibility-capture.csv'
        new=ROOT/'.test-rationalization-04A34.tmp'/('completion-review3-post-'+config+'-'+name+'.csv')
        a,b=old.read_bytes(),new.read_bytes();assert a==b,(config,name)
        with old.open(encoding='utf-8-sig',newline='') as f:count=len(list(csv.reader(f)))-1
        rows.append(dict(configuration=config,family_capture=name,pre_edit=old.relative_to(ROOT).as_posix(),pre_sha256=sha(a),post_sha256=sha(b),bytes=len(a),rows=count,byte_equal=True,authority='PRE_EDIT_LEGACY',scientific='NOT_GOLDEN',post_payload='Exact duplicate remains ignored; reference bytes are authoritative. Reproduce with capture-final/capture-final-spc against review3 build.',final_build='review3/'+config))
save('capture-comparison.json',dict(status='PASS',comparisons=rows,bit_policy='Exact CSV bytes and IEEE finite/infinite/signed-zero/raw-NaN bits. Managed tests additionally compare algorithmic NaN by classification. Geometry snapshot projects original properties; added derived SubtitleTop is tested independently.'))
print('PASS',sum(r['rows'] for r in rows)//2,'frozen rows per configuration; Debug and Release exact')
