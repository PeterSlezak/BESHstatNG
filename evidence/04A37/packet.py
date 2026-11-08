"""Publication proposal producer. The separate verifier is the independent reader."""
from pathlib import Path
import json
import re
import sys
from run import m, ROOT
from summarize import clean
import verify_packet as reader

HERE=Path(__file__).resolve().parent
RUN=ROOT/'.test-rationalization-04A37.tmp'


def save(name,value):(HERE/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
def load(name):return json.loads((HERE/name).read_bytes())


def receipts():
    data=load('command-receipts.json')
    data['commands']=[dict(name=p.parent.name,**clean(json.loads(p.read_bytes()))) for p in sorted((RUN/'commands').glob('*/command.json'))]
    save('command-receipts.json',data)


def build():
    (HERE/'privacy-check.json').touch(exist_ok=True)
    files=reader.scope()-{reader.SELF}
    for path in files:
        if path=='evidence/04A37/privacy-check.json':continue
        reader.validate_privacy((ROOT/path).read_text(encoding='utf-8-sig'))
    save('privacy-check.json',dict(status='PASS',files_checked=len(files),policy='All proposed text bytes; no absolute private home/host/user/SID/key markers, binaries/raw private traces or owner Plan/README payloads.',raw_private_outputs='EXCLUDED; retained under ignored isolated run only.'))
    decision=load('readiness.json')['status']
    rows=[dict(path=p,bytes=None,sha256=None) if p==reader.SELF else reader.pin(p) for p in sorted(reader.scope())]
    save('publication-manifest.json',dict(decision=decision,scope='04A37 software compatibility review proposal only; no publication/native/scientific authorization',files=rows))
    print('Manifest:',len(rows),'entries;',len(rows)-1,'external hashes; one null self;',decision)


def audits(label):
    r=m.Runner(RUN)
    command=r.command(label,[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A37','-p','test_audit.py','-v'])
    text=(RUN/'commands'/label/'stderr.log').read_text(encoding='utf-8-sig')
    total=int(re.search(r'Ran (\d+) tests?',text).group(1))
    assert command['exit_code']==0 and text.rstrip().endswith('OK'), 'Independent/adverse audits failed; do not mark READY'
    previous=load('audit-results.json')
    history=previous.get('runs',[])
    history.append(dict(label=label,**clean(command)))
    save('audit-results.json',dict(status='PASS',total=total,passed=total,failed=0,errors=0,runs=history,
        identities=re.findall(r'^(test_\S+ \([^)]*\))',text,re.M),scope='Independent packet/manifest/source/ownership/identity/capture/privacy/readiness and adverse tests; separate from historical Python aggregate.'))
    value=load('readiness.json')
    if value.get('coverage_route')=='OWNER_AUTHORIZED_LATE_ARCHIVED_ENTRY_ROUTE_A':
        supplement=reader.route_a_supplement()
        assert previous['supplemental']['status']=='PASS'
        value['status']=reader.READY;value['blockers']=[]
        value['supplemental_rows_per_configuration']=supplement['supplemental_rows_per_configuration']
        value['software_scope']='IMPLEMENTED_VALIDATED_WITH_OWNER_AUTHORIZED_LATE_ROUTE_A_COVERAGE; original chronology gap and native/scientific restrictions retained'
        result=load('audit-results.json');result['supplemental']=previous['supplemental'];save('audit-results.json',result)
    elif reader.required_preedit_coverage()['eligible']:
        value['status']=reader.READY;value['blockers']=[]
        value['software_scope']='IMPLEMENTED_VALIDATED_ZERO_IN_SCOPE_OPEN; native/scientific restrictions unchanged'
    else:
        value['status']='NOT_READY_04A37'
        value['blockers']=['MANDATORY_PREEDIT_PCA_BIPLOT_CROSS_PRODUCT_NOT_CAPTURED']
        value['software_scope']='IMPLEMENTATION_AND_CAPTURED_COMPATIBILITY_PASS; REQUIRED_PREEDIT_OPTION_COVERAGE_INCOMPLETE'
    save('readiness.json',value)
    receipts();build()
    print('PASS',total,'original packet audits;',value['status'],'; coverage route:',value.get('coverage_route','ORIGINAL_PREEDIT_ONLY'))


def independent(label):
    r=m.Runner(RUN)
    result=r.command(label,[sys.executable,'-B','evidence/04A37/verify_packet.py'])
    assert result['exit_code']==0
    value=load('audit-results.json');value['independent_reader']=clean(result)
    value['independent_reader']['qualification']='Successful read before adding this receipt; final direct read must verify the regenerated complete manifest.'
    save('audit-results.json',value);receipts();build()


if __name__=='__main__':
    action=sys.argv[1]
    if action=='build':build()
    elif action=='audit':audits(sys.argv[2])
    elif action=='verify':independent(sys.argv[2])
    else:raise ValueError(action)
