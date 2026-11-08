using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class Capture37 {
 static object Invoke(Type t,string n,params object[] a) { try{return t.GetMethod(n).Invoke(null,a);}catch(TargetInvocationException e){throw e.InnerException;} }
 public static void Run(string root,string config) {
  string folder=Path.Combine(root,".test-rationalization-04A37.tmp/builds/entry/"+config+"/legacy");
  Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A37.tmp/Original37.dll"));
  var old=fixture.GetType("Frozen37.Original37",true);var cases=fixture.GetType("Frozen37.Cases37",true);
  Func<object,string> snapshot=v=>(string)Invoke(cases,"Snapshot",new object[]{v});
  var output=new List<string>{"culture\toperation\tcase\tphase\tinput\toutput\terror"};
  var inputs=new List<string>{"culture\toperation\tcase\tinput"};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   Action<string,int,string,object[]> record=(name,id,phase,args)=> {
    object result=null;string error="None";
    try {result=Invoke(old,name,args);} catch(Exception e) {while(e is TargetInvocationException)e=e.InnerException;var a=e as ArgumentException;error=e.GetType().FullName+":"+(a==null?"":a.ParamName)+":"+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(e.Message));}
    // The deterministic factory source + case ID is canonical input authority for model objects.
    string input=String.Join("|",args.Select(a=>a==null||a is Array||a is string||a is ValueType?snapshot(a):a.GetType().FullName+":"+id));
    output.Add(String.Join("\t",new[]{culture,name,id.ToString(CultureInfo.InvariantCulture),phase,input,snapshot(result),error}));
    inputs.Add(String.Join("\t",new[]{culture,name,id.ToString(CultureInfo.InvariantCulture),input}));
   };
   for(int i=0;i<128;i++) {
    var p=Invoke(cases,"PcaModel",i);var f=Invoke(cases,"FactorModel",i);var ca=Invoke(cases,"CaModel",i);
    var labels=Invoke(cases,"Labels",i);var groups=Invoke(cases,"Groups",i);
    foreach(var n in new[]{"LoadingPlot2D","LoadingPlot3D","ScreePlot"})record("Pca"+n,i,"PRE_CHART",new[]{p});
    foreach(var n in new[]{"ScorePlot2D","ScorePlot3D"})record("Pca"+n,i,"PRE_CHART",new[]{p,labels,groups});
    record("PcaBiplot",i,"PRE_CHART",new[]{p,Invoke(cases,"Scale",i),labels,groups});
    foreach(var n in new[]{"LoadingPlot2D","LoadingPlot3D","ScreePlot"})record("Factor"+n,i,"PRE_CHART",new[]{f});
    foreach(var row in new[]{true,false}) {
     record("CaContribution",i*2+(row?0:1),"PRE_CHART",new[]{ca,(object)(i%6-1),row});
     record("CaPoints",i*2+(row?0:1),"AFTER_CHART_AND_SERIES",new[]{ca,(object)row});
     record("CaLabels",i*2+(row?0:1),"AFTER_CHART_AND_SERIES",new[]{ca,(object)row});
    }
    record("CaBounds",i,"PRE_CHART",new[]{ca});
    record("CaCaptions",i,"LATE_TRY_CATCH",new[]{ca});
    var layout=Invoke(cases,"Layout",i);
    record("DendrogramBounds",i,"AFTER_SERIES",new[]{layout});
    record("DendrogramTitles",i,"AFTER_AXES",new[]{layout,i%3==0?(object)" custom ":i%3==1?" ":null});
    var pct=new[]{(double)i,33.33333333333333,-0.0};
    record("Captions",i,"LATE_CAPTION",new object[]{pct,pct,new[]{"F1","F2","F3"}});
    int count=i%7; var xs=Enumerable.Range(0,count).Select(j=>(double)j).ToArray();var ys=xs.Select(x=>-x).ToArray();
    var pointLabels=Enumerable.Range(0,count).Select(j=>"L"+j).ToArray();
    var gs=Enumerable.Range(0,count).Select(j=>new[]{"B","a","A","(missing)","Ｂ","b",""}[j]).ToArray();
    record("Groups",i,"AFTER_CHART",new object[]{xs,ys,pointLabels,i%3==0?null:gs,"Scores"});
   }
  }
  string dest=Path.Combine(root,"evidence/04A37/preedit/capture-"+config+".tsv");
  if(File.Exists(dest))throw new InvalidOperationException("Frozen capture exists");
  File.WriteAllLines(dest,output,new System.Text.UTF8Encoding(false));
  string inputDest=Path.Combine(root,"evidence/04A37/preedit/inputs-"+config+".tsv");
  if(File.Exists(inputDest))throw new InvalidOperationException("Frozen inputs exist");
  File.WriteAllLines(inputDest,inputs,new System.Text.UTF8Encoding(false));
  Console.WriteLine("Captured "+(output.Count-1)+" original source-slice rows; "+config+"; NOT_GOLDEN; NO_NATIVE_EXECUTION");
 }
}
