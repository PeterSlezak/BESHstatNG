import unittest, importlib.util, ast, copy
from privacy_regex_guard import RegexProxy

def function_ast(path, name):
    tree = ast.parse(path.read_text(encoding='utf-8-sig'))
    return ast.dump(next((n for n in tree.body if isinstance(n, ast.FunctionDef) and n.name == name)), include_attributes=False)
paths = [ROOT / 'scripts' / ('verify-04a' + n + '-publication.py') for n in ['13', '14']]
assert function_ast(paths[0], 'scan') == function_ast(paths[1], 'scan')
assert function_ast(paths[0], 'git') == function_ast(paths[1], 'git')
authority = sha(function_ast(paths[0], 'scan').encode())
original = importlib.util.spec_from_file_location
proxies = []
source_pins = {}
cache = {}
stats = dict(fresh=0, reused=0)

def instrument(name, location, *args, **kwargs):
    spec = original(name, location, *args, **kwargs)
    path = Path(location)
    if path.parent.resolve() != (ROOT / 'scripts').resolve() or not (path.name.startswith('verify-04a') and path.name.endswith('-publication.py')):
        return spec
    loader = spec.loader

    class Loader:

        def create_module(self, s):
            return loader.create_module(s)

        def exec_module(self, module):
            before = pin(path)
            loader.exec_module(module)
            if callable(getattr(module, 'scan', None)) and hasattr(module, 're'):
                proxy = RegexProxy()
                module.re = proxy
                proxies.append((path.name, proxy))
                source_pins[path.relative_to(ROOT).as_posix()] = before
            if path in paths:
                assert sha(function_ast(path, 'scan').encode()) == authority
                original_scan = module.scan

                def scan(candidates):
                    findings = []
                    reviewed = set()
                    for candidate in candidates:
                        target = ROOT / candidate
                        if not target.is_file():
                            f, r = original_scan([candidate])
                        else:
                            key = (authority, str(module.ROOT), os.environ.get('COMPUTERNAME'), candidate, sha(target.read_bytes()))
                            if key not in cache:
                                cache[key] = original_scan([candidate])
                                stats['fresh'] += 1
                                assert sha(target.read_bytes()) == key[-1], 'Scanner input changed during evaluation'
                            else:
                                stats['reused'] += 1
                            f, r = copy.deepcopy(cache[key])
                        findings.extend(f)
                        reviewed.update(r)
                    return (findings, sorted(reviewed))
                module.scan = scan
    spec.loader = Loader()
    return spec
importlib.util.spec_from_file_location = instrument
