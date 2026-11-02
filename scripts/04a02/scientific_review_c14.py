"""Fail-closed review-record checker, never a reviewer or golden promoter.

Hashes bind supplied records. Genuine identity/qualification and signatures
require external owner verification; this code cannot establish those facts by
accepting a self-declared name. Two slots/files alone are not two human reviews.
"""
import argparse
from datetime import datetime
import importlib.util
import json
from pathlib import Path
import re
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('c14_review_reader',ROOT/'scripts/04a02/reference_reader_c14.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
require,Refused=reader.r.require,reader.r.Refused
SCHEMA=ROOT/'docs/architecture/04A02/04A02-C14-scientific-review.schema-v1.json'

def shape(value,schema):
    """Small standard-library validator for this schema's explicit vocabulary."""
    types={'object':dict,'array':list,'string':str,'boolean':bool,'integer':int}
    if 'type' in schema:require(type(value) is types[schema['type']],'Schema type mismatch')
    if 'const' in schema:require(value==schema['const'],'Schema constant mismatch')
    if 'enum' in schema:require(value in schema['enum'],'Schema enum mismatch')
    if isinstance(value,dict):
        require(set(schema.get('required',[]))<=set(value),'Missing required review field')
        props=schema.get('properties',{})
        if schema.get('additionalProperties') is False:require(set(value)<=set(props),'Unknown review field')
        for key,sub in props.items():
            if key in value:shape(value[key],sub)
    if isinstance(value,list):
        require(len(value)>=schema.get('minItems',0) and len(value)<=schema.get('maxItems',len(value)),'Review item count')
        for item in value:shape(item,schema.get('items',{}))
    if isinstance(value,str):
        require(len(value)>=schema.get('minLength',0),'Empty review field')
        if 'pattern' in schema:require(re.fullmatch(schema['pattern'],value) is not None,'Review field pattern')

def pinned(reference):
    require(isinstance(reference,dict) and set(reference)=={'path','sha256'},'Pinned evidence reference required')
    require(re.fullmatch('[0-9a-f]{64}',reference['sha256']) is not None,'SHA-256 required')
    path=Path(reference['path']);path=path if path.is_absolute() else ROOT/path
    return reader.read(path,reference['sha256'])

def completed_review(value,packet_ref):
    shape(value,json.loads(SCHEMA.read_bytes()))
    require(value['packet']==packet_ref,'Review packet mismatch')
    packet=reader.r.v2.strict_json(pinned(packet_ref))
    require(packet.get('schema')=='04A02-C14-scientific-review-packet-v1' and packet.get('P')=='NO','Unsupported review packet')
    require(value['slot']==packet['slot'],'Wrong reviewer slot')
    expected_packet=ROOT/('docs/architecture/04A02/04A02-C14-scientific-reviewer-'+str(value['slot'])+'.pending.json')
    require(Path(packet_ref['path']).absolute()==expected_packet and packet_ref['sha256']==reader.r.digest(expected_packet),'Unreviewed replacement packet')
    reviewer=value['reviewer']
    require(reviewer['origin']=='EXTERNAL_HUMAN' and reviewer['independent_of_codex_and_other_reviewer'] is True,'Genuine independent human verification required')
    require(all(reviewer[key].strip() for key in ['identity_id','name','role','qualification']),'Absent reviewer identity/qualification')
    datetime.fromisoformat(value['date'].replace('Z','+00:00'))
    require(value['signature']['kind']=='SIGNED_EXTERNAL_RECORD','Unsigned review')
    require(bool(pinned(value['signature']['record'])),'Empty unsigned external record')
    verification=reader.r.v2.strict_json(pinned(value['verification_receipt']))
    require(verification.get('status')=='OWNER_VERIFIED_EXTERNAL_HUMAN_REVIEW' and verification.get('verifier') and verification.get('source') and verification.get('date'),'No attributable external identity/signature verification')
    require(verification.get('reviewer_identity_id')==reviewer['identity_id'] and verification.get('signature_sha256')==value['signature']['record']['sha256'] and verification.get('packet_sha256')==packet_ref['sha256'],'Verification does not bind exact identity/signature/packet')
    require(value['cases']==packet['required_case_bindings'],'Unsupported/hash-mismatched case, missing reference or changed tolerance policy')
    require([c['case_id'] for c in value['cases']]==list(reader.PUBLICATIONS),'Unsupported or duplicate reference cases')
    for case in value['cases']:
        cid=case['case_id'];job=reader.h.OWNER_ROOT/reader.h.CASES[cid]
        require(case['publication']==dict(path=str(job/'publication-v3.json'),sha256=reader.PUBLICATIONS[cid]),'Unapproved reference publication')
        reader.verify_publication(job,reader.PUBLICATIONS[cid])
        policy=ROOT/'docs/architecture/04A02/04A02-C07-scientific-decision-form.json'
        require(case['scientific_policy']==dict(path=policy.relative_to(ROOT).as_posix(),sha256=reader.r.digest(policy)),'Unapproved tolerance policy')
        for key in ['publication','raw_stdout','raw_stderr','process_exit','owner_technical_receipt','scientific_policy']:
            raw=pinned(case[key])
            if key=='raw_stderr':require(raw==b'','Nonempty reference stderr')
            if key=='process_exit':require(reader.r.v2.strict_json(raw)=={'exit_code':0},'Nonzero reference exit')
        for item in case['comparisons']:pinned(item)
    require(value['tolerance_changes']==[],'Tolerance change requires a new separately approved policy and packet')
    require(value['P']=='NO','Review cannot approve P')
    return value

def two_reviews(first,second,packet_one,packet_two):
    a=completed_review(first,packet_one);b=completed_review(second,packet_two)
    require(a['slot']==1 and b['slot']==2,'Two exact different review slots required')
    norm=lambda s:' '.join(s.casefold().split())
    require(norm(a['reviewer']['identity_id'])!=norm(b['reviewer']['identity_id']) and norm(a['reviewer']['name'])!=norm(b['reviewer']['name']),'Same reviewer cannot fill both slots')
    require(a['signature']['record']['sha256']!=b['signature']['record']['sha256'] and a['verification_receipt']['sha256']!=b['verification_receipt']['sha256'],'Reused signature/verification is not independent')
    require(a['verdict']=='APPROVE_EVIDENCE' and b['verdict']=='APPROVE_EVIDENCE','Two approving scientific verdicts required')
    require(not a['deviations'] and not b['deviations'],'Unresolved deviations block proposed P readiness')
    return dict(status='TWO_SUPPLIED_REVIEW_RECORDS_VALIDATED_OWNER_P_STILL_REQUIRED',P='NO',promoted=False,
                human_authenticity_machine_proven=False,
                limitation='Schema/hash/link validation relies on genuine separately supplied owner verification of external humans and signatures; synthetic fixtures and self-declarations establish neither.')

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--review-one',type=Path,required=True);p.add_argument('--review-two',type=Path,required=True)
    p.add_argument('--packet-one',type=Path,required=True);p.add_argument('--packet-two',type=Path,required=True)
    a=p.parse_args();ref=lambda path:dict(path=str(path),sha256=reader.r.digest(path))
    result=two_reviews(reader.r.v2.strict_json(reader.read(a.review_one)),reader.r.v2.strict_json(reader.read(a.review_two)),ref(a.packet_one),ref(a.packet_two))
    print(json.dumps(result))
