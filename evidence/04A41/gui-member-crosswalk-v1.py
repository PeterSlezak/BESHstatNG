from common41 import *
import re
lock();matrix=load(HERE/'scope/ownership-parity-v1.json')['function_matrix']
def member(path,name):
    p=ROOT/path;text=p.read_text(encoding='utf-8-sig');lines=text.splitlines(keepends=True)
    for i,line in enumerate(lines):
        match=re.match(r'([ \t]*)(?:(?:Private|Public|Friend|Protected)\s+)?(?:Shared\s+)?(Sub|Function)\s+'+re.escape(name)+r'\b',line)
        if not match:continue
        end=next(j for j in range(i+1,len(lines)) if re.match(r'^'+re.escape(match[1])+r'End '+match[2]+r'\b',lines[j]))
        body=''.join(lines[i:end+1])
        return dict(source=pin(path),method=name,lines=[i+1,end+1],method_text_sha256=sha(body.encode()),Common_types_or_calls=sorted(set(re.findall(r'\b(?:[A-Za-z]+(?:Policy|Request|Options|Selection|Adapter|ApplicationService|ReportPlan))\b',body))),Core_types_or_calls=sorted(set(re.findall(r'(?i)\b(?:assumptions|parametric|nonparametric|contingencytable|Agreement|SampleSizeCalculator)\.[A-Za-z0-9_]+',body))),native_operations='Me/control state, acquisition, progress and writer operations remain native and unexecuted.')
    raise AssertionError('MEMBER_NOT_FOUND:'+path+':'+name)
uiby='BESHstatNG/src/UI/UibyID.vb';two='BESHstatNG/src/UI/UiTwoInputRefedits.vb';multi='BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb';nested='BESHstatNG/src/UI/Ui9ANOVA2nested.vb';sizing='BESHstatNG/src/UI/Ui12SampleSizeTtestSingleProp.vb';ct='BESHstatNG/src/UI/Ui82x2.vb';props='BESHstatNG/src/UI/Ui8Proportions.vb'
def gui_functions(row):
    name=row['function'];module=row['module']
    if module=='ParametricUDFs':
        if name.startswith('RMANOVA'):return [(multi,'Run1WayRmANOVA')]
        if name=='ANOVA2_NESTED':return [(nested,'Run2WayNested')]
        if name.startswith('ANOVA1'):return [(uiby,'RunOneWayANOVA'),(uiby,'RunOneWayANOVAThroughSharedLayer')]
        return [(two,'RunPairedTtest')] if name=='TTEST_PAIRED' else [(uiby,'RunTtest')]
    if module=='NonparametricUDFs':
        if name.startswith('MW_'):return [(uiby,'RunMannWhitney')]
        if name.startswith('WILCOX_'):return [(two,'RunWilcoxon')]
        if name.startswith('SPEARMAN'):return [(two,'RunSpearman')]
        if name.startswith('KENDALL'):return [(two,'RunKendall')]
        if name.startswith('KW_'):return [(uiby,'RunKruskallWalis')]
        return [(multi,'RunFriedman')]
    if module=='AssumptionsUDFs':
        if name=='BOX_M':return [(two,'RunHotteling')]
        if name=='MAUCHLY':return [(multi,'Run1WayRmANOVA')]
        if name in ['SHAPIRO_WILK','DAGOSTINO_PEARSON','ANDERSON_DARLING']:return [(uiby,'RunNormality')]
        if name=='SYMMETRY':return [(uiby,'RunSymmetry')]
        if name in ['GRUBBS','ROSNER']:return [(uiby,'RunOutliers')]
        return [(uiby,'RunHomogeneityVar')]
    if module=='ContingencyTableUDFs':
        if name=='CT_MANTEL_HAENSZEL':return [(multi,'RunMantelHaenszel')]
        if name in ['CT_TREND','CT_ORDINAL_ASSOC','CT_FFH_EXACT']:return [(multi,'RunRxC')]
        if 'PROPORTION' in name:return [(props,'btCompute_Click')]
        return [(ct,'btCompute_Click'),(multi,'RunRxC')] if name in ['CT_CHI2','CT_NOMINAL_ASSOC'] else [(ct,'btCompute_Click')]
    if module=='AgreementMethodUDFs':
        if name.startswith('PASSINGBABLOK'):return [(nested,'RunPassingBablok')]
        if name.startswith('DEMING'):return [(two,'RunDeming')]
        if name.startswith('BLANDALTMAN'):return [(nested,'RunBlandAltman')]
        if name.startswith('LINCCC'):return [(two,'RunLinsCCC')]
        if name.startswith('KAPPA'):return [(two,'RunCohensKappa')]
        return [(multi,'RunICC')]
    if name=='SSIZE_LOGRANK':return [(sizing,'RunLogRank')]
    if name.startswith('SSIZE_COX'):return [(sizing,'RunCox')]
    return [(sizing,{'SSIZE_ICC':'RunICC','SSIZE_BLANDALTMAN':'RunBlandAltman','SSIZE_PROP_SINGLE':'RunSingleProp','SSIZE_PROP_INDEP':'RunIndependentProp','SSIZE_TTEST_PAIRED':'RunPTtest','SSIZE_TTEST_UNPAIRED':'RunUPTtest'}[name])]
rows=[]
for row in matrix:
    mapping=[member(p,n) for p,n in gui_functions(row)]
    differences=[]
    if row['function']=='BOX_M':differences.append('GUI BoxM is embedded in the two-sample Hotelling workflow; UDF BoxM is a standalone grouped matrix interface, not an identical GUI request.')
    if row['function']=='MAUCHLY':differences.append('GUI Mauchly is selected within repeated ANOVA; standalone UDF matrix guards and result shape differ.')
    if row['function'] in ['SHAPIRO_WILK','DAGOSTINO_PEARSON','ANDERSON_DARLING']:differences.append('GUI eligibility/composed report error order differs from standalone UDF eligibility and error result; n=5000 NA eligibility does not prove a GUI report was written before DAgostino overflow.')
    if row['module']=='SampleSizeUDFs':differences.append('GUI spinner/default/text/allocation and optionality differ from UDF explicit arguments/metric spill; C26 decisions remain restricted.')
    if row['module']=='AgreementMethodUDFs':differences.append('GUI selection precedence, resampling minimum/spinners and native progress remain distinct from UDF parsed options; fitted numerical model is Core-owned.')
    if 'PAIRED_PROPORTIONS' in row['function']:differences.append('Current accepted05 corrected CoreLiddellTable dispatch is retained. Separate2x2 Liddell Risk ratio label is still restricted, not silently changed.')
    rows.append(dict(excel_name=row['excel_name'],function=row['function'],Windows_UDF=dict(source=row['after_source'],lines=row['after_lines'],method_sha256=row['after_method_sha256']),GUI_members=mapping,Common_calls_or_types=row['Common_calls_or_types'],Core_direct_calls_or_types=row['Core_direct_calls_or_types'],specific_divergences=differences,parity_limit='Read-only source correspondence, not native UI verification or equivalence of unlike host requests. Existing39 row/acquisition policy remains authoritative.',decision='BEHAVIOR_PRESERVING_IMPLEMENTED' if row['parent_changed'] else 'EXISTING_SHARED_PATH_RETAINED_WITHOUT_CHURN'))
assert len(rows)==76
save('scope/gui-current-member-crosswalk-v1.json',dict(functions=rows,scoped_exports=76,GUI_source_methods_authenticated=True,GUI_source_changed=False,native_executed=False,all_related_files_unmodified=True,gates=GATES))
print('76 function-to-actual-GUI-member source hashes/lines mapped; unlike interfaces explicitly separated',flush=True)
