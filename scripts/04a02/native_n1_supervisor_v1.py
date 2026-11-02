"""External N1 deadline/launch gate. Does not control an existing Excel process.

Current CLI admits only a complete independently reviewed exact owned context.
It supervises the launched helper; closure of the owned Excel remains a reviewed
owner action. No global shutdown, unowned process kill or automatic dialog action.
"""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('n1_old',ROOT/'scripts/04a02/gui_pilot_v2.py')
g=importlib.util.module_from_spec(spec);spec.loader.exec_module(g)


def gate(plan):
    missing=g.prerequisites(plan)
    if plan.get('native_context',{}).get('schema')!='04A02-C09-owned-N1-context-v1':missing.append('actual owned N1 context')
    for key in ('owned_launch_receipt','source_pins','native_helper','native_helper_sha256','risk_review_path','risk_review_sha256'):
        if not plan.get(key):missing.append(key)
    if plan.get('closure_protocol')!='OWNER_CLOSES_ONLY_RECORDED_DISPOSABLE_INSTANCE_AFTER_EVIDENCE':missing.append('reviewed closure protocol')
    return missing


def digest(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def execute(plan_path,expected_sha,output):
    if digest(plan_path)!=expected_sha:raise ValueError('Stage plan hash drift')
    plan=json.loads(plan_path.read_bytes())
    missing=gate(plan)
    if missing:raise ValueError('NO_GO: '+', '.join(missing))
    review=json.loads(Path(plan['risk_review_path']).read_bytes())
    if digest(plan['risk_review_path'])!=plan['risk_review_sha256'] or review.get('status')!='INDEPENDENTLY_ACCEPTED_N1_ONLY' or not review.get('reviewer') or not review.get('source'):
        raise ValueError('Missing genuine independent risk acceptance')
    for path,expected in plan['source_pins'].items():
        if digest(ROOT/path)!=expected:raise ValueError('Source drift: '+path)
    helper=Path(plan['native_helper'])
    if digest(helper)!=plan['native_helper_sha256']:raise ValueError('Native helper drift')
    launch=json.loads(Path(plan['owned_launch_receipt']['path']).read_bytes())
    if digest(plan['owned_launch_receipt']['path'])!=plan['owned_launch_receipt']['sha256'] or launch.get('status')!='OWNER_CREATED_EXCLUSIVE_DISPOSABLE_PROCESS':
        raise ValueError('No attributable exact owned-process launch receipt')
    context=plan['native_context']
    for key in g.CONTEXT:
        if context.get(key)!=plan['expected'].get(key) or context.get(key)!=launch.get(key):raise ValueError('Launch/context identity drift: '+key)
    output.mkdir(exist_ok=False)
    context_path=output/'context.json'
    with context_path.open('x') as f:json.dump(context,f,indent=2)
    work=output/'native';work.mkdir()
    command=[str(helper),str(context_path),str(work),digest(context_path)]
    try:
        with (output/'stdout.txt').open('xb') as out,(output/'stderr.txt').open('xb') as err:
            result=subprocess.run(command,cwd=ROOT,stdout=out,stderr=err,timeout=15,
                shell=False,creationflags=getattr(subprocess,'CREATE_NO_WINDOW',0))
        if result.returncode or (output/'stderr.txt').stat().st_size:raise ValueError('Native attempt failed; preserve and stop')
    except BaseException as error:
        with (output/'supervisor-failure.json').open('x') as f:json.dump(dict(status='FAILED_RETAINED_NO_RETRY',type=type(error).__name__,error=str(error)),f)
        raise


if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--plan',type=Path,required=True)
    p.add_argument('--sha256',required=True);p.add_argument('--output',type=Path,required=True)
    a=p.parse_args();execute(a.plan,a.sha256,a.output)
