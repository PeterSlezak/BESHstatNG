from common import *
version=sys.argv[1];rows=[]
for p in sorted((RUNROOT/'commands').glob('w2c3-*')):
 if not (p/'command.json').exists():continue
 r=json.loads((p/'command.json').read_bytes());rows.append(dict(label=p.name,command=clean(r),stdout_bytes=(p/'stdout.log').stat().st_size,stderr_bytes=(p/'stderr.log').stat().st_size))
save('command-receipts-'+version+'.json',dict(commands=rows,count=len(rows),scope='All completed C3 runner commands at this append-only checkpoint. Raw outputs retained locally; public structured receipts use established path sanitization and actual stdout/stderr hashes. Publication/export commands after this seal have separately versioned receipts.',new_original_queries=0))
if version=='v1':
 save('authority-ledger.json',dict(batch='04A.38',date='2026-11-10',current_authority='Existing W2 extraction/current-only validation and conditional W3; user continuation-v3 specifically authorizes duplicate helper removal and naming-policy extraction',new_original_cases_authorized=0,new_original_cases_executed=0,corpora=[dict(authority='GENUINE_PRE_EDIT',cases_per_configuration=28966,capture='PRE_EDIT_FROZEN_UNCHANGED'),*[dict(authority=a,cases_per_configuration=n,capture='LATE_ARCHIVED_ENTRY_SUPPLEMENT_UNCHANGED') for a,n in [('A',432),('B',12),('C',2),('D',4),('E',2),('F',2),('G',4)]],late_total_per_configuration=458,phase37='Separate immutable6912pre-edit+22050late/configuration',original_DLLs='PASSIVE_METADATA_INSPECTION_ONLY;NO_FIT_QUERY_OR_CAPTURE',native_scientific='NOT_RUN;NOT_GOLDEN',W3='NOT_STARTED;W2_NOT_COMPLETE'))
 print('completed commands',len(rows))
