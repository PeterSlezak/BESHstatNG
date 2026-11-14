"""Preserve first publication; exclude partial Git metadata from corrected derivative."""
from common import *
oldindex=W2/'continuation-v14-review-parts-index-v1.json';old=json.loads(oldindex.read_bytes());entry=json.loads((HERE/'git-entry-v1.json').read_bytes())['index_file'];assert pin(ROOT/'.git/index')==entry
snapshot=HERE/'git-entry-index-bytes-v1.bin'
with snapshot.open('xb') as f:f.write((ROOT/'.git/index').read_bytes())
assert (pin(snapshot)['bytes'],pin(snapshot)['sha256'])==(entry['bytes'],entry['sha256'])
save('publication-git-index-binding-v1.json',dict(status='EXACT_LIVE_INDEX_BYTES_RETAINED_AS_REVIEW_EVIDENCE_NOT_PARTIAL_CHECKOUT',entry=entry,snapshot=pin(snapshot),original_queries=0))
save('publication-safety-correction-v1.json',dict(first_index=pin(oldindex),first_manifest=pin(HERE/'publication-manifest-v1.json'),first_parts=old['parts'],failed_copy_command=pin(RUNROOT/'commands/w2c14-copied-default-publication-reader-v1/command.json'),failed_copy_stdout=pin(RUNROOT/'commands/w2c14-copied-default-publication-reader-v1/stdout.log'),cause='A literal .git/index from authenticated entry authority paths created a partial .git directory. The unchanged inherited C10 reader correctly required live compiled consumers. No source/CIL/reference defect or assertion waiver.',remedy='Retain first archives, manifest/index, failed extraction and streams. Corrected derivative omits Git metadata paths and retains identical index bytes under explicit evidence snapshot; genuine live checks remain required on the authoritative checkout.',policy='Reuse each exact first-inventory path/size/SHA/base-policy decision. Only new correction/source/index evidence receives fresh delta policy evaluation; no second expensive scanner campaign.',original_queries=0))
def write(name,text):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
v=(HERE/'verify_c14_preservation_v2.py').read_text();needle=" for row in load(root/(C14+'entry-authority-pins-v1.json'))['authorities']:\n  if row['path'].startswith('.test-rationalization-04A38.tmp/') and not live:"
assert v.count(needle)==1
v=v.replace(needle,""" for row in load(root/(C14+'entry-authority-pins-v1.json'))['authorities']:
  if row['path']=='.git/index' and not live:
   binding=load(root/(C14+'publication-git-index-binding-v1.json'));need(binding['entry']==row and binding['status']=='EXACT_LIVE_INDEX_BYTES_RETAINED_AS_REVIEW_EVIDENCE_NOT_PARTIAL_CHECKOUT','Wrong omitted Git index authority');check(binding['snapshot'],transition=False);need((binding['snapshot']['bytes'],binding['snapshot']['sha256'])==(row['bytes'],row['sha256']),'Missing exact retained Git index bytes')
  elif row['path'].startswith('.test-rationalization-04A38.tmp/') and not live:""")
write('verify_c14_preservation_v3.py',v)
v=(HERE/'publication_privacy_c14_v1.py').read_text();marker="stats=dict(prior_exact=0,fresh=0)"
assert v.count(marker)==1
v=v.replace(marker,"""delta_authority=json.loads((HERE/'publication-delta-policy-authority-v1.json').read_bytes());first_manifest=json.loads(checked(delta_authority['first_manifest']));first_receipt=json.loads(checked(delta_authority['first_policy_receipt']))
need(first_receipt['status']=='SELECTED_PAYLOAD_PRIVACY_POLICY_CHECKPOINT_PASS_NOT_READY' and first_receipt['manifest']==delta_authority['first_manifest'] and first_manifest['privacy_policy']==authority['base_policy'],'Wrong first checkpoint policy authority')
for row in first_manifest['entries']:
 if row['sha256'] is None:continue
 decision=row['privacy_decision'];need(decision['status']=='POLICY_EVALUATED' and decision['policy_sha256']==authority['base_policy']['sha256'] and decision['content_sha256']==row['sha256'],'Unattested first checkpoint decision')
 cache[row['path'],row['bytes'],row['sha256']]=decision['findings']
"""+marker)
write('publication_privacy_c14_v2.py',v)
save('publication-delta-policy-authority-v1.json',dict(first_manifest=pin(HERE/'publication-manifest-v1.json'),first_policy_receipt=pin(W2/'continuation-v14-publication-privacy-receipt-v1.json'),base_policy=pin(W2/'continuation-v6/publication_policy.py'),scope='Exact authenticated decisions reused for all unchanged bytes; new small corrective bookkeeping/index evidence only receives fresh unchanged policy. Full historical/Python/publication scanner campaigns are not repeated.',original_queries=0))
v=(HERE/'verify_c14_publication_v2.py').read_text().replace('publication_privacy_c14_v1','publication_privacy_c14_v2').replace('verify_c14_preservation_v2','verify_c14_preservation_v3').replace("MANIFEST=PREFIX+'publication-manifest-v1.json'","MANIFEST=PREFIX+'publication-manifest-v2.json'").replace("PREFIX+'review-exclusions-v1.json'","PREFIX+'review-exclusions-v2.json'")
write('verify_c14_publication_v3.py',v)
v=(HERE/'publish_parts_v1.py').read_text().replace('publication_privacy_c14_v1','publication_privacy_c14_v2').replace("MANIFEST=PREFIX+'publication-manifest-v1.json'","MANIFEST=PREFIX+'publication-manifest-v2.json'").replace('review-exclusions-v1.json','review-exclusions-v2.json').replace("PREVIOUS/'review-exclusions-v2.json'","PREVIOUS/'review-exclusions-v1.json'")
v=v.replace("    names={name for name in names if not name.startswith('.test-rationalization-04A38.tmp/')}","    names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v14-*') if p.is_file() and p.suffix in ['.json','.py','.md'])\n    names={name for name in names if not name.startswith('.test-rationalization-04A38.tmp/') and not name.startswith('.git/')}")
v=v.replace("names.add((W2/'continuation-v14-retained-private-reader-v1.py').relative_to(ROOT).as_posix())","names.add((W2/'continuation-v14-retained-private-reader-v2.py').relative_to(ROOT).as_posix())")
v=v.replace("excluded=previous['excluded']", "excluded=previous['excluded']+[json.loads((HERE/'publication-git-index-binding-v1.json').read_bytes())['entry']]")
v=v.replace('file-inventory-v1.json','file-inventory-v2.json').replace("save('publication-manifest-v1.json'","save('publication-manifest-v2.json'")
v=v.replace('continuation-v14-publication-privacy-receipt-v1.json','continuation-v14-publication-privacy-receipt-v2.json').replace('continuation-v14-review-parts-index-v1.json','continuation-v14-review-parts-index-v2.json').replace('continuation-v14-publication-receipt-v1.json','continuation-v14-publication-receipt-v2.json').replace("PRIVATE/'review-parts-v1'","PRIVATE/'review-parts-v2'").replace('W2_v14_','W2_v14_v2_')
v=v.replace("prior=json.loads((W2/'continuation-v13-review-parts-index-v1.json').read_bytes())\n    compressed=", "prior=json.loads((W2/'continuation-v14-review-parts-index-v1.json').read_bytes())\n    compressed=")
write('publish_parts_v2.py',v)
v=(HERE/'copy_review_v2.py').read_text().replace('verify_c14_publication_v2','verify_c14_publication_v3').replace('publication_privacy_c14_v1','publication_privacy_c14_v2').replace('continuation-v14-review-parts-index-v1.json','continuation-v14-review-parts-index-v2.json').replace('.c14p.tmp','.c14p2.tmp').replace('copied-default-publication-reader-v1','copied-default-publication-reader-v2').replace('continuation-v14-copied-review-reader-v1.json','continuation-v14-copied-review-reader-v2.json')
write('copy_review_v3.py',v)
v=(HERE/'finalize_live_v2.py').read_text().replace('verify_c14_preservation_v2','verify_c14_preservation_v3')
write('finalize_live_v3.py',v)
v=(HERE/'finalize_external_v2.py').read_text().replace('verify_c14_publication_v2','verify_c14_publication_v3').replace('verify_c14_preservation_v2','verify_c14_preservation_v3').replace('continuation-v14-retained-private-reader-v1.py','continuation-v14-retained-private-reader-v2.py').replace('continuation-v14-review-parts-index-v1.json','continuation-v14-review-parts-index-v2.json').replace('continuation-v14-publication-receipt-v1.json','continuation-v14-publication-receipt-v2.json').replace('continuation-v14-copied-review-reader-v1.json','continuation-v14-copied-review-reader-v2.json').replace('continuation-v14-publication-privacy-receipt-v1.json','continuation-v14-publication-privacy-receipt-v2.json')
needle=" 'The two historical privacy JSON ERRORs"
v=v.replace(needle," 'First seven review parts, first logical index/manifest and failed .c14p.tmp extraction remain immutable. Their literal .git/index incorrectly selected live mode in the unchanged inherited reader. Final derivative v2 uses exact index bytes as an evidence snapshot and omits .git metadata. No assertion was weakened; no full scanner/test campaign was repeated. Exact first-policy decisions are reused; only new corrective bookkeeping receives fresh delta policy evaluation. All first and final physical archives are privately pinned.', '',\n"+needle)
write('finalize_external_v3.py',v)
v=(W2/'continuation-v14-retained-private-reader-v1.py').read_text().replace("indexpath=W2/'continuation-v14-review-parts-index-v1.json'","indexpath=W2/'continuation-v14-review-parts-index-v2.json'")
needle="    for part in parts['parts']:\n        for record"
assert v.count(needle)==1
v=v.replace(needle,"    firstparts=json.loads((W2/'continuation-v14-review-parts-index-v1.json').read_bytes())\n    for part in firstparts['parts']+parts['parts']:\n        for record")
with (W2/'continuation-v14-retained-private-reader-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
print('Versioned publication correction prepared; first parts/extraction/seals retained; exact Git index moved to review evidence')
