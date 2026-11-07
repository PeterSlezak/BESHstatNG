"""04A.36 bounded local evidence helpers. No native host or Git mutation."""
from pathlib import Path
import sys,json,hashlib,subprocess,re,csv,io,os
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
BASE='92f5916d9dc0aaa727c99c0ceea9c89c10eefef3'
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode('utf-8-sig').rstrip('\r\n')
def sha(b):return hashlib.sha256(b).hexdigest()
def pin(path):
    b=(ROOT/path).read_bytes();return dict(path=str(path),bytes=len(b),sha256=sha(b))
def write(path,text):
    p=ROOT/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(text,encoding='utf-8',newline='\n')
def save(name,value):write('evidence/04A36/'+name,json.dumps(value,ensure_ascii=False,indent=2,allow_nan=False)+'\n')
def public(path):
    tokens=[os.environ.get('COMPUTERNAME','!unset!'),os.environ.get('USERNAME','!unset!')]
    private='/TestResults/' in path or path.startswith('TestResults/') or re.search(r'[A-Za-z]:|DESKTOP-|LAPTOP-|S-1-5-',path,re.I) or any(re.search(r'(?<![A-Za-z0-9])'+re.escape(t)+r'(?![A-Za-z0-9])',path,re.I) for t in tokens)
    return '<PRIVATE_PROTECTED_PATH:'+sha(path.encode())[:24]+'>' if private else path
def csv_records_only_crlf(b):
    quoted=False;result=bytearray()
    for c in b:
        if c==34:quoted=not quoted
        if c==10 and not quoted:result.append(13)
        result.append(c)
    return bytes(result)
if __name__=='__main__':
    assert git('branch','--show-current')=='codex/04A-36'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert not git('diff','--cached','--name-only') and not git('diff','--name-only')
    assert not (HERE/'entry-authority.json').exists()
    files=[dict(pin(p),path=public(p)) for p in git('ls-files').splitlines() if (ROOT/p).is_file()]
    save('entry-tracked-pins.json',dict(files=files,scope='Tracked files only. Historical ignored TestResults payloads NOT_REHASHED.'))
    prior=[]
    for r in json.loads((ROOT/'evidence/04A35/publication-manifest.json').read_bytes())['files']:
        if r['sha256'] is None:continue
        actual='evidence/04A35/auxilary.py' if r['path']=='evidence/04A35/aux.py' else r['path']
        b=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+actual],cwd=ROOT)
        lf=b.replace(b'\r\n',b'\n');cr=lf.replace(b'\n',b'\r\n')
        variants={'DIRECT':b,'LF':lf,'CRLF':cr,'CRLF_EXCEPT_FINAL_LITERAL_LF':cr[:-2]+b'\n' if cr.endswith(b'\r\n') else cr}
        if actual.endswith('.csv'):variants['CSV_RECORD_CRLF_EMBEDDED_LF']=csv_records_only_crlf(lf)
        hits=[k for k,v in variants.items() if len(v)==r['bytes'] and sha(v)==r['sha256']];assert hits,actual
        assert (ROOT/actual).read_bytes().replace(b'\r\n',b'\n')==lf
        prior.append(dict(accepted=r,current=pin(actual),committed_blob_sha256=sha(b),qualification=hits,owner_rename=actual!=r['path']))
    save('prior-04A35-publication-check.json',dict(status='PASS',files=prior,integration_commit='6f1a27dc2a3aaa3f58326557c087e508344500b8',owner_filename_correction_commit=BASE,qualification='Owner accepted35; committed single-parent integration plus explicit owner aux.py to auxilary.py filename correction. Historical manifest unchanged. EOL qualification proves exact original bytes.'))
    sources={}
    names=['BESHstatNG/src/Graphics/'+n+'.vb' for n in ['KaplanMeierPlotExcel','CoxSurvivalPlotExcel','SpcControlChartExcel']]
    names+=['BESHStat.Excel.Common/src/Results/SurvivalReportPolicy.vb','BESHStat.Excel.Common/src/Analysis/SpcUnivariateChartProjectionPolicy.vb','BESHstatNG/src/UI/Ui4Cox.Designer.vb','planning/BESHStatNG_CrossPlatform_MasterPlan.md']
    for p in names:
        sources[p]=pin(p);dest=HERE/'entry-source'/p;dest.parent.mkdir(parents=True,exist_ok=True);dest.with_suffix(dest.suffix+'.txt').write_bytes((ROOT/p).read_bytes())
    save('pre-edit-source-hashes.json',dict(sources=sources,authority='Actual merged-main bytes BEFORE any production edit'))
    save('entry-authority.json',dict(branch=git('branch','--show-current'),head=BASE,main=BASE,merge_base=git('merge-base','HEAD','main'),parents=git('show','-s','--format=%P',BASE),index='EMPTY',tracked_status=git('status','--short','--untracked-files=no'),untracked_at_capture=git('ls-files','--others','--exclude-standard').splitlines(),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),scientific='NOT_GOLDEN',scope='Official04A36 only; owner Master Plan unchanged; no37',toolchain=dict(dotnet=subprocess.check_output(['dotnet','--version'],text=True).strip(),node=subprocess.check_output(['node','--version'],text=True).strip(),python=sys.version.split()[0])))
    (ROOT/'.test-rationalization-04A36.tmp').mkdir(exist_ok=False)
    print('Captured',len(files),'tracked pins;',len(prior),'accepted35 artifacts; clean post35 merged base')
