using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

// Metadata only: no method, type initializer, native API, model or oracle runs.
public static class MethodBodies38 {
 static HashSet<string> NativeOwners;
 static Type RootType(Type t){while(t.DeclaringType!=null)t=t.DeclaringType;return t;}
 static string Hash(string path){using(var h=SHA256.Create())return String.Concat(h.ComputeHash(File.ReadAllBytes(path)).Select(b=>b.ToString("x2")));}
 static string AssemblyNameOf(Type t){return t.Assembly.FullName;}
 static object AssemblyRecord(Assembly a){return new{name=a.FullName,file_name=Path.GetFileName(a.Location),bytes=new FileInfo(a.Location).Length,sha256=Hash(a.Location),MVID=a.ManifestModule.ModuleVersionId.ToString()};}

 static readonly Dictionary<short,OpCode> Codes=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(c=>c.Value);
 static string TypeName(Type t){if(t.IsGenericParameter)return (t.DeclaringMethod==null?"!":"!!")+t.GenericParameterPosition;return (t.FullName??t.ToString())+" @ "+AssemblyNameOf(t);}
 static string Name(MethodBase m){return TypeName(m.DeclaringType)+"."+m.Name+(m.IsGenericMethod?"``"+m.GetGenericArguments().Length:"")+"("+String.Join(",",m.GetParameters().Select(p=>TypeName(p.ParameterType)))+")";}
 sealed class Instruction {public int offset;public string op;public object operand;public int[] branches;}
 static object Read(MethodBase method){
  var body=method.GetMethodBody();if(body==null)return null;var b=body.GetILAsByteArray();var list=new List<Instruction>();int p=0;
  while(p<b.Length){int start=p;short key=b[p++];if(key==0xfe)key=(short)(0xfe00|b[p++]);var code=Codes[key];var n=new Instruction{offset=start,op=code.Name};
   switch(code.OperandType){
    case OperandType.InlineNone:break;
    case OperandType.ShortInlineI:n.operand=(int)(sbyte)b[p++];break;
    case OperandType.InlineI:n.operand=BitConverter.ToInt32(b,p);p+=4;break;
    case OperandType.InlineI8:n.operand=BitConverter.ToInt64(b,p).ToString("X16",CultureInfo.InvariantCulture);p+=8;break;
    case OperandType.ShortInlineR:n.operand=BitConverter.ToInt32(b,p).ToString("X8",CultureInfo.InvariantCulture);p+=4;break;
    case OperandType.InlineR:n.operand=BitConverter.ToInt64(b,p).ToString("X16",CultureInfo.InvariantCulture);p+=8;break;
    case OperandType.ShortInlineVar:n.operand=(int)b[p++];break;
    case OperandType.InlineVar:n.operand=(int)BitConverter.ToUInt16(b,p);p+=2;break;
    case OperandType.ShortInlineBrTarget:int shortDistance=(sbyte)b[p++];n.branches=new[]{p+shortDistance};break;
    case OperandType.InlineBrTarget:int distance=BitConverter.ToInt32(b,p);p+=4;n.branches=new[]{p+distance};break;
    case OperandType.InlineSwitch:int count=BitConverter.ToInt32(b,p);p+=4;int end=p+4*count;var targets=new int[count];for(int i=0;i<count;i++){targets[i]=end+BitConverter.ToInt32(b,p);p+=4;}n.branches=targets;break;
    default:
     int token=BitConverter.ToInt32(b,p);p+=4;
     var ta=method.DeclaringType.GetGenericArguments();var ma=method.IsGenericMethod?method.GetGenericArguments():null;
     if(code.OperandType==OperandType.InlineString)n.operand="STRING:"+method.Module.ResolveString(token);
     else if(code.OperandType==OperandType.InlineMethod)n.operand="METHOD:"+Name(method.Module.ResolveMethod(token,ta,ma));
     else if(code.OperandType==OperandType.InlineType)n.operand="TYPE:"+TypeName(method.Module.ResolveType(token,ta,ma));
     else if(code.OperandType==OperandType.InlineField){var f=method.Module.ResolveField(token,ta,ma);n.operand="FIELD:"+f.DeclaringType.FullName+"."+f.Name+":"+TypeName(f.FieldType);}
     else if(code.OperandType==OperandType.InlineTok){var x=method.Module.ResolveMember(token,ta,ma);n.operand=x is Type?"TYPE:"+TypeName((Type)x):x is MethodBase?"METHOD:"+Name((MethodBase)x):"TOKEN:"+x.DeclaringType.FullName+"."+x.Name;}
     else throw new Exception("Unsupported IL operand "+code.OperandType);
     break;
   }
   list.Add(n);
  }
  // NOPs have no CLR effect; branches resolve to the next semantic instruction.
  var real=list.Where(n=>n.op!="nop").ToArray();
  Func<int,int> index=offset=>{if(offset==b.Length)return real.Length;if(!list.Any(n=>n.offset==offset))throw new Exception("Invalid IL boundary "+offset);int i=Array.FindIndex(real,n=>n.offset>=offset);return i<0?real.Length:i;};
  return new{method=Name(method),declaring_type=method.DeclaringType.FullName,member_name=method.Name,parameter_types=method.GetParameters().Select(p=>p.ParameterType.FullName).ToArray(),attributes=method.Attributes.ToString(),implementation_flags=method.GetMethodImplementationFlags().ToString(),raw_IL_sha256=String.Concat(SHA256.Create().ComputeHash(b).Select(x=>x.ToString("x2"))),type_initializer=method.DeclaringType.TypeInitializer!=null,nonliteral_static_fields=method.DeclaringType.GetFields(BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Count(f=>!f.IsLiteral),locals=body.LocalVariables.Select(v=>TypeName(v.LocalType)).ToArray(),init_locals=body.InitLocals,
   exceptions=body.ExceptionHandlingClauses.Select(e=>new{flags=e.Flags.ToString(),try_start=index(e.TryOffset),try_end=index(e.TryOffset+e.TryLength),handler_start=index(e.HandlerOffset),handler_end=index(e.HandlerOffset+e.HandlerLength),filter_start=e.Flags==ExceptionHandlingClauseOptions.Filter?(int?)index(e.FilterOffset):null,catch_type=e.Flags==ExceptionHandlingClauseOptions.Clause?TypeName(e.CatchType):null}).ToArray(),
   instructions=real.Select(n=>new{op=n.branches==null?n.op:n.op.Replace(".s",""),operand=n.branches==null?n.operand:(object)n.branches.Select(index).ToArray()}).ToArray()};
 }
 public static int Main(string[] args){try{
  string folder=Path.GetFullPath(args[0]),output=args[1];if(File.Exists(output))throw new Exception("Append-only output exists");
  NativeOwners=new HashSet<string>(File.ReadAllLines(args[2]),StringComparer.Ordinal);
  var typeRecords=new List<object>();var assemblyRecords=new List<object>();
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
  var result=new List<object>();foreach(string name in new[]{"BESHStatNG","BESHStat.Core","BESHStat.Excel.Common"}){
   var assembly=Assembly.LoadFrom(Path.Combine(folder,name+".dll"));assemblyRecords.Add(AssemblyRecord(assembly));
   var types=assembly.GetTypes().Where(t=>name=="BESHStatNG"?NativeOwners.Contains(RootType(t).FullName):name=="BESHStat.Core"?(RootType(t).FullName.StartsWith("BESHStatNG.WorksheetCompatibility.Udf",StringComparison.Ordinal)||RootType(t).FullName=="BESHStatNG.ConfidenceIntervalResult"||RootType(t).FullName=="BESHStatNG.CIformat"||RootType(t).FullName=="BESHStatNG.ResultTable"||RootType(t).FullName=="BESHStatNG.ResultTableOutputModel"||RootType(t).FullName=="BESHStatNG.Matrix.MatrixDecompositionCore"||t.FullName.StartsWith("BESHStatNG.parametric.Parametric+OneWayANOVA",StringComparison.Ordinal)||t.FullName=="Microsoft.VisualBasic.CompilerServices.ProjectData"):RootType(t).Namespace=="BESHStatNG.ExcelCommon"&&RootType(t).Name.Contains("38"));
   foreach(var t in types){
    typeRecords.Add(new{assembly=name,type=t.FullName,root_owner=RootType(t).FullName,base_type=t.BaseType==null?null:TypeName(t.BaseType),attributes=t.Attributes.ToString(),interfaces=t.GetInterfaces().Select(TypeName).OrderBy(x=>x).ToArray(),fields=t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Select(f=>new{name=f.Name,type=TypeName(f.FieldType),attributes=f.Attributes.ToString(),is_static=f.IsStatic,is_readonly=f.IsInitOnly,is_literal=f.IsLiteral,literal=f.IsLiteral?f.GetRawConstantValue():null}).ToArray()});
    foreach(var method in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Cast<MethodBase>().Concat(t.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly))){var row=Read(method);if(row!=null)result.Add(row);}}
  }
  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};File.WriteAllText(output,json.Serialize(new{schema="PASSIVE_METADATA_V6",metadata_only=true,original_semantic_calls=0,scope=NativeOwners.OrderBy(x=>x).ToArray(),assemblies=assemblyRecords,loaded_modules=AppDomain.CurrentDomain.GetAssemblies().Where(a=>!a.IsDynamic&&File.Exists(a.Location)).Select(AssemblyRecord).ToArray(),types=typeRecords,methods=result}),new System.Text.UTF8Encoding(false));Console.WriteLine("Read "+result.Count+" CLR method bodies; metadata only");return 0;
 }catch(Exception e){Console.Error.WriteLine(e.GetType().FullName+": "+e.Message);return 1;}}
}
