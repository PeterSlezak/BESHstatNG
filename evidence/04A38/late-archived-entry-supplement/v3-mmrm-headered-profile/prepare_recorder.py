"""Recorder-only adaptation; original statistics/import/report code is untouched."""
from common import *

def main():
 text=(V2/'GeeCapture38-v2.cs').read_text(encoding='utf-8');edits=[]
 def replace(old,new):
  nonlocal text
  assert text.count(old)==1,old
  text=text.replace(old,new,1);edits.append(dict(original=old,replacement=new))
 text=text.replace('GeeCapture38','MmrmCapture38').replace('HANDLE:GEE:SESSION1','HANDLE:MMRM:SESSION1')
 text=text.replace('BESHStatNG.GeeBranch38','BESHStatNG.MmrmBranch38')
 text=text.replace('BESHStatNG.WorksheetFunctions.GEEUDFs','BESHStatNG.WorksheetFunctions.MixedModelUDFs').replace('"_geeCache"','"_mmrmCache"')
 text=text.replace('BESH.REGR.GEE_FIT','BESH.REGR.MMRM_FIT').replace('@"^GEE:[0-9a-f]{32}$"','@"^MMRM:[0-9a-f]{32}$"')
 start=text.index('       var names=(string[])property("VarNames");');end=text.index('\n      }',start)
 original=text[start:end]
 new='''       var fittedParameters=(string[])property("FixedEffectNames");
       if(property("Result")==null||fittedParameters==null||!fittedParameters.Contains("treatment_active")||((double[,])property("FittedDesign")).GetLength(1)!=fittedParameters.Length)throw new Exception("FAILED_OR_FOREIGN_FITTED_MMRM_MODEL");
       Sessions.Add(new{case_id=(string)c["case_id"],raw_handle=ActiveHandle,symbol="HANDLE:MMRM:SESSION1",parameter_names=fittedParameters,
        fitted_design=Snap(property("FittedDesign")),fitted_visits=Snap(property("VisitValues")),alpha=Snap(property("Alpha")),
        source_rows_used=Snap(property("SourceRowsUsed")),source_rows_dropped=Snap(property("SourceRowsDropped")),
        inference=Snap(property("InferenceLabel")),covariance=Snap(property("CovarianceStructure")),fit_method=Snap(property("FitMethod")),creation_order=modelCreations});'''
 text=text[:start]+new+text[end:];edits.append(dict(original=original,replacement=new))
 replace('if(count!=12)throw new Exception("CASE_COUNT_MISMATCH");','if(count!=2)throw new Exception("CASE_COUNT_MISMATCH");')
 replace('Console.WriteLine("Captured12 "+role','Console.WriteLine("Captured2 "+role')
 with (HERE/'MmrmCapture38.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
 save('recorder-derivation.json',dict(original=pin(V2/'GeeCapture38-v2.cs'),derived=pin(HERE/'MmrmCapture38.cs'),edits=edits,
      systematic_replacements='GEE class/export/cache/handle names to original MMRM identities; count2; MMRM cached fixed-design/result provenance',
      handle_contract='Raw actual GUID retained in session sidecar; exactly one successful actual MMRM_FIT bound to declared HANDLE:MMRM:SESSION1. Only that random identity is symbolized; no numerical/output normalization.',
      changed_statistical_logic=False))
 materializer=(V2/'MaterializeGee38.cs').read_text(encoding='utf-8').replace('MaterializeGee38','MaterializeMmrm38').replace('Does not call GEE_FIT','Does not call MMRM_FIT')
 materializer=materializer.replace('BESHStatNG.WorksheetFunctions.GEEUDFs','BESHStatNG.WorksheetFunctions.MixedModelUDFs').replace('GEE_FIT','MMRM_FIT').replace('GEE_LSMESTIMATE','MMRM_LSMESTIMATE')
 old='  var values=(object[])typeof(Capture38).GetMethod("PublicArgs"'
 assert old in materializer
 materializer=materializer.replace(old,'  typeof(Capture38).GetField("Library",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,library);\n'+old)
 a=materializer.index('  var matrix=');b=materializer.index('  var result=',a)
 materializer=materializer[:a]+'''  var spec=new object[,]{{"Label","Weight","visit","treatment_active"},{"L",1.0,1.0,1.0}};
'''+materializer[b:]
 materializer=materializer.replace('if(value is int)return','if(value is bool)return new {kind="Boolean",value=(bool)value};\n  if(value is int)return')
 with (HERE/'MaterializeMmrm38.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(materializer)
 print('Prepared recorder and frozen INPUT factory adapter; no original/current model or query invoked')
if __name__=='__main__':main()
