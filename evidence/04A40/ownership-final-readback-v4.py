from pathlib import Path
p=Path(__file__).with_name('ownership-final-readback-v3.py');s=p.read_text(encoding='utf-8')
old="save(PRIVATE/'source-map-final-command-v2.json',load(PRIVATE/'commands/a40-source-map-final-v2/command.json'))"
assert s.count(old)==1
s=s.replace(old,"tool=ROOT/'.test-rationalization-04A39.tmp/map-tool/Inventory39.dll'")
exec(compile(s,str(p),'exec'))
