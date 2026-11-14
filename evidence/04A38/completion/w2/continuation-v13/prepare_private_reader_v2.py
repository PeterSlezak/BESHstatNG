"""Retain unexecuted predecessor; bind all current C13 command/build/execution roots."""
from common import *
source=W2/'continuation-v13-retained-private-reader-v1.py';v=source.read_text().replace("glob('w2c12-*')","glob('w2c13-*')")
assert v!=source.read_text()
v=v.replace("prior['private_pin_count']==len(prior['private_pins'])==8801","prior['private_pin_count']==len(prior['private_pins'])==8801 and len(prior['retained_prior_physical_archives'])==45")
target=W2/'continuation-v13-retained-private-reader-v2.py'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('private-reader-bounded-root-correction-v1.json',dict(before=pin(source),after=pin(target),
    reason='Pre-execution review caught a retained w2c12 glob. Successor explicitly pins w2c13 commands/builds/executions plus bounded current private root, all8801 inherited pins and45 archives. V1 retained unexecuted; no protected artifact changed.',original_queries=0))
