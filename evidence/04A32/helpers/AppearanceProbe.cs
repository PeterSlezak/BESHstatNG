// Additional host-free compatibility probe. Calls only managed validation/axis
// resolution against before/final Framework builds;never creates Excel objects.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
public static class Graphics32AppearanceProbe {
 static BindingFlags StaticFlags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 static string Bits(object x){if(x==null)return "Nothing";return BitConverter.DoubleToInt64Bits(Convert.ToDouble(x,CultureInfo.InvariantCulture)).ToString("X16",CultureInfo.InvariantCulture);}
 public static void Run(string build,string output) {
  var assembly=Assembly.LoadFrom(Path.Combine(build,"BESHstatNG.dll"));
  var facade=assembly.GetType("BESHStatNG.ViolinPlot",true);
  var result=facade.GetMethod("Compute").Invoke(null,new object[]{new double[]{1,2,3,4},new string[]{"a","a","b","b"},null});
  var rows=new List<string>{"family,property,input,exception,param_name,axis_ieee,major_ieee"};
  foreach(string family in new[]{"box","violin"}) {
   var type=assembly.GetType(family=="box"?"BESHStatNG.graphics.BoxPlotAppearance":"BESHStatNG.ViolinPlotAppearance",true);
   var owner=assembly.GetType(family=="box"?"BESHStatNG.graphics.BoxPlot":"BESHStatNG.ViolinPlotExcel",true);
   var properties=family=="box"?new[]{"FillTransparency","OutlineWeight","MeanLineWeight","ObservationMarkerSize","ObservationJitterHalfWidth"}:new[]{"FillTransparency","OutlineWeight","ObservationMarkerSize","InnerLineWeight","InnerBoxHalfWidth","WhiskerCapHalfWidth","YAxisMinimum","YAxisMaximum","YAxisMajorUnit"};
   var tests=new List<object[]> {new object[]{"default",null},new object[]{"SeriesColors",null},new object[]{"SeriesColors",new int[0]}};
   foreach(string property in properties) {
    var propertyType=type.GetProperty(property).PropertyType;
    var inputs=propertyType==typeof(int)?new[]{-1.0,0.0,1.0,4.0,72.0,73.0}:new[]{Double.NaN,Double.PositiveInfinity,-1.0,0.0,0.12,1.0};
    foreach(double value in inputs)tests.Add(new object[]{property,value});
   }
   foreach(var test in tests) {
    var appearance=Activator.CreateInstance(type);string property=(string)test[0];object value=test[1];
    if(property!="default") {
     var p=type.GetProperty(property);var target=Nullable.GetUnderlyingType(p.PropertyType)??p.PropertyType;
     p.SetValue(appearance,value==null||value is Array?value:Convert.ChangeType(value,target,CultureInfo.InvariantCulture),null);
    }
    string error="None",parameter="",axis="Nothing",major="Nothing";
    try {
     owner.GetMethod("ValidateAppearance",StaticFlags).Invoke(null,family=="box"?new object[]{appearance}:new object[]{appearance,result});
     if(family=="violin") {
      var context=owner.GetMethod("ResolveAxisContext",StaticFlags).Invoke(null,new object[]{result,appearance});
      axis=String.Join("|",new[]{"XMinimum","XMaximum","YMinimum","YMaximum"}.Select(name=>Bits(context.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(context))));
      major=Bits(type.GetProperty("YAxisMajorUnit").GetValue(appearance,null));
     }
    } catch(TargetInvocationException ex) {error=ex.InnerException.GetType().FullName;var a=ex.InnerException as ArgumentException;if(a!=null)parameter=a.ParamName??"";}
    string input=property=="default"?"Default":value==null?"Nothing":value is Array?"EmptyArray":Bits(value);
    rows.Add(String.Join(",",new[]{family,property,input,error,parameter,axis,major}));
   }
  }
  File.WriteAllLines(output,rows,new System.Text.UTF8Encoding(false));
 }
}
