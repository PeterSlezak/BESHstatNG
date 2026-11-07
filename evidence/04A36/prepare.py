"""Bounded source census and immutable pre-edit receipt preparation."""
from work import *
if __name__=='__main__':
    folder=HERE/'helpers';folder.mkdir(exist_ok=True)
    project=(ROOT/'evidence/04A35/helpers/Source35.csproj').read_text().replace('Source35','Source36')
    (folder/'Source36.csproj').write_text(project,encoding='utf-8')
    program=(ROOT/'evidence/04A35/helpers/Program.cs').read_text()
    program=program.replace('evidence/04A35/','evidence/04A36/').replace('"MultipleComparisonPlotExcel","AgreementPlotExcel","PsmLovePlotExcel"','"KaplanMeierPlotExcel","CoxSurvivalPlotExcel","SpcControlChartExcel"').replace('here+"entry-source/"+f+".vb.txt"','here+"entry-source/BESHstatNG/src/Graphics/"+f+".vb.txt"')
    begin=program.index('var supporting=');end=program.index('var current=',begin)
    program=program[:begin]+'''var supporting=Array.Empty<object>();
var paths=files.Select(f=>"BESHstatNG/src/Graphics/"+f+".vb").Concat(Directory.GetFiles("BESHStat.Core/src/Graphics","*36.vb")).Concat(Directory.GetFiles("BESHStat.Excel.Common/src/Analysis","*36.vb")).Concat(Directory.GetFiles("BESHStat.Excel.Common/src/Results","*36.vb")).Select(p=>p.Replace('\\\\','/')).ToArray();
'''+program[end:]
    program=program.replace('evidence/04A34/historic-census-metadata.json','evidence/04A35/historic-census-metadata.json')
    (folder/'Program.cs').write_text(program,encoding='utf-8')
    # Redact historical workstation identifiers in public path labels; digest
    # and byte counts remain exact, with reversible mapping only in local Git.
    pins=json.loads((HERE/'entry-tracked-pins.json').read_bytes())
    for r in pins['files']:
        if '/TestResults/' in r['path'] or r['path'].startswith('TestResults/'):
            r['path']='<PRIVATE_PROTECTED_PATH:'+sha(r['path'].encode())[:24]+'>'
    save('entry-tracked-pins.json',pins)
    source_checks=[]
    for n in ['13','21','22','23','24','29','34','35']:
        dossier=ROOT/('docs/architecture/batch-04A-'+n+'-owner-review.md')
        if dossier.is_file():source_checks.append(pin(dossier.relative_to(ROOT).as_posix()))
    extras=['evidence/04A13/issues.json','evidence/04A13/restriction-crosswalk.json','evidence/04A21/spc-phase-stage-contracts-v1.json','evidence/04A21/spc-exclusion-rule-limit-contracts-v1.json','evidence/04A21/original-control-crosswalk.json','evidence/04A22/original-control-crosswalk.json','evidence/04A23/original-control-crosswalk.json','evidence/04A24/original-control-crosswalk.json','evidence/04A29/graphics-family-casebook-v1.json','evidence/04A29/graphics-lineage-operation-evidence-v2.json','evidence/04A29/graphics-existing-core-crosswalk-v1.json','evidence/04A34/family-ownership-map.csv','evidence/04A34/coordinate-and-chart-spec-contract.json','evidence/04A35/member-ownership.csv','docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json']
    for p in extras:
        if (ROOT/p).is_file():source_checks.append(pin(p))
    save('authority-reading.json',dict(files=source_checks,decision='Current sources and accepted payloads inspected; stale Master Plan progress is superseded by explicit owner acceptance. Objectives/charter retained.'))
    frozen=['Legacy36.vb','LegacyPresentation36.vb','Cases36.vb','Cases36-v1.vb.txt','Cases36-v2.vb.txt','Cases36-v3.vb.txt','pre-edit-capture.csv','pre-edit-Release.csv','pre-edit-presentation.csv']
    save('pre-edit-capture-authority.json',dict(files=[pin('evidence/04A36/'+p) for p in frozen],production_unchanged=not bool(git('diff','--name-only')),rows_per_configuration=5472,presentation_rows=384,authority='PRE_EDIT_LEGACY_EXECUTED / ACCEPTED_CORE',scientific='NOT_GOLDEN',fixture_corrections=[dict(version=1,reason='Synthetic lower-two-sigma=0 exceeded lower-one-sigma=-1; Core constructor correctly rejected it before capture.'),dict(version=2,reason='Synthetic Infinity statistic is rejected by Core before renderer.'),dict(version=3,reason='Synthetic LCL=5 exceeded center=2; Core constructor correctly rejected it.')],original_successful_capture='pre-edit-capture.csv',qualification='Failed fixture attempts produced no successful corpus; original inputs retained as versioned .txt sources. First successful outputs never overwritten.'))
    print('Source reader prepared; pre-edit authority sealed')
