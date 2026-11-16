from common45 import *
import csv,re,collections,xml.etree.ElementTree as ET
lock();target=PRIVATE/'syntax-current-v1';assert not target.exists()
r=command('roslyn-source-inventory-v1',['dotnet',str(ROOT/'scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll'),'--write','--ledger='+str(target)])
assert not r['exit_code'],'SYNTAX_CENSUS_FAILED'
def table(n):return list(csv.DictReader((target/n).open(encoding='utf-8-sig',newline='')))
files=table('files.csv');symbols=table('symbols.csv');states=table('state.csv');edges=table('ui-wiring.csv');types=table('types.csv')
save(PRIVATE/'syntax-inputs-v1.json',dict(tables=[pin(p) for p in sorted(target.iterdir()) if p.is_file()],tool=pin('scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll'),tool_source=pin('scripts/windows-migration-audit/Program.cs'),command=r))
save('scope/syntax-schema-v1.json',dict(fields={n:list(table(n)[0]) for n in ['types.csv','symbols.csv','state.csv','ui-wiring.csv']},sample_edge=edges[0]))
print('Syntax census',len(files),len(symbols),len(states),len(edges),'tables preserved privately',flush=True)
