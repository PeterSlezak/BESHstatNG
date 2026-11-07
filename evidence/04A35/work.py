"""04A.35 local evidence helpers; no Git mutation or native host."""
from pathlib import Path
import sys,json,hashlib,subprocess,re,csv,io,os
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
BASE='f6b605b4a5fc3ffe4ab5f68ddb1f7ef75071311e'
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode('utf-8-sig').rstrip('\r\n')
def sha(b):return hashlib.sha256(b).hexdigest()
def pin(path):
    b=(ROOT/path).read_bytes();return dict(path=str(path),bytes=len(b),sha256=sha(b))
def write(path,text):
    p=ROOT/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(text,encoding='utf-8',newline='\n')
def save(name,value):write('evidence/04A35/'+name,json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n')
def public(path):
    tokens=[os.environ.get('COMPUTERNAME','!unset!'),os.environ.get('USERNAME','!unset!')]
    return '<PRIVATE_PROTECTED_PATH:'+sha(path.encode())[:24]+'>' if re.search(r'[A-Za-z]:|DESKTOP-|LAPTOP-|S-1-5-',path,re.I) or any(re.search(r'(?<![A-Za-z0-9])'+re.escape(t)+r'(?![A-Za-z0-9])',path,re.I) for t in tokens) else path
def method(source,name):
    m=re.search(r'^\s*(?:Public|Private|Friend) (?:Shared )?(?:Function|Sub) '+name+r'\b.*?^\s*End (?:Function|Sub)\b',source,re.M|re.S);assert m,name;return m.group()
if __name__=='__main__':
    assert git('branch','--show-current')=='codex/04A-35' and git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert not git('diff','--cached','--name-only') and not git('diff','--name-only')
    assert not (HERE/'entry-authority.json').exists()
    allfiles=[dict(pin(p),path=public(p)) for p in git('ls-files').splitlines() if (ROOT/p).is_file()]
    save('entry-tracked-pins.json',dict(files=allfiles,scope='Tracked source/evidence only; ignored historical TestResults payload population NOT_REHASHED'))
    prior=[]
    for r in json.loads((ROOT/'evidence/04A34/publication-manifest.json').read_bytes())['files']:
        if r['sha256'] is None:continue
        b=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+r['path']],cwd=ROOT)
        lf=b.replace(b'\r\n',b'\n');cr=lf.replace(b'\n',b'\r\n');variants={'DIRECT':b,'LF':lf,'CRLF':cr,'CRLF_EXCEPT_FINAL_LITERAL_LF':cr[:-2]+b'\n' if cr.endswith(b'\r\n') else cr}
        if r['path'].endswith('.csv'):
            s=io.StringIO(newline='');csv.writer(s).writerows(csv.reader(io.StringIO(lf.decode('utf-8-sig'),newline='')));variants['CSV_RECORD_CRLF_EMBEDDED_LF']=s.getvalue().encode()
        hits=[k for k,v in variants.items() if len(v)==r['bytes'] and sha(v)==r['sha256']];assert hits,r['path']
        now=pin(r['path']);assert (ROOT/r['path']).read_bytes().replace(b'\r\n',b'\n')==lf
        prior.append(dict(path=r['path'],accepted=r,current=now,committed_blob_sha256=sha(b),digest_qualification=hits))
    save('prior-04A34-publication-check.json',dict(status='PASS',integration_commit=BASE,parents=git('show','-s','--format=%P',BASE),files=prior,qualification='Owner accepted34; single-parent integration commit on actual local main, not a fabricated two-parent merge. Exact/EOL-qualified225 accepted artifacts committed; ZIP not write authority.'))
    sources={}
    for name in ['MultipleComparisonPlotExcel','AgreementPlotExcel','PsmLovePlotExcel']:
        p='BESHstatNG/src/Graphics/'+name+'.vb';sources[p]=pin(p);(HERE/'entry-source').mkdir(exist_ok=True);(HERE/'entry-source'/(name+'.vb.txt')).write_bytes((ROOT/p).read_bytes())
    save('pre-edit-source-hashes.json',dict(sources=sources,authority='Actual merged-main source bytes before production edits'))
    save('entry-authority.json',dict(branch=git('branch','--show-current'),head=BASE,main=BASE,merge_base=git('merge-base','HEAD','main'),integration_commit=BASE,pre_merge_34_base='b162a6ac801d7784fc5e27974fbf91aa57a2aee1',index='EMPTY',tracked_status=git('status','--short','--untracked-files=no'),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',scope='Official04A35 only; master plan retained; no36',toolchain=dict(dotnet=subprocess.check_output(['dotnet','--version'],text=True).strip(),node=subprocess.check_output(['node','--version'],text=True).strip(),python=sys.version.split()[0])))
    (ROOT/'.test-rationalization-04A35.tmp').mkdir(exist_ok=False)
    print('Captured',len(allfiles),'tracked pins;225 accepted artifacts;3 original adapters; verified merged main')
