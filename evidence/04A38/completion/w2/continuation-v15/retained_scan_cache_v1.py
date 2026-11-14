"""Reuse individually authenticated successful rows; preserve original exceptions.

No scanner/test assertion changes, no candidate omission and no helper launches.
The failed binary candidate remains uncached and reaches the unchanged C6 scanner.
"""
from common import *
from parallel_scan_v1 import authority as original_authority

def validated_rows(report,plan,known_failure_pin):
    assert report['authority']==plan['authority']==original_authority()
    rows=report['rows'];expected=plan['rows']
    assert [{k:r[k] for k in ['index','path','bytes','sha256']} for r in rows]==expected[:len(rows)],'Changed/dropped/duplicate prefix occurrence'
    for row in rows:assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
    if report['complete']:
        assert report['exit_code']==0 and len(rows)==len(expected) and report['error'] is None
    else:
        assert report['exit_code']==1 and len(rows)<len(expected) and 'UnicodeDecodeError:' in report['error']
        failed=expected[len(rows)];assert {k:failed[k] for k in ['path','bytes','sha256']}==known_failure_pin
        assert pin(ROOT/failed['path'])==known_failure_pin
        try:(ROOT/failed['path']).read_bytes().decode('utf-8-sig')
        except UnicodeDecodeError:pass
        else:raise AssertionError('Recorded binary exception no longer authentic')
    return rows

class Accelerator:
    def __init__(self,hook,work):
        self.hook=hook;self.work=work;self.fresh=0;self.attempts=[];self.deferred=[]
    def install(self):
        binding=json.loads((PREVIOUS/'publication-git-index-binding-v1.json').read_bytes());failed=binding['snapshot'];assert pin(ROOT/failed['path'])==failed
        base=PRIVATE/'python-v1/1-owner-replacement-v1/parallel-scan-1';rows=[];receipts=[]
        for number in range(1,6):
            folder=base/str(number);plan=json.loads((folder/'input.json').read_bytes());report=json.loads((folder/'raw-result.json').read_bytes())
            rows.extend(validated_rows(report,plan,failed));receipts.append(dict(plan=pin(folder/'input.json'),result=pin(folder/'raw-result.json'),complete=report['complete']))
        seen=set()
        for row in rows:
            key=(self.hook['authority'],str(ROOT),os.environ.get('COMPUTERNAME'),row['path'],row['sha256'])
            assert key not in seen;seen.add(key)
            self.hook['cache'][key]=(row['findings'],[tuple(v) for v in row['reviewed']])
        assert failed['path'] not in {r['path'] for r in rows},'Failed candidate cannot be cached as a result'
        self.attempts=[dict(kind='AUTHENTICATED_PREVIOUS_SUCCESSFUL_PER_FILE_ROWS_ONLY',files=len(rows),receipts=receipts)]
        self.deferred=[dict(candidate=failed,action='UNCHANGED_ORIGINAL_SEQUENTIAL_SCAN_RETAINS_UNICODE_EXCEPTION;NOT_OMITTED',historical_giant='Still retained, not reached if original earlier exception stops this candidate sequence.')]
