from common46 import *
import re
lock();p='BESHstatNG/src/DM/UIprocedures.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');blocks=[];records=[]
for name in ['TryParseUiDouble','ParseUiDouble','FormatUiDouble','CheckNumeric']:
 m=re.search(r'(?ms)^    Public Function '+name+r'\(.*?^    End Function',s);assert m;blocks.append(m.group());records.append(dict(name=name,source=pin(p),body_sha256=sha(m.group().encode()),text=m.group()))
head='''Option Strict Off
Option Explicit On
Imports System
Imports System.Globalization
Namespace Harness46
Friend Module SourceNumbers46
 Friend HostCulture As CultureInfo = CultureInfo.InvariantCulture
 Friend Function GetExcelNumberCulture() As CultureInfo
  Return HostCulture
 End Function
'''
target=ROOT/'BESHStatNG.Test/src/GuiWiring46Numbers.vb'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(head+'\n'.join(blocks)+'\nEnd Module\nEnd Namespace\n')
save('fixtures/numeric-source-v1.json',dict(members=records,projection=pin(target),qualification='Exact existing Windows CheckNumeric Object/late-binding semantics retained in a test-only module with Option Strict Off. Only closed fake Text/BackColor objects supplied;explicit fake host culture;unchanged NumberTextPolicy reused. No native AppGlobals acquisition or normalized parser.',native='NOT_RUN'))
