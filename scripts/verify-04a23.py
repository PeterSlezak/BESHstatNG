"""04A.23 offline verifier: preserved suite enumeration and isolated writers.

The installed owner Revision 2.9 plan is the only allowed preexisting edit.
No restore, native Excel, R, packaging, generators or signing execution.
"""
from pathlib import Path

source = Path(__file__).with_name('verify-04a21.py').read_text(encoding='utf-8-sig')
for old, new in [('04A-21', '04A-23'), ('04A21', '04A23'),
                 ('04a21', '04a23'), ('Spc21', 'Spc23')]:
    source = source.replace(old, new)
source = source.replace("assert not git('diff', '--cached', '--name-only') and not git('diff', '--name-only')",
    "assert not git('diff', '--cached', '--name-only') and set(git('diff', '--name-only').splitlines()) <= {'planning/BESHStatNG_CrossPlatform_MasterPlan.md'}")
exec(compile(source, __file__, 'exec'), globals())
