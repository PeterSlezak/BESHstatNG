from common47 import *
lock();source=HERE/'fixtures/GeeObserver47.vb';folder=PRIVATE/'gee-observer-v2';folder.mkdir()
s=(HERE/'gee-observer-v1.py').read_text();s=s[s.index('locator='):]
s=s.replace("['System.dll','System.Core.dll','System.Web.Extensions.dll']","['System.dll','System.Core.dll','System.Web.Extensions.dll','Facades/netstandard.dll']").replace("'GeeObserver47.exe'","'GeeObserver47v2.exe'").replace("'GEE-observer-'","'GEE-observer-v2-'").replace("'fixtures/gee-observer-v1.json'","'fixtures/gee-observer-v2.json'")
exec(compile(s,str(HERE/'gee-observer-v1.py'),'exec'))
