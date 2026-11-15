"""Qualify the newly added enum references; prior stopped compile retained, no tests started."""
from pathlib import Path
path=Path(__file__).with_name('wave2-v1.py');source=path.read_text().replace('wave2-v1','wave2-v2').replace('wave2-Debug-build-v1','wave2-Debug-build-v2').replace('wave2-Common-tests-v1','wave2-Common-tests-v2').replace('a40-wave2-Common-new','a40-wave2-Common-new-v2')
exec(compile(source,str(path),'exec'))
