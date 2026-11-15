"""Current main includes the owner's separate BuildDate-only commit observed during preflight."""
from common40 import *
MAIN='bba848305c2c46ff3fc835a1132522ebfec59514'
MERGE39='f2eb060c9c422c1fec4e82989d07907dbd4b9337'
def lock():
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
    assert git('branch','--show-current').decode().strip()=='codex/04A-40'
    assert not git('diff','--cached','--name-only')
    assert pin('BESHstatNG/Generated/VersionAttributes.vb')==load(HERE/'entry/authentication-v2.json')['preexisting_owner_edit']
