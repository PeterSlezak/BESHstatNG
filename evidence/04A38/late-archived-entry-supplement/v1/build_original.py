"""Offline compile of pristine original Core/Common/Windows; no restore/packaging."""
import json,shutil,sys
from pathlib import Path

ROOT=Path(__file__).resolve().parents[4]
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from run import m,PRIVATE as RUNROOT
from baseline_report import clean
from prepare_original import HERE,TREE,PRIVATE,BASE,sha,save
from resume_original import extended

def file_pin(path,role):
 data=path.read_bytes();return dict(role=role,name=path.name,bytes=len(data),sha256=sha(data))

def main():
 config=sys.argv[1];assert config in ['Debug','Release']
 auth=json.loads((HERE/'original-source-authenticity.json').read_bytes());assert auth['source_commit']==BASE
 assert not (TREE/'BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb').exists()
 finish_existing=len(sys.argv)>2 and sys.argv[2]=='--finish-existing'
 output=PRIVATE/'original-build'/config
 if finish_existing:assert output.exists() and config=='Debug'
 else:output.mkdir(parents=True,exist_ok=False)
 dependencies=[]
 # Only pre-existing offline third-party packages are copied. Original BESHStat
 # production assemblies are rebuilt below, never copied from current output.
 package_root=TREE/'packages'
 if not package_root.exists():
  for source in (ROOT/'packages').rglob('*'):
   if not source.is_file():continue
   target=package_root/source.relative_to(ROOT/'packages');target.parent.mkdir(parents=True,exist_ok=True)
   with target.open('xb') as stream:stream.write(source.read_bytes())
 for path in sorted(package_root.rglob('*')):
  if path.is_file():dependencies.append(dict(path='packages/'+path.relative_to(package_root).as_posix(),bytes=path.stat().st_size,sha256=sha(path.read_bytes()),role='EXISTING_OFFLINE_THIRD_PARTY_CACHE; NOT_GIT_SOURCE'))
 for project in ['BESHStat.Core','BESHStat.Excel.Common']:
  target=TREE/project/'obj';target.mkdir(exist_ok=True)
  for name in ['project.assets.json',project+'.vbproj.nuget.g.props',project+'.vbproj.nuget.g.targets']:
   source=ROOT/project/'obj'/name;destination=target/name
   if destination.exists():assert destination.read_bytes()==source.read_bytes()
   else:
    with destination.open('xb') as stream:stream.write(source.read_bytes())
   dependencies.append(file_pin(source,'EXISTING_OFFLINE_SDK_ASSET_METADATA; no source/calculation or current BESHStat DLL'))
 runner=m.Runner(RUNROOT);commands=[]
 for label,project in ([] if finish_existing else [('core','BESHStat.Core/BESHStat.Core.vbproj'),('common','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj')]):
  args=['dotnet','msbuild',str(TREE/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false',
   '/p:OutputPath='+str(output)+'\\','/p:IntermediateOutputPath='+str(PRIVATE/'original-obj'/config/label)+'\\','/m:4','/v:minimal','/nr:false']
  result=runner.command('late38-original-'+config+'-'+label,args,cwd=TREE);commands.append(result)
  assert result['exit_code']==0,(config,label)
 args=[runner.msbuild,str(TREE/'BESHstatNG/BESHstatNG.vbproj'),'/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory',
  '/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(output)+'\\',
  '/p:IntermediateOutputPath='+str(PRIVATE/'original-obj'/config/'host')+'\\','/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']
 if finish_existing:
  commands=[json.loads((RUNROOT/'commands'/('late38-original-'+config+'-'+label)/'command.json').read_bytes()) for label in ['core','common','host']]
  assert all(row['exit_code']==0 for row in commands)
 else:
  result=runner.command('late38-original-'+config+'-host',args,cwd=TREE);commands.append(result);assert result['exit_code']==0
 dna=package_root/'ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll'
 target=output/dna.name
 if target.exists():assert target.read_bytes()==dna.read_bytes()
 else:
  with target.open('xb') as stream:stream.write(dna.read_bytes())
 # Recheck every exported original source byte after compilation. Generated
 # obj/output files are not mislabeled as original Git content.
 hidden={}
 for item in json.loads((ROOT/'evidence/04A38/entry-source-pins.json').read_bytes())['files']:
  if item['path'].startswith('<PRIVATE_PROTECTED_PATH:'):hidden[item['path']]=None
 from prepare_original import git
 for real in git('ls-tree','-r','--name-only','-z',BASE).decode().split('\0'):
  if real:
   token='<PRIVATE_PROTECTED_PATH:'+sha(real.encode())[:24]+'>'
   if token in hidden:hidden[token]=real
 for row in auth['files']:
  path=hidden[row['path']] if row['path'].startswith('<PRIVATE_PROTECTED_PATH:') else row['path']
  data=extended(TREE/path).read_bytes();assert sha(data)==row['sha256']
 libraries=[file_pin(path,'COMPILED_PINNED_ORIGINAL' if path.name.startswith('BESHStat') else 'OFFLINE_RUNTIME_DEPENDENCY') for path in sorted(output.glob('*.dll'))]
 assert all((output/name).exists() for name in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll','BESHStatNG.dll','ExcelDna.Integration.dll'])
 save('build-original-'+config+'.json',dict(status='PINNED_ORIGINAL_COMPILED_OFFLINE',source_commit=BASE,configuration=config,source_tree_rechecked=len(auth['files']),
  output='<ISOLATED_ORIGINAL_BUILD>/'+config,assemblies=libraries,offline_inputs=dependencies,commands=clean(commands),
  native='NOT_RUN',restore='NOT_RUN',packaging='NOT_RUN',scientific='NOT_GOLDEN',chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY'))
 print('Compiled original',config,'Core/Common/Windows from pinned tree; sources rechecked; no current production DLL input')

if __name__=='__main__':main()
