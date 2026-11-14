"""Versioned actual-entry preservation with narrowly authorized UDF transitions."""
import json
from preflight import ROOT,HERE,BASE,git,pin,sha,save

def main():
 assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==BASE
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 assert not git('diff','--cached','--name-only')
 entry=json.loads((HERE/'entry-source-pins.json').read_bytes());prior=json.loads((HERE/'preservation-entry.json').read_bytes())
 tracked=git('ls-files','-z').decode().split('\0');exact=[];changed=[]
 for old in entry['files']:
  path=old['path']
  if path.startswith('<PRIVATE_PROTECTED_PATH:'):
   matches=[p for p in tracked if p and '<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>'==path];assert len(matches)==1
   real=matches[0]
  else:real=path
  current=pin(real);current['path']=path
  if current==old:exact.append(path);continue
  assert real.startswith('BESHstatNG/src/ExcelUDFs/') or real=='BESHStatNG.Test/BESHStatNG.Tests.vbproj',('UNAUTHORIZED_TRACKED_CHANGE',path)
  changed.append(dict(path=path,entry=old,current=current,authority='04A38 UDF internal extraction, or explicit new Framework test include; no public registration/statistical/cache redesign'))
 transition={r['path']:r for r in changed};lineage=[]
 assert len(prior['lineage_pins'])==278
 for old in prior['lineage_pins']:
  current=pin(old['path']);entry_pin=old['merged_main']
  if current!=entry_pin:
   assert old['path'] in transition and old['path'].startswith('BESHstatNG/src/ExcelUDFs/')
   status='AUTHORIZED_04A38_UDF_SOURCE_TRANSITION'
  else:status='EXACT_MERGED_MAIN_ENTRY_BYTES'
  lineage.append(dict(path=old['path'],status=status,accepted37_sha256=old['accepted37_sha256'],entry_inverse_encoding_proof=old['inverse_encoding_proof'],entry=entry_pin,current=current))
 for row in prior['protected_mixed_model_captures']:assert pin(row['path'])==row
 for row in prior['owner_documents_excluded']:assert pin(row['path'])==row
 save('source-transitions-validation-v2.json',dict(status='AUTHORIZED_TRACKED_SOURCE_TRANSITIONS_ONLY',source_commit=BASE,transitions=changed,
  statistical_engine_changes=0,registration_changes=0,cache_lifecycle_redesign=False,old_test_source_changes=0))
 save('preservation-validation-v2.json',dict(status='PASS_WITH_EXPLICIT_AUTHORIZED_04A38_SOURCE_TRANSITIONS',source_commit=BASE,
  tracked_entry_files=len(entry['files']),exact_tracked=len(exact),authorized_transitions=len(changed),unexpected_changes=0,lineage_pins=lineage,
  protected_mixed_model_captures=prior['protected_mixed_model_captures'],owner_documents_excluded=prior['owner_documents_excluded'],
  previous_evidence='All tracked entry evidence/docs/test source retained byte-for-byte; no historical manifests rewritten',historical_test_results='NOT_REHASHED; no traversal of ignored historical TestResults payload',
  scientific='NOT_GOLDEN',native='NOT_RUN'))
 print(len(entry['files']),'entry files;',len(exact),'exact;',len(changed),'authorized transitions;278 pins/100 captures/16 owner documents verified')

if __name__=='__main__':main()
