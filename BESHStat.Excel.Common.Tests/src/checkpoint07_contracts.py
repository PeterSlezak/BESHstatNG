"""Host-free C07 readiness/disposition/N0 contracts. No R, UIA or COM invocation."""
import copy
import hashlib
import json
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[2]


def require(ok, message):
    if not ok: raise ValueError(message)


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def delay_imports(path):
    """Inspect PE delay-import descriptors without loading a DLL."""
    data = Path(path).read_bytes()
    def at(fmt, pos):
        require(pos >= 0 and pos+struct.calcsize(fmt) <= len(data), 'truncated PE')
        return struct.unpack_from(fmt, data, pos)
    require(data[:2] == b'MZ', 'MZ required')
    start = at('<I', 0x3c)[0]
    require(data[start:start+4] == b'PE\0\0', 'PE signature')
    count = at('<H', start+6)[0]; optsize=at('<H', start+20)[0]; opt=start+24
    magic=at('<H',opt)[0];require(magic in (0x20b,0x10b), 'PE architecture')
    base=at('<Q' if magic==0x20b else '<I',opt+(24 if magic==0x20b else 28))[0]
    directory=opt+(112 if magic==0x20b else 96)
    number=at('<I',opt+(108 if magic==0x20b else 92))[0]
    if number <= 13:return []
    require(count<=128,'section count')
    sections=[at('<IIII',opt+optsize+i*40+8) for i in range(count)]
    def offset(rva):
        for virtual,address,size,raw in sections:
            if address<=rva<address+max(virtual,size) and rva-address<size:return raw+rva-address
        raise ValueError('unmapped delay RVA')
    rva,size=at('<II',directory+13*8)
    if not rva:return []
    pos=offset(rva);result=[]
    for i in range(min(1024,size//32 if size else 1024)):
        row=at('<IIIIIIII',pos+i*32)
        if not any(row):return result
        name_rva=row[1] if row[0]&1 else row[1]-base
        loc=offset(name_rva);end=data.find(b'\0',loc,min(len(data),loc+4096))
        require(end>=0,'unterminated delay name')
        result.append(data[loc:end].decode('ascii'))
    raise ValueError('unbounded delay descriptors')


def proposed_private_environment_patch(source):
    """Review-only successor text. Never mutates/imports/executes frozen operator."""
    anchor="                        env.update(R_HOME=str(r_home), PATH=str(r_home / 'bin/x64'), TEMP=str(job), TMP=str(job), LC_ALL='C')"
    require(source.count(anchor)==1,'exact frozen environment anchor required')
    replacement="""                        private_home = job / 'home'
                        private_library = job / 'user-library'
                        private_home.mkdir()
                        private_library.mkdir()
                        env.update(R_HOME=str(r_home), PATH=str(r_home / 'bin/x64'),
                                   HOME=str(private_home), R_USER=str(private_home),
                                   R_LIBS_USER=str(private_library), R_LIBS_SITE=str(private_library), R_LIBS='',
                                   TEMP=str(job), TMP=str(job), TMPDIR=str(job), LC_ALL='C', LANG='C', LANGUAGE='C')"""
    return source.replace(anchor,replacement)


def assess_n0(request):
    """Executable DRY RUN only. Even synthetic complete receipts authorize no live API."""
    required=['external_owner_decision','risk_owner_shared_receipt','exact_id_reviews','script_hash_review',
              'xll_hash_review','owned_pid_session_start_proof','exclusive_disposable_context_proof',
              'provider_read_side_effect_policy','rot_or_exact_instance_binding_review',
              'external_timeout_supervisor','no_write_before_after_proof','N1_state_permission']
    missing=[key for key in required if request.get(key) is not True]
    require(request.get('stage') == 'N0_HOST_FREE_DRY_RUN','not a live-stage runner')
    require(request.get('case_id') == '04A02-2X2-NATIVE-01','exact first pilot only')
    return dict(schema='04A02-C07-N0-result-v1',classification='HOST_FREE_DRY_RUN_NOT_NATIVE_AUTHORITY',
                stage='N0',native_calls_allowed=False,live_execution_authorized=False,
                status='NO_GO_PENDING_EVIDENCE' if missing else 'DRAFT_RECEIPTS_COMPLETE_AUTHENTICITY_AND_RUNTIME_NOT_QUALIFIED',
                missing=missing,note='Booleans are test inputs, never an attributable owner signature; no API callback is accepted or invoked.')


def validate_screen_fixture(record):
    require(record['classification']=='PROPOSED_INPUT_CONTRACT_NOT_NATIVE_CAPTURE','input-only proposal')
    require(record['case_id']=='04A02-2X2-NATIVE-01' and record['alpha']==.05,'case/alpha')
    require(record['orientation']==['R1C1=A','R1C2=B','R2C1=C','R2C2=D'],'orientation')
    require(len(record['counts'])==2 and all(len(row)==2 for row in record['counts']),'2x2 dimensions')
    for row in record['counts']:
        for value in row:require(type(value) is int and 0<=value<2**31,'nonnegative exact Int32 count')
    require(record['options']==dict(Fisher=True,OR=True,RR=True,Chi2=False,Association=False,Liddell=False),'explicit options')
    require(record['numeric_reference'] is None and record['native_executed'] is False,'no promoted input fixture')
    return True


def validate_dispositions(value, original):
    expected={i for case in original['cases'] for i in case['deferred_obligation_ids']}
    ids=[x['obligation_id'] for x in value['per_id']]
    require(len(ids)==len(set(ids)) and set(ids)==expected,'complete exact disposition set')
    for row in value['per_id']:
        require(row['cases']==[c['case_id'] for c in original['cases'] if row['obligation_id'] in c['deferred_obligation_ids']], 'per-case obligation map')
        require(row['owner_roles'] and row['earliest_batches'],'owner/consumer retained')
        require(not row['obligation_discharged'] and not row['N_released'],'field evidence is not obligation release')
        require(row['native_evidence']['status']=='NOT_RUN','no invented native evidence')
        require(row['risk_owner_review']['status']=='PENDING_EXTERNAL_REVIEW','no fake risk-owner signature')
        require(row['source_fields']['definition']['status']=='SOURCE_EVIDENCE_COMPLETE','definition evidence required')
        require(row['fake_host_scope'] and row['remaining_evidence'],'scope/gap cannot be blank')
    return True
