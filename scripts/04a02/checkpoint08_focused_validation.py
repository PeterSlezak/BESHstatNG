"""Sequential focused host-free C08 and retained C01-C07 contract validation."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--evidence', type=Path, required=True)
    parser.add_argument('--configuration', choices=['Debug', 'Release'], required=True)
    args = parser.parse_args()
    run = args.evidence.absolute()
    if run.parent != ROOT / 'TestResults' or not (run / 'start.json').is_file():
        raise ValueError('Protected new root required')
    config = args.configuration
    env = os.environ.copy()
    env['BESHSTAT_04A02_G3_CANDIDATES'] = str(run / 'executions' / ('c8D-Common' if config == 'Debug' else 'c8R-Common') / 'observations')
    files = ['scripts/tests/test_04a02_checkpoint08.py', 'scripts/tests/test_04a02_checkpoint08_integration.py',
             'scripts/tests/test_04a02_checkpoint08_dossier.py',
             'BESHStat.Excel.Common.Tests/src/test_checkpoint07_contracts.py',
             'BESHStat.Excel.Common.Tests/src/test_04a02_source_specs.py',
             'BESHStat.Excel.Common.Tests/src/test_capture_contract04a02.py',
             'BESHStat.Excel.Common.Tests/src/test_investigation06.py']
    for index, source in enumerate(files):
        name = 'focused-' + config + '-' + str(index)
        command = [sys.executable, '-B', source]
        started = time.monotonic()
        with (run / (name + '.log')).open('x', encoding='utf-8') as log:
            result = subprocess.run(command, cwd=ROOT, env=env, stdout=log, stderr=subprocess.STDOUT)
        with (run / (name + '-command.json')).open('x', encoding='utf-8') as stream:
            json.dump(dict(command=command, exit_code=result.returncode, seconds=time.monotonic()-started,
                           candidate_environment=env['BESHSTAT_04A02_G3_CANDIDATES']), stream, indent=2)
        print(config, source, result.returncode, flush=True)
        if result.returncode:
            print((run / (name + '.log')).read_text()[-5000:])
            return result.returncode
    return 0


if __name__ == '__main__':
    sys.exit(main())
