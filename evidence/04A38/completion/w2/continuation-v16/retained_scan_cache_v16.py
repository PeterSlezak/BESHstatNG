"""Authenticated successful C15 per-file memoization; original exceptions stay uncached.

No helper process or bulk scan is launched. Changed current bytes do not reuse
old findings. The historical policy, regex fallback, assertions and candidate
order are unchanged, including the retained binary snapshot's Unicode error.
"""
from common import *
import ast,collections

def function_ast(path,name):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'))
 return ast.dump(next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name==name),include_attributes=False)

def validated_report(report,plan,failed,historical,expected):
 assert report['authority']==plan['authority']==expected,'Retained scanner authority mismatch'
 success=report['rows'];planned=plan['rows']
 assert [{k:r[k] for k in ['index','path','bytes','sha256']} for r in success]==planned[:len(success)],'Changed/dropped/duplicated original per-file prefix'
 for row in success:assert historical[row['path']]=={k:row[k] for k in ['path','bytes','sha256']},'Retained result/input-freeze authority differs'
 if report['complete']:assert report['exit_code']==0 and len(success)==len(planned) and report['error'] is None
 else:
  assert report['exit_code']==1 and len(success)<len(planned) and 'UnicodeDecodeError:' in report['error']
  assert {k:planned[len(success)][k] for k in ['path','bytes','sha256']}==failed
 return success

def retained_inputs():
 path=PREVIOUS/'python-scan-input-freeze-v2.json'
 historical={r['path']:r for r in json.loads(path.read_bytes())['files'] if r['sha256'] is not None}
 index=json.loads((W2/'continuation-v15-review-parts-index-v2.json').read_bytes())
 excluded=json.loads((PREVIOUS/'review-exclusions-v1.json').read_bytes())
 for r in index['files']+excluded['excluded']:
  row={k:r[k] for k in ['path','bytes','sha256']}
  assert historical.setdefault(row['path'],row)==row,'Conflicting authenticated C15 retained input versions'
 return historical

class Accelerator:
 def __init__(self,hook,work):
  self.hook=hook;self.work=work;self.attempts=[];self.fresh=0;self.deferred=[];self.validated=0;self.reused=0;self.changed=[]
 def install(self):
  oldprivate=RUNROOT/'completion-w2-cont15'
  policy=ROOT/'scripts/verify-04a13-publication.py';guard=W2/'continuation-v6/privacy_regex_guard.py'
  expected=dict(kernel=pin(PREVIOUS/'parallel_scan_v1.py'),policy=pin(policy),scan_ast_sha256=sha(function_ast(policy,'scan').encode()),git_ast_sha256=sha(function_ast(policy,'git').encode()),guard=pin(guard),root=str(ROOT),computername=os.environ.get('COMPUTERNAME'),python=sys.version,hash_seed=os.environ.get('PYTHONHASHSEED'),python_executable=library_pin(Path(sys.executable)),HEAD=git('rev-parse','HEAD').decode().strip())
  assert expected['hash_seed']=='38'
  binding=json.loads((W2/'continuation-v14/publication-git-index-binding-v1.json').read_bytes());failed=binding['snapshot'];assert pin(ROOT/failed['path'])==failed
  base=oldprivate/'python-v1/1-owner-replacement-v1/parallel-scan-1'
  historical=retained_inputs()
  rows=[];receipts=[]
  for number in range(1,6):
   folder=base/str(number);planpath=folder/'input.json';reportpath=folder/'raw-result.json'
   plan=json.loads(planpath.read_bytes());report=json.loads(reportpath.read_bytes())
   assert report['authority']==plan['authority'],'Retained scanner authority mismatch'
   if report['authority']!=expected:
    self.attempts=[dict(status='CACHE_NOT_USED_ENVIRONMENT_DIFFERS_ORIGINAL_SEQUENTIAL_RETAINED',plan=pin(planpath),result=pin(reportpath))]
    return
   success=validated_report(report,plan,failed,historical,expected)
   rows.extend(success);receipts.append(dict(plan=pin(planpath),result=pin(reportpath),complete=report['complete']))
  seen=set()
  for row in rows:
   identity={k:row[k] for k in ['path','bytes','sha256']}
   assert historical[row['path']]==identity,'Retained result/input-freeze authority differs'
   key=(self.hook['authority'],str(ROOT),os.environ.get('COMPUTERNAME'),row['path'],row['sha256'])
   assert key not in seen;seen.add(key);self.validated+=1
   current=pin(ROOT/row['path'])
   if current!=identity:
    self.changed.append(dict(historical=identity,current=current,action='NOT_REUSED;UNCHANGED_ORIGINAL_SCAN_EVALUATES_CURRENT_BYTES'))
    continue
   self.hook['cache'][key]=(row['findings'],[tuple(v) for v in row['reviewed']]);self.reused+=1
  assert self.validated==5829 and failed['path'] not in {r['path'] for r in rows}
  self.attempts=[dict(status='AUTHENTICATED_SUCCESSFUL_PER_FILE_ROWS_ONLY',validated=self.validated,seeded=self.reused,changed_current_rows=self.changed,receipts=receipts)]
  self.deferred=[dict(candidate=failed,action='NOT_CACHED_OR_OMITTED;UNCHANGED_ORIGINAL_SCANNER_RETAINS_EXCEPTION_AND_ORDER')]
