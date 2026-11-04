"""Offline 04A.15 verification. No restore, native host, packaging or historical writers.

Reuse the accepted isolated verification workflows without modifying their sources.
Raw receipts stay local; public evidence is sealed separately.
"""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
os.environ['GIT_CONFIG_COUNT'] = '1'
os.environ['GIT_CONFIG_KEY_0'] = 'safe.directory'
os.environ['GIT_CONFIG_VALUE_0'] = ROOT.as_posix()
spec = importlib.util.spec_from_file_location('verification14', ROOT / 'scripts/verify-04a14.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)
base = prior.base


def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT, text=True).strip()


def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT / 'TestResults' or not run.name.startswith('04A-15-') or not (run / 'start.json').is_file():
        raise ValueError('Fresh initialized 04A.15 evidence required')
    return run


for module in [prior, prior.prior, prior.prior.prior, prior.prior.prior.prior, base, base.isolation]:
    module.evidence = evidence
base.projection.__globals__['evidence'] = evidence
exec(prior.prior.prior.prior.prior.final_source.replace('scripts/verify-04a08.py', 'scripts/verify-04a15.py'), base.__dict__)


def init(args):
    assert git('branch', '--show-current') == 'codex/04A-15'
    head = git('rev-parse', 'HEAD')
    assert head == git('rev-parse', 'main')
    assert not git('diff', '--cached', '--name-only') and not git('diff', '--name-only')
    run = Path(args.evidence).resolve()
    assert run.parent == ROOT / 'TestResults' and run.name.startswith('04A-15-')
    assert shutil.disk_usage(ROOT).free > 50 * 1024**3
    run.mkdir(exist_ok=False)
    entries, nested = [], []
    for folder in [ROOT / 'TestResults', *(ROOT / p / 'TestResults' for p in
                   ['BESHStat.Core.Tests', 'BESHStat.Excel.Common.Tests', 'BESHStatNG.Test'])]:
        for p in folder.rglob('*'):
            if p.is_file() and not p.is_relative_to(run):
                s = p.stat()
                row = dict(path=p.relative_to(ROOT).as_posix(), bytes=s.st_size,
                           mtime_ns=s.st_mtime_ns, ctime_ns=s.st_ctime_ns, ino=s.st_ino, dev=s.st_dev)
                (entries if folder == ROOT / 'TestResults' else nested).append(row)
    emit = base.isolation.emit
    emit(run / 'historical-metadata.json', entries)
    emit(run / 'nested-initial-metadata.json', nested)
    paths = set(git('ls-files').splitlines())
    pins = {p: base.isolation.sha(ROOT / p) for p in sorted(paths) if (ROOT / p).is_file()}
    emit(run / 'baseline.json', dict(head=head, main=head, branch='codex/04A-15', source_pins=pins,
                                    status=git('status', '--short', '--untracked-files=all')))
    protected = {p: h for p, h in pins.items() if p.startswith(('docs/', 'planning/', 'evidence/')) or p.endswith('VersionAttributes.vb')}
    emit(run / 'start.json', dict(source_pins=protected, historical_files=len(entries),
                                 disk_free_bytes=shutil.disk_usage(ROOT).free, starting_head=head))
    public = {p: h for p, h in pins.items() if any(k in p for k in
              ('Ui18MMRM', 'Ui19LMM', '/MixedModel', '/Mixed14', '/LegacyMixed14', 'UNAPPLIED.patch', 'MasterPlan', 'AGENTS.md'))
              and '/testresults/' not in p.lower()}
    out = ROOT / 'evidence/04A15'
    out.mkdir(exist_ok=False)
    emit(out / 'prechange-manifest.json', dict(base=head, branch='codex/04A-15', index_empty=True,
         tracked_worktree='CLEAN', initial_untracked=['scripts/verify-04a15.py'], source_pins=public,
         historical_files=len(entries), nested_historical_files=len(nested),
         scope='Selected source SHA-256; all tracked source hashes and historical membership/metadata retained locally. Not an all-historical-byte audit.',
         baseline_registry_sha256=base.isolation.sha(ROOT / 'evidence/04A14/baseline-registry.json'),
         restriction_crosswalk_sha256=base.isolation.sha(ROOT / 'evidence/04A14/restriction-crosswalk.json'),
         P='NO', N1='NO GO', N2='NO', C2='UNMET'))
    print('Initialized', len(entries), 'historical files;', len(nested), 'nested;', len(public), 'source pins', flush=True)
    return 0


def focused(args):
    run = evidence(args.evidence)
    code = base.command(run, args.name + '-build', ['dotnet', 'build',
        'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj', '-c', args.configuration, '--no-restore'])
    if code:
        return code
    args.action = 'common'
    args.filter = 'FullyQualifiedName~Mixed15|FullyQualifiedName~Mixed14'
    code = base.isolation.common(args)
    if code or not args.windows:
        return code
    code = base.windows_build(run, args.configuration, args.name)
    if code:
        return code
    args.action = 'legacy'
    args.name += '-windows'
    args.vstest = base.tools()[1]
    return base.isolation.common(args)


def docs(args):
    return base.command(evidence(args.evidence), args.name, [sys.executable, '-B', '-m', 'mkdocs', 'build',
        '--config-file', 'BESHStatNG_Help_MkDocs/mkdocs.latest.yml', '--site-dir', str(evidence(args.evidence) / args.name), '--strict'])


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('action', choices=['init', 'focused', 'entry', 'entry-python', 'final', 'execute', 'preserve', 'historical', 'semantic', 'common-python', 'docs'])
    p.add_argument('--evidence', required=True)
    p.add_argument('--configuration', choices=['Debug', 'Release'], default='Debug')
    p.add_argument('--name', default='focused-01')
    p.add_argument('--windows', action='store_true')
    p.add_argument('--legacy', action='store_true')
    p.add_argument('--vstest')
    p.add_argument('--candidate-name')
    a = p.parse_args()
    raise SystemExit(dict(init=init, focused=focused, entry=prior.entry, **{'entry-python': prior.entry_python},
        final=base.final, execute=base.execute, preserve=base.isolation.preservation,
        historical=prior.prior.prior.prior.historical, semantic=base.semantic,
        **{'common-python': base.common_python}, docs=docs)[a.action](a))
