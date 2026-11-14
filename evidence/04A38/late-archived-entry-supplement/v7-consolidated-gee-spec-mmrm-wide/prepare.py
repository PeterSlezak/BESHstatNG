"""Prepare only Approval G instrumentation/recorder mechanics in new files."""
from pathlib import Path
import re
HERE=Path(__file__).resolve().parent;OLD=HERE.parent/'v6-mmrm-intercept-group'
QUOTE=('I approve the consolidated four-case-per-configuration archived-entry supplement for Phase 04A.38, covering the GEE `spec` intercept-role rejection at original lines 1506–1509 and the MMRM wide-profile multiple-row rejection at original lines 592–594, once per Invariant and sk-SK culture in Debug and Release.\n\n'
 'Codex must authenticate the original models and sources, seal typed inputs and original plain/repeat/trace outputs before current comparisons, and prove the actual original guards. Preserve all earlier evidence and stop for separate approval if another essential gap emerges. If both gates pass, continue W2 and W3 of the existing 04A.38 plan.')
for name in ['common.py','preflight.py','build_original.py','compile_harness.py','MaterializeMmrm38.cs','MmrmCapture38.cs','execute_original.py','verify_original_source.py']:
 text=(OLD/name).read_text(encoding='utf-8').replace('mmrm38-v6','consolidated38-v7').replace('Append-only v6','Append-only v7')
 if name=='common.py':
  text=text.replace("V5=ROOT/", "W1=ROOT/'evidence/04A38/completion/w1'\nV6=ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'\nV5=ROOT/")
  text=re.sub(r"PREVIOUS='[^']+'","PREVIOUS='evidence/04A38/completion/w1/publication-manifest-v2.json'",text)
 if name=='preflight.py':
  text=re.sub(r'^QUOTE=.*$',lambda m:'QUOTE='+repr(QUOTE),text,flags=re.M).replace('==1250','==1454').replace("approval='F'","approval='G'")
  text=text.replace('scalar Intercept LS-means grouping guard','two consolidated GEE spec/MMRM wide guards').replace('cases_per_configuration=2','cases_per_configuration=4').replace("case_kinds=['mmrm-lsmeans-scalar-intercept-group']","case_kinds=['GEE_SPEC_INTERCEPT_ROLE','MMRM_WIDE_MULTIROW_REJECTION']")
  text=text.replace("public_entry='BESH.REGR.MMRM_LSMEANS',preceding_proposal=pin(V5/'owner-decision-needed.md')", "public_entries=['BESH.REGR.GEE_LSMESTIMATE','BESH.REGR.MMRM_LSMESTIMATE'],preceding_proposal=pin(W1/'owner-decision-needed.md')")
  text=text.replace("approvalE=pin(V5/'owner-authorization.json'),","approvalE=pin(V5/'owner-authorization.json'),approvalF=pin(V6/'owner-authorization.json'),")
  text=text.replace('predecessor_external_hashes=1249','predecessor_external_hashes=1453').replace("str(V5/'verify_closeout.py'),'v2'", "str(W1/'verify_publication.py'),'v2'")
  text=text.replace(" save('owner-authorization.json',", " save('before-state-source-test-projects.json',dict(files=[pin(p) for directory in ['BESHStat.Core','BESHStat.Excel.Common','BESHStatNG.Test','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHstatNG/src/ExcelUDFs'] for p in sorted((ROOT/directory).rglob('*')) if p.is_file() and p.suffix.lower() in ['.vb','.vbproj','.csproj'] and not any(part.lower() in ['bin','obj','testresults','packages'] for part in p.relative_to(ROOT).parts)],scope='Current source/projects/tests before G capture or production edit; no TestResults traversal'))\n save('owner-authorization.json',")
  text=text.replace('1249 predecessor','1453 predecessor').replace('original intercept guard1477-1480; Approval F','original GEE/MMRM guards; Approval G')
 if name=='compile_harness.py':
  text=text.replace("source.name=='MixedModelUDFs.vb'", "source.name in ['GeeUDFs.vb','MixedModelUDFs.vb','UdfDataImportMixedModel.vb']")
  a=text.index(" if filename.endswith('/MixedModelUDFs.vb'):");b=text.index('\n return text,maps',a)
  text=text[:a]+''' if filename.endswith('/GeeUDFs.vb'):
  member('GEE_LSMESTIMATE',[
   ('If Not TryGetGeeLsmEstimateSpec(spec, parameterNames, components, errorMessage) Then','G1.public.spec_rejected','True',True),
   ('If Not TryGetGeeLsmEstimateSpec(spec, parameterNames, components, errorMessage) Then','G1.public.components_count','components.Count',True),
   ('If Not TryGetGeeLsmEstimateSpec(spec, parameterNames, components, errorMessage) Then','G1.public.error','errorMessage',True)])
  member('TryGetGeeLsmEstimateSpec',[
   ('components = New List(Of GeeLsmEstimateComponent)()','G1.spec.entry','True',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','G1.header.weight_col','weightCol',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','G1.header.label_col','labelCol',False),
   ('Dim idx As Integer = GeeFindDesignColumnIndex(parameterNames, header(c))','G1.lookup.index','idx',True),
   ('If String.Equals(parameterNames(idx), "Intercept", StringComparison.OrdinalIgnoreCase) OrElse','G1.lookup.name','parameterNames(idx)',False),
   ('String.Equals(parameterNames(idx), "(Intercept)", StringComparison.OrdinalIgnoreCase) Then','G1.guard.intercept','True',True),
   ('errorMessage = "the intercept column should not be used as a profile column; it is supplied automatically by the fitted design."','G1.guard.error','errorMessage',True),
   ('errorMessage = "the intercept column should not be used as a profile column; it is supplied automatically by the fitted design."','G1.guard.components_count','components.Count',True)])
 elif filename.endswith('/UdfDataImportMixedModel.vb'):
  member('TryGetMmrmLsmEstimateAtSpec',[
   ('atProfile = Nothing','G2.parser.entry','True',False),
   ('If attemptedHeaderlessAt Then Return False','G2.headerless.attempted','attemptedHeaderlessAt',False),
   ('Dim profileColumns As New List(Of KeyValuePair(Of Integer, Integer))()','G2.branch.wide','True',False),
   ('If dataRow >= 0 Then','G2.wide.row','r',False),
   ('If dataRow >= 0 Then','G2.wide.prior_data_row','dataRow',False),
   ('If dataRow >= 0 Then','G2.guard.multirow','True',True),
   ('dataRow = r','G2.first_row.accepted','dataRow',True),
   ('errorMessage = "wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row."','G2.guard.error','errorMessage',True),
   ('errorMessage = "wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row."','G2.guard.atProfile_Nothing','atProfile Is Nothing',True)])
  member('TryGetMmrmHeaderlessNameValueAtSpec',[
   ('If arr.GetLength(1) <> 2 Then Return False','G2.headerless.columns','arr.GetLength(1)',False),
   ('If arr.GetLength(1) <> 2 Then Return False','G2.headerless.column_guard_true','arr.GetLength(1) <> 2',False)])
 elif filename.endswith('/MixedModelUDFs.vb'):
  member('MMRM_LSMESTIMATE',[
   ('Dim atProfile As MmrmLsmEstimateAtProfile = Nothing','G2.spec.accepted','True',False),
   ('If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then','G2.public.at_rejected','True',True),
   ('If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then','G2.public.atProfile_Nothing','atProfile Is Nothing',True),
   ('If Not Global.BESHStatNG.UdfDataImport.TryGetMmrmLsmEstimateAtSpec(at, h, atProfile, errorMessage) Then','G2.public.error','errorMessage',True)])
'''+text[b:]
 if name=='verify_original_source.py':text=text.replace("'<ISOLATED_HARNESS>/UdfDataImportMixedModel.vb','<ISOLATED_HARNESS>/MixedModelUDFs.vb'", "'<ISOLATED_HARNESS>/UdfDataImportMixedModel.vb','<ISOLATED_HARNESS>/MixedModelUDFs.vb','<ISOLATED_HARNESS>/GeeUDFs.vb'")
 if name=='MmrmCapture38.cs':
  text=text.replace('static string ActiveHandle;','static string ActiveHandle; static string ActiveFamily;')
  text=text.replace('"HANDLE:MMRM:SESSION1"','"HANDLE:"+ActiveFamily+":SESSION1"').replace('if(count!=2)','if(count!=4)').replace('Captured2 ','Captured4 ')
  text=text.replace('ActiveHandle=null;','ActiveHandle=null;ActiveFamily=(string)c["case_kind"]=="GEE_SPEC_INTERCEPT_ROLE"?"GEE":"MMRM";')
  text=text.replace('Library.GetType("BESHStatNG.WorksheetFunctions.MixedModelUDFs",true)', 'Library.GetType(ActiveFamily=="GEE"?"BESHStatNG.WorksheetFunctions.GEEUDFs":"BESHStatNG.WorksheetFunctions.MixedModelUDFs",true)').replace('gee.GetField("_mmrmCache",','gee.GetField(ActiveFamily=="GEE"?"_geeCache":"_mmrmCache",')
  text=text.replace('(string)op["target"]=="BESH.REGR.MMRM_FIT"','(string)op["target"]=="BESH.REGR.MMRM_FIT"||(string)op["target"]=="BESH.REGR.GEE_FIT"').replace('@"^MMRM:[0-9a-f]{32}$"','"^"+ActiveFamily+@":[0-9a-f]{32}$"')
  a=text.index('       var fittedParameters=');b=text.index('\n      }\n      kind=Kind',a)
  text=text[:a]+'''       var fittedDesign=(double[,])property("FittedDesign");
       var fittedParameters=ActiveFamily=="MMRM"?(string[])property("FixedEffectNames"):(string[])gee.GetMethod("BuildParameterNames",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{property("VarNames"),fittedDesign.GetLength(1)});
       if(property("Result")==null||fittedParameters==null||!fittedParameters.Contains("Intercept")||fittedDesign.GetLength(1)!=fittedParameters.Length)throw new Exception("FAILED_OR_FOREIGN_FITTED_MODEL");
       var fittedTime=(double[])property(ActiveFamily=="MMRM"?"VisitValues":"FittedTimeValues");
       if(ActiveFamily=="MMRM"&&(fittedTime.Length!=54||new[]{1.0,2.0,3.0}.Any(v=>fittedTime.Count(x=>x==v)!=18)))throw new Exception("MMRM_FACTORY_CHANGED");
       Sessions.Add(new{case_id=(string)c["case_id"],family=ActiveFamily,raw_handle=ActiveHandle,symbol="HANDLE:"+ActiveFamily+":SESSION1",parameter_names=fittedParameters,
        fitted_design=Snap(fittedDesign),fitted_time=Snap(fittedTime),alpha=Snap(property("Alpha")),creation_order=modelCreations,cached_result_nonnull=true,
        source_rows_used=ActiveFamily=="MMRM"?Snap(property("SourceRowsUsed")):"NOT_A_GEE_HANDLE_PROPERTY",source_rows_dropped=ActiveFamily=="MMRM"?Snap(property("SourceRowsDropped")):"NOT_A_GEE_HANDLE_PROPERTY"});'''+text[b:]
  a=text.index('   var query=Seq(ops[1]["arguments"])');b=text.index('\n   System.Threading.Thread.CurrentThread.CurrentCulture=old;',a)
  text=text[:a]+'''   var query=Seq(ops[1]["arguments"]).Select(Dict).ToArray();
   var values=query.Skip(1).Select(d=>Decode(Dict(d["value"]))).ToArray();
   var types=values.Select(v=>v==null?"Nothing":v.GetType().FullName).ToArray();
   if(values.OfType<Array>().Any(a=>a.GetType()!=typeof(object[,])||a.GetLowerBound(0)!=0||a.GetLowerBound(1)!=0))throw new Exception("WRONG_TYPED_ARRAY");
   cases.Add(new{case_id=(string)c["case_id"],culture,ui_culture=System.Threading.Thread.CurrentThread.CurrentUICulture.Name,fit=Snap(fit),query_arguments=Snap(values),query_runtime_types=types,before_culture=old.Name,before_ui_culture=oldUi.Name});'''+text[b:]
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE.parent/'v2-gee-profile/MaterializeGee38.cs').read_text(encoding='utf-8')
with (HERE/'MaterializeGee38.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
