"""Assembly references distinguish dependency edges from Windows closed generic arguments."""
from common40_v3 import *
import ast
lock()
for config in ['Debug','Release']:
    destination=PRIVATE/'abi-cil-final2'/config/'assembly-references.json'
    receipt=command('final2-'+config+'-assembly-references',['powershell','-NoProfile','-NonInteractive','-ExecutionPolicy','Bypass','-File',str(HERE/'assembly-references-v1.ps1'),'-LibraryDirectory',str(PRIVATE/'final2'/config/'lib'),'-Destination',str(destination)])
    assert receipt['exit_code']==0
    for row in load(destination):
        if row['assembly'] in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:
            assert not any(any(v in name for v in ['ExcelDna','Microsoft.Office.Interop','System.Windows.Forms','BESHStatNG,']) for name in row['references']),'HOST_ASSEMBLY_REFERENCE'
p=HERE/'compiled-index-v2.py';s=p.read_text(encoding='utf-8')
s=s.replace("if 'type' in v and 'fields' in v","if isinstance(v.get('type'),str) and 'fields' in v")
s=s.replace("if ' @ BESHStat.Excel.Common,' in r['method']","if r['method'].split(' @ ',1)[1].startswith('BESHStat.Excel.Common,')")
s=s.replace("assert not offenders,'HOST_DEPENDENCY_IN_COMMON'", """old_methods={r['method']:r for r in old['methods']}
    assert all(name in old_methods and hashed(old_methods[name])==hashed(next(r for r in now['methods'] if r['method']==name)) for name in offenders),'NEW_CLOSED_GENERIC_HOST_SPECIALIZATION'
    reference_proof=pin(PRIVATE/'abi-cil-final2'/config/'assembly-references.json')""")
s=s.replace('forbidden_host_dependency_calls=offenders',"forbidden_host_assembly_references=[],reference_proof=reference_proof,inherited_Windows_closed_generic_type_arguments=offenders")
original=save
def checked_save(name,value):
    path=Path(name)
    if not path.is_absolute():path=HERE/path
    if path.exists():assert load(path)==value,'PREVIOUS_INDEX_READER_CHANGED_OUTPUT'
    else:original(path,value)
ns=dict(globals(),save=checked_save)
exec(compile(s,str(p),'exec'),ns)
