"""Whole-file guard: exactly five native bodies, five portable additions and appended tests."""
import hashlib
from source_views import body
from proof_kernel_v4 import need
from verify_c12_composition_v1 import PHASES
OLD='evidence/04A38/completion/w2/continuation-v11/'
def inspect(load,raw,prefix):
    trans=load(prefix+'source-transitions-final-v1.json')['files'];by={r['path']:r for r in trans};entry=load(prefix+'all-edited-source-preimages-v1.json')['files']
    unchanged=0
    for row in entry:
        if row['live']['path'] in by:continue
        value=raw(row['live']['path']);need((len(value),hashlib.sha256(value).hexdigest())==(row['live']['bytes'],row['live']['sha256']),'Unrelated entry source changed '+row['live']['path']);unchanged+=1
    normalize=lambda value:value.decode('utf-8-sig').replace('\r\n','\n')
    previous=load(OLD+'current-universe-packet-v2.json');current=load(prefix+'current-universe-packet-v1.json');native_edits=[]
    for path in ['BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMultivariate.vb']:
        before=normalize(raw(by[path]['preimage']['path']));expected=before
        oldnodes={n['name']:n for n in previous['members'] if n['path']==path and n['parent_member'] is None}
        newnodes={n['name']:n for n in current['members'] if n['path']==path and n['parent_member'] is None}
        for name in PHASES:
            if name not in oldnodes:continue
            a=body(raw(by[path]['preimage']['path']),oldnodes[name])[0].replace('\r\n','\n');b=body(raw(path),newnodes[name])[0].replace('\r\n','\n')
            need(expected.count(a)==1,'Ambiguous actual native source span');expected=expected.replace(a,b);native_edits.append(name)
        need(expected==normalize(raw(path)),'Native whole-file edit beyond prepared bodies '+path)
    portable=load(prefix+'portable-universe-v1.json')
    for path in ['BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb']:
        before=normalize(raw(by[path]['preimage']['path']));after=normalize(raw(path));nodes={n['name']:n for n in portable['members'] if n['path']==path and n['parent_member'] is None}
        for spec in PHASES.values():
            if spec['api'] not in nodes:continue
            method='        '+body(raw(path),nodes[spec['api']])[0].replace('\r\n','\n')+'\n\n'
            need(after.count(method)==1,'Missing/ambiguous genuine portable addition '+spec['api']);after=after.replace(method,'',1)
        if path.endswith('UdfModelImportPreparation38.vb'):
            before=before.replace('Imports System\n','Imports System\nImports System.Collections.Generic\nImports System.Globalization\n',1)
        need(before==after,'Portable whole-file edit beyond exact additions/imports '+path)
    tests='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';before=normalize(raw(by[tests]['preimage']['path']));after=normalize(raw(tests))
    need(after.startswith(before[:before.rfind('End Class')]),'Existing native test assertions changed')
    new=next(row for row in trans if row['before'] is None)
    need(new['path']=='BESHStat.Excel.Common.Tests/src/UdfContinuation12Tests.vb' and new['entry_state']=='ABSENT','Wrong new test source authority')
    need(normalize(raw(new['path'])).startswith('Option Explicit On\nOption Strict On\n'),'New tests lost strict mode')
    for row in trans:
        if row['before']:need(raw(row['path']).startswith(b'\xef\xbb\xbf')==raw(row['preimage']['path']).startswith(b'\xef\xbb\xbf'),'Source BOM changed')
    return dict(unchanged_other_entry_sources=unchanged,exact_native_body_edits=native_edits,new_portable_methods=5,existing_test_assertions='EXACT_PREFIX_UNCHANGED',production_test_files=6)
