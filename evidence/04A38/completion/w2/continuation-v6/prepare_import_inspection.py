from common import *
source=(PREVIOUS/'inspect_stage.py').read_text(encoding='utf-8')
source=source.replace("HERE/'entry.json'","HERE/'implementation-plan.json'")
# Keep exact preimage/successor transitions, including unchanged planned test files.
source=source.replace("dict(before=row['live'],after=pin(src),snapshot=pin(dst))","dict(path=row['live']['path'],before=row['live'],preimage=row['preimage'],after=pin(src),snapshot=pin(dst))")
with (HERE/'inspect_stage.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
