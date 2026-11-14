from common import *
import re
h=json.loads((PREVIOUS/'historical-ownership.json').read_bytes())['members']
source=(ROOT/'BESHstatNG/src/ExcelUDFs/DistributionUDFs.vb').read_text(encoding='utf-8-sig')
for r in h:
    if r['path'].endswith('DistributionUDFs.vb'):
        print('CLOSED' if r['proof'] else 'OPEN',r['name'])
for match in re.finditer(r'^([ \t]*)(?:Public|Private|Friend) Function (\w+)\b',source,re.M):
    end=source.index('\n'+match[1]+'End Function',match.end())+len('\n'+match[1]+'End Function')
    s=source[match.start():end]
    if len(s)<1500:print(s)
