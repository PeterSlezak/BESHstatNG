from common import *
s=(HERE/'MethodMetadata-v4.cs').read_text(encoding='utf-8').replace('schema="PASSIVE_METADATA_V4"','schema="PASSIVE_METADATA_V5"')
s=s.replace('RootType(t).FullName.StartsWith("BESHStatNG.WorksheetCompatibility.Udf",StringComparison.Ordinal)', '(RootType(t).FullName.StartsWith("BESHStatNG.WorksheetCompatibility.Udf",StringComparison.Ordinal)||RootType(t).FullName=="BESHStatNG.ConfidenceIntervalResult"||RootType(t).FullName=="BESHStatNG.CIformat")')
with (HERE/'MethodMetadata-v5.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
names=(HERE/'native-metadata-owners.txt').read_text(encoding='utf-8').splitlines();names.append('BESHStatNG.WindowsSpreadsheetTransport')
with (HERE/'native-metadata-owners-v5.txt').open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(sorted(names))+'\n')
s=(HERE/'run_metadata_v4.py').read_text(encoding='utf-8').replace('metadata-v4','metadata-v5').replace('MethodMetadata-v4.cs','MethodMetadata-v5.cs').replace('native-metadata-owners.txt','native-metadata-owners-v5.txt')
with (HERE/'run_metadata_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('metadata-scope-v5.json',dict(canonical_UDF_owners=33,additional_native_dependency='BESHStatNG.WindowsSpreadsheetTransport',additional_Core_dependencies=['BESHStatNG.ConfidenceIntervalResult','BESHStatNG.CIformat'],purpose='Full Get2D acquisition and preserved existing CI formatter/cache/enum/default context; passive only; prior tools/outputs unchanged',original_queries=0,original_builds=0))
