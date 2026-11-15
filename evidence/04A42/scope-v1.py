from common42 import *
import re,collections
families=['MultivariateAnalysisUDFs','ClassificationReportUDFs','PropensityScoreUDFs','PlotDataUDFs']
def methods(p):
    lines=p.read_text(encoding='utf-8-sig').splitlines(keepends=True);out=[]
    for i,line in enumerate(lines):
        m=re.match(r'(\s*)(Public|Private|Friend|Protected)\s+(?:Shared\s+)?(?:Function|Sub)\s+(\w+)',line)
        if not m:continue
        end=next(j for j in range(i+1,len(lines)) if re.match('^'+re.escape(m[1])+r'End (?:Function|Sub)\b',lines[j]))
        out.append(dict(name=m[3],start=i+1,end=end+1,text=''.join(lines[i:end+1])))
    return out
if __name__=='__main__':
    lock();rows=[]
    for name in families:
        p=ROOT/('BESHstatNG/src/ExcelUDFs/'+name+'.vb');text=p.read_text(encoding='utf-8-sig')
        for m in methods(p):
            if not m['text'].lstrip().startswith('Public Function'):continue
            prefix='\n'.join(text.splitlines()[:m['start']-1]);attribute=prefix[prefix.rfind('<ExcelFunction'):];export=re.search(r'Name\s*:=\s*"([^"]+)"',attribute)
            if not export:continue
            rows.append(dict(module=name,function=m['name'],excel_name=export[1],source=pin(p),lines=[m['start'],m['end']],method_sha256=sha(m['text'].encode()),signature=m['text'].split('\n            Try')[0],ExcelFunction_attribute=attribute,Common_calls=sorted(set(re.findall(r'ExcelCommon\.([\w.]+)',m['text']))),host_errors=[s.strip() for s in m['text'].splitlines() if 'Return ExcelError' in s or 'Catch ex' in s],ownership='Core algorithms;Common payload/row-label-array preparation;Windows acquisition,error transport and session cache/lifetime stay Windows',existing_body=m['text']))
    exports=[]
    for p in (ROOT/'BESHstatNG/src/ExcelUDFs').rglob('*.vb'):
        exports.extend(re.findall(r'<ExcelFunction\s*\([\s\S]*?Name\s*:=\s*"([^"]+)"',p.read_text(encoding='utf-8-sig')))
    assert len(exports)==268
    save('scope/UDF-inventory-v1.json',dict(base=MAIN,scoped_exports=len(rows),total_exports=len(exports),family_counts=dict(collections.Counter(r['module'] for r in rows)),functions=rows,gates=GATES))
    print('Live scoped inventory',len(rows),dict(collections.Counter(r['module'] for r in rows)),flush=True)
