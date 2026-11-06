// Reflection against actual compiled Windows NUMERIC members only. No Chart/COM creation.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
public static class Graphics33Capture {
 static BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
 static string Bits(object value) {
  if(value==null)return "Nothing";
  var a=value as Array;if(a!=null)return String.Join("|",a.Cast<object>().Select(Bits));
  if(value is string)return (string)value;
  return BitConverter.DoubleToInt64Bits(Convert.ToDouble(value,CultureInfo.InvariantCulture)).ToString("X16");
 }
 static string Call(object obj,string name,params object[] args) {
  try {obj.GetType().GetMethod(name,F).Invoke(obj,args);return "None";}
  catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;return e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);}
 }
 static void Fields(List<string> rows,string family,string id,object obj,string error,params string[] names) {
  foreach(var name in names){var v=obj.GetType().GetField(name,F).GetValue(obj);var a=v as Array;
   rows.Add(String.Join(",",family,id,name,Bits(v),a==null?"scalar":String.Join("x",Enumerable.Range(0,a.Rank).Select(i=>a.GetLength(i).ToString())),error));}
 }
 public static void Run(string build,string output) {
  CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
  var asm=Assembly.LoadFrom(Path.Combine(build,"BESHstatNG.dll"));
  var xyz=asm.GetType("BESHStatNG.graphics.XYZscatter",true);var scatter=asm.GetType("BESHStatNG.graphics.ScatterPlotMatrix",true);
  var rows=new List<string>{"family,id,field,ieee_or_value,shape,exception"};
  double[][] inputs={new double[]{-3,0,2,8},new double[]{1},new double[]{0,0,0},new double[]{},new double[]{-0.0,0,1,1},new double[]{Double.NaN,1,Double.PositiveInfinity},new double[]{-1e150,1e150},new double[]{1e-150,2e-150}};
  string[] fields={"raw_mins_","raw_ranges_","rot_1","rot_2","x_norm","y_norm","z_norm","xs_norm_data","ys_norm_data","error_bars","xs_cage","ys_cage","ts","ft","tx","ftn","tsn","xs_x_axisticks","ys_x_axisticks","xs_y_axisticks","ys_y_axisticks","xs_z_axisticks","ys_z_axisticks","x_tick_labels","y_tick_labels","z_tick_labels","xs_xy_x_gridline","ys_xy_x_gridline","xs_xy_y_gridline","ys_xy_y_gridline","xs_xz_x_gridline","ys_xz_x_gridline","xs_xz_z_gridline","ys_xz_z_gridline","xs_yz_y_gridline","ys_yz_y_gridline","xs_yz_z_gridline","ys_yz_z_gridline","xs_cutplane_x","ys_cutplane_x","xs_cutplane_y","ys_cutplane_y","xs_cutplane_z","ys_cutplane_z"};
  for(int f=0;f<inputs.Length;f++)for(int flip=0;flip<8;flip++)for(int mode=0;mode<3;mode++) {
   var o=Activator.CreateInstance(xyz,true);var values=(double[])inputs[f].Clone();
   string error=Call(o,"dataInputs",values,values.Reverse().ToArray(),values.Select(x=>2*x).ToArray());
   if(error=="None")error=Call(o,"ScaleAxis",mode==1);
   if(error=="None")error=Call(o,"AxisDirectionInputs",(flip&1)!=0,(flip&2)!=0,(flip&4)!=0);
   if(error=="None")error=Call(o,"rotationAndZoomInputs",mode==2?-50.0:0.0,mode==2?-100.0:50.0,mode==2?200.0:50.0,mode==0?120.0:90.0,mode==0?60.0:0.0);
   foreach(var method in new[]{"get_rotations","cut_planes","cage_data","tick_marks_data","get_gridlines","normalizedData"})if(error=="None")error=Call(o,method);
   Fields(rows,"xyz",f+"-"+flip+"-"+mode,o,error,fields);
  }
  for(int p=1;p<=4;p++)for(int mode=0;mode<4;mode++)for(int edge=0;edge<4;edge++) {
   var data=new double[edge==1?1:5,p];for(int i=0;i<data.GetLength(0);i++)for(int j=0;j<p;j++)data[i,j]=edge==2?2.0:edge==3&&i==0?Double.NaN:(i*i+2*j*i-j);
   var o=Activator.CreateInstance(scatter,F,null,new object[]{data,Enumerable.Repeat("same",p).ToArray(),null,0.37},CultureInfo.InvariantCulture);
   Call(o,"settingInputs",(mode&1)!=0,(mode&2)!=0);
   for(int repeat=0;repeat<2;repeat++) {
    var error=Call(o,"transformData");Fields(rows,"scatter",p+"-"+mode+"-"+edge+"-"+repeat,o,error,"pMargin","ScaledData","CorrMatrix","x_var_labels","x_corr_labels","y_corr_labels","x_regline","y_regline");
   }
  }
  File.WriteAllLines(output,rows,new System.Text.UTF8Encoding(false));
 }
}
