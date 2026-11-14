from common import *
from verify_slice import body,member,tokens
o=(ROOT/'evidence/04A38/preedit/source/DistributionUDFs.vb.txt').read_text(encoding='utf-8-sig');n=(ROOT/'BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb').read_text(encoding='utf-8-sig')
s=body(member(o,'PRTRNG')).replace('ExcelError.ExcelErrorNum','Nothing').replace('iFault','fault').replace('Dim p As Double','Dim probability As Double').replace('IsNaN(p)','IsNaN(probability)').replace('IsInfinity(p)','IsInfinity(probability)').replace('Return p','Return probability')
print('expected',s);print('actual',body(member(n,'PrepareStudentizedRangeCdf')))
for name in ['text-domains-a1-v2-v2-Debug.json','compatibility-b1-v1.json']:
 x=json.loads((HERE/name).read_bytes());print(name,list(x));
 if 'results' in x:print('firstrowkeys',list(x['results'][0]))
