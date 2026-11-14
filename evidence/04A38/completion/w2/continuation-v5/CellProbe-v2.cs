// CURRENT-only before/after shipping libraries. Never load archived originals.
using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using ExcelDna.Integration;

sealed class CellProbe {
    sealed class Convertible : IConvertible {
        public readonly List<string> Calls = new List<string>();
        public bool ThrowNumber, ThrowText;
        public TypeCode GetTypeCode(){Calls.Add("type");return TypeCode.Double;}
        public double ToDouble(IFormatProvider p){Calls.Add("double:"+(p==null?"null":((CultureInfo)p).Name));if(ThrowNumber)throw new FormatException("CURRENT_NUMBER");return 1.25;}
        public string ToString(IFormatProvider p){Calls.Add("text:"+(p==null?"null":((CultureInfo)p).Name));if(ThrowText)throw new FormatException("CURRENT_TEXT");return "  custom  ";}
        public override string ToString(){Calls.Add("override");return "override";}
        public bool ToBoolean(IFormatProvider p){throw new InvalidCastException();}
        public byte ToByte(IFormatProvider p){throw new InvalidCastException();}
        public char ToChar(IFormatProvider p){throw new InvalidCastException();}
        public DateTime ToDateTime(IFormatProvider p){throw new InvalidCastException();}
        public decimal ToDecimal(IFormatProvider p){throw new InvalidCastException();}
        public short ToInt16(IFormatProvider p){throw new InvalidCastException();}
        public int ToInt32(IFormatProvider p){throw new InvalidCastException();}
        public long ToInt64(IFormatProvider p){throw new InvalidCastException();}
        public sbyte ToSByte(IFormatProvider p){throw new InvalidCastException();}
        public float ToSingle(IFormatProvider p){throw new InvalidCastException();}
        public object ToType(Type t,IFormatProvider p){throw new InvalidCastException();}
        public ushort ToUInt16(IFormatProvider p){throw new InvalidCastException();}
        public uint ToUInt32(IFormatProvider p){throw new InvalidCastException();}
        public ulong ToUInt64(IFormatProvider p){throw new InvalidCastException();}
    }
    struct Mutable : IConvertible {
        public List<string> Calls; public int Writes;
        public TypeCode GetTypeCode(){Calls.Add("type");return TypeCode.Double;}
        public double ToDouble(IFormatProvider p){Writes++;Calls.Add("double:"+Writes+":"+(p==null?"null":((CultureInfo)p).Name));return 1.25;}
        public string ToString(IFormatProvider p){Writes++;Calls.Add("text:"+Writes+":"+(p==null?"null":((CultureInfo)p).Name));return "  mutable  ";}
        public bool ToBoolean(IFormatProvider p){throw new InvalidCastException();}
        public byte ToByte(IFormatProvider p){throw new InvalidCastException();}
        public char ToChar(IFormatProvider p){throw new InvalidCastException();}
        public DateTime ToDateTime(IFormatProvider p){throw new InvalidCastException();}
        public decimal ToDecimal(IFormatProvider p){throw new InvalidCastException();}
        public short ToInt16(IFormatProvider p){throw new InvalidCastException();}
        public int ToInt32(IFormatProvider p){throw new InvalidCastException();}
        public long ToInt64(IFormatProvider p){throw new InvalidCastException();}
        public sbyte ToSByte(IFormatProvider p){throw new InvalidCastException();}
        public float ToSingle(IFormatProvider p){throw new InvalidCastException();}
        public object ToType(Type t,IFormatProvider p){throw new InvalidCastException();}
        public ushort ToUInt16(IFormatProvider p){throw new InvalidCastException();}
        public uint ToUInt32(IFormatProvider p){throw new InvalidCastException();}
        public ulong ToUInt64(IFormatProvider p){throw new InvalidCastException();}
    }
    static object Value(object v){
        if(v==null)return null;
        if(v is double)return new{type=v.GetType().FullName,bits=BitConverter.DoubleToInt64Bits((double)v).ToString("X16")};
        if(v is Array){var a=(Array)v;var cells=new List<object>();foreach(object c in a)cells.Add(Value(c));return new{type=v.GetType().FullName,lengths=Dims(a,false),bounds=Dims(a,true),cells=cells};}
        return new{type=v.GetType().FullName,text=Convert.ToString(v,CultureInfo.InvariantCulture)};
    }
    static int[] Dims(Array a,bool bounds){var r=new int[a.Rank];for(int i=0;i<r.Length;i++)r[i]=bounds?a.GetLowerBound(i):a.GetLength(i);return r;}
    static void Seed(){ProjectData.ClearProjectError();Information.Err().Number=5;Information.Err().Description="CURRENT_SEED";Information.Err().Source="CURRENT_ONLY";}
    static object State(){var e=Information.Err();return new{number=e.Number,description=e.Description,source=e.Source};}
    public static int Main(string[] args){
        var folder=Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(folder,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
        var host=Assembly.LoadFrom(Path.Combine(folder,"BESHStatNG.dll"));
        var flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
        var text=host.GetType("BESHStatNG.WorksheetFunctions.ExcelArgReaders").GetMethod("CellToTrimmedText",flags);
        var number=host.GetType("BESHStatNG.WorksheetFunctions.ExcelArgNumeric").GetMethod("TryGetDouble",flags);
        var numeric=host.GetType("BESHStatNG.UdfOutputTables").GetMethod("IsNumericValue",flags);
        var rows=new List<object>();
        foreach(string culture in new[]{"","sk-SK"}){
            CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
            var inputs=new object[]{null,ExcelMissing.Value,ExcelEmpty.Value,ExcelError.ExcelErrorNA,DBNull.Value,true,false,"  a\u00a0","1,5","",1.25,1f,1.25m,(short)2,3,4L,(byte)5,(sbyte)6,(ushort)7,8U,ulong.MaxValue,long.MaxValue,double.MaxValue,double.NaN,double.PositiveInfinity,BitConverter.Int64BitsToDouble(long.MinValue),4e-13,DateTime.MinValue,new object[,]{{1.0,"x"}},new Convertible(),new Convertible{ThrowNumber=true},new Convertible{ThrowText=true},new Mutable{Calls=new List<string>()}};
            for(int i=0;i<inputs.Length;i++)foreach(var method in new[]{text,number,numeric}){
                object input=inputs[i],value=null;var probe=input as Convertible;var trace=probe!=null?probe.Calls:input is Mutable?((Mutable)input).Calls:null;if(trace!=null)trace.Clear();
                Seed();var before=State();string error=null;
                try{value=method.Invoke(null,new[]{input});}catch(TargetInvocationException ex){error=ex.InnerException.GetType().FullName+":"+ex.InnerException.Message;}
                var after=State();var calls=trace==null?new string[0]:trace.ToArray();
                rows.Add(new{culture=culture,input=i,method=method.Name,value=Value(value),exception=error,calls=calls,before=before,after=after,mutable_writes=input is Mutable?(int?)((Mutable)input).Writes:null,array=input is Array?Value(input):null});
                ProjectData.ClearProjectError();
            }
        }
        if(File.Exists(args[1]))throw new Exception("Append-only output exists");
        File.WriteAllText(args[1],new JavaScriptSerializer{MaxJsonLength=int.MaxValue}.Serialize(new{classification="CURRENT_ONLY_BEFORE_AFTER_NOT_ORIGINAL_REFERENCE",original_calls=0,cases=rows}));
        Console.WriteLine("Current conversion observations "+rows.Count);return 0;
    }
}
