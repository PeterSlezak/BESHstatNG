"""Append-only public view of a private environment-path inspection receipt.

No source, typed input, original output, comparison, trace or seal is altered.
The raw worktree location is an environment diagnostic, like the private raw
command logs. All source provenance fields are retained exactly in public view.
"""
import json
from inspect_entry import ROOT,HERE,pin,save

def main():
 raw=json.loads((HERE/'entry-inspection.json').read_bytes())
 old=raw['git']['worktrees']
 need=ROOT.as_posix()
 assert old.count(need)==1 and ' [codex/04A-38]' in old
 raw['git']['worktrees']=old.replace(need,'<REPO_ROOT>')
 save('entry-inspection-public.json',raw)
 save('private-environment-control.json',dict(status='PUBLIC_ENVIRONMENT_REDACTION_ONLY',
      private_raw_receipt=pin(HERE/'entry-inspection.json'),public_view=pin(HERE/'entry-inspection-public.json'),
      json_pointer='/git/worktrees',replacement='<REPO_ROOT>',
      retained='Every other JSON field unchanged, including all4695 original source hashes,41 UDF snapshot proofs, Git identities and predecessor hashes',
      source_input_output_trace_normalization=False,
      publication_disposition='EXCLUDE_PRIVATE_RAW_ENVIRONMENT_LOG; retain raw file locally byte-for-byte; publish this disclosure and public view',
      seal_references='Original seals retain the raw diagnostic digest. The public reader independently pins the public view and original source against authenticated original-source metadata; no sealed statistical/input/source payload is omitted.'))
if __name__=='__main__':main()
