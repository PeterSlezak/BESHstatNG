"""Unexecuted input specification only; no original/current expected outputs."""
import itertools
from check_checkpoint import save

def cases():
 records=[]
 for culture,method,bins,weights in itertools.product(['Invariant','sk-SK'],['quantile','equalwidth','equal-width','equal_width','equal width','invalid-04A38'],[1,2,3,10],['missing','uniform','nonuniform','zero-first-bin','all-zero','negative']):
  phase='BinaryInputWeightValidation' if weights=='negative' else 'CoreMinimumBinGuard' if bins==1 else 'CalibrationBinAndReportPreparation'
  records.append(dict(case_id=f'PROPOSED/calibration/{culture}/method={method}/bins={bins}/weights={weights}',family='calibration',culture=culture,
   parameters=dict(method=method,bins=bins,weights=weights),y=[0,1,0,1],probabilities=[0.05,0.1,0.9,0.95],
   intended_source_phase=phase,shipping_path=True,expected_result='UNKNOWN_NOT_CAPTURED'))
 for culture,header,group_header,row_state in itertools.product(['Invariant','sk-SK'],['none','text','numeric-text','blank'],['none','present','misaligned'],['finite','nonfinite','blank-label','mixed-case-labels']):
  records.append(dict(case_id=f'PROPOSED/grouped/{culture}/header={header}/group-header={group_header}/rows={row_state}',family='grouped',culture=culture,
   parameters=dict(header=header,group_header=group_header,row_state=row_state),rows=4,columns=2,intended_source_phase='GroupHeaderAlignmentGuard' if group_header=='misaligned' else 'HeaderAlignmentAndFirstSeenCompleteCaseBuckets',
   shipping_path=True,expected_result='UNKNOWN_NOT_CAPTURED'))
 for culture,header,row_state in itertools.product(['Invariant','sk-SK'],['none','text','numeric-text','blank'],['finite','blank-group','blank-subgroup','NaN-response','Inf-response','mixed-case-labels']):
  records.append(dict(case_id=f'PROPOSED/nested/{culture}/header={header}/rows={row_state}',family='nested',culture=culture,
   parameters=dict(header=header,row_state=row_state),rows=4,columns=3,intended_source_phase='ResponseHeaderCaptionsAndRowFilter',shipping_path=True,expected_result='UNKNOWN_NOT_CAPTURED'))
 assert len(records)==432 and len({r['case_id'] for r in records})==432
 return records

if __name__=='__main__':
 save('proposed-missing-case-matrix.json',dict(status='PROPOSED_NOT_AUTHORIZED_NOT_EXECUTED',chronology='INPUT_SPECIFICATION_CREATED_AFTER_INITIAL_04A38_IMPLEMENTATION; NEVER_PRE_EDIT_FROZEN',
  cases=cases(),rows_per_configuration_if_separately_authorized=432,by_family=dict(calibration=288,grouped=96,nested=48),
  weights=dict(missing=None,uniform=[1,1,1,1],nonuniform=[1,2,1,2],zero_first_bin=[0,0,1,1],all_zero=[0,0,0,0],negative=[-1,1,1,1]),
  grouping_labels=[' a ','A','b',''],nested_labels='Independent group/subgroup labels, trimmed using the original current-culture conversion; known input ordinals only; worksheet rows UNKNOWN',
  source_phase_qualification='Intended phases inferred from source guards, not executed outcomes. Rejected cells never count as reached calculation/report cells.',
  requested_owner_decision='Remain blocked, or separately authorize only a clearly labeled late archived-entry supplemental oracle from the pinned original source. No exception is inferred; no expectation/capture generated.',
  existing_corpus='All original28966 cases/configuration remain immutable and authoritative; this specification neither replaces nor retroactively enlarges that corpus',
  bounds='No statistical correction, native Excel/R, gate release, test weakening, commit/stage/push/merge, publication or04A39 work'))
 print('Proposed432 unexecuted input cases:288 calibration,96 grouped,48 nested; no expectations')
