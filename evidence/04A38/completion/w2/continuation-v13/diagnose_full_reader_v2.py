"""Actual unsealed final reader replay with full uncaught diagnostic traceback."""
from common import *
from verify_c13_checkpoint_template_v2 import compute
result=compute()
save('independent-static-reader-v2.json',result)
print(json.dumps(result))
