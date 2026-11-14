"""Preserve the independent reader algorithm; bind it to the v4 input reader."""
from common import *
def main():
 data=(V3/'verify_original_source.py').read_bytes()
 with (HERE/'verify_original_source.py').open('xb') as stream:stream.write(data)
 save('independent-source-reader-lineage.json',dict(original=pin(V3/'verify_original_source.py'),v4=pin(HERE/'verify_original_source.py'),
      code_bytes_exact=True,binding='Relative import resolves v4 verify_inputs, PREFIX and BASE; reads current v4 source maps and pinned Git independently',producer_imports=False))
if __name__=='__main__':main()
