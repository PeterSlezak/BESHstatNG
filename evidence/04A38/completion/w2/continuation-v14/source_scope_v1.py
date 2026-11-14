"""Reverse exact native edits; prove unchanged inputs and existing Common API."""
from common import *
draft='--draft' in sys.argv
out=PRIVATE/'draft-proof-v1' if draft else HERE
trans=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'];plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());moves=json.loads((HERE/'implementation-source-composition-v1.json').read_bytes())
decode=lambda b:b.decode('utf-8-sig').replace('\r\n','\n');by={r['path']:r for r in trans};group={}
for m in moves['phases']:group.setdefault(m['path'],[]).append(m)
for path,items in group.items():
 text=decode((ROOT/path).read_bytes())
 for m in reversed(items):
  new=m.get('actual_native_call',m.get('current_whole_prefix'));old=m.get('original_segment',m.get('original_whole_prefix'));assert text.count(new)==1; text=text.replace(new,old,1)
 assert text==decode((ROOT/by[path]['preimage']['path']).read_bytes()),'Unexpected source edit '+path
path='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb';text=decode((ROOT/path).read_bytes());assert text.count(moves['ROC_current_native_target'])==1;text=text.replace(moves['ROC_current_native_target'],moves['ROC_original_native_target'],1);assert text==decode((ROOT/by[path]['preimage']['path']).read_bytes())
path='BESHStat.Excel.Common/src/Analysis/UdfClassLabelPreparation38.vb';text=decode((ROOT/path).read_bytes());assert text.count(moves['ROC_new_method'])==1;text=text.replace(moves['ROC_new_method'],'',1);assert text==decode((ROOT/by[path]['preimage']['path']).read_bytes()),'Existing Common resetting API changed'
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';old=decode((ROOT/by[path]['preimage']['path']).read_bytes());prefix=old[:old.rfind('End Class')];assert decode((ROOT/path).read_bytes()).startswith(prefix),'Historical adapter assertions changed'
approved=set(by);unchanged=[]
for row in json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files']:
 if row['live']['path'] not in approved:assert pin(ROOT/row['live']['path'])==row['live'];unchanged.append(row['live'])
for t in trans:assert pin(ROOT/t['path'])==t['after'] and (ROOT/t['path']).read_bytes()==(ROOT/t['snapshot']['path']).read_bytes()
result=dict(status='EXACT_REVERSE_NATIVE_AND_COMMON_ADDITION_SCOPE_PASS',unchanged_initial_paths=unchanged,source_transitions=pin(HERE/'source-transitions-final-v1.json'),historical_assertions_changed=0,Core_formulas_changed=0,original_queries=0)
with (out/'source-transition-independent-proof-v1.json').open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
print('Native prefix/body/case edits reversed exactly; old Common API and all unapproved paths/assertions unchanged')
