"""Collect retained TRX, Python and Node identities without rerunning any test."""
import argparse
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]


def python_rows(text, source=None):
    rows = []
    for line in text.splitlines():
        hit = re.match(r'^(test\S+) \(([^)]+)\) \.\.\. (ok|FAIL|ERROR|skipped.*)$', line)
        if not hit:
            continue
        identity = hit[2]
        if source:
            identity = identity.replace('__main__', Path(source).stem)
        if not identity.endswith('.' + hit[1]):
            identity += '.' + hit[1]
        rows.append(dict(identity=identity, outcome=hit[3], raw=line))
    return rows


def node_rows(text):
    rows = []
    command = ''
    lines = text.splitlines()
    index = 0
    while index < len(lines):
        line = lines[index]
        if line.startswith('> '):
            command = line[2:]
        if line.startswith('\u2714 '):
            raw = [line]
            while not re.search(r' \([0-9.]+ms\)$', raw[-1]):
                index += 1
                if index >= len(lines):
                    raise ValueError('Unterminated multiline Node name')
                raw.append(lines[index])
            name = re.sub(r' \([0-9.]+ms\)$', '', '\n'.join(raw)[2:])
            rows.append(dict(command=command, identity=name, outcome='passed', raw=raw))
        index += 1
    return rows


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--evidence', type=Path, required=True)
    args = parser.parse_args()
    run = args.evidence.absolute()
    trx = json.loads((run / 'test-identities-checkpoint08-final.json').read_bytes())
    output = dict(schema='04A02-C08-final-test-identities-v1', managed=trx, configurations={})
    for config, name, seam in [('Debug', 'c8D', 's8d'), ('Release', 'c8R', 's8r')]:
        text = (run / (name + '-full.log')).read_text(encoding='utf-8')
        full = python_rows(text)
        counts = [int(v) for v in re.findall(r'^Ran (\d+) tests in ', text, re.M)]
        node = node_rows(text)
        if sum(counts) != len(full) or len(node) != 916:
            raise ValueError('Python/Node identity count mismatch: ' + str((config, counts, len(full), len(node))))
        focused = []
        for path in sorted(run.glob('focused-' + config + '-*-command.json')):
            command = json.loads(path.read_bytes())
            log = path.with_name(path.name.replace('-command.json', '.log'))
            rows = python_rows(log.read_text(encoding='utf-8'), command['command'][-1])
            focused.append(dict(command=command, log=log.relative_to(ROOT).as_posix(), tests=rows))
        unique = {r['identity'] for r in full}
        unique.update(r['identity'] for f in focused for r in f['tests'])
        managed = [t for t in trx if ('/' + config + '/') in t['trx'] or
                   ('/' + name + '-') in t['trx'] or ('/' + seam + '/') in t['trx']]
        summary = dict(managed=sum(int(t['counters']['total']) for t in managed),
                       managed_failed=sum(int(t['counters']['failed']) for t in managed),
                       managed_not_executed=sum(int(t['counters']['notExecuted']) for t in managed),
                       python_full=len(full), python_focused=sum(len(f['tests']) for f in focused),
                       python_unique=len(unique), node=len(node), node_unique=len({(r['command'], r['identity']) for r in node}))
        output['configurations'][config] = dict(summary=summary, python_full=full, python_focused=focused, node=node)
        print(config, json.dumps(summary))
    path = run / 'final-test-identities-c08.json'
    with path.open('x', encoding='utf-8') as stream:
        json.dump(output, stream, indent=2, ensure_ascii=True)
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
