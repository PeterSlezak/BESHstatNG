from common43 import *
s=(HERE/'fixtures/Observe43V2.cs').read_text(encoding='utf-8-sig').replace('Observe43V2','Observe43V3').replace('Host.GetType("BESHStatNG.WorksheetFunctions."+module)','Host.GetType("BESHStatNG.WorksheetFunctions."+module,true,true)')
with (HERE/'fixtures/Observe43V3.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'capture-v2.py').read_text(encoding='utf-8-sig').replace('fixtures-before-v2','fixtures-before-v3').replace('Observe43V2','Observe43V3').replace('before-compile-v2','before-compile-v3').replace('before-execute-v2','before-execute-v3')
with (HERE/'capture-v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Filename/module capitalization resolved against actual CLR types without changing source;failed captures preserved',flush=True)
