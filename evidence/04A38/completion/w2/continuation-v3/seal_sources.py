from common import *
entry=json.loads((HERE/'entry.json').read_bytes());after=json.loads((HERE/'source-transitions-names-v3.json').read_bytes())['files'];rows=[]
for r in entry['source_before']:
 a=next(x for x in after if x['after']['path']==r['live']['path']);assert pin(ROOT/r['live']['path'])==a['after']
 rows.append(dict(path=r['live']['path'],before=r['live'],preimage=r['preimage'],after=a['after'],snapshot=a['snapshot']))
save('source-transitions-final.json',dict(files=rows,stage='names-v3',source_before_frozen_before_edit=True,duplicate_removal='Only two unused methods in UdfSpillCell38; canonical implementations and coordinate properties/ctor retained',new_portable_contracts=['UdfNamePreparation38','UdfVariableNameRange38'],original_queries=0,Core_numerics='NO_SOURCE_CHANGE'))
# Original transport and CI formatting code stays exact to accepted main, modulo recorded line endings.
deps=[]
for p in ['BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb','BESHStat.Core/src/StatTests/StatisticalResultContracts.vb']:
 b=(ROOT/p).read_bytes();blob=git('cat-file','blob',BASE+':'+p);assert b.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n');deps.append(dict(current=pin(ROOT/p),Git_sha256=sha(blob),identity='RAW' if blob==b else 'CRLF_LF_ONLY'))
save('unchanged-dependency-pins.json',dict(files=deps,original_queries=0,metadata='Passive metadata-v5:transport and entire ConfidenceIntervalResult/CIformat state/getters/ctors'))
