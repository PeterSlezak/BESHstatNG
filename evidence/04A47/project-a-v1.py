from common47 import *
import re,xml.etree.ElementTree as ET
lock();census=load(HERE/'scope/family-census-v1.json')
chosen={
'UiGLM':['GetFamilyCodeFromDisplayName','RefreshLinkOptionsForSelectedFamily','IsCurrentBinomialGlmFamily','GetClassificationThresholdUiValue','UpdateClassificationOptionsState','UpdatePowerLinkState','cbFamily_SelectedIndexChanged','cbLink_SelectedIndexChanged','cbOutputCalibrationTable_CheckedChanged','cbPerformClasification_CheckedChanged'],
'Ui13GEE':['GetFamilyCodeFromDisplayName','RefreshLinkOptionsForSelectedFamily','IsCurrentBinomialGeeFamily','GetClassificationThresholdUiValue','UpdateClassificationOptionsState','UpdatePowerLinkState','cbFamily_SelectedIndexChanged','cbLink_SelectedIndexChanged','cbOutputCalibrationTable_CheckedChanged','cbPerformClasification_CheckedChanged'],
'Ui18MMRM':['UpdateMMRMContrastControlEnabledState','IsMMRMReferenceGridModeSelected','SelectedComboText','cbMMRMControlLevel_SelectedIndexChanged','cbMMRMContrastMode_SelectedIndexChanged','MMRMReferenceGridControlChanged','MMRMContrastControl_CheckedOrSelectedChanged','ParseMMRMCovarianceOptimizerMode','ParseMMRMCovarianceGradientMode','ApplyMMRMCovarianceOptimizerSelections'],
'Ui19LMM':['UpdateLMMCovarianceDependentControls','cbCovarStruct_SelectedIndexChanged','cbRandomCovarStruct_SelectedIndexChanged','SelectedComboText','ApplyLMMCovarianceOptimizerSelections'],
'Ui11PCA':['HierarchicalDistanceChanged','HierarchicalMembershipModeChanged','GetSelectedHierarchicalDistanceMetric','FactorAnalysisExtractionChanged','FactorAnalysisRotationChanged','FactorAnalysisRetentionChanged','GetSelectedFAExtractionMethod','GetSelectedFARotationMethod','GetSelectedFARetentionMethod','GetSelectedFARetentionValue','DiscriminantPriorModeChanged','DiscriminantValidationChanged','GetSelectedDiscriminantPriorMode','GetSelectedDiscriminantValidationMode','GetSelectedDiscriminantRandomSeed','GetSelectedDiscriminantRegularization','UpdateDiscriminantOptionStates']}
def block(src,name):
 m=re.search(r'(?ms)^    (?:Private|Public|Friend)(?: Shared)? (Sub|Function) '+re.escape(name)+r'(?:\(|\b).*?^    End \1',src);assert m,name
 return m.group()
save('scope/selection-a-v1.json',dict(label='CURRENT_MAIN_PRE_EDIT_04A47_BEFORE_NEW_TESTS',selection=chosen,baseline=pin(HERE/'scope/family-census-v1.json'),native='NOT_RUN'))
transforms=[['Windows.Forms.ComboBox','FakeControl47'],['CoreServices.Logger.','FakeLogger47.'],['ParseUiDouble(','SourceNumbers46.ParseUiDouble('],['FormatUiDouble(','SourceNumbers46.FormatUiDouble('],['ParseUiInteger(','FakeText47.ParseUiInteger(']]
code='''Option Strict On
Option Explicit On
Imports System
Imports System.Drawing
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Test.Harness46
Namespace Harness47
''';records=[];bindings=[]
for owner,names in chosen.items():
 path='BESHstatNG/src/UI/'+owner+'.vb';src=(ROOT/path).read_text(encoding='utf-8-sig');parts=[block(src,n) for n in names];full='\n'.join(parts)
 fields=set(re.findall(r'\bMe\.([A-Za-z_]\w*)\.',full));fields-=set(names);fields-={'Tag','Text'}
 bindings_here=[]
 for name,p in zip(names,parts):
  match=re.search(r'\bHandles\s+((?:\w+\.\w+\s*,?\s*)+)',p.split('\n\n',1)[0])
  if match:
   for control,event in re.findall(r'(\w+)\.(\w+)',match[1]):fields.add(control);bindings_here.append(dict(owner=owner,handler=name,control=control,event=event,kind='Handles'))
 extras='\n'.join(re.findall(r'^    Private Const (?:MMRM|LMM)_.*$',src,re.M))+'\n'
 dynamics=[]
 for line in src.splitlines():
  m=re.search(r'AddHandler (?:Me\.)?(\w+)\.(\w+), AddressOf (?:Me\.)?(\w+)',line)
  if m and m[3] in names:
   fields.add(m[1]);dynamics.append(line.strip());bindings_here.append(dict(owner=owner,handler=m[3],control=m[1],event=m[2],kind='AddHandler'))
 code+='Friend Class '+owner+'Projection47\n Friend Tag As HelpTopic\n'+extras
 for field in sorted(fields):code+=' Friend WithEvents '+field+' As New FakeControl47()\n'
 if dynamics:code+=' Friend Sub New()\n'+'\n'.join(dynamics)+'\n End Sub\n'
 for name,p in zip(names,parts):
  changed=p
  for a,b in transforms:changed=changed.replace(a,b)
  code+=changed+'\n';records.append(dict(owner=owner,method=name,source=pin(path),source_text_sha256=sha(p.encode()),original_text=p,transforms=transforms,fields=sorted(fields),bindings=[r for r in bindings_here if r['handler']==name]))
 bindings+=bindings_here;code+='End Class\n'
code+='End Namespace\n';target=ROOT/'BESHStatNG.Test/src/GuiWiring47ProjectionA.vb'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(code)
save('fixtures/projections-a-v1.json',dict(label='CURRENT_MAIN_PRE_EDIT_04A47_SOURCE_BACKED_FAKE',members=records,projection=pin(target),bindings=bindings,host_substitutions=transforms,native='NOT_RUN',limitations='Constructors are fake field allocation plus exact selected dynamic AddHandler lines. No original form initialization. Supplied state vectors only;no WinForms grouping/focus/COM.'))
root=ET.Element('bindings')
for r in bindings:ET.SubElement(root,'binding',r)
out=HERE/'fixtures/bindings-a-v1.xml'
with out.open('xb') as f:f.write(ET.tostring(root,encoding='utf-8',xml_declaration=True))
project=ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj';s=project.read_text(encoding='utf-8-sig');needle='    <Compile Include="src\\GuiWiring46Fakes.vb" />';assert needle in s
s=s.replace(needle,'\n'.join('    <Compile Include="src\\'+n+'" />' for n in ['GuiWiring47Fakes.vb','GuiWiring47ProjectionA.vb','GuiWiring47StateTests.vb'])+'\n'+needle)
project.write_text(s,encoding='utf-8',newline='\n')
print('Sealed',len(records),'shipping methods;',len(bindings),'actual copied bindings',flush=True)
