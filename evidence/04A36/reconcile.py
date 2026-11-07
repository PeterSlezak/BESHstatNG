"""Classify real source-test deltas without altering any historical assertion."""
from work import *
from summarize import load,clean,py
if __name__=='__main__':
    baseline={r['path']:r for r in load(HERE/'entry-tracked-pins.json')['files']}
    batch=['BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHStat.Excel.Common/src/Results/SurvivalReportPolicy.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj']+['BESHstatNG/src/Graphics/'+n+'.vb' for n in ['KaplanMeierPlotExcel','CoxSurvivalPlotExcel','SpcControlChartExcel']]
    changed=git('diff','--name-only').splitlines();external=[p for p in changed if p not in batch]
    assert all(p.endswith('README.md') or p=='planning/BESHStatNG_CrossPlatform_MasterPlan.md' for p in external),external
    save('external-changes.json',dict(status='OWNER_CONFIRMATION_PENDING',files=[dict(path=p,entry=baseline[p],current=pin(p),diff_sha256=sha(git('diff','--',p).encode()),disposition='PRESERVED_EXTERNAL_DOCUMENTATION_CHANGE_NOT_36') for p in external],qualification='No36 script edits these files. Their changed bytes are preserved and excluded from proposed36 publication. One pending clarification asks owner to confirm the concurrent documentation updates.'))
    source=load(HERE/'source-and-node-validation.json')
    for r in source['new_nonpasses']:
        identity=r['identity'];paths=[];classification='UNCLASSIFIED';proof=''
        if 'test_no_designer_udf_numerical_packaging_or_framework_change' in identity:
            paths=['BESHStat.Core/README.md'];assert paths[0] in external
            classification='UNRELATED_EXTERNAL_DOCUMENTATION_SCOPE_ASSERTION'
            proof='scripts/tests/test_04a05_contingency.py:142-143 calls git diff --name-only (tracked paths only). The only changed tracked Core path is its external README. New36 Core files are unstaged/untracked; no designer/UDF/DNA/target change exists. Original assertion remains FAIL.'
        elif 'test_c09_c11_c12_dormant_candidates_gee_and_public_accepted_evidence_unchanged' in identity:
            paths=['docs/architecture/README.md'];assert paths[0] in external
            classification='UNRELATED_EXTERNAL_DOCUMENTATION_WHOLE_FILE_PIN'
            proof='scripts/tests/test_04a13_survival_workflow.py:148 pins generic architecture README to entry7d0b182f...; concurrent documentation update is03f16428... . Original C09/C11/C12 patches, numerical/GUI sources, dated dossiers and controls remain unchanged. No36 authorization is claimed for this README.'
        elif 'test_original_chart_renderer_formula_comment_and_series_indexing_preserved' in identity:
            paths=['BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb','BESHstatNG/src/Graphics/CoxSurvivalPlotExcel.vb']
            classification='AUTHORIZED_RENDERER_PREPARATION_SOURCE_SCOPE'
            proof='scripts/tests/test_04a13_survival_workflow.py:78 compares the entire retained post13 renderer tail, including the numeric preparation explicitly extracted in36. Public headers, native style tails and original timing are protected by current AST/IL/legacy captures; original source assertion remains FAIL.'
        elif 'test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved' in identity:
            paths=[p for p in batch if p in r['subtest']];assert len(paths)==1
            classification='AUTHORIZED_WHOLE_FILE_PIN'
            proof='scripts/tests/test_04a28_output.py:29 retains original whole-source SHA. Authorized36 renderer source transition; no old expected hash/test modified. Exact captured vectors and original public/native boundary checks pass.'
        r.update(classification=classification,proof=proof,sources=[dict(path=p,entry_sha256=baseline[p]['sha256'],final_sha256=pin(p)['sha256']) for p in paths])
    source['unclassified']=[r for r in source['new_nonpasses'] if r['classification']=='UNCLASSIFIED'];save('source-and-node-validation.json',source)
    v=load(HERE/'validation.json');v['unclassified_python']=source['unclassified'];v['fresh_main_python']={k:sum(r['baseline'][k] for r in source['python']) for k in ['total','passed','failed','errors']};v['qualified_new_python_nonpasses']=source['new_nonpasses'];save('validation.json',v)
    audits=[]
    for label in ['entry','final']:
        for batch_number in ['34','35']:
            r=py(label,'accepted'+batch_number+'-audits');r.pop('failure_receipts');audits.append(r)
    save('prior-packet-audits.json',dict(runs=audits,qualification='Real inherited/source-successor FAILs remain visible. Accepted35 owner filename correction aux.py -> auxilary.py is qualified independently, not by rewriting the old manifest/audit. Accepted34 manifest now sees authorized35/36 successor sources.'))
    timing=py('entry','help-serial-recheck');timing.pop('failure_receipts')
    save('baseline-help-timing.json',dict(original=next(r for r in source['python'] if r['suite']=='python-help')['baseline'],identity='test_mkdocs_htaccess.LatestMimeBuildTests.test_owner_wrapper_regenerates_configs_and_builds_both_targets_from_either_cwd',subtest="invocation='repository-root'",original_error='TimeoutExpired after original120-second child deadline',same_unchanged_source_recheck=timing,final_full=next({k:r[k] for k in ['total','passed','failed','errors','exit_code']} for r in source['python'] if r['suite']=='python-help'),disposition='Retained real pre-edit ERROR; unchanged7-case serial recheck and final16-case full Help pass. No deadline change or timing waiver.'))
    comparisons=[]
    for config,pre in [('Debug','pre-edit-capture.csv'),('Release','pre-edit-Release.csv')]:
        for family,before,after in [('KM_Cox_SPC_bands',pre,'post-edit-geometry-'+config+'.csv'),('SPC_ordered_presentation','pre-edit-presentation-v2'+('-Release' if config=='Release' else '')+'.csv','post-edit-presentation-'+config+'.csv'),('SPC_axis','pre-edit-axis-v2'+('-Release' if config=='Release' else '')+'.csv','post-edit-axis-'+config+'.csv')]:
            a=(HERE/before).read_bytes();b=(HERE/after).read_bytes();assert a==b,(config,family)
            rows=list(csv.DictReader(io.StringIO(a.decode('utf-8-sig'),newline='')))
            comparisons.append(dict(configuration=config,family=family,pre_edit=pin('evidence/04A36/'+before),post_edit=pin('evidence/04A36/'+after),rows=len(rows),successful_outputs=sum(r['exception']=='None' for r in rows),recorded_legacy_exceptions=dict(__import__('collections').Counter(r['exception'] for r in rows if r['exception']!='None')),byte_identical=True,mismatches=0))
    save('compatibility-comparison.json',dict(status='PASS_EXACT_SOFTWARE_COMPATIBILITY',comparisons=comparisons,rows_per_configuration=6240,scientific='NOT_GOLDEN',authority='Frozen pre-edit source slices / accepted22 legacy math; V2 correction explicitly retained with original mostly rejected (6 valid / 378 rejected) V1, links only entry assemblies.',policy='Exact bytes/IEEE/array shape/mask/order/exception type+parameter; no relaxed tolerance or locale substitution; native visual parity NOT_RUN.'))
    print('Classified',len(source['new_nonpasses']),'new source-test failures; unclassified',len(source['unclassified']),';6 exact comparisons')
