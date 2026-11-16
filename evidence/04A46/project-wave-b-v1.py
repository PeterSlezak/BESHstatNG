from common46 import *
import re
lock();census=load(HERE/'scope/family-census-v1.json');wavea=load(HERE/'fixtures/projections-wave-a-v1.json')
chosen={'Ui12SampleSizeTtestSingleProp':['btCompute_Click','CheckData','RunLogRank','RunCox','RunICC','RunBlandAltman','RunIndependentProp','RunSingleProp','RunPTtest','RunUPTtest','cbHypothesisType_SelectedIndexChanged','SelectedComboValue','RequiresKappaValue','RequiresCustom4Value','ParseOptionalUiDouble','SetDefaultIfBlank','AddUiError','TryValidateOptionalNumeric','ApplyUnpairedTLayout','ApplyIndependentProportionsLayout','ApplyCoxLayout'], 'Ui82x2':['btCompute_Click','GetResultWriter'],'Ui8Proportions':['btCompute_Click','GetResultWriter']}
replacements=[['MsgBox(','FakeMessages46.Record('],['AppGlobals.','FakeGlobals46.'],['CoreServices.Errors.LogAndThrow','FakeFaults46.Record'],['SampleSizePlanningPolicy.Calculate','FakePlanning46.Calculate']]
code='''Option Strict On
Option Explicit On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Imports Control = BESHStatNG.Test.Harness46.FakeControl46
Imports ExcelDnaResultWriter = BESHStatNG.Test.Harness46.FakeResultWriter46
Imports Range = BESHStatNG.Test.Harness46.FakeRange46
Imports ProcessListofResultTables = BESHStatNG.Test.Harness46.FakeTables46
Namespace Harness46
''';records=[]
for owner,names in chosen.items():
 path=next(r['path'] for r in census['members'] if r['owner']=='BESHStatNG.'+owner and not r['path'].endswith('.Designer.vb'));source=(ROOT/path).read_text(encoding='utf-8-sig');parts=[]
 for name in names:
  m=re.search(r'(?ms)^    Private (Sub|Function) '+re.escape(name)+r'\(.*?^    End \1',source);assert m,name;p=m.group();parts.append(p)
  records.append(dict(owner=owner,method=name,path=path,source=pin(path),source_text_sha256=sha(p.encode()),original_text=p,transforms=replacements+[['System.Windows.Forms.TextBox','FakeControl46']]))
 full='\n'.join(parts);existing={f for r in wavea['members'] if r['owner']==owner for f in r['field_names']};fields=set(re.findall(r'\bMe\.(\w+)\b',full))-existing-set(names)-{'Tag','Text','getDataMultipleGroups'}
 code+='Partial Friend Class '+owner+'Projection46\n'
 for f in sorted(fields):code+=' Friend WithEvents '+f+' As New '+('FakeRefEdit46' if f.lower().startswith('refedit') else 'FakeControl46')+'()\n'
 if owner=='Ui82x2':code+=' Private Function getDataMultipleGroups() As DataObj\n  FakeOutput46.Imports += 1\n  Return FakeOutput46.Imported\n End Function\n'
 for p in parts:
  for a,b in replacements:p=p.replace(a,b)
  p=p.replace('System.Windows.Forms.TextBox','FakeControl46');code+=p+'\n'
 code+='End Class\n'
code+='End Namespace\n';p=ROOT/'BESHStatNG.Test/src/GuiWiring46ExecutionProjections.vb'
with p.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
old=ROOT/'BESHStatNG.Test/src/GuiWiring46Projections.vb';text=old.read_text(encoding='utf-8')
for owner in chosen:text=text.replace('Friend Class '+owner+'Projection46','Partial Friend Class '+owner+'Projection46')
old.write_text(text,encoding='utf-8',newline='\n')
save('fixtures/projections-wave-b-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A46_SOURCE_DERIVED_FAKE',members=records,projection=pin(p),host_aliases='Fake in-memory workbook/writer/range descriptors and message/error sinks. Real unchanged ContingencyInput/Report policies execute;sample-size request policy is captured without numerical engine invocation. No source branch/body reordered.',native='NOT_RUN'))
print('Wave B adds',len(records),'original validation/dispatch/writer methods',flush=True)
