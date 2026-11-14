from common import *
for n in ['build.py','focused.py','compatibility.py','inspect_stage.py','selected_release.py','run_full_validation.py','MethodMetadata-v4.cs','native-metadata-owners.txt','run_metadata_v4.py','CallDomains.cs','TextDomainProbe-v2.cs']:
 source=(PREVIOUS/n).read_text(encoding='utf-8-sig')
 if n=='compatibility.py':source=source.replace("PREVIOUS/'MmrmCapture38.cs'","PREVIOUS/'MmrmCapture38.cs'") # unchanged historical recorder paths are explicit inside copied script
 if n=='run_full_validation.py':source=source.replace("label='w2c2-validation-v1'","label='w2c3-validation-v1'")
 if n=='run_metadata_v4.py':source=source.replace("HERE/'metadata-v4'/stage","HERE/'metadata-v4'/stage")
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/preflight-v1.json',dict(status='FAILED_SETUP_BEFORE_ANY_SOURCE_WRITE',script=pin(HERE/'preflight.py'),error='SyntaxError: duplicate status keyword in proposed receipt; v2 records git_status under separate key. No source change or original execution.'))
# Scope existing references to the accidentally duplicated members before removal.
import re
scope=[];references=[]
for c in ['Debug','Release']:
 data=json.loads((PREVIOUS/'metadata-v4/b1-v1'/('current-'+c+'.json')).read_bytes());allmethods=data['methods'];unused=[]
 for name in ['StackWithBlankRow','NormalizeBlanks']:
  definitions=[m for m in allmethods if m['declaring_type']=='BESHStatNG.ExcelCommon.UdfSpillCell38' and m['member_name']==name];assert len(definitions)==1
  calls=[m['method'] for m in allmethods if any(isinstance(i['operand'],str) and '.UdfSpillCell38 @ ' in i['operand'] and '.'+name+'(' in i['operand'] for i in m['instructions'])];assert not calls
  unused.append(dict(method=definitions[0]['method'],incoming_compiled_calls=calls))
 scope.append(dict(configuration=c,assemblies=data['assemblies'],members=unused))
for folder in ['BESHstatNG','BESHStat.Core','BESHStat.Excel.Common','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHStatNG.Test','BESHStat.OfficeAddin','BESHStat.WasmSmoke']:
 # Explicit source roots; do not visit ignored build/test output trees.
 paths=[p for p in git('ls-files','--cached','--others','--exclude-standard','--',folder).decode().splitlines() if p.endswith(('.vb','.cs','.mjs','.js')) and '/obj/' not in p and '/bin/' not in p]
 for path in paths:
  s=(ROOT/path).read_text(encoding='utf-8-sig',errors='replace')
  if re.search(r'UdfSpillCell38\.(?:StackWithBlankRow|NormalizeBlanks)',s):references.append(path)
assert not references
save('duplicate-surface-before.json',dict(status='UNINTENDED_NEW_PUBLIC_HELPERS_UNUSED_IN_REVIEWED_COMPILED_AND_SOURCE_SCOPE',scope=scope,source_references=references,source_scope='Tracked/current source in8production/test/host project roots, no bin/obj/ignored history',external_scope='New unaccepted04A38 Common helpers, never owner-accepted or released. Removal explicitly authorized in this continuation; no268ExcelABI change. Arbitrary external reflection cannot be enumerated.',original_queries=0))
