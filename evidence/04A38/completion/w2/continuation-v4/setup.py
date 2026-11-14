from pathlib import Path
p=Path(__file__).resolve().parent
s=(p.parent/'continuation-v3/common.py').read_text(encoding='utf-8').replace('04A38 C3','04A38 C4').replace("PREVIOUS=W2/'continuation-v2'","PREVIOUS=W2/'continuation-v3'").replace('completion-w2-cont3','completion-w2-cont4').replace('w2c3-','w2c4-')
with (p/'common.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
