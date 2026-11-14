from common import *
result=command('public-copy-reader',[sys.executable,'-B',str(HERE/'verify_public_copy.py')]);assert result['exit_code']==0
save('public-copy-reader.json',dict(command=clean(result),proof=json.loads((RUNROOT/'commands/consolidated38-v7-public-copy-reader/stdout.log').read_bytes())))
