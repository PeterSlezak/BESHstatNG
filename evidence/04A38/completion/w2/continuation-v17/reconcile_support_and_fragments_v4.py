"""Audit structural fragments and support children before proposed ledger promotion."""
from proof_support_v17_v5 import *
from collections import Counter
old=load(PREVIOUS/'current-ownership-v1.json');u=load(FINAL/'current-universe.json');o=load(W2.parent/'w1/original-universe.json');c=load(FINAL/'dependency-full-common.json')
meta=load(FINAL/'current-Debug.json');original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json')
mp,_=map_native(old['members'],u['members'],c['members']);contexts=load(HERE/'constructed-host-and-property-contexts-v3.json')['rows']
module_proofs=[]
for s in old['support']:
 if not s['distinct'] or s['fully_reconciled']:continue
 choices=[n for n in u['supports'] if (n['path'],n['owner'],n['kind'],n['declaration'])==(s['path'],s['owner'],s['kind'],s['declaration'])]
 assert len(choices)==1
 actual=choices[0];name=s['declaration'].split()[-1]
 owner=s['owner'].rstrip('.')+('.' if s['kind']=='ModuleBlock' else '+')+name
 ts=[t for t in meta['types'] if t['type']==owner];ots=[t for t in original['types'] if t['type']==owner]
 assert len(ts)==len(ots)==1,(owner,len(ts),len(ots))
 methods=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
 before=[m for m in original['methods'] if m['declaring_type']==owner and m['member_name'] in ['.ctor','.cctor']]
 if 'LsmEstimate' in name:
  assert any(r['type']['type']==owner for r in contexts);mode='FULL_CONSTRUCTED_COMMON_PROPERTY_DEFAULT_AND_NATIVE_OWNER_COMPOSITE'
 else:
  assert Counter(json.dumps(f,sort_keys=True) for f in ts[0]['fields'])==Counter(json.dumps(f,sort_keys=True) for f in ots[0]['fields']),owner
  assert len(methods)==len(before)
  for m in methods:assert instructions(m)==instructions(next(b for b in before if b['member_name']==m['member_name'])),owner
  mode='EXACT_NATIVE_FIELDS_AND_INITIALIZER_CIL_PLUS_INDIVIDUAL_ENTIRE_CHILD_BODY_PROOFS'
 children=[mp[k] for k in s['children']]
 module_proofs.append(dict(canonical_support_id=s['support_id'],original_entry_support=s,current_support_record=actual,complete_children=children,current_type=ts[0],original_type=ots[0],current_initializers=methods,original_initializers=before,mode=mode,ownership='Windows registration/cache/error/import/session boundaries; computational preparation and existing Core execution reconciled separately for every child. No module-wide substitute for child proof.',Release=DEFERRED))
assert len(module_proofs)==32
orig_frags=load(PREVIOUS/'original-fragment-parent-crosswalk-v2.json')['rows'];original_supports=o['supports']
nonmember=[]
for f in orig_frags:
 if f['member_id'] is not None or f['covered']:continue
 # Bind each structural fragment to actual enclosing declarations or a file-level
 # Option/Imports declaration. Never pretend it belongs to a fabricated member.
 enclosing=[s['support_id'] for s in original_supports if s['path']==f['path'] and s['span_start']<=f['span_start'] and s['span_start']+s['span_length']>=f['span_start']+f['span_length']]
 assert enclosing or f['kind'] in ['OptionStatement','ImportsStatement','NamespaceBlock','NamespaceStatement','EndNamespaceStatement'],(f['fragment_id'],f['kind'])
 nonmember.append(dict(fragment=f,enclosing_original_supports=enclosing,mode='FILE_OPTION_IMPORT_OR_NAMESPACE_COMPILER_POLICY_AND_INDIVIDUAL_DECLARATION_COMPOSITE' if not enclosing else 'STRUCTURAL_DECLARATION_INDIVIDUAL_CHILD_AND_NATIVE_FIELD_INITIALIZER_COMPOSITE',source_authority='Inherited full-source source pins and original support ledgers, exact prior readers, plus current Debug contexts and full child proof; no new source identity.'))
save('support-fragment-crosswalk-v3.json',dict(new_support=module_proofs,remaining_original_structural_fragments=nonmember,current_native_fragment_count=len(u['fragments']),prior_native_fragment_count=len(old['fragments']),fragment_denominator_policy='Source syntax now has12659 physical Windows fragments rather than13695 at v16. Removed code is preserved in original/entry ledgers and matched whole-phase Common source transitions. Logical current member identities remain726; original fragments remain20683. No old fragment ID or source span is renumbered.',Release=DEFERRED,original_queries=0))
print('Support declarations',len(module_proofs),'with exact initializers/fields and child dependencies;',len(nonmember),'original structural fragments separately bound')
