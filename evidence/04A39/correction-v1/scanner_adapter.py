"""Unchanged scanner with the authenticated necessary-condition email guard."""
from common import *
import ast, importlib.util, re

GUARD=ROOT/'evidence/04A38/completion/w2/continuation-v6/privacy_regex_guard.py'
spec=importlib.util.spec_from_file_location('historical_email_guard',GUARD)
guard=importlib.util.module_from_spec(spec);spec.loader.exec_module(guard)

def module(phase):
    path=ROOT/f'scripts/verify-04a{phase}-publication.py'
    spec=importlib.util.spec_from_file_location('actual_privacy_'+phase,path)
    m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
    m.re=guard.RegexProxy()
    return m

def function_ast(m):
    tree=ast.parse(Path(m.__file__).read_text(encoding='utf-8-sig'))
    return ast.dump(next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='scan'),include_attributes=False)

def detailed(m, paths):
    """Observe append locals without altering original findings or assertions."""
    detail=[]
    class Findings(list):
        def append(self,row):
            local=sys._getframe(1).f_locals
            if row['category']=='known_private_identifier':
                match=re.search(local['pattern'],local['value'])
            else:match=local['match']
            value=match.group()
            detail.append(dict(**row,offset=match.start(),match_sha256=sha(value.replace('\\\\','\\').casefold().encode()),raw_match=value))
            return super().append(row)
    source=Path(m.__file__).read_text(encoding='utf-8-sig')
    tree=ast.parse(source)
    func=next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='scan')
    # Replace only allocation of the findings list; every scanner branch stays intact.
    assign=next(n for n in ast.walk(func) if isinstance(n,ast.Assign) and isinstance(n.value,ast.Tuple) and ast.unparse(n.targets[0])=='(findings, reviewed)')
    assign.value.elts[0]=ast.Call(func=ast.Name(id='Findings',ctx=ast.Load()),args=[],keywords=[])
    ns={**vars(m),'Findings':Findings}
    exec(compile(ast.fix_missing_locations(ast.Module(body=[func],type_ignores=[])),m.__file__,'exec'),ns)
    actual,reviewed=ns['scan'](paths)
    assert len(actual)==len(detail)
    assert [dict((k,r[k]) for k in actual[i]) for i,r in enumerate(detail)]==actual
    return actual,reviewed,detail
