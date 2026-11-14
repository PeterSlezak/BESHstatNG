from common import *
from validation_report import safe_label
raw=HERE/'full-managed-Debug-v1.json'
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
save('full-managed-Debug-public-v1.json',data)
save('diagnostic-public-derivative.json',dict(raw_local=pin(raw),public=pin(HERE/'full-managed-Debug-public-v1.json'),changes=changes,
    excluded_raw_retained_locally=True,scope='Only synthetic display labels; all source/input/output/identity/outcome values unchanged'))
source=(PREVIOUS/'verify_derivative.py').read_text(encoding='utf-8')
with (HERE/'verify_derivative.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
import importlib.util
spec=importlib.util.spec_from_file_location('c5diagnostic',HERE/'verify_derivative.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
save('diagnostic-derivative-proof.json',module.inspect())
print('Only display labels changed:',len(changes))
