"""Move the existing GEE delimited-text branch; no alternate parser/defaults."""
from common import *
import textwrap

paths=['BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb',
       'BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb']
all_before=json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files']
rows=[next(r for r in all_before if r['live']['path']==p) for p in paths]
assert all(pin(ROOT/r['live']['path'])==r['live'] for r in rows)
save('implementation-input-packet-plan-v1.json',dict(paths=paths,source_before=rows,
    scope='Selected GEE_FIT transitive startParams dependency. Extract its existing delimited text branch once; no new grammar/defaults or native range/error policy change.',original_queries=0))
native=(ROOT/paths[0]).read_text(encoding='utf-8-sig')
start=native.index('            Dim parts As String() = s.Split({","c, ";"c, " "c, ChrW(9)}, StringSplitOptions.RemoveEmptyEntries)')
end=native.index('        End If',native.index('            Return True',start))
before=native[start:end].rstrip()
portable=textwrap.dedent(before.replace('ChrW(9)','Convert.ToChar(9)'))
call='            Return Global.BESHStatNG.ExcelCommon.UdfModelImportPreparation38.TryGetLooseNumericTextPrepared(s, values)'
native=native[:start]+call+'\n'+native[end:]
common=(ROOT/paths[1]).read_text(encoding='utf-8-sig')
signature='Public Shared Function TryGetLooseNumericTextPrepared(s As String, ByRef values As Double()) As Boolean'
addition='        '+signature+'\n'+textwrap.indent(portable,'            ')+'\n        End Function\n\n'
marker='    End Class\nEnd Namespace'
assert common.count(marker)==1
common=common.replace(marker,addition+marker)
for path,value in zip(paths,[native,common]):write_source(path,value)
save('implementation-input-source-move-v1.json',dict(path=paths[0],portable_path=paths[1],
    original_segment=before,portable_segment=portable,native_call=call,
    portable_signature=signature,aliases=[dict(before='ChrW(9)',after='Convert.ToChar(9)')],
    after=[pin(ROOT/p) for p in paths],original_queries=0,
    byref='Native values reset/missing checks precede typed dispatch. The exact original text branch retains its allocation, per-token conversion, finite rejection and false-path resets; no extra reset is introduced in the new Common API.'))
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
plan['source_paths']+=paths;plan['source_before']+=rows
plan['additive_scope']=pin(HERE/'implementation-input-packet-plan-v1.json')
save('implementation-plan-v2.json',plan)
print('Existing GEE start-parameter text grammar moved once; native range/argument policy retained')
