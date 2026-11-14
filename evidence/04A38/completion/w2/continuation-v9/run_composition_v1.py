from common_v2 import *
from verify_c9_composition_v1 import inspect
result=inspect();save('source-composition-v1.json',result);print('Full source composition PASS',len(result['rows']))
