// Current-only passive GUI/UDF shared-engine call edges. Never invokes a target.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Web.Script.Serialization;
class ConsumerEdges {
 static void Main(string[] args) {
  string folder=Path.GetFullPath(args[0]);AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
  var ops=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(o=>o.Value);var rows=new List<object>();var a=Assembly.LoadFrom(Path.Combine(folder,"BESHStatNG.dll"));
  foreach(var t in a.GetTypes())foreach(var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly)) {
   var body=m.GetMethodBody();if(body==null)continue;byte[] il=body.GetILAsByteArray();int p=0;
   while(p<il.Length) {
    int offset=p;short code=il[p++];if(code==254)code=(short)(0xfe00|il[p++]);var op=ops[code];int size=0;
    switch(op.OperandType){case OperandType.InlineNone:break;case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:size=1;break;case OperandType.InlineVar:size=2;break;case OperandType.InlineI8:case OperandType.InlineR:size=8;break;case OperandType.InlineSwitch:size=4+4*BitConverter.ToInt32(il,p);break;default:size=4;break;}
    if(op.OperandType==OperandType.InlineMethod) {
     var target=m.Module.ResolveMethod(BitConverter.ToInt32(il,p),t.IsGenericType?t.GetGenericArguments():null,m.IsGenericMethod?m.GetGenericArguments():null);string owner=target.DeclaringType.FullName??"";
     if(owner.StartsWith("BESHStatNG.Agreement.")||owner=="BESHStatNG.regression.BinaryClassificationCore"||owner=="BESHStatNG.regression.BinaryClassificationReporting"||owner=="BESHStatNG.ExcelCommon.RegressionClassificationPolicy")rows.Add(new{caller=t.FullName+"."+m.Name,caller_parameters=m.GetParameters().Select(x=>x.ParameterType.FullName).ToArray(),offset,op=op.Name,target_owner=owner,target=target.Name,target_parameters=target.GetParameters().Select(x=>x.ParameterType.FullName).ToArray(),target_assembly=target.DeclaringType.Assembly.GetName().Name});
    }
    p+=size;
   }
  }
  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};if(File.Exists(args[1]))throw new Exception("Append-only output exists");File.WriteAllText(args[1],json.Serialize(new{scope="CURRENT_PASSIVE_CALL_EDGES_ONLY_NO_GUI_OR_ENGINE_INVOCATION",rows})+"\n");Console.WriteLine(rows.Count);
 }
}
