from pathlib import Path
root=Path(__file__).resolve().parents[2];here=Path(__file__).parent
s=(root/'evidence/04A45/common45.py').read_text(encoding='utf-8-sig').replace('04A.45','04A.46').replace('04A45','04A46').replace('04A-45','04A-46').replace('runner45','runner46').replace("MAIN='350ffbc0924b09199249316a3ee2f31bc3dd98e8'","MAIN='ebc4a3e9df41ef8588aa7f6b25363f6d044c41a7'").replace("PRIOR='243b6dcd654767ffedb8fe3643b310c0c56cd6ac'","PRIOR='350ffbc0924b09199249316a3ee2f31bc3dd98e8'").replace('04A46_NOT_AUTHORIZED','04A47_NOT_AUTHORIZED').replace("'a45-'","'a46-'")
with (here/'common46.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(root/'evidence/04A45/run45.py').read_text(encoding='utf-8-sig').replace('04A45','04A46')
with (here/'run46.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(root/'evidence/04A45/authenticate-v1.py').read_text(encoding='utf-8-sig').replace('common45','common46').replace('codex/04A-45','codex/04A-46').replace('04A44','04A45').replace('accepted44','accepted45').replace('sealed44','sealed45').replace('merged44','merged45').replace('ACCEPTED44','ACCEPTED45').replace('merged44','merged45').replace('CURRENT_MAIN_PRE_EDIT_04A45','CURRENT_MAIN_PRE_EDIT_04A46').replace('entry_runner45','entry_runner46').replace("living_revision='2.21'","living_revision='2.22'")
s=s.replace("ROOT/'evidence/04A45/review_reader_v4.py'","ROOT/'evidence/04A45/review_reader.py'").replace("reader.verify(ROOT/d['archive']['path'],rp)['SHA_size_CRC']=='PASS'","reader.verify(ROOT/d['archive']['path'],rp)['status']=='PASS_EXTERNAL_AND_SEMANTIC'")
s=s.replace("d=load(rp);exact(d['archive'])","assert pin(rp)['sha256']=='46df6a9ed9d498555a3588de7625f0a1f70bc5fc5186bfe1329547b8eca893ed'\nd=load(rp);exact(d['archive'])")
with (here/'authenticate-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
