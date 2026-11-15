from common42 import *
import re
scope=module('scope_gui42',HERE/'scope-v1.py');matrix=load(HERE/'scope/ownership-parity-v1.json')['function_matrix']
ui='BESHstatNG/src/UI/';mapping={'PCA':(ui+'Ui11PCA.vb','RunPCA'),'FA':(ui+'Ui11PCA.vb','RunFA'),'KMEANS':(ui+'Ui11PCA.vb','RunKmeans'),'HCLUST':(ui+'Ui11PCA.vb','RunHierarchicalClustering'),'MCA':(ui+'Ui11PCA.vb','RunMCA'),'CA':(ui+'Ui0OneRefeditMulticol.vb','RunCA'),'DA':(ui+'Ui11PCA.vb','RunDiscriminantAnalysis'),'PS':(ui+'Ui20PropensityScoreMatching.vb','RunPSM'),'PSM':(ui+'Ui20PropensityScoreMatching.vb','RunPSM'),'HIST':(ui+'UibyID.vb','RunHistogram'),'ROC':(ui+'UibyID.vb','RunROC')}
out=[]
for r in matrix:
    family=r['function'].split('_')[0];target=mapping.get(family);members=[]
    if target:
        path,name=target;m=next(m for m in scope.methods(ROOT/path) if m['name']==name);members.append(dict(path=pin(path),method=name,lines=[m['start'],m['end']],method_sha256=sha(m['text'].encode()),request_calls=sorted(set(re.findall(r'\b(?:WindowsMultivariateWorkflowAdapter|Multivariate\w+Request|Psm\w+|BuildFitOptions|Application\.DoEvents)\b',m['text'])))))
    elif family=='CLASS' or r['function']=='CALIB_POINTS':
        path='BESHstatNG/src/RegModels/BinaryClassificationReporting.vb';names=['ComputeBinarySummary','BuildThresholdTable','BuildCalibrationBins','ComputeBrierScore','WrapResults']
        for m in scope.methods(ROOT/path):
            if m['name'] in names:members.append(dict(path=pin(path),method=m['name'],lines=[m['start'],m['end']],method_sha256=sha(m['text'].encode()),scope='Shared Windows report consumer,not a claimed one-to-one native form interface.'))
    else:
        members.append(dict(path=pin('BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb'),scope='Native rendering residual only;no GUI input equivalence asserted. Core KM computation and Common curve source are separately pinned.'))
    out.append(dict(function=r['function'],excel_name=r['excel_name'],consumer_members=members,GUI_source_changed=False,native_executed=False,parity='Source mapping only;row/default/guard/chart differences stay restricted;not GUI-UDF equality.',restricted_native_boundary=['Workbook/range acquisition','Native chart/report placement','Session lifetime/cache','Original scientific/output decisions']))
save('scope/gui-current-member-crosswalk-v1.json',dict(functions=out,scoped_exports=90,GUI_source_changed=False,native_executed=False,gates=GATES))
tests=load(HERE/'build-and-tests/Debug-identities-v2.json')['tests'];delta=load(HERE/'register/issue-delta-v1.json');links=[]
for issue in delta['issues']:
    tags=['Psm25','Love35'] if any('C25-' in a for a in issue['aliases']) else ['Multivariate18','Correspondence17','Cluster19','Multivariate20','UdfPreparation38Adapter','UdfMultivariate42']
    found=[{k:r[k] for k in ['suite','identity_sha256','occurrence','class_name','method','outcome']} for r in tests if any(t in r['class_name'] or t in r['method'] for t in tags)]
    links.append(dict(canonical_id=issue['canonical_id'],executed_managed_context=found,scope='Actual source-linked original/current unit/adapter identities;not native correction execution or an independent scientific oracle.',native_inferential_candidate_activated=False))
save('register/executed-context-crosswalk-v1.json',dict(issues=links,Debug=pin(HERE/'build-and-tests/Debug-identities-v2.json'),all_original_nonpasses_retained=True,scientific='NOT_GOLDEN'))
print('90 actual GUI/report consumer source/method links;canonical investigation/executed managed context crosswalk captured',flush=True)
