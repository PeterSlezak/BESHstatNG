from common import *
source=PREVIOUS/'host_contexts_v1.py';value=source.read_text(encoding='utf-8').replace('packet-v2-context-v5','packet-v1-context-v5')
old="[('CorrespondenceHandle','CA_FIT',4),('MultipleCorrespondenceHandle','MCA_FIT',4),('DiscriminantHandle','DA_FIT',14)]"
new="[('PcaHandle','PCA_FIT',7),('FactorAnalysisHandle','FA_FIT',7),('KMeansHandle','KMEANS_FIT',14),('HierarchicalHandle','HCLUST_FIT',9),('CorrespondenceHandle','CA_FIT',4),('MultipleCorrespondenceHandle','MCA_FIT',4),('DiscriminantHandle','DA_FIT',14)]"
assert old in value;value=value.replace(old,new).replace("new_support=name!='CorrespondenceHandle'","new_support=False")
start=value.index("    for name,api,owner in [('MCA_SUMMARY'")
stop=value.index("    configs.append(",start)
replacement="""    for nativeowner,name,owner,api in [
        (base,'FindWrappedDiscriminantTable','MultivariateAnalysisUDFsReports38','FindWrappedDiscriminantTablePrepared'),
        (base,'DA_COVARIANCE','MultivariateAnalysisUDFsReports38','FindDiscriminantGroupPrepared'),
        ('BESHStatNG.WorksheetFunctions.PlotDataUDFs','HIST_BINS','PlotDataUDFsReports38','BuildHistogramPartsPrepared'),
        ('BESHStatNG.WorksheetFunctions.PlotDataUDFs','HIST_NORMAL','PlotDataUDFsReports38','BuildHistogramNormalPartsPrepared'),
        ('BESHStatNG.WorksheetFunctions.PlotDataUDFs','BuildKaplanMeierStepTable','PlotDataUDFsReports38','BuildKaplanMeierStepTablePrepared'),
        ('BESHStatNG.WorksheetFunctions.PlotDataUDFs','StackHomogeneousTables','PlotDataUDFsReports38','StackHomogeneousTablesPrepared'),
        ('BESHStatNG.UdfDataImport','TryGetRocInput','UdfPlotDataPreparation38','CompleteRocInputPrepared'),
        ('BESHStatNG.UdfDataImport','TryGetSurvivalRecords','UdfPlotDataPreparation38','CompleteSurvivalRecordsPrepared')]:
        current=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==(nativeowner,name))
        old=next(m for m in previous['methods'] if (m['declaring_type'],m['member_name'])==(nativeowner,name))
        portable=next(m for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.'+owner,api))
        assert len(copies(old))==len(copies(current))+len(copies(portable)),('Object copy boundary changed',name,copies(old),copies(current),copies(portable))
        named=[defs[x['operand'][7:]] for x in current['instructions'] if x['op']=='ldftn' and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in defs]
        boundaries.append(dict(native=current,previous_native=old,portable=portable,previous_copy_sites=copies(old),native_copy_sites=copies(current),portable_copy_sites=copies(portable),
            actual_direct_policy_targets=named,exception_policy='Actual direct typed delegates. Native acquisition, throws/catches and Err/logging remain; no DynamicInvoke or new wrapping Catch. Extra Common stack frames and typed enumerable iteration are explicitly recorded, not claimed to have identical instruction layout.'))
"""
value=value[:start]+replacement+value[stop:]
value=value.replace('Three source-declared handles','All seven source-declared handles').replace('five exact additive Object-copy boundaries','eight exact additive Object-copy boundaries')
target=HERE/'host_contexts_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('host-context-tool-crosswalk-v1.json',dict(before=pin(source),after=pin(target),scope='All seven unchanged original handles, fields/accessors/constructors/writers and actual C13 eight-body copy/delegate boundaries. No new handle support claim or original execution.',original_queries=0))
