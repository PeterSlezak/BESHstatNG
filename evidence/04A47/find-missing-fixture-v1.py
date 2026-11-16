from common47 import *
import zipfile
lock();target='evidence/04A23/multivariate-spc-same-core-comparisons-v1.json';expected=load(ROOT/'evidence/04A24/frozen-capture-hashes.json')['files'][target];assert not (ROOT/target).exists()
matches=[];archives=[]
# Central-directory search only;not a fresh historical full ZIP/payload union audit.
for folder in ['.test-rationalization-04A38.tmp','.test-rationalization-04A39.tmp','.test-rationalization-04A39-correction-v1.tmp']:
 for p in (ROOT/folder).rglob('*.zip'):
  try:
   with zipfile.ZipFile(p) as z:
    found=[i for i in z.infolist() if i.filename.replace('\\','/').endswith(target)]
    for i in found:
     raw=z.read(i);matches.append(dict(archive=pin(p),member=i.filename,bytes=len(raw),sha256=sha(raw),matches_expected=len(raw)==expected['bytes'] and sha(raw)==expected['sha256']))
    archives.append(dict(path=p.relative_to(ROOT).as_posix(),central_directory_searched=True))
  except zipfile.BadZipFile:archives.append(dict(path=p.relative_to(ROOT).as_posix(),status='NOT_A_READABLE_ZIP;NO_MUTATION'))
save('preservation/missing-fixture-search-v1.json',dict(target=target,missing_in_current_repository=True,expected=expected,matched_members=matches,searched_archive_directories=len(archives),scope='Only known38/39 stores and ZIP central directories;matching payloads hashed if found. Not W3 full union proof.',restored=False))
print('Searched',len(archives),'known ZIP central directories;',len(matches),'matching member names;',sum(m['matches_expected'] for m in matches),'exact expected payloads',flush=True)
