from common import *
source=(HERE/'inspect_metadata.py').read_text(encoding='utf-8')
source=source.replace("stage/'metadata'","stage/'metadata-full'").replace("HERE/'metadata'/stage","HERE/'metadata-full'/stage")
source=source.replace("'MethodMetadata-v8.cs'","'MethodMetadata-v9.cs'")
source=source.replace("source=source.replace(needle,", "source=source.replace(needle,",1)
insert='''source=source.replace('RootType(t).Name.Contains("38")','true')
owners=HERE/'native-metadata-owners-full.txt'
if not owners.exists():
 with owners.open('x',encoding='utf-8',newline='\\n') as f:f.write((PREVIOUS/'native-metadata-owners-v7.txt').read_text(encoding='utf-8')+'\\nBESHStatNG.ExcelDnaSharedApplicationAdapter\\n')
'''
source=source.replace("src=HERE/'MethodMetadata-v9.cs'",insert+"src=HERE/'MethodMetadata-v9.cs'")
source=source.replace("str(PREVIOUS/'native-metadata-owners-v7.txt')","str(owners)")
source=source.replace("stage+'-metadata", "stage+'-full-metadata")
source=source.replace("save('metadata-'+stage", "save('metadata-full-'+stage")
with (HERE/'inspect_metadata_full.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
