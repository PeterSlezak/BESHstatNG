"""Move typed UDF labels/formula diagnostics, retaining distinct legacy policies."""
from collections import defaultdict
import json,re,textwrap
from preflight import ROOT,HERE,save,pin
from run import PRIVATE,m
from extract_numeric_preparation import index,argument_names

SELECT={
 'CoxUDFs':{'ResidualLabel'},
 'GEEUDFs':{'GeeFindHeaderIndex','GeeFindDesignColumnIndex','GeeNormalizeDesignColumnName','BuildGeeTimeLabels','BuildParameterNames'},
 'LmmUDFs':{'NormalizeNameList','DefaultRandomCovarianceName'},
 'MixedModelUDFs':{'DefaultNames','NormalizeMmrmNameList','AddInterceptNameIfRequested','FindDesignColumnIndex'},
 'RegressionFormulaUDFs':{'BuildValidationFailureMessage','TryLocateOffendingFragment','ExtractBalancedFunctionCall','BuildAvailableReferenceSummary','FormatSampleList'},
 'ZIPUDFs':{'BuildParameterNames'},
 'UdfDataImport':{'SequenceEqualStrings','NormalizeMmrmNameList','DefaultMmrmNames','MmrmFindHeaderIndex','MmrmFindDesignColumnIndex','MmrmNormalizeDesignColumnName'},
 'UdfScalarUtilities':{'CloneStringArray'},
}

def main():
 folder=HERE/'development/metadata-extraction-v1';folder.mkdir(parents=True,exist_ok=False)
 runner=m.Runner(PRIVATE);inventory=folder/'current-members.json';positions=folder/'current-positions.json'
 assert runner.command('metadata-inventory-v1',['dotnet',str(PRIVATE/'inventory-v1/bin/Inventory38.dll'),str(inventory)])['exit_code']==0
 assert runner.command('metadata-positions-v1',['dotnet',str(PRIVATE/'trace-points-v1/bin/TracePoints38.dll'),str(inventory),str(positions)])['exit_code']==0
 current=json.loads(inventory.read_bytes());points={p['symbol_id']:p for p in json.loads(positions.read_bytes())}
 selected=[r for r in current['members'] if r['kind']=='FunctionBlock' and r['name'] in SELECT.get(r['containing_type'].split('.')[-1],set())]
 assert len(selected)==sum(map(len,SELECT.values()))
 groups=defaultdict(list);edits=defaultdict(list);mapping=[]
 for member in selected:
  data=(ROOT/member['path']).read_bytes();t=data.decode('utf-8-sig')
  begin=index(t,member['span_start']);end=index(t,member['span_start']+member['span_length']);body=index(t,points[member['symbol_id']]['insertion_utf16']);finish=t.rfind('End Function',begin,end)
  code=textwrap.dedent(' '*(begin-t.rfind('\n',0,begin)-1)+t[begin:end]).replace('\r\n','\n')
  code=re.sub(r'^(?:Private|Friend) Function ','Public Shared Function ',code,count=1)
  assert 'As Object' not in member['signature']
  owner=member['containing_type'].split('.')[-1];cls=owner+'Metadata38'
  if owner=='MixedModelUDFs':code=code.replace('NormalizeDesignColumnName(', 'MixedModelUDFsOptions38.NormalizeDesignColumnName(')
  code=code.replace('UBound(out)','(out.Length - 1)')
  groups[cls].append(textwrap.indent(code,'        '))
  args=argument_names(member['signature']);call='Return Global.BESHStatNG.ExcelCommon.'+cls+'.'+member['name']+'('+', '.join(args)+')'
  eol='\r\n' if '\r\n' in t else '\n';suffix=' '*(finish-t.rfind('\n',0,finish)-1)
  edits[member['path']].append((body,finish,call+eol+suffix))
  mapping.append(dict(source_member_id=member['symbol_id'],signature=member['signature'],path=member['path'],portable=cls+'.'+member['name']))
 source='''Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports BESHStatNG

Namespace ExcelCommon
'''
 for cls,methods in groups.items():source+='    Public NotInheritable Class '+cls+'\n        Private Sub New()\n        End Sub\n\n'+'\n\n'.join(methods)+'\n    End Class\n'
 source+='End Namespace\n'
 raw=folder/'metadata.raw.vb.txt';raw.write_text(source,encoding='utf-8',newline='\n');rewritten=folder/'metadata.rewritten.vb.txt'
 tool=PRIVATE/'text-rewrite-v2/bin/PortableTextRewrite38.dll';assert tool.exists()
 assert runner.command('metadata-text-rewrite-v1',['dotnet',str(tool),str(raw),str(rewritten),str(PRIVATE/'builds/output-v1/Debug/legacy/BESHStat.Core.dll')])['exit_code']==0
 target=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfMetadataPreparation38.vb';assert not target.exists();target.write_bytes(rewritten.read_bytes())
 for path,changes in edits.items():
  data=(ROOT/path).read_bytes();t=data.decode('utf-8-sig')
  for start,end,replacement in sorted(changes,reverse=True):t=t[:start]+replacement+t[end:]
  (ROOT/path).write_bytes((b'\xef\xbb\xbf' if data.startswith(b'\xef\xbb\xbf') else b'')+t.encode('utf-8'))
 save('metadata-preparation-transition-v1.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=mapping,common=pin(target.relative_to(ROOT).as_posix()),windows=[pin(path) for path in sorted(edits)]))
 print('Extracted',len(selected),'typed label/formula metadata helpers')

if __name__=='__main__':main()
