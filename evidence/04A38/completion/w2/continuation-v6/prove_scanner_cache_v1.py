"""Prove unchanged scanner per-file composition including positive findings."""
from common import *
import ast,copy,importlib.util
from privacy_regex_guard import RegexProxy
paths=[ROOT/'scripts'/('verify-04a'+n+'-publication.py') for n in ['13','14']]
def function_ast(path,name):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'))
 return ast.dump(next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name==name),include_attributes=False)
assert function_ast(paths[0],'scan')==function_ast(paths[1],'scan')
assert function_ast(paths[0],'git')==function_ast(paths[1],'git')
spec=importlib.util.spec_from_file_location('proof_scanner',paths[0]);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);module.re=RegexProxy()
folder=PRIVATE/'scanner-composition-fixtures-v1';folder.mkdir()
values=['ordinary content','scanner example '+'one'+'@'+'sample.example','two\n'+'https'+':/'+'/user'+':pass'+'@host.example',json.dumps(dict(s='x'+'@'+'sample.example'))]
candidates=[]
for i,value in enumerate(values):
 p=folder/(str(i)+('.json' if i==3 else '.txt'));p.write_text(value,encoding='utf-8');candidates.append(p.relative_to(ROOT).as_posix())
samples=[candidates,candidates[::-1],candidates+[candidates[1]],[],[candidates[2],candidates[0]]]
positive=0
for candidates in samples:
 expected=module.scan(candidates);findings=[];reviewed=set()
 for candidate in candidates:
  f,r=module.scan([candidate]);findings.extend(copy.deepcopy(f));reviewed.update(r)
 assert expected==(findings,sorted(reviewed));positive+=len(findings)
save('scanner-cache-proof-v1.json',dict(status='EXACT_ORIGINAL_SCAN_COMPOSITION_PASS',samples=len(samples),positive_findings=positive,source_pins=[pin(p) for p in paths],scan_ast_sha256=sha(function_ast(paths[0],'scan').encode()),git_ast_sha256=sha(function_ast(paths[0],'git').encode()),
 law='Original scan initializes deterministic environment/ROOT tokens and patterns, then only appends per-file findings and unions reviewed set. Ordered concatenation preserves findings including duplicates; union/sort preserves reviewed output. Cache uses exact AST/ROOT/COMPUTERNAME/path/bytes and deepcopy; missing files bypass cache. Git HEAD fixed by task and final Git receipts. Fixture duplicates and positive findings preserved.',
 scanner_assertions_changed=0,filtered_files=0,positive_results_waived=0,original_queries=0))
print('Exact scanner composition controls PASS',len(samples),positive)
