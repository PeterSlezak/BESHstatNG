from common40_v2 import *
def lock():
    assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
    assert git('branch','--show-current').decode().strip()=='codex/04A-40'
    assert not git('diff','--cached','--name-only')
    assert pin('BESHstatNG/Generated/VersionAttributes.vb')==load(HERE/'entry/authentication-v3.json')['preexisting_owner_edit']
