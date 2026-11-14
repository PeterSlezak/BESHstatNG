from common import *
from verify_diagnostic_derivative import inspect
save('diagnostic-derivative-verification.json',inspect())
s=(HERE/'publish_v2.py').read_text(encoding='utf-8').replace("included='All current C2", "private_diagnostic_exclusion=json.loads((HERE/'diagnostic-public-derivative.json').read_bytes())['raw_local'],rejected_prior_manifest='v1 is preserved forensic privacy-rejected attempt; its excluded raw diagnostic is unavailable from derivative',included='All current C2")
with (HERE/'publish_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
records=[]
for folder in sorted((RUNROOT/'commands').glob('w2c2-*')):
 if (folder/'command.json').is_file():records.append(dict(label=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
save('command-receipts-closeout-v2.json',dict(commands=records,scope='All completed local continuation commands through v2 public-reader/adverse checks; final export verification receipts remain external to avoid recursive archive/manifest hashing',raw_source_diagnostics='Private raw logs and raw full-managed diagnostic receipt retained locally; public test identity labels explicitly derived without changing source/statistical/reference/trace data'))
