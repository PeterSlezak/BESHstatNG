"""Original scanner functions read exact ZIP members, not a staging directory."""
from witness_lib import *
import ast,collections,re,unicodedata,zipfile

EXPECTED={'13':'b26596e5bf9644e81a479fad4f28d3db8d8af23611175b8e582c759007009a03','14':'b4b4fd3567947d807c73e63022a0128ded37199248d0067f786cb472811d5a25'}
def guard():
    path=HERE/'dependencies/privacy_regex_guard.py'
    assert digest(path)=='ad17f82adca850afb94fa60d3c0ca5866b446860a7724dc77a5e5b3111c9ce5f'
    return imported('authenticated_email_guard_witness',path)
def scanner(phase):
    path=ROOT/('scripts/verify-04a'+phase+'-publication.py');assert digest(path)==EXPECTED[phase]
    m=imported('unchanged_physical_scanner_'+phase,path);m.re=guard().RegexProxy()
    return m
def function_ast(m,name):
    return ast.dump(next(n for n in ast.parse(Path(m.__file__).read_text(encoding='utf-8-sig')).body if isinstance(n,ast.FunctionDef) and n.name==name),include_attributes=False)
def tokens():
    users={p.name for p in ROOT.parents if p.parent.name.casefold()=='users'}
    assert users and os.environ.get('COMPUTERNAME'),'OWNER_LOCAL_CONTEXT_UNAVAILABLE'
    return users|{os.environ['COMPUTERNAME']}
class Member:
    def __init__(self,archive,name,codec):self.archive=archive;self.name=name;self.codec=codec
    def is_file(self):return self.name in self.archive.NameToInfo
    def read_text(self,encoding):
        assert encoding=='utf-8-sig'
        data=self.archive.read(self.name);value=data.decode(self.codec,errors='strict')
        if self.codec!='utf-8-sig':assert value.encode(self.codec)==data
        return value
class ZipRoot:
    parents=ROOT.parents
    name=ROOT.name
    def __init__(self,archive,codec):self.archive=archive;self.codec=codec
    def __truediv__(self,path):return Member(self.archive,path,self.codec)
    def __fspath__(self):return str(ROOT)
    def as_posix(self):return ROOT.as_posix()
def safe_name(name):
    assert name and not name.startswith('/') and '\\' not in name and ':' not in name
    assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in name.split('/'))
    assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',name)
def inventory(archive):
    names=archive.namelist();assert len(names)==len(set(names))==len({n.casefold() for n in names})
    rows=[]
    for info in archive.infolist():
        safe_name(info.filename);assert not info.is_dir() and ((info.external_attr>>16)&0o170000)!=0o120000
        data=archive.read(info)
        rows.append(dict(path=info.filename,bytes=len(data),sha256=sha(data),crc32=f'{info.CRC:08x}'))
    return rows
def codecs(archive,rows):
    sources={r['path']:r for r in load(OLD/'entry/source-lock.json')['files']}
    selected=[]
    for row in rows:
        data=archive.read(row['path'])
        try:data.decode('utf-8-sig',errors='strict');views=['utf-8-sig']
        except UnicodeDecodeError:
            assert row['path'] in sources and row['sha256']==sources[row['path']]['sha256'],'UNKNOWN_NON_UTF8_MEMBER'
            views=['cp1250','cp1252']
            for codec in views:assert data.decode(codec,errors='strict').encode(codec)==data
        selected.extend(dict(path=row['path'],encoding_view=codec,source_sha256=row['sha256']) for codec in views)
    return selected
def detailed(m,names):
    """Observer only: original decisions are compared to direct scan in controls."""
    details=[]
    class Findings(list):
        def append(self,row):
            local=sys._getframe(1).f_locals
            match=re.search(local['pattern'],local['value']) if row['category']=='known_private_identifier' else local['match']
            details.append(dict(row,offset=match.start(),match_sha256=sha(match.group().replace('\\\\','\\').casefold().encode()),raw_match=match.group()))
            super().append(row)
    tree=ast.parse(Path(m.__file__).read_text(encoding='utf-8-sig'));func=next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='scan')
    assign=next(n for n in ast.walk(func) if isinstance(n,ast.Assign) and isinstance(n.value,ast.Tuple) and ast.unparse(n.targets[0])=='(findings, reviewed)')
    assign.value.elts[0]=ast.Call(func=ast.Name(id='Findings',ctx=ast.Load()),args=[],keywords=[])
    ns=dict(vars(m),Findings=Findings);exec(compile(ast.fix_missing_locations(ast.Module(body=[func],type_ignores=[])),m.__file__,'exec'),ns)
    f,r=ns['scan'](names)
    assert [{k:d[k] for k in f[i]} for i,d in enumerate(details)]==f
    return f,r,details

def run():
    verify_frozen();context=tokens();save(PRIVATE/'owner-context.json',dict(owner_identifiers=sorted(context),ROOT=str(ROOT),hostname=os.environ['COMPUTERNAME']))
    modules={p:scanner(p) for p in ('13','14')}
    assert function_ast(modules['13'],'scan')==function_ast(modules['14'],'scan')
    assert function_ast(modules['13'],'git')==function_ast(modules['14'],'git')
    archived=[]
    for n,record in enumerate(load(HERE/'entry.json')['input_archives']):
        label='main' if n==0 else 'postseal'
        with zipfile.ZipFile(ROOT/record['path']) as z:
            rows=inventory(z);assert len(rows)==(2060 if label=='main' else 19)
            # Independently recheck every indexed member, including gate/postseal metadata.
            index=load_index=json.loads(z.read('.review-inventory.json' if label=='main' else '.postseal-inventory.json'))
            expected={r['path']:r for r in index['files']}
            selfname='.review-inventory.json' if label=='main' else '.postseal-inventory.json'
            assert set(r['path'] for r in rows)==set(expected)|{selfname}
            for row in rows:
                if row['path']!=selfname:assert all(row[k]==expected[row['path']][k] for k in ('bytes','sha256','crc32'))
            views=codecs(z,rows)
        archived.append(dict(label=label,archive=record,files=rows,views=views,index_self_excluding=True,CRC_SHA_size='PASS'))
    save('physical-inputs.json',dict(archives=archived,owner_context=dict(token_count=len(context),token_fingerprints=sorted(sha(t.casefold().encode()) for t in context),
        username_present=True,hostname_present=True,raw_context=pin(PRIVATE/'owner-context.json')),scope='Every physical path/member, including generated metadata; UTF8sig plus both strict round-tripping legacy source views. Original scanner adds content, NFKC, JSON-decoded and base64-decoded views. No stage input.'))
    reports=[]
    for phase,m in modules.items():
        for package in archived:
            f=[];reviewed=set();start=utc();journal=PRIVATE/('scan-'+phase+'-'+package['label']+'.jsonl')
            with zipfile.ZipFile(ROOT/package['archive']['path']) as z,journal.open('x',encoding='utf-8',newline='\n') as log:
                for i,view in enumerate(package['views']):
                    m.ROOT=ZipRoot(z,view['encoding_view'])
                    found,seen=m.scan([view['path']])
                    f.extend(dict(row,encoding_view=view['encoding_view']) for row in found)
                    reviewed.update(seen)
                    log.write(json.dumps(dict(view=view,findings=len(found),reviewed=seen))+'\n');log.flush()
                    if (i+1)%400==0:print('Physical original scanner',phase,package['label'],i+1,'member views completed',flush=True)
            report=dict(scanner_phase=phase,archive=package['archive'],physical_members=len(package['files']),member_views=len(package['views']),
                findings=f,reviewed=sorted(reviewed),start_utc=start,end_utc=utc(),journal=pin(journal),scanner=pin(m.__file__),guard=pin(HERE/'dependencies/privacy_regex_guard.py'),
                scan_AST_sha256=sha(function_ast(m,'scan').encode()),guard_stats=dict(skips=m.re.skips,fallbacks=m.re.fallbacks),original_scan_callable=True)
            save(PRIVATE/('scanner-'+phase+'-'+package['label']+'.json'),report);reports.append(report)
            assert len(f)==(25 if package['label']=='main' else 0),'UNEXPECTED_PHYSICAL_FINDING_POPULATION'
            print('Original scanner',phase,package['label'],'completed:',len(f),'findings;',len(package['files']),'physical members',flush=True)
    assert reports[0]['findings']==reports[2]['findings'] and reports[1]['findings']==reports[3]['findings']
    save('scanner-physical-results.json',dict(reports=reports,owner_local_tokens_applied=True,source_rules_changed=False,fresh_executions=4,
        scanner_findings_are_not_automatic_pass=True,fixture_disposition_required=True,raw_context_not_published=True,physical_archive_hashes_rechecked=[pin(r['path']) for r in load(HERE/'entry.json')['input_archives']]))

if __name__=='__main__':run()
