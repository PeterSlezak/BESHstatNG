using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
public static class CapturePresentation36 {
 static object P(object v,string n){return v.GetType().GetProperty(n).GetValue(v,null);}
 static string Q(object v){return "\""+Convert.ToString(v,CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\"";}
 public static void Run(string root,string label,string config,string output,bool post){
  string folder=Path.Combine(root,".test-rationalization-04A36.tmp/builds/"+label+"/"+config+"/legacy");
  Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Core.dll"));var common=Assembly.LoadFrom(Path.Combine(folder,"BESHStat.Excel.Common.dll"));
  var legacy=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A36.tmp/Legacy36.dll"));
  var fixture=Assembly.LoadFrom(Path.Combine(root,".test-rationalization-04A36.tmp/LegacyPresentation36.dll"));var original=fixture.GetType("Legacy36.OriginalPresentation36",true);var snapshot=legacy.GetType("Legacy36.Cases36",true).GetMethod("Snapshot");
  var rows=new List<string>{"culture,case,output,exception"};
  foreach(var culture in new[]{"","sk-SK"}){CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);
   for(int i=0;i<192;i++){
    object result=null;string error="None";
    try{
     var panel=original.GetMethod("Panel").Invoke(null,new object[]{i});var spec=original.GetMethod("Specifications").Invoke(null,new object[]{i});
     var a=new object[]{panel,spec,i%4,(i&4)!=0,(i&8)!=0,(i&16)!=0,(i&32)!=0,(i&64)!=0};
     if(!post)result=original.GetMethod("Series").Invoke(null,a);
     else{var plan=Activator.CreateInstance(common.GetType("BESHStatNG.ExcelCommon.SpcPanelPresentation36",true),a);result=((IEnumerable)P(plan,"Series")).Cast<object>().Select(v=>(object)new object[]{P(v,"Role").ToString(),P(v,"Name"),((double?[])P(v,"Values")).Select(x=>x.HasValue?(object)x.Value:null).ToArray(),P(v,"LastValueOrdinal"),P(v,"ShowLimitLabel"),P(v,"HideLegend")}).ToArray();}
    }catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;var a=e as ArgumentException;error=e.GetType().FullName+":"+(a==null?"":a.ParamName);}
    rows.Add(String.Join(",",new object[]{culture,i,snapshot.Invoke(null,new object[]{result}),error}.Select(Q)));
   }
  }
  var path=Path.Combine(root,output);if(File.Exists(path))throw new Exception("Frozen output exists");File.WriteAllLines(path,rows,new System.Text.UTF8Encoding(false));Console.WriteLine("Captured "+(rows.Count-1)+" ordered SPC intent rows; "+config+"; NOT_GOLDEN");
 }
}
