using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class PostCompletionSeries34Capture {

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

  var folder=Path.Combine(root,".test-rationalization-04A34.tmp/builds/review3/"+config+"/legacy");
  foreach(var name in new[]{"BESHStat.Core.dll","BESHStat.Excel.Common.dll","BESHstatNG.dll"})Assembly.LoadFrom(Path.Combine(folder,name));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/completion-legacy-series.dll"));
  var create=fixture.GetType("Batch34SeriesFixtures.CompletionSeriesCases34",true).GetMethod("Arguments");
  var snapshot=fixture.GetType("Batch34CompletionFixtures.Completion34Cases",true).GetMethod("Snapshot");
  var rows=new List<string>{"culture,family,method,case,output,exception"};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   foreach(var family in new[]{"Dumbbell","Ladder","Agreement","Comparison","Cdf"}) {
    var module=fixture.GetType("Batch34SeriesFixtures.Legacy"+family+"Series34",true);
    foreach(var method in module.GetMethods(BindingFlags.Public|BindingFlags.Static).OrderBy(x=>x.Name,StringComparer.Ordinal))for(int i=0;i<160;i++) {
     var args=(object[])create.Invoke(null,new object[]{method,i});object result=null;string error="None";
     var target=finalCommon.GetType("BESHStatNG.ExcelCommon.Chart34."+family+"Series."+family+"SeriesPreparation34",true).GetMethod(method.Name);
     try{result=target.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
     rows.Add(String.Join(",",new object[]{culture,family,method.Name,i,snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
    }
   }
  }
  var path=Path.Combine(root,".test-rationalization-04A34.tmp/completion-review3-post-"+config+"-series.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen corpus already exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
