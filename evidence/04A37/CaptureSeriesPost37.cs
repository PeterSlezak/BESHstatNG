using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class CaptureSeriesPost37 {
 static object P(object v,string n){return v.GetType().GetProperty(n).GetValue(v,null);}
 static object M(object v,string n,params object[] a){return v.GetType().GetMethod(n).Invoke(v,a);}
 static object[] Shape(object s,int slot){return new object[]{P(s,"Name"),P(s,"XValues"),P(s,"YValues"),slot};}
 public static void Run(string root,string label,string config) {
  string folder=Path.Combine(root,".test-rationalization-04A37.tmp/builds/"+label+"/"+config+"/legacy");
  var core=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));
  var common=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Excel.Common.dll"));
  var a=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A37.tmp/Series37.dll"));
  var series=a.GetType("Frozen37.OriginalSeries37");var cases=a.GetType("Frozen37.Cases37");
  var gt=core.GetType("BESHStatNG.DendrogramChartGeometry37",true);var pt=common.GetType("BESHStatNG.ExcelCommon.DendrogramChartPresentation37",true);
  var et=pt.GetConstructors()[0].GetParameters()[1].ParameterType;
  Func<object,string> snapshot=v=>(string)cases.GetMethod("Snapshot").Invoke(null,new object[]{v});
  var rows=new List<string>{"culture\tsource\tcase\tlabelMode\tgeometryAndIdentity\tseries\tbounds\ttitles\terror"};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int source=0;source<2;source++)for(int id=0;id<128;id++)for(int mode=0;mode<3;mode++) {
    object layout=(source==0?cases.GetMethod("Layout"):series.GetMethod("FittedLayout")).Invoke(null,new object[]{id});var lt=layout.GetType();
    var geometry=new[]{"PolylineX","PolylineY","ClusterPolylineX","ClusterPolylineY","LeafX","LeafY","LeafLabels","LeafOriginalRowOrder","Orientation","HeightMode","MaximumHeight","LeafCount","CutLineX","CutLineY"}.Select(n=> {object v=lt.GetProperty(n).GetValue(layout,null);if(v is System.Collections.IList && !(v is Array))return ((System.Collections.IList)v).Cast<object>().ToArray();return v;}).ToArray();
    object result=null,bounds=null,titles=null;string error="None";
    try {
     var g=Activator.CreateInstance(gt,new[]{layout});var p=Activator.CreateInstance(pt,new[]{g,Enum.ToObject(et,mode)});var output=new List<object>();
     output.Add(Shape(M(p,"BaseSeries"),-1));
     foreach(var item in (System.Collections.IEnumerable)M(p,"Clusters"))output.Add(Shape(item,((int[])P(item,"SourceOrdinals"))[0]));
     var cut=M(p,"CutSeries");if(cut!=null)output.Add(Shape(cut,-1));
     var leaf=M(p,"LeafSeries");if(leaf!=null)output.Add(new object[]{P(leaf,"Name"),P(leaf,"XValues"),P(leaf,"YValues"),P(leaf,"Labels"),P(leaf,"SourceOrdinals"),(int)P(g,"Orientation")});
     result=output.ToArray();var b=M(g,"Bounds");bounds=new object[]{P(b,"XMin"),P(b,"XMax"),P(b,"YMin"),P(b,"YMax")};
     titles=new object[]{M(p,"DistanceTitle",new object[]{null}),M(g,"SuggestedAxisTitle")};
    }
    catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;error=e.GetType().FullName+":"+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(e.Message));}
    rows.Add(String.Join("\t",new[]{culture,source.ToString(),id.ToString(),mode.ToString(),snapshot(geometry),snapshot(result),snapshot(bounds),snapshot(titles),error}));
   }
  }
  string dest=Path.Combine(root,"evidence/04A37/postedit/series-"+config+".tsv");if(File.Exists(dest))throw new InvalidOperationException("Post series capture exists");
  File.WriteAllLines(dest,rows,new System.Text.UTF8Encoding(false));Console.WriteLine("Captured "+(rows.Count-1)+" post-edit dendrogram series/layout rows; "+config+"; NOT_GOLDEN");
 }
}
