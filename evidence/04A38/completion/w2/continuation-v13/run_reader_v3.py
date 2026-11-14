from common import *
from verify_c13_checkpoint_template_v3 import compute
result=compute();save('independent-static-reader-v3.json',result);print(json.dumps(result))
