"""Independently extract exact logical union inside the run, then execute copied default reader."""
from common import *
import zipfile
from publication_privacy_c11_v1 import safe_path
from verify_c11_publication_v1 import inspect_parts

index_path=W2/'continuation-v11-review-parts-index-v1.json'
doc=json.loads(index_path.read_bytes());expected={r['path']:r for r in doc['files']}
physical=inspect_parts(index_path)
target=ROOT/'.c11p.tmp'
assert max(len(str(target/name)) for name in expected)<260,'Use only a bounded project-contained review path'
target.mkdir(exist_ok=False)
seen=set();casefold=set()
for part in doc['parts']:
    with zipfile.ZipFile(ROOT/part['path']) as archive:
        for item in archive.infolist():
            name=item.filename;safe_path(name)
            assert name not in seen and name.casefold() not in casefold
            seen.add(name);casefold.add(name.casefold());row=expected[name];assert row['part']==part['name']
            data=archive.read(name);assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            destination=target/name;assert destination.resolve().is_relative_to(target.resolve())
            destination.parent.mkdir(parents=True,exist_ok=True)
            with destination.open('xb') as stream:stream.write(data)
assert seen==set(expected)
result=command('copied-default-publication-reader-v1',[sys.executable,'-B',str(target/HERE.relative_to(ROOT)/'verify_c11_publication_v1.py')],cwd=target)
assert result['exit_code']==0,'Retain failed copied reader; never claim acceptance from it'
raw_result=json.loads((RUNROOT/'commands/w2c11-copied-default-publication-reader-v1/stdout.log').read_bytes())
receipt=dict(status='C11_ALL_PARTS_EXACT_UNION_AND_COPIED_EXECUTABLE_READER_PASS_NOT_READY',parts_index=pin(index_path),
    physical=physical,extracted_files=len(seen),copied_executable=clean(result),copied_result=raw_result,
    scope='Selected logical union only; no .git/omitted owner/raw private file is inferred from this copy. Every authority remains separate in final live preservation.',original_queries=0)
path=W2/'continuation-v11-copied-review-reader-v1.json'
with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(receipt,stream,ensure_ascii=False,indent=2);stream.write('\n')
print('C11 physical union and independent copied default reader PASS_NOT_READY',flush=True)
