from publication_helpers import *
import collections
path=PRIVATE/'neutral-replay-raw-v1.json';reports=[]
for phase in ['13','14']:
 m=physical.scanner(phase);found,reviewed=m.scan([path.relative_to(ROOT).as_posix()]);assert found
 raw=PRIVATE/('neutral-raw-failed-scan-'+phase+'-v1.json');save(raw,dict(findings=found,reviewed=reviewed,original_raw=pin(path),source=pin(m.__file__)))
 reports.append(dict(scanner=phase,status='FAIL_RAW_RECEIPT;UNWAIVED',original_source=pin(m.__file__),reported_view_detections=len(found),categories=dict(collections.Counter(r['category'] for r in found)),raw_scan=pin(raw)))
save('privacy/new-report-raw-failure-v1.json',dict(kind='RAW_FAILURE_PRESERVED;SANITIZED_SUCCESSOR_IS_SEPARATE_REPRESENTATION',reports=reports,raw_original=pin(path),presentation_crosswalk=pin(HERE/'privacy/new-report-presentation-crosswalk-v1.json'),original_rule_sources_unchanged=True,no_unique_count_inferred_from_repeated_views=True,gates=GATES))
print('Both original scanners confirm new raw receipt FAIL;failed snapshots retained,not recolored by sanitized successor',flush=True)
