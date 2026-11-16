from common45 import *
lock();sources=runner().__class__.build.__globals__['source_pins']();entry=load(HERE/'entry/source-preimages-v1.json')['source_pins']
changed=[p for p in entry if sources.get(p)!=entry[p]];added=[p for p in sources if p not in entry]
assert changed==['BESHStatNG.Test/BESHStatNG.Tests.vbproj']
assert len(added)==5 and all(p.startswith('BESHStatNG.Test/src/') for p in added)
save('matrix/source-freeze-v1.json',dict(kind='POST_WAVE_AB_SOURCE_FREEZE',utc=utc(),source_pins=sources,source_delta=[dict(before=dict(path=p,sha256=entry[p]),after=pin(p)) for p in changed],new_tests=[pin(p) for p in added],production_changed=[],Core_Common_unchanged=True,Windows_production_unchanged=True,targets=dict(Core='netstandard2.0',Common='netstandard2.0',Windows='net48'),gates=GATES))
save('fixtures/ribbon-projection-final-v1.json',dict(load(HERE/'fixtures/ribbon-projection-v1.json'),projection=pin('BESHStatNG.Test/src/RibbonProjection45.vb'),failed_initial_projection_retained=True))
save('fixtures/option-projection-final-v1.json',dict(load(HERE/'fixtures/option-projection-v1.json'),projection=pin('BESHStatNG.Test/src/OptionProjection45.vb')))
print('Source frozen: one test project registration and five Windows test sources;zero production changes',flush=True)
