from common import *
s=(HERE/'verify_slice_v4.py').read_text(encoding='utf-8')
needle="records[c]=(o,n)\n"
extra="  previousOriginal=load('evidence/04A38/completion/w2/continuation-v3/metadata-v5/names-v3/original-'+c+'.json');need(o['assemblies']==previousOriginal['assemblies'] and o['original_semantic_calls']==n['original_semantic_calls']==0,'Original/current assembly or provenance substituted')\n  built=load(base+'build-parametric-v4-'+c+'.json');need(built['all_exit_zero'],'Current production build failed')\n  for a in n['assemblies']:need(dict(name=a['file_name'],bytes=a['bytes'],sha256=a['sha256']) in built['assemblies'],'Loaded current binary not built source')\n"
assert needle in s;s=s.replace(needle,needle+extra)
needle="before=load(base+'inversion-before-'+c+'.json');after=load(base+'inversion-parametric-v4-'+c+'.json');"
assert needle in s;s=s.replace(needle,needle+"need(before['original_calls']==after['original_calls']==0 and before['classification']==after['classification']=='CURRENT_ONLY_RUNTIME_PROBE_NOT_ORIGINAL_REFERENCE','Unapproved original observation');")
with (HERE/'verify_slice_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
# Ownership reader uses the stronger additive checker without changing earlier results.
s=(HERE/'verify_ownership.py').read_text(encoding='utf-8').replace('verify_slice_v4','verify_slice_v5')
with (HERE/'verify_ownership_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
