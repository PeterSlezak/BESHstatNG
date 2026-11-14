"""Prepare current driver only after the immutable original-output seal exists."""
from pathlib import Path
import hashlib,re
HERE=Path(__file__).resolve().parent
SEAL='edcad8369d6d3db5c9bdb7c61518a01951059b8fbc7ff2f3326738c858ab28da'
assert hashlib.sha256((HERE/'original-output-seal.json').read_bytes()).hexdigest()==SEAL
text=(HERE.parent/'v4-mmrm-duplicate-guards/execute_current.py').read_text(encoding='utf-8')
text=re.sub("SEAL='[a-f0-9]+'",'SEAL='+repr(SEAL),text)
with (HERE/'execute_current.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
