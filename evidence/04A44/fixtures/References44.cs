using System;using System.IO;using System.Linq;using System.Reflection;using System.Web.Script.Serialization;
class References44 {static void Main(string[] a){var x=Assembly.ReflectionOnlyLoadFrom(a[0]);var refs=x.GetReferencedAssemblies().Select(r=>r.FullName).OrderBy(r=>r).ToArray();File.WriteAllText(a[1],new JavaScriptSerializer().Serialize(new{name=x.GetName().Name,references=refs}));}}
