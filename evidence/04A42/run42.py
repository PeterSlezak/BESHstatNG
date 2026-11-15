"""Run an evidence recipe with local imports and protected failure diagnostics."""
from pathlib import Path
import datetime,json,runpy,sys,traceback,uuid
here=Path(__file__).resolve().parent
sys.path.insert(0,str(here))
target=here/sys.argv[1]
sys.argv=[str(target)]+sys.argv[2:]
try:runpy.run_path(str(target),run_name='__main__')
except Exception as exc:
    private=here.parents[1]/'.test-rationalization-04A42.tmp/failed-attempts'
    private.mkdir(parents=True,exist_ok=True)
    with (private/(uuid.uuid4().hex+'.json')).open('x',encoding='utf-8') as f:
        json.dump(dict(utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),recipe=target.name,type=type(exc).__name__,traceback=traceback.format_exc()),f,indent=2)
    print('Recipe failed:',target.name,type(exc).__name__,'; exact diagnostics retained privately',flush=True)
    sys.exit(1)
