"""Collect retained C09 identities; never rerun tests or replace an earlier receipt."""
import argparse
import importlib.util
import json
from pathlib import Path
import re
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('collector08',ROOT/'scripts/04a02/checkpoint08_collect_results.py')
old=importlib.util.module_from_spec(spec);spec.loader.exec_module(old)


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--evidence',type=Path,required=True);a=p.parse_args()
    run=a.evidence.absolute();trx=json.loads((run/'test-identities-checkpoint09-final.json').read_bytes())
    output=dict(schema='04A02-C09-test-identities-v1',managed=trx,configurations={})
    for config,name,old_seam,new_seam in [('Debug','c9D','o9d','s9d'),('Release','c9R','o9r','s9r')]:
        text=(run/(name+'-full.log')).read_text(encoding='utf-8')
        full=old.python_rows(text);node=old.node_rows(text);focused=[]
        assert len(full)==sum(map(int,re.findall(r'^Ran (\d+) tests in ',text,re.M)))
        assert len(node)==916
        for path in sorted(run.glob('focused-'+config+'-*-command.json')):
            command=json.loads(path.read_bytes());log=path.with_name(path.name.replace('-command.json','.log'))
            focused.append(dict(command=command,log=str(log.relative_to(ROOT)),tests=old.python_rows(log.read_text(encoding='utf-8'),command['command'][-1])))
        unique={x['identity'] for x in full};unique.update(x['identity'] for f in focused for x in f['tests'])
        managed=[t for t in trx if '/'+config+'/' in t['trx'] or '/'+name+'-' in t['trx'] or '/'+old_seam+'/' in t['trx'] or '/'+new_seam+'/' in t['trx']]
        summary=dict(managed=sum(int(x['counters']['total']) for x in managed),managed_failed=sum(int(x['counters']['failed']) for x in managed),
            managed_not_executed=sum(int(x['counters']['notExecuted']) for x in managed),python_full=len(full),
            python_focused=sum(len(f['tests']) for f in focused),python_unique=len(unique),node=len(node),
            node_unique=len({(x['command'],x['identity']) for x in node}))
        output['configurations'][config]=dict(summary=summary,python_full=full,python_focused=focused,node=node)
        print(config,json.dumps(summary))
    with (run/'final-test-identities-c09.json').open('x',encoding='utf-8') as f:json.dump(output,f,indent=2,ensure_ascii=True)


if __name__=='__main__':main()
