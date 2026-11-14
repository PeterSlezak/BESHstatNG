"""Append-only bounded MMRM gate packet; no external/Git publication."""
from common import *
from verify_supplement import inspect
from verify_packet import inspect as packet
def main():
 proof=inspect();save('compatibility-comparison.json',proof)
 result=command('preservation-gate',[sys.executable,'-B',str(V2/'verify_preservation.py')]);assert result['exit_code']==0
 preservation=json.loads((RUNROOT/'commands/mmrm38-v3-preservation-gate/stdout.log').read_bytes())
 save('preservation.json',preservation)
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert check.returncode==0 and not git('diff','--cached','--name-only')
 save('git-gate.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),
      merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status=git('status','--short').decode(),git_writes=False))
 save('readiness.json',dict(status='NOT_READY_04A38',mmrm_evidence='Exact2/2 each configuration; publication adverse gate pending',new_production_changes=0,
      remaining='Existing704 historical/719 current/317 support/inline reconciliation and import/profile/report migration;full READY verifier not completed',
      gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),native='NOT_RUN',scientific='NOT_GOLDEN'))
 previous=json.loads((ROOT/PREVIOUS).read_bytes());paths={r['path'] for r in previous['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file())
 self_path=(HERE/'publication-manifest.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory.json').relative_to(ROOT).as_posix()
 paths.update([self_path,inventory])
 save('file-inventory.json',dict(paths=sorted(paths),excluded_private='Prior raw environment diagnostic remains excluded; no prior intended payload lost. Isolated binaries/raw logs retained privately.',upload='NOT_CREATED'))
 save('publication-manifest.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='BOUNDED_MMRM_GATE_PACKET_ONLY',predecessor=pin(ROOT/PREVIOUS),
      entries=[dict(path=p,bytes=None,sha256=None) if p==self_path else pin(ROOT/p) for p in sorted(paths)]))
 print(json.dumps(packet()))
if __name__=='__main__':main()
