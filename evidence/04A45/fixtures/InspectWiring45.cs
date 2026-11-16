// Metadata and IL only. Never constructs a form, initializes Excel-DNA or resolves Application.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Script.Serialization;
static class InspectWiring45 {
 static readonly Dictionary<short,OpCode> Ops=typeof(OpCodes).GetFields().Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(o=>o.Value);
 static string Dir;
 static Assembly Resolve(object sender,ResolveEventArgs e) {
  var path=Path.Combine(Dir,new AssemblyName(e.Name).Name+".dll");
  return File.Exists(path)?Assembly.ReflectionOnlyLoadFrom(path):Assembly.ReflectionOnlyLoad(e.Name);
 }
 static List<object> Tokens(MethodBase method) {
  var rows=new List<object>();var body=method.GetMethodBody();if(body==null)return rows;
  var code=body.GetILAsByteArray();int pos=0;
  while(pos<code.Length) {
   int offset=pos;short value=code[pos++];if(value==0xfe)value=(short)(0xfe00|code[pos++]);var op=Ops[value];int size;
   switch(op.OperandType) {
    case OperandType.InlineNone:size=0;break;
    case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:size=1;break;
    case OperandType.InlineVar:size=2;break;
    case OperandType.InlineI8:case OperandType.InlineR:size=8;break;
    case OperandType.InlineSwitch:size=4+4*BitConverter.ToInt32(code,pos);break;
    default:size=4;break;
   }
   if(op.OperandType==OperandType.InlineString) {
    var text=method.Module.ResolveString(BitConverter.ToInt32(code,pos));
    if(new[]{"WorkbookBeforeClose","SheetBeforeDelete","WorkbookDeactivate","SheetSelectionChange","SheetActivate","SelectionChange"}.Contains(text))
     rows.Add(new {offset=offset,opcode=op.Name,owner="DECLARED_EVENT_NAME",name=text,signature="Whitelisted COM event name;no other string operands published"});
   }
   if(op.OperandType==OperandType.InlineMethod||op.OperandType==OperandType.InlineField||op.OperandType==OperandType.InlineType||op.OperandType==OperandType.InlineTok) {
    int token=BitConverter.ToInt32(code,pos);
    var member=method.Module.ResolveMember(token,method.DeclaringType.IsGenericType?method.DeclaringType.GetGenericArguments():null,method.IsGenericMethod?method.GetGenericArguments():null);
    rows.Add(new {offset=offset,opcode=op.Name,owner=member.DeclaringType==null?"":member.DeclaringType.FullName,name=member.Name,signature=member.ToString()});
   }
   pos+=size;
  }
  if(pos!=code.Length)throw new InvalidDataException("IL boundary mismatch");return rows;
 }
 static void Main(string[] args) {
  Dir=Path.GetFullPath(args[0]);AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve+=Resolve;
  var assembly=Assembly.ReflectionOnlyLoadFrom(Path.Combine(Dir,"BESHStatNG.dll"));
  var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly;
  var types=assembly.GetTypes().Where(t=>t.FullName.StartsWith("BESHStatNG.Ui")||t.FullName.Contains("Excel2007RefEdit")||t.FullName.Contains("WindowsDocumentLifecycle")||t.FullName.Contains("WinFormsProgressReporter")||t.FullName.Contains("HelpWiring")||t.FullName.Contains("WindowsMixedModelWorkflowAdapter")||t.FullName.Contains("RegressionEffectsController")||t.FullName=="BESHStatNG.Ribbon").ToArray();
  var rows=types.Select(t=>new {owner=t.FullName,base_type=t.BaseType==null?"":t.BaseType.FullName,fields=t.GetFields(flags).Select(f=>new{name=f.Name,type=f.FieldType.FullName}).ToArray(),methods=t.GetMethods(flags).Cast<MethodBase>().Concat(t.GetConstructors(flags)).Select(m=>new{name=m.Name,signature=m.ToString(),tokens=Tokens(m)}).ToArray()}).ToArray();
  var j=new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=100};File.WriteAllText(args[1],j.Serialize(new{kind="REFLECTION_ONLY_METADATA_AND_IL_NOT_RUNTIME_BINDING",mvid=assembly.ManifestModule.ModuleVersionId.ToString(),types=rows}));
 }
}
