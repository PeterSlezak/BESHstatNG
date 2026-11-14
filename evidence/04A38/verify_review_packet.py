"""Independent local NOT_READY review-packet reader; standard library only.

This bounded reader verifies payload integrity, fixed chronology anchors and
preservation. It deliberately cannot certify completed migration ownership or
issue READY. A later acceptance reader must implement that remaining gate.
No generator, implementation or test producer is imported.
"""
import argparse,csv,hashlib,json,re
from pathlib import Path,PurePosixPath

SELF='evidence/04A38/publication-manifest.json'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
GATES={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'}
ANCHORS={
 'evidence/04A38/preedit/frozen-v2/receipt.json':'9f9a55337311bf37e010dcc078cb2ff8a8384122c1b105c4593732e90e13d165',
 'evidence/04A38/preedit/addendum-frozen-v1/receipt.json':'35c790471841ce33fd2857ca881ff84103b2856cf61139fe1312bf002eb30646',
}
class InvalidPacket(ValueError):pass
def require(condition,message):
 if not condition:raise InvalidPacket(message)
def digest(data):return hashlib.sha256(data).hexdigest()
def read_json(path):
 def unique(pairs):
  result={}
  for key,value in pairs:
   if key in result:raise InvalidPacket('Repeated JSON key')
   result[key]=value
  return result
 return json.loads(path.read_text(encoding='utf-8'),object_pairs_hook=unique)

def safe_path(value):
 require(isinstance(value,str) and bool(value),'Missing path')
 p=PurePosixPath(value)
 require(not p.is_absolute() and '\\' not in value and ':' not in value,'Absolute or noncanonical path')
 require(p.as_posix()==value and all(part not in {'','.','..'} for part in p.parts),'Traversal/noncanonical path')
 require(all(not part.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',part) for part in p.parts),'Invalid Windows path')
 require(all(not re.fullmatch(r'(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\..*)?',part,re.I) for part in p.parts),'Reserved Windows path')
 permitted=value.startswith(('evidence/04A38/','BESHstatNG/src/ExcelUDFs/','BESHStat.Core/src/WorksheetFunctions/'))
 permitted=permitted or value.startswith(('BESHStat.Excel.Common/src/Analysis/Udf','BESHStat.Excel.Common/src/Results/Udf'))
 permitted=permitted or value in {
  'BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb','BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb',
  'BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb',
  'BESHStatNG.Test/BESHStatNG.Tests.vbproj','docs/architecture/batch-04A-38-owner-review.md',
  'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb','BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb'}
 require(permitted,'Foreign/owner-document publication path')
 return p

def check_entries(root,document):
 require(isinstance(document,dict) and document.get('status')=='NOT_READY_04A38','This reader cannot approve a READY claim')
 entries=document.get('entries');require(isinstance(entries,list) and bool(entries),'Missing entries')
 seen=set();self_count=0;size=0
 paths=[row.get('path') for row in entries if isinstance(row,dict)]
 require(len(paths)==len(entries) and paths==sorted(paths),'Malformed/reordered manifest entries')
 for row in entries:
  relative=row['path'];p=safe_path(relative);folded=relative.casefold()
  require(folded not in seen,'Duplicate/casefold-colliding path');seen.add(folded)
  require(set(row)=={'path','bytes','sha256'},'Malformed entry schema')
  if relative==SELF:
   self_count+=1;require(row['bytes'] is None and row['sha256'] is None,'Recursive/non-null self-entry');continue
  require(isinstance(row['bytes'],int) and not isinstance(row['bytes'],bool) and row['bytes']>=0,'Malformed size')
  require(isinstance(row['sha256'],str) and re.fullmatch('[0-9a-f]{64}',row['sha256']),'Malformed digest')
  target=root.joinpath(*p.parts)
  require(target.resolve().is_relative_to(root.resolve()),'Symlink escapes root')
  require(target.is_file(),'Missing payload: '+relative)
  data=target.read_bytes();require(len(data)==row['bytes'] and digest(data)==row['sha256'],'Corrupt payload: '+relative)
  size+=len(data)
 require(self_count==1,'Exactly one null self-entry required')
 return dict(external_files=len(entries)-1,payload_bytes=size,paths=set(paths))

def verify(root,document=None):
 if document is None:document=read_json(root/SELF)
 integrity=check_entries(root,document);paths=integrity.pop('paths')
 require('docs/architecture/batch-04A-38-owner-review.md' in paths,'Missing owner dossier')
 inventory=read_json(root/'evidence/04A38/file-inventory.json')
 require(set(inventory['paths'])==paths and len(inventory['paths'])==len(paths),'File inventory/manifest omissions or duplicates')
 for path,expected in ANCHORS.items():
  require(digest((root/path).read_bytes())==expected,'Frozen chronology receipt changed')
  receipt=read_json(root/path)
  require(receipt['source_commit']==BASE and receipt['chronology']=='CAPTURED_AND_SEALED_BEFORE_FIRST_04A38_PRODUCT_EDIT' and receipt['late_capture_exception'] is False,'False pre-edit chronology')
  for row in receipt['files']:
   actual=(root/row['path']).read_bytes()
   require(len(actual)==row['bytes'] and digest(actual)==row['sha256'],'Frozen dependency overwritten')
 readiness=read_json(root/'evidence/04A38/readiness.json')
 require(readiness['status']=='NOT_READY_04A38' and readiness['gates']==GATES,'False READY or released gate')
 require(readiness['scientific']=='NOT_GOLDEN' and readiness['native']=='NOT_RUN','Native/scientific claim changed')
 progress=read_json(root/'evidence/04A38/ownership-progress-validation-v1-v2.json')
 require(progress['entry_members']==704 and progress['historical_members']==703 and progress['final_dispositions_closed']==0,'False ownership closure')
 require(progress['classifications']=={'OPEN_REMAINING_MEMBER_BEHAVIOR_REVIEW':505,'PORTED_CONSUMER_EDGE_VERIFIED_REMAINING_BODY_REVIEW_REQUIRED':199},'Progress ledger population changed without independent review')
 with (root/'evidence/04A38/udf-export-inventory.csv').open(encoding='utf-8',newline='') as f:exports=list(csv.DictReader(f))
 require(len(exports)==268,'Missing export')
 names=[r['effective_excel_name'].casefold() for r in exports];require(len(set(names))==268,'Duplicate export')
 for config in ['Debug','Release']:
  folder=root/'evidence/04A38/compatibility/validation-v1'/config/'inspection'
  old=read_json(root/('evidence/04A38/preedit/compiled-'+config+'.json'));current=read_json(folder/'compiled.json')
  require(old['exports']==current['exports'] and len(current['exports'])==268,'Changed registration metadata')
 comparison=read_json(root/'evidence/04A38/compatibility/validation-v1/comparison.json')
 require(comparison['status']=='EXACT_SOURCE_COMPATIBILITY_ONLY' and comparison['rows_per_configuration']==28966 and len(comparison['results'])==10,'Incomplete exact comparisons')
 for row in comparison['results']:
  require(row['exact_bytes'] and not row['mismatches'] and row['native_attempts']==0,'Compatibility/native mismatch')
  reference=root/row['reference']['path'];current=root/row['current']['path']
  require(reference.read_bytes()==current.read_bytes(),'Actual comparison bytes differ')
 preservation=read_json(root/'evidence/04A38/preservation-validation-v2.json')
 require(len(preservation['lineage_pins'])==278 and len(preservation['protected_mixed_model_captures'])==100 and len(preservation['owner_documents_excluded'])==16,'Lost preservation population')
 for row in preservation['lineage_pins']:
  actual=(root/row['path']).read_bytes();require(digest(actual)==row['current']['sha256'],'Current lineage bytes changed')
  if row['status']=='AUTHORIZED_04A38_UDF_SOURCE_TRANSITION':require(row['path'].startswith('BESHstatNG/src/ExcelUDFs/'),'Foreign source transition')
  else:require(row['current']==row['entry'],'False exact source preservation')
 for row in preservation['protected_mixed_model_captures']+preservation['owner_documents_excluded']:
  data=(root/row['path']).read_bytes();require(digest(data)==row['sha256'] and len(data)==row['bytes'],'Protected capture/owner document changed')
 return dict(status='VALID_NOT_READY_REVIEW_PACKET',**integrity,registration_exports=268,exact_rows_per_configuration=28966,lineage_pins=278,protected_captures=100,
  ownership='NOT_CLOSED; this bounded integrity reader cannot authorize READY',native='NOT_RUN',scientific='NOT_GOLDEN')

if __name__=='__main__':
 parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2]);args=parser.parse_args()
 try:print(json.dumps(verify(args.root)))
 except (InvalidPacket,OSError,ValueError,KeyError,TypeError) as error:
  print(json.dumps(dict(status='NOT_READY_04A38',integrity_error=str(error))));raise SystemExit(1)
