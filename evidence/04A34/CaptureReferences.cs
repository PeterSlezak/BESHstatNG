using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
public static class References34Capture {
 static string Bits(object v) {
  if(v==null)return "Nothing";
  if(v is bool)return v.ToString();
  var a=v as Array;if(a!=null)return "["+String.Join("|",a.Cast<object>().Select(Bits))+"]";
  if(v.GetType().Name=="ChartAxisScaleResult")return String.Join("|",new[]{"Min","Max","Scale"}.Select(n=>n+"="+Bits(v.GetType().GetProperty(n).GetValue(v,null))));
  return BitConverter.DoubleToInt64Bits(Convert.ToDouble(v,CultureInfo.InvariantCulture)).ToString("X16");
 }
 static void Record(List<string> rows,Type t,string method,string id,params object[] args) {
  string result,error="None";
  try{result=Bits(t.GetMethod(method).Invoke(null,args));}
  catch(TargetInvocationException e){result="EXCEPTION";var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
  rows.Add(String.Join(",",CultureInfo.CurrentCulture.Name,method,id,result,error));
 }
 public static void Run(string root) {
  var build=Path.Combine(root,".test-rationalization-04A34.tmp/builds/before/Debug/legacy");
  var core=Assembly.LoadFrom(Path.Combine(build,"BESHStat.Core.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/LegacyReference34.dll")).GetType("BESHStatNG.TestFixtures34.LegacyReference34",true);
  var specialty=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/LegacySpecialty34.dll")).GetType("BESHStatNG.TestFixtures34.LegacySpecialty34",true);
  var rows=new List<string>{"culture,member,case,result_bits,exception"};
  var panel=core.GetType("BESHStatNG.StatisticalProcessControl.SpcPanelType",true);
  double[][] vectors={null,new double[]{},new double[]{-0.0},new double[]{-4,2,7},new double[]{Double.NaN,Double.PositiveInfinity}};
  foreach(var culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int i=0;i<vectors.Length;i++) {
    Record(rows,fixture,"HasAnyFinite",i.ToString(),new object[]{vectors[i]});
    for(int j=0;j<vectors.Length;j++)Record(rows,fixture,"ComparisonBasis",i+"-"+j,new object[]{vectors[i],vectors[j],vectors[j]});
   }
   foreach(string p in Enum.GetNames(panel))foreach(double lo in new[]{Double.PositiveInfinity,-1.0,0.0,0.95,2.0,Double.NaN})foreach(double hi in new[]{Double.NegativeInfinity,0.0,1.0,2.0,Double.NaN})Record(rows,fixture,"SpcBounds",p+"-"+Bits(lo)+"-"+Bits(hi),lo,hi,Enum.Parse(panel,p));
   foreach(double v in new[]{-0.0,0.0,1.0,Double.NaN,Double.PositiveInfinity})Record(rows,fixture,"FitLine",Bits(v),v,2.0,3.0,-2.0);
   var bin=core.GetType("BESHStatNG.regression.CalibrationBinSummary",true);
   var list=(IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(bin));
   foreach(double v in new[]{-0.0,0.0,0.5,Double.NaN,Double.PositiveInfinity,Double.NegativeInfinity}) {
    var row=Activator.CreateInstance(bin);bin.GetField("MeanPredicted").SetValue(row,v);bin.GetField("ObservedRate").SetValue(row,v);list.Add(row);
   }
   Record(rows,specialty,"Calibration","ordered-nan-infinities",new object[]{list});
  }
  var path=Path.Combine(root,"evidence/04A34/pre-edit-reference-capture.csv");
  if(File.Exists(path))throw new InvalidOperationException("Frozen capture exists");
  File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));
 }
}
