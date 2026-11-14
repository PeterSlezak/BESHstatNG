"""Authenticate live C6 before source edits; no tests or original queries."""
from common import *
import importlib.util, zipfile

def load_module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
    return module

def main():
    PRIVATE.mkdir(exist_ok=False)
    sys.path.append(str(PREVIOUS))
    checks = []
    for name, args in [('branch', ['branch', '--show-current']), ('HEAD', ['rev-parse', 'HEAD']), ('main', ['rev-parse', 'main']), ('merge-base', ['merge-base', 'HEAD', 'main']), ('index', ['diff', '--cached', '--name-only']), ('status', ['status', '--porcelain=v1']), ('diff-check', ['diff', '--check'])]:
        start = time.monotonic()
        result = subprocess.run(['git', '-c', 'safe.directory=' + ROOT.as_posix(), *args], cwd=ROOT, capture_output=True)
        checks.append(dict(check=name, exit_code=result.returncode, stdout=result.stdout.decode(), stderr=result.stderr.decode(), seconds=time.monotonic()-start))
        assert result.returncode == 0
    assert checks[0]['stdout'].strip() == 'codex/04A-38'
    assert all(row['stdout'].strip() == BASE for row in checks[1:4]) and checks[4]['stdout'] == ''
    save('git-entry-v1.json', dict(checks=checks, index='EMPTY', baseline=BASE))
    seal = json.loads((PREVIOUS/'review-seal-v5.json').read_bytes())
    assert pin(ROOT/seal['reader']['path']) == seal['reader']
    start = time.monotonic()
    ownership = load_module('c6_checkpoint_entry', PREVIOUS/'verify_checkpoint_v5.py').compute()
    save('v6-ownership-entry-v1.json', ownership)
    save('v6-reader-execution-v1.json', dict(reader=seal['reader'], exit_code=0, seconds=time.monotonic()-start, original_queries=0))
    start = time.monotonic()
    preservation = load_module('c6_preservation_entry', PREVIOUS/'verify_preservation_v1.py').inspect()
    save('v6-live-preservation-entry-v1.json', preservation)
    save('v6-preservation-execution-v1.json', dict(reader=pin(PREVIOUS/'verify_preservation_v1.py'), exit_code=0, seconds=time.monotonic()-start))
    diagnostic = load_module('c6_diagnostic_entry', PREVIOUS/'verify_derivative_v1.py').inspect()
    save('v6-private-diagnostic-entry-v1.json', diagnostic)
    manifest = json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes()); rows = manifest['entries']
    nulls = []
    for row in rows:
        if row['sha256'] is None:
            assert row['path'] == (PREVIOUS/'publication-manifest-v1.json').relative_to(ROOT).as_posix() and row['bytes'] is None
            nulls.append(row)
        else: assert pin(ROOT/row['path']) == row, row['path']
    assert len(nulls) == 1 and len(rows) == 5640
    publication = json.loads((W2/'continuation-v6-publication-receipt-v1.json').read_bytes())
    archive = RUNROOT/'completion-w2-cont6/publication-v1'/publication['archive']['name']
    assert archive.stat().st_size == publication['archive']['bytes'] and sha(archive.read_bytes()) == publication['archive']['sha256']
    with zipfile.ZipFile(archive) as source:
        names = [r.filename for r in source.infolist()]
        assert len(names) == len(set(names)) == len(rows) and set(names) == {r['path'] for r in rows}
        for row in rows:
            data = source.read(row['path'])
            if row['sha256'] is not None: assert (len(data), sha(data)) == (row['bytes'], row['sha256'])
        assert source.testzip() is None
    save('v6-publication-entry-v1.json', dict(status='LIVE_ALL_PAYLOAD_HASH_AND_STANDALONE_ZIP_HASH_CRC_PASS', hashed_payloads=5639, null_self_entries=1, manifest=pin(PREVIOUS/'publication-manifest-v1.json'), publication_receipt=pin(W2/'continuation-v6-publication-receipt-v1.json'), final_git_receipt=pin(W2/'continuation-v6-final-git-preservation-v1.json'), archive=publication['archive'], privacy='Exact authenticated bytes of the final C6 privacy-verified archive; no new C6 privacy claim', original_queries=0))
    refs = {}
    def walk(value):
        if isinstance(value, dict):
            if all(k in value for k in ['path','bytes','sha256']) and isinstance(value['path'],str) and value['sha256'] is not None:
                refs[value['path'],value['bytes'],value['sha256']] = value
            for child in value.values(): walk(child)
        elif isinstance(value,list):
            for child in value: walk(child)
    for name in ['command-receipts-v1.json','diagnostic-preservation-final-v1.json','attempt-accounting-final-v1.json','review-seal-v5.json','../continuation-v6-final-git-preservation-v1.json']:
        walk(json.loads((PREVIOUS/name).read_bytes()))
    for row in refs.values(): assert pin(ROOT/row['path']) == row
    save('v6-explicit-private-and-seal-pins-v1.json',dict(checked=list(refs.values()), all_exact=True))
    paths = ['BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb', 'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb', 'BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb', 'BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb', 'BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb', 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
    preimages = []
    for path in paths:
        original = ROOT/path; target = HERE/'source-before'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
        with target.open('xb') as stream: stream.write(original.read_bytes())
        preimages.append(dict(live=pin(original),preimage=pin(target)))
    save('implementation-plan-v1.json',dict(source_before=preimages,new_sources=[],queue=pin(PREVIOUS/'remaining-work-queue-v1.json'),scope='A trimmed copy phase; B unchanged output dependencies; C Box-M preparation and unchanged assumptions engines/reports; D paired sizing and exact ordered adjacent families only when completely provable',original_queries=0))
    save('entry-v1.json',dict(status='LIVE_C6_AUTHENTICATED_BEFORE_C7_SOURCE_EDITS', historical=ownership['historical'],current=ownership['current'],support=ownership['support'],fragments=ownership['fragments'],instructions=pin(ROOT/'AGENTS.md'),master_plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),reader=pin(HERE/'entry_v1.py'),original_queries=0,interruption='Initial read-only authentication ran before server restart; no C7 source edits or tests existed. The uncompleted exhaustive privacy invocation has no reusable completion receipt. Corrected entry authenticates every stored ZIP member and CRC against final C6 receipt without repeating historical regex scans.'))
    print('C7 live entry PASS; no original execution, restore, tests or source changes',flush=True)

if __name__ == '__main__': main()
