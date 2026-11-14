from common import *
s=(HERE/'MethodMetadata-v5.cs').read_text(encoding='utf-8')
needle='RootType(t).FullName=="BESHStatNG.CIformat"'
assert needle in s
s=s.replace(needle,needle+'||RootType(t).FullName=="BESHStatNG.ResultTable"||RootType(t).FullName=="BESHStatNG.ResultTableOutputModel"||RootType(t).FullName=="BESHStatNG.Matrix.MatrixDecompositionCore"||t.FullName.StartsWith("BESHStatNG.parametric.Parametric+OneWayANOVA",StringComparison.Ordinal)||t.FullName=="Microsoft.VisualBasic.CompilerServices.ProjectData"')
s=s.replace('schema="PASSIVE_METADATA_V5"','schema="PASSIVE_METADATA_V6"')
with (HERE/'MethodMetadata-v6.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'run_metadata_v5.py').read_text(encoding='utf-8').replace('metadata-v5','metadata-v6').replace('MethodMetadata-v5','MethodMetadata-v6').replace('native-metadata-owners-v5','native-metadata-owners-v6')
with (HERE/'run_metadata_v6.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'native-metadata-owners-v5.txt').read_text(encoding='utf-8')+'\nBESHStatNG.Matrix.Matrix\n'
with (HERE/'native-metadata-owners-v6.txt').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
