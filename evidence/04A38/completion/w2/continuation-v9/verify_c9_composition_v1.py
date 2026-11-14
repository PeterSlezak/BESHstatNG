"""Independent full-phase reconstruction with explicit source-bound substitutions."""
import json,re,hashlib
from pathlib import Path
from source_views import body
ROOT=Path(__file__).resolve().parents[5];PREFIX=Path(__file__).resolve().parent.relative_to(ROOT).as_posix()+'/'
def need(value,message):
    if not value:raise ValueError(message)
def tokens(text):
    result=[]
    for m in re.finditer(r'"(?:[^"\n]|"")*"|\x27[^\n]*|[^\s]',text):
        t=m[0]
        if t.startswith("'"):continue
        result.append(t if t.startswith('"') else t.lower())
    return ''.join(result)
def phase(text):
    text=text.replace('\r\n','\n');header=re.search(r'\)\s+As [^\n]+\n',text);need(header is not None,'Missing typed function boundary')
    return text[header.end():text.rfind('End Function')].strip()
def function(text,name):
    match=re.search(r'(?m)^\s*(?:Public|Private|Friend) (?:Shared )?Function '+re.escape(name)+r'\(',text);need(match is not None,'Missing complete named source')
    end=text.index('End Function',match.end())+len('End Function');return text[match.start():end]
def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {}
    raw=lambda p:overrides[p] if p in overrides else (root/p).read_bytes()
    load=lambda p:json.loads(raw(p));read=lambda p:raw(p).decode('utf-8-sig').replace('\r\n','\n')
    u=load(PREFIX+'current-universe-packet-v1.json');pu=load(PREFIX+'portable-universe-v1.json');before=load('evidence/04A38/completion/w2/continuation-v8/current-universe-packet-v1.json');extraction=load(PREFIX+'source-extraction-v1.json');trans={r['path']:r for r in load(PREFIX+'source-transitions-final-v1.json')['files']};rows=[]
    byname={n['name']:n for n in u['members'] if n['parent_member'] is None};pby={(n['owner'].split('.')[-1],n['name']):n for n in pu['members']}
    for section,owner in [('agreement','UdfAgreementReportPreparation38'),('contingency','UdfContingencyPreparation38')]:
        for entry in extraction[section]:
            old=next(n for n in before['members'] if n['symbol_id']==entry['parent']);n=byname[old['name']];oldtext=body(raw(trans[n['path']]['preimage']['path']),old)[0].replace('\r\n','\n');newtext=body(raw(n['path']),n)[0].replace('\r\n','\n');original=entry['original_phase'];call=entry['new_call']
            need(oldtext.count(original)==1,'Phase is not authenticated immediate whole parent');need(newtext==oldtext.replace(original,call.rstrip(),1) if section=='agreement' else newtext==oldtext.replace(original+'\n',call,1),'Changed code outside extracted parent phase')
            pn=pby[owner,entry['api']];prepared=phase(body(raw(pn['path']),pn)[0]);expected=original
            if section=='agreement':
                expected=expected.replace('Return PrepareResultTableForUdf(t)','Return t')
                if entry['api']=='BuildRegressionCoefficients':
                    expected=expected.replace('fit.SlopeCI','slope').replace('fit.InterceptCI','intercept').replace('mdl.SlopeCI','slope').replace('mdl.InterceptCI','intercept')
                    expected=re.sub(r't.AddTitle\("[^\"]+"\)','t.AddTitle(title)',expected)
            else:expected=expected.replace('Return ExcelError.ExcelErrorNum','Return Nothing').replace('Dim body As Object(,)','Dim reportBody As Object(,)')+'\nReturn reportBody'
            need(tokens(prepared)==tokens(expected),'Portable full phase reconstruction differs '+old['name']);rows.append(dict(parent=old['symbol_id'],api=pn['symbol_id'],proof='AUTHENTIC_FULL_PARENT_REPLACEMENT_AND_FULL_PHASE_RECONSTRUCTION',original_phase_sha256=hashlib.sha256(original.encode()).hexdigest()))
    native='BESHstatNG/src/RegModels/BinaryClassificationReporting.vb';original=read(trans[native]['preimage']['path']);current=read(native)
    for entry in extraction['classification']:
        name=entry['name'];old=entry['original_whole_body'];need(old in original,'Unpinned classification original body');header=old[:old.index(' As Object\n')+len(' As Object')];need(function(current,name).strip()==(header+'\n'+entry['new_call']+'\n        End Function').strip(),'Native reporting facade changed outside its phase')
        expected=entry['original_phase'].replace('MissingOutputValue','missingValue').replace('PrepareResultTableForUdfLocal(out)','UdfSpillPreparation38.NormalizeBlanks(out)');expected=re.sub(r'PercentOrMissingValue\((summary\.\w+|r\.\w+)\)',r'PercentOrMissingValue(\1, missingValue)',expected);pn=pby['UdfClassificationReportPreparation38',name];actual=phase(body(raw(pn['path']),pn)[0]);need(tokens(actual)==tokens(expected),'Classification full report differs');rows.append(dict(parent=name,api=pn['symbol_id'],proof='FULL_REPORT_ROW_ORDER_SCALAR_TYPES_OPAQUE_HOST_SENTINEL_PRESERVED'))
    oldnormal=phase(function(original,'PrepareResultTableForUdfLocal'));normal=phase(function(read('BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb'),'NormalizeBlanks'));normal=re.sub(r'Dim arr As Object\(,\) = TryCast\(table, Object\(,\)\)\s*','',normal,count=1);normal=re.sub(r'\barr\b','table',normal)
    normal=re.sub(r'If v Is Nothing Then\s*out\(r, c\) = String.Empty[^\n]*\n\s*ElseIf TypeOf v Is DBNull Then\s*out\(r, c\) = String.Empty[^\n]*', 'If v Is Nothing OrElse TypeOf v Is DBNull Then\n out(r, c) = String.Empty',normal)
    need(tokens(normal)==tokens(oldnormal),'Generated-matrix blank-copy reconstruction differs');rows.append(dict(proof='FULL_NORMALIZER_RECONSTRUCTION_TYPED_GENERATED_OBJECT_MATRIX_ONLY',scope='TryCast is identity on the actual Object(,) argument; Nothing/DBNull branches combine to the original short-circuit predicate. Both copies are zero-based row-major, preserve cell references and throw identically outside supported bounds.'))
    contracts=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];strict=phase(function(read('BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'),'TryStrictVector'))
    for name,dim,index in [('TryReadStrictNumericVectorFromSingleColumn','rows','i'),('TryReadStrictNumericVectorFromSingleRow','cols','j')]:
        c=next(c for c in contracts if c['current_record']['name']==name);old=phase(c['original_whole_body']);axis=0 if dim=='rows' else 1;old=re.sub(r'Dim '+dim+r' As Integer = arr.GetLength\('+str(axis)+r'\)\s*','',old,count=1)
        blank='Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell';numeric='Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble'
        for v in ['last','0',index]:
            cell='arr('+v+', 0)' if axis==0 else 'arr(0, '+v+')'
            old=old.replace(blank+'('+cell+')','blank('+v+')').replace(numeric+'('+cell+')','numeric('+v+')')
        for a,b in [(dim,'count'),(index,'index'),('out','result'),('d','value')]:old=re.sub(r'\b'+a+r'\b',b,old)
        need(tokens(old)==tokens(strict),'Full original strict vector loop differs under actual callbacks '+name);rows.append(dict(parent=c['original_id'],proof='FULL_ORIGINAL_LOOP_TO_ACCEPTED_PREPARED_LOOP_EXACT_SOURCE_COMPOSITION',bindings=dict(axis=axis,blank='arr(index,0)' if axis==0 else 'arr(0,index)',numeric='same captured array cell'),reset='Both resets precede first observable callback. Between them only intrinsic length access and pure compiler closure/delegate construction occur; output remains the actual ByRef alias, never an extra boxed copy.'))
    return dict(status='FULL_SOURCE_PHASE_COMPOSITION_PASS_NOT_READY',rows=rows,original_queries=0)
if __name__=='__main__':print(json.dumps(inspect()))
