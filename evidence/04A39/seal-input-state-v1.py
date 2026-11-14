"""Final pre-Python source/fixture state and report-only attempt inventory."""
from common39 import *
import ast
import re

source_lock()
for path in HERE.rglob('*.py'):ast.parse(path.read_text(encoding='utf-8-sig'),filename=str(path))
old=HERE/'expand-source-links-v5.py';new=HERE/'expand-source-links-v6.py'
equivalence=[]
for word,text in [('Alpha','Alpha Alpha2'),('Alpha','preAlpha'),('A_b','A_b-A_b2'),('SelectNested','SelectNested(x)'),('Group9','Group9.Member'),('Alpha','αAlphaβ')]:
    a=bool(re.search(r'(?<![\w])'+re.escape(word)+r'(?![\w])',text));b=word in set(re.findall(r'\w+',text))
    assert a==b;equivalence.append(dict(word=word,text=text,both=a))
save('execution/focused-lookup-stop-v1.json',dict(status='STOPPED_FOCUSED_DIAGNOSTIC_ONLY',
    stopped=pin(old),completed=pin(new),
    reason='Repeated per-declaration regex compilation was slow. Owned helper interrupted through its execution session before any register-v4 payload was written; token-set membership is the same word-boundary predicate and completed in2.55s.',
    controls=equivalence,test_campaign_identities_started=0,production_changes=0,
    raw_stdout=pin(PRIVATE/'commands/a39v1-packet-source-links-v5/stdout.log'),
    raw_stderr=pin(PRIVATE/'commands/a39v1-packet-source-links-v5/stderr.log'),
    diagnostic_limit='Attempted read-only Win32_Process command-line query was sandbox-denied; no Stop-Process action executed. Session interrupt stopped the owned helper.'))
sources=runner().__class__.build.__globals__['source_pins']()
save('matrix/final-source-state-v1.json',dict(utc=utc(),source_pins=sources,
    builds=[pin(HERE/'build-and-tests/Debug-build-v4.json'),pin(HERE/'build-and-tests/Release-build-v4.json')],
    matrix=pin(HERE/'matrix/verification-matrix-v1.json'),
    build_evolution='v1 initial test compile error;v2 successful nine-step Debug graph with report-only name collision;v4 copied identical production/portable outputs and recompiled only one changed new Legacy test. Single complete Debug and targeted Release final campaigns. No product edit or scientific correction.',
    unchanged_production=True,final_added_managed_occurrences=33,
    privacy_waivers_changed=False,methods_Ribbon_Help_capabilities_changed=False,
    full_campaigns=dict(Debug_managed=1,Release_managed_full=0,Release_targeted=1,Python_scheduled=1,Node=1)))
print('Source/input state ready for single final Python campaign',flush=True)
