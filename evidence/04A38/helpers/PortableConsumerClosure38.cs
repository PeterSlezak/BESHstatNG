using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Web.Script.Serialization;

// Original production assembly metadata only. Never invokes a method or a type
// initializer. Records exact resolved CIL edges, including reachable host helpers.
public static class PortableConsumerClosure38 {
 static readonly Dictionary<short,OpCode> Opcodes=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(c=>c.Value);
 static string Identity(MethodBase m){return m.DeclaringType.FullName+"."+m.Name+"("+String.Join(",",m.GetParameters().Select(p=>p.ParameterType.FullName??p.ParameterType.ToString()))+")";}
 static IEnumerable<KeyValuePair<int,MethodBase>> Calls(MethodBase m){
  var body=m.GetMethodBody();if(body==null)yield break;var bytes=body.GetILAsByteArray();
  for(int i=0;i<bytes.Length;){int offset=i;short key=bytes[i++];if(key==0xfe)key=(short)(0xfe00|bytes[i++]);var op=Opcodes[key];int size;
   switch(op.OperandType){case OperandType.InlineNone:size=0;break;case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:size=1;break;case OperandType.InlineVar:size=2;break;case OperandType.InlineI8:case OperandType.InlineR:size=8;break;case OperandType.InlineSwitch:size=4+4*BitConverter.ToInt32(bytes,i);break;default:size=4;break;}
   if(op.OperandType==OperandType.InlineMethod){int token=BitConverter.ToInt32(bytes,i);yield return new KeyValuePair<int,MethodBase>(offset,m.Module.ResolveMethod(token,m.DeclaringType.GetGenericArguments(),m.IsGenericMethod?m.GetGenericArguments():null));}
   i+=size;
  }
 }
 public static int Main(string[] args){try{return Execute(args);}catch(Exception e){Console.Error.WriteLine(e.GetType().FullName+": "+e.Message);return 1;}}
 static int Execute(string[] args){
  string directory=Path.GetFullPath(args[0]);if(File.Exists(args[2]))throw new Exception("Append-only consumer closure exists");
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var p=Path.Combine(directory,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
  var serializer=new JavaScriptSerializer();serializer.MaxJsonLength=int.MaxValue;
  var census=serializer.Deserialize<Dictionary<string,object>>(File.ReadAllText(args[1],Encoding.UTF8));
  var owners=new HashSet<string>(((IEnumerable)census["members"]).Cast<Dictionary<string,object>>().Select(r=>(string)r["containing_type"]),StringComparer.Ordinal);
  var assembly=Assembly.LoadFrom(Path.Combine(directory,"BESHStatNG.dll"));
  var roots=new List<MethodBase>();
  foreach(var type in assembly.GetTypes().Where(t=>owners.Any(o=>t.FullName.Replace('+','.')==o||t.FullName.Replace('+','.').StartsWith(o+".",StringComparison.Ordinal)))){
   roots.AddRange(type.GetMethods(BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly));
   roots.AddRange(type.GetConstructors(BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly));
   if(type.TypeInitializer!=null)roots.Add(type.TypeInitializer);
  }
  var pending=new Queue<MethodBase>(roots);var visited=new HashSet<string>(StringComparer.Ordinal);var methods=new List<object>();
  while(pending.Count>0){var method=pending.Dequeue();string key=Identity(method);if(!visited.Add(key))continue;
   var edges=new List<object>();foreach(var call in Calls(method)){
    var target=call.Value;string targetAssembly=target.DeclaringType.Assembly.GetName().Name;
    edges.Add(new{offset=call.Key,target=Identity(target),assembly=targetAssembly});
    if((targetAssembly=="BESHStatNG"||targetAssembly=="BESHStat.Core"||targetAssembly=="BESHStat.Excel.Common")&&!visited.Contains(Identity(target)))pending.Enqueue(target);
   }
   methods.Add(new{method=key,declaring_type=method.DeclaringType.FullName,name=method.Name,root=roots.Any(m=>Identity(m)==key),calls=edges});
  }
  File.WriteAllText(args[2],serializer.Serialize(new{provenance="PRODUCTION_CIL_TRANSITIVE_CALL_CLOSURE; METADATA_ONLY; NO_NATIVE_EXECUTION",root_count=roots.Select(Identity).Distinct().Count(),methods}),new UTF8Encoding(false));
  Console.WriteLine("Resolved Windows/Core/Common consumer closure: "+methods.Count+" methods");return 0;
 }
}
