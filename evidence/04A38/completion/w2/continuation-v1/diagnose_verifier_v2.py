import difflib
from verify_p1_v2 import *
p=PREFIX+'UDFdataImport/UdfDataImport.vb'
old=(HERE/'source-before/p1'/(p+'.txt')).read_text(encoding='utf-8-sig')
expected=body(member(old,'TryGetNestedThreeColumnData'))
expected=expected[:expected.index('Dim arr As Object(,)')]+expected[expected.index('If arr Is Nothing'):]
expected=re.sub(r'Convert.ToString\((arr\([^\n]*?\))\).Trim\(\)',r'labelText(\1)',expected)
expected=expected.replace('g <> "" AndAlso sg <> ""','Not LegacyUdfText38.EqualText(g, "") AndAlso Not LegacyUdfText38.EqualText(sg, "")').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble','numeric')
now=(ROOT/NUMERIC).read_text(encoding='utf-8-sig');actual=body(member(now,'TryGetNestedThreeColumnData'))
print('\n'.join(difflib.unified_diff(tokens(expected),tokens(actual))))
