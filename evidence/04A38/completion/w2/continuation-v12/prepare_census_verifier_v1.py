from common import *
source=PREVIOUS/'verify_census_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace('from proof_kernel_v3 import','from proof_kernel_v4 import').replace('from verify_composition_v2 import tokens,PHASES,unwrap','from verify_c12_composition_v1 import tokens,PHASES,unwrap')
value=value.replace("newclasses={'Private Class KMeansHandle','Private Class HierarchicalHandle','Private Class CorrespondenceHandle'}", "newclasses={'Private Class MultipleCorrespondenceHandle','Private Class DiscriminantHandle'}")
value=value.replace("classes={'Private Class PcaHandle','Private Class FactorAnalysisHandle'}|newclasses", "classes={'Private Class PcaHandle','Private Class FactorAnalysisHandle','Private Class KMeansHandle','Private Class HierarchicalHandle','Private Class CorrespondenceHandle'}|newclasses")
value=value.replace('PHASES[name][2].items()',"PHASES[name]['bindings'].items()")
target=HERE/'verify_c12_census_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('census-verifier-source-crosswalk-v1.json',dict(before=pin(source),after=pin(target),scope='Same independent universe/fragment/complement reconstruction with two newly measured cache declarations, five retained ones and C12 argument bindings.',original_queries=0))
from verify_c12_source_transition_v1 import inspect
load=lambda p:json.loads((ROOT/p).read_bytes());raw=lambda p:(ROOT/p).read_bytes()
save('whole-file-source-transition-proof-v1.json',inspect(load,raw,HERE.relative_to(ROOT).as_posix()+'/'))
print('Whole-file source and old assertion prefixes authenticated')
