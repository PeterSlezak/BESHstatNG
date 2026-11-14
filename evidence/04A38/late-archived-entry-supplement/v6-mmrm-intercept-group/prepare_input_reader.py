"""Derive independent scalar input assertions after, and anchored to, the seal."""
from pathlib import Path
import re
HERE=Path(__file__).resolve().parent;OLD=HERE.parent/'v5-mmrm-unmatched-profile'
text=(OLD/'verify_inputs.py').read_text(encoding='utf-8')
text=text.replace('v5-mmrm-unmatched-profile/','v6-mmrm-intercept-group/').replace('4d8621915832c5a795838e9b2282b75bf75503e6d1dd98b1fa97cb098953c2cb','ea476dcd4b1e60c4ddb19cc47a25e01f32a440cbe6ffb2f57928e6b086db8954')
q=(HERE/'owner-authorization.json').read_text(encoding='utf-8');import json
quote=json.loads(q)['owner_quote']
a=text.index('QUOTE=');b=text.index('\nKIND=',a);text=text[:a]+'QUOTE='+repr(quote)+text[b:]
text=text.replace("approval['approval']=='E'","approval['approval']=='F'").replace('Approval E','Approval F').replace("'approvalD']","'approvalD','approvalE']")
text=text.replace('mmrm-lsmeans-wide-visit-99-unmatched','mmrm-lsmeans-scalar-intercept-group').replace('AUTHORIZED-MMRM-UNMATCHED/','AUTHORIZED-MMRM-INTERCEPT/')
a=text.index("   need(args[1]==dict(kind='Array'");b=text.index("   need(c['session']",a)
text=text[:a]+'''   need(args[1]==dict(kind='String',value='Intercept'),'Not exact scalar String Intercept')
   typed=materialized['cases'][i]
   need(typed['case_id']==c['case_id'] and typed['culture']==typed['ui_culture']==c['culture'] and typed['group_runtime_type']=='System.String','Wrong actual .NET scalar/culture')
   need(typed['group']=='SSW50ZXJjZXB0' and typed['alpha']=='N','Materialized scalar literal/alpha differ')
'''+text[b:]
a=text.index("  guards=[r for r in info['instrumentation']");b=text.index('\n return dict',a)
text=text[:a]+'''  guards=[r for r in info['instrumentation'] if r['event']=='guard.intercept']
  need(len(guards)==1 and guards[0]['member']=='TryGetDesignColumnValues' and guards[0]['archived_source_line']==1478 and guards[0]['source_anchor']=='String.Equals(h.FixedEffectNames(idx), "(Intercept)", StringComparison.OrdinalIgnoreCase) Then' and guards[0]['after_anchor'] is True,'Missing original1477-1478 true-arm probe')
'''+text[b:]
text=text.replace('UNMATCHED_TYPED_INPUT_SOURCE_SEAL_PASS','INTERCEPT_TYPED_INPUT_SOURCE_SEAL_PASS')
with (HERE/'verify_inputs.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
