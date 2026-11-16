from common45 import *
import re
lock();obligations=load(HERE/'scope/original-obligations-v1.json')['rows'];ids={o['obligation_id'] for o in obligations};register=load(ROOT/'evidence/04A39/register/defects-v7.json')
def strings(x):
 if isinstance(x,str):yield x
 elif isinstance(x,dict):
  for v in x.values():yield from strings(v)
 elif isinstance(x,list):
  for v in x:yield from strings(v)
transport=load(ROOT/'docs/architecture/04A27/transport-restrictions-v1.json');importer=sorted(ids&set(strings(transport)));assert len(importer)==44
authorities=['planning/BESHStatNG_Phase04A_Windows_Portability_Audit_20261018.md','docs/architecture/WindowsMigrationLedger/index.json','docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json','docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json','docs/architecture/batch-04A-02-owner-accepted-2026-11-02.md','docs/architecture/batch-04A-03-owner-review.md','docs/architecture/batch-04A-05-final-owner-review.md','docs/architecture/batch-04A-09-owner-review.md','docs/architecture/batch-04A-11-owner-review.md','docs/architecture/batch-04A-12-owner-review.md','docs/architecture/batch-04A-13-owner-review.md','docs/architecture/batch-04A-27-final-owner-review.md','docs/architecture/batch-04A-28-owner-review.md','docs/architecture/batch-03-01-versioned-application-settings.md','docs/architecture/batch-03-02-diagnostics-and-run-correlation.md','docs/architecture/batch-03-03-execution-coordination.md','docs/architecture/batch-03-04-fault-model.md','docs/architecture/batch-03-05-help-version-feature-metadata.md','docs/architecture/batch-03-06-document-lifecycle.md']
for p in authorities:assert (ROOT/p).is_file()
rows=[]
for path in authorities:
 text=(ROOT/path).read_text(encoding='utf-8-sig')
 found=sorted(set(re.findall(r'\b(?:04A02-[A-Z0-9-]+|C\d{2}-[A-Z0-9-]+)\b',text)))
 rows.append(dict(source=pin(path),original_case_ids=found,disposition='HISTORICAL_AUTHORITY_RETAINED;NO_TRANSFER_OF_CASE_SPECIFIC_EXCEPTION'))
selected=[]
for issue in register['issues']:
 locations=issue.get('source_locations',[]);serialized=json.dumps(locations)
 if any(p in serialized for p in ['Refedit/','WindowsDocumentLifecycle','Ui4Cox','Ui4KMandLogRank','Ui12GlobalSettings','UiUpdateAvailable','UiGLM','Ui13GEE','UibyID','Ui9ANOVA2nested']):
  selected.append(dict(canonical_id=issue['canonical_id'],original_status=issue['status'],aliases=issue['aliases'],record_sha256=sha(json.dumps(issue,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()),classification='WINDOWS_HOST_ONLY_NATIVE_PENDING',native_executed=False,original48_exit_status=None,next_gate='Exact original obligation and owner Windows/COM decision;04A46/47 family-specific coverage remains separate'))
save('scope/carry-over-v1.json',dict(authorities=rows,importer44_original_obligations=importer,importer_authority=pin('docs/architecture/04A27/transport-restrictions-v1.json'),case_specific_exceptions_transferable=False,selected_original_canonical_issues=selected,all_original_obligations='evidence/04A45/scope/original-obligations-v1.json',restriction_graph='evidence/04A45/register/interim-delta-v1.json',UiUpdateAvailable='STATIC_SOURCE_VERIFIED_HOST_ONLY;constructor,buttons,link and updater actions never invoked;network/settings approval absent',C09_C11_C12='Original checkpoint/source cases remain restricted,including grouped/nested/normality,LM default/Data header and GEE classification/QIC/row-axis. Case identities are taken from original authorities,not newly coined.',survival='Cox/log-rank/plot/progress/controller cases remain restricted;fake cancellation cannot prove synchronous fit interruption',gates=GATES))
print('44 importer obligations and',len(selected),'source-linked original canonical issues retained;authority/source cases pinned',flush=True)
