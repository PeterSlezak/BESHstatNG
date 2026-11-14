"""Retain inherited missing-file TypeError as rejection, narrowly in its directed control."""
from common_v2 import *
source=(HERE/'seal_and_controls_v3.py').read_text(encoding='utf-8');needle="        except (ValueError,AssertionError,OSError,KeyError) as error:rows.append(dict(name=name,status='REJECTED',reason=clean(str(error))));return"
assert needle in source
source=source.replace(needle,needle+"\n        except TypeError as error:\n            if name!='Missing protected earlier reference':raise\n            rows.append(dict(name=name,status='REJECTED',reason='Inherited C8 reader hashes the explicitly missing None input and raises TypeError; reader fails closed. First control-harness attempt retained.',exception_type=type(error).__name__));return")
with (HERE/'seal_and_controls_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('control-harness-correction-v1.json',dict(first_command=pin(RUNROOT/'commands/w2c9-checkpoint-controls-v3/command.json'),first_stderr=pin(RUNROOT/'commands/w2c9-checkpoint-controls-v3/stderr.log'),before=pin(HERE/'seal_and_controls_v3.py'),after=pin(HERE/'seal_and_controls_v4.py'),scope='Only the exact missing prior C8 reference control accepts inherited TypeError as rejection; other TypeErrors remain errors. Reader already failed closed, no source/test/reader acceptance rule changed.'))
print('Directed missing-old control exception qualified')
