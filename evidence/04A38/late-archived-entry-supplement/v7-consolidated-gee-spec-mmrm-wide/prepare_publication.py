from common import *
s=(V6/'publish_gate.py').read_text(encoding='utf-8').replace('mmrm38-v6','consolidated38-v7').replace('BOUNDED_V6_INTERCEPT_GROUP_GATE_ONLY','BOUNDED_V7_APPROVAL_G_GATE_ONLY').replace("intercept_group_evidence='2/2 exact each configuration; publication adverse gate pending'","approval_G_evidence='4/4 exact each configuration; publication adverse gate pending'")
with (HERE/'publish_gate.py').open('x',encoding='utf-8') as f:f.write(s)
s=(V6/'validate_prior.py').read_text(encoding='utf-8')
anchor=' def run(lane):'
extra=""" for name,folder,pattern in [('intercept-adverse',str(V6),'test_supplement.py'),('intercept-packet-adverse',str(V6),'test_packet.py'),('w1-adverse',str(W1),'test_w1_v2.py'),('w1-publication-adverse',str(W1),'test_publication.py')]:
  lanes.append((name,['-m','unittest','discover','-s',folder,'-p',pattern,'-v']))
 lanes.extend([('intercept-reader',[str(V6/'verify_supplement.py')]),('w1-publication-reader',[str(W1/'verify_publication.py'),'v2'])])
"""
s=s.replace(anchor,extra+anchor)
with (HERE/'validate_prior.py').open('x',encoding='utf-8') as f:f.write(s)
print('Publication/prior commands ready')
