"""Fresh passive Windows export/IL and syntax inspection for a source slice."""
import shutil
from common import *
def main():
 stage=sys.argv[1];folder=PRIVATE/stage/'inspection';folder.mkdir(exist_ok=False)
 records=[];facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 for name,src in [('Inspect38',ROOT/'evidence/04A38/helpers/Inspect38.cs'),('Closure38',ROOT/'evidence/04A38/helpers/PortableConsumerClosure38.cs'),('MethodBodies38',W2/'MethodBodies-v2.cs')]:
  exe=folder/(name+'.exe');r=command(stage+'-'+name+'-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(facade),'/out:'+str(exe),str(src)]);assert r['exit_code']==0;records.append(clean(r))
  for config in ['Debug','Release']:
   lib=PRIVATE/stage/config/'lib';out=HERE/'inspection'/stage/(name+'-'+config+'.json');out.parent.mkdir(parents=True,exist_ok=True)
   if name=='Inspect38':args=[str(lib),str(out),str(ROOT/'evidence/04A38/entry-members.json')]
   elif name=='Closure38':args=[str(lib),str(ROOT/'evidence/04A38/entry-members.json'),str(out)]
   else:args=[str(lib),str(out)]
   r=command(stage+'-'+name+'-'+config,[str(exe),*args]);assert r['exit_code']==0;records.append(clean(r))
 # Source identity counts must be refreshed after moves; no original census/query.
 census=RUNROOT/'mmrm38-v6/w1-census-tool-v2/bin/Census.dll';assert census.is_file()
 r=command(stage+'-current-syntax',['dotnet',str(census),str(ROOT),str(HERE/('current-universe-'+stage+'.json'))]);assert r['exit_code']==0;records.append(clean(r))
 before=json.loads((HERE/'implementation-plan-v2.json').read_bytes())['source_before'];after=[]
 for row in before:
  src=ROOT/row['live']['path'];dst=HERE/'source-after'/stage/(row['live']['path']+'.txt');dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst)
  after.append(dict(path=row['live']['path'],before=row['live'],preimage=row['preimage'],after=pin(src),snapshot=pin(dst)))
 save('source-transitions-'+stage+'.json',dict(stage=stage,files=after,Core_formatter=pin(ROOT/'BESHStat.Core/src/RegModels/MixedModels/MixedModelPostEstimation.vb'),existing_Core_mask=pin(ROOT/'BESHStat.Core/src/WorksheetFunctions/UdfProfilePreparation38.vb'),original_queries=0))
 exports=[]
 for c in ['Debug','Release']:
  a=json.loads((ROOT/'evidence/04A38/preedit'/('compiled-'+c+'.json')).read_bytes())['exports'];b=json.loads((HERE/'inspection'/stage/('Inspect38-'+c+'.json')).read_bytes())['exports'];key=lambda r:r['effective_excel_name']
  assert len(b)==268 and sorted(a,key=key)==sorted(b,key=key);exports.append(dict(configuration=c,exact=268))
 save('inspection-'+stage+'.json',dict(commands=records,exports=exports,metadata_only=True,original_queries=0));print('Fresh exports268 exact each; CIL/syntax/source transitions recorded',stage)
if __name__=='__main__':main()
