from common import *
source=(PREVIOUS/'focused_BC_v1.py').read_text(encoding='utf-8')
source=source.replace('FullyQualifiedName~PcaCoreTests|FullyQualifiedName~FactorAnalysisCoreTests',
    'FullyQualifiedName~FactorAnalysisCoreTests|FullyQualifiedName~ClusterAnalysisCoreTests|FullyQualifiedName~CaCoreTests')
source=source.replace('FullyQualifiedName~UdfContinuation10Tests.Pca|FullyQualifiedName~UdfContinuation10Tests.Factor',
    'FullyQualifiedName~UdfContinuation11Tests')
source=source.replace('FullyQualifiedName~Continuation10Pca|FullyQualifiedName~Continuation10Factor',
    'FullyQualifiedName~Continuation11')
with (HERE/'focused_v1.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(source)
save('focused-runner-authority-v1.json',dict(before=pin(PREVIOUS/'focused_BC_v1.py'),
    after=pin(HERE/'focused_v1.py'),change='Exact C11/new-test and existing family Core filters; Debug only. All assertions unchanged.',
    first_build=pin(HERE/'build-packet-v1-Debug.json'),retry=pin(HERE/'build-packet-v2-Debug.json'),
    correction='Sandbox SDK sentinel access required approved offline compilation retry; no source/compiler defect and no restore/install.',original_queries=0))
