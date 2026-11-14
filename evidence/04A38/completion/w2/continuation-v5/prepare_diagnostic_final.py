from common import *
from validation_report import safe_label
raw=HERE/'full-managed-Debug-v3.json'
data=json.loads(raw.read_bytes())
changes=[]
def walk(value,path):
    if isinstance(value,dict):
        for key,child in value.items():
            if key=='test_name' and isinstance(child,str):
                label=safe_label(child)
                if label!=child:
                    changes.append(dict(pointer=path+[key],original_label_sha256=sha(child.encode()),public_label_sha256=sha(label.encode()),public_label=label))
                    value[key]=label
            else:walk(child,path+[key])
    elif isinstance(value,list):
        for i,child in enumerate(value):walk(child,path+[i])
walk(data,[])
save('full-managed-Debug-public-v2.json',data)
save('diagnostic-public-derivative-v2.json',dict(raw_local=pin(raw),public=pin(HERE/'full-managed-Debug-public-v2.json'),changes=changes,
    excluded_raw_retained_locally=True,scope='Only synthetic display labels; all source/input/output/identity/outcome values unchanged'))
source=(PREVIOUS/'verify_derivative.py').read_text(encoding='utf-8').replace('diagnostic-public-derivative.json','diagnostic-public-derivative-v2.json')
with (HERE/'verify_derivative_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
import importlib.util
spec=importlib.util.spec_from_file_location('c5diagnostic',HERE/'verify_derivative_v2.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
save('diagnostic-derivative-proof-v2.json',module.inspect())
print('Only display labels changed:',len(changes))
