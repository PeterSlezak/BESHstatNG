"""Read-only diagnostic and scanner investigation; raw detail stays in protected evidence."""
from common import *
import collections, difflib, importlib.util, re

lock()
current=load(PRIOR/'python-node-scanners/python-identities-v1.json')
baseline=load(ROOT/'evidence/04A38/completion/w3/python-node-scanners/python-identities-v1.json')
delta=load(PRIOR/'python-node-scanners/python-delta-v2.json')
rows=[]
for drift in delta['diagnostic_drifts']:
    event=next(e for e in current['events'] if e['identity']==drift['identity'] and e['occurrence']==drift['occurrence'] and e['subtest']==drift['subtest'])
    before=event['previous'];after=event
    rows.append(dict(identity=event['identity'],occurrence=event['occurrence'],subtest=event['subtest'],before_message=before['message'],after_message=after['message'],
        before_trace=before['traceback'],after_trace=after['traceback'],message_diff=list(difflib.unified_diff(before['message'].splitlines(),after['message'].splitlines())),trace_diff=list(difflib.unified_diff(before['traceback'].splitlines(),after['traceback'].splitlines()))))
save(PRIVATE/'diagnostic-analysis.json',rows)
print('54 diagnostic difference structures:',dict(collections.Counter((len(r['before_message']),len(r['after_message'])) for r in rows)))
print('Identity and exception-only diff summaries (no raw values):')
for r in rows:
    added=[s for s in r['message_diff'] if s.startswith('+') and not s.startswith('+++')]
    removed=[s for s in r['message_diff'] if s.startswith('-') and not s.startswith('---')]
    print(r['identity'], 'removed/added lines',len(removed),len(added),'hashword',sum('hash' in x.lower() for x in added),'diffword',sum('diff' in x.lower() for x in added))

freeze=load(PRIOR/'python-node-scanners/input-freeze-v1.json')
committed=set(git('ls-tree','-r','--name-only','main').decode().splitlines())
paths={r['path'] for r in freeze['files'] if r['path'] not in committed and not r['path'].startswith('.')} | set(git('diff','--name-only').decode().splitlines()) | {'evidence/04A39/python-node-scanners/input-freeze-v1.json'}
save(PRIVATE/'reconstructed-candidates.json',sorted(paths))
for phase in ('13','14'):
    spec=importlib.util.spec_from_file_location('privacy'+phase,ROOT/f'scripts/verify-04a{phase}-publication.py')
    scanner=importlib.util.module_from_spec(spec);spec.loader.exec_module(scanner)
    candidates=sorted(paths|{scanner.MANIFEST,scanner.PRIVACY})
    findings,reviewed=scanner.scan(candidates)
    save(PRIVATE/f'privacy-{phase}-replay.json',dict(candidates=candidates,findings=findings,reviewed=reviewed,scanner=pin(ROOT/f'scripts/verify-04a{phase}-publication.py')))
    print('Reconstructed prior privacy domain',phase,'candidates',len(candidates),'findings',len(findings),'categories',dict(collections.Counter(f['category'] for f in findings)))
    print('Finding source files',len({f['path'] for f in findings}))
