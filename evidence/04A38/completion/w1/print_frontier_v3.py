import json
from pathlib import Path
HERE=Path(__file__).resolve().parent
data=json.loads((HERE/'unresolved-branch-review-frontier-v3.json').read_bytes())
for i,r in enumerate(data['candidates']):
 print(json.dumps(dict(index=i,path=r['path'].split('/ExcelUDFs/')[-1],member=r['member'],line=r['line'],condition=r['condition'][:220],literals=r['arm_literals'],entries=r['entered_counts']),ensure_ascii=True))
