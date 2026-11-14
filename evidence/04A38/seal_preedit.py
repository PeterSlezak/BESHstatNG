"""One-shot genuine pre-production freeze. Refuses any tracked product edit.

This receipt does not close ownership, implementation, scientific or native
gates. The 04A.37 Route A exception is neither needed nor imported here.
"""
from datetime import datetime, timezone
import json
import shutil
from preflight import ROOT, HERE, BASE, GATES, git, pin, save
from preedit_reader import inspect


def main():
    assert git('branch','--show-current').decode().strip()=='codex/04A-38'
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==BASE
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert not git('diff','--check')
    baseline=json.loads((HERE/'entry-baseline.json').read_bytes())
    assert baseline['managed']=={c:{'Passed':20507,'Failed':14} for c in ['Debug','Release']}
    assert baseline['accepted37_identity_multiset_delta']==0
    assert json.loads((HERE/'preservation-entry.json').read_bytes())['status']=='PASS_BEFORE_PRODUCT_EDITS'
    result, detailed=inspect(ROOT)
    source=HERE/'preedit/attempts/capture-v10'
    assert (source/'Capture38.cs.txt').read_bytes()==(HERE/'helpers/Capture38.cs').read_bytes()
    frozen=HERE/'preedit/frozen-v1'
    frozen.mkdir(exist_ok=False)
    for original,new in [(source/'cases.jsonl','cases.jsonl'),(source/'Capture38.cs.txt','Capture38.cs'),
                         (source/'capture-Debug-original-v1.tsv','original-Debug.tsv'),
                         (source/'capture-Release-original-v1.tsv','original-Release.tsv')]:
        shutil.copyfile(original,frozen/new)
    save('preedit/frozen-v1/independent-controls.json',result)
    # All declared input source, bindings, successful reference commands, and the
    # selected original/repeat/trace records are externally pinned. Old failed
    # attempts remain at their original versioned paths, outside this valid freeze.
    dependencies=[
        'evidence/04A38/entry-authority.json','evidence/04A38/entry-baseline.json',
        'evidence/04A38/entry-source-pins.json','evidence/04A38/entry-members.json',
        'evidence/04A38/preedit/entry-source-snapshot.json',
        'evidence/04A38/preedit/compiled-Debug.json','evidence/04A38/preedit/compiled-Release.json',
        'evidence/04A38/preedit/clone-Debug-receipt.json','evidence/04A38/preedit/clone-Release-receipt.json',
        'evidence/04A38/preedit/clone-Debug-trace-trace-v1-receipt.json',
        'evidence/04A38/preedit/clone-Release-trace-trace-v1-receipt.json',
        'evidence/04A38/clone_reference.py','evidence/04A38/capture_attempt.py',
        'evidence/04A38/trace_reference.py','evidence/04A38/preedit_reader.py',
        'evidence/04A38/test_preedit_reader.py','evidence/04A38/preservation-entry.json',
        'evidence/04A38/preedit/dependency-bindings.json',
        'evidence/04A38/preedit/attempts/capture-v3/specification.json',
    ]
    for row in json.loads((HERE/'preedit/entry-source-snapshot.json').read_bytes())['files']:
        dependencies.append(row['snapshot']['path'])
    for directory in [HERE/'preedit/native-safety',HERE/'preedit/attempts/reachability-v1',source,frozen]:
        dependencies.extend(p.relative_to(ROOT).as_posix() for p in directory.rglob('*') if p.is_file())
    for config in ['Debug','Release']:
        receipt=json.loads((HERE/('preedit/clone-'+config+'-receipt.json')).read_bytes())
        for row in receipt['source_files']:
            # The original UDF files are immutable snapshots; the accepted host
            # transport/helper bindings additionally have live entry-source pins.
            dependencies.append(row['path'])
    rows=[pin(p) for p in sorted(set(dependencies))]
    save('preedit/frozen-v1/receipt.json',dict(
        status='PRE_EDIT_FROZEN',version='04A38-original-v1',
        sealed_at_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,
        chronology='CAPTURED_AND_SEALED_BEFORE_FIRST_04A38_PRODUCT_EDIT',
        late_capture_exception=False,product_edits_at_seal=False,index_empty=True,
        branch='codex/04A-38',head=BASE,main=BASE,merge_base=BASE,
        configurations=result['configurations'],
        entry_sources=41,exports=268,behavioral_members=704,callable_members=643,
        historical_members=703,additional_accepted_member='Get2D native-coercion callback; no new export',
        files=rows,comparison_policy=dict(
            strings='Exact UTF-8/base64 text and ordered records; cultures pinned per case.',
            arrays='Exact element type, rank, lower bound, length, order, value types and payloads.',
            numbers='Exact IEEE bits, including signed zero and NaN payload/sign; separate original per configuration; no tolerance.',
            exceptions='Exact managed type, parameter, message, dispatch phase and ordered caught-error events. Reflection-only rejections are separately identified and never shipping Excel claims.',
            handles='Only random GUID entropy symbolized by creation ordinal; actual prefix, 32-hex N format, case and handle relationships retained. Original dictionaries and cleanup execute unchanged in an isolated process.',
            metadata='Declared fixture-only cached elapsed fields set to zero and UTC metadata fixed before queries; nonfinite/not-recorded markers retained. No coefficient, data, likelihood, iteration or output-cell masking.',
            source_identity='Input/model/fitted ordinals and returned labels retained; plain arrays provide no observed physical worksheet rows.'),
        native_boundary='UNREGISTERED_ORIGINAL_SOURCE_WITH_EXPLICIT_WIZARD_FALSE_AND_FAIL_CLOSED_COERCION_TEST_DOUBLES; zero native attempts. Native wizard-true, registration, range acquisition and Excel marshaling are source/metadata contracts only.',
        native='NATIVE_EXCEL_NOT_RUN',scientific='NOT_GOLDEN',gates=GATES,
        negative_audit=dict(command='configured-python -B -m unittest discover -s evidence/04A38 -p test_preedit_reader.py -v',tests=28,passed=28,failed=0,errors=0,exit_code=0,
            observed_duration_seconds=12.721,original_artifact_hashes_rechecked=True),
        exclusions='No .NET binaries/private raw logs or ignored historical TestResults payload included. Earlier attempts, nonpasses and inherited failures remain immutable.',
        readiness='NOT_READY_04A38; ownership review, consumed implementation, focused/full final validation and publication packet remain required. This freeze does not authorize a later missing-coverage supplement.'))
    print('SEALED genuine pre-production original-v1;',pin('evidence/04A38/preedit/frozen-v1/receipt.json')['sha256'])


if __name__=='__main__':main()
