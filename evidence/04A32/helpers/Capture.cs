// Host-free reflection against the actual prebuilt Framework adapter. No Excel
// objects are created. Source and assembly hashes are recorded by the driver.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
public static class Graphics32Capture {
 static BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
 static string Bits(object value) {
  if(value==null)return "Nothing";
  var a=value as Array;
  if(a!=null)return String.Join("|",a.Cast<object>().Select(x=>Bits(x)));
  if(value is bool)return (bool)value?"True":"False";
  return BitConverter.DoubleToInt64Bits(Convert.ToDouble(value,CultureInfo.InvariantCulture)).ToString("X16",CultureInfo.InvariantCulture);
 }
 static void Fields(List<string> rows,string family,string id,object obj,string[] names,string error) {
  foreach(string name in names) {
   var value=obj.GetType().GetField(name,Flags).GetValue(obj);var a=value as Array;
   var shape=a==null?"scalar":String.Join("x",Enumerable.Range(0,a.Rank).Select(i=>a.GetLength(i).ToString(CultureInfo.InvariantCulture)));
   rows.Add(String.Join(",",new[]{family,id,name,Bits(value),shape,error}));
  }
 }
 static string Call(object obj,string method,object[] args) {
  try {obj.GetType().GetMethod(method,Flags).Invoke(obj,args);return "None";}
  catch(TargetInvocationException ex){return ex.InnerException.GetType().FullName;}
 }
 public static void Run(string build,string output) {
  var old=CultureInfo.CurrentCulture;CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
  try {
   var assembly=Assembly.LoadFrom(Path.Combine(build,"BESHstatNG.dll"));
   var box=assembly.GetType("BESHStatNG.graphics.BoxPlot",true);var hist=assembly.GetType("BESHStatNG.graphics.Histogram",true);
   var rows=new List<string>{"family,id,field,ieee_or_value,shape,exception"};
   double[][] fixtures={new double[]{4,1,4,2,1},new double[]{-9,-5,-3,-1},new double[]{-5,-1,0,3,6},new double[]{0,0,0},new double[]{3},new double[]{2,1},new double[]{},new double[]{1,2,3,4,5,6,7,8,9,100},new double[]{Double.NaN,1,Double.PositiveInfinity},new double[]{-0.0,0,1,1},new double[]{-100,1,2,3,4,5,6,7,8,9,100},new double[]{1e-150,2e-150,3e-150},new double[]{1e150,2e150,3e150}};
   var boxFields=new[]{"pPlotBlank","pPlotMedian","pPlotQ3","pPlotMedianMinus","pPlotQ3Minus","pWhiskerQ1","pWhiskerQ3","pQ1","pQ3","pPlotMeans","pMins","pMaxs","pMedians","pArOutliersSmall","pArOutliersBig","pArNOutliersSmall","pArNOutliersBig"};
   for(int f=0;f<fixtures.Length;f++) {
    double[][] groups={ (double[])fixtures[f].Clone(),new double[]{8,1,3,3,4}};
    var obj=Activator.CreateInstance(box,Flags,null,new object[]{groups,new string[]{"same","same"},null,""},CultureInfo.InvariantCulture);
    for(int repeat=0;repeat<2;repeat++) {
     var error=Call(obj,"Calculate",new object[]{});
     if(error=="None")error=Call(obj,"CalcForPlotting",new object[]{});
     Fields(rows,"box",f+"-"+repeat,obj,boxFields,error);
     if(groups[0].Length>0)groups[0][0]=groups[0][0]+1;
    }
   }
   var hfields=new[]{"arBinMidVal","arFreq","arXi","arGauss","pQ1","pMedian","pQ3","pWhiskerLow","pWhiskerHigh","arOutliers","pHasBoxSummary"};
   foreach(var rule in new[]{"","(Sturges)","(Doane)","(Scott)","(Freedman-Diaconis)"}) {
    for(int f=0;f<fixtures.Length;f++) {
     var obj=Activator.CreateInstance(hist,Flags,null,new object[]{fixtures[f].Clone()},CultureInfo.InvariantCulture);
     foreach(bool overlay in new[]{false,true})foreach(bool tukey in new[]{false,true}) {
      string id=rule+"-"+f+"-"+overlay+"-"+tukey;
      string error=Call(obj,"compute",new object[]{overlay,rule,tukey});
      Fields(rows,"histogram",id,obj,hfields,error);
     }
    }
   }
   File.WriteAllLines(output,rows,new System.Text.UTF8Encoding(false));
  } finally {CultureInfo.CurrentCulture=old;}
 }
}
