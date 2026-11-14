from common import *
assert json.loads((HERE/'entry-executable-authentication-v1.json').read_bytes())['status']=='AUTHENTICATED'
q=json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes());assert len(q['rows'])==24
paths=['BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPsm.vb','BESHStat.Excel.Common/src/Analysis/UdfClassLabelPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfPsmDataPreparation38.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb','BESHStat.Excel.Common.Tests/src/UdfContinuation14Tests.vb'];before=[];new=[]
for name in paths:
 p=ROOT/name;target=HERE/'source-before'/(name+'.txt')
 if p.exists():
  if target.exists():assert target.read_bytes()==p.read_bytes()
  else:
   target.parent.mkdir(parents=True,exist_ok=True)
   with target.open('xb') as f:f.write(p.read_bytes())
  before.append(dict(live=pin(p),preimage=pin(target)))
 else:new.append(dict(path=name,entry_state='ABSENT'))
save('implementation-plan-v1.json',dict(source_paths=paths,source_before=before,new_sources=new,queue=pin(HERE/'entry-queue-receipt-v1.json'),entry=pin(HERE/'entry-executable-authentication-v1.json'),
 changes=['Retain old Common GuessPositiveClass body and reset contract byte-for-byte; add post-conversion entry that uses local result and writes the output alias only on True. Native reset and conversion remain in the same order.',
 'Extract typed PSM option construction/final normalization-validation, named-table routing, raw summary layout, model variable names and model matrix preparation. Native conversions/evaluation order, actual engine/materialization/cache/Guid/time/wizard/exception context stay host-owned.',
 'No new Windows callable or callback declaration: preserve exact726 native identities. New methods/enumeration are portable only. Add Common tests and append adapter tests without changing existing assertions.'],original_queries=0))
meta=json.loads((PREVIOUS/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());target=next(m for m in meta['methods'] if m['declaring_type']=='BESHStatNG.ExcelCommon.UdfClassLabelPreparation38' and m['member_name']=='GuessPositiveClass')
callers=[m for m in meta['methods'] if any(i.get('operand')=='METHOD:'+target['method'] for i in m['instructions'])]
assert len(callers)==1 and callers[0]['declaring_type']=='BESHStatNG.UdfDataImport' and callers[0]['member_name']=='GuessPositiveClass'
save('roc-compatibility-decision-v1.json',dict(status='NARROW_SOURCE_BACKED_REPAIR_PERMITTED_CLOSURE_PENDING_COMPLETE_PROOF',prior_defect=pin(PREVIOUS/'open-byref-alias-defect-v1.json'),existing_Common_method=target,actual_shipping_callers=callers,
 original_prediction='Original resets before explicit conversion and assigns output only immediately before each True return afterward. False and exception preserve any alias mutation during conversion/status callbacks.',
 repair='New API calls the unchanged existing Common API with a private local output; copies to caller output only when result True. False/throw do not overwrite native output alias. Every existing direct caller keeps the old public API and reset contract.',
 required_proof='Exact archived original body and actual native/Common callbacks, assignment/return adjacency and absence of intervening external effects, current focused diagnostics and actual Debug/Release CIL. Do not count as original oracle or supplement.',
 original_execution=0,new_original_reference_inputs=0,new_reference_captures=0,closed_before_validation=0))
print('Seven source/test paths and narrow ROC decision published before edits')
