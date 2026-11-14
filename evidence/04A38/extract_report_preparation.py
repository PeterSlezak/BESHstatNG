"""Move typed report builders into immutable existing-model projections.

Only original typed statistical inputs enter Common. Native spill cells/errors
are materialized at the actual Windows boundary; no worksheet is touched.
"""
from collections import defaultdict
import json
import re
import sys
import textwrap
from preflight import ROOT,HERE,pin,save
from run import PRIVATE,m
from extract_numeric_preparation import index,argument_names

REPORTS={
    'AssumptionsUDFs':{'BuildGrubbsTable','BuildRosnerTable'},
    'MultivariateAnalysisUDFs':{'BuildClusterPreprocessingTable','BuildRemovedRowsOutput','BuildHierarchicalLeafOrderTable',
        'BuildCaEigenOutput','BuildCaOverviewOutput','BuildCaAxisMetricOutput','BuildMcaCategoryOverviewOutput','BuildMcaCategoryAxisMetricOutput',
        'BuildMcaBurtOutput','BuildMcaIndicatorOutput','BuildDiscriminantGroupSummaryTable','BuildDiscriminantMeansTable'},
    'GEEUDFs':{'BuildLabeledMatrixOutput'},
    'SampleSizeUDFs':{'MakeMetricValueTable','MakeTwoGroupTable','MakeEquivalenceTwoGroupTable','MakeIndependentProportionsTable',
        'MakeEquivalenceIndependentProportionsTable','MakeLogRankPlanningTable','MakeCoxPlanningTable','MakeIccPlanningTable','MakeBlandAltmanPlanningTable'},
    'UdfRegressionOutput':{'BuildResidualVectorOutput','BuildResidualMatrixOutput'},
    'UdfOutputTables':{'BuildNamedMatrixOutput','BuildCaseMatrixOutput','BuildSimpleNoteTable'},
}
AUXILIARY={
    'MultivariateAnalysisUDFs':{'ResolveAvailableAxisCount','CombinedCategoryHeaders'},
    'UdfRegressionOutput':{'CategoryHeaders'},
    'UdfOutputTables':{'SafeCiLabel','SafeCiText'},
}


def main():
    label,version=sys.argv[1:3];runner=m.Runner(PRIVATE)
    private=PRIVATE/('report-extraction-'+version);private.mkdir(exist_ok=False)
    folder=HERE/'development'/('report-extraction-'+version);folder.mkdir(parents=True,exist_ok=False)
    inventory=folder/'current-members.json';positions=folder/'current-positions.json'
    result=runner.command('report-inventory-'+version,['dotnet',str(PRIVATE/'inventory-v1/bin/Inventory38.dll'),str(inventory)]);assert result['exit_code']==0
    result=runner.command('report-positions-'+version,['dotnet',str(PRIVATE/'trace-points-v1/bin/TracePoints38.dll'),str(inventory),str(positions)]);assert result['exit_code']==0
    current=json.loads(inventory.read_bytes());points={p['symbol_id']:p for p in json.loads(positions.read_bytes())}
    selected=[r for r in current['members'] if r['kind']=='FunctionBlock' and r['name'] in REPORTS.get(r['containing_type'].split('.')[-1],set())|AUXILIARY.get(r['containing_type'].split('.')[-1],set())]
    assert len(selected)==sum(map(len,REPORTS.values()))+sum(map(len,AUXILIARY.values()))
    edits=defaultdict(list);groups=defaultdict(list);mapping=[]
    for member in selected:
        path=member['path'];data=(ROOT/path).read_bytes();text=data.decode('utf-8-sig')
        begin=index(text,member['span_start']);end=index(text,member['span_start']+member['span_length']);body=index(text,points[member['symbol_id']]['insertion_utf16'])
        finish=text.rfind('End Function',begin,end);assert finish>body
        code=textwrap.dedent(' '*(begin-text.rfind('\n',0,begin)-1)+text[begin:end]).replace('\r\n','\n')
        code=re.sub(r'^(?:Private|Friend|Public) Function ', 'Public Shared Function ',code,count=1)
        owner=member['containing_type'].split('.')[-1];is_report=member['name'] in REPORTS.get(owner,set())
        if is_report:
            header_end=code.index('\n',code.index(' As Object')) if '\n' in code[code.index(' As Object'):] else len(code)
            # The return type is the last As Object declaration before the body,
            # not an Object used in an array declaration inside the report.
            prefix=code[:body-begin]
            candidates=list(re.finditer(r'\) As Object(?:\(,\))?',code))
            assert candidates,member['signature']
            match=candidates[0]
            code=code[:match.start()]+') As UdfReportProjection38'+code[match.end():]
        assert not re.search(r'\bAs Object(?:\s*[,)]|\s*=)',member['signature']),member['signature']
        target_class=owner+'Reports38';groups[target_class].append(textwrap.indent(code,'        '))
        args=argument_names(member['signature'])
        call='Global.BESHStatNG.ExcelCommon.'+target_class+'.'+member['name']+'('+', '.join(args)+')'
        if is_report:call='MaterializePortableReport38('+call+')'
        eol='\r\n' if '\r\n' in text else '\n';suffix=' '*(finish-text.rfind('\n',0,finish)-1)
        # An As Object(,) original signature needs an explicit array cast under
        # Option Strict; scalar/error-capable originals retain As Object.
        if is_report and member['signature'].endswith('As Object(,)'):call='DirectCast('+call+', Object(,))'
        edits[path].append((body,finish,'Return '+call+eol+suffix))
        mapping.append(dict(original_signature_key=member['signature_key'],source_member_id=member['symbol_id'],path=path,signature=member['signature'],
            common_owner='BESHStatNG.ExcelCommon.'+target_class+'.'+member['name'],role='TYPED_REPORT' if is_report else 'TYPED_REPORT_METADATA',
            native_consumer=member['containing_type']+'.'+member['name']))
    source='''Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.SampleSizeCalc
Imports BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38
Imports BESHStatNG.ExcelCommon.UdfOutputTablesReports38
Imports BESHStatNG.ExcelCommon.UdfRegressionOutputReports38

Namespace ExcelCommon
'''
    for name,methods in groups.items():source+='    Public NotInheritable Class '+name+'\n        Private Sub New()\n        End Sub\n\n'+'\n\n'.join(methods)+'\n    End Class\n'
    source+='End Namespace\n';source=source.replace('regression.','Global.BESHStatNG.regression.')
    raw=folder/'reports.raw.vb.txt';raw.write_text(source,encoding='utf-8',newline='\n')
    tool=private/'tool';tool.mkdir()
    result=runner.command('report-tool-'+version,['dotnet','msbuild',str(HERE/'helpers/ReportRewrite38.csproj'),'/t:Build','/p:ProjectAssetsFile='+str(ROOT/'evidence/04A37/helpers/obj/project.assets.json'),'/p:IntermediateOutputPath='+str(tool/'obj')+'/', '/p:OutputPath='+str(tool/'bin')+'/']);assert result['exit_code']==0
    rewritten=folder/'reports.rewritten.vb.txt'
    result=runner.command('report-rewrite-'+version,['dotnet',str(tool/'bin/ReportRewrite38.dll'),str(raw),str(rewritten),str(PRIVATE/'builds'/label/'Debug/legacy/BESHStat.Core.dll')]);assert result['exit_code']==0
    value=rewritten.read_text(encoding='utf-8')
    assert not any(word in value for word in ['ExcelDna','ExcelError','ExcelReference','XlCall','System.Drawing','Windows.Forms','Microsoft.VisualBasic'])
    target=ROOT/'BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb';assert not target.exists()
    target.write_text(value,encoding='utf-8',newline='\n')
    for path,changes in edits.items():
        data=(ROOT/path).read_bytes();text=data.decode('utf-8-sig')
        for begin,end,value in sorted(changes,reverse=True):text=text[:begin]+value+text[end:]
        (ROOT/path).write_bytes((b'\xef\xbb\xbf' if data.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8'))
    save('report-preparation-transition-'+version+'.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=mapping,
        common=pin(target.relative_to(ROOT).as_posix()),windows=[pin(path) for path in sorted(edits)],
        policy='Existing Core ResultTableOutputModel report data, copied defensively; native fault/blank/p-value cell marshaling remains Windows. No raw native input object is passed into Common builders.'))
    print('Extracted',len(selected),'typed report/metadata helpers;',len(edits),'Windows files consume Common')


if __name__=='__main__':main()
