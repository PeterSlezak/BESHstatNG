from common43 import *
import re,collections
def methods(p):
    lines=p.read_text(encoding='utf-8-sig').splitlines(keepends=True);out=[]
    for i,line in enumerate(lines):
        m=re.match(r'(\s*)(Public|Private|Friend|Protected)\s+(?:Shared\s+)?(?:Function|Sub)\s+(\w+)',line)
        if not m:continue
        end=next(j for j in range(i+1,len(lines)) if re.match('^'+re.escape(m[1])+r'End (?:Function|Sub)\b',lines[j]))
        out.append(dict(name=m[3],start=i+1,end=end+1,text=''.join(lines[i:end+1])))
    return out
if __name__=='__main__':
    lock();families=[];exports=[];files=[];all_exports=[]
    for p in sorted((ROOT/'BESHstatNG/src/ExcelUDFs').rglob('*.vb')):
        s=p.read_text(encoding='utf-8-sig');declarations=list(re.finditer(r'Private ReadOnly (\w+) As New ConcurrentDictionary\(Of String, (\w+)\)\(StringComparer.OrdinalIgnoreCase\)',s));all_exports+=re.findall(r'<ExcelFunction\s*\([\s\S]*?Name\s*:=\s*"([^"]+)"',s)
        if not declarations:continue
        files.append(pin(p));members=methods(p)
        for m in members:
            if not m['text'].lstrip().startswith('Public Function'):continue
            prefix='\n'.join(s.splitlines()[:m['start']-1]);attribute=prefix[prefix.rfind('<ExcelFunction'):];name=re.search(r'Name\s*:=\s*"([^"]+)"',attribute)
            if not name:continue
            direct=[d for d in declarations if d[1] in m['text']]
            if len(declarations)==1:decl=declarations[0]
            elif direct:assert len(direct)==1;decl=direct[0]
            else:
                key=m['name'].split('_')[0].lower();decl=next(d for d in declarations if d[1].lower()=='_'+key+'cache')
            cache=decl[1];role='FIT' if m['name'].endswith('_FIT') else 'DROP' if m['name'].endswith('_DROP') else 'CLEAR_ALL' if m['name'].endswith('_CLEAR_ALL') else 'CLEANUP' if m['name'].endswith('_CLEANUP') else 'QUERY'
            exports.append(dict(module=p.stem,function=m['name'],excel_name=name[1],role=role,cache=cache,cache_value_type=decl[2],source=pin(p),method_sha256=sha(m['text'].encode()),lines=[m['start'],m['end']],signature=m['text'].split('\n            Try')[0],attributes=attribute,body=m['text'],host_errors=[v.strip() for v in m['text'].splitlines() if 'Return ExcelError' in v or 'Catch ex' in v],cache_mutation_lines=[v.strip() for v in m['text'].splitlines() if any(t in v for t in ['.TryRemove(','.Clear()','Cache(handle','Cache(key'])],coercion='Windows ExcelArgReaders.AsString: missing->Nothing,Convert.ToString with current culture then Trim;direct/helper and catches preserved.',workbook_identity='NOT_ESTABLISHED',verified_session_epoch='NOT_ESTABLISHED',recalc_flags='Explicit attribute text and compiled metadata retained;no registration flag changes.',output_shape='Actual typed execution cases and cumulative sealed corpus;not inferred from Object return alone.'))
        for d in declarations:
            owned=[r for r in exports if r['source']['path']==p.relative_to(ROOT).as_posix() and r['cache']==d[1]];fit=next(r for r in owned if r['role']=='FIT');prefix=re.search(r'"([A-Z0-9]+):"\s*&\s*Guid.NewGuid\(\).ToString\("N"\)',fit['body']);assert prefix
            families.append(dict(family=prefix[1],module=p.stem,path=pin(p),dictionary=d[1],cache_value_type=d[2],declaration=d[0],declaration_line=s.count('\n',0,d.start())+1,declaration_sha256=sha(d[0].encode()),generation=prefix[1]+':'+('N_GUID_32_HEX'),FIT=fit['function'],queries=[r['function'] for r in owned if r['role']=='QUERY'],drop=[r['function'] for r in owned if r['role']=='DROP'],clear=[r['function'] for r in owned if r['role'] in ['CLEAR_ALL','CLEANUP']],public_exports=[r['excel_name'] for r in owned],owner='WINDOWS_SESSION_GLOBAL_DICTIONARY',scope='Session-global static storage;workbook attribution NOT_ESTABLISHED',thread_semantics='Atomic dictionary operation only;model mutability and Count-then-Clear not made serializable.',memory='Explicit remove/clear;no TTL/LRU/capacity/disposal policy activated.',status='RETAINED_RESTRICTED_NATIVE_LIFECYCLE'))
    assert len(all_exports)==268 and len(families)==18 and len(files)==12
    occurrences=[]
    for p in sorted((ROOT/'BESHstatNG/src').rglob('*.vb')):
        if b'ConcurrentDictionary' in p.read_bytes():occurrences.append(pin(p))
    save('scope/cache-export-inventory-v1.json',dict(base=MAIN,families=families,cache_families=len(families),cache_source_modules=len(files),handle_exports=len(exports),total_exports=268,exports=exports,all_Windows_ConcurrentDictionary_source_occurrences=occurrences,helper=pin('BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb'),input_policy_map=pin('evidence/04A39/map/method-policy-map-v1.json'),extraction_ledger=pin('evidence/04A38/entry-members.json'),canonical_register=pin('evidence/04A39/register/defects-v7.json'),accepted_deltas=[pin('evidence/04A'+n+'/register/'+f) for n,f in [('40','issue-delta-v4.json'),('41','issue-delta-v2.json'),('42','issue-delta-v1.json')]],gates=GATES))
    print('Live',len(families),'families /',len(files),'cache modules /',len(exports),'handle exports /268 total',flush=True)
