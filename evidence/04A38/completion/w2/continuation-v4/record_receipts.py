from common import *
version=sys.argv[1];rows=[]
for p in sorted((RUNROOT/'commands').glob('w2c4-*/command.json')):
 r=json.loads(p.read_bytes());rows.append(dict(label=p.parent.name,command=clean(r),stdout_bytes=(p.parent/'stdout.log').stat().st_size,stderr_bytes=(p.parent/'stderr.log').stat().st_size))
save('command-receipts-'+version+'.json',dict(commands=rows,count=len(rows),scope='All completed C4 runner commands at checkpoint; actual failures/exit codes preserved. Raw logs local, public structured receipts sanitized. Later export/live commands have external receipt.',original_queries=0))
print('Completed C4 commands',len(rows))
