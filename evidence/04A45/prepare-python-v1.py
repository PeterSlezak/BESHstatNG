from common45 import *
lock();provenance=[]
def write(n,s):
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
source=ROOT/'evidence/04A44/python-campaign-v2.py';s=source.read_text(encoding='utf-8-sig').replace('common44','common45')
s=s.replace("source=ROOT/'.test-rationalization-04A43.tmp/python-tools-v1'","source=ROOT/'.test-rationalization-04A44.tmp/python-tools-v2'")
s=s.replace("'.test-rationalization-04A43.tmp/python-private-v1','.test-rationalization-04A44.tmp/python-private-v2'","'.test-rationalization-04A44.tmp/python-private-v2','.test-rationalization-04A45.tmp/python-private-v1'")
s=s.replace("'.test-rationalization-04A43.tmp/executions/a43-final-Debug-Common-semantic/observations','.test-rationalization-04A44.tmp/executions/a44-final-Debug-Common-semantic/observations'","'.test-rationalization-04A44.tmp/executions/a44-final-Debug-Common-semantic/observations','.test-rationalization-04A45.tmp/executions/a45-final-Debug-Common-semantic/observations'")
s=s.replace("BASE='3955a6e9142256df6bc1fd53a924010c58367b51'","BASE='243b6dcd654767ffedb8fe3643b310c0c56cd6ac'")
s=s.replace("\"ROOT/'.test-rationalization-04A43.tmp'\",\"ROOT/'.test-rationalization-04A44.tmp'\"","\"ROOT/'.test-rationalization-04A44.tmp'\",\"ROOT/'.test-rationalization-04A45.tmp'\"")
s=s.replace("'a43pyv1-','a44pyv2-'","'a44pyv2-','a45pyv1-'")
s=s.replace("target=PRIVATE/'python-tools-v2'","target=PRIVATE/'python-tools-v1'").replace("(PRIVATE/'python-private-v2').mkdir()","(PRIVATE/'python-private-v1').mkdir()")
s=s.replace("ROOT/'.test-rationalization-04A43.tmp/python-tools-v1/python-discovery-inventory-v1.json'","ROOT/'.test-rationalization-04A44.tmp/python-tools-v2/python-discovery-inventory-v1.json'")
s=s.replace('max_workers=3','max_workers=6').replace('exact_accepted43_module_identity_multiset','exact_accepted44_module_identity_multiset').replace('python/discovery-v2.json','python/discovery-v1.json')
write('python-campaign-v1.py',s)
source2=ROOT/'evidence/04A44/reconcile-python-v3.py';s=source2.read_text(encoding='utf-8-sig').replace('common44','common45').replace("target=PRIVATE/'python-tools-v2'","target=PRIVATE/'python-tools-v1'")
s=s.replace('evidence/04A43/python/parent-event-reconciliation-v2.json','evidence/04A44/python/parent-event-reconciliation-v2.json').replace('python/discovery-v2.json','python/discovery-v1.json').replace('workers_at_peak=3','workers_at_peak=6')
write('reconcile-python-v1.py',s)
save('python/driver-provenance-v1.json',dict(campaign_source=pin(source),campaign=pin(HERE/'python-campaign-v1.py'),reconciliation_source=pin(source2),reconciliation=pin(HERE/'reconcile-python-v1.py'),original_test_sources_modified=False,changes='Private disjoint paths,accepted44 comparison,reviewed-main context and six-worker receipt;original test bodies and retained scanner policy unchanged'))
