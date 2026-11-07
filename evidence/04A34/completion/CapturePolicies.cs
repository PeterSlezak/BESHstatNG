using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class CompletionPolicies34Capture {
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root) {
  Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/completion-legacy-policies.dll"));
  var module=fixture.GetType("Batch34PolicyFixtures.LegacyPolicies34",true);
  var create=fixture.GetType("Batch34PolicyFixtures.CompletionPolicyCases34",true).GetMethod("Arguments");
  var snapshot=fixture.GetType("Batch34CompletionFixtures.Completion34Cases",true).GetMethod("Snapshot");
  var rows=new List<string>{"culture,method,case,output,exception"};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   foreach(var method in module.GetMethods(BindingFlags.Public|BindingFlags.Static).OrderBy(x=>x.Name,StringComparer.Ordinal))for(int i=0;i<80;i++) {
    var args=(object[])create.Invoke(null,new object[]{method,i});string error="None";object result=null;
    try{result=method.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
    rows.Add(String.Join(",",new object[]{culture,method.Name,i,snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
   }
  }
  var path=Path.Combine(root,"evidence/04A34/completion/pre-edit-policies.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen corpus already exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
