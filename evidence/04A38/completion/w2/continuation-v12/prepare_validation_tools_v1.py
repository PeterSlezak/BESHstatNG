from common import *
rows=[]
for name in ['build_v1.py','focused_v1.py']:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    if name=='focused_v1.py':
        value=value.replace('UdfContinuation11Tests','UdfContinuation12Tests').replace('FullyQualifiedName~Continuation11','FullyQualifiedName~Continuation12')
        value=value.replace('FullyQualifiedName~FactorAnalysisCoreTests|FullyQualifiedName~ClusterAnalysisCoreTests|FullyQualifiedName~CaCoreTests','FullyQualifiedName~CaCoreTests|FullyQualifiedName~DiscriminantAnalysisCoreTests')
        value=value.replace('stage,config=sys.argv[1:3];run=',"stage,config=sys.argv[1:3];assert config=='Debug','Release unit tests NOT_RUN_OWNER_PREFERENCE';run=")
    target=HERE/name
    with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(target),changes='C12 current common paths/filters and explicit Debug-only focus guard; shipping build remains offline/current only.'))
save('validation-tools-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
