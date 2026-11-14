from common import *
import importlib.util
spec=importlib.util.spec_from_file_location('w3_review_reader',HERE/'publication/verify-review-v1.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
result=reader.verify(HERE/'publication/transport-manifest-v1.json',HERE/'publication/parts-index-v1.json',repo=ROOT,extract=ROOT/'.test-rationalization-w3x.tmp')
save('publication/physical-readback-v1.json',result)
print(json.dumps(result),flush=True)
