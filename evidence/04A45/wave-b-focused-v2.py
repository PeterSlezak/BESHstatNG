from common45 import *
source=(HERE/'wave-b-focused-v1.py').read_text().replace('wave-b-tests-compile-v1','wave-b-tests-compile-v2').replace('wave-b-obj-v1','wave-b-obj-v2').replace('wave-b-focused-v1','wave-b-focused-v2').replace('waves/b-v1','waves/b-v2')
exec(compile(source,str(HERE/'wave-b-focused-v1.py'),'exec'))
