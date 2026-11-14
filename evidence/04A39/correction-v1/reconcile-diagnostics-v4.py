"""Reader-only correction: Counter receives identity keys, never mapping values."""
from pathlib import Path
path=Path(__file__).with_name('reconcile-diagnostics-v3.py')
source=path.read_text(encoding='utf-8')
needle="exec(compile(source.replace(old,new),str(prior),'exec'))"
replacement="""source=source.replace(old,new)
assert source.count('collections.Counter(byid)')==1
source=source.replace('collections.Counter(byid)','collections.Counter(byid.keys())')
source=source.replace("PRIVATE/'raw-python-authorities'", "PRIVATE/'raw-python-authorities-v4'")
exec(compile(source,str(prior),'exec'))"""
assert source.count(needle)==1
exec(compile(source.replace(needle,replacement),str(path),'exec'))
