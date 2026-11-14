"""Prepare new v5 observation-only recorder sources before any input seal."""
from pathlib import Path
HERE=Path(__file__).resolve().parent
OLD=HERE.parent/'v4-mmrm-duplicate-guards'
QUOTE=('I approve the additional two-case-per-configuration MMRM unmatched-profile archived-entry supplement for Phase 04A.38, limited to `MMRM_LSMEANS` with the wide-form `visit=99.0` profile, one case in Invariant and one in sk-SK, in both Debug and Release.\n\n'
 'The original source and typed inputs must be authenticated and sealed, the actual original no-matching-rows guard must be reached, and original outputs must be sealed before current-code comparisons. Preserve all previous evidence and stop for separate authorization if another essential reference gap emerges.')
text=(OLD/'preflight.py').read_text(encoding='utf-8')
start=text.index('QUOTE=');end=text.index('\n\ndef main()',start)
text=text[:start]+'QUOTE='+repr(QUOTE)+text[end:]
text=text.replace('==1004','==1119').replace("approval='D'","approval='E'")
text=text.replace('precisely two duplicate guards in two cultures','precisely the unmatched wide visit99 LS-means profile in two cultures')
text=text.replace('cases_per_configuration=4','cases_per_configuration=2').replace("case_kinds=['duplicate-visit-time','duplicate-fitted-column']","case_kinds=['mmrm-lsmeans-wide-visit-99-unmatched']")
text=text.replace("public_entry='BESH.REGR.MMRM_LSMESTIMATE',preceding_proposal=pin(V3/", "public_entry='BESH.REGR.MMRM_LSMEANS',preceding_proposal=pin(V4/")
text=text.replace("approvalC=pin(V3/'owner-authorization.json'),", "approvalC=pin(V3/'owner-authorization.json'),approvalD=pin(V4/'owner-authorization.json'),")
text=text.replace('predecessor_external_hashes=1003','predecessor_external_hashes=1118').replace("str(V3/'verify_closeout.py')","str(V4/'verify_closeout.py')")
anchor='  snapshots.append(dict('
idx=text.index(anchor)
text=text[:idx]+'''  if row['source']['path'].endswith('/MixedModelUDFs.vb'):
   assert len(snapshot)==92704 and sha(snapshot)=='596071f9fdb00fb94585e929a6899494293d2dc7ee0064faadf098b0d95dc13d'
   lines=snapshot.decode('utf-8-sig').splitlines();assert lines[1425].strip()=='If matchedCount <= 0 Then'
'''+text[idx:]
text=text.replace(" print('Authenticated live Git,1003 predecessor payloads,4695 original files,41 snapshots and original guard lines726/744; Approval D recorded')", " print('Authenticated live Git,1118 predecessor payloads,4695 original files,41 snapshots and original mask line1426; Approval E recorded')")
with (HERE/'preflight.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE/'compile_harness.py').read_text(encoding='utf-8')
text=text.replace('  Dim text As String = If(TypeOf value Is Double,', '''  Dim text As String
  If value Is Nothing Then
   text = "N"
  ElseIf TypeOf value Is Boolean() Then
   Dim mask As Boolean() = DirectCast(value, Boolean())
   text = "System.Boolean[" & mask.GetLowerBound(0).ToString(CultureInfo.InvariantCulture) & ":" & mask.Length.ToString(CultureInfo.InvariantCulture) & "]{"
   For i As Integer = 0 To mask.Length - 1
    If i > 0 Then text &= "|"
    text &= If(mask(i), "B1", "B0")
   Next
   text &= "}"
  Else
   text = If(TypeOf value Is Double,''')
text=text.replace('  Events.Add(name & "=" & text)', '  End If\n  Events.Add(name & "=" & text)')
start=text.index("  member('MMRM_LSMESTIMATE',[")
end=text.index('\n return text,maps',start)
text=text[:start]+'''  member('MMRM_LSMEANS',[
   ('ElseIf IsMmrmWorksheetRangeArgument(group) Then','public.range','True',True),
   ('ElseIf IsMmrmWorksheetRangeArgument(group) Then','public.alpha_missing','ExcelArgPredicates.IsMissingArg(alpha)',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.mask_rejected','True',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.mask','rowMask',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.description','profileDescription',True),
   ('If Not TryBuildMmrmAtProfileRowMask(h, atProfile, rowMask, profileDescription, errorMessage) Then','public.error','errorMessage',True),
   ('If table Is Nothing Then Return ExcelError.ExcelErrorNA','report.continued','True',False)])
  member('TryBuildMmrmAtProfileRowMask',[
   ('rowMask = Nothing','mask.entry','True',False),
   ('rowMask = New Boolean(n - 1) {}','mask.rows','n',False),
   ('rowMask = New Boolean(n - 1) {}','mask.visit_specified','atProfile.VisitSpecified',False),
   ('rowMask = New Boolean(n - 1) {}','mask.target_visit','atProfile.VisitValue',False),
   ('rowMask(i) = keep','row.index','i',False),
   ('rowMask(i) = keep','row.visit','h.VisitValues(i)',False),
   ('rowMask(i) = keep','row.keep','keep',False),
   ('If matchedCount <= 0 Then','guard.unmatched','True',True),
   ('If matchedCount <= 0 Then','guard.matched_count','matchedCount',True),
   ('If matchedCount <= 0 Then','guard.mask','rowMask',True),
   ('errorMessage = "no observed design rows matched the supplied LS-means profile."','guard.error','errorMessage',True),
   ('errorMessage = "no observed design rows matched the supplied LS-means profile."','guard.description','profileDescription',True)])
'''+text[end:]
(HERE/'compile_harness.py').write_text(text,encoding='utf-8',newline='\n')
text=(HERE/'MmrmCapture38.cs').read_text(encoding='utf-8').replace('if(count!=4)','if(count!=2)').replace('Captured4 ','Captured2 ')
text=text.replace('string culture=(string)c["culture"];', 'var originalCulture=System.Threading.Thread.CurrentThread.CurrentCulture;var originalUiCulture=System.Threading.Thread.CurrentThread.CurrentUICulture;string culture=(string)c["culture"];')
text=text.replace('var fittedParameters=(string[])property("FixedEffectNames");', '''var fittedParameters=(string[])property("FixedEffectNames");
       var fittedVisits=(double[])property("VisitValues");var fittedDesign=(double[,])property("FittedDesign");
       if(fittedVisits==null||fittedVisits.Length!=54||fittedDesign.GetLength(0)!=54||new[]{1.0,2.0,3.0}.Any(v=>fittedVisits.Count(x=>x==v)!=18))throw new Exception("ORIGINAL_FACTORY_VISIT_CONTEXT_MISMATCH");''')
text=text.replace('trace.Add(new{case_id=(string)c["case_id"],culture,operations=witnesses});count++;', '''trace.Add(new{case_id=(string)c["case_id"],culture,operations=witnesses});count++;
    System.Threading.Thread.CurrentThread.CurrentCulture=originalCulture;System.Threading.Thread.CurrentThread.CurrentUICulture=originalUiCulture;
    if(!Object.ReferenceEquals(System.Threading.Thread.CurrentThread.CurrentCulture,originalCulture)||!Object.ReferenceEquals(System.Threading.Thread.CurrentThread.CurrentUICulture,originalUiCulture))throw new Exception("CULTURE_RESTORE_FAILED");''')
(HERE/'MmrmCapture38.cs').write_text(text,encoding='utf-8',newline='\n')
