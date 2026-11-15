// Host-free current-main compatibility observations. No Excel, COM or GUI activation.
using System;using System.Collections;using System.Collections.Generic;using System.Globalization;
using System.IO;using System.Linq;using System.Reflection;using System.Web.Script.Serialization;
class Observe41 {
 static readonly List<object> Rows=new List<object>();static Assembly Host;
 static object V(object x){
  if(x==null)return new{type="Nothing"};Type t=x.GetType();
  if(x is double)return new{type="Double",bits=BitConverter.DoubleToInt64Bits((double)x).ToString("x16")};
  if(x is Array){Array a=(Array)x;return new{type=t.FullName,bounds=Enumerable.Range(0,a.Rank).Select(i=>new[]{a.GetLowerBound(i),a.GetLength(i)}).ToArray(),values=a.Cast<object>().Select(V).ToArray()};}
  if(t.IsEnum||x is string||x is bool||x is int||x is long||x is decimal)return new{type=t.FullName,value=Convert.ToString(x,CultureInfo.InvariantCulture)};
  return new{type=t.FullName,value=Convert.ToString(x,CultureInfo.InvariantCulture)};
 }
 static object[,] Col(params object[] a){var x=new object[a.Length,1];for(int i=0;i<a.Length;i++)x[i,0]=a[i];return x;}
 static object[,] Sequence(int n,bool constant=false,bool huge=false){var x=new object[n,1];for(int i=0;i<n;i++)x[i,0]=constant?4.0:huge?(i+1)*1e150:(double)i+Math.Sin(i);return x;}
 static object[,] Groups(){var x=new object[12,3];for(int i=0;i<12;i++)for(int j=0;j<3;j++)x[i,j]=(double)(i+j*2)+Math.Sin(i+j*.7);return x;}
 static object[,] Nested(){var x=new object[12,3];for(int i=0;i<12;i++){x[i,0]=i<6?"B":"A";x[i,1]=(i/3).ToString(CultureInfo.InvariantCulture);x[i,2]=i*.7+Math.Sin(i);}return x;}
 static object Default(MethodInfo m,ParameterInfo p){string n=p.Name.ToLowerInvariant();bool sample=m.DeclaringType.Name=="SampleSizeUDFs";
  if(n=="randomseed")return 173;
  if(n=="alpha")return .05;
  if(n=="beta")return .2;
  if(p.IsOptional)return null;
  switch(n){
   case "side":return "two";
   case "x":case "xrange":case "control":case "group1":return Col(1.0,2.0,2.0,4.0,5.0,6.0,7.0,8.0,9.0,10.0,12.0,14.0);
   case "y":case "yrange":case "experimental":case "group2":return Col(1.2,2.4,3.1,4.5,5.2,6.5,7.4,8.9,9.1,10.9,12.2,14.5);
   case "data":return m.Name=="ANOVA2_NESTED"?Nested():m.Name.StartsWith("ICC")||m.Name.StartsWith("RM")||m.Name.StartsWith("FRIEDMAN")||m.Name=="MAUCHLY"||m.Name=="BOX_M"?Groups():Sequence(12);
   case "groups":return m.Name=="BOX_M"?Col("A","A","A","A","A","A","B","B","B","B","B","B"):Groups();
   case "table":return new object[,]{{1.0,9.0},{11.0,3.0}};
   case "stackedtables":return new object[,]{{1.0,9.0,11.0,3.0},{2.0,4.0,3.0,8.0}};
   case "rater1":return Col("a","a","a","b","b","b","c","c","c","a","b","c");
   case "rater2":return Col("a","a","b","b","b","c","c","c","a","a","b","c");
   case "responders":case "responders1":case "controlresponders":return 14.0;
   case "responders2":case "experimentalresponders":return 18.0;
   case "totaln":case "totaln1":case "totaln2":case "controltotal":case "experimentaltotal":return 40.0;
   case "respondersonly1":return 4.0;case "respondersonly2":return 7.0;case "respondersboth":return 10.0;
   case "margin":return .2;case "lowermargin":case "lowerallowablelimit":return -.3;case "upperallowablelimit":return 1.5;
   case "meandifference":return 1.0;case "sddifference":case "commonsd":case "covariatesd":return 2.0;
   case "controltoexperimentalratio":return 1.0;
   case "anticipatedproportion":case "experimentalproportion":return .6;
   case "nullproportion":case "controlproportion":return .4;
   case "hazardratio":case "hazardratioperunit":return .7;
   case "controleventproportion":case "experimentaleventproportion":return .5;
   case "nullicc":return .2;case "alternativeicc":return .6;case "observationspersubject":return 3.0;
   case "desiredhalfwidth":return .5;
  }
  return sample?(object)1.0:null;
 }
 static void Add(MethodInfo m,string id,object[] a){
  var materialized=a.Select(V).ToArray();try{
   object value=m.Invoke(null,a);Rows.Add(new{id=id,method=m.DeclaringType.FullName+"."+m.Name,culture=CultureInfo.CurrentCulture.Name,input=materialized,outcome="VALUE",output=V(value),exception=(object)null});
  }catch(Exception e){if(e is TargetInvocationException)e=e.InnerException;Rows.Add(new{id=id,method=m.DeclaringType.FullName+"."+m.Name,culture=CultureInfo.CurrentCulture.Name,input=materialized,outcome="EXCEPTION",output=(object)null,exception=new{type=e.GetType().FullName,message=e.Message}});}
 }
 static object[] Args(MethodInfo m){return m.GetParameters().Select(p=>Default(m,p)).ToArray();}
 static void Special(string module,string method,string tag,Action<object[]> mutate){var m=Host.GetType("BESHStatNG.WorksheetFunctions."+module,true).GetMethod(method);var a=Args(m);mutate(a);Add(m,CultureInfo.CurrentCulture.Name+"/"+module+"/"+method+"/"+tag,a);}
 static void Main(string[] args){Host=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"BESHStatNG.dll"));
  foreach(string culture in new[]{"","sk-SK"}){
   CultureInfo.CurrentCulture=CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
   foreach(string module in new[]{"ParametricUDFs","NonparametricUDFs","AssumptionsUDFs","ContingencyTableUDFs","AgreementMethodUDFs","SampleSizeUDFs"}){
    foreach(var m in Host.GetType("BESHStatNG.WorksheetFunctions."+module,true).GetMethods(BindingFlags.Public|BindingFlags.Static).Where(m=>m.GetCustomAttributes(false).Any(a=>a.GetType().Name=="ExcelFunctionAttribute")).OrderBy(m=>m.Name)){
     var a=Args(m);Add(m,culture+"/"+module+"/"+m.Name+"/ordinary",a);
     a=Args(m);a[0]="invalid";Add(m,culture+"/"+module+"/"+m.Name+"/invalid-first",a);
    }
   }
   foreach(int n in new[]{0,1,2,3,8,9,4999,5000,5001})Special("AssumptionsUDFs","SHAPIRO_WILK","n"+n,a=>a[0]=Sequence(n));
   foreach(string method in new[]{"SHAPIRO_WILK","DAGOSTINO_PEARSON","ANDERSON_DARLING"}){
    Special("AssumptionsUDFs",method,"constant",a=>a[0]=Sequence(12,true));Special("AssumptionsUDFs",method,"huge",a=>a[0]=Sequence(12,false,true));
   }
   foreach(string method in new[]{"FLIGNER_KILLEEN","LEVENE","BARTLETT","SQUARED_RANKS"})Special("AssumptionsUDFs",method,"empty-group",a=>a[0]=new object[,]{{null,1.0},{null,2.0},{null,3.0}});
   Special("ParametricUDFs","ANOVA2_NESTED","duplicate-labels",a=>a[1]="Factor,Factor,Response");
   foreach(string option in new[]{"main","sw","invalid"})Special("ParametricUDFs","ANOVA2_NESTED","output-"+option,a=>a[2]=option);
   foreach(string option in new[]{"none","gg","hf","both","invalid"})Special("ParametricUDFs","RMANOVA1","correction-"+option,a=>a[2]=option);
   foreach(string option in new[]{"two","lower","upper","invalid"})Special("NonparametricUDFs","MW_P_EXACT","side-"+option,a=>a[2]=option);
   foreach(string method in new[]{"CT_FISHER_2X2","CT_MCNEMAR_EXACT","CT_RISK_RATIO","CT_ODDS_RATIO"}){
    Special("ContingencyTableUDFs",method,"zero-margin",a=>a[0]=new object[,]{{0.0,0.0},{3.0,4.0}});
    Special("ContingencyTableUDFs",method,"transposed",a=>a[0]=new object[,]{{1.0,11.0},{9.0,3.0}});
   }
   Special("SampleSizeUDFs","SSIZE_ICC","infinite-count",a=>a[2]=double.PositiveInfinity);
   foreach(string method in new[]{"TTEST_PAIRED"}){
    Special("ParametricUDFs",method,"paired-omissions",a=>{a[0]=Col("Before",1.0,null,3.0,4.0);a[1]=Col("After",2.0,3.0,4.0,6.0);});
    Special("ParametricUDFs",method,"nonzero-bounds",a=>{Array x=Array.CreateInstance(typeof(object),new[]{3,1},new[]{5,7});x.SetValue(1.0,5,7);x.SetValue(2.0,6,7);x.SetValue(4.0,7,7);a[0]=x;a[1]=x;});
   }
  }
  var serializer=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};File.WriteAllText(args[0],serializer.Serialize(new{kind="CURRENT_MAIN_PRE_EDIT41_OR_EXACT_SUCCESSOR_COMPATIBILITY;NOT_SCIENTIFIC_ORIGINAL",seed=173,rows=Rows}));
 }
}
