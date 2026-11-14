using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Script.Serialization;

// Metadata only: no method, type initializer, native API, model or oracle runs.
public static class MethodBodies38 {
 static readonly Dictionary<short,OpCode> Codes=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(c=>c.Value);
 static string TypeName(Type t){if(t.IsGenericParameter)return "!"+t.GenericParameterPosition;return t.FullName??t.ToString();}
 static string Name(MethodBase m){return m.DeclaringType.FullName+"."+m.Name+"("+String.Join(",",m.GetParameters().Select(p=>TypeName(p.ParameterType)))+")";}
 sealed class Instruction {public int offset;public string op;public object operand;public int[] branches;}
 static object Read(MethodInfo method){
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
  var real=list.Where(n=>n.op!="nop").ToArray();Func<int,int> index=offset=>Array.FindIndex(real,n=>n.offset>=offset);
  return new{method=Name(method),type_initializer=method.DeclaringType.TypeInitializer!=null,nonliteral_static_fields=method.DeclaringType.GetFields(BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Count(f=>!f.IsLiteral),locals=body.LocalVariables.Select(v=>TypeName(v.LocalType)).ToArray(),init_locals=body.InitLocals,
   exceptions=body.ExceptionHandlingClauses.Select(e=>new{flags=e.Flags.ToString(),try_start=index(e.TryOffset),try_end=index(e.TryOffset+e.TryLength),handler_start=index(e.HandlerOffset),handler_end=index(e.HandlerOffset+e.HandlerLength),catch_type=e.Flags==ExceptionHandlingClauseOptions.Clause?TypeName(e.CatchType):null}).ToArray(),
   instructions=real.Select(n=>new{op=n.branches==null?n.op:n.op.Replace(".s",""),operand=n.branches==null?n.operand:(object)n.branches.Select(index).ToArray()}).ToArray()};
 }
 public static int Main(string[] args){try{
  string folder=Path.GetFullPath(args[0]),output=args[1];if(File.Exists(output))throw new Exception("Append-only output exists");
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
  var result=new List<object>();foreach(string name in new[]{"BESHStatNG","BESHStat.Core","BESHStat.Excel.Common"}){
   var assembly=Assembly.LoadFrom(Path.Combine(folder,name+".dll"));
   var types=assembly.GetTypes().Where(t=>name=="BESHStatNG"?t.Namespace=="BESHStatNG.WorksheetFunctions"||t.FullName=="BESHStatNG.UdfDataImport":t.FullName.StartsWith("BESHStatNG.WorksheetCompatibility.Udf")||t.FullName.StartsWith("BESHStatNG.ExcelCommon.")&&t.Name.Contains("38"));
   foreach(var t in types)foreach(var method in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly)){var row=Read(method);if(row!=null)result.Add(row);}
  }
  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};File.WriteAllText(output,json.Serialize(result),new System.Text.UTF8Encoding(false));Console.WriteLine("Read "+result.Count+" CLR method bodies; metadata only");return 0;
 }catch(Exception e){Console.Error.WriteLine(e.GetType().FullName+": "+e.Message);return 1;}}
}
