"""Box host enum values separately at the original conditional output sites."""
from common import *
assert (HERE/'error-cell-current-only-counterexample-v1.json').is_file()
path='BESHStat.Excel.Common/src/Analysis/UdfW2CompletionPreparation38.vb';raw=(ROOT/path).read_bytes();snap=HERE/'error-cell-production-before-v1.vb.txt';snap.write_bytes(raw);s=raw.decode('utf-8')
start=s.index('        Public Shared Function ComputeSurvivalMedianCi(');end=s.index('        End Function',start)+len('        End Function');old=s[start:end]
new=old.replace('ComputeSurvivalMedianCi(', 'ComputeSurvivalMedianCi(Of TUnavailable As Structure)(',1).replace('unavailable As Object','unavailable As TUnavailable')
assert new.count(', unavailable, CType(')==3
new=new.replace(', unavailable, CType(',', CType(unavailable, Object), CType(')
assert old!=new;s=s[:start]+new+s[end:];(ROOT/path).write_bytes(s.encode('utf-8'))
save('error-cell-production-correction-v1.json',dict(path=path,before=pin(snap),after=pin(ROOT/path),old_method=old,new_method=new,reason='Native supplies ExcelError as a value-type generic argument; Common boxes it at each original conditional cell assignment. This preserves distinct boxes and avoids early boxing/shared Object aliasing. Common has no Excel-DNA static reference; other consumers may supply their own value type. Core computations and all Windows source bodies are unchanged.',counterexample=pin(HERE/'error-cell-current-only-counterexample-v1.json'),original_queries=0))
freeze=json.loads((HERE/'production-test-freeze-v6.json').read_bytes());freeze['prior_candidate']=pin(HERE/'production-test-freeze-v6.json')
for r in freeze['files']:
 r['after']=pin(ROOT/r['path']);p=HERE/'source-after-v7'/r['path'];p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes((ROOT/r['path']).read_bytes());r['snapshot']=pin(p)
freeze['sources']=[r['after'] for r in freeze['files']];save('production-test-freeze-v7.json',freeze)
for a,b in [('build_final_Debug_v4.py','build_final_Debug_v7.py'),('inspect_final_Debug_v4.py','inspect_final_Debug_v7.py'),('compatibility_Debug_v2.py','compatibility_Debug_v3.py')]:
 s=(HERE/a).read_text(encoding='utf-8').replace('production-test-freeze-v4.json','production-test-freeze-v7.json')
 if b.startswith('inspect'):s=s.replace('final-v4','final-v7').replace('inspection-v4','inspection-v7').replace('-v17-final4','-v17-final7')
 with (HERE/b).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Conditional error-cell boxing restored with a portable value-type parameter; Core and Native source unchanged')
