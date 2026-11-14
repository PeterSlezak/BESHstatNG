"""Reconstruct the historical candidate domain using frozen paths AND Git ignore rules."""
from common import *
import collections, importlib.util, subprocess

lock()
paths=load(PRIVATE/'reconstructed-candidates.json')
p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'check-ignore','--stdin'],input=('\n'.join(paths)+'\n').encode(),cwd=ROOT,capture_output=True)
assert p.returncode in (0,1)
ignored=set(p.stdout.decode().splitlines())
paths=set(paths)-ignored
assert all((ROOT/p).stat().st_size<100_000_000 for p in paths)
assert not any(p.startswith(('evidence/04A38/','TestResults/','.')) for p in paths)
save(PRIVATE/'stopped-reconstruction-v1.json',dict(status='STOPPED_DOMAIN_RECONSTRUCTION_ERROR',session_exit=1,reason='Initial absent-in-main filter omitted Git ignore rules; interrupted. No parent identity executed. No original modified. Replaced with ignored-path subtraction before replay.',ignored_paths=sorted(ignored)))
for phase in ('13','14'):
    spec=importlib.util.spec_from_file_location('privacy'+phase,ROOT/f'scripts/verify-04a{phase}-publication.py')
    scanner=importlib.util.module_from_spec(spec);spec.loader.exec_module(scanner)
    candidates=sorted(paths|{scanner.MANIFEST,scanner.PRIVACY})
    findings,reviewed=scanner.scan(candidates)
    save(PRIVATE/f'privacy-{phase}-replay-v2.json',dict(candidates=candidates,findings=findings,reviewed=reviewed,scanner=pin(ROOT/f'scripts/verify-04a{phase}-publication.py')))
    print('Prior-domain replay',phase,'candidates',len(candidates),'findings',len(findings),'categories',dict(collections.Counter(f['category'] for f in findings)),'source files',len({f['path'] for f in findings}),flush=True)
