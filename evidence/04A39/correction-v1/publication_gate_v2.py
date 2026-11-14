"""Encoding-aware publication scan. Raw historical UTF8 scanner ERROR remains ERROR.

Exact frozen legacy source bytes stay in the package. Both strict, round-tripping
Central-European and Western Windows text interpretations are scanned, separately.
Unknown non-UTF8 files and any sensitive match without exact disposition abort.
"""
from publication_gate import safe_name, physical_inventory, StageRoot
from scanner_adapter import module, detailed
from common import *

class TextPath:
    def __init__(self,path,codec):self.path=path;self.codec=codec
    def is_file(self):return self.path.is_file()
    def read_text(self,encoding):
        assert encoding=='utf-8-sig'
        raw=self.path.read_bytes();text=raw.decode(self.codec,errors='strict')
        assert text.encode(self.codec)==raw,'ENCODING_ROUNDTRIP_FAILURE'
        return text
class EncodedRoot(StageRoot):
    def __init__(self,stage,codec):super().__init__(stage);self.codec=codec
    def __truediv__(self,path):return TextPath(self.stage/path,self.codec)

def scan(stage,names,source_pins):
    sources={r['path']:r for r in source_pins}
    utf8=[];legacy=[];encodings=[]
    for name in names:
        data=(Path(stage)/name).read_bytes()
        try:data.decode('utf-8-sig',errors='strict');utf8.append(name)
        except UnicodeDecodeError:
            assert name in sources and sha(data)==sources[name]['sha256'],'UNKNOWN_NON_UTF8_PAYLOAD'
            for codec in ('cp1250','cp1252'):
                assert data.decode(codec,errors='strict').encode(codec)==data
            legacy.append(name);encodings.append(dict(path=name,sha256=sha(data),privacy_views=['cp1250','cp1252'],original_bytes_preserved=True))
    findings=[];details=[];reviewed=[]
    for codec,paths in [('utf-8-sig',utf8),('cp1250',legacy),('cp1252',legacy)]:
        m=module('13');m.ROOT=StageRoot(stage) if codec=='utf-8-sig' else EncodedRoot(stage,codec)
        f,r,d=detailed(m,paths)
        findings.extend(f);reviewed.extend(r)
        details.extend(dict(row,encoding_view=codec) for row in d)
    return findings,reviewed,details,encodings

def finding_key(row,file_sha):
    return (row['path'],file_sha,row['origin'],row.get('line'),row['category'],row['offset'],row['match_sha256'],row['encoding_view'])

def decide(findings,details,files,reviewed):
    bypath={r['path']:r for r in files};allowed={tuple(r['key']):r for r in reviewed}
    assert len(allowed)==len(reviewed),'DUPLICATE_DISPOSITION'
    unresolved=[];used=set()
    for row in details:
        key=finding_key(row,bypath[row['path']]['sha256'])
        if key not in allowed:unresolved.append(row)
        else:
            entry=allowed[key]
            assert entry['disposition']=='REVIEWED_PUBLIC_OR_SYNTHETIC_SOURCE_LITERAL' and entry['proof'] and entry['source']
            used.add(key)
    assert used==set(allowed),'STALE_OR_BROAD_ALLOWANCE'
    assert not unresolved,'UNRESOLVED_SENSITIVE_MATERIAL'
    return dict(scanner_findings=len(findings),reviewed_exact_occurrences=len(used),unresolved=0,privacy_status='PASS_STAGED_DERIVATIVE_ONLY',repository_candidate_status='FAIL_UNWAIVED',historical_utf8_gate_status='ERROR_LEGACY_SOURCE_ENCODING_UNWAIVED')
