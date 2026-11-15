"""Additive preflight after owner commits the previously observed BuildDate edit."""
from pathlib import Path
path=Path(__file__).with_name('authenticate-v1.py');source=path.read_text()
source=source.replace('from common40 import *','from common40_v2 import *')
source=source.replace("assert git('rev-parse','HEAD^').decode().strip()==PREVIOUS", "assert git('rev-parse','HEAD^').decode().strip()==MERGE39\nassert git('rev-parse',MERGE39+'^').decode().strip()==PREVIOUS\nassert git('diff','--name-only',MERGE39,MAIN).decode().splitlines()==['BESHstatNG/Generated/VersionAttributes.vb']")
source=source.replace("assert git('diff','--name-only').decode().splitlines()==['BESHstatNG/Generated/VersionAttributes.vb']", "assert not git('diff','--name-only')")
source=source.replace("parent=PREVIOUS,", "parent=MERGE39,")
source=source.replace("parent=PREVIOUS,merge_base=MAIN", "parent=MERGE39,merge39=MERGE39,pre39=PREVIOUS,merge_base=MAIN")
source=source.replace("'entry/source-preimages-v1.json'", "'entry/source-preimages-v2.json'")
source=source.replace("'entry/authentication-v1.json'", "'entry/authentication-v2.json'")
source=source.replace("'BuildDate2026-11-02 to2026-11-14 only; preserve bytes and do not run version-generator target.'", "'Owner committed previously observed BuildDate2026-11-14 in separate bba commit during preflight. Preserve current bytes; compile-only runner skips generator. Initial stale-anchor attempt stopped before capture or implementation; original script retained.'")
source=source.replace("'direct child',PREVIOUS", "'owner BuildDate parent',MERGE39")
assert "OLD/'" not in source
exec(compile(source,str(path),'exec'))
