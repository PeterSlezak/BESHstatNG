"""Retain provisional extraction bytes, then normalize only new body indentation."""
from common import *
composition=json.loads((HERE/'implementation-source-composition-v1.json').read_bytes())
files=['BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb'];rows=[]
for name in files:
    path=ROOT/name;before=pin(path);snapshot=HERE/'source-provisional/v1'/(name+'.txt');snapshot.parent.mkdir(parents=True,exist_ok=True)
    with snapshot.open('xb') as stream:stream.write(path.read_bytes())
    source=path.read_bytes().decode('utf-8-sig').replace('\r\n','\n')
    for phase in composition['phases']:
        marker='        Public Shared Function '+phase['portable']+'('
        if marker not in source:continue
        a=source.index('\n',source.index(marker))+1;b=source.index('        End Function',a);lines=source[a:b].splitlines()
        minimum=min(len(line)-len(line.lstrip()) for line in lines if line.strip())
        block='\n'.join((' '*12+line[minimum:]) if line.strip() else '' for line in lines)+'\n'
        source=source[:a]+block+source[b:];phase['portable_segment']=block
    write_source(name,source);rows.append(dict(before=before,provisional_snapshot=pin(snapshot),after=pin(path)))
save('implementation-source-composition-v2.json',dict(composition,formatting_transitions=rows,qualification='Whitespace-only preparation after exact extraction; provisional bytes retained. No tests or builds were run on provisional indentation.'))
print('Only newly extracted bodies indented; first source versions preserved')
