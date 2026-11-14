"""Resolve actual closed generic/embedded/interface metadata, without aliases."""
from common import *
source=(HERE/'MethodMetadata-v10.cs').read_text(encoding='utf-8')
source=source.replace(' static HashSet<string> NativeOwners;',''' static HashSet<string> NativeOwners;
 static Queue<MethodBase> Pending=new Queue<MethodBase>();
 static HashSet<string> Recorded=new HashSet<string>(StringComparer.Ordinal);
 static void Enqueue(MethodBase m){if(new[]{"BESHStatNG","BESHStat.Core","BESHStat.Excel.Common"}.Contains(m.DeclaringType.Assembly.GetName().Name))Pending.Enqueue(m);}
 static object TypeRecord(Type t){return new{assembly=t.Assembly.GetName().Name,type=t.FullName,root_owner=RootType(t).FullName,base_type=t.BaseType==null?null:TypeName(t.BaseType),attributes=t.Attributes.ToString(),interfaces=t.GetInterfaces().Select(TypeName).OrderBy(x=>x).ToArray(),fields=t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Select(f=>new{name=f.Name,type=TypeName(f.FieldType),attributes=f.Attributes.ToString(),is_static=f.IsStatic,is_readonly=f.IsInitOnly,is_literal=f.IsLiteral,literal=f.IsLiteral?f.GetRawConstantValue():null}).ToArray()};}
''')
before='var body=method.GetMethodBody();if(body==null)return null;'
after='''var body=method.GetMethodBody();if(body==null)return new{method=Name(method),declaring_type=method.DeclaringType.FullName,member_name=method.Name,parameter_types=method.GetParameters().Select(p=>p.ParameterType.FullName).ToArray(),attributes=method.Attributes.ToString(),implementation_flags=method.GetMethodImplementationFlags().ToString(),no_body=true,instructions=new object[0],exceptions=new object[0],locals=new string[0]};'''
assert source.count(before)==1;source=source.replace(before,after)
before='n.operand="METHOD:"+Name(method.Module.ResolveMethod(token,ta,ma));'
after='{var resolved=method.Module.ResolveMethod(token,ta,ma);Enqueue(resolved);n.operand="METHOD:"+Name(resolved);}'
assert source.count(before)==1;source=source.replace(before,after)
before='var row=Read(method);if(row!=null)result.Add(row);'
after='if(Recorded.Add(Name(method))){var row=Read(method);if(row!=null)result.Add(row);}'
assert source.count(before)==1;source=source.replace(before,after)
before='  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};'
after='''  var extraTypes=new HashSet<Type>();
  while(Pending.Count>0){var method=Pending.Dequeue();if(!Recorded.Add(Name(method)))continue;var row=Read(method);if(row!=null)result.Add(row);extraTypes.Add(method.DeclaringType);}
  var baseTypes=new HashSet<string>(typeRecords.Select(r=>(string)r.GetType().GetProperty("type").GetValue(r,null)),StringComparer.Ordinal);
  foreach(var t in extraTypes.OrderBy(t=>t.FullName))if(baseTypes.Add(t.FullName))typeRecords.Add(TypeRecord(t));
  var json=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};'''
assert source.count(before)==1;source=source.replace(before,after).replace('PASSIVE_METADATA_V6','PASSIVE_METADATA_V7_ACTUAL_CONSTRUCTED_HANDLES')
with (HERE/'MethodMetadata-v11.cs').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
script=(HERE/'inspect_metadata_v2.py').read_text(encoding='utf-8').replace('metadata-all-v2','metadata-all-v3').replace('metadata-v2','metadata-v3').replace('context-v2','context-v3').replace('MethodMetadata-v10.cs','MethodMetadata-v11.cs')
with (HERE/'inspect_metadata_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(script)
save('actual-constructed-metadata-scope-v1.json',dict(source=pin(HERE/'MethodMetadata-v11.cs'),prior=pin(HERE/'MethodMetadata-v10.cs'),scope='Actual ResolveMethod handles, including closed constructed generic members, embedded compiler members and abstract interface declarations; recursive actual internal call operands. No name substitution or generic alias rule. Complete current/original fields and instructions are read without method/type-initializer execution.',original_queries=0))
