"""Retain failed long-path attempt; verify unchanged sealed ZIP at short copy root."""
import sys, json, hashlib, zipfile, textwrap
from pathlib import Path
HERE = Path(__file__).resolve().parent / 'continuation-v7'
sys.path.insert(0, str(HERE))
from common import *
from verify_publication_v1 import inspect, inspect_zip
folder = PRIVATE / 'publication-v1'
archive = folder / 'BESHstatNG_04A38_W2_continuation_v7_review.zip'
reproduction = folder / 'determinism-reproduction-v1.zip'
manifest = HERE / 'publication-manifest-v1.json'
rows = json.loads(manifest.read_bytes())['entries']
with archive.open('rb') as stream: digest = hashlib.file_digest(stream, 'sha256').hexdigest()
with reproduction.open('rb') as stream: repeat = hashlib.file_digest(stream, 'sha256').hexdigest()
assert digest == repeat and archive.stat().st_size == reproduction.stat().st_size
physical = inspect_zip(archive)
print('Retained ZIP complete SHA/CRC/privacy PASS; two serializations exact', flush=True)
copied = ROOT / '.c7-review-copy-v2.tmp'
assert max(len(str(copied / row['path'])) for row in rows) < 260
copied.mkdir(exist_ok=False)
with zipfile.ZipFile(archive) as source:
    for item in source.infolist():
        target = copied / item.filename
        assert target.resolve().is_relative_to(copied.resolve())
        target.parent.mkdir(parents=True, exist_ok=True)
        with target.open('xb') as stream: stream.write(source.read(item.filename))
result = command('publication-copied-reader-v2', [sys.executable, '-B', str(copied / HERE.relative_to(ROOT) / 'verify_publication_v1.py')], cwd=copied)
assert result['exit_code'] == 0, 'Retain every failed copied attempt'
copy_result = json.loads((RUNROOT / 'commands/w2c7-publication-copied-reader-v2/stdout.log').read_bytes())
# Exact copied inventory bytes and executable verification also bind the live
# selected inventory; live Git is separately checked by postseal_v1.py.
live = dict(status='LIVE_AND_COPIED_SELECTED_BYTES_EXACT_NOT_READY', copied_result=copy_result)
for row in rows:
    assert (ROOT / row['path']).read_bytes() == (copied / row['path']).read_bytes()
recovery = dict(executor=pin(Path(__file__)), retained_first_attempt=dict(command='w2c7-publication-copied-reader-v1', exit_code=1, reason='Extended-length Windows path containing authenticated sibling ..', stdout=pin(RUNROOT / 'commands/w2c7-publication-copied-reader-v1/stdout.log'), stderr=pin(RUNROOT / 'commands/w2c7-publication-copied-reader-v1/stderr.log')), unchanged_manifest=pin(manifest), short_copy=copied.relative_to(ROOT).as_posix(), sealed_payload_edits=0)
# Reuse the sealed publication's unchanged five adverse controls and receipt
# construction. This is invocation recovery, never an assertion/reader patch.
sealed_source = (HERE / 'publish_v1.py').read_text(encoding='utf-8')
tail = textwrap.dedent(sealed_source[sealed_source.index('    controls=[]'):sealed_source.rindex('\nelse:raise ValueError')])
assert tail.count("receipt=dict(status=") == 1
tail = tail.replace('receipt=dict(status=', 'receipt=dict(recovery=recovery,status=', 1)
exec(compile(tail, str(Path(__file__)) + ':sealed-receipt-tail', 'exec'))
