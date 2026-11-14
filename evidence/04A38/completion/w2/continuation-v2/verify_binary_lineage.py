"""Fresh passive original/current loaded-DLL hash binding to immutable G/build receipts."""
from common import *
G=ROOT/'evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide'
records=[]
for c in ['Debug','Release']:
 original=json.loads((G/('build-original-'+c+'.json')).read_bytes());print(c,'original-receipt-keys',list(original))
 for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll']:
  p=RUNROOT/'consolidated38-v7/original-build'/c/name;rec=library_pin(p)
  # Search only the exact immutable build receipt, never mutable current output.
  def candidates(x):
   if isinstance(x,dict):
    if x.get('sha256')==rec['sha256']:yield x
    for v in x.values():yield from candidates(v)
   elif isinstance(x,list):
    for v in x:yield from candidates(v)
  matches=list(candidates(original));assert len(matches)>=1,'Original loaded binary hash not in approved immutable G receipt '+name
  meta=json.loads((HERE/'metadata-v4/b1-v1'/('original-'+c+'.json')).read_bytes());assembly=next(r for r in meta['assemblies'] if r['file_name']==name);assert assembly['sha256']==rec['sha256'] and assembly['bytes']==rec['bytes']
  now=PRIVATE/'b1-v1'/c/'lib'/name;current=library_pin(now);build=json.loads((HERE/('build-b1-v1-'+c+'.json')).read_bytes());assert current in build['assemblies']
  observed=json.loads((HERE/'metadata-v4/b1-v1'/('current-'+c+'.json')).read_bytes());assert next(r for r in observed['assemblies'] if r['file_name']==name)['sha256']==current['sha256']
  records.append(dict(configuration=c,name=name,original=rec,original_G_receipt=pin(G/('build-original-'+c+'.json')),current=current,original_module=assembly['MVID'],current_module=next(r['MVID'] for r in observed['assemblies'] if r['file_name']==name),original_build_or_query='NOT_EXECUTED;METADATA_AND_HASH_ONLY'))
engine=ROOT/'BESHStat.Core/src/BaseStat/Distributions.vb';raw=git('cat-file','blob',BASE+':BESHStat.Core/src/BaseStat/Distributions.vb');assert raw.replace(b'\r\n',b'\n')==engine.read_bytes().replace(b'\r\n',b'\n')
save('binary-source-lineage.json',dict(status='IMMUTABLE_G_ORIGINAL_AND_FRESH_CURRENT_BINARY_IDENTITIES_BOUND',records=records,numerical_engine=pin(engine),engine_pinned_Git_sha256=sha(raw),engine_identity='RAW' if raw==engine.read_bytes() else 'CRLF_LF_ONLY',original_queries=0,original_compilations=0))
