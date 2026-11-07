using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
public static class Layouts34Capture {
 static string Bits(object v) {
  if(v==null)return "Nothing";
  if(v is bool)return v.ToString();
  if(v is string)return (string)v;
  return BitConverter.DoubleToInt64Bits(Convert.ToDouble(v,CultureInfo.InvariantCulture)).ToString("X16");
 }
 static string Fields(object obj){return String.Join("|",obj.GetType().GetFields().OrderBy(f=>f.Name,StringComparer.Ordinal).Select(f=>f.Name+"="+Bits(f.GetValue(obj))));}
 public static void Run(string root) {
  var folder=Path.Combine(root,".test-rationalization-04A34.tmp/builds/before/Debug/legacy");
  var core=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));
  var host=Assembly.LoadFrom(Path.Combine(folder,"BESHstatNG.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/LegacyLayouts34.dll"));
  var bullet=core.GetType("BESHStatNG.BulletChartCore",true).GetMethod("ComputeBasic").Invoke(null,new object[]{new object[]{"A","B"},new double[]{40,70},new double[]{60,80},new double[,]{{50,80,100},{50,80,100}},null});
  var sankey=core.GetType("BESHStatNG.SankeyPlotCore",true).GetMethod("ComputeStaged").Invoke(null,new object[]{new object[,]{{"A","X","Z"},{"B","X","Y"}},new double[]{1,3},null,null});
  var rows=new List<string>{"culture,family,mask,width_bits,height_bits,options,result,exception"};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   foreach(var family in new[]{"Bullet","Sankey"}) {
    var appearanceType=host.GetType("BESHStatNG."+family+(family=="Bullet"?"ChartAppearance":"PlotAppearance"),true);
    var module=fixture.GetType("BESHStatNG.TestFixtures34.Legacy"+family+"Layout34",true);
    var method=module.GetMethod("BuildDrawingLayout");
    var result=family=="Bullet"?bullet:sankey;
    for(int mask=0;mask<32;mask++)foreach(double w in new[]{80.0,620.0})foreach(double h in new[]{40.0,420.0}) {
     var appearance=Activator.CreateInstance(appearanceType);
     string[] toggles=family=="Bullet"?new[]{"ShowMeasureLabels","ShowSubtitles","ShowScaleLabels","ShowScaleTickMarks","ShowTargetValueLabels"}:new[]{"ShowLegend","ShowNodeLabels","ShowStageLabels"};
     for(int t=0;t<toggles.Length;t++)appearanceType.GetProperty(toggles[t]).SetValue(appearance,(mask&(1<<t))!=0,null);
     appearanceType.GetProperty("ChartTitle").SetValue(appearance,(mask&16)!=0?"Title":"",null);
     object[] args;
     if(family=="Bullet")args=new object[]{w,h,result,appearance};
     else {
      var itemType=module.GetNestedType("LegendItem");
      var list=(IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
      for(int i=0;i<7;i++)list.Add(Activator.CreateInstance(itemType));
      args=new object[]{w,h,result,appearance,list};
     }
     string output,error="None";
     try{output=Fields(method.Invoke(null,args));}
     catch(TargetInvocationException e){output="EXCEPTION";error=e.InnerException.GetType().FullName;}
     rows.Add(String.Join(",",culture,family,mask,Bits(w),Bits(h),String.Join("|",toggles.Select(t=>t+"="+Bits(appearanceType.GetProperty(t).GetValue(appearance,null)))),output,error));
    }
   }
  }
  string path=Path.Combine(root,"evidence/04A34/pre-edit-layout-capture.csv");
  if(File.Exists(path))throw new InvalidOperationException("Capture exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
