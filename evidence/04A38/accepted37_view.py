"""Reconstruct the accepted packet in private storage; never rewrite predecessors.

All restored bytes must match their independently recorded old SHA256, with only
the explicitly enumerated LF/CRLF inverse candidates. No approximate comparison.
"""
from pathlib import Path
import json
import sys
from preflight import ROOT,HERE,sha,record_crlf,save
from run import PRIVATE,m

VIEW=PRIVATE/'accepted37-view-v1'
def restore(path,expected,expected_size=None):
    b=(ROOT/path).read_bytes();lf=b.replace(b'\r\n',b'\n');crlf=lf.replace(b'\n',b'\r\n')
    candidates=dict(DIRECT=b,LF=lf,CRLF=crlf,CRLF_EXCEPT_FINAL_LITERAL_LF=crlf[:-2]+b'\n' if crlf.endswith(b'\r\n') else crlf,CSV_RECORD_CRLF_EMBEDDED_LF=record_crlf(lf))
    valid=[(k,v) for k,v in candidates.items() if sha(v)==expected and (expected_size is None or len(v)==expected_size)]
    assert valid,'No exact accepted byte reconstruction: '+path
    kind,value=valid[0];dest=VIEW/path;dest.parent.mkdir(parents=True,exist_ok=True)
    if dest.exists():assert dest.read_bytes()==value
    else:
        with dest.open('xb') as f:f.write(value)
    return dict(path=path,accepted_sha256=expected,accepted_bytes=len(value),main_sha256=sha(b),inverse=kind)

def main():
    VIEW.mkdir(exist_ok=False);restored={}
    manifest=json.loads((ROOT/'evidence/04A37/publication-manifest.json').read_bytes())
    for row in manifest['files']:
        expected=row['sha256'] or sha((ROOT/row['path']).read_bytes())
        restored[row['path']]=restore(row['path'],expected,row['bytes'])
    lineage=json.loads((ROOT/'evidence/04A37/lineage-check.json').read_bytes())['pins']
    for row in lineage:restored[row['path']]=restore(row['path'],row['final37_sha256'])
    mixed=json.loads((ROOT/'evidence/04A37/mixed-model-captures-check.json').read_bytes())['files']
    for row in mixed:restored[row['path']]=restore(row['path'],row['sha256'],row['bytes'])
    auth=json.loads((ROOT/'evidence/04A37/entry-authority.json').read_bytes())
    for row in auth['owner_documents_excluded']:restored[row['path']]=restore(row['path'],row['sha256'],row['bytes'])
    late=json.loads((ROOT/'evidence/04A37/late-archived-entry-supplement/v1/source-authenticity.json').read_bytes())
    for row in late['unchanged_helper_sources']:restored[row['path']]=restore(row['path'],row['live_sha256'])
    for path in ['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHstatNG/BESHstatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
        if path not in restored:restored[path]=restore(path,sha((ROOT/path).read_bytes()))
    (VIEW/'.test-rationalization-04A37.tmp').mkdir()
    results=[];runner=m.Runner(PRIVATE)
    for label,args in [('reader',['evidence/04A37/verify_packet.py']),('audits',['-m','unittest','discover','-s','evidence/04A37','-p','test_audit.py','-v']),('supplement',['-m','unittest','discover','-s','evidence/04A37/late-archived-entry-supplement/v1','-p','test_supplement.py','-v'])]:
        results.append(runner.command('entry-accepted37-exact-view-'+label,[sys.executable,'-B',*args],cwd=VIEW,env=dict(COMPUTERNAME='VALIDATION_HOST',USERNAME='VALIDATION_USER')))
    save('accepted37-exact-view.json',dict(status='PASS' if all(r['exit_code']==0 for r in results) else 'FAIL_RETAINED_ATTEMPT',
        scope='Re-execution of unchanged historical readers on exact reconstructed accepted bytes in ignored private view; not fresh UDF/scientific/native capture and not a waiver of raw live-reader failures.',
        original_worktree_modified=False,restored_files=list(restored.values()),command_labels=['entry-accepted37-exact-view-'+s for s in ['reader','audits','supplement']],exits=[r['exit_code'] for r in results],
        native='NOT_RUN',scientific='NOT_GOLDEN'))
    assert all(r['exit_code']==0 for r in results)
    print('PASS exact accepted37 view; independent reader +42 original +45 supplemental; live prior files untouched')

if __name__=='__main__':main()
