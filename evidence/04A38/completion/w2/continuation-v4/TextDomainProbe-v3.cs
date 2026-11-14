using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualBasic.CompilerServices;
public static class TextDomainProbe {
 static string Hash(byte[] bytes){using(var h=SHA256.Create())return String.Concat(h.ComputeHash(bytes).Select(b=>b.ToString("x2")));}
 static string Hex(string value){return value==null?"NULL":BitConverter.ToString(Encoding.Unicode.GetBytes(value));}
 public static int Main(string[] args){try{
  string input=args[0],output=args[1];if(File.Exists(output))throw new Exception("Append-only output exists");
  var serializer=new JavaScriptSerializer{MaxJsonLength=Int32.MaxValue};var doc=serializer.Deserialize<Dictionary<string,object>>(File.ReadAllText(input));var calls=((System.Collections.IEnumerable)doc["calls"]).Cast<Dictionary<string,object>>().ToArray();
  string[] oldValues={null,"","MEAN","mean","ｍｅａｎ","ML","ＭＬ","mL","VARIMAX","varimax","VARIＭAX","I","i","ı","İ","a","A","á","a\u0301","\u00ad","\u0000","CORR","cov","NotAnAlias","ｶ","カ"};
  var values=new HashSet<string>(oldValues,StringComparer.Ordinal);foreach(var s in new[]{"ž","Ž","ľ","Ľ"," ","\t","x\u00ad","\u00adx","x\0","\0x"," \u00ad "," \0 "})values.Add(s);
  foreach(var call in calls){var choices=((System.Collections.IEnumerable)call["choices"]).Cast<object>().ToArray();foreach(string value in choices)values.Add(value);if((string)call["kind"]=="EqualText")values.Add((string)call["right_literal"]);}
  foreach(var s in values.Where(s=>s!=null).ToArray()){values.Add(s.ToLowerInvariant());values.Add(s+"\u00ad");values.Add("\u00ad"+s);values.Add(s+"\0");values.Add(" "+s+" ");}
  var ordered=values.OrderBy(s=>s,StringComparer.Ordinal).ToArray();var results=new List<object>();var synthetic=new List<object>();int failures=0;
  var before=CultureInfo.CurrentCulture;var beforeUi=CultureInfo.CurrentUICulture;
  try{foreach(string culture in new[]{"","sk-SK"}){
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
   int rawPairs=0,rawDiff=0,nonnullDiff=0;var rawMismatches=new List<object>();
   foreach(string a in oldValues)foreach(string b in oldValues){rawPairs++;bool native=Operators.CompareString(a,b,true)==0,portable=LegacyUdfText38.EqualText(a,b);if(native!=portable){rawDiff++;if(a!=null&&b!=null)nonnullDiff++;rawMismatches.Add(new{left_utf16=Hex(a),right_utf16=Hex(b),native,portable});}}
   synthetic.Add(new{culture=culture==""?"Invariant":culture,pairs=rawPairs,differences=rawDiff,nonnull_differences=nonnullDiff,mismatches=rawMismatches,scope="UNREACHABLE_SYNTHETIC_NULL_IGNORABLE_DOMAIN_RETAINED_AS_NONPASS"});
   foreach(var call in calls){int checkedValues=0,diff=0;var bad=new List<object>();string kind=(string)call["kind"];var choices=(((System.Collections.IEnumerable)call["choices"]).Cast<object>().ToArray()).Cast<string>().ToArray();
    foreach(string value in ordered){checkedValues++;
     if(kind=="EqualText"){
      string right=(string)call["right_literal"];bool native=Operators.CompareString(value,right,true)==0,portable=LegacyUdfText38.EqualText(value,right);
      if(native!=portable){diff++;bad.Add(new{left_utf16=Hex(value),right_utf16=Hex(right),native,portable});}
     }else{
      string native=value;foreach(string choice in choices)if(Operators.CompareString(value,choice,true)==0){native=choice;break;}
      string portable=LegacyUdfText38.CaseToken(value,choices);if(!Object.Equals(native,portable)){diff++;bad.Add(new{left_utf16=Hex(value),native_utf16=Hex(native),portable_utf16=Hex(portable)});}
     }
    }
    failures+=diff;results.Add(new{culture=culture==""?"Invariant":culture,site_id=call["site_id"],kind,checked_values=checkedValues,differences=diff,mismatches=bad});
   }
   // These are current-only policy checks; no BESH original UDF is invoked.
   if(UdfScalarUtilitiesOptions38.NormalizeText(null)!=String.Empty||UdfScalarUtilitiesOptions38.NormalizeToken(null)!=String.Empty)throw new Exception("Null normalizer domain changed");
   if(UdfScalarUtilitiesOptions38.NormalizeText("\u00ad")!="\u00ad"||UdfScalarUtilitiesOptions38.NormalizeToken("\0")!="\0")throw new Exception("Ignorables were incorrectly claimed removed");
   if(UdfScalarUtilitiesOptions38.NormalizeMixedModelOptionToken("\u00ad\0")!=String.Empty)throw new Exception("Mixed-option filtering changed");
  }}finally{CultureInfo.CurrentCulture=before;CultureInfo.CurrentUICulture=beforeUi;}
  var operation=typeof(Operators).GetMethod("CompareString");var il=operation.GetMethodBody().GetILAsByteArray();
  var result=new{status=failures==0?"ALL_CURRENT_CALLER_DOMAINS_MATCH_FRAMEWORK_TEXT_OPERATION":"CURRENT_CALLER_DOMAIN_FAIL",original_BESH_queries=0,source_table_sha256=Hash(File.ReadAllBytes(input)),values_per_site=ordered.Length,direct_sites=calls.Count(c=>(string)c["kind"]=="EqualText"),direct_source_lines=calls.Where(c=>(string)c["kind"]=="EqualText").Select(c=>(string)c["line_site_id"]).Distinct().Count(),CaseToken_sites=calls.Count(c=>(string)c["kind"]=="CaseToken"),CaseToken_choice_occurrences=calls.Sum(c=>((System.Collections.IEnumerable)c["choices"]).Cast<object>().Count()),values_utf16=ordered.Select(Hex).ToArray(),cultures=2,total_site_value_observations=results.Count*ordered.Length,source_domain_differences=failures,synthetic,results,runtime=Environment.Version.ToString(),comparison_operation=operation.DeclaringType.FullName+"."+operation.Name,comparison_assembly=operation.DeclaringType.Assembly.FullName,comparison_IL_sha256=Hash(il),scope="Standard Invariant/sk-SK host cultures; no arbitrary throwing CultureInfo-subclass equivalence claim"};
  File.WriteAllText(output,serializer.Serialize(result),new UTF8Encoding(false));Console.WriteLine(result.status+" observations="+result.total_site_value_observations+" source_differences="+failures+";original_BESH_queries=0");return failures==0?0:1;
 }catch(Exception e){Console.Error.WriteLine(e.GetType().FullName+":"+e.Message);return 1;}}
}
