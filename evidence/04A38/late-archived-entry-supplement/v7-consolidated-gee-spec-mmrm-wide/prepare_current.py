"""Prepare current driver only after independently checked original sealing."""
from common import *
from verify_inputs import inspect
inspect()
seal=sha((HERE/'original-output-seal.json').read_bytes())
text=(V6/'execute_current.py').read_text(encoding='utf-8')
text=text.replace('262229291122a20199982d8278f96e5d37a0531f4fdecf204c33c9db7e0748c0',seal)
text=text.replace("HERE/'MmrmCapture38.cs'","HERE/'MmrmCapture38-v2.cs'")
with (HERE/'execute_current.py').open('x',encoding='utf-8') as stream:stream.write(text)
save('driver-preparation-attempt.json',dict(previous_attempt='PowerShell inline Python quoting failed before parsing; no current file/build/execution occurred',corrective_action='File-based script',original_output_seal=pin(HERE/'original-output-seal.json')))
print(seal)
