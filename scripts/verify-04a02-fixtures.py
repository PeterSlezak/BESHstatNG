"""04A.02 test-only isolation; never execute a Common assembly in repository bin.

All outputs use exclusive creation beneath a new 04A.02 evidence root. The live
verification script is preserved; a hash-bound, diff-recorded execution copy
redirects TRX and replaces Common execution with a fully copied assembly sandbox.
No packages are restored or downloaded by these commands.
"""
import argparse
import difflib
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


def emit(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('x', encoding='utf-8', newline='\n') as stream:
        json.dump(data, stream, indent=2)
        stream.write('\n')


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def execution_name(name):
    if not re.fullmatch(r'[A-Za-z0-9][A-Za-z0-9-]*', name):
        raise ValueError('Simple unique execution name required')
    return name


def evidence(value):
    result = Path(value).resolve()
    if result.parent != ROOT / 'TestResults' or not result.name.startswith('04A-02-initial-fixtures-'):
        raise ValueError('Expected a new top-level 04A.02 evidence root')
    if not (result / 'start.json').is_file() or not (result / 'historical-metadata.json').is_file():
        raise ValueError('Preflight and historical protection snapshot required')
    return result


def common(args):
    run = evidence(args.evidence)
    execution_name(args.name)
    execution = run / 'executions' / args.name
    legacy = args.action == 'legacy'
    base = execution / 'sandbox' / ('Legacy' if legacy else 'Common') / 'bin' / args.configuration / ('net48' if legacy else 'net10.0')
    if execution.exists():
        raise ValueError('Execution names cannot be reused')
    source = (ROOT / 'BESHStatNG.Test' / 'bin' / args.configuration if legacy else
              ROOT / 'BESHStat.Excel.Common.Tests' / 'bin' / args.configuration / 'net10.0')
    shutil.copytree(source, base)
    data_source = ROOT / 'BESHStatNG.Test' / 'TestData'
    if legacy:
        # The legacy project does not copy its source TestData into bin.
        # Loaders probe base/TestData first, so supply an immutable checked copy.
        shutil.copytree(data_source, base / 'TestData', dirs_exist_ok=True)
    for relative in ('../../../../TestResults/Codex/Batch0205/Wire', '../../../../TestResults/Codex/Batch0207'):
        if not (base / relative).resolve().is_relative_to(execution / 'sandbox'):
            raise ValueError('Internal writer would escape sandbox')
    copied = []
    for original in sorted(source.rglob('*')):
        if original.is_file():
            target = base / original.relative_to(source)
            digest = sha(original)
            if digest != sha(target):
                raise ValueError('Assembly/dependency/TestData copy mismatch')
            copied.append(dict(path=original.relative_to(source).as_posix(), sha256=digest))
    if legacy:
        for original in sorted(data_source.rglob('*')):
            if original.is_file():
                target = base / 'TestData' / original.relative_to(data_source)
                digest = sha(original)
                if digest != sha(target):
                    raise ValueError('Legacy source TestData copy mismatch')
                copied.append(dict(path='TestData/' + original.relative_to(data_source).as_posix(), sha256=digest))
    env = os.environ.copy()
    env['BESHSTAT_04A02_SOURCE_ROOT'] = str(ROOT)
    env['BESHSTAT_04A02_OBSERVATIONS'] = str(execution / 'observations')
    env['BESHSTAT_KR_EXPORT_DIR'] = str(execution / 'csv-exports')
    assembly = 'BESHStatNG.Test.dll' if legacy else 'BESHStat.Excel.Common.Tests.dll'
    command = ([args.vstest] if legacy else ['dotnet', 'vstest']) + [str(base / assembly),
               '/Logger:trx;LogFileName=' + ('Legacy' if legacy else 'Common') + '.trx', '/ResultsDirectory:' + str(execution / 'results')]
    if args.filter:
        command.append('/TestCaseFilter:' + args.filter)
    started = time.monotonic()
    with (execution / 'test.log').open('x', encoding='utf-8') as log:
        result = subprocess.run(command, cwd=ROOT, env=env, stdout=log, stderr=subprocess.STDOUT)
    record = dict(command=command, exit_code=result.returncode, seconds=time.monotonic() - started,
                  copied=copied, observations=[p.relative_to(run).as_posix() for p in execution.rglob('*.candidate.json')])
    emit(execution / 'execution.json', record)
    print((execution / 'test.log').read_text(encoding='utf-8'))
    return result.returncode


# Full lines, not partial fragments: any changed or duplicated command requires review.
SCRIPT_ANCHORS = {
    'root': "$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path",
    'results': "$results = Join-Path $root 'TestResults\\Codex'",
    'python': "    $resolvedPython = & (Join-Path $PSScriptRoot 'resolve-beshstatng-python.ps1') -PythonExe $PythonExe",
    'core': "Invoke-Checked 'dotnet' @('test', '.\\BESHStat.Core.Tests\\BESHStat.Core.Tests.vbproj', '-c', $Configuration, '--logger', 'trx;LogFileName=Core.trx', '--results-directory', $results)",
    'common': "Invoke-Checked 'dotnet' @('test', '.\\BESHStat.Excel.Common.Tests\\BESHStat.Excel.Common.Tests.vbproj', '-c', $Configuration, '--logger', 'trx;LogFileName=Common.trx', '--results-directory', $results)",
    'office-test': "    Invoke-Checked 'dotnet' @('test', '.\\BESHStat.OfficeAddin\\tests\\OfficeAddin.Tests.csproj', '-c', $Configuration, '--logger', 'trx;LogFileName=OfficeAddin.trx', '--results-directory', $results)",
    'office-build': "    Invoke-Checked 'dotnet' @('build', '.\\BESHStat.OfficeAddin\\BESHStat.OfficeAddin.csproj', '-c', $Configuration)",
    'smoke-test': "    Invoke-Checked 'dotnet' @('test', '.\\BESHStat.WasmSmoke\\tests\\Execution\\Execution.Tests.csproj', '-c', $Configuration, '--logger', 'trx;LogFileName=OfficeExecution.trx', '--results-directory', $results)",
    'smoke-build': "    Invoke-Checked 'dotnet' @('build', '.\\BESHStat.WasmSmoke\\BESHStat.WasmSmoke.csproj', '-c', $Configuration)",
    'legacy-build': "    $legacyArguments = @('.\\BESHStatNG.Test\\BESHStatNG.Tests.vbproj', '/t:Build', \"/p:Configuration=$Configuration\", '/m')",
    'legacy-test': '    Invoke-Checked $vstest @($dll, \'/Logger:trx;LogFileName=Legacy.trx\', "/ResultsDirectory:$results")',
}


def transform_script(original, run, config, name):
    execution_name(name)
    if config not in ('Debug', 'Release'):
        raise ValueError('Only Debug/Release verification is authorized')
    lines = original.splitlines()
    for key, anchor in SCRIPT_ANCHORS.items():
        if lines.count(anchor) != 1:
            raise ValueError('Expected exactly one unchanged script anchor: ' + key)
    expected_dotnet = {SCRIPT_ANCHORS[k] for k in ('core', 'common', 'office-test', 'office-build', 'smoke-test', 'smoke-build')}
    actual_dotnet = [line for line in lines if line.lstrip().startswith("Invoke-Checked 'dotnet'")]
    if set(actual_dotnet) != expected_dotnet or len(actual_dotnet) != 6:
        raise ValueError('Unexpected dotnet command; refusing implicit restore or execution')
    relative = run.relative_to(ROOT).as_posix()
    if not re.fullmatch(r'TestResults/04A-02-initial-fixtures-[A-Za-z0-9-]+', relative):
        raise ValueError('Unsafe evidence path for PowerShell literal')
    helper = "Invoke-Checked $PythonExe @('-B', 'scripts/verify-04a02-fixtures.py', '{action}', '--evidence', '" + relative + "', '--configuration', $Configuration, '--name', '" + name + "-{suffix}'{extra})"
    replacements = {
        'root': '$root = (Get-Location).Path',
        'results': "$results = Join-Path $root '" + relative + '/' + config + '/' + name + "/results'",
        'python': "    $resolvedPython = & (Join-Path $root 'scripts/resolve-beshstatng-python.ps1') -PythonExe $PythonExe",
        'common': "Invoke-Checked 'dotnet' @('build', '.\\BESHStat.Excel.Common.Tests\\BESHStat.Excel.Common.Tests.vbproj', '-c', $Configuration, '--no-restore')\n" + helper.format(action='common', suffix='Common', extra=''),
        'legacy-test': '    ' + helper.format(action='legacy', suffix='Legacy', extra=", '--vstest', $vstest"),
    }
    for key in ('core', 'office-test', 'smoke-test'):
        replacements[key] = SCRIPT_ANCHORS[key].replace("'--logger'", "'--no-restore', '--logger'")
    for key in ('office-build', 'smoke-build'):
        replacements[key] = SCRIPT_ANCHORS[key][:-1] + ", '--no-restore')"
    copied = original
    for key, replacement in replacements.items():
        # Anchors were checked as unique full lines; replacement is line-based too.
        copied = '\n'.join(replacement if line == SCRIPT_ANCHORS[key] else line for line in copied.split('\n'))
    for line in copied.splitlines():
        if line.lstrip().startswith("Invoke-Checked 'dotnet'") and "'--no-restore'" not in line:
            raise ValueError('Implicit restore remains after transformation')
    return copied


def prepare(args):
    run = evidence(args.evidence)
    execution_name(args.name)
    source = ROOT / 'scripts' / 'verify-beshstatng.ps1'
    pins = json.loads((run / 'start.json').read_bytes()).get('source_pins', {})
    if pins.get('scripts/verify-beshstatng.ps1') != sha(source):
        raise ValueError('Verification source hash differs from preflight; no repinning')
    original = source.read_text(encoding='utf-8')
    copied = transform_script(original, run, args.configuration, args.name)
    destination = run / args.configuration / args.name / 'verify-isolated.ps1'
    destination.parent.mkdir(parents=True, exist_ok=True)
    with destination.open('x', encoding='utf-8', newline='\n') as stream:
        stream.write(copied)
    emit(destination.with_suffix('.provenance.json'), dict(source=source.relative_to(ROOT).as_posix(), source_sha256=sha(source),
         execution_copy_sha256=sha(destination), diff=''.join(difflib.unified_diff(original.splitlines(True), copied.splitlines(True)))))
    print(destination.relative_to(ROOT))
    return 0


def preservation(args):
    run = evidence(args.evidence)
    before = json.loads((run / 'historical-metadata.json').read_bytes())
    errors = []
    expected = {entry['path'] for entry in before}
    for entry in before:
        path = ROOT / entry['path']
        if not path.is_file():
            errors.append('Missing ' + entry['path'])
            continue
        stat = path.stat()
        actual = (stat.st_size, stat.st_mtime_ns, stat.st_ctime_ns, stat.st_ino, stat.st_dev)
        if actual != tuple(entry[key] for key in ('bytes', 'mtime_ns', 'ctime_ns', 'ino', 'dev')):
            errors.append('Metadata drift ' + entry['path'])
    current = {p.relative_to(ROOT).as_posix() for p in (ROOT / 'TestResults').rglob('*') if p.is_file() and not p.is_relative_to(run)}
    errors.extend('Unexpected historical-root membership ' + p for p in sorted(current - expected))
    pins = json.loads((run / 'start.json').read_bytes())['source_pins']
    errors.extend('Source pin drift ' + p for p, digest in pins.items() if sha(ROOT / p) != digest)
    record = dict(status='PASS' if not errors else 'FAIL', historical_files=len(before), errors=errors,
                  method='Complete initial path membership, size, creation/write ns, file identity; frozen source hashes. No claim of rereading all historical bytes.',
                  disk_free_bytes=shutil.disk_usage(ROOT).free)
    emit(run / ('preservation-' + args.name + '.json'), record)
    print(json.dumps(record, indent=2))
    return int(bool(errors))


def summarize(args):
    run = evidence(args.evidence)
    ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    runs = []
    for path in sorted(run.rglob('*.trx')):
        tree = ET.parse(path)
        definitions = {n.attrib['id']: n.find('t:TestMethod', ns).attrib for n in tree.findall('.//t:UnitTest', ns)}
        results = []
        for node in tree.findall('.//t:UnitTestResult', ns):
            method = definitions.get(node.attrib['testId'], {})
            results.append(dict(identity=method.get('className', '') + '.' + method.get('name', '') + ' :: ' + node.attrib['testName'], outcome=node.attrib['outcome']))
        runs.append(dict(trx=path.relative_to(ROOT).as_posix(), counters=tree.find('.//t:Counters', ns).attrib, results=results))
    emit(run / ('test-identities-' + args.name + '.json'), runs)
    for item in runs:
        print(item['trx'], item['counters'])
    return 0


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['common', 'legacy', 'prepare', 'preservation', 'summarize'])
    parser.add_argument('--evidence', required=True)
    parser.add_argument('--configuration', choices=['Debug', 'Release'], default='Debug')
    parser.add_argument('--name', default='focused-Debug')
    parser.add_argument('--filter')
    parser.add_argument('--vstest')
    arguments = parser.parse_args()
    sys.exit(dict(common=common, legacy=common, prepare=prepare, preservation=preservation, summarize=summarize)[arguments.action](arguments))
