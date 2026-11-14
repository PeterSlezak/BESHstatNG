"""Append-only v4 derivatives; never modify an earlier packet."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
HERE=Path(__file__).resolve().parent
V3=ROOT/'evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile'

def write(name,text):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)

def main():
 text=(V3/'common.py').read_text(encoding='utf-8').replace('v3 evidence utilities','v4 evidence utilities').replace('mmrm38-v3','mmrm38-v4')
 text=text.replace("PREVIOUS='evidence/04A38/late-archived-entry-supplement/v2-gee-profile/publication-manifest-closeout-v1.json'", "V3=ROOT/'evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile'\nPREVIOUS='evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile/publication-manifest-closeout-v1.json'")
 write('common.py',text)
 write('build_original.py',(V3/'build_original.py').read_text(encoding='utf-8').replace('mmrm38-v3','mmrm38-v4'))
 for name in ['execute_original.py','MaterializeMmrm38.cs']:
  write(name,(V3/name).read_text(encoding='utf-8'))
 text=(V3/'MmrmCapture38.cs').read_text(encoding='utf-8')
 assert text.count('if(count!=2)')==1 and text.count('Captured2 ')==1
 write('MmrmCapture38.cs',text.replace('if(count!=2)','if(count!=4)').replace('Captured2 ','Captured4 '))
 # Preserve the proven compiler/native-boundary implementation; add only
 # observational hooks inside the two source true branches and for indices.
 text=(V3/'compile_harness.py').read_text(encoding='utf-8')
 anchor="   ('atProfile.VisitSpecified = True','selection.visit','requestedName',False),"
 assert text.count(anchor)==1
 text=text.replace(anchor,"""   ('If atProfile.VisitSpecified Then','visit.previously_specified','atProfile.VisitSpecified',False),
   ('If atProfile.VisitSpecified Then','visit.normalized_dispatch','MmrmNormalizeDesignColumnName(requestedName)',False),
   ('If atProfile.VisitSpecified Then','guard.duplicate_visit_time','True',True),
"""+anchor)
 anchor="   ('atProfile.ProfileValues.Add(New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue With {','selection.design','h.FixedEffectNames(idx)',False)])"
 assert text.count(anchor)==1
 text=text.replace(anchor,"""   ('Dim idx As Integer = MmrmFindDesignColumnIndex(h.FixedEffectNames, requestedName)','design.resolved_index','idx',True),
   ('For Each existing As Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue In atProfile.ProfileValues','design.existing_count','atProfile.ProfileValues.Count',False),
   ('If existing.ColumnIndex = idx Then','design.existing_index','existing.ColumnIndex',False),
   ('If existing.ColumnIndex = idx Then','guard.duplicate_design','True',True),
   ('atProfile.ProfileValues.Add(New Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs.MmrmLsmEstimateProfileValue With {','selection.design','h.FixedEffectNames(idx)',False)])""")
 write('compile_harness.py',text)
 print('Prepared original-only build, observational trace compiler, recorder and INPUT factory adapters; no case executed')
if __name__=='__main__':main()
