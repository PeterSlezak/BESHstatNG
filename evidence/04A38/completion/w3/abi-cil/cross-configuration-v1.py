"""Explicit cross-configuration inventory; never compare optimized raw IL hashes."""
from common import *
from collections import Counter

debug=load(PRIVATE/'Debug/metadata/current.json')
release=load(PRIVATE/'Release/metadata/current.json')
dm={r['method']:r for r in debug['methods']}
rm={r['method']:r for r in release['methods']}
missing=[dm[k] for k in dm.keys()-rm.keys()]
added=[rm[k] for k in rm.keys()-dm.keys()]
signatures=[];edges=[];exceptions=[]
def calls(r):
    return [(x['op'],x['operand']) for x in r.get('instructions',[]) if x['op'] in ['call','callvirt','newobj','ldftn','ldvirtftn']]
for k in sorted(dm.keys()&rm.keys()):
    a,b=dm[k],rm[k]
    if a['signature']!=b['signature'] or a['attributes']!=b['attributes']:
        signatures.append(dict(method=k,Debug_signature=a['signature'],Release_signature=b['signature'],Debug_attributes=a['attributes'],Release_attributes=b['attributes']))
    if calls(a)!=calls(b):edges.append(dict(method=k,Debug=calls(a),Release=calls(b)))
    ae=[{k:v for k,v in x.items() if k not in ['try_start','try_end','handler_start','handler_end','filter_start']} for x in a.get('exceptions',[])]
    be=[{k:v for k,v in x.items() if k not in ['try_start','try_end','handler_start','handler_end','filter_start']} for x in b.get('exceptions',[])]
    if ae!=be:exceptions.append(dict(method=k,Debug=ae,Release=be))
public_missing=[r for r in missing if 'Public' in r['attributes'] and not r['member_name'].startswith('_Lambda$')]
save('abi-cil/cross-configuration-v1.json',dict(Debug_methods=len(dm),Release_methods=len(rm),
    Debug_only=missing,Release_only=added,signature_attribute_differences=signatures,
    ordered_call_delegate_dependency_differences=edges,exception_metadata_differences=exceptions,
    type_metadata_exact=debug['types']==release['types'],public_missing=public_missing,
    scope='Diagnostic inventory; differences require source-backed optimization dispositions, never raw IL hash equality assumptions.'))
print('Debug-only',[(r['member_name'],r['attributes']) for r in missing],flush=True)
print('Release-only',[(r['member_name'],r['attributes']) for r in added],flush=True)
print('Signature changes',len(signatures),'call-edge differences',len(edges),'exception metadata differences',len(exceptions),'types exact',debug['types']==release['types'],flush=True)
print('Changed signature methods',[r['method'] for r in signatures][:10],flush=True)
print('Changed edge methods',[r['method'] for r in edges][:15],flush=True)
