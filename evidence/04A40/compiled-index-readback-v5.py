"""Reuse successful reference extraction; append-only equality-check existing indices."""
from common40_v3 import *
lock()
for config in ['Debug','Release']:
    for row in load(PRIVATE/'abi-cil-final2'/config/'assembly-references.json'):
        if row['assembly'] in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:
            assert not any(any(v in name for v in ['ExcelDna','Microsoft.Office.Interop','System.Windows.Forms','BESHStatNG,']) for name in row['references'])
p=HERE/'compiled-index-v4.py';text=p.read_text(encoding='utf-8');text=text[text.index("p=HERE/'compiled-index-v2.py'"):]
text=text.replace("ns=dict(globals(),save=checked_save)","s=s.replace('from common40_v3 import *\\n','')\nns=dict(globals(),save=checked_save)")
exec(compile(text,str(p),'exec'))
