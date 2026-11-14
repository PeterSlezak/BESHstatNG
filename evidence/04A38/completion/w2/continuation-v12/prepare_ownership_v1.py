from common import *
source=PREVIOUS/'build_ownership_v1.py';value=source.read_text(encoding='utf-8')
value=value.replace('from proof_kernel_v3 import','from proof_kernel_v4 import').replace('current-universe-packet-v2.json','current-universe-packet-v1.json')
value=value.replace("newclasses={'Private Class KMeansHandle','Private Class HierarchicalHandle','Private Class CorrespondenceHandle'}", "newclasses={'Private Class MultipleCorrespondenceHandle','Private Class DiscriminantHandle'}")
value=value.replace("classes={'Private Class PcaHandle','Private Class FactorAnalysisHandle'}|newclasses", "classes={'Private Class PcaHandle','Private Class FactorAnalysisHandle','Private Class KMeansHandle','Private Class HierarchicalHandle','Private Class CorrespondenceHandle'}|newclasses")
value=value.replace('len(new_support)==3','len(new_support)==2').replace('C11_','C12_').replace('C11 narrows','C12 narrows')
value=value.replace('Exact335-entry v10','Exact302-entry v11').replace('five fully proved','seven fully proved')
with (HERE/'build_ownership_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('ownership-tool-crosswalk-v1.json',dict(before=pin(source),after=pin(HERE/'build_ownership_v1.py'),
    scope='Same exact universe/complement/fragment construction, with two measured new handle classes and all five retained handle declarations. New33 claims require complete existing contracts/callbacks, not count substitution.',original_queries=0))
