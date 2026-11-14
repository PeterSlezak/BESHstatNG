using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

// Recorder/input decoder only. No statistical, import, option, table or expected
// result logic. Invokes the separately compiled authentic/current UDF sources.
public static class GeeCapture38 {
 static string ActiveHandle;
 static readonly List<object> Sessions=new List<object>();
 static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
 static Assembly Library,Core;static string Phase;static Type Branch;
 sealed class Log:BESHStatNG.AppInfrastructure.IAppLogger {
  public readonly List<string> Errors=new List<string>();
  public void Info(string m){} public void Warn(string m){} public void Debug(string m){} public void Trace(string m){} public void Dispose(){}
  public void WriteError(string m){Errors.Add("TEXT:"+B64(m));} public void Error(string m){WriteError(m);}
  public void WriteError(Exception e,string m){Errors.Add(B64(m)+":"+GeeCapture38.Error(e));} public void Error(Exception e,string m=null){WriteError(e,m);}
 }
 static string B64(string s){return Convert.ToBase64String(Encoding.UTF8.GetBytes(s??""));}
 static string Hash(byte[] data){using(var sha=SHA256.Create())return String.Concat(sha.ComputeHash(data).Select(b=>b.ToString("x2",Inv)));}
 static string Error(Exception e){while(e is TargetInvocationException&&e.InnerException!=null)e=e.InnerException;var a=e as ArgumentException;return e.GetType().FullName+";message="+B64(e.Message)+";parameter="+(a==null?"N":"S"+B64(a.ParamName));}
 static Dictionary<string,object> Dict(object o){return (Dictionary<string,object>)o;}
 static object[] Seq(object o){return ((IEnumerable)o).Cast<object>().ToArray();}
 static object Decode(Dictionary<string,object> d){
  switch((string)d["kind"]){
   case "Nothing":return null;
   case "HandleReference":if((string)d["session"]!="fresh-case"||ActiveHandle==null)throw new Exception("MISSING_FITTED_HANDLE");return ActiveHandle;
   case "String":return (string)d["value"];
   case "Int32":return Convert.ToInt32(d["value"],Inv);
   case "Double":return BitConverter.Int64BitsToDouble(unchecked((long)Convert.ToUInt64((string)d["bits"],16)));
   case "ExcelMissing":case "ExcelEmpty":{
    var t=AppDomain.CurrentDomain.GetAssemblies().Single(a=>a.GetName().Name=="ExcelDna.Integration").GetType("ExcelDna.Integration."+(string)d["kind"],true);
    return t.GetField("Value",BindingFlags.Public|BindingFlags.Static).GetValue(null);}
   case "ExcelError":{
    var t=AppDomain.CurrentDomain.GetAssemblies().Single(a=>a.GetName().Name=="ExcelDna.Integration").GetType("ExcelDna.Integration.ExcelError",true);return Enum.Parse(t,(string)d["name"]);}
   case "Array":{
    if((string)d["element_type"]!="System.Object")throw new Exception("Unsupported input element type");
    int rank=Convert.ToInt32(d["rank"],Inv);var lengths=Seq(d["lengths"]).Select(v=>Convert.ToInt32(v,Inv)).ToArray();var lower=Seq(d["lower_bounds"]).Select(v=>Convert.ToInt32(v,Inv)).ToArray();var upper=Seq(d["upper_bounds"]).Select(v=>Convert.ToInt32(v,Inv)).ToArray();
    if(rank!=lengths.Length||rank!=lower.Length||rank!=upper.Length||Enumerable.Range(0,rank).Any(i=>upper[i]!=lower[i]+lengths[i]-1))throw new Exception("Invalid input bounds");
    var values=Seq(d["values"]);var array=Array.CreateInstance(typeof(object),lengths,lower);if(array.Length!=values.Length)throw new Exception("Invalid input cardinality");var indices=(int[])lower.Clone();
    for(int i=0;i<values.Length;i++){array.SetValue(Decode(Dict(values[i])),indices);for(int dim=rank-1;dim>=0;dim--){indices[dim]++;if(indices[dim]<=upper[dim])break;indices[dim]=lower[dim];}}
    return array;}
   default:throw new Exception("Unsupported input kind");
  }
 }
 static string Snap(object value){
  if(value==null)return "N";
  if(value is string && ActiveHandle!=null && (string)value==ActiveHandle)return "HANDLE:GEE:SESSION1";
  if(value is string)return "S"+B64((string)value);
  if(value is double)return "D"+BitConverter.DoubleToInt64Bits((double)value).ToString("X16",Inv);
  if(value is float)return "F"+BitConverter.ToInt32(BitConverter.GetBytes((float)value),0).ToString("X8",Inv);
  if(value is bool)return (bool)value?"B1":"B0";
  if(value is DBNull)return "DBNULL";
  if(value is decimal)return "M"+String.Join(",",Decimal.GetBits((decimal)value).Select(v=>v.ToString("X8",Inv)));
  var type=value.GetType();if(type.IsEnum)return "ENUM"+type.FullName+":"+Convert.ToInt64(value,Inv);
  var array=value as Array;if(array!=null)return type.GetElementType().FullName+"["+String.Join(",",Enumerable.Range(0,array.Rank).Select(i=>array.GetLowerBound(i)+":"+array.GetLength(i)))+"]{"+String.Join("|",array.Cast<object>().Select(Snap))+"}";
  if(type.IsPrimitive)return type.FullName+":"+Convert.ToString(value,Inv);
  if(type.FullName=="ExcelDna.Integration.ExcelEmpty"||type.FullName=="ExcelDna.Integration.ExcelMissing")return type.FullName;
  var sequence=value as IEnumerable;if(sequence!=null)return "SEQ"+type.FullName+"{"+String.Join("|",sequence.Cast<object>().Select(Snap))+"}";
  throw new Exception("Unrecordable result type: "+type.FullName);
 }
 static string Kind(object result){if(result==null)return "RETURN_NULL";if(result is string && ActiveHandle!=null && (string)result==ActiveHandle)return "HANDLE_CREATED";if(result is string)return "RETURN_TEXT";if(result.GetType().FullName=="ExcelDna.Integration.ExcelError")return "RETURN_ERROR_CELL";if(result is Array)return ((Array)result).Length==0?"RETURN_EMPTY_ARRAY":"RETURN_ARRAY";return "RETURN_SCALAR";}
 static object[] Branches(){return Branch==null?new object[0]:((IEnumerable)Branch.GetMethod("Snapshot").Invoke(null,null)).Cast<object>().ToArray();}
 static int Execute(string[] args){
  string directory=Path.GetFullPath(args[0]),input=args[1],output=args[2],receipt=args[3],role=args[4];if(File.Exists(output)||File.Exists(receipt))throw new Exception("Append-only capture exists");
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var path=Path.Combine(directory,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
  Core=Assembly.LoadFrom(Path.Combine(directory,"BESHStat.Core.dll"));var common=Assembly.LoadFrom(Path.Combine(directory,"BESHStat.Excel.Common.dll"));
  Assembly.LoadFrom(Path.Combine(directory,"ExcelDna.Integration.dll"));Library=Assembly.LoadFrom(Path.Combine(directory,"BESHStat.Core.Tests.dll"));
  if(role=="original"&&(Core.GetType("BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38")!=null||common.GetType("BESHStatNG.ExcelCommon.UdfScalarChoice38")!=null))throw new Exception("ORIGINAL_LINKED_CURRENT_HELPER");
  foreach(var a in new[]{Core,common,Library})if(Path.GetDirectoryName(a.Location)!=directory)throw new Exception("ASSEMBLY_CONTEXT_MISMATCH");
  Branch=Library.GetType("BESHStatNG.GeeBranch38");
  var exports=Library.GetTypes().SelectMany(t=>t.GetMethods(BindingFlags.Public|BindingFlags.Static)).Where(m=>m.CustomAttributes.Any(a=>a.AttributeType.FullName=="ExcelDna.Integration.ExcelFunctionAttribute")).ToDictionary(m=>(string)m.CustomAttributes.Single(a=>a.AttributeType.FullName=="ExcelDna.Integration.ExcelFunctionAttribute").NamedArguments.Single(a=>a.MemberName=="Name").TypedValue.Value,m=>m,StringComparer.Ordinal);
  if(exports.Count!=268)throw new Exception("REGISTRATION_COUNT_MISMATCH");
  var log=new Log();BESHStatNG.AppInfrastructure.CoreServices.Logger=log;
  if(!Object.ReferenceEquals(Core.GetType("BESHStatNG.AppInfrastructure.CoreServices").GetProperty("Logger").GetValue(null,null),log))throw new Exception("LOGGER_CONTEXT_MISMATCH");
  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};var trace=new List<object>();var outcomes=new Dictionary<string,int>();int count=0;var started=DateTime.UtcNow;
  using(var writer=new StreamWriter(output,false,new UTF8Encoding(false))){writer.NewLine="\n";writer.WriteLine("case_id\tinput_sha256\toperations");
   foreach(var line in File.ReadLines(input,Encoding.UTF8)){
    var c=Dict(json.DeserializeObject(line));string culture=(string)c["culture"];System.Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(culture);System.Threading.Thread.CurrentThread.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
    var operations=new List<object>();var witnesses=new List<object>();
    ActiveHandle=null;
    var gee=Library.GetType("BESHStatNG.WorksheetFunctions.GEEUDFs",true);
    var cache=gee.GetField("_geeCache",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
    cache.GetType().GetMethod("Clear").Invoke(cache,null);
    int modelCreations=0;
    foreach(var item in Seq(c["operations"])){
     var op=Dict(item);log.Errors.Clear();if(Branch!=null)Branch.GetMethod("Clear").Invoke(null,null);object[] supplied=null;string before="N",result="N",error="None",kind="UNSET";
     Phase="BUILD_INPUT";
     try{
      MethodInfo target=(string)op["role"]=="PUBLIC_EXPORT"?exports[(string)op["target"]]:Library.GetType((string)op["owner"],true).GetMethod((string)op["target"],BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
      var parameters=target.GetParameters();var declarations=Seq(op["arguments"]).Select(Dict).ToArray();if(parameters.Length!=declarations.Length||parameters.Where((p,i)=>p.Name!=(string)declarations[i]["name"]).Any())throw new Exception("ARGUMENT_ORDER_MISMATCH");
      supplied=declarations.Select(d=>Decode(Dict(d["value"]))).ToArray();before=Snap(supplied);Phase="INVOKE_ORIGINAL_OR_CURRENT_SOURCE";var actual=target.Invoke(null,supplied);
      if((string)op["target"]=="BESH.REGR.GEE_FIT") {
       ActiveHandle=actual as string;
       if(ActiveHandle==null||!System.Text.RegularExpressions.Regex.IsMatch(ActiveHandle,@"^GEE:[0-9a-f]{32}$"))throw new Exception("FIT_DID_NOT_SUCCEED:"+Snap(actual));
       modelCreations++;
       var h=cache.GetType().GetProperty("Item").GetValue(cache,new object[]{ActiveHandle});
       Func<string,object> property=name=>h.GetType().GetProperty(name).GetValue(h,null);
       var names=(string[])property("VarNames");
       var fittedParameters=(string[])gee.GetMethod("BuildParameterNames",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{names,((double[,])property("FittedDesign")).GetLength(1)});
       if(!fittedParameters.Contains("X1"))throw new Exception("APPROVED_DESIGN_COLUMN_X1_MISSING");
       Sessions.Add(new{case_id=(string)c["case_id"],raw_handle=ActiveHandle,symbol="HANDLE:GEE:SESSION1",parameter_names=fittedParameters,
        fitted_design=Snap(property("FittedDesign")),fitted_time=Snap(property("FittedTimeValues")),has_time=Snap(property("HasTime")),alpha=Snap(property("Alpha")),creation_order=modelCreations});
      }
      kind=Kind(actual);result=Snap(actual);Phase="RETURNED_FROM_SOURCE";
     }catch(Exception e){kind="EXCEPTION";error=Error(e);}
     string key=(string)op["role"]+"/"+kind;if(!outcomes.ContainsKey(key))outcomes[key]=0;outcomes[key]++;
     operations.Add(new{role=(string)op["role"],target=(string)op["target"],kind,phase=Phase,input=before,result,byref_arguments=Snap(supplied),error,caught_errors=log.Errors.ToArray()});
     witnesses.Add(new{role=(string)op["role"],target=(string)op["target"],events=Branches()});
    }
    if(modelCreations!=1)throw new Exception("EXACTLY_ONE_SUCCESSFUL_FIT_REQUIRED");
    writer.WriteLine((string)c["case_id"]+"\t"+Hash(Encoding.UTF8.GetBytes(line+"\n"))+"\t"+json.Serialize(operations));
    trace.Add(new{case_id=(string)c["case_id"],culture,operations=witnesses});count++;
   }
  }
  int native=(int)Library.GetType("ExcelDna.Integration.XlCall").GetField("NativeAttempts").GetValue(null);if(native!=0)throw new Exception("NATIVE_ATTEMPT");
  if(count!=12)throw new Exception("CASE_COUNT_MISMATCH");
  File.WriteAllText(output+".sessions.json",json.Serialize(Sessions),new UTF8Encoding(false));
  if(Branch!=null)File.WriteAllText(output+".branches.json",json.Serialize(trace),new UTF8Encoding(false));
  var assemblies=AppDomain.CurrentDomain.GetAssemblies().Where(a=>!a.IsDynamic&&a.GetName().Name.StartsWith("BESHStat",StringComparison.OrdinalIgnoreCase)).Select(a=>new{name=a.GetName().Name,mvid=a.ManifestModule.ModuleVersionId.ToString(),sha256=Hash(File.ReadAllBytes(a.Location)),bytes=new FileInfo(a.Location).Length}).ToArray();
  File.WriteAllText(receipt,json.Serialize(new{role,configuration=Environment.GetEnvironmentVariable("BESHSTAT_LATE38_CONFIGURATION"),source_commit="ba1636f5356ac761509c2cfa16fc66b92430935d",reference_class="LATE_ARCHIVED_ENTRY_SUPPLEMENT",chronology="CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY",started_utc=started.ToString("o",Inv),completed_utc=DateTime.UtcNow.ToString("o",Inv),cases=count,outcomes,native_attempts=native,assemblies,runtime=Environment.Version.ToString(),native="NOT_RUN",scientific="NOT_GOLDEN"}),new UTF8Encoding(false));
  Console.WriteLine("Captured12 "+role+" cases; "+json.Serialize(outcomes)+"; native attempts0");return 0;
 }
 public static int Main(string[] args){try{return Execute(args);}catch(Exception e){Console.Error.WriteLine(Error(e));return 1;}}
}
