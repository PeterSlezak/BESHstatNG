"""Retain first generated preimages, then normalize new method indentation only."""
from common import *
doc=json.loads((HERE/'implementation-source-composition-v1.json').read_bytes());snapshots=[]
for row in doc['source_after']:
    source=ROOT/row['path'];assert pin(source)==row
    target=HERE/'source-provisional/v1'/(row['path']+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write(source.read_bytes())
    snapshots.append(dict(live=row,provisional=pin(target)))
paths={'prepared':'BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb'}
report='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb'
texts={p:(ROOT/p).read_bytes().decode('utf-8-sig').replace('\r\n','\n') for p in [report,paths['prepared']]}
for phase in doc['phases']:
    old=phase['portable_segment'];lines=old.splitlines(True)
    minimum=min(len(line)-len(line.lstrip(' ')) for line in lines if line.strip())
    after=''.join((' '*(12-minimum)+line if minimum<=12 else line[minimum-12:]) if line.strip() else line for line in lines)
    path=paths.get(phase['owner'],report)
    needle='        Public Shared Function '+phase['portable_signature']+'\n'+old
    assert texts[path].count(needle)==1
    texts[path]=texts[path].replace(needle,'        Public Shared Function '+phase['portable_signature']+'\n'+after,1)
    phase['portable_segment']=after
for path,value in texts.items():write_source(path,value)
doc.update(provisional_preimages=snapshots,previous=pin(HERE/'implementation-source-composition-v1.json'),source_after=[pin(ROOT/r['path']) for r in doc['source_after']],
    whitespace='Only new prepared method indentation normalized before any compilation or test; first source bytes and all earlier receipts retained.')
save('implementation-source-composition-v2.json',doc)
print('New Common method indentation normalized; complete first generated source snapshots retained')
