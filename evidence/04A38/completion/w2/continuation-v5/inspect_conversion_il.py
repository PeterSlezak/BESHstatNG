from common import *
for role in ['original','current']:
    d=json.loads((HERE/'dependency-metadata-v7'/(role+'-Debug.json')).read_bytes())
    for owner,name in [('BESHStatNG.WorksheetFunctions.ExcelArgReaders','CellToTrimmedText'),('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble'),
                       ('BESHStatNG.ExcelCommon.UdfCellConversion38','CellToTrimmedText'),('BESHStatNG.ExcelCommon.UdfCellConversion38','ConvertFiniteNumber')]:
        matches=[m for m in d['methods'] if m['declaring_type']==owner and m['member_name']==name]
        for m in matches:
            print(role,owner,name,'\n',json.dumps(m['instructions']))
