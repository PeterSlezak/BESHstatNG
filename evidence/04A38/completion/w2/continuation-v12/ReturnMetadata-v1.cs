// Passive current-only declaration signatures. No initializer or target invocation.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Web.Script.Serialization;
class ReturnMetadata {
 static void Main(string[] args) {
  string folder=Path.GetFullPath(args[0]);AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
  var owners=new HashSet<string>(File.ReadAllLines(args[2]).Where(x=>x.Length>0));var rows=new List<object>();
  foreach(string name in new[]{"BESHStat.Core","BESHStat.Excel.Common","BESHStatNG"}) {
   var a=Assembly.LoadFrom(Path.Combine(folder,name+".dll"));
   foreach(var t in a.GetTypes().Where(t=>owners.Contains(t.FullName)))foreach(var m in t.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly))rows.Add(new{declaring_type=t.FullName,member_name=m.Name,parameter_types=m.GetParameters().Select(p=>p.ParameterType.FullName).ToArray(),return_type=m.ReturnType.FullName,metadata_token=m.MetadataToken,assembly=name});
  }
  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};if(File.Exists(args[1]))throw new Exception("Append-only output exists");File.WriteAllText(args[1],json.Serialize(new{scope="CURRENT_DECLARATIONS_ONLY_NO_TARGET_EXECUTION",methods=rows})+"\n");Console.WriteLine(rows.Count);
 }
}
