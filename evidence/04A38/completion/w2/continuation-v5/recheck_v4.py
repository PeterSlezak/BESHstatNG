"""Finish the historical reader against authenticated retained C4 source bytes."""
from common import *
import importlib
sys.path.insert(0, str(PREVIOUS))
entry = json.loads((HERE / 'entry.json').read_bytes())
overrides = {r['live']['path']: (ROOT / r['preimage']['path']).read_bytes() for r in entry['source_before']}
manifest = json.loads((PREVIOUS / 'publication-manifest-v3.json').read_bytes())
pins = {r['path']: r for r in manifest['entries']}
for path, raw in overrides.items():
    assert (len(raw), sha(raw)) == (pins[path]['bytes'], pins[path]['sha256'])
result = importlib.import_module('verify_publication').inspect(
    manifest_path=(PREVIOUS / 'publication-manifest-v3.json').relative_to(ROOT).as_posix(),
    overrides=overrides, semantic=False)
save('v4-publication-historical-replay.json', dict(result=result,
    projection=[dict(path=path, bytes=len(raw), sha256=sha(raw)) for path, raw in overrides.items()],
    semantic_checks='Separate verify_slice/ownership/retained/fragment/preservation entry receipts passed before edits',
    prior_failed_attempt='verify_publication-entry.json: long scan reached live semantic checks after C5 edits; retained timing failure'))
print('C4 publication replay passed', flush=True)
