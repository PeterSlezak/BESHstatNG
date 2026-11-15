from common44 import *
s=(HERE/'python-dispositions-v1.py').read_text().replace("assert capture_equal(case,{'Remove_Item'},allchanged)==b['message']","assert b['message'].splitlines()[0]=='Items in the second set but not the first:' and {ast.literal_eval(v) for v in b['message'].splitlines()[1:] if v.strip()}==introduced # Set-list ordering depends on recorded PYTHONHASHSEED38;raw exact message/trace retained.")
with (HERE/'python-dispositions-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
