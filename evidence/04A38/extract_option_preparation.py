"""Extract original normalized String options with explicit legacy comparison.

Excel object/sentinel/numeric coercion stays at the original Windows boundary.
This stage excludes numeric and sentinel-sensitive policies for separate typed
preparation; it does not classify them complete merely because names moved.
"""
from collections import defaultdict
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import textwrap
from preflight import ROOT,HERE,pin,save
from run import PRIVATE,m
from extract_numeric_preparation import index,argument_names

EXCLUDED={'NormalizeFitOptions','ParseEnumValue','ParseEstimand','NormalizeNameList','NormalizeMmrmNameList',
    'TryParseOptionalFiniteDouble','ParseAlphaOrDefault','ParseOptionalNullableDouble','ParseOptionalSeed','TryParseAlpha',
    'ParseKwType','ParseFriedmanType','ParseSurvivalWeightMethod'}
EXTRA={'GetCoxTieMethod','GetRegressionFamilyCode','GetRegressionLinkName','GetFormulaAddressingMode','GetHistogramBinRule',
    'ResolveSampleSizeHypothesisType','TryParseLeveneCenter','TryParseSymmetryMethod','ApplyMixedModelOptimizerShortcutToGradient'}

TEXT_HELPER='''Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    '''+"''' <summary>Explicit historical Windows Option Compare Text semantics for normalized UDF options.</summary>"+'''
    Public NotInheritable Class LegacyUdfText38
        Private Sub New()
        End Sub

        Public Shared Function EqualText(left As String, right As String) As Boolean
            Dim options As CompareOptions = CompareOptions.IgnoreCase Or CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreWidth
            Return CultureInfo.CurrentCulture.CompareInfo.Compare(If(left, String.Empty), If(right, String.Empty), options) = 0
        End Function

        Public Shared Function CaseToken(value As String, choices As String()) As String
            For Each choice As String In choices
                If EqualText(value, choice) Then Return choice
            Next
            Return value
        End Function
    End Class
End Namespace
'''


def main():
    label,version=sys.argv[1:3]
    folder=PRIVATE/('option-extraction-'+version);folder.mkdir(exist_ok=False)
    receipt_dir=HERE/'development'/('option-extraction-'+version);receipt_dir.mkdir(parents=True,exist_ok=False)
    runner=m.Runner(PRIVATE)
    inventory=receipt_dir/'current-members.json';positions=receipt_dir/'current-positions.json'
    result=runner.command('option-current-inventory-'+version,['dotnet',str(PRIVATE/'inventory-v1/bin/Inventory38.dll'),str(inventory)]);assert result['exit_code']==0
    result=runner.command('option-current-positions-'+version,['dotnet',str(PRIVATE/'trace-points-v1/bin/TracePoints38.dll'),str(inventory),str(positions)]);assert result['exit_code']==0
    current=json.loads(inventory.read_bytes());old=json.loads((HERE/'entry-members.json').read_bytes())
    lookup={r['signature_key']:r for r in current['members'] if r['kind'] in ['FunctionBlock','SubBlock']}
    points={r['symbol_id']:r for r in json.loads(positions.read_bytes())}
    selected=[r for r in old['members'] if r['kind'] in ['FunctionBlock','SubBlock'] and not r['exported'] and
        (r['name'].startswith(('Parse','Normalize','TryParse')) or r['name'] in EXTRA) and r['name'] not in EXCLUDED]
    groups=defaultdict(list);edits=defaultdict(list);mapping=[]
    class_names={r['containing_type']:r['containing_type'].split('.')[-1]+'Options38' for r in selected}
    for member in selected:
        live=lookup[member['signature_key']];path=member['path'];data=(ROOT/path).read_bytes();text=data.decode('utf-8-sig')
        start=index(text,live['span_start']);end=index(text,live['span_start']+live['span_length']);body=index(text,points[live['symbol_id']]['insertion_utf16'])
        terminator='End Sub' if live['kind']=='SubBlock' else 'End Function'
        finish=text.rfind(terminator,start,end);assert finish>body
        original=text[start:end];column=start-text.rfind('\n',0,start)-1
        portable=textwrap.dedent(' '*column+original).replace('\r\n','\n')
        portable=re.sub(r'^(?:Friend|Private|Public) (Function|Sub) ',r'Public Shared \1 ',portable,count=1)
        names=argument_names(member['signature']);assert names
        first=names[0];native_object=bool(re.search(r'\b'+re.escape(first)+r'\s+As\s+Object',member['signature']))
        if native_object:
            portable=re.sub(r'\b'+re.escape(first)+r'\s+As\s+Object',first+' As String',portable,count=1)
            # These selected methods only use the original string reader. Native
            # numeric and sentinel-sensitive paths were deliberately excluded.
            portable=re.sub(r'(?:(?:Global\.)?BESHStatNG\.WorksheetFunctions\.)?ExcelArgReaders\.AsString\(\s*'+re.escape(first)+r'\s*\)',first,portable)
            portable=re.sub(r'\bAsString\(\s*'+re.escape(first)+r'\s*\)',first,portable)
        assert not any(word in portable for word in ['ExcelArg','ExcelError','ExcelEmpty','ExcelMissing','IsMissingArg','TryGetDouble',' As Object']),member['signature']
        for owner,target_class in class_names.items():
            portable=portable.replace('Global.'+owner+'.',target_class+'.').replace(owner+'.',target_class+'.')
        owner=member['containing_type'];target_class=class_names[owner]
        groups[target_class].append(textwrap.indent(portable,'        '))
        call_args=list(names)
        if native_object:call_args[0]='Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+first+')'
        eol='\r\n' if '\r\n' in text else '\n'
        suffix=' '* (finish-text.rfind('\n',0,finish)-1)
        statement=('' if live['kind']=='SubBlock' else 'Return ')+'Global.BESHStatNG.ExcelCommon.'+target_class+'.'+member['name']+'('+', '.join(call_args)+')'+eol+suffix
        edits[path].append((body,finish,statement))
        mapping.append(dict(original_member_id=member['symbol_id'],current_member_id=live['symbol_id'],source_path=path,signature=member['signature'],
            portable_owner='BESHStatNG.ExcelCommon.'+target_class+'.'+member['name'],native_string_boundary=native_object,
            consuming_windows_helper=owner+'.'+member['name']))
    imports='''Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.regression
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38

Namespace ExcelCommon
'''
    classes=[]
    for name,methods in groups.items():
        constants=''
        if name=='SampleSizeUDFsOptions38':
            constants='''        Private Const HypothesisSuperiority As String = "superiority"
        Private Const HypothesisNonInferiority As String = "noninferiority"
        Private Const HypothesisEquivalence As String = "equivalence"
'''
        classes.append('    Public NotInheritable Class '+name+'\n        Private Sub New()\n        End Sub\n'+constants+'\n'+'\n\n'.join(methods)+'\n    End Class\n')
    raw=receipt_dir/'UdfOptionPreparation38.raw.vb.txt';raw.write_text(imports+'\n'.join(classes)+'End Namespace\n',encoding='utf-8',newline='\n')
    tool=PRIVATE/('text-rewrite-'+version);tool.mkdir(exist_ok=False)
    result=runner.command('option-text-tool-build-'+version,['dotnet','msbuild',str(HERE/'helpers/PortableTextRewrite38.csproj'),'/t:Build',
        '/p:ProjectAssetsFile='+str(ROOT/'evidence/04A37/helpers/obj/project.assets.json'),'/p:IntermediateOutputPath='+str(tool/'obj')+'/', '/p:OutputPath='+str(tool/'bin')+'/']);assert result['exit_code']==0
    rewritten=receipt_dir/'UdfOptionPreparation38.rewritten.vb.txt'
    result=runner.command('option-text-rewrite-'+version,['dotnet',str(tool/'bin/PortableTextRewrite38.dll'),str(raw),str(rewritten),str(PRIVATE/'builds'/label/'Debug/legacy/BESHStat.Core.dll')]);assert result['exit_code']==0
    text=rewritten.read_text(encoding='utf-8')
    assert not re.search(r'ExcelDna|ExcelError|ExcelReference|XlCall|System\.Drawing|Windows\.Forms|Microsoft\.VisualBasic',text)
    target=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb';helper=ROOT/'BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb'
    assert not target.exists() and not helper.exists()
    target.write_text(text,encoding='utf-8',newline='\n');helper.write_text(TEXT_HELPER,encoding='utf-8',newline='\n')
    for path,changes in edits.items():
        data=(ROOT/path).read_bytes();text=data.decode('utf-8-sig')
        for begin,end,value in sorted(changes,reverse=True):text=text[:begin]+value+text[end:]
        (ROOT/path).write_bytes((b'\xef\xbb\xbf' if data.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8'))
    save('option-preparation-transition-'+version+'.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=mapping,
        common=[pin(target.relative_to(ROOT).as_posix()),pin(helper.relative_to(ROOT).as_posix())],
        windows=[pin(path) for path in sorted(edits)],string_comparison_intent='Explicit CurrentCulture IgnoreCase/IgnoreKanaType/IgnoreWidth matching, preserving Windows Option Compare Text behavior; no culture normalization.'))
    print('Extracted',len(selected),'normalized String option helpers;',len(edits),'Windows files consume Common; no acceptance claim')


if __name__=='__main__':main()
