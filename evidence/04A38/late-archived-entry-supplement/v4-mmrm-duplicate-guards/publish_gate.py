"""Append-only v4 gate inventory; no external or Git publication."""
from common import *
from verify_supplement import inspect
from verify_packet import inspect as packet
def main():
 proof=inspect();save('compatibility-comparison.json',proof)
 result=command('preservation-gate',[sys.executable,'-B',str(V2/'verify_preservation.py')]);assert result['exit_code']==0
 save('preservation.json',json.loads((RUNROOT/'commands/mmrm38-v4-preservation-gate/stdout.log').read_bytes()))
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0 and not git('diff','--cached','--name-only')
 save('git-gate.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
      merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status=git('status','--short').decode(),git_writes=False))
 save('readiness.json',dict(status='NOT_READY_04A38',duplicate_guard_evidence='4/4 exact each configuration; publication adverse gate pending',new_production_changes=0,
      remaining='Whole-body historical/current/support/inline ownership and remaining import/profile/report extraction; full READY verifier unfinished',
      gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),native='NOT_RUN',scientific='NOT_GOLDEN'))
 paths={r['path'] for r in json.loads((ROOT/PREVIOUS).read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 manifest=(HERE/'publication-manifest.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 save('file-inventory.json',dict(paths=sorted(paths),excluded_private='Earlier raw path-bearing diagnostic and private binaries/raw logs; prior intended payloads retained',upload='NOT_CREATED'))
 save('publication-manifest.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='BOUNDED_V4_DUPLICATE_GUARD_GATE_ONLY',predecessor=pin(ROOT/PREVIOUS),
      entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(packet()))
if __name__=='__main__':main()
