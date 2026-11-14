from common import *
for label in ['w2c4-validation-v1','w2c5-validation-v1','w2c5-validation-v2']:
    for suffix in ['python-tools','python-help','python-scripts']:
        p=RUNROOT/'commands'/(label+'-'+suffix)/'command.json'
        print(label,suffix, {k:json.loads(p.read_bytes())[k] for k in ['exit_code','seconds']} if p.exists() else 'RUNNING_OR_NOT_STARTED')
