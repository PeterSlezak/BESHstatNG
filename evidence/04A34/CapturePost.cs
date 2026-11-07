// Host-free execution against compiled pre-edit numerical slices and real adapters.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
public static class Specialty34PostCapture {
 static BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
 static string Bits(object v) {
  if(v==null)return "Nothing";
  var a=v as Array;
  if(a!=null)return "["+String.Join("|",a.Cast<object>().Select(Bits))+"]";
  if(v is bool)return v.ToString();
  return BitConverter.DoubleToInt64Bits(Convert.ToDouble(v,CultureInfo.InvariantCulture)).ToString("X16");
 }
 static void Record(List<string> rows,Type t,string method,string id,object[] args,bool byref=false) {
  string result,exception="None";
  var input=Bits(args);
  try {var v=t.GetMethod(method,F).Invoke(null,args);result=byref?Bits(args):Bits(v);}
  catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;exception=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);result="EXCEPTION";}
  rows.Add(String.Join(",",CultureInfo.CurrentCulture.Name,t.FullName,method,id,input,result,exception));
 }
 public static void Run(string root) {
  string build=Path.Combine(root,".test-rationalization-04A34.tmp/builds/final/Debug/legacy");
  Assembly.LoadFrom(Path.Combine(build,"BESHStat.Core.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A34.tmp/LegacySpecialty34.dll")).GetType("BESHStatNG.TestFixtures34.LegacySpecialty34",true);
  var host=Assembly.LoadFrom(Path.Combine(build,"BESHstatNG.dll"));
  var ladder=host.GetType("BESHStatNG.LadderPlotExcel",true);
  var ecdf=host.GetType("BESHStatNG.CumulativeDistributionPlotExcel",true);
  var gif=host.GetType("BESHStatNG.GifAnimator",true);
  var rows=new List<string>{"culture,type,member,case,input_bits,result_bits,exception"};
  double[][] vectors={null,new double[]{},new double[]{1},new double[]{-4,-1,2,9},new double[]{2,2,2,2},new double[]{-0.0,0,1,1,2},new double[]{Double.NaN,1,Double.PositiveInfinity},new double[]{Double.NegativeInfinity,Double.PositiveInfinity},new double[]{1e150,2e150},new double[]{1e-150,2e-150}};
  foreach(var culture in new[]{"", "sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int i=0;i<vectors.Length;i++) {
    foreach(double gap in new[]{-1.0,0.0,0.5,20.0,Double.NaN,Double.PositiveInfinity}) {
     Record(rows,fixture,"SpreadLabelPositions",i+"-"+Bits(gap),new object[]{vectors[i],-2.0,4.0,gap});
     Record(rows,ladder,"SpreadLabelPositions",i+"-"+Bits(gap),new object[]{vectors[i],-2.0,4.0,gap});
    }
    Record(rows,fixture,"Symmetry",i.ToString(),new object[]{vectors[i]==null?null:vectors[i].Clone()});
    for(int j=0;j<4;j++) {
     var y=vectors[i]==null?null:vectors[i].Reverse().ToArray();
     if(j==3)y=new double[]{2,3};
     object[] a={vectors[i],y,j==0?-10.0:0.0,j==1?10.0:2.0,null,null};
     Record(rows,fixture,"ExtendStepToAxisBounds",i+"-"+j,a,true);
     a=new object[]{vectors[i],y,j==0?-10.0:0.0,j==1?10.0:2.0,null,null};
     Record(rows,ecdf,"ExtendStepToAxisBounds",i+"-"+j,a,true);
    }
   }
   foreach(double v in new[]{-2.0,-0.0,0.0,0.5,1.0,3.0,Double.NaN,Double.PositiveInfinity,Double.NegativeInfinity}) {
    Record(rows,fixture,"BulletX",Bits(v),new object[]{v,15.0,200.0});
    Record(rows,fixture,"BulletClip",Bits(v),new object[]{v});
    Record(rows,fixture,"SankeyX",Bits(v),new object[]{v,10.0,400.0});
    Record(rows,fixture,"SankeyY",Bits(v),new object[]{v,20.0,300.0});
   }
   foreach(int n in new[]{-2,-1,0,1,2,5})Record(rows,fixture,"StageCentres",n.ToString(CultureInfo.InvariantCulture),new object[]{n,10.0,400.0});
   foreach(int ms in new[]{Int32.MinValue,-1,0,1,5,10,15,25,100,Int32.MaxValue}) {
    Record(rows,fixture,"MsToCentiseconds",ms.ToString(CultureInfo.InvariantCulture),new object[]{ms});
    Record(rows,gif,"MsToCentiseconds",ms.ToString(CultureInfo.InvariantCulture),new object[]{ms});
   }
  }
  string output=Path.Combine(root,"evidence/04A34/post-edit-compatibility-capture.csv");
  if(File.Exists(output))throw new InvalidOperationException("Frozen capture exists");
  File.WriteAllLines(output,rows,new System.Text.UTF8Encoding(false));
 }
}
