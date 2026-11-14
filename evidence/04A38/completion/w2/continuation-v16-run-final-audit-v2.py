import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent/'continuation-v16'))
"""Single final real-byte private audit and complete live source/Git preservation."""
from common import *
import copy,importlib.util
manifest=W2/'continuation-v16-retained-private-preservation-v2.json'
private=command('postseal-private-reader-v2',[sys.executable,'-I','-B',str(HERE/'verify_c16_private_v1.py'),'--verify',str(manifest)])
assert private['exit_code']==0
folder=RUNROOT/'commands/w2c16-postseal-private-reader-v2'
observed=json.loads((folder/'stdout.log').read_bytes())
assert observed['status']=='ACTUAL_PRIVATE_FILE_IDENTITIES_BYTES_AND_PHYSICAL_ARCHIVE_HASHES_PASS'
doc=json.loads(manifest.read_bytes())
spec=importlib.util.spec_from_file_location('c16_final_private_controls',HERE/'verify_c16_private_v1.py');module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
first=doc['private_pins'][0];controls=[]
for label,value in [('modified private bytes',b'changed'),('missing private bytes',None)]:
 try:module.inspect(manifest,{first['path']:value})
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:controls.append(dict(name=label,status='REJECTED',reason=str(e)))
 else:raise AssertionError('Private tamper accepted')
receipt=dict(status=observed['status'],manifest=pin(manifest),reader=pin(HERE/'verify_c16_private_v1.py'),private_pin_count=observed['private_pin_count'],inherited_private_count=21208,inherited_postseal_count=31,physical_archives=observed['physical_archives'],inherited_physical_archives=101,unique_real_files_hashed=observed['unique_files_hashed'],real_filesystem_identity_count=len(observed['real_file_identities']),raw_command=pin(folder/'command.json'),raw_stdout=pin(folder/'stdout.log'),raw_stderr=pin(folder/'stderr.log'),controls=controls,ignored_TestResults='NOT_ENUMERATED',original_queries=0)
path=W2/'continuation-v16-private-reader-verification-v1.json'
with path.open('x',encoding='utf-8') as f:json.dump(receipt,f,indent=2)
# The versioned preservation reader retains every inherited assertion and adds
# v16 successors. This is a live full-source/Git check, not a historical scanner.
r=command('postseal-live-preservation-v1',[sys.executable,'-I','-B',str(HERE/'verify_c16_preservation_v1.py')])
assert r['exit_code']==0
folder=RUNROOT/'commands/w2c16-postseal-live-preservation-v1';result=json.loads((folder/'stdout.log').read_bytes())
assert result['Git']=='LIVE_PASS' and result['owner_documents_checked']==16
path=W2/'continuation-v16-live-preservation-v1.json'
with path.open('x',encoding='utf-8') as f:json.dump(dict(status=result['status'],result=clean(result),command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),reader=pin(HERE/'verify_c16_preservation_v1.py'),original_queries=0),f,indent=2)
print('Final real private and live preservation audits passed',receipt['private_pin_count'],receipt['physical_archives'],flush=True)
