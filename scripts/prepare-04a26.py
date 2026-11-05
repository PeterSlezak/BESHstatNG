"""Entry qualification and source map. Prior receipts are hash-verified, never rewritten."""
import csv
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import sys
s=importlib.util.spec_from_file_location('c26',Path(__file__).with_name('capture-04a26-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
ROOT,RUN,OUT=c.ROOT,c.RUN,c.OUT
FORMS=[c.FORM,'BESHstatNG/src/UI/UiTwoInputRefedits.vb','BESHstatNG/src/UI/Ui9ANOVA2nested.vb','BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb']
def prepare():
    qualification=json.loads((ROOT/'TestResults/04A-25/run01/builds/delivery-build-Debug.json').read_bytes())['source_pins']
    differences=[]
    for p,h in qualification.items():
        actual=c.sha(ROOT/p)
        if h!=actual:
            b=(ROOT/p).read_bytes().replace(b'\r\n',b'\n')
            line_only=h in {hashlib.sha256(v).hexdigest() for v in [b,b.replace(b'\n',b'\r\n')]}
            differences.append(dict(path=p,prior=h,actual=actual,classification='LINE_ENDINGS_ONLY' if line_only else 'SEMANTIC_CHANGE_REQUIRES_FRESH_BASELINE'))
    c.emit('entry-source-qualification.json',dict(previous_final_build_sha256=c.sha(ROOT/'TestResults/04A-25/run01/builds/delivery-build-Debug.json'),compiled_members=len(qualification),differences=differences,exact=not differences,policy='Reuse only exact immutable managed/Node receipts with unchanged source/test inputs and verified TRX hashes; current-branch Python measured separately. No historical totals assumed.'))
    print('Previous compiled qualification:',len(qualification),'members;',len(differences),'differences')
    files={}
    for p in FORMS:
        text=(RUN/'preimages'/Path(p).name).read_text(encoding='utf-8-sig');methods=c.methods(text)
        files[p]=dict(sha256=c.sha(RUN/'preimages'/Path(p).name),method_sha256={n:hashlib.sha256(b.encode()).hexdigest() for n,b in methods.items()},methods={n:b for n,b in methods.items() if n in ['RunPTtest','RunUPTtest','RunSingleProp','RunIndependentProp','RunLogRank','RunCox','RunICC','RunBlandAltman','RunDeming','RunLinsCCC','RunCohensKappa','CheckData','getData','getPairedCategoricalData']})
    c.emit('prechange-method-provenance.json',dict(files=files,hashing='Source bytes and normalized-LF UTF-8 member bodies; actual merged main, not archive reconstruction.'))
    control_path='docs/architecture/WindowsMigrationLedger/ui-controls.csv';ledger_path='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json';native_path='docs/architecture/04A02/native-restriction-crosswalk-v1.json'
    controls=list(csv.DictReader((ROOT/control_path).read_text(encoding='utf-8-sig').splitlines()));ledger=json.loads((ROOT/ledger_path).read_bytes())
    names=[Path(p).stem for p in FORMS];selected=[r for r in controls if Path(r['path']).stem in [n+'.Designer' for n in names]];ids={r['control_id'] for r in selected};obligations=[r for r in ledger if set(r.get('linked_control_ids',[]))&ids]
    digest=lambda value:hashlib.sha256(json.dumps(value,sort_keys=True,ensure_ascii=False,separators=(',',':')).encode()).hexdigest()
    populations={name:dict(control_ids=sorted(r['control_id'] for r in selected if Path(r['path']).stem==name+'.Designer'),obligation_ids=sorted(r['obligation_id'] for r in obligations if any(name in v for v in r.get('linked_control_ids',[])))) for name in names}
    c.emit('original-control-crosswalk.json',dict(authorities={p:c.sha(ROOT/p) for p in [control_path,ledger_path,native_path]},populations=populations,controls=[dict(id=r['control_id'],sha256=digest(r)) for r in selected],obligations=[dict(id=r['obligation_id'],sha256=digest(r),linked_controls=r.get('linked_control_ids',[]),status='OPEN') for r in obligations],counts=dict(host_controls=len(ids),linked_open_obligations=len(obligations),overall_controls=len(controls),overall_obligations=len(ledger),original_source_members=3866),overlap='Host-wide populations include unrelated form modes; source-backed linked obligations overlap and are never added as disjoint method populations. No native release inferred.',native_crosswalk_sha256=c.sha(ROOT/native_path),releases=0,discharges=0,gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET')))
    print('Host controls',len(ids),'linked OPEN obligations',len(obligations))
    c.emit('implementation-plan.json',dict(modified=FORMS+['BESHStatNG.Test/BESHStatNG.Tests.vbproj'],new=['Focused planning request/text and agreement option/axis/report policies','26 managed/adapter/Python tests, isolated helpers/evidence, one dossier'],protected='Core, paired/importer policies, graphics, UDF/Designer/Ribbon, frameworks/packages, all unrelated form members and every prior evidence/capture/test.',assurance='No separate solver or GUI found by live Core/Common/Windows source search. Existing achieved ICC power and LoA precision metadata retained.'))
if __name__=='__main__':prepare()
