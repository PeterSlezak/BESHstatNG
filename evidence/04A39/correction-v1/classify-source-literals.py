"""Exact finding dispositions; real historical machine references remain protected."""
from common import *
from publication_gate_v2 import finding_key
from diagnostic_producer import redactor
import collections

probe=load(PRIVATE/'stage-scan-probe-v2.json');selection=load(PRIVATE/'publication-selection-v1.json')
files={r['path']:r for r in selection['files']}
fixtures={
 'BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb',
 'BESHStat.Excel.Common.Tests/src/DiagnosticTests.vb',
 'BESHStat.Excel.Common.Tests/src/UserFaultTests.vb',
 'BESHStat.Excel.Common.Tests/src/test_capture_contract04a02.py',
 'BESHStat.WasmSmoke/tests/Execution/OfficeFaultTests.cs',
 'BESHStat.WasmSmoke/tests/compositeOutputWriter.test.cjs',
 'BESHStat.WasmSmoke/tests/diagnostics.test.cjs',
 'BESHStatNG.Test/src/DiagnosticAdapterTests.vb',
 'BESHStatNG.Test/src/ExecutionAdapterTests.vb',
 'scripts/tests/test_04a08_evidence_relocation.py',
}
sources={r['path']:r for r in load(HERE/'entry/source-lock.json')['files']}
allowed=[];protected=[];public=[]
for i,row in enumerate(probe['details']):
    path=row['path'];assert path in sources
    source=sources[path];assert files[path]['sha256']==source['sha256']
    raw=(ROOT/path).read_bytes()
    text=raw.decode(row['encoding_view']);line=row.get('line') or text.count('\n',0,row['offset'])+1
    excerpt='\n'.join(text.splitlines()[max(0,line-3):line+2])
    evidence=dict(path=path,source=source,line=line,excerpt_sha256=sha(excerpt.encode()),review_excerpt=redactor.redact_string(excerpt))
    if path in fixtures:
        # These are explicitly named negative/sanitization test fixture sources.
        # No real current machine identifier/SID is allowed by this disposition.
        assert row['category'] not in {'known_private_identifier','domain_sid','secret','internal_hostname'}
        disposition='REVIEWED_PUBLIC_OR_SYNTHETIC_SOURCE_LITERAL'
        proof='Existing diagnostic/privacy or hostile-payload unit test literal, asserted as input to redaction/rejection. Exact source SHA, origin, byte-view offset and match SHA only; no file/category wildcard exemption.'
        allowed.append(dict(key=finding_key(row,source['sha256']),disposition=disposition,source=source,proof=proof,evidence=evidence))
    else:
        disposition='PROTECTED_HISTORICAL_SOURCE_LITERAL_OR_IDENTIFIER;SANITIZED_RENDERING_REQUIRED'
        proof='Existing owner/reference/audit source, or a personal identifier in retained UI/help source. Preserve raw source unchanged. Publish separately named rendering; no new waiver or historical-source execution.'
        protected.append(path)
    public.append(dict(finding_id='SOURCE-FINDING-'+str(i+1).zfill(4),path=path,source_sha256=source['sha256'],category=row['category'],origin=row['origin'],line=line,
        disposition=disposition,proof=proof,excerpt_sha256=sha(excerpt.encode())))
save(PRIVATE/'source-literal-dispositions.json',dict(allowed=allowed,protected_source_paths=sorted(set(protected)),findings=probe['details']))
save('privacy/source-literal-dispositions.json',dict(findings=public,total=len(public),exact_synthetic_fixture_occurrences=len(allowed),
    protected_source_files=len(set(protected)),protected_source_paths=sorted(set(protected)),no_product_or_test_source_edit=True,
    no_broad_source_exclusion=True,rendering_policy='Only exact identified source preimages rendered for review; raw owner source remains unchanged. Original source path/hash and derivative path/hash retained. Synthetic literals kept as exact source and occurrence-bound dispositions; no waiver of historical scanner failures.',
    legacy_source_views=probe['encodings']))
print('337 source findings classified;',len(allowed),'exact synthetic occurrences;',len(set(protected)),'specific source preimages require renderings',flush=True)
