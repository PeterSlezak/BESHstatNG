"""Retain exact preimages before changing production or test sources."""
from common import *
import shutil

PRIVATE.mkdir(exist_ok=False)
paths = ['BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb',
         'BESHstatNG/src/ExcelUDFs/UdfOutputTables.vb',
         'BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',
         'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
rows = []
for path in paths:
    src = ROOT / path
    dst = HERE / 'source-before' / (path + '.txt')
    dst.parent.mkdir(parents=True, exist_ok=True)
    with dst.open('xb') as stream:
        stream.write(src.read_bytes())
    rows.append(dict(live=pin(src), preimage=pin(dst)))
save('entry.json', dict(source_before=rows, new_sources=['BESHStat.Excel.Common/src/Analysis/UdfCellConversion38.vb'],
                        original_queries=0, original_compilations=0,
                        original_cases=28966, late_separate_cases=458))
for name in ['build.py', 'focused.py', 'inspect_stage.py', 'MethodMetadata-v6.cs',
             'native-metadata-owners-v6.txt', 'run_metadata_v6.py', 'compatibility.py',
             'selected_release.py', 'run_full_validation.py', 'report_managed_v2.py', 'report_validation.py']:
    text = (PREVIOUS / name).read_text(encoding='utf-8').replace('w2c4-validation-v1', 'w2c5-validation-v1')
    if name == 'focused.py':
        text = text.replace('FullyQualifiedName~UdfPreparation38Tests', 'FullyQualifiedName~UdfPreparation38Tests')
    with (HERE / name).open('x', encoding='utf-8', newline='\n') as stream:
        stream.write(text)
required = ['owner-review.md', 'NEXT_WAVE_HANDOVER.md', 'source-transitions.json', 'validation.json',
            'nonpasses.json', 'publication-manifest-v3.json', 'historical-ownership.json',
            'current-ownership.json', 'original-fragment-parent-crosswalk.json',
            'production-consumer-edges.json', 'binary-source-lineage.json',
            'independent-slice-proof-final.json', 'independent-ownership-proof-final.json']
save('authority-inputs.json', dict(previous=[pin(PREVIOUS / name) for name in required],
                                 instructions=pin(ROOT / 'AGENTS.md'),
                                 master_plan=pin(ROOT / 'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),
                                 scope='W2 only; W3 conditional on genuine zero OPEN; no new original execution'))
nonpasses = json.loads((PREVIOUS / 'nonpasses.json').read_bytes())
print('Frozen nonpass sections:', list(nonpasses))
print('Exact preimages retained:', len(rows))
