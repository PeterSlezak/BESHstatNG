from common47 import *
import re,collections
lock();census=load(HERE/'scope/family-census-v1.json');owners={'BESHStatNG.'+n for n in census['owners']};old=load(ROOT/'evidence/04A45/scope/execution-chains-v1.json');rows=[]
for r in old['chains']:
 if r['owner'] in owners:
  assert pin(r['path'])['sha256']==r['source_sha256'];rows.append(dict(r,classification='SOURCE_CALL_CANDIDATE_WITH_CURRENT_COMPILED_OWNER_IDENTITY',evidence47='evidence/04A47/scope/compiled-members-v1.json',native='NOT_RUN'))
save('scope/execution-chains-v1.json',dict(chains=rows,total45=599,intersection47=len(rows),original=pin('evidence/04A45/scope/execution-chains-v1.json'),limits='Source calls/caller/argument hashes only,not a semantic interprocedural graph or proof of dynamic subscription. Current production bytes/compiled members unchanged;projection tests separately account executed edges.',native='NOT_RUN'))
baseline=load(ROOT/'evidence/04A46/build-and-tests/Debug-identities-v1.json');current=load(HERE/'build-and-tests/Debug-identities-v1.json');index={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in current['tests']};contracts=[]
for r in baseline['tests']:
 if re.search(r'Regression(?:09|10|11)|Gee12|Mixed(?:14|15|16)|Multivariate(?:17|18|19|20)|Spc(?:21|22|23|24)|Psm25|Survival13|ExecutionAdapter|WindowsHarness45',r['class_name']):
  assert index[(r['suite'],r['identity_sha256'],r['occurrence'])]['outcome']==r['outcome'];contracts.append({k:r[k] for k in ['suite','identity_sha256','occurrence','class_name','method','outcome']})
save('scope/accepted-contract-crosswalk-v1.json',dict(classification='ALREADY_TESTED_ACCEPTED_CONTRACT',tests=contracts,count=len(contracts),current_outcomes_exact=True,counted_as_new47=False,qualification='Fresh full Debug retains these accepted source/engine/report contracts and their failures;not new fake/native execution or independent statistical validation.'))
before=load(ROOT/'evidence/04A46/abi-cil/UI-and-attribution-v1.json');after=load(HERE/'abi-cil/UI-and-attribution-v1.json')
for a,b in zip(before['rows'],after['rows']):assert a['configuration']==b['configuration'] and a['export_item_digests']==b['export_item_digests'] and a['exports']==b['exports']==268
save('abi-cil/accepted46-exact-v1.json',dict(all268_items_exact46=True,configurations=['Debug','Release'],before=pin('evidence/04A46/abi-cil/UI-and-attribution-v1.json'),after=pin(HERE/'abi-cil/UI-and-attribution-v1.json'),production_methods_changed=[],generated_newline=pin(HERE/'entry/generated-newline-transition-v1.json'),native='NOT_RUN'))
print('Source-chain47 intersection',len(rows),';accepted contracts',len(contracts),';268 item digests/config exact46',flush=True)
