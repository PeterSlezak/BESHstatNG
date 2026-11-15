// Current-main observations, not historical original references. No Excel/UI/COM calls.
using System;using System.Collections;using System.Collections.Generic;using System.Globalization;
using System.IO;using System.Linq;using System.Reflection;using System.Web.Script.Serialization;
using BESHStatNG;using BESHStatNG.regression;using BESHStatNG.survival;using BESHStatNG.ExcelCommon;
class Observe40 {
 static List<object> rows=new List<object>();static bool policy;
 static string Bits(double x){return BitConverter.DoubleToInt64Bits(x).ToString("x16");}
 static object Value(object x){if(x==null)return null;if(x is double)return new{type="Double",bits=Bits((double)x)};
   if(x is Array){var a=(Array)x;return new{type=x.GetType().FullName,bounds=Enumerable.Range(0,a.Rank).Select(i=>new[]{a.GetLowerBound(i),a.GetLength(i)}).ToArray(),values=a.Cast<object>().Select(Value).ToArray()};}
   if(x is string||x is bool||x is int||x is long||x.GetType().IsEnum)return Convert.ToString(x,CultureInfo.InvariantCulture);return null;}
 static object State(object model){var d=new SortedDictionary<string,object>();for(Type t=model.GetType();t!=null;t=t.BaseType)
   foreach(var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)){
    var v=f.GetValue(model);if(v==null||v is double||v is int||v is bool||v is string||v is Array||f.FieldType.IsEnum)d[t.Name+"."+f.Name]=Value(v);
   }return d;}
 static object Call(object target,string name,params object[] args){var m=target.GetType().GetMethods().Single(x=>x.Name.Equals(name,StringComparison.OrdinalIgnoreCase)&&x.GetParameters().Length>=args.Length&&x.GetParameters().Skip(args.Length).All(p=>p.IsOptional));
   var p=m.GetParameters();var a=Enumerable.Range(0,p.Length).Select(i=>i<args.Length?args[i]:Type.Missing).ToArray();try{return m.Invoke(target,a);}catch(TargetInvocationException e){throw e.InnerException;}}
 static object Shared(string cls,string name,params object[] a){var m=typeof(RegressionOptions).Assembly.GetType("BESHStatNG.ExcelCommon."+cls,true).GetMethods().Single(x=>x.Name==name);try{return m.Invoke(null,a);}catch(TargetInvocationException e){throw e.InnerException;}}
 static void Add(string id,object input,Func<object> fn){try{rows.Add(new{id,input,outcome="VALUE",output=fn(),exception=(string)null});}catch(Exception e){rows.Add(new{id,input,outcome="EXCEPTION",output=(object)null,exception=e.GetType().FullName+":"+e.Message});}}
 static RegressionOptions Options(double alpha,int iter,double eps,bool residual=false){return new RegressionOptions{Alpha=alpha,MaximumIterations=iter,Epsilon=eps,Residuals=residual,Covariance=false,IterationDetails=false,EmIterations=40};}
 static object Configure(string family,RegressionOptions o){
   if(family=="LM"){var m=new LinearModel();if(policy)Shared("LinearBinomialRegressionPolicy","ConfigureUdfFit",m,o);else{m.Alpha=o.Alpha;m.bComputeResiduals=o.Residuals;m.bReturnCov=o.Covariance;}return State(m);}
   if(family=="GLM"||family=="NB2"){GLM m=family=="GLM"?new GLM(FamilyUtils.createFamily("Gaussian"),LinkUtils.createLink("Identity")):new GLM_NB(LinkUtils.createLink("Log"));
    if(policy)Shared("RegressionRequestPolicy","ConfigureUdfGlmFit",m,o);else{m.bComputeResiduals=o.Residuals;m.bReturnCov=o.Covariance;m.bIterationDetails=o.IterationDetails;m.settingInputs(o.Alpha,o.MaximumIterations,o.Epsilon);}return State(m);}
   if(family=="ORDINAL"){var m=new OrdinalLogitModel();if(policy)Shared("ExtendedRegressionFamilyPolicy","ConfigureUdfOrdinalFit",m,o);else{m.bComputeResiduals=o.Residuals;m.bReturnCov=o.Covariance;m.bIterationDetails=o.IterationDetails;m.SettingInputs(o.Alpha,o.MaximumIterations,o.Epsilon);}return State(m);}
   if(family=="MULTINOMIAL"){var m=new MultinomialLogitModel();if(policy)Shared("ExtendedRegressionFamilyPolicy","ConfigureUdfMultinomialFit",m,o);else{m.bComputeResiduals=o.Residuals;m.bReturnCov=o.Covariance;m.bIterationDetails=o.IterationDetails;m.settingInputs(o.Alpha,o.MaximumIterations,o.Epsilon);}return State(m);}
   if(family=="ZIP"){var m=new ZeroInflatedPoisson();if(policy)Shared("ExtendedRegressionFamilyPolicy","ApplyZipFitSettings",m,o);else m.settingInputs(o.Alpha,o.MaximumIterations,o.EmIterations,o.Epsilon);return State(m);}
   if(family=="GEE"){var m=new GEE(FamilyUtils.createFamily("Gaussian"),LinkUtils.createLink("Identity"),GEEcovStructUtils.createGEEcovMat("Independence"),"Robust");var g=new GeeWorkflowOptions{UseP=true};
    if(policy)Shared("GeeWorkflowPolicy","ConfigureUdfFit",m,o,g);else{m.bComputeResiduals=o.Residuals;m.bIterationDetails=o.IterationDetails;m.settingInputs(o.Alpha,o.MaximumIterations,o.Epsilon,g.UseP);}return State(m);}
   throw new Exception("unknown family");
 }
 static double[,] Data(){return new double[,]{{1,0},{2.1,1},{2.8,2},{4.2,3},{4.8,4},{6.2,5},{7.1,6},{8.3,7}};}
 static object Lm(double alpha){var data=Data();var m=new LinearModel();var o=Options(alpha,20,1e-8,true);
   if(policy)Shared("LinearBinomialRegressionPolicy","ConfigureUdfFit",m,o);else{m.Alpha=alpha;m.bComputeResiduals=true;m.bReturnCov=false;}
   m.Data(data,new[]{"Y","X"},new[]{3,7,11,15,19,23,27,31},new[]{1.0,2,1,3,2,1,2,1});m.Fit();return new{config=State(m),coefficients=Value(m.results.Coeffs_est),se=Value(m.results.Coeffs_SEs),names=m.results.varNames,covariance=Value(m.Covariance),fitted=Value(m.Fitted)};}
 static object Gee(bool start,bool weights){var data=Data();var m=new GEE(FamilyUtils.createFamily("Gaussian"),LinkUtils.createLink("Identity"),GEEcovStructUtils.createGEEcovMat("Independence"),"Robust");var o=Options(.1,30,1e-8,false);var g=new GeeWorkflowOptions{UseP=false};
   if(policy)Shared("GeeWorkflowPolicy","ConfigureUdfFit",m,o,g);else{m.bComputeResiduals=false;m.bIterationDetails=false;m.settingInputs(.1,30,1e-8,false);}
   m.data(data,new object[]{"b","b","a","a","d","d","c","c"},new[]{3,7,11,15,19,23,27,31},null,weights?new[]{1.0,2,1,3,2,1,2,1}:null,new[]{0.0,3,0,7,0,2,0,11});m.setVarNames(new[]{"Y","X"},"Subject",null,weights?"W":null,"Visit");
   if(start)m.startParams=new[]{1.0,1.0};m.Fit(start);return new{state=State(m),coefficients=m.results==null?null:Value(m.results.Coeffs_est),predicted=Value(m.PredictedResponses),observed=Value(m.ObservedResponses)};}
 static object Logrank(bool stratified,bool noevents){var rec=new List<SurvivalRecord>();double[] times={1,1,2,3,4,5,1,2,2,4,5,6};
  for(int i=0;i<times.Length;i++)rec.Add(new SurvivalRecord{Index=i+20,Time=times[i],Censorship=noevents?0:(i==0||i==7?0:1),Group=i<6?1:2,strGroup=i<6?"A":"B",Stratum=stratified?(i%2==0?"S1":"S2"):"",strStratum=stratified?(i%2==0?"S1":"S2"):"",Covariates=new[]{(double)i}});
  var km=new Survival_KM_LR(rec);var result=km.WeightedLogRankTest("logrank");return result==null?null:new{stat=Value(result.TestStatistics1),p=Value(result.Pvalue)};}
 static void Main(string[] args){policy=args.Length>1&&args[1]=="policy";
  foreach(string culture in new[]{"", "sk-SK"}){System.Threading.Thread.CurrentThread.CurrentCulture=culture==""?CultureInfo.InvariantCulture:new CultureInfo(culture);
   foreach(string family in new[]{"LM","GLM","NB2","ZIP","GEE","ORDINAL","MULTINOMIAL"})foreach(double alpha in new[]{.05,.1}){
    string f=family;double a=alpha;Add(culture+"/options/"+f+"/"+Bits(a),new{family=f,alpha=Bits(a),iterations=37,epsilon=Bits(1e-9),residual=f=="ORDINAL"||f=="MULTINOMIAL"},()=>Configure(f,Options(a,37,1e-9,f=="ORDINAL"||f=="MULTINOMIAL")));}
   foreach(double alpha in new[]{.05,.1}){double a=alpha;Add(culture+"/LMfit/"+Bits(a),new{data=Value(Data()),alpha=Bits(a),rowIds=new[]{3,7,11,15,19,23,27,31},weights=new[]{1.0,2,1,3,2,1,2,1}},()=>Lm(a));}
   foreach(bool start in new[]{false,true})foreach(bool weights in new[]{false,true}){bool s=start,w=weights;Add(culture+"/GEEfit/"+s+"/"+w,new{data=Value(Data()),suppliedStart=s,weights=w,clusters=new[]{"b","b","a","a","d","d","c","c"},visits=new[]{0,3,0,7,0,2,0,11}},()=>Gee(s,w));}
   foreach(bool strata in new[]{false,true})foreach(bool none in new[]{false,true}){bool s=strata,n=none;Add(culture+"/logrank/"+s+"/"+n,new{earliestCensorEventTie=true,stratified=s,noEvents=n,times=new[]{1,1,2,3,4,5,1,2,2,4,5,6}},()=>Logrank(s,n));}
  }
  var serializer=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};File.WriteAllText(args[0],serializer.Serialize(new{kind="04A40_CURRENT_MAIN_PRE_CHANGE_OR_SUCCESSOR",rows=rows}));
 }
}
