using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class PostCompletionPreparation34Capture {

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

  string folder=Path.Combine(root,".test-rationalization-04A34.tmp/builds/review3/"+config+"/legacy");
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
     var names=new Dictionary<string,string>{{"Bullet","BulletChartExcel"},{"Sankey","SankeyPlotExcel"},{"Cdf","CumulativeDistributionPlotExcel"},{"Dumbbell","DumbbellPlotExcel"},{"Ladder","LadderPlotExcel"},{"Hull","ConvexHullPlotExcel"},{"Polar","PolarPlotExcel"},{"Spc","graphics.SpcControlChartExcel"}};
     var owner=finalHost.GetType("BESHStatNG."+((family=="Spc"&&(method.Name=="NormalizeText"||method.Name=="NormalizeNumberFormat"))?"graphics.SpcControlChartAppearanceOptions":names[family]),true);
     var target=owner.GetMethod(method.Name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
     try{result=target.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
     rows.Add(String.Join(",",new object[]{culture,family,method.Name,i,snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
    }
   }
  }
  string path=Path.Combine(root,".test-rationalization-04A34.tmp/completion-review3-post-"+config+"-preparation.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen corpus already exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
