"""Final external additive authorities; never overwrite a sealed payload."""
from common import *
def external(name,doc):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    return pin(path)
def preserve():
    result=command('final-live-preservation-v1',[sys.executable,'-B',str(HERE/'verify_c13_preservation_v1.py')]);assert result['exit_code']==0
    folder=RUNROOT/'commands/w2c13-final-live-preservation-v1';proof=json.loads((folder/'stdout.log').read_bytes())
    assert proof['Git']=='LIVE_PASS' and proof['owner_documents_checked']==16 and proof['lineage_pins']==278 and proof['protected_captures']==100 and proof['original_snapshots']==41
    receipt=external('continuation-v13-live-preservation-v1.json',dict(status='C13_LIVE_ORIGINAL_OWNER_TRACKED_SOURCE_PRESERVATION_PASS_NOT_READY',reader=pin(HERE/'verify_c13_preservation_v1.py'),result=proof,
        execution=clean(result),raw_command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),original_queries=0))
    print(json.dumps(dict(live_preservation=receipt)))
if __name__=='__main__':preserve()
