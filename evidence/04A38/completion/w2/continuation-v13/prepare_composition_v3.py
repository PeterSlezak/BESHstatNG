from common import *
source=HERE/'verify_c13_composition_v2.py';v=source.read_text()
v=v.replace("expected=original_body.replace('AsString('+argument+')','arg')","expected=original_body.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+argument+')',argument)")
v=v.replace("tokens(unwrap_cases(phase(target_body)))","tokens(re.sub(r'LegacyUdfText38.EqualText\\(token, (\"[^\"]+\")\\)',r'token = \\1',unwrap_cases(phase(target_body))))")
with (HERE/'verify_c13_composition_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
save('composition-option-binding-correction-v1.json',dict(before=pin(source),after=pin(HERE/'verify_c13_composition_v3.py'),
    reason='Bind the actual fully qualified original host AsString expression to its original parameter name. ParseRocDirection current grammar uses the unchanged explicit LegacyUdfText38.EqualText policy for four exact literal comparisons. Full source comparison remains mandatory.',original_queries=0))
