"""Proposal only: serialize a bounded decision; do not invoke any implementation."""
from prepare_original import ROOT,HERE,pin,save

def main():
 variants=[
  ('wide-time', [['time'],[1.0]], 'wide parser -> time setting'),
  ('name-value-time', [['Name','Value'],['time',1.0]], 'name/value parser -> time setting'),
  ('name-value-visit', [['Name','Value'],['visit',1.0]], 'name/value parser -> visit alias'),
  ('duplicate-time-visit', [['Name','Value'],['time',1.0],['visit',2.0]], 'second time/visit setting rejection'),
  ('duplicate-design', [['Name','Value'],['X1',1.0],['x1',2.0]], 'second fitted-column setting rejection'),
  ('wide-multiple-rows', [['time'],[1.0],[2.0]], 'multiple nonblank wide rows rejection')]
 cases=[]
 for culture in ['Invariant','sk-SK']:
  for name,values,branch in variants:
   cases.append(dict(case_id='PROPOSED-GEE-PROFILE/'+culture+'/'+name,culture=culture,configurations=['Debug','Release'],
    public_entry='BESH.REGR.GEE_LSMESTIMATE',at=dict(element_type='System.Object',rank=2,lower_bounds=[0,0],lengths=[len(values),len(values[0])],
     values=values,cell_types=[['System.String' if isinstance(x,str) else 'System.Double' for x in row] for row in values]),
    argument_order=['handle','spec','covarianceType','alpha','at','scale'],
    other_arguments='Reuse original successful real_model_at_profile GEE fit/spec/defaults from pinned Capture38.cs; materialize all fit inputs and record exact IEEE bits before any authorized capture',
    target_branch_hypothesis_only=branch,expected_result='UNKNOWN_NOT_CAPTURED',execution='NOT_AUTHORIZED_NOT_EXECUTED'))
 save('proposed-gee-profile-decision.json',dict(status='PROPOSED_NOT_AUTHORIZED_NOT_EXECUTED',cases_per_configuration=12,cases=cases,
  earliest_missing='PROPOSED-GEE-PROFILE/Invariant/name-value-time',
  required_new_authority='User section4.6: a newly uncovered essential original behavior outside approved432 requires a new owner decision',
  scope='A separate bounded late archived-entry GEE-profile supplement only, using the same authenticated main; source capture sealed before current comparison; no scientific/native changes',
  original_factory=pin(ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs'),
  discovery=pin(HERE/'remaining-profile-coverage.json'),
  qualifications=['No current or original implementation was executed for these proposed cases',
   'The432-case authorization does not authorize these12 cases',
   'A later authorization would not make these genuine pre-edit captures',
   'This is the first newly proven gap, not a certification that no additional ownership/coverage gaps exist']))
 print('Proposed12 GEE profile cases/configuration; no implementation executed')

if __name__=='__main__':main()
