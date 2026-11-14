"""Inspect original/current IL in separate processes; no original executions."""
from capture_current import *
def main():
 runner=m.Runner(RUNROOT);folder=RUNROOT/'completion-w2/method-reader-v2';folder.mkdir(exist_ok=False);exe=folder/'MethodBodies38.exe'
 r=runner.command('w2-method-reader-v2-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'MethodBodies-v2.cs')]);assert r['exit_code']==0
 records=[clean(r)]
 for config in ['Debug','Release']:
  for role,build in [('original',RUNROOT/'consolidated38-v7/original-build'/config),('current',RUNROOT/'completion-w2/profiles-v3'/config/'lib')]:
   r=runner.command('w2-method-reader-v2-'+role+'-'+config,[str(exe),str(build),str(HERE/('method-bodies-v2-'+role+'-'+config+'.json'))]);records.append(clean(r));assert r['exit_code']==0
 with (HERE/'method-reader-v2-receipts.json').open('x',encoding='utf-8') as f:json.dump(dict(commands=records,tool_source=pin(HERE/'MethodBodies-v2.cs'),original_model_or_query_executions=0,scope='ReadMethodBody metadata only; no constructor/method invocation'),f,indent=2)
if __name__=='__main__':main()
