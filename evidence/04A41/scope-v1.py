from common41 import *
import re,collections
lock()
families={'ParametricUDFs':'Parametric/ANOVA/t tests','NonparametricUDFs':'Rank and correlation tests','AssumptionsUDFs':'Assumptions/normality/outliers','ContingencyTableUDFs':'Contingency/proportions','AgreementMethodUDFs':'Agreement','SampleSizeUDFs':'Sample-size planning'}
def methods(path):
    text=path.read_text(encoding='utf-8-sig');lines=text.splitlines(keepends=True);result=[]
    for i,line in enumerate(lines):
        m=re.match(r'(\s*)(Public|Private|Friend|Protected)\s+(?:Shared\s+)?(?:Function|Sub)\s+(\w+)',line)
        if not m:continue
        end=next(j for j in range(i+1,len(lines)) if re.match(r'^'+re.escape(m[1])+r'End (?:Function|Sub)\b',lines[j]))
        result.append(dict(name=m[3],start=i+1,end=end+1,text=''.join(lines[i:end+1])))
    return result
rows=[];whole=[]
for name,family in families.items():
    p=ROOT/('BESHstatNG/src/ExcelUDFs/'+name+'.vb');text=p.read_text(encoding='utf-8-sig');whole.append(pin(p))
    for method in methods(p):
        if not method['text'].lstrip().startswith('Public Function'):continue
        prefix='\n'.join(text.splitlines()[:method['start']-1]);attribute=prefix[prefix.rfind('<ExcelFunction'):]
        export=re.search(r'Name\s*:=\s*"([^"]+)"',attribute)
        if not export:continue
        body=method['text'];signature=body.split('\n            Try')[0]
        windows=sorted(set(re.findall(r'(?:UdfDataImport|ExcelArgReaders)\.(\w+)',body)))
        common=sorted(set(re.findall(r'ExcelCommon\.([\w.]+)',body)))
        core=sorted(set(re.findall(r'(?i)\b(?:assumptions|parametric|nonparametric|contingencytable|Agreement|SampleSizeCalculator)\.[A-Za-z0-9_]+',body)))
        rows.append(dict(family=family,module=name,function=method['name'],excel_name=export[1],source=pin(p),method_lines=[method['start'],method['end']],method_text_sha256=sha(body.encode()),signature=signature,Windows_import_calls=windows,Common_preparation_calls=common,Core_direct_calls_or_types=core,Windows_result_calls=sorted(set(re.findall(r'\b(?:PrepareResultTableForUdf|MaterializePortableReport38|BuildTwoValueTable|BuildTable|MaterializeSampleSizeResult38|LoggedUdfError|StackWithBlankRow)\b',body))),Windows_only=['Excel-DNA exports and optional-argument/error transport','Range/native acquisition and logging; no native execution here'],source_ownership='Core numerical engines; Common typed request/result/option policies; Windows transport and acquisition remain Windows.',GUI_parity='Source-level parity mapping; documented C26/normality/default/row-policy divergences remain restricted, not forced equal.',error_precedence=[line.strip() for line in body.splitlines() if 'Then Return ExcelError.' in line or 'Catch ex As' in line],original_reference_class='CURRENT_MAIN_PRE_EDIT41;new bounded fixtures are not additions to the sealed PRE_EDIT_FROZEN corpus'))
assert len(rows)==76
all_exports=[]
for p in (ROOT/'BESHstatNG/src/ExcelUDFs').rglob('*.vb'):
    text=p.read_text(encoding='utf-8-sig');all_exports.extend(dict(path=p.relative_to(ROOT).as_posix(),name=n) for n in re.findall(r'<ExcelFunction\s*\([\s\S]*?Name\s*:=\s*"([^"]+)"',text))
assert len(all_exports)==268
save('scope/UDF-inventory-v1.json',dict(kind='AUTHENTICATED_PRE_EDIT_SOURCE_INVENTORY;NOT_SCIENTIFIC_OR_NATIVE_RELEASE',base=MAIN,scoped_exports=76,total_exports=268,family_counts=dict(collections.Counter(r['module'] for r in rows)),functions=rows,all_export_names=all_exports,principal_files=whole,scope_gaps=['No separate exported two-way crossed ANOVA was found among these six principal files; Core/GUI paths are mapped separately, not invented as an export.','All six source files have existing38 preparation; functions already sharing correct paths will not be churned.'],gates=GATES))
register=load(ROOT/'evidence/04A39/register/defects-v7.json')
aliases=['C14-INPUT-FINITE-01','C26-ICC-NONFINITE-COUNT-01','C26-INDEPENDENT-PROP-DEFAULTS-01','C26-GUI-UDF-CONTRACT-01','C26-RESAMPLING-MINIMUM-01']
selected=[i for i in register['issues'] if any(a in i['aliases'] for a in aliases)]
assert len(selected)==5
save(PRIVATE/'canonical-scoped-issues-v1.json',dict(issues=selected,register=pin(ROOT/'evidence/04A39/register/defects-v7.json')))
print('76 scoped exports /268 total; method/source/error-order and existing Common mappings captured',flush=True)
