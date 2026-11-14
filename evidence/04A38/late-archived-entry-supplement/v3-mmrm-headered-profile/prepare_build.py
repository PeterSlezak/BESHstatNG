"""Append-only adaptation of the proven original-only offline build driver."""
from common import *
def main():
 old=(V2/'build_original.py').read_text(encoding='utf-8')
 start=old.index('import json, os, shutil, subprocess, sys')
 end=old.index('def main():')
 text=old[:start]+'import shutil\nfrom common import *\n\n'+old[end:]
 text=text.replace("entry['original_tree_rehashed_against_live_git']","entry['status']=='LIVE_ENTRY_AND_ORIGINAL_SOURCE_AUTHENTICATED'")
 text=text.replace("runner=m.Runner(RUNROOT)","runner=m.Runner(RUNROOT)")
 text=text.replace("'gee38-v2-original-'","'mmrm38-v3-original-'")
 with (HERE/'build_original.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
 save('build-driver-derivation.json',dict(original=pin(V2/'build_original.py'),derived=pin(HERE/'build_original.py'),
      changes='New packet/private output directories, independently reauthenticated entry receipt, new command labels. All original project/compiler options retained.',
      source_tree='Read-only pristine original tree, reauthenticated against live Git archive',current_binary_inputs=False))
if __name__=='__main__':main()
