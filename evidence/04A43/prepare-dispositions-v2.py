from common43 import *
s=(HERE/'python-dispositions-v1.py').read_text()
s=s.replace("assert oldarg==old_modified and newarg==new_modified", "assert oldarg==old_modified and newarg==new_modified-{'BESHStatNG.Test/BESHStatNG.Tests.vbproj'}\n            verifier=ROOT/('scripts/verify-04a22-publication.py' if 'membership_hashes' in r['identity'] else 'scripts/verify-04a24-publication.py')\n            assert \"'BESHStatNG.Test/BESHStatNG.Tests.vbproj'\" in verifier.read_text(encoding='utf-8-sig')")
s=s.replace('actual_git_diff_exact=True','actual_git_diff_minus_historically_allowed_test_project_exact=True,verifier_source=pin(verifier)')
s=s.replace("r['sealed']['sha256']==before and r['current']['sha256']==after", "r['sealed']['sha256']==before and r['current']['sha256']==after")
s=s.replace('before42 edits;not a new42 mutation.','before43 edits;not a new43 mutation.')
with (HERE/'python-dispositions-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
