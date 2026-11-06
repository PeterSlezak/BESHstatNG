"""Independent, read-only 04A33 manifest verifier. Does not import the producer."""
from pathlib import Path
import hashlib,json,re,sys
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
SELF='evidence/04A33/publication-manifest.json'
SOURCES=['BESHstatNG/src/Graphics/XYZscatter.vb', 'BESHstatNG/src/Graphics/ScatterPlotMatrix.vb', 'BESHstatNG/src/Graphics/3Dobjects.vb', 'BESHStat.Core/src/Graphics/XyzGeometryCore.vb', 'BESHStat.Core/src/Graphics/XyzProjectionContext.vb', 'BESHStat.Core/src/Graphics/ScatterMatrixSeriesCore.vb', 'BESHStat.Core/src/Graphics/ScatterMatrixPanelData.vb', 'BESHStat.Core/src/Graphics/WireObjectGeometryCore.vb', 'BESHStat.Core.Tests/src/Graphics/Graphics33Tests.vb', 'BESHStat.Core.Tests/src/Graphics/LegacyXyz33.vb', 'BESHStat.Core.Tests/src/Graphics/LegacyScatter33.vb', 'BESHStat.Core.Tests/src/Graphics/LegacyWire33.vb', 'BESHStatNG.Test/src/Charts/Graphics33AdapterTests.vb', 'BESHStatNG.Test/BESHStatNG.Tests.vbproj', 'docs/architecture/batch-04A-33-owner-review.md']
def expected_scope():
 return set(SOURCES+[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts])
def file_pin(path):
 p=(ROOT/path).resolve();assert p.is_relative_to(ROOT.resolve()) and p.is_file(),'Missing/escaping payload'
 h=hashlib.sha256()
 with p.open('rb') as f:
  for block in iter(lambda:f.read(1024*1024),b''):h.update(block)
 return dict(path=path,bytes=p.stat().st_size,sha256=h.hexdigest())
def validate_rows(rows,expected=None):
 assert isinstance(rows,list) and rows
 paths=[r['path'] for r in rows]
 assert all(isinstance(p,str) and p and '\\' not in p and ':' not in p and not p.startswith('/') and all(x not in ('','.','..') for x in p.split('/')) for p in paths),'Noncanonical path'
 assert len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Duplicate/Windows-case-ambiguous path'
 if expected is not None:assert set(paths)==set(expected),'Missing/foreign publication entry'
 assert paths.count(SELF)==1,'Self entry required'
 for row in rows:
  assert set(row)=={'path','bytes','sha256'}
  if row['path']==SELF:
   assert row['bytes'] is None and row['sha256'] is None,'Recursive self hash forbidden'
  else:
   assert type(row['bytes']) is int and row['bytes']>=0
   assert isinstance(row['sha256'],str) and re.fullmatch('[0-9a-f]{64}',row['sha256'])
   assert row==file_pin(row['path']),'Payload byte/hash mismatch: '+row['path']
def verify_manifest(path=None,expected=None):
 path=Path(path or HERE/'publication-manifest.json');v=json.loads(path.read_bytes())
 assert v['schema']=='04A33-publication-v1'
 validate_rows(v['files'],expected_scope() if expected is None else expected)
 return len(v['files'])
if __name__=='__main__':print('PASS independent manifest verifier;',verify_manifest(),'exact entries;null self;canonical scope')
