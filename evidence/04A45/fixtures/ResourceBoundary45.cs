using System;using System.IO;using System.Linq;using System.Reflection;using System.Resources;using System.Web.Script.Serialization;
static class ResourceBoundary45 {
 static string dir;
 static Assembly Resolve(object sender,ResolveEventArgs e){var p=Path.Combine(dir,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.ReflectionOnlyLoadFrom(p):Assembly.ReflectionOnlyLoad(e.Name);}
 static void Main(string[] args){dir=Path.GetFullPath(args[0]);AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve+=Resolve;
  var rows=new[]{"BESHStat.Core","BESHStat.Excel.Common","BESHStatNG"}.Select(n=>{var a=Assembly.ReflectionOnlyLoadFrom(Path.Combine(dir,n+".dll"));return new {name=n,references=a.GetReferencedAssemblies().Select(r=>r.FullName).OrderBy(r=>r).ToArray(),target=a.GetCustomAttributesData().Where(d=>d.AttributeType.FullName=="System.Runtime.Versioning.TargetFrameworkAttribute").Select(d=>(string)d.ConstructorArguments[0].Value).ToArray()};}).ToArray();
  var host=Assembly.ReflectionOnlyLoadFrom(Path.Combine(dir,"BESHStatNG.dll"));string xml=null;
  foreach(var name in host.GetManifestResourceNames().Where(n=>n.EndsWith(".resources"))){using(var stream=host.GetManifestResourceStream(name))using(var rr=new ResourceReader(stream)){string type;byte[] data;try{rr.GetResourceData("RibbonXML",out type,out data);}catch(ArgumentException){continue;}if(type!="ResourceTypeCode.String")throw new InvalidDataException("Ribbon resource is not string");using(var br=new BinaryReader(new MemoryStream(data)))xml=br.ReadString();}}
  if(xml==null||xml!=File.ReadAllText(args[1]))throw new InvalidDataException("Compiled Ribbon resource differs");
  File.WriteAllText(args[2],new JavaScriptSerializer().Serialize(new {rows=rows,compiled_Ribbon_exact_source=true,no_attributes_constructed=true,no_resource_objects_constructed=true}));
 }
}
