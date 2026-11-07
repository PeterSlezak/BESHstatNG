"""Finalize source ownership, immutable entry-corpus and explicit API inventory."""
from work import *
P=HERE.parent
paths=git('diff','--name-only').splitlines()+git('ls-files','--others','--exclude-standard').splitlines()
sources=sorted({p for p in paths if not p.startswith('evidence/') and (p.startswith(('BESHstatNG/src/Graphics/','BESHStat.Core/','BESHStat.Excel.Common/','BESHStat.Core.Tests/','BESHStat.Excel.Common.Tests/','BESHStatNG.Test/')) or p=='docs/architecture/batch-04A-34-owner-review.md')})
assert all((ROOT/p).is_file() for p in sources)
p=P/'verify_manifest.py';s=p.read_text(encoding='utf-8');a=s.index('SOURCES=');b=s.index('\ndef expected_scope',a)
s=s[:a]+'SOURCES='+repr(sources)+'\n'+s[b:]
s=s.replace("v['decision']=='NOT_READY_04A34'","v['decision']=='READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW'").replace('null self; NOT_READY retained','null self; software review candidate only')
write(p.relative_to(ROOT),s)
snapshot={r['path']:r for r in json.loads((HERE/'entry.json').read_bytes())['files']}
frozen=[]
for p in sorted((ROOT/'evidence/04A34').glob('pre-edit-*'))+list((ROOT/'evidence/04A34').glob('post-edit-compatibility-capture.csv')):
    key=p.relative_to(ROOT).as_posix();a=snapshot[key];b=pin(key);assert a==b,(a,b)
    frozen.append(b)
save('frozen-corpus-preservation.json',dict(status='PASS',files=frozen,root_corpora_unchanged=True,qualification='Exact bytes and digests from continuation entry; original numerical body/source hashes retained. Supplemental continuation goldens were executed before corresponding new methods were installed.'))
d=json.loads((HERE/'member-consumption.json').read_bytes());nodes=json.loads((HERE/'member-source.json').read_bytes())['current']
contracts=[]
for path in sources:
    if not path.startswith(('BESHStat.Core/','BESHStat.Excel.Common/')):continue
    ns=[n for n in nodes if n['path']==path]
    edges=[e for r in d['members'] for e in r['portable_edges'] if e['target_path']==path]
    # DTO getters are reached through the producing function; entry accepted
    # types also retain their original direct Windows references.
    assert edges,path
    contracts.append(dict(source=pin(path),public_headers=sorted({n['signature'] for n in ns if n['signature'].startswith('Public ')}),windows_consumption=sorted({e['site'] for e in edges}),call_targets=sorted({e['target'] for e in edges}),authority='PRE_EDIT_LEGACY and accepted engines; same-Core identity vectors labelled separately; HAND_DERIVED_INVARIANT where analytical; NOT_GOLDEN',shape='Readonly scalar/axis/label/legend intent or defensive-copy vectors; input/source ordinals are zero-based and independent of one-based Excel Points. No physical worksheet row invented.',native='SOURCE_ONLY for mixed COM/shape path; actual pure Windows preparation invoked in Framework tests; native measurements/format/lifetime remain host'))
old=json.loads((HERE/'checkpoint/coordinate-and-chart-spec-contract.json').read_bytes())
shapes=old['shape_and_identity'];shapes['bullet']='Explicit-size scale/band/bar/target rectangles, labels, tick geometry and palette-index/format intent; original clip/admission/defaults; RGB/font/shape ownership remains Windows.'
shapes['sankey']='Existing flow solver unchanged. Explicit-size ordered node/ribbon cubic anchors, stage/legend/label geometry; existing basis/culture formatting; native text measurement, RGB/freeform/resize/cleanup remain Windows.'
shapes['dumbbell']='Accepted paired categories/auxiliary SourceIndex vectors; connectors preserve orientation and empty admission, axis125/tick rules and numeric labels; native date interpretation/pixel offset/styles unchanged.'
shapes['cdf']='Accepted empirical/fitted/percentile vectors and defaults; portable axes/formats/legend branch intent and three-point L references. Invalid/no-extension alias behavior preserved in the Windows private wrapper.'
shapes['ladder']='Stable source/input ordinal and tie order, endpoint/side/label vectors and text; explicit acquired plot height/separation; original spreading/clamping reused. HashSet group legend state and physical acquisition stay Windows.'
write('evidence/04A34/coordinate-and-chart-spec-contract.json',json.dumps(dict(schema='04A34-contract-v2',status='COMPLETE_04A34_FOUNDATION',contracts=contracts,shape_and_identity=shapes,scientific='NOT_GOLDEN',physical_worksheet_rows='NOT_AVAILABLE for values-only arrays',public_windows_api='Merged-main public headers preserved; only explicitly mapped private duplicate/relocated helpers removed; see source-ownership.json',exclusions=old['exclusions']),ensure_ascii=False,indent=2)+'\n')
v=json.loads((P/'deferred-issues.json').read_bytes());v['open_batch_work']='No unresolved04A34 candidate; specialized35/36/37 reservations and existing defect/native/scientific gates remain. Canonical29 uncertainties171/172 unchanged.'
write('evidence/04A34/deferred-issues.json',json.dumps(v,ensure_ascii=False,indent=2)+'\n')
print('Inventoried',len(sources),'source/test/project/dossier paths;',len(contracts),'portable source contracts; frozen corpus preserved')
