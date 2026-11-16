from common47 import *
import re,xml.etree.ElementTree as ET,shutil
lock();chosen={
'UiGLM':['BeginRegressionComputation','EndRegressionComputation','IsRegressionCancellationRequested','IsRegressionInterruptionRequested','FinishRegressionComputation','btInterrupt_Click'],
'Ui13GEE':['BeginRegressionComputation','EndRegressionComputation','IsRegressionCancellationRequested','IsRegressionInterruptionRequested','FinishRegressionComputation','btInterrupt_Click'],
'Ui4Cox':['BeginRegressionComputation','EndRegressionComputation','IsRegressionCancellationRequested','IsRegressionInterruptionRequested','FinishRegressionComputation','btInterrupt_Click'],
'Ui18MMRM':['btInterrupt_Click','IsMMRMCancellationRequested','IsMMRMInterruptionRequested'],
'Ui19LMM':['btInterrupt_Click'],
'Ui20PropensityScoreMatching':['BuildFitOptions','ParseEnum','ParseOptionalDouble','PopulateEnumCombo','InitializeDefaultUiValues','SetOptionControlsForCurrentSelections','RefreshEstimandChoicesForRunMethod','OptionComboChanged','BeginPsmComputation','FinishPsmComputation','EndPsmComputation'],
'Ui21ControlCharts':['rbPhase_CheckedChanged','UpdateQuickPhaseControls','SetRoleEnabled','IsAttributeChart','IsSubgroupChart','IsTimeWeightedChart','ChartCanUseSpecificationLines'],
'Ui22MultivariateControlCharts':['rbPca_CheckedChanged','UpdatePcaSelectionControls','chkWriteDiagnostics_CheckedChanged','UpdateDiagnosticsControls','SetRoleEnabled'],
'Ui4KMandLogRank':['checkInputs','optOutputRange_Click','optWorksheet_Click','optWorkbook_Click']}
def block(src,name):
 m=re.search(r'(?ms)^    (?:Private|Public|Friend)(?: Shared)? (Sub|Function) '+re.escape(name)+r'(?:\(|\b).*?^    End \1',src);assert m,name
 return m.group()
save('scope/selection-b-v1.json',dict(label='CURRENT_MAIN_PRE_EDIT_04A47_SOURCE_BACKED_WAVE_B',selection=chosen,native='NOT_RUN'))
transforms=[['Windows.Forms.Application.DoEvents()', 'Me.Pump.DoEvents()'],['CoreServices.Regression.', 'Me.Bridge.'],['Windows.Forms.ProgressBarStyle.', 'FakeProgressStyle47.'],['ProgressBarStyle.', 'FakeProgressStyle47.'],['As ComboBox','As FakeControl47'],['As ListBox','As FakeControl47'],['As Label','As FakeControl47'],['As Button','As FakeControl47'],['ComboBoxStyle.DropDownList','0'],['ParseUiDouble(','SourceNumbers46.ParseUiDouble('],['ParseUiInteger(','FakeText47.ParseUiInteger('],['FormatUiDouble(','SourceNumbers46.FormatUiDouble('],['CheckRefEdit(', 'FakeRanges46.CheckRefEdit('],['RefEditReset(', 'FakeRanges46.RefEditReset(']]
# Match existing helper names exactly; fake range boundary is already accepted46.
fake46=(ROOT/'BESHStatNG.Test/src/GuiWiring46Fakes.vb').read_text();assert 'Function CheckRefEdit' in fake46
code='''Option Strict On
Option Explicit On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.CausalInference
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG.Test.Harness46
Namespace Harness47
''';records=[];bindings=[]
ap=ROOT/'BESHStatNG.Test/src/GuiWiring47ProjectionA.vb';a=ap.read_text();(PRIVATE/'source-a-before-b.vb').write_text(a,encoding='utf-8');a=a.replace('Friend Class ','Friend Partial Class ');ap.write_text(a,encoding='utf-8',newline='\n')
for owner,names in chosen.items():
 path='BESHstatNG/src/UI/'+owner+'.vb';src=(ROOT/path).read_text(encoding='utf-8-sig');parts=[block(src,n) for n in names];full='\n'.join(parts)
 existing=re.search(r'(?ms)Friend Partial Class '+owner+r'Projection47\b.*?^End Class',a);oldfields=set(re.findall(r'Friend WithEvents (\w+)',existing.group() if existing else ''))
 fields=set(re.findall(r'\bMe\.([A-Za-z_]\w*)',full))
 # Unqualified control fields are declaration-backed,not lexical guesses alone.
 original_fields=set(re.findall(r'Friend WithEvents (\w+)',(ROOT/('BESHstatNG/src/UI/'+owner+'.Designer.vb')).read_text(encoding='utf-8-sig')))
 fields|={n for n in original_fields if re.search(r'\b'+re.escape(n)+r'\b',full)}
 fields-=set(names)|oldfields|{'Close','Tag','Text'};here=[];dynamics=[]
 for name,p in zip(names,parts):
  match=re.search(r'\bHandles\s+((?:\w+\.\w+\s*,?\s*)+)',p.split('\n\n',1)[0])
  if match:
   for control,event in re.findall(r'(\w+)\.(\w+)',match[1]):fields.add(control) if control not in oldfields else None;here.append(dict(owner=owner,handler=name,control=control,event=event,kind='Handles'))
 for line in src.splitlines():
  m=re.search(r'AddHandler (?:Me\.)?(\w+)\.(\w+), AddressOf (?:Me\.)?(\w+)',line)
  if m and m[3] in names:dynamics.append(line.strip());fields.add(m[1]);here.append(dict(owner=owner,handler=m[3],control=m[1],event=m[2],kind='AddHandler'))
 code+='Friend Partial Class '+owner+'Projection47\n'
 if not existing:code+=' Friend Tag As HelpTopic\n'
 code+=' Friend ReadOnly Pump As New FakePump47()\n Friend ReadOnly Bridge As New FakeRegressionBridge47()\n Friend Closes As Integer\n Friend Sub Close()\n Closes += 1\n End Sub\n'
 bools=sorted(set(re.findall(r'\bp(?:Regression|Mmrm|Lmm|lmm)\w+',full)))
 for name in bools:code+=' Friend '+name+' As Boolean\n'
 for field in sorted(fields):
  t='FakeRefEdit46' if field.startswith('RefEdit') else 'FakeControl47'
  code+=' Friend WithEvents '+field+' As New '+t+'()\n'
 if dynamics:code+=' Friend Sub New()\n'+'\n'.join(dynamics)+'\n End Sub\n'
 for name,p in zip(names,parts):
  changed=p
  for x,y in transforms:changed=changed.replace(x,y)
  changed=changed.replace('FakeFakeProgressStyle47.','FakeProgressStyle47.')
  code+=changed+'\n';records.append(dict(owner=owner,method=name,source=pin(path),original_text=p,source_text_sha256=sha(p.encode()),transforms=transforms+[['FakeFakeProgressStyle47.','FakeProgressStyle47.']],fields=sorted(fields),bindings=[b for b in here if b['handler']==name]))
 bindings+=here;code+='End Class\n'
code+='End Namespace\n';target=ROOT/'BESHStatNG.Test/src/GuiWiring47ProjectionB.vb';target.write_text(code,encoding='utf-8',newline='\n')
save('fixtures/projections-b-v1.json',dict(label='CURRENT_MAIN_PRE_EDIT_04A47_SOURCE_BACKED_FAKE',members=records,bindings=bindings,projection=pin(target),native='NOT_RUN',boundaries='DoEvents replaced by closed counter/throw sink;regression callback store per fake instance;Close only counter. No shipping fit/import/writer/renderer touched.'))
root=ET.Element('bindings')
for r in bindings:ET.SubElement(root,'binding',r)
with (HERE/'fixtures/bindings-b-v1.xml').open('xb') as f:f.write(ET.tostring(root,encoding='utf-8',xml_declaration=True))
project=ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj';s=project.read_text(encoding='utf-8-sig');needle='    <Compile Include="src\\GuiWiring47Fakes.vb" />';assert needle in s
s=s.replace(needle,'\n'.join('    <Compile Include="src\\'+n+'" />' for n in ['GuiWiring47ProjectionB.vb','GuiWiring47LongrunTests.vb','GuiWiring47ContractTests.vb'])+'\n'+needle);project.write_text(s,encoding='utf-8',newline='\n')
print('WaveB sealed',len(records),'source bodies;',len(bindings),'binding occurrences',flush=True)
