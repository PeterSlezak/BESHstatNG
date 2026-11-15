// Executed current-main compatibility probes; no Excel/native UI or independent scientific oracle.
using System;using System.Collections;using System.Collections.Generic;using System.Globalization;
using System.IO;using System.Linq;using System.Reflection;using System.Web.Script.Serialization;
class Observe42 {
 static List<object> Rows=new List<object>();static Assembly Host;static Dictionary<string,object> Handles=new Dictionary<string,object>();static string Profile;
 static object V(object x){if(x==null)return new{type="Nothing"};Type t=x.GetType();if(x is double)return new{type="Double",bits=BitConverter.DoubleToInt64Bits((double)x).ToString("x16")};if(x is Array){Array a=(Array)x;return new{type=t.FullName,bounds=Enumerable.Range(0,a.Rank).Select(i=>new[]{a.GetLowerBound(i),a.GetLength(i)}).ToArray(),values=a.Cast<object>().Select(V).ToArray()};}return new{type=t.FullName,value=Convert.ToString(x,CultureInfo.InvariantCulture)};}
 static object[,] Col(params object[] a){var x=new object[a.Length,1];for(int i=0;i<a.Length;i++)x[i,0]=a[i];return x;}
 static object[,] Vector(string n){return Col(Enumerable.Range(0,24).Select(i=>n=="groups"?(object)(i<12?"B":"A"):n=="id"?(object)("row"+(90-i)):n=="status"||n=="treatment"||n=="y"?(object)(double)(i%2):n=="probabilities"||n=="existingScore"?(object)(.15+.03*i):n=="weights"?(object)(i%5==0?0.0:1.0):(object)(i*.7+Math.Sin(i)+1)).ToArray());}
 static object[,] Data(bool category=false){var a=new object[24,3];for(int i=0;i<24;i++)for(int j=0;j<3;j++)a[i,j]=category?(object)("level"+((i/(j+1)+j)%3)):(object)(Math.Sin(i*(j+1)*.7)+i*.2+(j+1)*Math.Cos(i*.3+j));return a;}
 static string Family(MethodInfo m){return m.Name.StartsWith("PS")?"PS":m.Name.Split('_')[0];}
 static object[] Args(MethodInfo m){return m.GetParameters().Select(p=>Default(m,p)).ToArray();}
 static object Default(MethodInfo m,ParameterInfo p){string n=p.Name;bool fit=m.Name.EndsWith("_FIT");
  if(n=="handle")return Handles.ContainsKey(Family(m))?Handles[Family(m)]:"MISSING";
  if(n=="randomSeed")return 173;
  if(n=="includeHeader")return true;
  if(n=="x")return Data(m.Name=="MCA_FIT");
  if(n=="newX"||n=="covariates"||n=="data")return Data();
  if(n=="groups"||n=="id"||n=="status"||n=="treatment"||n=="y"||n=="probabilities"||n=="existingScore"||n=="outcome"||n=="marker"||n=="time")return Vector(n);
  if(n=="table")return fit?(object)new object[,]{{8.0,5.0,4.0},{4.0,9.0,3.0},{5.0,2.0,7.0}}:"scores";
  if(n=="retentionMethod")return "fixed";if(n=="retentionValue")return 2.0;
  if(n=="extractionMethod")return "principalcomponents";if(n=="rotationMethod")return "none";
  if(n=="scoreMethod"&&m.Name=="PS_FIT")return "existing";
  if(n=="matchingOptions")return "seed=173;caliperscale=none";
  if(n=="numberOfClusters")return 3;
  if(n=="varNames"&&fit&&m.Name!="MCA_FIT")return "x1,x2,x3";
  if(n=="hasHeader")return false;
  return null;
 }
 static object Invoke(MethodInfo m,object[] a){try{return m.Invoke(null,a);}catch(TargetInvocationException e){throw e.InnerException;}}
 static object Add(MethodInfo m,string tag,object[] a){object input=V(a);try{var x=Invoke(m,a);Rows.Add(new{id=CultureInfo.CurrentCulture.Name+"/"+Profile+"/"+m.Name+"/"+tag,method=m.DeclaringType.FullName+"."+m.Name,input=input,outcome="VALUE",output=V(x),exception=(object)null});return x;}catch(Exception e){Rows.Add(new{id=CultureInfo.CurrentCulture.Name+"/"+Profile+"/"+m.Name+"/"+tag,method=m.DeclaringType.FullName+"."+m.Name,input=input,outcome="EXCEPTION",output=(object)null,exception=new{type=e.GetType().FullName,message=e.Message}});return null;}}
 static void Change(MethodInfo m,object[] a,string name,object value){int i=Array.FindIndex(m.GetParameters(),p=>p.Name==name);if(i>=0)a[i]=value;}
 static void ProfileArgs(MethodInfo m,object[] a){for(int k=0;k<a.Length;k++){var b=a[k] as object[,];if(b==null)continue;if(Profile=="reverse"&&b.GetLength(0)==24){var c=(object[,])b.Clone();for(int i=0;i<24;i++)for(int j=0;j<b.GetLength(1);j++)c[i,j]=b[23-i,j];a[k]=c;}if(Profile=="filtered"&&b.GetLength(0)==24&&new[]{"x","newX","covariates","marker"}.Contains(m.GetParameters()[k].Name))b[3,0]=null;}
  if(Profile=="duplicates"){Change(m,a,"varNames","2.000000000000001,dup,dup");Change(m,a,"rowLabels",Col(Enumerable.Range(0,24).Select(i=>(object)(i%3==0?"dup":"id"+(90-i))).ToArray()));}
 }
 static void Main(string[] argv){Host=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"BESHStatNG.dll"));var modules=new[]{"MultivariateAnalysisUDFs","ClassificationReportUDFs","PropensityScoreUDFs","PlotDataUDFs"};var methods=modules.SelectMany(n=>Host.GetType("BESHStatNG.WorksheetFunctions."+n,true).GetMethods(BindingFlags.Public|BindingFlags.Static)).Where(m=>m.GetCustomAttributes(false).Any(a=>a.GetType().Name=="ExcelFunctionAttribute")).OrderBy(m=>m.Name).ToArray();
  foreach(string culture in new[]{"","sk-SK"})foreach(string profile in new[]{"ordinary","reverse","filtered","duplicates"}){CultureInfo.CurrentCulture=CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);Profile=profile;Handles.Clear();
   foreach(var m in methods.Where(m=>m.Name.EndsWith("_FIT"))){var a=Args(m);ProfileArgs(m,a);Handles[Family(m)]=Add(m,"fit",a);}
   foreach(var m in methods.Where(m=>!m.Name.EndsWith("_FIT")&&!m.Name.EndsWith("_DROP")&&m.Name!="PS_CLEANUP")){var a=Args(m);ProfileArgs(m,a);var first=Add(m,"ordinary",a);a=Args(m);ProfileArgs(m,a);Change(m,a,"includeHeader",false);Add(m,"no-header",a);
    if(profile=="ordinary"){
     a=Args(m);a[0]="invalid";Add(m,"invalid-first",a);
     a=Args(m);var ar=a[0] as Array;if(ar!=null&&ar.Rank==2){var low=Array.CreateInstance(typeof(object),new[]{ar.GetLength(0),ar.GetLength(1)},new[]{3,2});for(int i=0;i<ar.GetLength(0);i++)for(int j=0;j<ar.GetLength(1);j++)low.SetValue(ar.GetValue(i,j),i+3,j+2);a[0]=low;Add(m,"nonzero-bounds",a);}
     a=Args(m);Change(m,a,"weights",Vector("weights"));Change(m,a,"threshold",.5);Change(m,a,"thresholds",Col(.5,.5,0.0,1.0));Change(m,a,"direction","lower");Change(m,a,"source","validation");Add(m,"weighted-ties-alternative",a);
     var table=first as object[,];if(table!=null&&table.Length>0){table[0,0]="CALLER_MUTATION";Add(m,"repeat-after-caller-mutation",Args(m));}
    }
   }
   if(profile=="ordinary")foreach(string key in new[]{"summary","options","import","rowaudit","matcheddata","matches","weights","balance","effect","loveplot","subclass","overlapbins","warnings","unknown"}){var m=methods.First(t=>t.Name=="PS_TABLE");var a=Args(m);Change(m,a,"table",key);Add(m,"table-"+key,a);}
   foreach(var m in methods.Where(m=>m.Name.EndsWith("_DROP")||m.Name=="PS_CLEANUP"))Add(m,"drop",Args(m));
  }
  var s=new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=200};File.WriteAllText(argv[0],s.Serialize(new{label="CURRENT_MAIN_PRE_EDIT42;NOT_GOLDEN",seed=173,rows=Rows}),new System.Text.UTF8Encoding(false));Console.WriteLine("Materialized42 cases "+Rows.Count);
 }
}
