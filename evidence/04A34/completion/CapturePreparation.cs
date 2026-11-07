using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class CompletionPreparation34Capture {
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root) {
  string folder=Path.Combine(root,".test-rationalization-04A34.tmp/builds/final/Debug/legacy");
  foreach(string name in new[]{"BESHStat.Core.dll","BESHStat.Excel.Common.dll","BESHstatNG.dll"})Assembly.LoadFrom(Path.Combine(folder,name));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/completion-legacy-preparation.dll"));
  var helper=fixture.GetType("Batch34PreparationFixtures.CompletionPreparationCases34",true);
  var create=helper.GetMethod("Arguments");var snapshot=helper.GetMethod("Snapshot");
  var rows=new List<string>{"culture,family,method,case,output,exception"};
  foreach(string culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   foreach(string family in new[]{"Bullet","Sankey","Cdf","Dumbbell","Ladder","Hull","Polar","Spc"}) {
    var module=fixture.GetType("Batch34PreparationFixtures.Legacy"+family+"Preparation34",true);
    foreach(var method in module.GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m=>!m.Name.StartsWith("IsFinite")).OrderBy(x=>x.Name,StringComparer.Ordinal))for(int i=0;i<160;i++) {
     var args=(object[])create.Invoke(null,new object[]{method,i});string error="None";object result=null;
     try{result=method.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
     rows.Add(String.Join(",",new object[]{culture,family,method.Name,i,snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
    }
   }
  }
  string path=Path.Combine(root,"evidence/04A34/completion/pre-edit-preparation.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen corpus already exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
