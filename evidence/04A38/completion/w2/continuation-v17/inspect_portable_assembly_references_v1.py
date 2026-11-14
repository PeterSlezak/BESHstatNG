"""Passive current-only assembly references and original/current conditional boxing."""
from proof_support_v17_v4 import *
tool=PRIVATE/'assembly-reference-inspection';tool.mkdir(exist_ok=False)
source=tool/'References17.cs'
source.write_text('''using System;using System.IO;using System.Linq;using System.Reflection;using System.Web.Script.Serialization;
class References17 { static void Main(string[] args) { var rows=new[]{"BESHStat.Core.dll","BESHStat.Excel.Common.dll"}.Select(n=>new {name=n,references=Assembly.ReflectionOnlyLoadFrom(Path.Combine(args[0],n)).GetReferencedAssemblies().Select(a=>a.FullName).ToArray()}).ToArray();File.WriteAllText(args[1],new JavaScriptSerializer().Serialize(rows));}}
''',encoding='utf-8')
exe=tool/'References17.exe';lib=PRIVATE/'combined-v7/Debug/lib';output=HERE/'portable-assembly-references-v1.json';commands=[]
r=command('passive-reference-tool-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(source)]);commands.append(clean(r));assert r['exit_code']==0
r=command('passive-current-assembly-references',[str(exe),str(lib),str(output)]);commands.append(clean(r));assert r['exit_code']==0
for row in load(output):assert not any(any(s in a for s in ['ExcelDna','Microsoft.Office','System.Windows.Forms']) for a in row['references'])
meta=load(FINAL/'current-Debug.json');original=load(W2/'continuation-v14/metadata-all/packet-v1-context-v5/original-Debug.json')
old=method(original,'BESHStatNG.UdfDataImport','TryComputeSurvivalMedianCi')
now=[m for m in meta['methods'] if m['declaring_type']=='BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38' and m['member_name']=='ComputeSurvivalMedianCi' and m['signature']['generic_arguments'] and not m['signature']['generic_definition']]
assert len(now)==1;now=now[0]
boxes=lambda m:[dict(index=i,instruction=x) for i,x in enumerate(m['instructions']) if x['op']=='box' and 'ExcelDna.Integration.ExcelError' in str(x['operand'])]
assert len(boxes(old))==len(boxes(now))==3
assert now['signature']['generic_arguments'][0]['name'].startswith('ExcelDna.Integration.ExcelError @ ')
save('portable-reference-and-error-box-parity-v1.json',dict(commands=commands,tool_source=pin(source),current_assemblies=[library_pin(lib/n) for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']],reference_inspection=pin(output),original_native_method=old,current_constructed_portable_method=now,original_box_sites=boxes(old),current_box_sites=boxes(now),decision='Three original conditional enum-box sites retained in the constructed Common method. The enum type is supplied by the Windows caller as a generic value argument; neither portable assembly statically references Excel-DNA/Office/WinForms. Native consumer and portable custom-enum tests both PASS distinct-box checks.',final_focused=pin(HERE/'error-cell-focused-final-recheck-v1.json'),original_queries=0,Release=DEFERRED))
print('Passive portable assembly references PASS; exact three conditional native enum-box sites preserved')
