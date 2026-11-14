using System;
using System.Globalization;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualBasic.CompilerServices;
public static class CulturePolicyProbe {
 public static int Main() {
  string[] values={null,"","MEAN","mean","ｍｅａｎ","ML","ＭＬ","mL","VARIMAX","varimax","VARIＭAX","I","i","ı","İ","a","A","á","a\u0301","\u00ad","\u0000","CORR","cov","NotAnAlias","ｶ","カ"};
  int pairs=0;var prior=CultureInfo.CurrentCulture;var ui=CultureInfo.CurrentUICulture;
  try { foreach(string culture in new[]{"","sk-SK"}) {
   CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
   foreach(string a in values) foreach(string b in values) {
    bool runtime=Operators.CompareString(a,b,true)==0;bool portable=LegacyUdfText38.EqualText(a,b);
    if(runtime!=portable)throw new Exception("Text policy mismatch");pairs++;
   }
   string[] aliases={"","CORRELATION","CORR","COVARIANCE","COV","RAW","ORIGINAL"};
   foreach(string value in values){string expected=value;
    foreach(string alias in aliases)if(Operators.CompareString(value,alias,true)==0){expected=alias;break;}
    if(!Object.Equals(expected,LegacyUdfText38.CaseToken(value,aliases)))throw new Exception("Case token order/null mismatch");
   }
  }} finally {CultureInfo.CurrentCulture=prior;CultureInfo.CurrentUICulture=ui;}
  Console.WriteLine("CURRENT_RUNTIME_POLICY_CONTROL_PASS pairs="+pairs+" cultures=2 original_BESH_UDF_calls=0");return 0;
 }
}
