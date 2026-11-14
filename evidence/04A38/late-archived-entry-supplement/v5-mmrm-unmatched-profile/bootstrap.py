"""Copy reusable recorder/build mechanics into a separate, unoccupied v5 packet."""
from pathlib import Path
HERE=Path(__file__).resolve().parent
OLD=HERE.parent/'v4-mmrm-duplicate-guards'
for name in ['common.py','build_original.py','compile_harness.py','MaterializeMmrm38.cs','MmrmCapture38.cs','verify_original_source.py','execute_original.py']:
 text=(OLD/name).read_text(encoding='utf-8').replace('mmrm38-v4','mmrm38-v5').replace('v4 evidence','v5 evidence')
 if name=='common.py':
  text=text.replace("V3=ROOT/", "V4=ROOT/'evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards'\nV3=ROOT/")
  text=text.replace("PREVIOUS='evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile/", "PREVIOUS='evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
