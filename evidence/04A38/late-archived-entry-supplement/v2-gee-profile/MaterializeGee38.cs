using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

// Calls the frozen INPUT factory only. Does not call GEE_FIT or any query.
public static class MaterializeGee38 {
 static object Typed(object value) {
  if(value==null)return new {kind="Nothing"};
  if(value is string)return new {kind="String",value=(string)value};
  if(value is double)return new {kind="Double",bits=BitConverter.DoubleToInt64Bits((double)value).ToString("X16")};
  if(value is int)return new {kind="Int32",value=(int)value};
  var a=value as Array;
  if(a!=null)return new {kind="Array",element_type=a.GetType().GetElementType().FullName,rank=a.Rank,
   lengths=Enumerable.Range(0,a.Rank).Select(a.GetLength).ToArray(),lower_bounds=Enumerable.Range(0,a.Rank).Select(a.GetLowerBound).ToArray(),
   upper_bounds=Enumerable.Range(0,a.Rank).Select(a.GetUpperBound).ToArray(),values=a.Cast<object>().Select(Typed).ToArray()};
  throw new Exception("Unsupported factory input type: "+value.GetType().FullName);
 }
 public static int Main(string[] args) {
  string folder=Path.GetFullPath(args[0]);
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var p=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
  var library=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.Tests.dll"));
  var owner=library.GetType("BESHStatNG.WorksheetFunctions.GEEUDFs",true);
  var fit=owner.GetMethod("GEE_FIT");
  var values=(object[])typeof(Capture38).GetMethod("PublicArgs",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{"BESH.REGR.GEE_FIT",fit,true});
  var matrix=(object[,])typeof(Capture38).GetMethod("Matrix",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{36,2});
  var spec=new object[,]{{"Label","Weight","X1"},{"L",1.0,matrix[0,0]}};
  var result=new {fit_arguments=fit.GetParameters().Select((p,i)=>new {name=p.Name,value=Typed(values[i])}).ToArray(),
   spec=Typed(spec),query_defaults=owner.GetMethod("GEE_LSMESTIMATE").GetParameters().Where(p=>p.IsOptional).Select(p=>new {name=p.Name,value=Typed(p.DefaultValue)}).ToArray(),
   execution="INPUT_FACTORY_ONLY_NO_MODEL_OR_QUERY_EXECUTION"};
  if(File.Exists(args[1]))throw new Exception("Append-only materialization exists");
  File.WriteAllText(args[1],new JavaScriptSerializer{MaxJsonLength=int.MaxValue}.Serialize(result)+"\n",new UTF8Encoding(false));
  return 0;
 }
}
