from common import *
from verify_c13_checkpoint_template_v4 import compute
result=compute();save('independent-static-reader-v4.json',result);print(json.dumps(result))
