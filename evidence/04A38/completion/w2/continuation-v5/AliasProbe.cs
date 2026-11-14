// Current shipping implementations only. Expected side effects follow source assignment order.
using System;using System.IO;using System.Reflection;using System.Globalization;using System.Collections.Generic;using System.Web.Script.Serialization;
class AliasProbe {
 delegate bool Parse(object value,ref int output);
 delegate bool Positive(object value,ref int output,int defaultValue,int minimum);
 sealed class Box {public int Value=42;}
 sealed class Number:IConvertible {
  public Box Box;public double Value;public List<string> Calls=new List<string>();
  public double ToDouble(IFormatProvider p){Calls.Add("double:"+(p==null?"null":((CultureInfo)p).Name));Box.Value=17;return Value;}
  public TypeCode GetTypeCode(){return TypeCode.Double;}
  public string ToString(IFormatProvider p){return "number";}
  public bool ToBoolean(IFormatProvider p){throw new NotSupportedException();}public byte ToByte(IFormatProvider p){throw new NotSupportedException();}
  public char ToChar(IFormatProvider p){throw new NotSupportedException();}public DateTime ToDateTime(IFormatProvider p){throw new NotSupportedException();}
  public decimal ToDecimal(IFormatProvider p){throw new NotSupportedException();}public short ToInt16(IFormatProvider p){throw new NotSupportedException();}
  public int ToInt32(IFormatProvider p){throw new NotSupportedException();}public long ToInt64(IFormatProvider p){throw new NotSupportedException();}
  public sbyte ToSByte(IFormatProvider p){throw new NotSupportedException();}public float ToSingle(IFormatProvider p){throw new NotSupportedException();}
  public object ToType(Type t,IFormatProvider p){throw new NotSupportedException();}public ushort ToUInt16(IFormatProvider p){throw new NotSupportedException();}
  public uint ToUInt32(IFormatProvider p){throw new NotSupportedException();}public ulong ToUInt64(IFormatProvider p){throw new NotSupportedException();}
 }
 static void Main(string[] args){
  string folder=Path.GetFullPath(args[0]);AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
  var host=Assembly.LoadFrom(Path.Combine(folder,"BESHStatNG.dll"));var type=host.GetType("BESHStatNG.WorksheetFunctions.ExcelArgNumeric");var flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
  var binary=(Parse)Delegate.CreateDelegate(typeof(Parse),type.GetMethod("TryGetBinary01",flags));var whole=(Parse)Delegate.CreateDelegate(typeof(Parse),type.GetMethod("TryGetWholeNumber",flags));var positive=(Positive)Delegate.CreateDelegate(typeof(Positive),type.GetMethod("TryGetOptionalPositiveInteger",flags));
  var rows=new List<object>();foreach(string culture in new[]{"","sk-SK"}){CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   foreach(string name in new[]{"binary-invalid","whole-invalid","whole-nonfinite","positive-invalid","binary-overflow"}){
    var box=new Box();var input=new Number{Box=box,Value=name=="whole-nonfinite"?double.NaN:name=="binary-overflow"?double.MaxValue:2.25};bool? result=null;string exception=null;
    try{if(name.StartsWith("binary"))result=binary(input,ref box.Value);else if(name.StartsWith("whole"))result=whole(input,ref box.Value);else result=positive(input,ref box.Value,10,1);}catch(Exception ex){exception=ex.GetType().FullName;}
    rows.Add(new{culture=culture,name=name,result=result,exception=exception,output=box.Value,source_assignment_order_expected_output=17,conversion_calls=input.Calls});
   }
  }
  if(File.Exists(args[1]))throw new Exception("Append-only output exists");File.WriteAllText(args[1],new JavaScriptSerializer{MaxJsonLength=int.MaxValue}.Serialize(new{classification="CURRENT_ONLY_ALIASED_BYREF_PROBE_NOT_ORIGINAL_REFERENCE",original_calls=0,cases=rows}));Console.WriteLine("Current alias observations "+rows.Count);
 }
}
