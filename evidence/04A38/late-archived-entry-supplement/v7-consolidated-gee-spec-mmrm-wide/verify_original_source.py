"""Independent live Git/source-map/compilation-input authentication.

No builder, instrumenter, capture, or source-authentication producer imported.
"""
import io,json,re,subprocess,tarfile
from pathlib import Path
from verify_inputs import ROOT,PREFIX,BASE,need,load,digest

def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX
 raw=subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),'archive','--format=tar',BASE],cwd=root)
 blobs={}
 with tarfile.open(fileobj=io.BytesIO(raw),mode='r:') as archive:
  for item in archive:
   if item.isfile():blobs[item.name]=archive.extractfile(item).read()
 need(len(blobs)==4695,'Wrong original Git tree')
 entry=load(here/'entry-inspection.json')
 need(digest(raw)==entry['original_git_archive_sha256'],'Original Git archive identity changed')
 checked=0;maps_checked=0
 for config in ['Debug','Release']:
  for mode in ['plain','trace']:
   info=load(here/('compile-original-'+config+'-'+mode+'.json'))
   for row in info['sources']:
    p=row['path']
    if p.startswith('<ORIGINAL_TREE>/'):
     data=blobs[p.removeprefix('<ORIGINAL_TREE>/')]
     need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Compiled source differs from pinned Git: '+p);checked+=1
    elif mode=='trace' and p in ['<ISOLATED_HARNESS>/UdfDataImportMixedModel.vb','<ISOLATED_HARNESS>/MixedModelUDFs.vb','<ISOLATED_HARNESS>/GeeUDFs.vb']:
     filename=p.rsplit('/',1)[1];sourcepath=next(m['source_path'] for m in info['instrumentation'] if m['source_path'].endswith('/'+filename))
     text=blobs[sourcepath].decode('utf-8-sig').replace('\r\n','\n');original=text
     groups={}
     for record in info['instrumentation']:
      if record['source_path']==sourcepath:groups.setdefault(record['member'],[]).append(record)
     for name,records in groups.items():
      match=re.search(r'(?m)^\s*(?:Private|Friend|Public) (?:Function|Sub) '+re.escape(name)+r'\b[\s\S]*?^\s*End (?:Function|Sub)',text)
      need(match is not None,'Source map member absent');block=match[0]
      original_start=original.index(('Sub ' if 'Sub '+name in original else 'Function ')+name)
      for record in records:
       anchor=record['source_anchor'];need(block.count(anchor)==1,'Ambiguous source-path witness')
       need(record['archived_source_line']==original[:original.index(anchor,original_start)].count('\n')+1,'Changed archived line map')
       hook='MmrmBranch38.Hit("'+record['event']+'", '+record['value_expression']+')'
       replacement=anchor+'\n'+hook if record['after_anchor'] else hook+'\n'+anchor
       block=block.replace(anchor,replacement,1);maps_checked+=1
      text=text[:match.start()]+block+text[match.end():]
     data=text.encode('utf-8');need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Instrumented library source not exact observational overlay')
    elif p.startswith('evidence/04A38/preedit/native-safety/'):
     data=(root/p).read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Frozen native/fixture slice changed')
    else:need(p=='<ISOLATED_HARNESS>/MmrmBranch38.vb','Foreign compilation input')
 fixture=(root/'evidence/04A38/preedit/native-safety/FixtureInputs38.vb').read_text(encoding='utf-8')
 source=blobs['BESHStatNG.Test/src/MixedModels/MixedModelOutputAndUdfTests.vb'].decode('utf-8-sig').replace('\r\n','\n')
 for name in ['BuildSyntheticMmrmInputs','BuildSyntheticLmmInputs']:
  original=re.search(r'^\s*Private Shared Sub '+name+r'\([\s\S]*?^\s*End Sub',source,re.M)[0]
  need(original.replace('Private Shared Sub '+name,'Public Shared Sub '+name,1).strip() in fixture,'Original input fixture changed')
 return dict(status='PINNED_ORIGINAL_AND_OBSERVATIONAL_MAP_PASS',git_files=4695,production_compilation_inputs_verified=checked,source_map_hooks_verified=maps_checked,
      statistical_logic_substitutions=0,native='NOT_RUN')
if __name__=='__main__':print(json.dumps(inspect()))
