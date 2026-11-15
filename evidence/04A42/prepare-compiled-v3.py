from common42 import *
for old,new in [('compiled-and-fixtures-v2.py','compiled-and-fixtures-v3.py'),('compatibility-v2.py','compatibility-v3.py')]:
    s=(HERE/old).read_text(encoding='utf-8-sig');s=s.replace('from common42 import *','from common42 import *\n_original_command=command\ndef command(name,args,**kw):return _original_command("corrected3-"+name,args,**kw)')
    s=s.replace("PRIVATE/'compiled-v2'","PRIVATE/'compiled-v3'")
    if old.startswith('compiled'):
        s=s.replace("PRIVATE/'abi-cil-v2'","PRIVATE/'abi-cil-v3'").replace("'compiled-method-differences2-'","'compiled-method-differences3-'").replace("'-proof-v2.json',dict","'-proof-v3.json',dict").replace("'fixture-final2-'","'fixture-final3-'")
        for family in ['41','39','40','42']:s=s.replace("+'-"+family+"-v2.json'","+'-"+family+"-v3.json'")
    else:
        for a,b in [('compatibility-current-v2','compatibility-current-v3'),('-driver-v2.py','-driver-v3.py'),('-driver-provenance-v2.json','-driver-provenance-v3.json'),("'case-identities-'+config+'-v2.json'","'case-identities-'+config+'-v3.json'"),("'compatibility/'+config+'-v2.json'","'compatibility/'+config+'-v3.json'")]:s=s.replace(a,b)
    with (HERE/new).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Final compiled/current-only receipts use freshv3 paths and command labels;failed reporting attempts retained',flush=True)
