from pathlib import Path
root=Path(__file__).resolve().parents[2];here=root/'evidence/04A40'
s=(here/'repository-privacy-witness-v1.py').read_text(encoding='utf-8').replace("'-v1.json'","'-v2.json'").replace('repository-privacy-witness-v1.json','repository-privacy-witness-v2.json')
(here/'repository-privacy-witness-v2.py').open('x',encoding='utf-8').write(s)
