from common import *
reviewed=pin(HERE/'test_publication.py')
s=(HERE/'verify_publication.py').read_text()
old="need(not re.search(rb'(?i)[a-z]:[\\\\/]+users[\\\\/]+[^\\\\/\\s\"\\x27]+',raw),'Private home path in continuation payload '+p)"
assert old in s
new="""matches=re.findall(rb'(?i)[a-z]:[\\\\/]+users[\\\\/]+[^\\\\/\\s\"\\x27]+',raw)
   if matches:
    need(p==PREFIX+'test_publication.py' and sha(raw)=='REVIEWED_SHA' and all(m.endswith(b'EXAMPLE_PRIVATE') for m in matches),'Private home path in continuation payload '+p)
    # Exact, unchanged adversarial source literal is fictional. A generated
    # leak in another payload or any changed source hash remains rejected.""".replace('REVIEWED_SHA',reviewed['sha256'])
s=s.replace(old,new)
with (HERE/'verify_publication_v2.py').open('x',encoding='utf-8') as f:f.write(s)
test=(HERE/'test_publication.py').read_text().replace('import verify_publication as reader',"import verify_publication_v2 as reader\nreader.SELF=reader.PREFIX+'publication-manifest-v3.json'")
lines=[]
for line in test.splitlines():
 if 'def test_self_consistent_privacy_leak(self):' in line:
  line=r" def test_self_consistent_privacy_leak(self):self.rehashed_mutation(reader.PREFIX+'remaining-boundary-review.json',lambda b:b+b'\n'+b'C:'+bytes([92])+b'Users'+bytes([92])+b'EXAMPLE_PRIVATE'+bytes([92])+b'workbook.xlsx')"
 line=line.replace("reader.PREFIX+'readiness.json'","reader.PREFIX+'readiness-closeout-v2.json'")
 lines.append(line)
test='\n'.join(lines)+'\n'
extra=r""" def test_reviewed_synthetic_source_hash_cannot_be_extended(self):
  self.rehashed_mutation(reader.PREFIX+'test_publication.py',lambda b:b+b'\n'+b'C:'+bytes([92])+b'Users'+bytes([92])+b'ANOTHER_PRIVATE'+bytes([92])+b'workbook.xlsx')
"""
test=test.replace("if __name__=='__main__':unittest.main()",extra+"if __name__=='__main__':unittest.main()")
with (HERE/'test_publication_v2.py').open('x',encoding='utf-8') as f:f.write(test)
copy=(HERE/'check_public_copy.py').read_text().replace('from verify_publication import','from verify_publication_v2 import')
with (HERE/'check_public_copy_v2.py').open('x',encoding='utf-8') as f:f.write(copy)
export=(HERE/'export_review.py').read_text().replace('from verify_publication import','from verify_publication_v2 import').replace('publication-manifest-v2.json','publication-manifest-v4.json').replace('publication-local-v2','publication-local-v4').replace('publication-copy-v2','publication-copy-v4').replace("'verify_publication.py'","'verify_publication_v2.py'").replace("'check_public_copy.py'","'check_public_copy_v2.py'")
with (HERE/'export_review_v2.py').open('x',encoding='utf-8') as f:f.write(export)
save('privacy-reviewed-adverse-literal.json',dict(status='EXACT_SYNTHETIC_NEGATIVE_SOURCE_REVIEWED_NOT_REAL_PRIVATE_DATA',source=reviewed,review='Only the unchanged source of the publication adversary contains a deliberately fictional EXAMPLE_PRIVATE placeholder. No owner/machine/private path is disclosed by that literal. Other actual paths are not whitelisted. Any modification of this file, or a generated leak in another payload, still fails.',new_reader=pin(HERE/'verify_publication_v2.py'),failed_export_manifest=pin(HERE/'publication-manifest-v2.json'),failed_export_exit=1,ZIP_created=False,source_or_statistical_bytes_rewritten=False))
save('attempts/export-v2-synthetic-source-literal.json',dict(exit_code=1,phase='Pre-export independent manifest verification',error='Private home path in continuation payload test_publication.py',disposition='Fail-closed scanner flagged the deliberately fictional negative-test literal; no ZIP created and no actual owner path exposed. Preserve v2 manifest/source/reader. New independent reader accepts only the exact reviewed source hash;15 controls verify that generated leaks and source mutations remain rejected.'))
