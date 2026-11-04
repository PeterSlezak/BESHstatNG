"""04A.17 source-derived audit, immutable lineage and successor pins. No native execution."""
import argparse
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'TestResults/04A-17-20261104-run01'
OUT = ROOT / 'evidence/04A17'
BASE = '13c6364f63fb8d4a254e5759e871896caf29cd7b'
FORM = 'BESHstatNG/src/UI/Ui11PCA.vb'
PAGES = ['BESHStatNG_Help_MkDocs/docs/methods/' + x + '.md' for x in ['principal-component-analysis', 'correspondence-analysis', 'multiple-correspondence-analysis', 'k-means-clustering', 'hierarchical-clustering', 'factor-analysis', 'discriminant-analysis']]
CHANGED = {FORM, 'BESHstatNG/BESHStatNG.vbproj', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj'} | set(PAGES)
spec = importlib.util.spec_from_file_location('contract14', ROOT / 'scripts/capture-04a14-contract.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def git(*args):
    return prior.git(*args)


def original(path):
    return subprocess.check_output(['git', '-c', 'safe.directory=' + ROOT.as_posix(), 'show', BASE + ':' + path], cwd=ROOT).decode('utf-8-sig').replace('\r\n', '\n')


def members(path, text):
    return prior.get_members(path, text)


def write(name, value):
    (OUT / name).write_text(json.dumps(prior.sanitize(value), indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf-8', newline='\n')


def audit():
    assert git('branch', '--show-current') == 'codex/04A-17'
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert not git('diff', '--name-only') and not git('diff', '--cached', '--name-only')
    paths = [FORM, 'BESHstatNG/src/UI/Ui11PCA.Designer.vb', 'BESHstatNG/src/UI/Ui11PCA.resx', 'BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb',
             'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb', 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMultivariate.vb',
             'BESHstatNG/src/DM/ExcelDnaDataImporter.vb', 'BESHstatNG/src/DM/DataObjContracts.vb', 'BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb'] + PAGES
    paths += [p.relative_to(ROOT).as_posix() for p in (ROOT / 'BESHStat.Core/src/RegModels/Multivariate').glob('*.vb')]
    paths += ['BESHstatNG/src/Graphics/' + n + '.vb' for n in ['PcaPlotExcel', 'CorrespondenceAnalysisPlotExcel', 'ClusterAnalysisPlotExcel', 'FactorAnalysisPlotExcel']]
    records = {}
    for path in paths:
        b = (ROOT / path).read_bytes()
        text = b.decode('utf-8-sig').replace('\r\n', '\n')
        methods = members(path, text) if path.endswith('.vb') and not path.endswith('.Designer.vb') else {}
        records[path] = dict(sha256=sha(ROOT / path), bytes=len(b), encoding='UTF-8 BOM' if b.startswith(b'\xef\xbb\xbf') else 'UTF-8',
            crlf=b.count(b'\r\n'), bare_lf=b.count(b'\n')-b.count(b'\r\n'), final_newline=b.endswith(b'\n'),
            methods={n: dict(sha256=hashlib.sha256(t.encode()).hexdigest(), first_line=text[:text.index(t)].count('\n')+1) for n,t in methods.items()})
    write('prechange-method-inventory.json', dict(base=BASE, files=records))
    write('implementation-plan.json', dict(scope='04A.17 only; created before production edits',
        routes={n: dict(input='Host acquisition remains distinct; detached arrays/provenance after acquisition',
            options='Source-derived Common compatibility projection and typed family options',
            core='Original data/settings and Fit/Calculate order; no estimator changes',
            report='Core wrapResults -> detached Common document -> original native writer',
            plots=p) for n,p in [('RunPCA','Scree, scores2D, loadings2D, biplot(0,0.5,1), scores3D, loadings3D'),
                ('RunMCA','Correspondence, column contributions axes 0 then 1'),('RunKmeans','No GUI chart; row-label/membership report intent'),
                ('RunHierarchicalClustering','Optional original dendrogram settings'),('RunFA','Scree, loading2D, loading3D'),('RunDiscriminantAnalysis','No GUI chart; classification report intent')]},
        other={'RunSPM':'Unchanged', 'standaloneCA':'Shared portable intent only; native RunCA unchanged, no-label j-1 defect retained for 04A.18/40-41'},
        files='Common multivariate policy/input/report files, one Windows adapter, Ui11PCA, explicit project Compile entries, new tests, seven audited Help pages, scripts/evidence/dossier',
        boundaries='Original validation precedence/messages, native workbook/sheet/renderer call order, Excel parsing and COM remain Windows; no stricter candidate validation activated',
        tolerance='max(1e-9, abs(expected)*1e-9) for finite doubles; exact labels/types/order/shapes/metadata and nonfinite classes; fixed before comparisons. Fixed seeds only for exact stochastic comparisons; no sign/tied-subspace normalization in production.'))
    rows = list(csv.DictReader((ROOT / prior.CONTROLS).read_text(encoding='utf-8-sig').splitlines()))
    ledger = json.loads((ROOT / prior.LEDGER).read_bytes())
    relevant = [r for r in rows if r['path'].split('/')[-1] in ['Ui11PCA.Designer.vb', 'Ui0OneRefeditMulticol.Designer.vb']]
    write('original-control-crosswalk.json', dict(authorities={p:dict(sha256=sha(ROOT / p),bytes=(ROOT / p).stat().st_size) for p in [prior.CONTROLS,prior.LEDGER]},
        controls=[dict(control_id=r['control_id'], path=r['path'], state=r['state'], source_row_sha256=prior.digest(r)) for r in relevant],
        obligations=[dict(obligation_id=r['obligation_id'], linked_control_ids=r.get('linked_control_ids',[]), source_record_sha256=r.get('source_record_sha256'), original_row_sha256=prior.digest(r), status='OPEN', capture_authorized=r.get('capture_authorized'), affected_live_golden_blocked=r.get('affected_live_golden_blocked')) for r in ledger if set(r.get('linked_control_ids',[])) & {v['control_id'] for v in relevant}],
        original_case='04A02-PCA-NATIVE-01', native='NOT_RUN', historical_PCA_populations=dict(blocked_controls=75,deferred_obligations=76),
        interpretation='Original ledger populations overlap. Current source/control declarations are reconciled separately; no inferred release or obligation discharge.', releases=0,discharges=0,P='NO',N1='NO GO',N2='NO',C2='UNMET'))
    print('Prechange audit',len(records),'source files;',len(relevant),'original control records',flush=True)


def source():
    assert git('branch','--show-current') == 'codex/04A-17'
    assert git('rev-parse','HEAD') == git('rev-parse','main') == BASE
    assert not git('diff','--cached','--name-only')
    changed = set(git('diff','--name-only').splitlines())
    assert changed <= CHANGED, changed-CHANGED
    pins = json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    for path,h in pins.items():
        if path not in changed: assert sha(ROOT/path)==h,path
    old = members(FORM,original(FORM)); new=members(FORM,(ROOT/FORM).read_text(encoding='utf-8-sig').replace('\r\n','\n'))
    methods={n:dict(before=hashlib.sha256(t.encode()).hexdigest(),after=hashlib.sha256(new[n].encode()).hexdigest()) for n,t in old.items() if t!=new[n]}
    assert old['RunSPM']==new['RunSPM'] and old['btCalculate_Click']==new['btCalculate_Click']
    write('source-contract.json',dict(base=BASE,protected_initial_tracked_count=len(pins)-len(changed),
        private_initial_source_map=dict(alias='LOCAL_BASELINE:initial-tracked-map',sha256=sha(RUN/'baseline.json'),bytes=(RUN/'baseline.json').stat().st_size),
        changed={p:dict(before=pins[p],after=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(changed)},
        new_sources={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in git('ls-files','--others','--exclude-standard').splitlines() if p.endswith(('.vb','.py','.vbproj'))},
        changed_form_methods=methods,unchanged_form_methods=sorted(set(old)-set(methods)), index_empty=True,Core_UDF_Designers_Renderers_unchanged=True))
    retained={p:dict(sha256=h,bytes=(ROOT/p).stat().st_size) for p,h in pins.items() if p.startswith(('evidence/04A14/','evidence/04A15/','evidence/04A16/'))}
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    assert len(registry['files']) == 100
    for p,pin in registry['files'].items():
        assert sha(ROOT/p)==pin['sha256'] and (ROOT/p).stat().st_size==pin['bytes'],p
    crosswalk=json.loads((ROOT/'evidence/04A14/restriction-crosswalk.json').read_bytes())
    for p,h in crosswalk['authorities'].items(): assert sha(ROOT/p)==h,p
    fit15=json.loads((ROOT/'evidence/04A15/fit-comparisons-v1.json').read_bytes())
    write('retained-evidence.json',dict(files=retained,registry_sha256=sha(ROOT/'evidence/04A14/baseline-registry.json'),
        captures=registry['files'],original_authorities=crosswalk['authorities'],batch15_records=len(fit15['records']),batch15_statuses=fit15['statuses'],
        mixed_lineage='177 controls / 69 OPEN obligations / 67 restricted; zero releases/discharges',scientific='P NO / N1 NO GO / N2 NO / Gate C2 UNMET'))
    write('documentation-impact.json',{p:dict(sha256=sha(ROOT/p),disposition='Same-batch editorial clarification of existing Windows input/label/seed/prior/plot behavior and Office unavailability; no new statistical/GUI feature.') for p in PAGES})
    print('PASS protected initial tracked bytes, SPM/dispatch and exact changed-source inventory')


if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['audit','source']);globals()[p.parse_args().action]()
