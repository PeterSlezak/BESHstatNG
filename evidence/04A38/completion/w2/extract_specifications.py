"""Move family-specific spec preparation and saved-row selection, no new oracle."""
from extract_profiles import *

def main():
 g=read(GEE);m=read(MMRM);i=read(IMPORT);common=read(COMMON)
 with (HERE/'attempt-profile-common-v2.vb.txt').open('x',encoding='utf-8') as f:f.write(common)
 methods=[]
 host='Global.BESHStatNG.WorksheetFunctions.'
 for family,source,meta in [('Gee',g,'GEEUDFsMetadata38'),('Mmrm',i,'UdfDataImportMetadata38')]:
  old=member(source,'TryGet'+family+'LsmEstimateSpec');start=old.index('        Dim rows As Integer') if family=='Mmrm' else old.index('            Dim rows As Integer')
  b=reindent(old[start:old.rindex('\n')]);b=b.replace(host+'MixedModelUDFs.MmrmLsmEstimateComponent','TComponent').replace(host+'MixedModelUDFs.MmrmLsmEstimateProfileValue','TValue')
  b=b.replace('New GeeLsmEstimateComponent','New TComponent').replace('New GeeLsmEstimateProfileValue','New TValue')
  b=b.replace(host+'ExcelArgPredicates.IsBlankCell','isBlank').replace('ExcelArgPredicates.IsBlankCell','isBlank')
  b=b.replace(host+'ExcelArgReaders.CellToTrimmedText','cellText').replace('ExcelArgReaders.CellToTrimmedText','cellText')
  b=b.replace(host+'ExcelArgNumeric.TryGetFiniteDouble','tryFinite').replace('ExcelArgNumeric.TryGetFiniteDouble','tryFinite')
  for name in ['FindHeaderIndex','FindDesignColumnIndex']:b=b.replace(family+name+'(',meta+'.'+family+name+'(')
  model='parameterNames As String()' if family=='Gee' else 'names As Func(Of String())'
  if family=='Mmrm':b=b.replace('h.FixedEffectNames','names()')
  methods.append(('''        Public Shared Function TryGetFAMILYSpec(Of TComponent As {UdfFAMILYComponent38(Of TValue), New}, TValue As {UdfLsmProfileValue38, New})(
            arr As Object(,), MODEL, isBlank As Func(Of Object, Boolean), cellText As Func(Of Object, String), tryFinite As FiniteDoubleReader,
            ByRef components As List(Of TComponent), ByRef errorMessage As String) As Boolean
'''.replace('FAMILY',family).replace('MODEL',model))+b+'\n        End Function')
  prefix=old[:start];indent='        ' if family=='Mmrm' else '            '
  types=host+'MixedModelUDFs.MmrmLsmEstimateComponent, '+host+'MixedModelUDFs.MmrmLsmEstimateProfileValue' if family=='Mmrm' else 'GeeLsmEstimateComponent, GeeLsmEstimateProfileValue'
  prefix+=indent+'Return Global.BESHStatNG.ExcelCommon.UdfModelProfilePreparation38.TryGet'+family+'Spec(Of '+types+')(\n'+indent+'    arr, '+('Function() h.FixedEffectNames' if family=='Mmrm' else 'parameterNames')+', AddressOf '+host+'ExcelArgPredicates.IsBlankCell,\n'+indent+'    AddressOf '+host+'ExcelArgReaders.CellToTrimmedText, AddressOf '+host+'ExcelArgNumeric.TryGetFiniteDouble,\n'+indent+'    components, errorMessage)\n'+indent[:-4]+'End Function'
  source=source.replace(old,prefix)
  if family=='Mmrm':i=source
  else:g=source
 classes=''
 for family in ['Gee','Mmrm']:
  time='Time' if family=='Gee' else 'Visit'
  classes+='''    Public Class UdfFAMILYComponent38(Of TValue)
        Public Property Label As String
        Public Property Weight As Double
        Public Property TIMESpecified As Boolean
        Public Property TIMEValue As Double
        Public Property ProfileValues As New List(Of TValue)()
    End Class

'''.replace('FAMILY',family).replace('TIME',time)
 common=common.replace('    Public NotInheritable Class UdfModelProfilePreparation38',classes+'    Public NotInheritable Class UdfModelProfilePreparation38')
 common=common.replace('    End Class\nEnd Namespace','\n\n'+'\n\n'.join(methods)+'\n    End Class\nEnd Namespace')
 g=inherited(g,'GeeLsmEstimateComponent','UdfGeeComponent38(Of GeeLsmEstimateProfileValue)')
 m=inherited(m,'MmrmLsmEstimateComponent','UdfMmrmComponent38(Of MmrmLsmEstimateProfileValue)')
 # Row-mask allocation and preconditions stay before this call, including private
 # ByRef mask allocation if an exceptional comparison interrupts a partial loop.
 old=member(m,'TryBuildMmrmAtProfileRowMask')
 a=old.index('            For i As Integer = 0 To n - 1');z=old.index('\n            If matchedCount <= 0 Then',a)
 loop=old[a:z]
 coreloop=reindent(loop).replace('atProfile.VisitSpecified','timeSpecified').replace('atProfile.VisitValue','targetTime').replace('h.VisitValues','timeValues').replace('h.FittedDesign','design').replace('atProfile.ProfileValues','profiles')
 coreloop=coreloop.replace('MmrmLsmEstimateProfileValue','T').replace('profileValue.ColumnIndex','column(profileValue)').replace('profileValue.Value','value(profileValue)').replace('regression.MixedModelPostEstimation.NearlyEqual','equal')
 replacement='''            Global.BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38.FillRowMask(
                h.FittedDesign, h.VisitValues, atProfile.VisitSpecified, atProfile.VisitValue,
                atProfile.ProfileValues, Function(profile) profile.ColumnIndex, Function(profile) profile.Value,
                AddressOf regression.MixedModelPostEstimation.NearlyEqual, rowMask, matchedCount)
'''
 m=m.replace(old,old[:a]+replacement+old[z:])
 corepath=Path('BESHStat.Core/src/WorksheetFunctions/UdfProfilePreparation38.vb');core=read(corepath)
 core=core.replace('    End Class\nEnd Namespace','''        '''+"''' <summary>Fill the caller-owned mask in saved-design row order, retaining partial writes on exceptions.</summary>"+'''
        Public Shared Sub FillRowMask(Of T)(design As Double(,), timeValues As Double(),
            timeSpecified As Boolean, targetTime As Double, profiles As IEnumerable(Of T),
            column As Func(Of T, Integer), value As Func(Of T, Double), equal As Func(Of Double, Double, Boolean),
            ByRef rowMask As Boolean(), ByRef matchedCount As Integer)
            Dim n As Integer = design.GetLength(0)
'''+coreloop+'''
        End Sub
    End Class
End Namespace''')
 for token in ['ExcelArg','Global.BESHStatNG.WorksheetFunctions',' h,','h.']:assert token not in common,token
 for path,text in [(GEE,g),(MMRM,m),(IMPORT,i),(COMMON,common),(corepath,core)]:put(path,text)
 with (HERE/'specification-extraction-v1.json').open('x',encoding='utf-8') as f:json.dump(dict(status='IMPLEMENTED_COMPATIBILITY_PENDING',members=['TryGetGeeLsmEstimateSpec','TryGetMmrmLsmEstimateSpec','TryBuildMmrmAtProfileRowMask'],portable='Family-specific Common typed specifications; Core fills original caller-owned mask',changed=[str(p) for p in [GEE,MMRM,IMPORT,COMMON,corepath]],original_new_reference_executions=0),f,indent=2)
 print('GEE/MMRM specs and MMRM saved-row loop delegated; compatibility pending')
if __name__=='__main__':main()
