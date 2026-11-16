from common45 import *
# Preserve the generated recipe and its declared ceiling. Actual unchanged worker
# implementation runs at peak three, so report that measured schedule,not six.
source=(HERE/'reconcile-python-v1.py').read_text().replace('workers_at_peak=6','workers_at_peak=3')
exec(compile(source,str(HERE/'reconcile-python-v1.py'),'exec'))
