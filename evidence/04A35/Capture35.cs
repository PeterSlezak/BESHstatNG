using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class Capture35 {
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root,string label,string config,string output,bool post){
  string folder=Path.Combine(root,".test-rationalization-04A35.tmp/builds/"+label+"/"+config+"/legacy");
  foreach(var name in new[]{"BESHStat.Core.dll","BESHStat.Excel.Common.dll","BESHstatNG.dll"})Assembly.LoadFrom(Path.Combine(folder,name));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A35.tmp/Legacy35.dll"));
  var original=fixture.GetType("Legacy35.Original35",true);var cases=fixture.GetType("Legacy35.Cases35",true);var snapshot=cases.GetMethod("Snapshot");
  var rows=new List<string>{"culture,member,case,input,output,exception"};
  Action<string,int,object[]> record=(name,index,args)=>{
   var method=original.GetMethod(name);object result=null;string error="None";
   try{result=method.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
   rows.Add(String.Join(",",new object[]{CultureInfo.CurrentCulture.Name,name,index,snapshot.Invoke(null,new object[]{args}),snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
  };
  foreach(var culture in new[]{"","sk-SK"}){
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int i=0;i<280;i++){
    var data=cases.GetMethod("Data").Invoke(null,new object[]{i});record("Comparison",i,new object[]{data});
    record("BuildTitle",i,new object[]{data});record("ValidatePlotData",i,new object[]{data});
    record("Love",i,new object[]{cases.GetMethod("Rows").Invoke(null,new object[]{i})});
    double[] nums=(double[])cases.GetMethod("Numbers").Invoke(null,new object[]{i});double a=nums[0],b=i%3==0?a:3.0;
    record("Regression",i,new object[]{a,b,i%2==0?-0.0:2.0,i%3==0?Double.NaN:-1.2});
    record("Identity",i,new object[]{a,b});
    record("Bland",i,new object[]{i%17==0?null:(i%13==0?new double[]{}:nums),a,b,-b,i%2==0,i%5==0?null:nums,i%7==0?null:new double[]{1.0}});
    record("LabelMap",i,new object[]{i%11==0?null:new string[]{null,"a","α","a"},i%9-2});
   }
  }
  string path=Path.Combine(root,output);if(File.Exists(path))throw new InvalidOperationException("Capture already exists");File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
  Console.WriteLine("Captured "+(rows.Count-1)+" original source rows; "+config+"; NOT_GOLDEN");
 }
}
