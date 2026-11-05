"""Restore entry encoding conventions on authorized modified files only."""
import importlib.util
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parents[1]
s=importlib.util.spec_from_file_location('capture26_encoding',ROOT/'scripts/capture-04a26-contract.py');c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def main():
    rows=[]
    for p in [c.FORM,'BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStatNG_Help_MkDocs/docs/methods/sample-size-icc.md','BESHStatNG_Help_MkDocs/docs/methods/sample-size-bland-altman.md']:
        original=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',c.BASE+':'+p],cwd=ROOT)
        before=(ROOT/p).read_bytes();text=before.decode('utf-8-sig').replace('\r\n','\n')
        if not original.endswith(b'\n'):text=text.rstrip('\n')
        if b'\r\n' in original:text=text.replace('\n','\r\n')
        after=(b'\xef\xbb\xbf' if original.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8')
        assert before.decode('utf-8-sig').replace('\r\n','\n').rstrip('\n')==after.decode('utf-8-sig').replace('\r\n','\n').rstrip('\n')
        (ROOT/p).write_bytes(after)
        rows.append(dict(path=p,before_sha256=__import__('hashlib').sha256(before).hexdigest(),after_sha256=c.sha(ROOT/p),classification='RESTORED_ENTRY_BOM_NEWLINE_CONVENTION / text-content unchanged'))
    c.emit('encoding-restoration.json',dict(files=rows,scope='Only authorized modified files; no history, owner plan or numerical source touched. Windows assembly recompiled and full Windows Debug rerun; Release full qualification uses restored bytes. Strict MkDocs repeated on final Help bytes.'))
if __name__=='__main__':main()
