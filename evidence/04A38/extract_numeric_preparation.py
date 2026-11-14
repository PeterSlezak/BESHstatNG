"""Checked one-shot extraction of original typed numerical UDF preparation.

No estimator is replaced. Original method bodies become Core authority and the
actual original Windows private helpers delegate with unchanged signatures.
"""
from datetime import datetime,timezone
import hashlib
import json
import re
import textwrap
from collections import defaultdict
from preflight import ROOT,HERE,BASE,GATES,git,pin,save
from preedit_addendum_reader import inspect

SELECTION={
    'ClassificationReportUDFs':{'ComputeWeightedEventRate'},
    'ContingencyTableUDFs':{'Is2x2','TableTotal','EffectiveDf','TransposeIntMatrix'},
    'MultivariateAnalysisUDFs':{'PercentOfTotal','Cumulative','ColumnSumsOfSquares'},
    'CoxUDFs':{'LookupStepValue'},
    'GEEUDFs':{'BuildGeeFittedDesignWithIntercept','CloneDoubleArray','NearlyEqual'},
    'GLMUDFs':{'ComputeBinaryEventRate'},
    'LmmUDFs':{'MatrixColumnCount'},
    'MixedModelUDFs':{'AddInterceptIfRequested'},
    'MultinomialLogitUDFs':{'GetCategoriesInModelOrder','ComputeMultinomialEtas','ComputeMultinomialProbabilities'},
    'OrdinalLogitUDFs':{'ComputeOrdinalProbabilities'},
    'UdfRegressionOutput':{'ComputeLinearPredictor'},
    'ZIPUDFs':{'ResponseColumnsMatch','CountZeros'},
    'UdfDataImport':{'HasOnlyFinite','TryGetNonnegativeIntegerResponse','RowsToMatrix','IsValidProbability','ClampProbabilityValue','CountDistinctOutcomeCategories'},
    'UdfLinearAlgebra':{'TryInvertMatrix'},
}


def index(text,utf16):
    return len(text.encode('utf-16-le')[:utf16*2].decode('utf-16-le'))


def argument_names(signature):
    start=signature.index('(');depth=0;quoted=False;parts=[];current=''
    for ch in signature[start+1:]:
        if ch=='"':quoted=not quoted
        if not quoted:
            if ch=='(':depth+=1
            if ch==')':
                if depth==0:
                    parts.append(current);break
                depth-=1
            if ch==',' and depth==0:
                parts.append(current);current='';continue
        current+=ch
    result=[]
    for part in parts:
        if not part.strip():continue
        match=re.match(r'\s*(?:(?:Optional|ByVal|ByRef|ParamArray)\s+)*(\[?\w+\]?)',part)
        assert match,part
        result.append(match.group(1))
    return result


def main():
    assert git('rev-parse','HEAD').decode().strip()==BASE
    assert git('branch','--show-current').decode().strip()=='codex/04A-38'
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    controls=inspect(ROOT)
    assert controls['total_rows_per_configuration']==28966
    addendum=HERE/'preedit/addendum-frozen-v1/receipt.json'
    assert hashlib.sha256(addendum.read_bytes()).hexdigest()=='35c790471841ce33fd2857ca881ff84103b2856cf61139fe1312bf002eb30646'
    for row in json.loads(addendum.read_bytes())['files']:assert pin(row['path'])==row
    data=json.loads((HERE/'entry-members.json').read_bytes())
    points={r['symbol_id']:r for r in json.loads((HERE/'preedit/attempts/reachability-v1/positions.json').read_bytes())}
    selected=[r for r in data['members'] if r['kind']=='FunctionBlock' and r['name'] in SELECTION.get(r['containing_type'].split('.')[-1],set())]
    assert len(selected)==sum(map(len,SELECTION.values()))
    target=ROOT/'BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb'
    assert not target.exists()
    replacements=defaultdict(list);core=[];mapping=[]
    for member in selected:
        path=member['path'];raw=(ROOT/path).read_bytes()
        assert hashlib.sha256(raw).hexdigest()==member['source_sha256']
        text=raw.decode('utf-8-sig');start=index(text,member['span_start']);end=index(text,member['span_start']+member['span_length'])
        body=index(text,points[member['symbol_id']]['insertion_utf16'])
        end_statement=text.rfind('End Function',start,end);assert end_statement>body
        original=text[start:end]
        column=start-(text.rfind('\n',0,start)+1)
        portable=textwrap.dedent(' '*column+original)
        portable=re.sub(r'^(?:Friend|Private|Public) Function ', 'Public Shared Function ',portable,count=1)
        assert portable.startswith('Public Shared Function ')
        copy_note=''
        if member['name']=='AddInterceptIfRequested':
            assert 'If Not includeIntercept Then Return x' in portable
            portable=portable.replace('If Not includeIntercept Then Return x','If Not includeIntercept Then Return DirectCast(x.Clone(), Double(,))',1)
            copy_note='Fresh copy on no-intercept branch, required to avoid sharing a returned portable array with its input; values/bounds/order retained. Old private reference identity is not an Excel output.'
        core.append("        ' Original source: "+path+' / '+member['name']+'\n'+textwrap.indent(portable.replace('\r\n','\n'),'        '))
        eol='\r\n' if '\r\n' in text else '\n'
        indent=' '*(body-(text.rfind('\n',0,body)+1))
        arguments=argument_names(member['signature'])
        statement='Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.'+member['name']+'('+', '.join(arguments)+')'+eol+' '*(end_statement-(text.rfind('\n',0,end_statement)+1))
        replacements[path].append((body,end_statement,statement))
        mapping.append(dict(original_member_id=member['symbol_id'],source_path=path,source_signature=member['signature'],source_sha256=member['source_sha256'],
            portable_owner='BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.'+member['name'],
            actual_windows_consumer=member['containing_type']+'.'+member['name'],transformation='Original typed body moved; original private Windows signature delegates in its original position.',
            compatibility_substitution=copy_note,statistical_engine_change=False))
    content='''Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Globalization

Namespace WorksheetCompatibility
    '''+"''' <summary>Original UDF numeric preparation, independent of Excel registration, cells and session caches.</summary>"+'''
    Public NotInheritable Class UdfNumericPreparation38
        Private Sub New()
        End Sub

'''+ '\n\n'.join(core)+'''
    End Class
End Namespace
'''
    assert not re.search(r'ExcelDna|ExcelError|ExcelReference|XlCall|System\.Drawing|Windows\.Forms|Microsoft\.VisualBasic',content)
    writes={}
    for path,edits in replacements.items():
        raw=(ROOT/path).read_bytes();text=raw.decode('utf-8-sig')
        for start,end,value in sorted(edits,reverse=True):text=text[:start]+value+text[end:]
        writes[path]=(b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8')
    save('implementation-entry.json',dict(status='AUTHORIZED_04A38_IMPLEMENTATION_START',utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,
        first_valid_freeze=pin('evidence/04A38/preedit/frozen-v2/receipt.json'),additional_freeze=pin('evidence/04A38/preedit/addendum-frozen-v1/receipt.json'),
        original_rows_per_configuration=28966,tracked_changes_before_first_edit=[],index_empty=True,
        initial_scope=dict(new_core_file=target.relative_to(ROOT).as_posix(),typed_methods=len(selected),windows_files=sorted(writes)),
        ownership_plan='Complete704-member source/CIL preparation plan exists;626 extraction/consumption reviews remain open until actual implementation and final verification.',
        gates=GATES,native='NOT_RUN',scientific='NOT_GOLDEN',next_batch='04A39_NOT_AUTHORIZED'))
    target.parent.mkdir(parents=True,exist_ok=True);target.write_text(content,encoding='utf-8',newline='\n')
    for path,value in writes.items():(ROOT/path).write_bytes(value)
    save('numeric-preparation-transition-v1.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=mapping,
        core=pin(target.relative_to(ROOT).as_posix()),windows=[pin(p) for p in sorted(writes)],
        note='UDF PercentOfTotal semantics deliberately remain distinct from04A37 graphics percentage guards; no graphics/Core estimator source changed.'))
    print('Extracted',len(selected),'real typed preparation methods into Core;',len(writes),'Windows helper files delegate; not acceptance-ready')


if __name__=='__main__':main()
