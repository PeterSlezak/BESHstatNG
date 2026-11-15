from common43 import *
s=(ROOT/'evidence/04A42/compiled-and-fixtures-v3.py').read_text(encoding='utf-8-sig').replace('from common42','from common43').replace('corrected3-','final-').replace('a42-wave-b3-v1','a43-wave-b2-v1').replace('a42-final2-v2','a43-final-v1').replace('matrix/source-freeze-v2.json','matrix/source-freeze-v1.json').replace("PRIVATE/'compiled-v3'","PRIVATE/'compiled'").replace("PRIVATE/'abi-cil-v3'","PRIVATE/'abi-cil'")
s=s.replace("'evidence/04A41/abi-cil/'+config+'-proof-v1.json'","'evidence/04A42/abi-cil/'+config+'-proof-v3.json'").replace("'-proof-v3.json',dict","'-proof-v1.json',dict").replace('baseline41=','baseline42=')
a=s.index('owners=[');b=s.index('\nunexpected=',a);s=s[:a]+"owners=['WorksheetFunctions.MultivariateAnalysisUDFs','WorksheetFunctions.PropensityScoreUDFs','WorksheetFunctions.UdfCacheHelpers','ExcelCommon.UdfModelHandleDescriptor','ExcelCommon.UdfHandleLifecycleDecision','ExcelCommon.UdfModelHandlePolicy']"+s[b:]
s=s.replace("'fixture-final3-'","'fixture-final-'").replace("'compiled-method-differences3-'","'compiled-method-differences-'")
for family in ['39','40','41','42']:s=s.replace("+'-"+family+"-v3.json'","+'-"+family+"-v1.json'")
s=s.replace("compare42=module('compare42',HERE/'verify-wave-v1.py')","sys.path.insert(0,str(ROOT/'evidence/04A42'))\ncompare42=module('compare42',ROOT/'evidence/04A42/verify-wave-v1.py')")
s=s.replace("PRIVATE/'fixtures-before-v1/Observe42.exe'","ROOT/'.test-rationalization-04A42.tmp/fixtures-before-v1/Observe42.exe'").replace("PRIVATE/'fixtures-before-v1/observations.json'","ROOT/'.test-rationalization-04A42.tmp/fixtures-before-v1/observations.json'")
s+='''
compare43=module('compare43_relational',HERE/'compare43_v3.py')
a,b,current=execute_fixture('43',PRIVATE/'fixtures-before-v3/Observe43V3.exe',[str(HERE/'scope/cache-export-inventory-v1.json')],PRIVATE/'fixtures-before-v3/observations.json')
assert compare43.canonical(a)==compare43.canonical(b)
save('fixtures/'+config+'-43-v1.json',dict(kind='POST_EDIT_CURRENT43_RELATIONAL_COMPATIBILITY',operations=1118,graphs=36,families=18,baseline=pin(PRIVATE/'fixtures-before-v3/observations.json'),current=current,generated_reference_bijection_validated=True,raw_tokens_grammar_uniqueness_and_object_association_verified=True,nonvolatile_typed_outputs_errors_shapes_exact=True,clock_fields=pin(HERE/'fixtures/volatile-fields-v1.json'),normalization=pin(HERE/'fixtures/reference-normalization-v3.json'),embedded_raw_kind_is_driver_schema_not_capture_time=True,scientific='NOT_GOLDEN'))
a,b,current=execute_fixture('43-races',PRIVATE/'races-before-v1/Race43.exe',[str(HERE/'scope/cache-export-inventory-v1.json')],PRIVATE/'races-before-v1/observations.json')
left=compare43.validate_races(a);right=compare43.validate_races(b);assert left['identities']==right['identities']
save('fixtures/'+config+'-43-races-v1.json',dict(kind='POST_EDIT_CURRENT43_BOUNDED_HOST_CACHE_RACES;NOT_LIVE_EXCEL',baseline=pin(PRIVATE/'races-before-v1/observations.json'),current=current,before=left,after=right,identities_exact=True,allowed_outcome_invariants_pass=True,fixed_schedule_equality_claimed=False,live_Excel='NOT_RUN'))
print(config,'43 lifecycle reference graphs36/operations1118 and864 race identities PASS',flush=True)
'''
with (HERE/'compiled-and-fixtures-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(ROOT/'evidence/04A42/compatibility-v3.py').read_text(encoding='utf-8-sig').replace('from common42','from common43').replace('corrected3-','final-').replace("PRIVATE/'compiled-v3'","PRIVATE/'compiled'").replace('compatibility-current-v3','compatibility-current-v1').replace('matrix/source-freeze-v2.json','matrix/source-freeze-v1.json').replace("stage='a42-'","stage='a43-'")
for a,b in [('-driver-v3.py','-driver-v1.py'),('-driver-provenance-v3.json','-driver-provenance-v1.json'),("'case-identities-'+config+'-v3.json'","'case-identities-'+config+'-v1.json'"),("'compatibility/'+config+'-v3.json'","'compatibility/'+config+'-v1.json'"),('41 current-only29424','43 current-only29424')]:s=s.replace(a,b)
with (HERE/'compatibility-v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('Final scoped CIL/268 exports,cumulative39–43 fixtures and current-only historical corpus recipes prepared',flush=True)
