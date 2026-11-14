"""Retain the strict first proof; add explicit retained-host initializer proof."""
from capture_current import *
s=(HERE/'verify_delegated_bodies.py').read_text(encoding='utf-8').replace("'method-bodies-current-'","'method-bodies-v2-current-'").replace("'method-bodies-original-'","'method-bodies-v2-original-'")
s=s.replace('def body(r,relocate):','def body(r,relocate,retained_host_context=False):').replace("if r['type_initializer'] or r['nonliteral_static_fields']:return None","if (r['type_initializer'] or r['nonliteral_static_fields']) and not retained_host_context:return None")
s=s.replace("   ob=body(original,True);tb=body(target,False) if target else None",'''   type_name=c['method'].split('(')[0].rsplit('.',1)[0];ctor=type_name+'..cctor()'
   oc=original_il[config].get(ctor);cc=current_il[config].get(ctor)
   retained=(original['type_initializer']==wrapper['type_initializer'] and original['nonliteral_static_fields']==wrapper['nonliteral_static_fields'] and
      ((oc is None and cc is None) or (oc is not None and cc is not None and body(oc,False,True)==body(cc,False,True))))
   ob=body(original,True,retained);tb=body(target,False) if target else None''')
s=s.replace('raw=body(original,False);wb=body(wrapper,False) if wrapper else None','raw=body(original,False,retained);wb=body(wrapper,False,retained) if wrapper else None')
s=s.replace("semantics='Full instruction/literal/local/exception-arm match in both configurations; no source-body closure from a call edge alone'","semantics='Full instruction/literal/local/exception-arm match in both configurations; original/current Windows module constructors and static-field counts also match; the portable callee has no type initializer or nonliteral static fields. Host module initialization remains in the native wrapper. No body closure from a call edge alone'")
with (HERE/'verify_delegated_bodies_v2.py').open('x',encoding='utf-8') as f:f.write(s)
