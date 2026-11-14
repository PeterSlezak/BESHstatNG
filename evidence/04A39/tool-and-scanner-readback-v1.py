"""Postcampaign tool byte authentication and exact serial-output delta, reporting only."""
from common39 import *

baseline=load(W3/'matrix/tool-fingerprints-v1.json');tools=[]
for row in baseline['external_tools']:
    path=Path(row['path'])
    with path.open('rb') as stream:actual=hashlib.file_digest(stream,'sha256').hexdigest()
    assert path.stat().st_size==row['bytes'] and actual==row['sha256']
    tools.append(dict(name=path.name,bytes=row['bytes'],sha256=actual,exact_W3=True,path_scope='OWNER_EXTERNAL_TOOL;PRIVATE_PATH_NOT_EXPORTED'))
for config in ['Debug','Release']:
    for row in load(W3/('abi-cil/'+config+'-inspection-v1.json'))['tools']:
        assert pin(row['path'])==row
        if row not in tools:tools.append(dict(row,exact_W3=True))
save('entry/tool-byte-readback-v1.json',dict(tools=tools,baseline=pin(W3/'matrix/tool-fingerprints-v1.json'),
    no_install_or_download=True,scope='Exact tools used by authenticated runner/metadata/fixture commands; postcampaign unchanged-byte proof, not a new execution.'))
rows=[]
for name in ['strict','profile','docs']:
    current=PRIVATE/'commands'/('a39serial-'+name)
    previous=ROOT/'.test-rationalization-04A38.tmp/commands'/('w3v1-serial-'+name)
    row=dict(category=name,current=[pin(current/x) for x in ['stdout.log','stderr.log']],previous=[pin(previous/x) for x in ['stdout.log','stderr.log']],
        stdout_exact=(current/'stdout.log').read_bytes()==(previous/'stdout.log').read_bytes(),
        stderr_exact=(current/'stderr.log').read_bytes()==(previous/'stderr.log').read_bytes())
    rows.append(row)
save('python-node-scanners/serial-delta-v1.json',dict(entries=rows,exits=[1,1,0],
    strict='Exact same NormalPlot ambiguity identity and trace; actual nonpass.',
    profile='Exact same opaque INTEGRITY_FAILURE stdout. W3 CRLF warnings removed from stderr after authorized merge/EOL transition. Exact originating assertion not exposed by CLI; do not invent a cause.',
    dispositions=['INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED','INHERITED_RESTRICTED;EXPLICIT_OWNER_DECISION_REQUIRED','VERIFIED_PASS'],
    no_reexecution=True))
source_lock();print('Tool bytes and serial nonpass delta authenticated',flush=True)
