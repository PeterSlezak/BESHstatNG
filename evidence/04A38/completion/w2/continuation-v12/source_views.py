"""Authenticate raw source before applying Roslyn UTF16 spans.

Original census uses accepted Git LF bytes. Protected archived snapshots can be
CRLF equivalents. Live census uses actual worktree bytes. Choose only the view
whose complete raw byte hash equals the census source hash, then validate the
entire extracted body hash. Do not normalize a source arbitrarily.
"""
import hashlib
def sha(data):return hashlib.sha256(data).hexdigest()
def view(raw,expected):
 candidates=[('RAW_BYTES',raw),('EXPLICIT_CRLF_TO_LF_IDENTITY',raw.replace(b'\r\n',b'\n'))]
 for policy,data in candidates:
  if sha(data)==expected:return data.decode('utf-8-sig'),policy
 raise ValueError('No authenticated source-byte view matches census')
def body(raw,record):
 text,policy=view(raw,record['source_sha256'])
 start=record['span_start'];end=start+record['span_length']
 result=text.encode('utf-16-le')[start*2:end*2].decode('utf-16-le')
 if sha(result.encode('utf-8'))!=record['body_sha256']:raise ValueError('Whole Roslyn body hash does not match extracted span')
 return result,policy
