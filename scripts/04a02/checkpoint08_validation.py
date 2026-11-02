"""Sequential validation in an already protected C08 root; exclusive logs/receipts."""
import argparse
import json
from pathlib import Path
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--evidence', type=Path, required=True)
    parser.add_argument('--configuration', choices=['Debug', 'Release'], required=True)
    parser.add_argument('--name', required=True)
    args = parser.parse_args()
    run = args.evidence.absolute()
    if run.parent != ROOT / 'TestResults' or not (run / 'start.json').is_file():
        raise ValueError('Protected new root required')
    config, name = args.configuration, args.name
    commands = [
        ('prepare', [sys.executable, '-B', 'scripts/verify-04a02-fixtures.py', 'prepare', '--evidence', str(run),
                     '--configuration', config, '--name', name]),
        ('full', ['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
                  str(run / config / name / 'verify-isolated.ps1'), '-Legacy', '-Wasm', '-Js', '-Python',
                  '-VisualStudio', '2026', '-PythonExe', sys.executable, '-Configuration', config]),
        ('seams', [sys.executable, '-B', 'BESHStatNG.Test/C07.HostFreeSeamRunner.py', '--evidence', str(run),
                   '--configuration', config, '--name', 's8d' if config == 'Debug' else 's8r']),
    ]
    # Preparation can already exist only when its exclusive provenance was created
    # in this checkpoint; never re-create or replace that file.
    if (run / config / name / 'verify-isolated.ps1').exists():
        commands = commands[1:]
    for label, command in commands:
        started = time.monotonic()
        path = run / (name + '-' + label + '.log')
        with path.open('x', encoding='utf-8') as log:
            result = subprocess.run(command, cwd=ROOT, stdout=log, stderr=subprocess.STDOUT)
        with (run / (name + '-' + label + '-command.json')).open('x', encoding='utf-8') as stream:
            json.dump(dict(command=command, cwd=str(ROOT), exit_code=result.returncode,
                           seconds=time.monotonic() - started, log=path.relative_to(ROOT).as_posix()), stream, indent=2)
        print(label, config, result.returncode, flush=True)
        if result.returncode:
            print(path.read_text(encoding='utf-8', errors='replace')[-5000:], flush=True)
            return result.returncode
    return 0


if __name__ == '__main__':
    sys.exit(main())
