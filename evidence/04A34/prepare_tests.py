"""Copy frozen legacy fixtures into test builds; only absolute namespace binding changes."""
from entry import ROOT,HERE,save,pin,sha
from integrate import write
if __name__=='__main__':
    rows=[]
    for name in ['LegacySpecialty34','LegacyReference34']:
        path='BESHStat.Core.Tests/src/Graphics/'+name+'.vb'
        b=(HERE/(name+'.vb')).read_text()
        write(path,b.replace('Namespace BESHStatNG.TestFixtures34','Namespace Global.BESHStatNG.TestFixtures34'))
        rows.append(dict(source=pin('evidence/04A34/'+name+'.vb'),compiled=pin(path),only_change='Namespace BESHStatNG.TestFixtures34 -> Namespace Global.BESHStatNG.TestFixtures34; prevent test-project RootNamespace nesting.'))
    path='BESHStatNG.Test/src/Charts/LegacyLayouts34.vb'
    write(path,(HERE/'LegacyLayouts34.vb').read_text().replace('Namespace BESHStatNG.TestFixtures34','Namespace Global.BESHStatNG.TestFixtures34'))
    rows.append(dict(source=pin('evidence/04A34/LegacyLayouts34.vb'),compiled=pin(path),only_change='Absolute Global namespace binding.'))
    save('fixture-binding-adjustments.json',dict(files=rows,scientific='NOT_GOLDEN'))
