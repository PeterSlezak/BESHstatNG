"""Populate the authenticated C6 per-file cache using read-only original scan calls.

One logical scanner lane runs after other test lanes. At most five helper
processes plus its parent evaluate the exact original candidate multiset.
Neither historical scanner source nor any unittest assertion is rewritten.
"""
from common import *
import ast,collections,importlib.util,traceback,datetime
POLICY=ROOT/'scripts/verify-04a13-publication.py'
GUARD=W2/'continuation-v6/privacy_regex_guard.py'
def function_ast(path,name):
    tree=ast.parse(path.read_text(encoding='utf-8-sig'))
    return ast.dump(next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name==name),include_attributes=False)
def authority():return dict(kernel=pin(HERE/'parallel_scan_v1.py'),policy=pin(POLICY),scan_ast_sha256=sha(function_ast(POLICY,'scan').encode()),git_ast_sha256=sha(function_ast(POLICY,'git').encode()),guard=pin(GUARD),root=str(ROOT),computername=os.environ.get('COMPUTERNAME'),python=sys.version,python_executable=library_pin(Path(sys.executable)),HEAD=git('rev-parse','HEAD').decode().strip())
def original_module():
    spec=importlib.util.spec_from_file_location('c8_original_scan13',POLICY);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);return module
def validate(report,plan):
    assert report['authority']==plan['authority'],'Changed scanner source/environment authority'
    assert report['complete'] and report['exit_code']==0,'Abnormal scanner helper exit'
    expected=collections.Counter((r['index'],r['path'],r['bytes'],r['sha256']) for r in plan['rows'])
    actual=collections.Counter((r['index'],r['path'],r['bytes'],r['sha256']) for r in report['rows'])
    assert expected==actual and len(report['rows'])==len(plan['rows']),'Dropped/duplicate/changed scanner file occurrence'
    for r in report['rows']:
        data=(ROOT/r['path']).read_bytes();assert (len(data),sha(data))==(r['bytes'],r['sha256']),'Scanner input changed after evaluation'
    return report['rows']
def child(plan_path,output):
    work=Path.cwd().resolve();plan=json.loads(Path(plan_path).read_bytes());assert work.is_relative_to(PRIVATE)
    def bounded(value):
        if isinstance(value,(str,bytes,os.PathLike)):
            target=Path(os.fsdecode(value)).absolute().resolve();assert target.is_relative_to(work),'Scanner helper refused write outside its private root'
    def audit(event,args):
        if event=='open':
            mode=args[1] or '';flags=args[2] or 0
            if any(c in mode for c in 'wax+') or flags&(os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):bounded(args[0])
        elif event in ['os.remove','os.rmdir','os.mkdir','os.chmod','os.truncate']:bounded(args[0])
        elif event in ['os.rename','os.replace']:bounded(args[0]);bounded(args[1])
    sys.addaudithook(audit);assert authority()==plan['authority']
    module=original_module();sys.path.insert(0,str(GUARD.parent));from privacy_regex_guard import RegexProxy
    proxy=RegexProxy();module.re=proxy;rows=[];start=time.monotonic();error=None
    try:
        for row in plan['rows']:
            data=(ROOT/row['path']).read_bytes();assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            findings,reviewed=module.scan([row['path']])
            data=(ROOT/row['path']).read_bytes();assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            rows.append(dict(row,findings=findings,reviewed=reviewed))
    except Exception:
        error=traceback.format_exc()
    value=dict(authority=authority(),rows=rows,complete=error is None,exit_code=0 if error is None else 1,error=error,seconds=time.monotonic()-start,pid=os.getpid(),regex_guard=dict(skips=proxy.skips,fallbacks=proxy.fallbacks))
    with Path(output).open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps(dict(files=len(rows),complete=value['complete'],seconds=value['seconds'],pid=value['pid'])),flush=True)
    raise SystemExit(value['exit_code'])
def batches(rows,count):
    lanes=[[] for _ in range(count)];sizes=[0]*count
    for row in sorted(rows,key=lambda r:(-r['bytes'],r['index'])):
        target=min(range(count),key=lambda i:(sizes[i],i));lanes[target].append(row);sizes[target]+=row['bytes']
    return [sorted(lane,key=lambda r:r['index']) for lane in lanes if lane]
def run(rows,folder,count=5):
    from concurrent.futures import ThreadPoolExecutor
    folder.mkdir(exist_ok=False);plans=[];auth=authority()
    for number,batch in enumerate(batches(rows,count),1):
        work=folder/str(number);work.mkdir();(work/'temp').mkdir();(work/'cache').mkdir()
        plan=dict(authority=auth,rows=batch)
        with (work/'input.json').open('x',encoding='utf-8') as f:json.dump(plan,f,ensure_ascii=False,indent=2)
        plans.append((number,work,plan))
    def execute(item):
        number,work,plan=item;output=work/'raw-result.json'
        args=[sys.executable,'-B',str(HERE/'parallel_scan_v1.py'),'child',str(work/'input.json'),str(output)]
        overrides=dict(TEMP=str(work/'temp'),TMP=str(work/'temp'),PYTHONPYCACHEPREFIX=str(work/'cache'),PYTHONHASHSEED='38')
        started=datetime.datetime.now(datetime.timezone.utc).isoformat();begin=time.monotonic()
        result=subprocess.run(args,cwd=work,env=dict(os.environ,**overrides),capture_output=True)
        execution=dict(args=args,cwd=str(work),environment=overrides,start_utc=started,end_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),seconds=time.monotonic()-begin,exit_code=result.returncode,input=pin(work/'input.json'),python_executable=library_pin(Path(sys.executable)))
        with (work/'command.json').open('x',encoding='utf-8') as f:json.dump(execution,f,ensure_ascii=False,indent=2)
        with (work/'stdout.log').open('xb') as f:f.write(result.stdout)
        with (work/'stderr.log').open('xb') as f:f.write(result.stderr)
        assert result.returncode==0 and output.is_file(),'Failed or missing scanner helper; retain partial outputs'
        report=json.loads(output.read_bytes());values=validate(report,plan)
        return dict(number=number,plan=pin(work/'input.json'),result=pin(output),command=pin(work/'command.json'),stdout=pin(work/'stdout.log'),stderr=pin(work/'stderr.log'),exit_code=result.returncode,seconds=report['seconds'],pid=report['pid'],files=len(values),regex_guard=report['regex_guard']),values
    with ThreadPoolExecutor(max_workers=min(count,len(plans))) as pool:results=list(pool.map(execute,plans))
    values=sorted([r for _,batch in results for r in batch],key=lambda r:r['index'])
    assert [(r['index'],r['path']) for r in values]==[(r['index'],r['path']) for r in rows]
    return values,[r for r,_ in results]
class Accelerator:
    def __init__(self,hook,work):self.hook=hook;self.work=work;self.attempts=[];self.fresh=0;self.invocations=0
    def prefill(self,module,candidates):
        # Only the actual immutable scanner domain uses process precomputation.
        # Patched roots/git functions, generators and small/adverse calls retain
        # the original authenticated sequential C6 function/caching behavior.
        if not isinstance(candidates,list) or len(candidates)<32 or Path(module.ROOT).resolve()!=ROOT:return
        if getattr(module.git,'__code__',None) is None or Path(module.git.__code__.co_filename).resolve()!=Path(module.__file__).resolve():return
        assert function_ast(Path(module.__file__),'scan')==function_ast(POLICY,'scan') and function_ast(Path(module.__file__),'git')==function_ast(POLICY,'git')
        rows=[];seen=set();auth=self.hook['authority']
        for index,path in enumerate(candidates):
            target=ROOT/path
            if not target.is_file():continue # Original function retains its missing-file assertion.
            data=target.read_bytes();key=(auth,str(module.ROOT),os.environ.get('COMPUTERNAME'),path,sha(data))
            if key in self.hook['cache'] or key in seen:continue
            seen.add(key);rows.append(dict(index=index,path=path,bytes=len(data),sha256=sha(data)))
        if not rows:return
        self.invocations+=1;values,receipts=run(rows,self.work/('parallel-scan-'+str(self.invocations)),5)
        for r in values:
            key=(auth,str(module.ROOT),os.environ.get('COMPUTERNAME'),r['path'],r['sha256'])
            self.hook['cache'][key]=(r['findings'],[tuple(v) for v in r['reviewed']])
        self.fresh+=len(values);self.attempts.append(dict(files=len(values),receipts=receipts,ordered_inputs_sha256=sha(json.dumps(rows,sort_keys=True).encode())))
    def install(self):
        previous=importlib.util.spec_from_file_location;accelerator=self
        def instrument(name,location,*args,**kwargs):
            spec=previous(name,location,*args,**kwargs);path=Path(location)
            if path.resolve() not in [(ROOT/'scripts/verify-04a13-publication.py').resolve(),(ROOT/'scripts/verify-04a14-publication.py').resolve()]:return spec
            loader=spec.loader
            class Loader:
                def create_module(self,s):return loader.create_module(s)
                def exec_module(self,module):
                    loader.exec_module(module);sequential=module.scan
                    def scan(candidates):
                        accelerator.prefill(module,candidates)
                        return sequential(candidates)
                    module.scan=scan
            spec.loader=Loader();return spec
        importlib.util.spec_from_file_location=instrument
if __name__=='__main__':
    assert sys.argv[1]=='child';child(sys.argv[2],sys.argv[3])
