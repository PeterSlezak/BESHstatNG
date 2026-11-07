// Metadata-token-normalized comparison of the accepted partial portable methods.
// No product method is invoked; both assemblies are loaded with LoadFile.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
public static class CompareEntryIl34 {
 static Dictionary<short,OpCode> ops=typeof(OpCodes).GetFields().Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(o=>o.Value);
 static string Member(MemberInfo m){return m.DeclaringType+"::"+m;}
 static string IL(MethodBase m){
  var b=m.GetMethodBody();if(b==null)return "NO_BODY";
  var data=b.GetILAsByteArray();var rows=new List<string>();
  rows.Add("locals:"+String.Join("|",b.LocalVariables.Select(v=>v.LocalType+":"+v.IsPinned)));
  rows.Add("exceptions:"+String.Join("|",b.ExceptionHandlingClauses.Select(c=>c.Flags+":"+c.TryOffset+":"+c.TryLength+":"+c.HandlerOffset+":"+c.HandlerLength+":"+(c.Flags==ExceptionHandlingClauseOptions.Clause?Convert.ToString(c.CatchType):""))));
  for(int i=0;i<data.Length;){short key=data[i++];if(key==254)key=(short)(0xfe00|data[i++]);var op=ops[key];string value="";int n=0;
   switch(op.OperandType){
    case OperandType.InlineNone:break;
    case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:n=1;break;
    case OperandType.InlineVar:n=2;break;
    case OperandType.InlineI8:case OperandType.InlineR:n=8;break;
    case OperandType.InlineSwitch:n=4+4*BitConverter.ToInt32(data,i);break;
    default:n=4;break;
   }
   if(op.OperandType==OperandType.InlineString)value=m.Module.ResolveString(BitConverter.ToInt32(data,i));
   else if(new[]{OperandType.InlineMethod,OperandType.InlineField,OperandType.InlineType,OperandType.InlineTok}.Contains(op.OperandType))value=Member(m.Module.ResolveMember(BitConverter.ToInt32(data,i),m.DeclaringType.GetGenericArguments(),m.IsGenericMethod?m.GetGenericArguments():null));
   else value=String.Join("",data.Skip(i).Take(n).Select(x=>x.ToString("X2")));
   rows.Add(op.Name+" "+value);i+=n;
  }return String.Join("\n",rows);
 }
 public static string Compare(string root,string[] types){
  var old=Assembly.LoadFile(Path.Combine(root,".test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll"));
  var current=Assembly.LoadFile(Path.Combine(root,".test-rationalization-04A34.tmp/builds/review/Debug/legacy/BESHStat.Core.dll"));
  var rows=new List<string>{"type,member,status"};
  foreach(var name in types){var a=old.GetType("BESHStatNG."+name,true);var b=current.GetType(a.FullName,true);
   var oldMethods=a.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Cast<MethodBase>().Concat(a.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance));
   var newMethods=b.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance|BindingFlags.DeclaredOnly).Cast<MethodBase>().Concat(b.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance));
   foreach(var m in oldMethods){var hits=newMethods.Where(x=>x.ToString()==m.ToString()).ToArray();if(hits.Length!=1||IL(m)!=IL(hits[0]))throw new InvalidOperationException("Partial method changed: "+name+"::"+m);
    rows.Add(name+",\""+m.ToString().Replace("\"","\"\"")+"\",MATCH_NORMALIZED_IL");
   }
  }
  return String.Join("\r\n",rows)+"\r\n";
 }
}
