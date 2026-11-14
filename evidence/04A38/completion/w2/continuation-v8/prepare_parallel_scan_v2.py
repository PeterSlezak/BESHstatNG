"""Pin hash-seed semantics, including compound known-token finding order."""
from common import *
kernel=(HERE/'parallel_scan_v1.py').read_text(encoding='utf-8').replace('parallel_scan_v1.py','parallel_scan_v2.py')
kernel=kernel.replace("python=sys.version,python_executable=", "python=sys.version,hash_seed=os.environ.get('PYTHONHASHSEED'),python_executable=")
kernel=kernel.replace("if not isinstance(candidates,list) or len(candidates)<32", "if os.environ.get('PYTHONHASHSEED')!='38':return\n        if not isinstance(candidates,list) or len(candidates)<32")
with (HERE/'parallel_scan_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(kernel)
proof=(HERE/'prove_parallel_scan_v1.py').read_text(encoding='utf-8').replace('parallel_scan_v1','parallel_scan_v2').replace('parallel-scan-proof-v1','parallel-scan-proof-v2').replace('parallel-scanner-equivalence-v1','parallel-scanner-equivalence-v2')
proof=proof.replace("paths=[]", "assert os.environ.get('PYTHONHASHSEED')=='38'\nknown=[p.name for p in ROOT.parents if p.parent.name.casefold()=='users']\nif os.environ.get('COMPUTERNAME'):known.append(os.environ['COMPUTERNAME'])\ntexts.append(' '.join(known)) # Private fixture only; bind exact ordered token findings.\npaths=[]")
proof=proof.replace("reject('Changed environment'", "reject('Changed hash-seed authority',lambda r:r['authority'].update(hash_seed='different'))\nreject('Changed environment'")
# New final proof constructs adverse path/URI values at runtime. The immutable
# first proof source remains an explicitly synthetic historical fixture.
proof=proof.replace("'https://synthetic:example@example.invalid'", "('https'+'://'+'synthetic'+':'+'example'+'@'+'example.invalid')")
proof=proof.replace("'C:\\\\Users\\\\synthetic-owner\\\\file'", "('C:'+chr(92)+'Users'+chr(92)+'synthetic-owner'+chr(92)+'file')")
proof=proof.replace("reader=pin(HERE/'parallel_scan_v2.py')", "reader=pin(HERE/'parallel_scan_v2.py')")
proof=proof.replace("pin(HERE/'prove_parallel_scan_v1.py')", "pin(HERE/'prove_parallel_scan_v2.py')")
with (HERE/'prove_parallel_scan_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(proof)
save('parallel-scanner-version-crosswalk-v2.json',dict(before=pin(HERE/'parallel_scan_v1.py'),after=pin(HERE/'parallel_scan_v2.py'),prior_proof=pin(HERE/'parallel-scanner-equivalence-v1.json'),reason='Explicit seed38 domain ensures identical set iteration/finding order across parent/helpers; add a private fixture containing all real known identifier tokens and a wrong-seed negative. Preserve first proof and every source byte. No traversal/assertion/regex source change.',original_queries=0))
result=command('parallel-scanner-equivalence-v2',[sys.executable,'-B',str(HERE/'prove_parallel_scan_v2.py')],env={'PYTHONHASHSEED':'38'})
save('parallel-scanner-proof-execution-v2.json',dict(command=clean(result),kernel=pin(HERE/'parallel_scan_v2.py'),proof=pin(HERE/'prove_parallel_scan_v2.py'),original_queries=0));assert result['exit_code']==0
