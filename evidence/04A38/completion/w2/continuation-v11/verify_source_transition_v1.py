"""Whole-file transition proof, preserving every unrelated entry source and assertion."""
from source_views import body
from proof_kernel_v3 import need
from verify_composition_v2 import PHASES
import hashlib

OLD='evidence/04A38/completion/w2/continuation-v10/'

def inspect(load,raw,prefix):
    trans=load(prefix+'source-transitions-final-v1.json')['files'];by={r['path']:r for r in trans}
    entry=load(prefix+'all-edited-source-preimages-v1.json')['files']
    for row in entry:
        if row['live']['path'] in by:continue
        data=raw(row['live']['path'])
        need((len(data),hashlib.sha256(data).hexdigest())==(row['live']['bytes'],row['live']['sha256']),'Unrelated entry source changed '+row['live']['path'])
    normalize=lambda b:b.decode('utf-8-sig').replace('\r\n','\n')
    host='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb'
    old=load(OLD+'current-universe-packet-v2.json');current=load(prefix+'current-universe-packet-v2.json')
    before=normalize(raw(by[host]['preimage']['path']));expected=before
    oldnodes={n['name']:n for n in old['members'] if n['path']==host and n['parent_member'] is None}
    nodes={n['name']:n for n in current['members'] if n['path']==host and n['parent_member'] is None}
    for name in PHASES:
        a=body(raw(by[host]['preimage']['path']),oldnodes[name])[0].replace('\r\n','\n')
        b=body(raw(host),nodes[name])[0].replace('\r\n','\n')
        need(expected.count(a)==1,'Ambiguous actual original whole-file span')
        expected=expected.replace(a,b)
    need(expected==normalize(raw(host)),'Native whole file changed outside exactly five reviewed bodies')
    common='BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb'
    before=normalize(raw(by[common]['preimage']['path']))
    portable=load(prefix+'portable-universe-v1.json');apis={n['name']:n for n in portable['members'] if n['path']==common}
    methods=[]
    for api,_,_ in PHASES.values():
        methods.append('        '+body(raw(common),apis[api])[0].replace('\r\n','\n')+'\n')
    expected=before.replace('Imports System.Globalization\n','Imports System.Globalization\nImports System.Linq\n')
    need(expected.count('    End Class')==1,'Ambiguous portable class terminator')
    expected=expected.replace('    End Class','\n'+'\n'.join(methods)+'    End Class')
    need(expected==normalize(raw(common)),'Portable whole file changed beyond five methods and required LINQ import')
    native_tests='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
    before=normalize(raw(by[native_tests]['preimage']['path']));after=normalize(raw(native_tests))
    need(after.startswith(before[:before.rfind('End Class')]),'Existing native test/assertion source changed')
    new_tests=next(r for r in trans if r['before'] is None)
    need(new_tests['path']=='BESHStat.Excel.Common.Tests/src/UdfContinuation11Tests.vb' and new_tests['entry_state']=='ABSENT','New test entry authority differs')
    need(normalize(raw(new_tests['path'])).startswith('Option Explicit On\nOption Strict On\n'),'New portable tests lost strict mode')
    for row in trans:
        if row['before']:
            need(raw(row['path']).startswith(b'\xef\xbb\xbf')==raw(row['preimage']['path']).startswith(b'\xef\xbb\xbf'),'Source BOM changed')
    return dict(unchanged_other_entry_sources=len(entry)-3,exact_native_body_edits=5,new_portable_methods=5,
        existing_test_assertions='SOURCE_PREFIX_UNCHANGED',production_test_files=4)
