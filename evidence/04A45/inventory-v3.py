from common45 import *
import shutil
lock();tool=PRIVATE/'syntax-tool-v3';tool.mkdir()
original=ROOT/'scripts/windows-migration-audit/Program.cs';source=original.read_text(encoding='utf-8-sig')
edits=[]
for name in ['review-decisions.json','wiring-review-decisions.json','control-review-decisions.json']:
 a='scripts/windows-migration-audit/'+name;b='.test-rationalization-04A45.tmp/nonexistent-'+name
 assert source.count(a)==1;source=source.replace(a,b);edits.append([a,b])
(tool/'Program.cs').write_text(source,encoding='utf-8')
shutil.copy2(ROOT/'scripts/windows-migration-audit/WindowsMigrationAudit.csproj',tool/'WindowsMigrationAudit.csproj')
r=command('syntax-tool-v3-build',['dotnet','build',str(tool/'WindowsMigrationAudit.csproj'),'--configfile',str(ROOT/'scripts/windows-migration-audit/NuGet.Config'),'/p:NuGetAudit=false','-v:minimal']);assert not r['exit_code']
save('scope/census-tool-provenance-v3.json',dict(original=pin(original),driver=pin(tool/'Program.cs'),project=pin(tool/'WindowsMigrationAudit.csproj'),substitutions=edits,reason='Fresh Roslyn syntax census only;stale historical semantic/wiring/control review decisions are not applied or rewritten. No source parser logic changed;no package dependencies or downloads.',native='NOT_RUN'))
source=(HERE/'inventory-v1.py').read_text().replace('syntax-current-v1','syntax-current-v3').replace('roslyn-source-inventory-v1','roslyn-source-inventory-v3').replace("str(ROOT/'scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll')","str(tool/'bin/Debug/net10.0/WindowsMigrationAudit.dll')").replace('syntax-inputs-v1','syntax-inputs-v3').replace('syntax-schema-v1','syntax-schema-v3')
exec(compile(source,str(HERE/'inventory-v1.py'),'exec'))
