"""Append audit receipts and versioned closeout; never reseal v1."""
from capture_current import *
from baseline_report import python_receipt
from verify_publication import inspect
from verify_closeout import inspect as closeout
def main():
 inspect()
 tests=python_receipt('w2-publication-adverse');assert tests['total']==16 and tests['exit_code']==0
 source=python_receipt('w2-independent-slice-adverse-v3');assert source['total']==17 and source['exit_code']==0
 public=json.loads((RUNROOT/'commands/w2-public-copy-reader/stdout.log').read_bytes());assert public['status']=='W2_PUBLIC_COPY_PASS_NOT_READY'
 with (HERE/'independent-publication-audits.json').open('x',encoding='utf-8') as f:json.dump(dict(source_and_IL_controls=source,manifest_controls=tests,public_copy=public,batch_ready=False),f,indent=2)
 records=[]
 for folder in sorted((RUNROOT/'commands').glob('w2-*')):
  if (folder/'command.json').exists():records.append(dict(label=folder.name,receipt=clean(json.loads((folder/'command.json').read_bytes()))))
 with (HERE/'command-receipts-closeout-v2.json').open('x',encoding='utf-8') as f:json.dump(dict(commands=records,private_raw_logs='Preserved locally; public source/input/output/branch bytes unmodified',late_original_queries_after_G=0),f,indent=2)
 paths={r['path'] for r in json.loads((HERE/'publication-manifest-v1.json').read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and p.name not in {'source-before-inventory.json','preimage-location-transition.json'})
 manifest=(HERE/'publication-manifest-closeout-v2.json').relative_to(ROOT).as_posix();inventory=(HERE/'file-inventory-closeout-v2.json').relative_to(ROOT).as_posix();paths.update([manifest,inventory])
 with (ROOT/inventory).open('x',encoding='utf-8') as f:json.dump(dict(paths=sorted(paths),private_exclusions_unchanged=json.loads((HERE/'file-inventory.json').read_bytes())['private_exclusions'],boundary='Complete current intended W2 publication; compact derivative/live Git/private binaries distinction remains'),f,indent=2)
 with (ROOT/manifest).open('x',encoding='utf-8') as f:json.dump(dict(status='NOT_READY_04A38',predecessor=pin(HERE/'publication-manifest-v1.json'),entries=[dict(path=p,bytes=None,sha256=None) if p==manifest else pin(ROOT/p) for p in sorted(paths)]),f,indent=2)
 print(json.dumps(closeout()))
if __name__=='__main__':main()
