using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class PostCompletionExtras34Capture {

 static object InvokeTarget(MethodInfo method,object[] args,out string error){
  error="None";try{return method.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);return null;}
 }
 static string ProjectSnapshot(object value,Type template){
  if(value==null)return "Nothing";
  if(value is double)return BitConverter.DoubleToInt64Bits((double)value).ToString("X16",CultureInfo.InvariantCulture);
  if(value is bool||value is int||value is string)return Convert.ToString(value,CultureInfo.InvariantCulture);
  if(value is Array)return "["+String.Join("|",((Array)value).Cast<object>().Select(v=>ProjectSnapshot(v,v==null?typeof(object):v.GetType())))+"]";
  return String.Join("|",template.GetProperties().OrderBy(p=>p.Name,StringComparer.Ordinal).Select(p=>p.Name+"="+ProjectSnapshot(value.GetType().GetProperty(p.Name).GetValue(value,null),p.PropertyType)));
 }
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root,string config) {
  var finalFolder=Path.Combine(root,".test-rationalization-04A34.tmp/builds/review3/"+config+"/legacy");
  var finalCore=Assembly.LoadFrom(Path.Combine(finalFolder,"BESHStat.Core.dll"));
  var finalCommon=Assembly.LoadFrom(Path.Combine(finalFolder,"BESHStat.Excel.Common.dll"));
  var finalHost=Assembly.LoadFrom(Path.Combine(finalFolder,"BESHstatNG.dll"));

  Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/builds/review3/"+config+"/legacy/BESHStat.Core.dll"));
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
      var target=finalCore.GetType("BESHStatNG."+family+"MeasuredGeometryCore",true).GetMethod(family+method.Name); string error;var result=InvokeTarget(target,args,out error);
      rows.Add(String.Join(",",new object[]{culture,family,method.Name,i,input,ProjectSnapshot(result,method.ReturnType),error}.Select(Q)));
     }
    }
   }
  }
  string path=Path.Combine(root,".test-rationalization-04A34.tmp/completion-review3-post-"+config+"-extra-geometry.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen corpus already exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
