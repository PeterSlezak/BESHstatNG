"""New test import correction; prior compile failure retained, no test identity had started."""
from pathlib import Path
path=Path(__file__).with_name('wave1-v1.py');source=path.read_text().replace('wave1-v1','wave1-v2').replace('wave1-Debug-build-v1','wave1-Debug-build-v2').replace('wave1-Common-tests-v1','wave1-Common-tests-v2').replace("'a40-wave1-Common'","'a40-wave1-Common-v2'")
exec(compile(source,str(path),'exec'))
