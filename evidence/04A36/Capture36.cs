using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class Capture36 {
 static object P(object v,string n){return v.GetType().GetProperty(n).GetValue(v,null);}
 static object Invoke(MethodInfo m,object[] a){try{return m.Invoke(null,a);}catch(TargetInvocationException e){throw e.InnerException;}}
 static object Portable(Assembly core,Assembly common,string name,object[] a){
  if(name.StartsWith("KM")){
   int role=int.Parse(name.Substring(2));var t=core.GetType("BESHStatNG.KaplanMeierSeriesGeometry36",true);
   var v=Invoke(t.GetMethod(role==0?"Survival":role==1?"Censor":"Confidence"),role<2?a:new object[]{a[0],a[1],role==2});
   return new object[]{P(v,"XValues"),P(v,"YValues"),P(v,"AssignValues")};
  }
  if(name=="Cox"){
   var t=core.GetType("BESHStatNG.CoxCurveGeometry36",true);Invoke(t.GetMethod("Validate"),a);var x=(double[])a[0];if(x.Length==0)return null;
   var scale=Invoke(t.GetMethod("Axis"),new object[]{x});var c=common.GetType("BESHStatNG.ExcelCommon.SurvivalChartPresentation36",true);
   return new object[]{Invoke(t.GetMethod("Snapshot"),new object[]{a[0]}),Invoke(t.GetMethod("Snapshot"),new object[]{a[1]}),P(scale,"Min"),P(scale,"Max"),P(scale,"Scale"),0.0,1.0,0.2,c.GetField("BaselineName").GetValue(null),c.GetField("CoxTitle").GetValue(null),c.GetField("ProbabilityTitle").GetValue(null),c.GetField("CoxTimeTitle").GetValue(null)};
  }
  if(name=="Bands"){
   var v=Activator.CreateInstance(core.GetType("BESHStatNG.SpcZoneBands36",true),a);
   var bands=(double?[][])P(v,"Values");var r=new List<object>{P(v,"ValidPointCount")};
   foreach(var b in bands)r.Add(b.Select(x=>x.HasValue?(object)x.Value:null).ToArray());return r.ToArray();
  }
  throw new Exception(name);
 }
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root,string label,string config,string output,bool post){
  string folder=Path.Combine(root,".test-rationalization-04A36.tmp/builds/"+label+"/"+config+"/legacy");
  var core=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));var common=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Excel.Common.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A36.tmp/Legacy36.dll"));var original=fixture.GetType("Legacy36.Original36",true);var cases=fixture.GetType("Legacy36.Cases36",true);var snapshot=cases.GetMethod("Snapshot");
  var rows=new List<string>{"culture,member,case,input,output,exception"};
  Action<string,int,object[]> record=(name,index,a)=>{
   object result=null;string error="None";
   try{result=post?Portable(core,common,name,a):Invoke(original.GetMethod(name),a);}catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;var ex=e as ArgumentException;error=e.GetType().FullName+":"+(ex==null?"":ex.ParamName);}
   rows.Add(String.Join(",",new object[]{CultureInfo.CurrentCulture.Name,name,index,snapshot.Invoke(null,new object[]{a}),snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
  };
  foreach(var culture in new[]{"","sk-SK"}){
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int i=0;i<152;i++){
    var data=cases.GetMethod("KM").Invoke(null,new object[]{i});
    for(int g=0;g<4;g++)for(int role=0;role<4;role++)record("KM"+role,i*4+g,new object[]{data,g});
    record("Cox",i,new object[]{cases.GetMethod("Numbers").Invoke(null,new object[]{i}),cases.GetMethod("Numbers").Invoke(null,new object[]{i%3==0?i+1:i})});
    record("Bands",i,new object[]{cases.GetMethod("Points").Invoke(null,new object[]{i}),new double[]{-5,-0.0,0,Double.NaN,Double.NegativeInfinity}[i%5]});
   }
  }
  string path=Path.Combine(root,output);if(File.Exists(path))throw new InvalidOperationException("Capture already exists");File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
  Console.WriteLine("Captured "+(rows.Count-1)+" source-slice rows; "+config+"; NOT_GOLDEN");
 }
}
