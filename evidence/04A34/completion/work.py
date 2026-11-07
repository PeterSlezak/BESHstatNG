"""Incremental completion helpers. Original packet/captures are immutable authorities."""
from pathlib import Path
import sys,json,re,hashlib,subprocess,csv
ROOT=Path(__file__).resolve().parents[3];HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent))
from entry import git,pin,public
def sha(b):return hashlib.sha256(b).hexdigest()
def save(name,value):
    (HERE/name).write_text(json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf-8',newline='\n')
def write(path,text):
    p=ROOT/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(text,encoding='utf-8',newline='\n')
def src(name):return (ROOT/('BESHstatNG/src/Graphics/'+name+'.vb')).read_text(encoding='utf-8-sig')
def method(text,name):
    m=re.search(r'^[ \t]*(?:Public|Private|Friend) (?:Shared )?(?:Function|Sub) '+re.escape(name)+r'\b.*?^[ \t]*End (?:Function|Sub)\b',text,re.M|re.S)
    assert m,name
    return m.group()
def header_body(raw):
    lines=raw.splitlines();i=0;balance=lines[0].count('(')-lines[0].count(')')
    while balance>0:i+=1;balance+=lines[i].count('(')-lines[i].count(')')
    return '\n'.join(lines[:i+1]),'\n'.join(lines[i+1:-1])+'\n'
def replace(text,name,new):
    old=method(text,name);assert text.count(old)==1,name
    return text.replace(old,new)
def dto(name,fields):
    return 'Public NotInheritable Class '+name+'\n'+'\n'.join('    Public ReadOnly Property '+n+' As '+t for n,t in fields)+'\n    Public Sub New('+', '.join('input'+n+' As '+t for n,t in fields)+')\n'+'\n'.join('        Me.'+n+' = input'+n for n,t in fields)+'\n    End Sub\nEnd Class\n'
HEADER='Option Explicit On\nOption Strict On\nOption Infer On\nImports System\nImports System.Linq\nImports System.Globalization\nImports System.Collections.Generic\nImports BESHStatNG\n'
if __name__=='__main__':
    assert git('branch','--show-current')=='codex/04A-34'
    assert not git('diff','--cached','--name-only')
    assert not (HERE/'entry.json').exists()
    manifest=json.loads((HERE.parent/'publication-manifest.json').read_bytes())
    for r in manifest['files']:
        if r['sha256'] is not None:
            b=(ROOT/r['path']).read_bytes();assert len(b)==r['bytes'] and sha(b)==r['sha256'],r['path']
    for name in ['publication-manifest.json','family-coverage.json','family-ownership-map.csv','operation-map-final.csv','validation.json','source-ownership.json','source-transitions.json','source-preservation.json','coordinate-and-chart-spec-contract.json','privacy-check.json']:
        write('evidence/04A34/completion/checkpoint/'+name,(HERE.parent/name).read_text(encoding='utf-8'))
    write('evidence/04A34/completion/checkpoint/owner-review.md',(ROOT/'docs/architecture/batch-04A-34-owner-review.md').read_text(encoding='utf-8'))
    names=['BulletChartExcel','SankeyPlotExcel','CumulativeDistributionPlotExcel','DumbbellPlotExcel','LadderPlotExcel','AgreementPlotExcel','ConvexHullPlot','KiteChart','PolarPlot','MultipleComparisonPlotExcel','PsmLovePlotExcel','SpcControlChartExcel']
    for n in names:write('evidence/04A34/completion/entry-source/'+n+'.vb.txt',src(n))
    files=git('ls-files').splitlines()+git('ls-files','--others','--exclude-standard').splitlines()
    save('entry.json',dict(head=git('rev-parse','HEAD'),main=git('rev-parse','main'),merge_base=git('merge-base','HEAD','main'),index='EMPTY',branch=git('branch','--show-current'),checkpoint_manifest=pin('evidence/04A34/completion/checkpoint/publication-manifest.json'),files=[dict(pin(p),path=public(p)) for p in sorted(set(files)) if (ROOT/p).is_file() and not p.startswith('evidence/04A34/completion/')],qualification='Continuation checkpoint only; original4074-entry ledger and frozen captures retained. No owner changes observed.'))
    print('Verified and preserved partial checkpoint; source snapshots frozen')
