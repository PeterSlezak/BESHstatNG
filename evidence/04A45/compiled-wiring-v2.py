from common45 import *
source=(HERE/'compiled-wiring-v1.py').read_text().replace('wiring-inspector-v1','wiring-inspector-v2').replace('wiring-inspector-compile-v1','wiring-inspector-compile-v2').replace("+'-v1'","+'-v2'").replace("+'-v1.json'","+'-v2.json'")
exec(compile(source,str(HERE/'compiled-wiring-v1.py'),'exec'))
