"""Independent complete union extraction and default copied executable proof reader."""
from common import *
import zipfile
from publication_privacy_c14_v1 import safe_path
from verify_c14_publication_v2 import inspect_parts
index=W2/'continuation-v14-review-parts-index-v1.json';doc=json.loads(index.read_bytes());physical=inspect_parts(index)
expected={row['path']:row for row in doc['files']};target=ROOT/'.c14p.tmp'
assert max(len(str(target/name)) for name in expected)<260
target.mkdir(exist_ok=False);seen=set();casefold=set()
for part in doc['parts']:
    with zipfile.ZipFile(ROOT/part['path']) as archive:
        for info in archive.infolist():
            name=info.filename;safe_path(name);assert name not in seen and name.casefold() not in casefold
            seen.add(name);casefold.add(name.casefold());row=expected[name];assert row['part']==part['name']
            data=archive.read(name);assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            path=target/name;assert path.resolve().is_relative_to(target.resolve());path.parent.mkdir(parents=True,exist_ok=True)
            with path.open('xb') as stream:stream.write(data)
assert seen==set(expected)
result=command('copied-default-publication-reader-v1',[sys.executable,'-B',str(target/HERE.relative_to(ROOT)/'verify_c14_publication_v2.py')],cwd=target)
assert result['exit_code']==0,'Retain any failed copy and its trace; never infer acceptance'
actual=json.loads((RUNROOT/'commands/w2c14-copied-default-publication-reader-v1/stdout.log').read_bytes())
receipt=dict(status='C14_EXACT_LOGICAL_UNION_AND_COPIED_EXECUTABLE_READER_PASS_NOT_READY',parts_index=pin(index),physical=physical,extracted_files=len(seen),copied_executable=clean(result),copied_result=actual,
    scope='Selected complete review only; omitted raw/private/original owner history and live Git are separately verified. No inference from copied payloads to missing authoritative files.',original_queries=0)
path=W2/'continuation-v14-copied-review-reader-v1.json'
with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(receipt,stream,ensure_ascii=False,indent=2);stream.write('\n')
print('Complete copied review proof PASS_NOT_READY',len(seen))
