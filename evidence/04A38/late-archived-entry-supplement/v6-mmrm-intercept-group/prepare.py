"""Prepare separate Approval F sources; never edit predecessor artifacts."""
from pathlib import Path
import re
HERE=Path(__file__).resolve().parent;OLD=HERE.parent/'v5-mmrm-unmatched-profile'
QUOTE=('I approve the additional two-case-per-configuration MMRM intercept-group archived-entry supplement for Phase 04A.38, limited to `MMRM_LSMEANS(handle, "Intercept", Nothing)`, one case in Invariant and one in sk-SK, in both Debug and Release.\n\n'
 'Codex must authenticate the original source and fitted model, seal typed inputs and genuine original outputs before current comparisons, and prove that the original intercept-rejection guard at lines 1477–1479 executes. All previous evidence must remain intact. Any further essential reference gap requires separate owner authorization. For the next prompt, can you create a "larger" prompt to complete the whole 04A.38 in next 2 or 3 batches (if that\'s feasible).')
for name in ['common.py','preflight.py','build_original.py','compile_harness.py','MaterializeMmrm38.cs','MmrmCapture38.cs','seal_inputs.py','execute_original.py','verify_original_source.py']:
 text=(OLD/name).read_text(encoding='utf-8').replace('mmrm38-v5','mmrm38-v6').replace('Append-only v5','Append-only v6')
 if name=='common.py':
  text=text.replace("V4=ROOT/", "V5=ROOT/'evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile'\nV4=ROOT/")
  text=text.replace("PREVIOUS='evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/publication-manifest-closeout-v1.json'", "PREVIOUS='evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/publication-manifest-closeout-v2.json'")
 if name=='preflight.py':
  text=re.sub(r'^QUOTE=.*$',lambda m:'QUOTE='+repr(QUOTE),text,flags=re.M)
  text=text.replace('==1119','==1250').replace("approval='E'","approval='F'").replace('unmatched wide visit99 LS-means profile','scalar Intercept LS-means grouping guard')
  text=text.replace('mmrm-lsmeans-wide-visit-99-unmatched','mmrm-lsmeans-scalar-intercept-group').replace("preceding_proposal=pin(V4/","preceding_proposal=pin(V5/")
  text=text.replace("approvalD=pin(V4/'owner-authorization.json'),","approvalD=pin(V4/'owner-authorization.json'),approvalE=pin(V5/'owner-authorization.json'),")
  text=text.replace('predecessor_external_hashes=1118','predecessor_external_hashes=1249').replace("str(V4/'verify_closeout.py')","str(V5/'verify_closeout.py'),'v2'")
  text=text.replace("assert lines[1425].strip()=='If matchedCount <= 0 Then'","assert lines[1476].strip().startswith('If String.Equals(h.FixedEffectNames(idx),') and lines[1479].strip()=='Return False'")
  text=text.replace('1118 predecessor','1249 predecessor').replace('original mask line1426; Approval E','original intercept guard1477-1480; Approval F')
 if name=='compile_harness.py':
  a=text.index(" if filename.endswith('/UdfDataImportMixedModel.vb'):");b=text.index('\n return text,maps',a)
  text=text[:a]+''' if filename.endswith('/MixedModelUDFs.vb'):
  member('MMRM_LSMEANS',[
   ('ElseIf IsMmrmWorksheetRangeArgument(group) Then','public.range','True',True),
   ('Dim groupName As String = ExcelArgReaders.AsString(group)','public.scalar_group','groupName',True),
   ('Dim groupName As String = ExcelArgReaders.AsString(group)','public.alpha_missing','ExcelArgPredicates.IsMissingArg(alpha)',True),
   ('ElseIf Not TryGetDesignColumnValues(h, groupName, resolvedGroupName, groupValues, errorMessage) Then','public.group_rejected','True',True),
   ('ElseIf Not TryGetDesignColumnValues(h, groupName, resolvedGroupName, groupValues, errorMessage) Then','public.resolved_name','If(resolvedGroupName Is Nothing, "N", "S" & Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(resolvedGroupName)))',True),
   ('ElseIf Not TryGetDesignColumnValues(h, groupName, resolvedGroupName, groupValues, errorMessage) Then','public.values','groupValues',True),
   ('ElseIf Not TryGetDesignColumnValues(h, groupName, resolvedGroupName, groupValues, errorMessage) Then','public.error','errorMessage',True),
   ('If table Is Nothing Then Return ExcelError.ExcelErrorNA','report.continued','True',False)])
  member('TryGetDesignColumnValues',[
   ('resolvedName = Nothing','helper.entry','requestedName',False),
   ('errorMessage = Nothing','initialized.resolved_name','If(resolvedName Is Nothing, "N", "S" & Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(resolvedName)))',True),
   ('errorMessage = Nothing','initialized.values','values',True),
   ('errorMessage = Nothing','initialized.error','If(errorMessage Is Nothing, "N", "S" & Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(errorMessage)))',True),
   ('Dim idx As Integer = FindDesignColumnIndex(h.FixedEffectNames, requestedName)','lookup.index','idx',True),
   ('If String.Equals(h.FixedEffectNames(idx), "Intercept", StringComparison.OrdinalIgnoreCase) OrElse','lookup.resolved_name','h.FixedEffectNames(idx)',False),
   ('String.Equals(h.FixedEffectNames(idx), "(Intercept)", StringComparison.OrdinalIgnoreCase) Then','guard.intercept','True',True),
   ('errorMessage = "the intercept column cannot be used as a grouping factor."','guard.error_assigned','errorMessage',True),
   ('errorMessage = "the intercept column cannot be used as a grouping factor."','guard.resolved_name','If(resolvedName Is Nothing, "N", "S" & Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(resolvedName)))',True),
   ('errorMessage = "the intercept column cannot be used as a grouping factor."','guard.values','values',True)])
'''+text[b:]
 if name=='MmrmCapture38.cs':
  text=text.replace('fitted_design=Snap(property("FittedDesign"))','intercept_indices=fittedParameters.Select((n,i)=>new{n,i}).Where(v=>String.Equals(v.n,"Intercept",StringComparison.OrdinalIgnoreCase)).Select(v=>v.i).ToArray(),fitted_design=Snap(property("FittedDesign"))')
  a=text.index('var query=Seq(ops[1]["arguments"])');b=text.index('\n   System.Threading.Thread.CurrentThread.CurrentCulture=old;',a)
  text=text[:a]+'''var query=Seq(ops[1]["arguments"]).Select(Dict).ToArray();var group=Decode(Dict(query[1]["value"]));
   if(group==null||group.GetType()!=typeof(string)||(string)group!="Intercept"||Decode(Dict(query[2]["value"]))!=null)throw new Exception("WRONG_TYPED_SCALAR_GROUP");
   cases.Add(new{case_id=(string)c["case_id"],culture,ui_culture=System.Threading.Thread.CurrentThread.CurrentUICulture.Name,fit=Snap(fit),group=Snap(group),group_runtime_type=group.GetType().FullName,
    alpha=Snap(Decode(Dict(query[2]["value"]))),before_culture=old.Name,before_ui_culture=oldUi.Name});'''+text[b:]
 if name=='seal_inputs.py':
  text=text.replace("source_member='TryBuildMmrmAtProfileRowMask',original_true_branch_line=1426,original_true_branch='If matchedCount <= 0 Then'", "source_member='TryGetDesignColumnValues',original_true_branch_line=1477,original_true_branch='Intercept or (Intercept) ordinal-ignore-case rejection, lines1477-1480'")
  text=text.replace("['public range recognition','wide single visit parsing','formed profile','mask loop over genuine saved rows','no-match true arm','ByRef state propagated through public return']", "['public scalar name route','successful fitted name lookup with observed index','intercept condition true at1477-1478','error assignment1479 thenFalse return1480','private ByRef state propagated through public return']")
  text=text.replace('Trace-only observation of row visits/keep and Boolean mask','Trace-only observation of actual lookup index/name and initialized/returned ByRef name/vector/error')
  text=text.replace('approved target99 does not occur','approved scalar Intercept is an actual fitted name; observe index rather than assume it')
  text=text.replace("'AUTHORIZED-MMRM-UNMATCHED/'", "'AUTHORIZED-MMRM-INTERCEPT/'").replace('mmrm-lsmeans-wide-visit-99-unmatched','mmrm-lsmeans-scalar-intercept-group')
  a=text.index("   group=dict(kind='Array'");b=text.index("\n   c['operations'][1]",a)
  text=text[:a]+"   group=dict(kind='String',value='Intercept')"+text[b:]
  text=text.replace('private mask is trace-only ByRef observation','private name/vector/error are trace-only ByRef observations')
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
