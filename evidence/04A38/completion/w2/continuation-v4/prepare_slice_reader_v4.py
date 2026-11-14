from common import *
s=(HERE/'verify_slice_v3.py').read_text(encoding='utf-8')
needle="facade=lookup(o['methods'],'BESHStatNG.Matrix.Matrix','MatInv');engineCalls="
assert needle in s
s=s.replace(needle,"facade=lookup(o['methods'],'BESHStatNG.Matrix.Matrix','MatInv');need(not facade['type_initializer'] and not facade['nonliteral_static_fields'] and not facade['exceptions'],'Bypassed facade has observable static/catch context');need(all(i['op'].startswith(('ldarg','ldloc','stloc','br')) or i['op'] in ['call','ret'] for i in facade['instructions']) and sum(i['op']=='call' for i in facade['instructions'])==1,'Facade does more than forward');engineCalls=")
with (HERE/'verify_slice_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
