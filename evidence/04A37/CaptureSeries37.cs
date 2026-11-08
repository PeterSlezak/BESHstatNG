using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class CaptureSeries37 {
 public static void Run(string root) {
  Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A37.tmp/builds/entry/Debug/legacy/BESHStat.Core.dll"));
  var a=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A37.tmp/Series37.dll"));
  var old=a.GetType("Frozen37.Original37");var series=a.GetType("Frozen37.OriginalSeries37");var cases=a.GetType("Frozen37.Cases37");
  Func<object,string> snapshot=v=>(string)cases.GetMethod("Snapshot").Invoke(null,new object[]{v});
  var rows=new List<string>{"culture\tsource\tcase\tlabelMode\tgeometryAndIdentity\tseries\tbounds\ttitles\terror"};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int source=0;source<2;source++)for(int id=0;id<128;id++)for(int mode=0;mode<3;mode++) {
    object layout=(source==0?cases.GetMethod("Layout"):series.GetMethod("FittedLayout")).Invoke(null,new object[]{id});var lt=layout.GetType();
    var geometry=new[]{"PolylineX","PolylineY","ClusterPolylineX","ClusterPolylineY","LeafX","LeafY","LeafLabels","LeafOriginalRowOrder","Orientation","HeightMode","MaximumHeight","LeafCount","CutLineX","CutLineY"}.Select(n=> {object v=lt.GetProperty(n).GetValue(layout,null);if(v is System.Collections.IList && !(v is Array))return ((System.Collections.IList)v).Cast<object>().ToArray();return v;}).ToArray();
    object result=null,bounds=null,titles=null;string error="None";
    try {var enumType=series.GetMethod("DendrogramSeries").GetParameters()[1].ParameterType;result=series.GetMethod("DendrogramSeries").Invoke(null,new[]{layout,Enum.ToObject(enumType,mode)});bounds=old.GetMethod("DendrogramBounds").Invoke(null,new[]{layout});titles=old.GetMethod("DendrogramTitles").Invoke(null,new[]{layout,(object)null});}
    catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;error=e.GetType().FullName+":"+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(e.Message));}
    rows.Add(String.Join("\t",new[]{culture,source.ToString(),id.ToString(),mode.ToString(),snapshot(geometry),snapshot(result),snapshot(bounds),snapshot(titles),error}));
   }
  }
  string dest=Path.Combine(root,"evidence/04A37/preedit/series.tsv");if(File.Exists(dest))throw new InvalidOperationException("Frozen series capture exists");
  File.WriteAllLines(dest,rows,new System.Text.UTF8Encoding(false));Console.WriteLine("Captured "+(rows.Count-1)+" original dendrogram series/layout rows; fitted and synthetic; NOT_GOLDEN");
 }
}
