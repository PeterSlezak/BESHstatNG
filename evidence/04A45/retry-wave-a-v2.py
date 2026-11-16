from common45 import *
import stat
source=(HERE/'wave-v1.py').read_text()
source=source.replace("label='a45-wave-'+wave+'-v1'", "label='a45-wave-'+wave+'-v2'")
source=source.replace("shutil.copytree(old,target)","shutil.copytree(old,target)\n for p in target.rglob('*'):\n  if p.is_file():p.chmod(stat.S_IREAD | stat.S_IWRITE)")
source=source.replace("'wave-a-obj'","'wave-a-obj-v2'").replace("'wave-a-'","'wave-a-v2-'").replace("wave-a-build-v1.json","wave-a-build-v2.json").replace("wave+'-v1.json'","wave+'-v2.json'")
exec(compile(source,str(HERE/'wave-v1.py'),'exec'))
