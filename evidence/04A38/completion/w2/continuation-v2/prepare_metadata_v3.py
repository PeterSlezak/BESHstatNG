"""New passive reader; older metadata tools and their receipts remain immutable."""
from common import *
s=(W2/'MethodBodies-v2.cs').read_text(encoding='utf-8-sig')
s=s.replace('using System.Web.Script.Serialization;','using System.Web.Script.Serialization;\nusing System.Security.Cryptography;')
s=s.replace('public static class MethodBodies38 {','''public static class MethodBodies38 {
 static HashSet<string> NativeOwners;
 static Type RootType(Type t){while(t.DeclaringType!=null)t=t.DeclaringType;return t;}
 static string Hash(string path){using(var h=SHA256.Create())return String.Concat(h.ComputeHash(File.ReadAllBytes(path)).Select(b=>b.ToString("x2")));}
 static string AssemblyNameOf(Type t){return t.Assembly.FullName;}
 static object AssemblyRecord(Assembly a){return new{name=a.FullName,file_name=Path.GetFileName(a.Location),bytes=new FileInfo(a.Location).Length,sha256=Hash(a.Location),MVID=a.ManifestModule.ModuleVersionId.ToString()};}
''')
s=s.replace('if(t.IsGenericParameter)return "!"+t.GenericParameterPosition;return t.FullName??t.ToString();','if(t.IsGenericParameter)return (t.DeclaringMethod==null?"!":"!!")+t.GenericParameterPosition;return (t.FullName??t.ToString())+" @ "+AssemblyNameOf(t);')
s=s.replace('m.DeclaringType.FullName+"."+m.Name+"("','m.DeclaringType.FullName+" @ "+AssemblyNameOf(m.DeclaringType)+"."+m.Name+(m.IsGenericMethod?"``"+m.GetGenericArguments().Length:"")+"("')
s=s.replace('var real=list.Where(n=>n.op!="nop").ToArray();Func<int,int> index=offset=>Array.FindIndex(real,n=>n.offset>=offset);','''var real=list.Where(n=>n.op!="nop").ToArray();
  Func<int,int> index=offset=>{if(offset==b.Length)return real.Length;if(!list.Any(n=>n.offset==offset))throw new Exception("Invalid IL boundary "+offset);int i=Array.FindIndex(real,n=>n.offset>=offset);return i<0?real.Length:i;};''')
s=s.replace('return new{method=Name(method),','return new{method=Name(method),declaring_type=method.DeclaringType.FullName,member_name=method.Name,parameter_types=method.GetParameters().Select(p=>p.ParameterType.FullName).ToArray(),attributes=method.Attributes.ToString(),implementation_flags=method.GetMethodImplementationFlags().ToString(),raw_IL_sha256=String.Concat(SHA256.Create().ComputeHash(b).Select(x=>x.ToString("x2"))),')
s=s.replace('catch_type=e.Flags==ExceptionHandlingClauseOptions.Clause?TypeName(e.CatchType):null','filter_start=e.Flags==ExceptionHandlingClauseOptions.Filter?(int?)index(e.FilterOffset):null,catch_type=e.Flags==ExceptionHandlingClauseOptions.Clause?TypeName(e.CatchType):null')
s=s.replace('if(File.Exists(output))throw new Exception("Append-only output exists");','''if(File.Exists(output))throw new Exception("Append-only output exists");
  NativeOwners=new HashSet<string>(File.ReadAllLines(args[2]),StringComparer.Ordinal);
  var typeRecords=new List<object>();var assemblyRecords=new List<object>();''')
s=s.replace('var assembly=Assembly.LoadFrom(Path.Combine(folder,name+".dll"));','var assembly=Assembly.LoadFrom(Path.Combine(folder,name+".dll"));assemblyRecords.Add(AssemblyRecord(assembly));')
start='var types=assembly.GetTypes().Where('
a=s.index(start);b=s.index(';',a)
s=s[:a]+'''var types=assembly.GetTypes().Where(t=>name=="BESHStatNG"?NativeOwners.Contains(RootType(t).FullName):name=="BESHStat.Core"?RootType(t).FullName.StartsWith("BESHStatNG.WorksheetCompatibility.Udf",StringComparison.Ordinal):RootType(t).Namespace=="BESHStatNG.ExcelCommon"&&RootType(t).Name.Contains("38"))'''+s[b:]
s=s.replace('foreach(var t in types)foreach(var method in','''foreach(var t in types){
    typeRecords.Add(new{assembly=name,type=t.FullName,root_owner=RootType(t).FullName,base_type=t.BaseType==null?null:TypeName(t.BaseType),attributes=t.Attributes.ToString(),interfaces=t.GetInterfaces().Select(TypeName).OrderBy(x=>x).ToArray(),fields=t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Select(f=>new{name=f.Name,type=TypeName(f.FieldType),attributes=f.Attributes.ToString(),is_static=f.IsStatic,is_readonly=f.IsInitOnly,is_literal=f.IsLiteral,literal=f.IsLiteral?f.GetRawConstantValue():null}).ToArray()});
    foreach(var method in''')
s=s.replace('if(row!=null)result.Add(row);}','if(row!=null)result.Add(row);}}')
s=s.replace('json.Serialize(result)','json.Serialize(new{schema="PASSIVE_METADATA_V3",metadata_only=true,original_semantic_calls=0,scope=NativeOwners.OrderBy(x=>x).ToArray(),assemblies=assemblyRecords,loaded_modules=AppDomain.CurrentDomain.GetAssemblies().Where(a=>!a.IsDynamic&&File.Exists(a.Location)).Select(AssemblyRecord).ToArray(),types=typeRecords,methods=result})')
with (HERE/'MethodMetadata-v3.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)

u=json.loads((HERE/'current-universe-b1-v1.json').read_bytes());import re
owners=set()
for r in u['supports']:
 if r['kind']=='ModuleBlock':
  raw=(ROOT/r['path']).read_bytes().decode('utf-8-sig').encode('utf-16-le');fragment=raw[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
  name=re.search(r'(?im)^\s*(?:(?:Public|Friend|Partial)\s+)*Module\s+(\w+)',fragment).group(1)
  owners.add(r['owner'].rstrip('.')+'.'+name)
assert len(owners)==33
with (HERE/'native-metadata-owners.txt').open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(sorted(owners))+'\n')
save('metadata-v3-scope.json',dict(owners=sorted(owners),raw_module_declarations=sum(r['kind']=='ModuleBlock' for r in u['supports']),native_callback='Historical704th accepted native callback remains separately governed; not asserted covered by this33-UDF-module metadata scope',old_tool_unchanged=pin(W2/'MethodBodies-v2.cs'),new_tool=pin(HERE/'MethodMetadata-v3.cs')))
