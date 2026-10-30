"""Run an unchanged Common test assembly in a unique, fully copied sandbox.

Assembly-relative ../../../../TestResults writers resolve below the sandbox,
independently of TRX routing. Never execute the repository-bin assembly here.
"""
import argparse,json,pathlib,shutil,subprocess,sys
import owner_closeout as o
R=o.R;H=o.H;c=o.c
def routes(base,sandbox):
    base=base.resolve();sandbox=sandbox.resolve()
    outputs=[(base/'../../../../TestResults/Codex'/p).resolve() for p in ('Batch0205/Wire','Batch0207')]
    if not all(p.is_relative_to(sandbox) for p in outputs):raise ValueError('Writer escapes isolated sandbox')
    if not sandbox.is_relative_to(H.resolve()):raise ValueError('Sandbox is outside unique closeout evidence')
    return outputs
def run(name,config,results,focused=False):
    before=o.read(H/'writer-baseline.json');assert not o.metadata_errors(before)
    sandbox=H/'executions'/name/'sandbox';base=sandbox/'Common/bin'/config/'net10.0';assert not sandbox.exists(), 'Execution names must be unique'
    outputs=routes(base,sandbox);src=R/'BESHStat.Excel.Common.Tests/bin'/config/'net10.0'
    assert (src/'BESHStat.Excel.Common.Tests.dll').is_file()
    shutil.copytree(src,base)
    copied=[dict(original=c.pin(p),copy=c.pin(base/p.relative_to(src))) for p in sorted(src.rglob('*')) if p.is_file()]
    assert all(e['original']['sha256']==e['copy']['sha256'] for e in copied)
    assert (base/'TestData/OfficeOutput/anova-base.json').is_file() and (base/'TestData/CompositeOutput/composite-c10.json').is_file()
    args=['dotnet','vstest',str(base/'BESHStat.Excel.Common.Tests.dll'),'/Logger:trx;LogFileName=Common.trx','/ResultsDirectory:'+str(results)]
    if focused:args+=['/TestCaseFilter:FullyQualifiedName~OfficeOutputPlanAdapterTests|FullyQualifiedName~CompositeOutputTests']
    result=subprocess.run(args,cwd=R)
    assert not o.metadata_errors(before),'Historical internal writer metadata changed'
    actual=[c.pin(p) for d in outputs for p in sorted(d.rglob('*.json'))]
    assert len(actual)==11,('Both internal writers must execute all11 fixtures',len(actual))
    expected={e['path']:e for e in o.read(H/'history-hashes.json')}
    for e in before:assert c.sha(R/e['path'])==expected[e['path']]['sha256']
    record=dict(status='PASS' if result.returncode==0 else 'FAIL',exit_code=result.returncode,command=args,assembly_base=base.relative_to(R).as_posix(),resolved_internal_output_roots=[p.relative_to(R).as_posix() for p in outputs],copied_dependencies_and_testdata=copied,isolated_internal_outputs=actual,historical_metadata_before=before,historical_metadata_after=[o.metadata(R/e['path']) for e in before],historical_hashes=[c.pin(R/e['path']) for e in before],product_test_source_modified=False)
    o.emit(H/('isolation-'+name+'.json'),record)
    return result.returncode
if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--name',required=True);p.add_argument('--configuration',choices=['Debug','Release'],required=True);p.add_argument('--results',required=True);p.add_argument('--focused',action='store_true');a=p.parse_args()
    sys.exit(run(a.name,a.configuration,pathlib.Path(a.results),a.focused))
