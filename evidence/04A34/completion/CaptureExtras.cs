using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class CompletionExtras34Capture {
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root) {
  Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/completion-legacy-extras.dll"));
  var cases=fixture.GetType("Batch34CompletionFixtures.Completion34Cases",true);
  var create=cases.GetMethod("Arguments");var snapshot=cases.GetMethod("Snapshot");var invoke=cases.GetMethod("Invoke");
  var rows=new List<string>{"culture,family,method,case,input,output,exception"};
  foreach(string culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   foreach(string family in new[]{"Bullet","Sankey"}) {
    var module=fixture.GetType("Batch34CompletionFixtures.Legacy"+family+"Extras34",true);
    foreach(var method in module.GetMethods(BindingFlags.Public|BindingFlags.Static).OrderBy(x=>x.Name,StringComparer.Ordinal)) {
     for(int i=0;i<=method.GetParameters().Length*8;i++) {
      var args=(object[])create.Invoke(null,new object[]{method,i});
      string input=(string)snapshot.Invoke(null,new object[]{args});
      object[] values={method,args,null};var result=invoke.Invoke(null,values);
      rows.Add(String.Join(",",new object[]{culture,family,method.Name,i,input,snapshot.Invoke(null,new object[]{result}),values[2]}.Select(Q)));
     }
    }
   }
  }
  string path=Path.Combine(root,"evidence/04A34/completion/pre-edit-extra-geometry.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen corpus already exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
