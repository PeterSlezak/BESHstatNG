"""Actual13 publication scanner on memory-bound payloads; sanitize only separately named transport derivatives."""
import importlib.util
import os
from pathlib import Path
import re
import unicodedata

ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('actual_publication13_privacy39',ROOT/'scripts/verify-04a13-publication.py')
actual=importlib.util.module_from_spec(spec);spec.loader.exec_module(actual)
SOURCE=actual.scan.__code__

class Payload:
    def __init__(self,data):self.data=data
    def is_file(self):return True
    def read_text(self,encoding):return self.data.decode(encoding)

class MemoryRoot:
    parents=ROOT.parents
    name=ROOT.name
    def __init__(self,path,data):self.path=path;self.data=data
    def __truediv__(self,path):
        assert path==self.path
        return Payload(self.data)
    def __fspath__(self):return str(ROOT)
    def as_posix(self):return ROOT.as_posix()

def scan(path,data):
    before=actual.ROOT
    try:
        actual.ROOT=MemoryRoot(path,data)
        assert actual.scan.__code__ is SOURCE
        return actual.scan([path])[0]
    finally:actual.ROOT=before

TOKENS={p.name for p in ROOT.parents if p.parent.name.casefold()=='users'}
if os.environ.get('COMPUTERNAME'):TOKENS.add(os.environ['COMPUTERNAME'])
PATTERNS={
    'home':r'(?i)(?:[a-z]:[/\\]+users[/\\]+[^/\\\s"<>]+|[/\\](?:home|Users)[/\\]+[^/\\\s"<>]+)',
    'drive':r'(?i)\b[a-z]:[/\\]+[A-Za-z0-9_][^\s"<>]*',
    'unc':r'\\\\[A-Za-z0-9_.-]+\\[A-Za-z0-9_$-]+',
    'sid':r'S-1-5-21-\d+-\d+-\d+-\d+',
    'email':r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b',
    'secret':r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY|\b(?:gh[pousr]_[A-Za-z0-9]{20,}|AKIA[A-Z0-9]{16}|sk-[A-Za-z0-9]{24,})',
    'credential':r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@',
    'connection':r'(?i)(?:password|pwd|accountkey|client_secret)\s*[=:]\s*[A-Za-z0-9+/]{8,}',
    'internal':r'(?i)\b(?:https?://)?[A-Za-z0-9_-]+\.(?:local|internal|corp)\b'}

def redact_string(text):
    text=unicodedata.normalize('NFKC',text)
    for token in sorted(TOKENS,key=len,reverse=True):
        text=re.sub(r'(?i)(?<![A-Za-z0-9])'+re.escape(token)+r'(?![A-Za-z0-9])','<PRIVATE_IDENTIFIER>',text)
    for label,pattern in PATTERNS.items():text=re.sub(pattern,'<REDACTED_'+label.upper()+'>',text)
    return text

def redact(value):
    if isinstance(value,str):return redact_string(value)
    if isinstance(value,list):return [redact(x) for x in value]
    if isinstance(value,dict):
        pairs=[(redact_string(k),redact(v)) for k,v in value.items()]
        assert len({k for k,v in pairs})==len(pairs),'Redacted key collision; do not replace authority silently'
        return dict(pairs)
    return value
