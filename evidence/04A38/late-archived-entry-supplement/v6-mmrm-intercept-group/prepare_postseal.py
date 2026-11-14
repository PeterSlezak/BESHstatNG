"""Prepare F current driver and independent assertions only after original seal."""
from pathlib import Path
import hashlib,re
HERE=Path(__file__).resolve().parent;OLD=HERE.parent/'v5-mmrm-unmatched-profile'
SEAL='262229291122a20199982d8278f96e5d37a0531f4fdecf204c33c9db7e0748c0'
assert hashlib.sha256((HERE/'original-output-seal.json').read_bytes()).hexdigest()==SEAL
text=(OLD/'execute_current.py').read_text(encoding='utf-8');text=re.sub("SEAL='[0-9a-f]+'",'SEAL='+repr(SEAL),text)
with (HERE/'execute_current.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(OLD/'verify_supplement.py').read_text(encoding='utf-8').replace('edcad8369d6d3db5c9bdb7c61518a01951059b8fbc7ff2f3326738c858ab28da',SEAL)
text=text.replace("PREVIOUS='evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/publication-manifest-closeout-v1.json'", "PREVIOUS='evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/publication-manifest-closeout-v2.json'")
a=text.index('ERROR=');b=text.index('\ndef time(',a)
text=text[:a]+'''ERROR='the intercept column cannot be used as a grouping factor.'
MESSAGE='BESH.REGR.MMRM_LSMEANS error: '+ERROR
EVENTS=['public.alpha_missing=True','public.scalar_group=Intercept','helper.entry=Intercept','initialized.error=N','initialized.values=N','initialized.resolved_name=N','lookup.index=0','lookup.resolved_name=Intercept','guard.intercept=True','guard.values=N','guard.resolved_name=N','guard.error_assigned='+ERROR,'public.error='+ERROR,'public.values=N','public.resolved_name=N','public.group_rejected=True']
'''+text[b:]
text=text.replace("true_guard_original_line=1426", "true_guard_original_lines=[1477,1478,1479,1480]")
a=text.index("    private_ByRef=dict(");b=text.index(',events=EVENTS))',a)
text=text[:a]+"    private_ByRef=dict(resolvedName=None,values=None,errorMessage=ERROR,observation='TRACE_ONLY_PRIVATE_STATE')"+text[b:]
text=text.replace("unmatched_guard_cases=2", "intercept_guard_cases=2").replace('MMRM_UNMATCHED_PROFILE_EVIDENCE_PASS_NOT_BATCH_READY','MMRM_INTERCEPT_GROUP_EVIDENCE_PASS_NOT_BATCH_READY')
text=text.replace('lateMMRMunmatched=2)', 'lateMMRMunmatched=2,lateMMRMintercept=2)')
text=text.replace("'Lost real fixed-design/session provenance')", "'Lost real fixed-design/session provenance')")
text=text.replace("need(re.fullmatch('MMRM:[0-9a-f]{32}',session['raw_handle']),'Synthetic handle')", "need(re.fullmatch('MMRM:[0-9a-f]{32}',session['raw_handle']),'Synthetic handle')\n    need(session['intercept_indices']==[0],'Original/current actual fitted intercept index differs')")
with (HERE/'verify_supplement.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
for name in ['verify_packet.py','test_packet.py','publish_gate.py','validate_gate.py','validate_prior.py']:
 text=(OLD/name).read_text(encoding='utf-8').replace('mmrm38-v5','mmrm38-v6').replace('BOUNDED_V5_UNMATCHED_PROFILE_GATE_ONLY','BOUNDED_V6_INTERCEPT_GROUP_GATE_ONLY').replace('unmatched_profile_evidence','intercept_group_evidence')
 if name=='validate_prior.py':
  text=text.replace(" lanes=[", " v5=p+'late-archived-entry-supplement/v5-mmrm-unmatched-profile'\n lanes=[('unmatched-reader',[v5+'/verify_supplement.py']),('unmatched-closeout-reader',[v5+'/verify_closeout.py','v2']),")
  text=text.replace("('duplicate-adverse',v4,", "('unmatched-adverse',v5,'test_supplement.py'),('unmatched-manifest-adverse',v5,'test_packet.py'),('old-group-gap-adverse',v5,'test_group_gap.py'),('unmatched-closeout-adverse',v5,'test_closeout.py'),('duplicate-adverse',v4,")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
