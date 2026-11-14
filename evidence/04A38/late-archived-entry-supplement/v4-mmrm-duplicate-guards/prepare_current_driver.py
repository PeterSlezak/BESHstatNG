"""Bind proven current-only driver to this already sealed original packet."""
from common import *
def main():
 text=(V3/'execute_current.py').read_text(encoding='utf-8')
 old="SEAL='0cd8079316509a4e5a24f9164569b0ff7702c04e314970a866d36fddc6b96fdb'"
 assert text.count(old)==1
 value=sha((HERE/'original-output-seal.json').read_bytes())
 assert value=='c9f1c38bddee70d7f8ca722d85ff4db3bc1f79288a0ad237423bbca89fca26f3'
 with (HERE/'execute_current.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text.replace(old,"SEAL='"+value+"'"))
 save('current-driver-lineage.json',dict(original=pin(V3/'execute_current.py'),v4=pin(HERE/'execute_current.py'),original_output_seal=pin(HERE/'original-output-seal.json'),
      change='Only fixed oracle-seal identity; common/compile/input imports resolve this versioned packet; current production assemblies must match the preserved checkpoint'))
if __name__=='__main__':main()
