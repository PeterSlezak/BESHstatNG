using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Web.Script.Serialization;

// Metadata/IL inspection only. No UDF, cache initializer, Excel API or COM call.
public static class Inspect38 {
 static readonly Dictionary<short,OpCode> Opcodes=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(c=>c.Value);
 static string Identity(MethodBase m){return m.DeclaringType.FullName+"."+m.Name+"("+String.Join(",",m.GetParameters().Select(p=>p.ParameterType.FullName??p.ParameterType.ToString()))+")";}
 static object Attribute(CustomAttributeData a){return new{type=a.AttributeType.FullName,constructor=a.ConstructorArguments.Select(v=>new{type=v.ArgumentType.FullName,value=v.Value}).ToArray(),named=a.NamedArguments.Select(v=>new{name=v.MemberName,type=v.TypedValue.ArgumentType.FullName,value=v.TypedValue.Value}).ToArray()};}
 static object[] Calls(MethodBase m){
  var body=m.GetMethodBody();if(body==null)return new object[0];var bytes=body.GetILAsByteArray();var rows=new List<object>();
  for(int i=0;i<bytes.Length;){int offset=i;short key=bytes[i++];if(key==0xfe)key=(short)(0xfe00|bytes[i++]);var op=Opcodes[key];int size;
   switch(op.OperandType){case OperandType.InlineNone:size=0;break;case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:size=1;break;case OperandType.InlineVar:size=2;break;case OperandType.InlineI8:case OperandType.InlineR:size=8;break;case OperandType.InlineSwitch:size=4+4*BitConverter.ToInt32(bytes,i);break;default:size=4;break;}
   if(op.OperandType==OperandType.InlineMethod){
    int token=BitConverter.ToInt32(bytes,i);var target=m.Module.ResolveMethod(token,m.DeclaringType.GetGenericArguments(),m.IsGenericMethod?m.GetGenericArguments():null);
    rows.Add(new{offset,opcode=op.Name,target=Identity(target),assembly=target.DeclaringType.Assembly.GetName().Name,metadata_token=token});
   }
   i+=size;
  }
  return rows.ToArray();
 }
 public static int Main(string[] args){try{return Inspect(args);}catch(Exception e){Console.Error.WriteLine(e.GetType().FullName+": "+e.Message);return 1;}}
 static int Inspect(string[] args){
  string directory=Path.GetFullPath(args[0]),destination=args[1];if(File.Exists(destination))throw new Exception("Append-only inspection exists");
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(directory,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
  var assembly=Assembly.LoadFrom(Path.Combine(directory,"BESHStatNG.dll"));
  var serializer=new JavaScriptSerializer();serializer.MaxJsonLength=int.MaxValue;
  var inventory=serializer.Deserialize<Dictionary<string,object>>(File.ReadAllText(args[2],Encoding.UTF8));
  var sourceTypes=new HashSet<string>(((System.Collections.IEnumerable)inventory["members"]).Cast<Dictionary<string,object>>().Select(r=>(string)r["containing_type"]),StringComparer.Ordinal);
  var methods=new List<object>();var exports=new List<object>();
  foreach(var type in assembly.GetTypes().Where(t=>sourceTypes.Contains(t.FullName)||sourceTypes.Contains(t.FullName.Replace('+','.')))){
   foreach(var method in type.GetMethods(BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)){
    methods.Add(new{method=Identity(method),declaring_type=type.FullName,name=method.Name,return_type=method.ReturnType.FullName,attributes=method.CustomAttributes.Select(Attribute).ToArray(),calls=Calls(method)});
    var export=method.CustomAttributes.SingleOrDefault(a=>a.AttributeType.FullName=="ExcelDna.Integration.ExcelFunctionAttribute");
    if(export!=null){
     string effective=(string)export.NamedArguments.Single(a=>a.MemberName=="Name").TypedValue.Value;
     exports.Add(new{effective_excel_name=effective,method=Identity(method),return_type=method.ReturnType.FullName,attribute=Attribute(export),
       arguments=method.GetParameters().Select(p=>new{name=p.Name,type=p.ParameterType.FullName,position=p.Position,optional=p.IsOptional,has_default=p.HasDefaultValue,default_type=p.DefaultValue==null?null:p.DefaultValue.GetType().FullName,default_value=p.DefaultValue,attributes=p.CustomAttributes.Select(Attribute).ToArray()}).ToArray()});
    }
   }
  }
  if(exports.Count!=268)throw new Exception("Compiled export count is not268");
  File.WriteAllText(destination,serializer.Serialize(new{provenance="COMPILED_ENTRY_METADATA_AND_IL_ONLY; NATIVE_NOT_RUN",assembly=assembly.GetName().Name,mvid=assembly.ManifestModule.ModuleVersionId.ToString(),exports,methods}),new UTF8Encoding(false));
  Console.WriteLine("Inspected268 compiled exports and "+methods.Count+" methods; no UDF/native execution");return 0;
 }
}
