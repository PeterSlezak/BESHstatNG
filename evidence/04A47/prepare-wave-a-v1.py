from common47 import *
import re
lock()
s=(ROOT/'evidence/04A46/census-v1.py').read_text().replace('common46','common47').replace('04A46','04A47').replace('CURRENT_MAIN_PRE_EDIT_04A46','CURRENT_MAIN_PRE_EDIT_04A47')
s=re.sub(r"targets=\[.*?\]", "targets=['UiGLM','Ui13GEE','Ui18MMRM','Ui19LMM','Ui11PCA','Ui20PropensityScoreMatching','Ui21ControlCharts','Ui22MultivariateControlCharts','Ui4Cox','Ui4KMandLogRank']",s,count=1)
s=s.replace("ROOT/('.test-rationalization-04A45.tmp/builds/a45-final-v1/'","ROOT/('.test-rationalization-04A46.tmp/builds/a46-final-v1/'")
s=s.replace("outside46=", "outside47=").replace("outside46", "outside47")
with (HERE/'census-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
paths=['planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.22.md','docs/architecture/batch-04A-45-owner-review.md','docs/architecture/batch-04A-46-owner-review.md','evidence/04A45/native-risk/owner-decision-v1.md','evidence/04A46/native-risk/owner-decision-addendum-v1.md']
paths+= [p.relative_to(ROOT).as_posix() for p in (ROOT/'docs/architecture').glob('*.md') if re.search(r'04[Aa][-.](?:01|09|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|27|28|39|40|41|42|43|44)',p.name)]
texts={p:(ROOT/p).read_text(encoding='utf-8-sig') for p in paths}
assert 'Revision 2.23' in texts[paths[0]] and '| **04A.47** |' in texts[paths[1]]
save('planning/entry-authorities-v1.json',dict(files=[pin(p) for p in paths],charter=next(l for l in texts[paths[1]].splitlines() if l.startswith('| **04A.47** |')),living_revision='2.23',native='NOT_APPROVED',scientific='NOT_GOLDEN',gates=GATES))
