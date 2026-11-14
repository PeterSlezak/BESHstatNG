from common import *
x=json.loads((HERE/'remaining-work-queue.json').read_bytes());x['supersedes_current_reading']='remaining-work-queue.json: tentative FormatDescriptiveTable label is not an actual member and is withdrawn; original queue/attempt retained'
names=['TryInvertMatrix','ParseEqualVarianceMode','ANOVA1','ANOVA1_WELCH']
x['first_members']=[r for name in names for r in x['members'] if r['name']==name]
assert len(x['first_members'])==4
x['first_action']='TryInvertMatrix exception/Err-state context proof remains OPEN. Next cohesive family: ParametricUDFs.ParseEqualVarianceMode then ANOVA1/ANOVA1_WELCH report/import composition; use exact member IDs below. No new archived reference execution.'
save('remaining-work-queue-v2.json',x)
