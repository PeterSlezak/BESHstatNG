"""Complete documented compatibility shims and remove superseded numeric loops."""
from work import *
from freeze import method
if __name__=='__main__':
    p='BESHstatNG/src/Graphics/SpcControlChartExcel.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    for name in ['IncludeValuesInBounds','IncludeNullableInBounds','HasAnyValue','IsNumericChartValue','IsFinite']:
        s=s.replace(method(s,name),'')
    write(p,s)
    p='BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb';s=(ROOT/p).read_text(encoding='utf-8-sig')
    needle='SurvivalChartPresentation36.SeriesName(plotData, groupIndex, KaplanMeierSeriesRole36.UpperConfidence)';pos=s.rfind(needle);assert pos>=0
    s=s[:pos]+s[pos:].replace(needle,needle.replace('UpperConfidence','LowerConfidence'),1);write(p,s)
    source='evidence/04A36/LegacyPresentation36V2.vb'
    write('BESHStat.Excel.Common.Tests/src/LegacyPresentation36V2.vb',(ROOT/source).read_text())
    for p in ['BESHStat.Excel.Common.Tests/src/Graphics36PresentationTests.vb','BESHStatNG.Test/src/Charts/Graphics36AdapterTests.vb']:
        s=(ROOT/p).read_text(encoding='utf-8-sig').replace('OriginalPresentation36.','OriginalPresentation36V2.')
        write(p,s)
    p='BESHStatNG.Test/BESHStatNG.Tests.vbproj';s=(ROOT/p).read_text(encoding='utf-8-sig')
    anchor='    <Compile Include="src\\Charts\\Graphics36AdapterTests.vb" />'
    assert anchor in s
    s=s.replace(anchor,'    <Compile Include="..\\BESHStat.Excel.Common.Tests\\src\\LegacyPresentation36V2.vb"><Link>Graphics36\\LegacyPresentation36V2.vb</Link></Compile>\n'+anchor);write(p,s)
    plan='planning/BESHStatNG_CrossPlatform_MasterPlan.md'
    save('external-owner-plan-change.json',dict(path=plan,current=pin(plan),entry=load if False else json.loads((HERE/'pre-edit-source-hashes.json').read_bytes())['sources'][plan],disposition='EXTERNAL_OWNER_MODIFICATION_PRESERVED',reason='Owner-managed Revision2.13 appeared during execution. No36 script wrote this file; concise36 objective and restrictions inspected. Excluded from proposed36 source/publication changes; no rollback or rewrite.',git_diff_sha256=sha(git('diff','--',plan).encode())))
    print('Retained v1 fixtures; tests use corrected v2; superseded loops removed; owner plan preserved')
