"""04A.22 offline validation using the accepted isolated verification workflow.

The historical harness is read, never edited. Batch names, focused filter and
fresh evidence destination change; suite enumeration and isolation do not.
No restore, signing, packaging, Excel or R execution.
"""
from pathlib import Path

source = Path(__file__).with_name('verify-04a21.py').read_text(encoding='utf-8-sig')
for old, new in [('04A-21', '04A-22'), ('04A21', '04A22'),
                 ('04a21', '04a22'), ('Spc21', 'Spc22')]:
    source = source.replace(old, new)
exec(compile(source, __file__, 'exec'), globals())
