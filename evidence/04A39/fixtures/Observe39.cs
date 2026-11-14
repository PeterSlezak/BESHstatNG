// Observation only. Runs detached host-free arguments against an explicitly pinned current DLL.
// No original-reference DLL, native Excel, estimators, workbook or UI calls.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.IO;
using BESHStatNG;
using ExcelDna.Integration;

public static class Observe39 {
    static readonly List<object> Rows = new List<object>();
    static string Bits(double value) { return BitConverter.DoubleToInt64Bits(value).ToString("x16", CultureInfo.InvariantCulture); }
    static object Cell(object value) {
        return new { type=value==null ? "Nothing" : value.GetType().FullName,
            text=Convert.ToString(value, CultureInfo.InvariantCulture),
            bits=value is double ? Bits((double)value) : null };
    }
    static object Invoke(string type, string name, params object[] arguments) {
        var method=typeof(DataObj).Assembly.GetType("BESHStatNG."+type,true).GetMethods(BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public).Single(m=>m.Name==name);
        try { return method.Invoke(null,arguments); }
        catch(TargetInvocationException e) { throw e.InnerException; }
    }
    static void Capture(string id, object input, Func<object> operation) {
        try { Rows.Add(new {id=id,input=input,outcome="VALUE",output=operation(),exception=(string)null}); }
        catch(Exception e) { Rows.Add(new {id=id,input=input,outcome="EXCEPTION",output=(object)null,exception=e.GetType().FullName+": "+e.Message}); }
    }
    static object Nested(object response, string culture) {
        var raw=new object[,] {{"G","S",response},{"G","S",culture=="sk-SK"?"3,5":"3.5"},{"H","T",2.0},{"H",null,999.0}};
        var imported=new DataObj();
        imported.DataImportRawMatrix(raw,new[]{"Group","Nested","Y"},firstSourceRow:20,CharCols:1);
        string error="prior";
        var adapted=WindowsPairedInputAdapter.SelectNested(imported,ref error);
        if(adapted==null) return new {error=error,empty=true};
        var original=WindowsPairedInputAdapter.GetNestedEvidence(adapted);
        var core=WindowsPairedInputAdapter.GetNestedCoreInput(adapted);
        var values=new List<object>();
        for(int row=0;row<core.Responses.Blocks.Count;row++) {
            var before=original.Responses.Blocks[row][0];var after=core.Responses.Blocks[row][0];
            values.Add(new {selectedRow=before.SelectedRow,sourceRow=before.WorksheetRow,sourceColumn=before.WorksheetColumn,
                original=Bits(before.Value),core=Bits(after.Value),sameBits=Bits(before.Value)==Bits(after.Value),
                matrix=Cell(adapted.X[row,2])});
        }
        return new {error=error,rows=values,groups=original.GroupLabels,nested=original.NestedLabels};
    }
    public static int Main(string[] args) {
        var prior=Thread.CurrentThread.CurrentCulture;
        try {
            foreach(var culture in new[]{"","sk-SK"}) {
                Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(culture);
                var responses=new Dictionary<string,object> {
                    {"double17",1.2345678901234567}, {"double16",123456789012345.67},
                    {"near2pow53",9007199254740991.0}, {"2pow53",9007199254740992.0},
                    {"positive-zero",0.0}, {"negative-zero",BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000UL))},
                    {"small-finite",1e-200}, {"large-finite",1e200}, {"maximum-finite",Double.MaxValue},
                    {"dot-text","1.2345678901234567"}, {"comma-text","1,2345678901234567"},
                    {"exponent-text","1.2345678901234567e-100"}, {"bad-text","bad"},
                    {"blank",null}, {"error",ExcelError.ExcelErrorNA}
                };
                foreach(var pair in responses) {
                    var response=pair.Value;
                    Capture("nested/"+(culture==""?"Invariant":culture)+"/"+pair.Key,Cell(response),()=>Nested(response,culture));
                }
                foreach(var pair in new Dictionary<string,object> { {"missing",ExcelMissing.Value},{"empty",ExcelEmpty.Value},{"nothing",null},
                    {"error",ExcelError.ExcelErrorDiv0},{"bool",true},{"numeric",0.75},{"text","0.75"},{"date",new DateTime(2026,1,2)},
                    {"nan",Double.NaN},{"infinity",Double.PositiveInfinity},{"large",1e20} }) {
                    var input=pair.Value;
                    Capture("numeric/"+(culture==""?"Invariant":culture)+"/"+pair.Key,Cell(input),()=>Cell(Invoke("WorksheetFunctions.ExcelArgNumeric","TryGetDouble",input)));
                    Capture("text/"+(culture==""?"Invariant":culture)+"/"+pair.Key,Cell(input),()=>Invoke("WorksheetFunctions.ExcelArgReaders","CellToTrimmedText",input));
                }
            }
            foreach(var lower in new[]{0,1,-3}) {
                Array array=Array.CreateInstance(typeof(object),new[]{2,2},new[]{lower,lower+2});
                array.SetValue(1.25,lower,lower+2);array.SetValue(2.5,lower,lower+3);
                array.SetValue(3.75,lower+1,lower+2);array.SetValue(5.0,lower+1,lower+3);
                Capture("bounds/"+lower,new{lower=lower,otherLower=lower+2},()=> {
                    var result=(object[,])Invoke("WindowsSpreadsheetTransport","CopyValues",array);
                    return new{lowerRow=result.GetLowerBound(0),lowerColumn=result.GetLowerBound(1),cells=result.Cast<object>().Select(Cell).ToArray()};
                });
            }
            if(File.Exists(args[0]))throw new IOException("Append-only fixture destination exists");
            File.WriteAllText(args[0],new JavaScriptSerializer {MaxJsonLength=int.MaxValue}.Serialize(new {
                kind="04A39_CURRENT_PRE_CHANGE",reference="ACCEPTED_CURRENT_W3_BINARY_NOT_HISTORICAL_ORIGINAL",rows=Rows}),new System.Text.UTF8Encoding(false));
            Console.WriteLine("Captured "+Rows.Count+" distinct observation cases");return 0;
        } finally {Thread.CurrentThread.CurrentCulture=prior;}
    }
}
