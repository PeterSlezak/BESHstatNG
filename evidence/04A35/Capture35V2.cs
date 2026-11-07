using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Collections;
public static class Capture35V2 {
 static object P(object v,string name){return v.GetType().GetProperty(name).GetValue(v,null);}
 static object Invoke(MethodInfo m,object[] args){try{return m.Invoke(null,args);}catch(TargetInvocationException e){throw e.InnerException;}}
 static object Series(object value){return ((IEnumerable)value).Cast<object>().Select(v=>(object)new object[]{P(v,"Name"),P(v,"XValues"),P(v,"YValues")}).ToArray();}
 static object Portable(Assembly core,Assembly common,Assembly host,string name,object[] args){
  var policy=common.GetType("BESHStatNG.ExcelCommon.ComparisonChartPolicy35",true);
  if(name=="BuildTitle"||name=="ValidatePlotData")return Invoke(host.GetType("BESHStatNG.graphics.MultipleComparisonPlotExcel",true).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Static),args);
  if(name=="Comparison"){
   Invoke(policy.GetMethod("ValidatePlotData"),args);var data=args[0];var geo=Activator.CreateInstance(core.GetType("BESHStatNG.MultipleComparisonGeometry35",true),new object[]{P(data,"Estimates"),P(data,"LowerLimits"),P(data,"UpperLimits")});
   var height=common.GetType("BESHStatNG.ExcelCommon.Chart34.ComparisonSeries.ComparisonSeriesPreparation34",true).GetMethod("SuggestedHeight");
   return new object[]{P(geo,"YValues"),P(geo,"PlusError"),P(geo,"MinusError"),P(geo,"MinimumX"),P(geo,"MaximumX"),Invoke(policy.GetMethod("BuildTitle"),args),Invoke(height,args)};
  }
  if(name=="LabelMap"){
   var labels=(string[])args[0];var ordinals=(int[])Invoke(policy.GetMethod("LabelOrdinals"),new object[]{args[1],labels.Length});
   return ordinals.Select(i=>{var row=Invoke(policy.GetMethod("LabelAt"),new object[]{labels,i});return (object)new object[]{P(row,"OrderedOrdinal"),P(row,"ExcelPointIndex"),P(row,"Text")};}).ToArray();
  }
  if(name=="Love"){
   var projection=Activator.CreateInstance(common.GetType("BESHStatNG.ExcelCommon.PsmLovePlotProjection",true),args);
   var spec=Activator.CreateInstance(common.GetType("BESHStatNG.ExcelCommon.PsmLoveChartSpec35",true),new object[]{projection});
   var values=new List<object>();var include=spec.GetType().GetMethod("IncludesChannel");var fields=new[]{"Before","AfterMatching","AfterWeighting"};
   for(int i=0;i<3;i++)if((bool)include.Invoke(spec,new object[]{i}))values.Add(P(spec,fields[i]));
   return new object[]{P(spec,"Names"),values.ToArray(),P(spec,"YValues"),P(spec,"Labels"),P(spec,"LabelSeriesOrdinal"),P(spec,"Threshold"),P(spec,"MaximumX"),projection.GetType().GetMethod("BuildDataTable").Invoke(projection,null)};
  }
  return Series(Invoke(common.GetType("BESHStatNG.ExcelCommon.AgreementPlotPolicy35",true).GetMethod(name),args));
 }
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root,string label,string config,string output,bool post){
  string folder=Path.Combine(root,".test-rationalization-04A35.tmp/builds/"+label+"/"+config+"/legacy");
  var core=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));var common=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Excel.Common.dll"));var host=Assembly.LoadFrom(Path.Combine(folder,"BESHstatNG.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A35.tmp/Legacy35V2.dll"));
  var original=fixture.GetType("Legacy35.Original35",true);var cases=fixture.GetType("Legacy35.Cases35",true);var snapshot=cases.GetMethod("Snapshot");
  var rows=new List<string>{"culture,member,case,input,output,exception"};
  Action<string,int,object[]> record=(name,index,args)=>{
   var method=original.GetMethod(name);object result=null;string error="None";
   try{result=post?Portable(core,common,host,name,args):method.Invoke(null,args);}catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;var a=e as ArgumentException;error=e.GetType().FullName+":"+(a==null?"":a.ParamName);}
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
