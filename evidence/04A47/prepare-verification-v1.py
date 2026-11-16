from common47 import *
lock();rows=[]
def adapt(name,source,edits):
 s=(ROOT/source).read_text(encoding='utf-8-sig')
 for a,b in edits:
  assert a in s,(name,a);s=s.replace(a,b)
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
 rows.append(dict(source=pin(source),driver=pin(HERE/name),edits=edits))
adapt('managed-final-v1.py','evidence/04A46/managed-final-v1.py',[('common46','common47'),('a46-','a47-'),("['GuiWiring46'","['GuiWiring47','GuiWiring46','Regression09','Regression10','Regression11','Gee12','Mixed14','Mixed15','Mixed16','Multivariate17','Multivariate18','Multivariate19','Multivariate20','Spc21','Spc22','Spc23','Spc24','Psm25','Survival13'")])
adapt('reconcile-managed-v1.py','evidence/04A46/reconcile-managed-v1.py',[('common46','common47'),('evidence/04A45/build-and-tests/Debug-identities-v1.json','evidence/04A46/build-and-tests/Debug-identities-v1.json'),('ADDED46','ADDED47'),("['GuiWiring46StateTests','GuiWiring46ExecutionTests','GuiWiring46PolicyTests']","['GuiWiring47StateTests','GuiWiring47LongrunTests','GuiWiring47ContractTests']")])
for name in ['compiled-and-fixtures-v1.py','compatibility-v1.py','cumulative-v1.py','progress-stub-v1.py','serial-checks-v1.py','resource-boundary-v1.py','abi-ui-v1.py','release-disposition-v1.py']:
 s=(ROOT/'evidence/04A46'/name).read_text(encoding='utf-8-sig').replace('common46','common47').replace('a46-','a47-').replace('04A46.tmp','04A47.tmp')
 # Product ABI comparison to45 is a byte-exact transitive baseline,also checked to46 in final ABI supplement.
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
 rows.append(dict(source=pin('evidence/04A46/'+name),driver=pin(HERE/name),scope='Same disjoint accepted46 verification recipe;new versioned47 output roots;original assertions/test sources unchanged.'))
save('matrix/verification-driver-provenance-v1.json',dict(rows=rows,old_test_sources_modified=False,full_Release='NOT_RUN',Node='INHERITED_916_PASS;NOT_RERUN',native='NOT_RUN'))
