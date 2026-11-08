"""Bounded live tracked-byte, lineage and protected-capture preservation. No payload sweep."""
from pathlib import Path
import hashlib
import json
import subprocess

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
SOURCES = [
 'BESHStat.Core/src/Graphics/MultivariatePlotGeometry37.vb',
 'BESHStat.Excel.Common/src/Results/MultivariateChartPresentation37.vb',
 'BESHstatNG/src/Graphics/PcaPlotExcel.vb',
 'BESHstatNG/src/Graphics/FactorAnalysisPlotExcel.vb',
 'BESHstatNG/src/Graphics/ClusterAnalysisPlotExcel.vb',
 'BESHstatNG/src/Graphics/CorrespondenceAnalysisPlotExcel.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37Cases.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37GeometryTests.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37Original.vb',
 'BESHStat.Core.Tests/src/Graphics/Graphics37OriginalSeries.vb',
 'BESHStat.Excel.Common.Tests/src/Graphics37CurrentPrefixes.vb',
 'BESHStat.Excel.Common.Tests/src/Graphics37PresentationTests.vb',
 'BESHStatNG.Test/src/Charts/Graphics37AdapterTests.vb',
 'BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj',
 'BESHStatNG.Test/BESHStatNG.Tests.vbproj',
 'docs/architecture/batch-04A-37-owner-review.md']


def sha(b):
    return hashlib.sha256(b).hexdigest()


def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT).decode('utf-8-sig').strip()


def pin(path):
    b = (ROOT / path).read_bytes()
    return dict(path=path, bytes=len(b), sha256=sha(b))


def save(name, value):
    (HERE / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')


def main():
    entry = json.loads((HERE / 'entry-tracked-pins.json').read_bytes())['files']
    live_paths = git('ls-files').splitlines()
    by_digest = {sha(p.encode())[:24]: p for p in live_paths}
    changes = []; baseline = {}; missing = []
    for row in entry:
        path = row['path']
        if path.startswith('<PRIVATE_PROTECTED_PATH:'):
            path = by_digest[path.split(':')[1][:-1]]
        baseline[path] = row
        assert (ROOT / path).is_file(), 'Missing tracked entry payload'
        actual = pin(path)
        if (actual['bytes'], actual['sha256']) != (row['bytes'], row['sha256']):
            assert path in SOURCES, 'Unexpected tracked modification: ' + path
            changes.append(dict(path=path, entry_bytes=row['bytes'], entry_sha256=row['sha256'], final_bytes=actual['bytes'], final_sha256=actual['sha256'], reason='Authorized04A37 consumed adapter or additive test project links'))
    assert len(entry) == len(live_paths) == 4504
    assert len(changes) == 6
    for row in json.loads((HERE / 'entry-authority.json').read_bytes())['owner_documents_excluded']:
        assert row == pin(row['path'])
        assert row['path'] not in SOURCES
    lineage = []
    prior = json.loads((ROOT / 'evidence/04A36/lineage-check.json').read_bytes())['pins']
    for row in prior:
        p = row['path']; actual = pin(p)
        assert baseline[p]['sha256'] == row['final36_sha256'], ('Entry lineage drift', p)
        changed = actual['sha256'] != row['final36_sha256']
        assert not changed or p.startswith('BESHstatNG/src/Graphics/') and p in SOURCES
        lineage.append(dict(path=p, original_04A28_sha256=row['original_04A28_sha256'], accepted36_sha256=row['final36_sha256'],
            merged_main_sha256=baseline[p]['sha256'], final37_sha256=actual['sha256'],
            transition='AUTHORIZED_04A37_ADAPTER_SUCCESSOR' if changed else 'PRESERVED_ENTRY_BYTES'))
    assert len(lineage) == 278
    save('lineage-check.json', dict(status='PASS', version='04A37-v1', pins=lineage,
        qualification='All original278 assertions and accepted29 canonical corrections/30-36 transitions remain immutable; four authorized current adapter successors, no retroactive old-pin success.'))
    captures = []
    for p, row in json.loads((ROOT / 'evidence/04A14/baseline-registry.json').read_bytes())['files'].items():
        actual = pin(p)
        assert actual['sha256'] == row['sha256']
        captures.append(actual)
    assert len(captures) == 100
    save('mixed-model-captures-check.json', dict(status='PASS_EXACT_BYTES', count=100, files=captures, scientific='NOT_GOLDEN'))
    save('preservation.json', dict(status='PASS_EXACT_ENTRY_BYTES', tracked_entry_files=4504,
        unchanged_tracked_files=4504-len(changes), authorized_tracked_changes=changes,
        unexpected_changes=[], owner_documents_unchanged=16, owner_documents_in_manifest=0,
        historical_evidence_dossiers_and_test_sources='All tracked predecessors and old test sources preserved; only the two approved test projects have additive links.',
        ignored_historical_payload='NOT_REHASHED; metadata-only historic-census-metadata.json; no cleanup or restructure.',
        packaged_dependencies='All4504 tracked entry files were present and byte-checked; ignored/untracked packaged dependencies are not claimed as verified bytes.',
        archive_boundary='Live Git proof, not an uploaded ZIP completeness claim.'))
    save('source-transitions.json', dict(files=[dict(pin(p), entry_sha256=baseline.get(p, {}).get('sha256')) for p in SOURCES],
        framework_targets='UNCHANGED netstandard2.0 Core/Common; Windows net48; modern tests net10.0',
        protected_engines='PCA/Factor/CA/Cluster statistical sources unchanged', next_batch='04A38_NOT_STARTED'))
    check = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'diff', '--check'], cwd=ROOT, capture_output=True, text=True)
    assert check.returncode == 0
    assert not git('diff', '--cached', '--name-only')
    head = git('rev-parse', 'HEAD')
    assert head == git('rev-parse', 'main') == git('merge-base', 'HEAD', 'main') == 'a0680f546c5130fa5143c0b0c7136f852d650cc5'
    assert git('branch', '--show-current') == 'codex/04A-37'
    save('git-final.json', dict(branch='codex/04A-37', head=head, main=head, merge_base=head, index='EMPTY',
        full_diff_check_exit=check.returncode, full_diff_check_output=check.stdout+check.stderr,
        status_short=git('status', '--short').splitlines(), diff_stat=git('diff', '--stat'),
        publication='UNSTAGED_UNCOMMITTED; NO_PUSH_MERGE_UPLOAD_DEPLOY', next_batch='04A38_NOT_STARTED'))
    print('PASS4504 tracked entry bytes; six authorized changes;278 lineage pins;100 exact protected captures;16 owner documents unchanged; full diff-check0; empty index')


if __name__ == '__main__':
    main()
