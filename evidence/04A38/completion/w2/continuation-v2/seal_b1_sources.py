from common import *
import shutil
entry=json.loads((HERE/'entry.json').read_bytes());records=[]
items=[dict(before=r['live'],preimage=r['preimage']) for r in entry['source_before']]
extra=json.loads((HERE/'source-before-b1-addendum.json').read_bytes());items.append(dict(before=extra['source'],preimage=extra['snapshot']))
for r in items:
 p=r['before']['path'];after=pin(ROOT/p);dst=HERE/'source-after/b1-final'/(p+'.txt');dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(ROOT/p,dst)
 records.append(dict(path=p,before=r['before'],preimage=r['preimage'],after=after,snapshot=pin(dst),changed=r['before']!=after))
save('source-transitions-final.json',dict(stage='b1-v1',files=records,source_before_captured_before_edit=True,source_after_compiled_in=['build-b1-v1-Debug.json','build-b1-v1-Release.json','full-managed-Debug-v1.json'],Core_numerical_engine=pin(ROOT/'BESHStat.Core/src/BaseStat/Distributions.vb'),portable_targets=['AssumptionsUDFsOptions38.TryParseLeveneCenterFromReader','UdfNumericPreparation38.PrepareStudentizedRangeCdf','UdfNumericPreparation38.PrepareStudentizedRangeTail','UdfNumericPreparation38.PrepareFDistributionDensity','UdfSpillPreparation38.StackWithBlankRow','UdfSpillPreparation38.NormalizeBlanks'],new_original_queries=0))
print('Final source transitions',len(records),'changed',sum(r['changed'] for r in records))
