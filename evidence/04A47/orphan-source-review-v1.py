from common47 import *
import collections
lock();path='BESHstatNG/src/UI/Ui20PropensityScoreMatching.vb';designer='BESHstatNG/src/UI/Ui20PropensityScoreMatching.Designer.vb';name='btInterrupt_Click';src=(ROOT/path).read_text(encoding='utf-8-sig');design=(ROOT/designer).read_text(encoding='utf-8-sig')
assert src.count(name)==1 and 'btInterrupt' not in design
rows=[]
for c in ['Debug','Release']:
 t=next(t for t in load(PRIVATE/'entry-builds'/c/'wiring.json')['types'] if t['owner']=='BESHStatNG.Ui20PropensityScoreMatching')
 assert t['base_type']=='System.Windows.Forms.Form' and not any(f['name'].casefold().endswith('btinterrupt') for f in t['fields'])
 declared=next(m for m in t['methods'] if m['name']==name);callers=[m['name'] for m in t['methods'] if name in json.dumps(m['tokens'])];assert not callers
 rows.append(dict(configuration=c,method_signature=declared['signature'],compiled_field_present=False,compiled_static_callers=callers,base=t['base_type']))
d=load(HERE/'scope/family-review-v1.json');cols=d['member_columns'];cl=cols.index('classification');nm=cols.index('name');ow=cols.index('owner');reason=cols.index('reason');evidence=cols.index('evidence');found=[]
for r in d['members']:
 if r[ow]=='BESHStatNG.Ui20PropensityScoreMatching' and r[nm]==name:
  found.append(r[0]);r[cl]='SOURCE_AND_IL_ONLY';r[reason]='Non-executable informational method: no declared/inherited product btInterrupt control,Handles/AddHandler binding or compiled static caller found. No current interrupt UI action claimed;no new button/fix authorized.';r[evidence]='evidence/04A47/scope/orphan-method-v1.json'
assert len(found)==1;d['counts']=dict(collections.Counter(r[cl] for r in d['members']));d['orphan_source_methods']=1
save('scope/family-review-v2.json',d)
save('scope/orphan-method-v1.json',dict(member_id=found[0],source=pin(path),designer=pin(designer),method=name,has_control=False,has_source_binding=False,has_compiled_static_caller=False,compiled=rows,classification='SOURCE_AND_IL_ONLY',canonical_id=None,correction='NOT_AUTHORIZED;no cleanup or UI addition',qualification='Not a supported current control or executed native action. Static/source proof of absence is bounded to current declared product control and bindings;no runtime UI enumeration performed.'))
print('PSM informational interrupt method has no control/binding/static caller;explicit source-only orphan,not an executed action',flush=True)
