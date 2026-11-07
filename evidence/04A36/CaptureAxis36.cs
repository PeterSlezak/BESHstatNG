using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class CaptureAxis36 {
 static object P(object v,string n){return v.GetType().GetProperty(n).GetValue(v,null);}
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root,string label,string config,string output,bool post){
  string folder=Path.Combine(root,".test-rationalization-04A36.tmp/builds/"+label+"/"+config+"/legacy");
  Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));var common=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Excel.Common.dll"));Assembly.LoadFrom(Path.Combine(folder,"BESHStatNG.Test.dll"));
  var legacy=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A36.tmp/Legacy36.dll"));
  var presentation=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A36.tmp/LegacyPresentation36.dll"));var cases=presentation.GetType("Legacy36.OriginalPresentation36",true);var snapshot=legacy.GetType("Legacy36.Cases36",true).GetMethod("Snapshot");
  var original=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A36.tmp/LegacyAxis36.dll")).GetType("Legacy36.OriginalAxis36",true);
  var rows=new List<string>{"culture,case,output,exception"};
  foreach(var culture in new[]{"","sk-SK"}){CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int i=0;i<192;i++){
    object result=null;string error="None";
    try{
     var panel=cases.GetMethod("Panel").Invoke(null,new object[]{i});var spec=cases.GetMethod("Specifications").Invoke(null,new object[]{i});var a=new object[]{panel,spec,(i&4)!=0,(i&8)!=0};
     if(!post)result=original.GetMethod("Bounds").Invoke(null,a);
     else{var scale=common.GetType("BESHStatNG.ExcelCommon.SpcPanelPresentation36",true).GetMethod("AxisBounds").Invoke(null,a);result=new double[]{(double)P(scale,"Min"),(double)P(scale,"Max")};}
    }catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;var a=e as ArgumentException;error=e.GetType().FullName+":"+(a==null?"":a.ParamName);}
    rows.Add(String.Join(",",new object[]{culture,i,snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
   }
  }
  var path=Path.Combine(root,output);if(File.Exists(path))throw new Exception("Frozen output exists");File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));Console.WriteLine("Captured "+(rows.Count-1)+" accepted legacy axis rows; "+config+"; NOT_GOLDEN");
 }
}
