"""Preserve the sealed reader; add a public-entry variant with one explicit private log disposition."""
from inspect_entry import HERE,pin,save
def main():
 old=(HERE/'verify_inputs.py').read_text(encoding='utf-8')
 new=old.replace("import hashlib,json","import hashlib,json\nfrom environment_control_reader import inspect as public_entry,PRIVATE as PRIVATE_ENVIRONMENT")
 a="for row in seal['files']:\n  p="
 assert new.count(a)==1
 new=new.replace(a,"for row in seal['files']:\n  if row['path']==PRIVATE_ENVIRONMENT:\n   public_entry(root);continue\n  p=")
 a="entry=load(here/'entry-inspection.json')";assert new.count(a)==1
 new=new.replace(a,'entry=public_entry(root)')
 with (HERE/'verify_inputs_public.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(new)
 save('public-reader-derivation.json',dict(original_reader=pin(HERE/'verify_inputs.py'),public_reader=pin(HERE/'verify_inputs_public.py'),
      source_input_output_trace_checks_unchanged=True,only_difference='One private raw worktree-path log uses the independently pinned exact public view; every source provenance field compared to original-source-authenticity metadata.'))
if __name__=='__main__':main()
