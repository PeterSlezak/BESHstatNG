using System;
using System.Globalization;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualBasic.CompilerServices;
public static class CulturePolicyProbeV2 {
 static string Hex(string s){return s==null?"NULL":BitConverter.ToString(System.Text.Encoding.Unicode.GetBytes(s));}
 public static int Main(){
  string[] values={null,"","MEAN","mean","ｍｅａｎ","ML","ＭＬ","mL","VARIMAX","varimax","VARIＭAX","I","i","ı","İ","a","A","á","a\u0301","\u00ad","\u0000","CORR","cov","NotAnAlias","ｶ","カ"};
  int pairs=0,differences=0,nonnull=0;var prior=CultureInfo.CurrentCulture;var ui=CultureInfo.CurrentUICulture;
  try {foreach(string culture in new[]{"","sk-SK"}){
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
   foreach(string a in values)foreach(string b in values){bool runtime=Operators.CompareString(a,b,true)==0;bool portable=LegacyUdfText38.EqualText(a,b);pairs++;
    if(runtime!=portable){differences++;if(a!=null&&b!=null)nonnull++;Console.WriteLine("DIFF culture="+(culture==""?"Invariant":culture)+" left_utf16="+Hex(a)+" right_utf16="+Hex(b)+" runtime="+runtime+" portable="+portable);}
   }
  }}finally{CultureInfo.CurrentCulture=prior;CultureInfo.CurrentUICulture=ui;}
  Console.WriteLine("CURRENT_RUNTIME_POLICY_CONTROL pairs="+pairs+" differences="+differences+" nonnull_differences="+nonnull+" original_BESH_UDF_queries=0");return differences==0?0:1;
 }
}
