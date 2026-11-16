from common47 import *
lock();source=HERE/'fixtures/HelpersObserver47v2.cs';before=(HERE/'fixtures/HelpersObserver47.cs').read_text();old='Check(Object.ReferenceEquals(ctl,controlArgs[0]),"ByRef control identity changed")';new='Check(ctlType.IsValueType&&((BESHStatNG.regression.MixedModelControl)controlArgs[0]).CovarianceOptimizerMode==BESHStatNG.regression.MixedModelCovarianceOptimizerMode.AverageInformationReml,"ByRef value-control default changed")';assert old in before
with source.open('x',encoding='utf-8',newline='\n') as f:f.write(before.replace(old,new).replace('SAME_CONTROL_WITH_CURRENT_DEFAULT_SELECTIONS','BYREF_VALUE_CONTROL_CURRENT_DEFAULT_SELECTIONS'))
folder=PRIVATE/'helpers-observer-v2';folder.mkdir();s=(HERE/'helpers-observer-v1.py').read_text();s=s[s.index('locator='):]
s=s.replace("'HelpersObserver47.exe'","'HelpersObserver47v2.exe'").replace("'helper-observer-'","'helper-observer-v2-'").replace("'fixtures/helpers-observer-v1.json'","'fixtures/helpers-observer-v2.json'")
exec(compile(s,str(HERE/'helpers-observer-v1.py'),'exec'))
