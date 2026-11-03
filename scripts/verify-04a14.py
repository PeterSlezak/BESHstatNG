"""04A.14 offline harness. Raw local evidence is ignored; publication is separate.
Reuse accepted isolated runners, no historical writer overwrite, restore or native use.
"""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('verification11', ROOT / 'scripts/verify-04a11.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)
base = prior.base
BASE = 'b02480a71c1f2de45beb9cb0726a564eae15076b'


def git(*args):
    return prior.prior.prior.git(*args)


def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT / 'TestResults' or not run.name.startswith('04A-14-') or not (run / 'start.json').is_file():
        raise ValueError('Fresh initialized 04A.14 evidence required')
    return run


for module in [prior, prior.prior, prior.prior.prior, base, base.isolation]:
    module.evidence = evidence
base.projection.__globals__['evidence'] = evidence
exec(prior.prior.prior.prior.final_source.replace('scripts/verify-04a08.py', 'scripts/verify-04a14.py'), base.__dict__)


def init(args):
    assert git('branch', '--show-current') == 'codex/04A-14'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert not git('diff', '--cached', '--name-only') and not git('diff', '--name-only')
    assert shutil.disk_usage(ROOT).free > 50 * 1024**3
    run = Path(args.evidence).resolve()
    assert run.parent == ROOT / 'TestResults' and run.name.startswith('04A-14-')
    run.mkdir(exist_ok=False)
    entries = []
    for p in (ROOT / 'TestResults').rglob('*'):
        if p.is_file() and not p.is_relative_to(run):
            s = p.stat()
            entries.append(dict(path=p.relative_to(ROOT).as_posix(), bytes=s.st_size,
                                mtime_ns=s.st_mtime_ns, ctime_ns=s.st_ctime_ns, ino=s.st_ino, dev=s.st_dev))
    base.isolation.emit(run / 'historical-metadata.json', entries)
    nested = []
    for project in ['BESHStat.Core.Tests', 'BESHStat.Excel.Common.Tests', 'BESHStatNG.Test']:
        for p in (ROOT / project / 'TestResults').rglob('*'):
            if p.is_file():
                st = p.stat()
                nested.append(dict(path=p.relative_to(ROOT).as_posix(), bytes=st.st_size,
                    mtime_ns=st.st_mtime_ns, ctime_ns=st.st_ctime_ns, ino=st.st_ino, dev=st.st_dev))
    base.isolation.emit(run / 'nested-initial-metadata.json', nested)
    paths = set(git('ls-files').splitlines()) | {p.relative_to(ROOT).as_posix() for p in (ROOT / 'docs').rglob('*') if p.is_file()}
    pins = {p: base.isolation.sha(ROOT / p) for p in sorted(paths) if (ROOT / p).is_file()}
    base.isolation.emit(run / 'baseline.json', dict(head=BASE, main=BASE, branch='codex/04A-14',
        source_pins=pins, status=git('status', '--short', '--untracked-files=all')))
    protected = {p: h for p, h in pins.items() if p.startswith(('docs/', 'planning/', 'evidence/')) or p.endswith('VersionAttributes.vb')}
    base.isolation.emit(run / 'start.json', dict(source_pins=protected, historical_files=len(entries),
        disk_free_bytes=shutil.disk_usage(ROOT).free, starting_head=BASE))
    out = ROOT / 'evidence/04A14'
    out.mkdir(exist_ok=False)
    # Historical filename keys remain exclusively in the ignored raw baseline.
    public = {p: h for p, h in pins.items() if any(k in p for k in ('Ui18MMRM', 'Ui19LMM', '/MixedModel', '/MMRM', '/Lmm', 'UNAPPLIED.patch', 'MasterPlan', 'AGENTS.md')) and '/testresults/' not in p.lower()}
    base.isolation.emit(out / 'prechange-manifest.json', dict(base=BASE, branch='codex/04A-14',
        initial_tracked_worktree='CLEAN', index_empty=True, historical_files=len(entries),
        source_pins=public, private_pins_not_listed=len(pins) - len(public),
        scope='Compact selected mixed-model/dormant source pins. Full tracked/docs source pins including unrelated and historical filename keys retained in ignored local baseline; historical results retention is membership/metadata, not full byte audit.',
        P='NO', N1='NO GO', N2='NO', C2='UNMET', native_executed=False))
    print('Initialized', len(entries), 'historical files; public pins', len(public),
          'private historical filename pins', len(pins) - len(public), flush=True)
    return 0


def focused(args):
    run = evidence(args.evidence)
    code = base.command(run, args.name + '-build', ['dotnet', 'build',
        'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj', '-c', args.configuration, '--no-restore'])
    if code:
        return code
    args.action = 'common'
    args.filter = 'FullyQualifiedName~Mixed14'
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


def entry(args):
    run = evidence(args.evidence)
    codes = [base.command(run, args.name + '-Core', ['dotnet', 'test',
        'BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj', '-c', args.configuration, '--no-restore',
        '--logger', 'trx;LogFileName=Core.trx', '--results-directory', str(run / args.name / 'results')])]
    build = base.command(run, args.name + '-common-build', ['dotnet', 'build',
        'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj', '-c', args.configuration, '--no-restore'])
    codes.append(build)
    if not build:
        args.action = 'common'
        args.filter = None
        code = base.isolation.common(args)
        codes.append(code)
        build = base.windows_build(run, args.configuration, args.name)
        codes.append(build)
        if not build:
            args.action = 'legacy'
            args.name += '-windows'
            args.vstest = base.tools()[1]
            codes.append(base.isolation.common(args))
    return max(codes)


def entry_python(args):
    run = evidence(args.evidence)
    codes = []
    for label, folder in [('tools', 'BESHstatNG/tools/tests'), ('help', 'BESHStatNG_Help_MkDocs/tests'),
                          ('scripts', 'scripts/tests'), ('common', 'BESHStat.Excel.Common.Tests/src')]:
        codes.append(base.command(run, args.name + '-' + label, [sys.executable, '-B', '-m',
            'unittest', 'discover', '-s', folder, '-p', 'test_*.py', '-v']))
    return max(codes)


def capture(args):
    run = evidence(args.evidence)
    env = os.environ.copy()
    env['BESHSTAT_04A14_CAPTURE'] = 'PRECHANGE-b02480a'
    assert git('diff', '--name-only') in ('', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj')
    if args.windows:
        code = base.windows_build(run, args.configuration, args.name)
        if code:
            return code
        # The isolated execution helper inherits the process environment.
        os.environ['BESHSTAT_04A14_CAPTURE'] = env['BESHSTAT_04A14_CAPTURE']
        args.action = 'legacy'
        args.filter = 'FullyQualifiedName~Mixed14Baselines'
        args.vstest = base.tools()[1]
        return base.isolation.common(args)
    return base.command(run, args.name, ['dotnet', 'test',
        'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj', '-c', args.configuration, '--no-restore',
        '--filter', 'FullyQualifiedName~Mixed14Baselines', '--logger', 'trx;LogFileName=capture.trx',
        '--results-directory', str(run / args.name / 'results')], env)


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('action', choices=['init', 'focused', 'capture', 'entry', 'entry-python', 'final', 'execute', 'preserve', 'historical', 'semantic', 'common-python'])
    p.add_argument('--evidence', required=True)
    p.add_argument('--configuration', choices=['Debug', 'Release'], default='Debug')
    p.add_argument('--name', default='focused-01')
    p.add_argument('--windows', action='store_true')
    p.add_argument('--legacy', action='store_true')
    p.add_argument('--vstest')
    p.add_argument('--candidate-name')
    a = p.parse_args()
    raise SystemExit(dict(init=init, focused=focused, capture=capture, entry=entry, **{'entry-python': entry_python}, final=base.final,
                         execute=base.execute, preserve=base.isolation.preservation,
                         historical=prior.prior.prior.historical, semantic=base.semantic, **{'common-python': base.common_python})[a.action](a))
