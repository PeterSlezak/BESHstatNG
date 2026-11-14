from common import *
import re
sys.path.insert(0,str(PREVIOUS))
from verify_names import member,body,tokens
originals=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'];old=json.loads((PREVIOUS/'historical-ownership.json').read_bytes())['members'];optionfile=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb';opt=optionfile.read_text(encoding='utf-8-sig').replace('\r\n','\n');rows=[]
for path,owner,target,first in [('BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs','MultivariateAnalysisUDFsOptions38','v'),('BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportAgreement.vb','BESHStatNG.UdfDataImport','UdfDataImportOptions38','arg'),('BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb','BESHStatNG.WorksheetFunctions.ParametricUDFs','ParametricUDFsOptions38','arg')]:
 src=ROOT/next(r for r in originals if r['source']['path']==path)['snapshot']['path'];original=src.read_text(encoding='utf-8-sig').replace('\r\n','\n');native=(ROOT/path).read_text(encoding='utf-8-sig').replace('\r\n','\n')
 for h in old:
  if h['path']!=path or h['proof'] or not h['name'].startswith('Parse'):continue
  name=h['name']
  if path.endswith('UdfDataImportAgreement.vb') and name not in ['ParseAgreementCiMethod','ParseBlandAltmanMode','ParseBlandAltmanScale','ParseBlandAltmanXAxisMode','ParseBlandAltmanPlotMode','ParseKappaWeighting','ParseDemingVarianceModel','ParseIccModel']:continue
  if target=='ParametricUDFsOptions38' and name!='ParseEqualVarianceMode':continue
  a=body(member(original,name));b=body(member(opt,name));matches=re.findall(r'Select Case LegacyUdfText38.CaseToken\((\w+), (?:New String\(\) )?\{([^\n]*)\}\)',b);choices=[]
  for var,items in matches:
   choices.extend(re.findall(r'"((?:""|[^"])*)"',items));b=b.replace('Select Case LegacyUdfText38.CaseToken('+var+', New String() {'+items+'})','Select Case '+var).replace('Select Case LegacyUdfText38.CaseToken('+var+', {'+items+'})','Select Case '+var)
  b=re.sub(r'LegacyUdfText38.EqualText\((\w+), ("(?:""|[^"])*")\)',r'\1 = \2',b)
  assert tokens(a)==tokens(b),'Option body/source alias-return-error mismatch '+name
  assert re.search(r'^\s*Dim \w+ As String = NormalizeToken\('+first+r'\)',a),'Pre-conversion operation or unproved normalizer '+name
  wrapper=body(member(native,name));assert len(wrapper.splitlines())==1 and 'AsString('+first+')' in wrapper and target+'.'+name in wrapper
  rows.append(dict(original_id=h['canonical_original_id'],path=path,name=name,native_owner=owner,common_owner='BESHStatNG.ExcelCommon.'+target,choices=choices,source=pin(src),native=pin(ROOT/path),common=pin(optionfile),first_argument=first,full_source_operations_equivalent=True))
assert len(rows)==30,(len(rows),[r['name'] for r in rows])
save('option-body-inventory.json',dict(status='SOURCE_COMPOSITION_CANDIDATES_NEED_CURRENT_PROBE_AND_CONTEXT',rows=rows,scope='All21multivariate+8agreement+1equalvariance; no unrelated parent closure'))
save('option-probe-packets.json',dict(classification='CURRENT_ONLY_NOT_ARCHIVED_REFERENCE',rows=rows,original_calls=0))
print('Whole-source candidates',len(rows))
