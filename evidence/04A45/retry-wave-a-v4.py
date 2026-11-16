from common45 import *
source=(HERE/'retry-wave-a-v3.py').read_text().replace('wave-a-v3','wave-a-v4').replace('waves/a-v3','waves/a-v4')
exec(compile(source,str(HERE/'retry-wave-a-v3.py'),'exec'))
