"""Reproduce the merge gate and preserve the owner-authorized newline crosswalk."""
from common39 import *
from collections import Counter
import platform
import zipfile

assert git('rev-parse','HEAD').decode().strip() == MAIN
assert git('rev-parse','main^').decode().strip() == 'ba1636f5356ac761509c2cfa16fc66b92430935d'
assert git('branch','--show-current').decode().strip() == 'codex/04A-39'
assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
freeze = ROOT/'evidence/04A38/completion/w2/continuation-v17-postcorrection-v1/production-test-freeze-v1.json'
assert pin(freeze)['sha256'] == '43cd9af03ab09829c205b694f6650df1779766692159da791144b9f48f7c868e'
delivery = load(W3/'final/delivery-receipt-v1.json')
assert pin(W3/'final/delivery-receipt-v1.json')['sha256'] == '2b8edd7147d7f0f9f648ee7e8bff1ee425f41d53cc72df785a15782168344f89'
for row in delivery.values():
    if isinstance(row,dict) and {'path','sha256','bytes'} <= row.keys(): assert pin(row['path']) == row
tree = {}
for line in git('ls-tree','-r','main').splitlines():
    meta,path=line.split(b'\t',1)
    assert path.decode().casefold() not in tree
    tree[path.decode().casefold()] = (path.decode(),meta.split()[2].decode())
files=load(freeze)['sources']
blobs=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks','cat-file','--batch'],
    input=('\n'.join(tree[x['path'].casefold()][1] for x in files)+'\n').encode(),cwd=ROOT)
inventory={x['path']:x for x in load(W3/'publication/final-logical-inventory-v1.json')['files']}
parts=load(W3/'publication/parts-index-v1.json')
paths={x['name']:x['path'] for x in parts['reference_parts']}
paths[Path(parts['new_active']['path']).name]=parts['new_active']['path']
archives={};rows=[];position=0
PRIVATE.mkdir(exist_ok=False)
snapshot=PRIVATE/'source-preimages-v1';snapshot.mkdir()
for row in files:
    end=blobs.index(b'\n',position);length=int(blobs[position:end].split()[2])
    blob=blobs[end+1:end+1+length];position=end+length+2
    data=(ROOT/row['path']).read_bytes();original=data
    if sha(data)!=row['sha256']:
        entry=inventory[row['path']];assert entry['sha256']==row['sha256']
        part=entry['part']
        if part not in archives:archives[part]=zipfile.ZipFile(ROOT/paths[part])
        original=archives[part].read(row['path'])
    assert sha(original)==row['sha256']
    assert original.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n')==data.replace(b'\r\n',b'\n')
    target=snapshot/row['path'];target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(data)
    rows.append(dict(accepted=row,current=pin(row['path']),git_path=tree[row['path'].casefold()][0],
        git_blob_oid=tree[row['path'].casefold()][1],git_bytes=len(blob),git_sha256=sha(blob),
        git_classification='EXACT_BYTES' if sha(blob)==row['sha256'] else 'OWNER_AUTHORIZED_LINE_ENDINGS_ONLY',
        live_classification='EXACT_BYTES' if sha(data)==row['sha256'] else 'OWNER_AUTHORIZED_LINE_ENDINGS_ONLY'))
for archive in archives.values():archive.close()
save('entry/line-ending-reconciliation-v1.json',dict(authority='Owner instruction in current conversation, 2026-11-14',
    exact_quote='I authorize the line-endings change as acceptable. Please continue with 04A.39',
    original_hashes_modified=False,rows=rows,git_counts=dict(Counter(x['git_classification'] for x in rows)),
    live_counts=dict(Counter(x['live_classification'] for x in rows)),semantic_differences=0))
save('entry/source-preimages-v1.json',dict(kind='04A39_CURRENT_PRE_CHANGE',main=MAIN,
    files=[x['current'] for x in rows],private_detached_snapshot=snapshot.relative_to(ROOT).as_posix(),
    git_tree_omissions=[],source_count=len(rows)))
commands=[]
for args in [('branch','--show-current'),('rev-parse','HEAD','main'),('merge-base','HEAD','main'),
             ('status','--porcelain=v1','--untracked-files=all'),('diff','--cached','--name-status'),
             ('diff','--check'),('log','--all','-15','--format=%H %P %s')]:
    output=git(*args);commands.append(dict(arguments=args,stdout=output.decode(),stdout_sha256=sha(output),exit_code=0,utc=utc()))
authorities=[freeze,W3/'final/delivery-receipt-v1.json',ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md',
    ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md',ROOT/'AGENTS.md']
save('entry/merge-preflight-v1.json',dict(status='04A38_OWNER_ACCEPTED_AND_MERGED_VERIFIED',utc=utc(),
    main=MAIN,parent=git('rev-parse','main^').decode().strip(),branch='codex/04A-39',branch_parent=MAIN,
    owner_acceptances=[dict(scope='W2',date='2026-11-13'),dict(scope='W3 software qualified',date='2026-11-14'),
                       dict(scope='Phase04A.38 overall',date='2026-11-14')],
    acceptance_basis='Owner assertions in current prompt; no invented prior receipt/signature',
    commands=commands,index=pin('.git/index'),authorities=[pin(x) for x in authorities],
    gate_proof=pin(HERE/'entry/line-ending-reconciliation-v1.json'),gates=GATES,
    inherited_preservation=dict(file_pins=37484,archive_records=154,physical_scope='Inherited qualified W3 audit; no new whole-union byte audit claimed'),
    preflight_attempts=['Initial branch creation denied by filesystem sandbox before any index/branch write; authorized tool escalation then succeeded.',
        'Initial inspection used absent evidence/04A04/07/27 paths; actual authorities found under docs/architecture.',
        'One read-only diagnostic assumed dictionary for the1514-entry list; corrected diagnostic only; no campaign started.']))
versions=[]
for label,args in [('dotnet',['dotnet','--info']),('node',['node','--version']),('Python',[sys.executable,'--version'])]:
    result=command('entry-'+label,args);versions.append(dict(tool=label,receipt=result))
save('entry/environment-v1.json',dict(utc=utc(),platform=platform.platform(),python=sys.version,tools=versions,
    note='Culture-specific fixtures explicitly set and restore their thread cultures; no global locale changes.'))
print('Entry sealed:',len(rows),'sources; accepted merged main',MAIN,flush=True)
