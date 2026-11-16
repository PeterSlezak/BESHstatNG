from common47 import *
import re,shutil
lock();folder=PRIVATE/'failed-source-a1';folder.mkdir()
for p in ['GuiWiring47ProjectionA.vb','GuiWiring47Fakes.vb','GuiWiring47StateTests.vb']:shutil.copy2(ROOT/'BESHStatNG.Test/src'/p,folder/p)
p=ROOT/'BESHStatNG.Test/src/GuiWiring47ProjectionA.vb';s=p.read_text().replace('Imports BESHStatNG.ExcelCommon','Imports BESHStatNG.regression\nImports BESHStatNG.ExcelCommon')
for owner,prefix in [('Ui18MMRM','MMRM'),('Ui19LMM','LMM')]:s=s.replace('Friend Class '+owner+'Projection47','Friend Class '+owner+'Projection47\n Friend WithEvents cb'+prefix+'CovOptimizerMode As New FakeControl47()\n Friend WithEvents cb'+prefix+'CovGradientMode As New FakeControl47()')
s=s.replace('TryRefreshMMRMContrastControlsFromCurrentSelections()', 'FakeAcquisition47.RefreshMMRM()')
p.write_text(s,encoding='utf-8',newline='\n')
d=load(HERE/'fixtures/projections-a-v1.json')
for m in d['members']:m['transforms'].append(['TryRefreshMMRMContrastControlsFromCurrentSelections()','FakeAcquisition47.RefreshMMRM()'])
d['projection']=pin(p);d['qualification']='MMRM pre-fit refresh is an explicitly throwing closed acquisition endpoint. Parent event Try/Catch unchanged;refresh body itself remains native pending. Actual Core FamilyUtils module import added;four fake fields added. Failed compile/source retained.'
save('fixtures/projections-a-v2.json',d)
p=ROOT/'BESHStatNG.Test/src/GuiWiring47Fakes.vb';s=p.read_text();start=s.index('  Friend Function ParseUiInteger');end=s.index(' End Module',start)
src=(ROOT/'BESHstatNG/src/DM/UIprocedures.vb').read_text(encoding='utf-8-sig');methods=[]
for name in ['TryParseUiInteger','ParseUiInteger']:
 m=re.search(r'(?ms)^    Public Function '+name+r'\(.*?^    End Function',src);assert m;methods.append(m.group())
replacement='\n'.join(methods).replace('TryParseUiDouble(', 'SourceNumbers46.TryParseUiDouble(')+'\n'
s=s[:start]+replacement+s[end:];s=s.replace('End Namespace',' Friend Module FakeAcquisition47\n  Friend Calls As Integer\n  Friend Sub RefreshMMRM()\n   Calls += 1\n   Throw New InvalidOperationException("HOST_PENDING_MMRM_REFRESH")\n  End Sub\n End Module\nEnd Namespace')
p.write_text(s,encoding='utf-8',newline='\n');save('fixtures/integer-source-v1.json',dict(source=pin('BESHstatNG/src/DM/UIprocedures.vb'),originals=methods,transforms=[['TryParseUiDouble(','SourceNumbers46.TryParseUiDouble(']],projection=pin(p)))
