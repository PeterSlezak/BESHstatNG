using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

// Executes actual snapshotted/current UDF source in an unregistered library.
// No replacement statistic, portable policy or expected result is implemented.
// Only deterministic synthetic INPUTS and declared native test doubles live here.
public static class Capture38 {
 sealed class CaptureLogger : BESHStatNG.AppInfrastructure.IAppLogger {
  public readonly List<object> Errors=new List<object>();
  public void Info(string m){}public void Warn(string m){}public void Debug(string m){}public void Trace(string m){}public void Dispose(){}
  public void WriteError(string m){Errors.Add(new object[]{"message",m});}public void Error(string m){WriteError(m);}
  public void WriteError(Exception e,string m){Errors.Add(new object[]{m,Capture38.Error(e)});}public void Error(Exception e,string m=null){WriteError(e,m);}
 }
 static Assembly Library,Core;static string phase;static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
 static Dictionary<string,object> activeCase;
 static Dictionary<string,MethodInfo> exports;static Dictionary<string,string> seeds=new Dictionary<string,string>();
 static Dictionary<string,string> handleTokens=new Dictionary<string,string>(StringComparer.Ordinal);static int nextHandle;
 static void FixtureClock(object value,HashSet<object> seen){
  if(value==null||!seen.Add(value))return;var type=value.GetType();if(type.Namespace==null||!type.Namespace.StartsWith("BESHStatNG"))return;
  for(var t=type;t!=null;t=t.BaseType)foreach(var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
   if(f.Name=="CompTime"&&f.FieldType==typeof(double)){double v=(double)f.GetValue(value);if(!Double.IsNaN(v)&&!Double.IsInfinity(v))f.SetValue(value,0.0);}
  foreach(var p in type.GetProperties(BindingFlags.Public|BindingFlags.Instance).Where(p=>p.GetIndexParameters().Length==0)){
   bool timing=p.Name=="ExecutionTimeMs"||(type.Name=="MixedModelPerformanceDiagnostics"&&p.Name.EndsWith("TimeMs",StringComparison.Ordinal));
   if(timing&&p.PropertyType==typeof(double)&&p.CanWrite){double v=(double)p.GetValue(value,null);if(!Double.IsNaN(v)&&!Double.IsInfinity(v))p.SetValue(value,0.0,null);}
   if((p.Name=="CreatedUtc"||p.Name=="ExecutionStartedUtc"||p.Name=="ExecutionCompletedUtc")&&p.CanWrite&&p.GetValue(value,null)!=null)p.SetValue(value,new DateTime(2000,1,2,3,4,5,DateTimeKind.Utc),null);
   if(p.Name=="Model"||p.Name=="Result"||p.Name=="PerformanceDiagnostics")FixtureClock(p.GetValue(value,null),seen);
  }
 }
 static void ControlCachedFixtureClocks(){
  var seen=new HashSet<object>();foreach(var type in Library.GetTypes())foreach(var f in type.GetFields(BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)){
   if(!f.Name.EndsWith("Cache",StringComparison.Ordinal)&&!f.Name.EndsWith("cache",StringComparison.Ordinal))continue;
   var values=f.GetValue(null) as IEnumerable;if(values==null)continue;
   foreach(var pair in values){var p=pair.GetType().GetProperty("Value");if(p!=null)FixtureClock(p.GetValue(pair,null),seen);}
  }
 }
 static string T(Dictionary<string,object>d,string key,string fallback=""){return d.ContainsKey(key)?Convert.ToString(d[key],Inv):fallback;}
 static int I(Dictionary<string,object>d,string key,int fallback=0){return d.ContainsKey(key)?Convert.ToInt32(d[key],Inv):fallback;}
 static string Hash(string s){using(var h=SHA256.Create())return String.Concat(h.ComputeHash(Encoding.UTF8.GetBytes(s)).Select(b=>b.ToString("x2")));}
 static string B64(string s){return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));}
 static double D(string s){return s=="NaN"?BitConverter.Int64BitsToDouble(unchecked((long)0x7ff8000000000038)):s=="+Inf"?Double.PositiveInfinity:s=="-Inf"?Double.NegativeInfinity:s=="-0"?BitConverter.Int64BitsToDouble(long.MinValue):Double.Parse(s,Inv);}
 static string Error(Exception e){while(e is TargetInvocationException)e=e.InnerException;var a=e as ArgumentException;return e.GetType().FullName+":"+(a==null?"":a.ParamName)+":"+B64(e.Message);}
 static object Sentinel(string name){var a=AppDomain.CurrentDomain.GetAssemblies().First(x=>x.GetName().Name=="ExcelDna.Integration");var type=a.GetType("ExcelDna.Integration."+name);return type.GetField("Value",BindingFlags.Public|BindingFlags.Static).GetValue(null);}
 static object[,] Col(params object[] values){var a=new object[values.Length,1];for(int i=0;i<values.Length;i++)a[i,0]=values[i];return a;}
 static object[,] Vector(int n,string kind){var a=new object[n,1];for(int i=0;i<n;i++)a[i,0]=kind=="binary"?(object)(i%3==0?1.0:0.0):kind=="count"?(object)(i%7==0?0.0:(double)((i*5)%9)):kind=="status"?(object)(i%4==0?0.0:1.0):kind=="time"?(object)(1.0+(i*7)%31):kind=="probability"?(object)(0.1+0.8*((i*7)%19)/18.0):kind=="group"?(object)("G"+(i%3+1)):kind=="subject"?(object)("S"+(i/4+1)):kind=="visit"?(object)(double)(i%4+1):kind=="id"?(object)(101+i*7):(object)(3.0+0.4*i+Math.Sin((i+1)*0.7));return a;}
 static object[,] Matrix(int n,int p){var a=new object[n,p];for(int i=0;i<n;i++)for(int j=0;j<p;j++)a[i,j]=1.0+0.3*i+Math.Sin((i+1)*(j+1)*0.7)+((i+2*j)%5)*0.11;return a;}
 static object[,] Groups(int n){var a=new object[n,3];for(int i=0;i<n;i++)for(int j=0;j<3;j++)a[i,j]=1.0+j*0.4+i*0.2+Math.Sin((i+1)*(j+1)*0.4);return a;}
 static object[,] Categories(int n,int p){var a=new object[n,p];for(int i=0;i<n;i++)for(int j=0;j<p;j++)a[i,j]="C"+j+":"+(i%(j+2));return a;}
 static object[,] Table(){return new object[,]{{8.0,5.0},{4.0,9.0}};}
 static object[] MixedInputs(bool lmm){
  var m=Library.GetType("BESHStatNG.UdfFixtureInputs38").GetMethod(lmm?"BuildSyntheticLmmInputs":"BuildSyntheticMmrmInputs",BindingFlags.Public|BindingFlags.Static);
  var args=m.GetParameters().Select(p=>p.ParameterType==typeof(bool)?(object)false:null).ToArray();m.Invoke(null,args);return args;
 }
 static string Prefix(string name){if(name.StartsWith("BESH.PS.")||name.StartsWith("BESH.PSM."))return "PSM";var last=name.Split('.').Last();return last.Contains("_")?last.Substring(0,last.IndexOf('_')):last;}
 static string SeedExport(string prefix){return prefix=="PSM"?"BESH.PS.FIT":exports.Keys.Single(k=>k.EndsWith("."+prefix+"_FIT"));}
 static bool BinaryQuery(string name){return Regex.IsMatch(name,@"\.(GLM|GEE)_(CLASS|THRESH|CALIB|BRIER)$");}
 static object Default(ParameterInfo p){var type=p.ParameterType.IsByRef?p.ParameterType.GetElementType():p.ParameterType;if(type.IsValueType)return Activator.CreateInstance(type);return null;}
 static object NumericDefault(string name){
  string key=name.ToLowerInvariant();if(key=="alpha")return 0.05;if(key=="beta")return 0.2;if(key.Contains("proportion"))return key.StartsWith("null")?0.5:key.StartsWith("control")?0.4:0.6;
  if(key=="nullicc")return 0.3;if(key=="alternativeicc")return 0.6;if(key=="observationspersubject")return 3.0;
  if(key.Contains("total")||key=="totaln")return 50.0;if(key.Contains("responders"))return 15.0;
  if(key.Contains("margin")||key.Contains("allowable"))return key.StartsWith("lower")?-0.3:0.3;
  if(key.Contains("ratio")&& !key.Contains("hazard"))return 1.0;if(key.Contains("hazard"))return 0.7;
  if(key.Contains("sd"))return 2.0;if(key=="desiredhalfwidth")return 0.5;if(key=="q")return 3.5;if(key=="v")return 20.0;if(key=="r")return 5.0;if(key=="df1")return 5.0;if(key=="df2")return 10.0;if(key=="side")return 2.0;return 1.0;
 }
 static object[] PublicArgs(string name,MethodInfo method,bool seed){
  int n=36,predictors=name.Contains(".MULTI.")?4:2;string prefix=Prefix(name);bool bin=BinaryQuery(name);var args=method.GetParameters().Select(Default).ToArray();
  object[] mixed=(prefix=="LMM"||prefix=="MMRM")?MixedInputs(prefix=="LMM"):null;
  var pars=method.GetParameters();
  for(int i=0;i<pars.Length;i++){
   string k=pars[i].Name.ToLowerInvariant();if(pars[i].IsOptional){args[i]=pars[i].DefaultValue==DBNull.Value?null:pars[i].DefaultValue;continue;}
   if(k=="handle"){args[i]=EnsureSeed(prefix,bin);continue;}
   if(pars[i].ParameterType==typeof(double)){args[i]=NumericDefault(pars[i].Name);continue;}
   if(pars[i].ParameterType==typeof(string)){args[i]=k=="side"?"two-sided":"valid-text";continue;}
   switch(k){
    case "x":case "xrange":case "xcount":case "newx":case "newcountx":args[i]=name.Contains(".AGREE.")||name.Contains(".NP.")||name.Contains(".PAR.TTEST")?Vector(n,"continuous"):Matrix(n,predictors);break;
    case "y":case "yrange":args[i]=Vector(n,name.Contains(".CLASS.")||name.EndsWith("CALIB_POINTS")||prefix=="GEE"?"binary":prefix=="ORDLOGIT"||prefix=="MNLOGIT"?"count":prefix=="GLMNB"||prefix=="ZIP"?"count":"continuous");break;
    case "data":args[i]=name.EndsWith("BOX_M")?Matrix(n,3):name.EndsWith("MAUCHLY")||name.EndsWith("ICC_FIT")||name.EndsWith("ICC_VALUE")||name.EndsWith("ICC_RC")||name.Contains("FRIEDMAN")||name.Contains("RMANOVA")?Groups(18):Vector(n,"continuous");break;
    case "table":args[i]=name.StartsWith("BESH.PS.")? (object)"summary":Table();break;
    case "stackedtables":args[i]=new object[,]{{8.0,5.0},{4.0,9.0},{7.0,3.0},{4.0,8.0}};break;
    case "groups":args[i]=name.Contains(".MULTI.")||name.EndsWith("BOX_M")?Vector(n,"group"):Groups(12);break;
    case "group1":case "control":args[i]=Vector(12,"continuous");break;
    case "group2":case "experimental":args[i]=Vector(14,"continuous");break;
    case "rater1":args[i]=Col("a","a","b","c","b","a","c","b","c","a","b","c");break;
    case "rater2":args[i]=Col("a","b","b","c","a","a","c","b","c","b","b","a");break;
    case "time":case "timerange":args[i]=Vector(n,"time");break;
    case "status":case "statusrange":args[i]=Vector(n,"status");break;
    case "group":case "grouprange":args[i]=Vector(n,"group");break;
    case "subject":args[i]=Vector(n,"subject");break;
    case "visit":args[i]=Vector(n,"visit");break;
    case "clusterid":args[i]=Vector(n,"subject");break;
    case "probabilities":args[i]=Vector(n,"probability");break;
    case "marker":args[i]=Vector(n,"continuous");break;
    case "formula":args[i]="A + B";break;
    case "spec":args[i]=new object[,]{{"Label","Intercept","x1","x2"},{"L",0.0,1.0,0.0}};break;
    case "residtype":args[i]="pearson";break;
    default:args[i]=NumericDefault(pars[i].Name);break;
   }
  }
  Action<string,object> set=(key,value)=>{int i=Array.FindIndex(pars,a=>a.Name.Equals(key,StringComparison.OrdinalIgnoreCase));if(i>=0)args[i]=value;};
  if(mixed!=null){
   if(prefix=="MMRM"){set("y",mixed[0]);set("x",mixed[1]);set("subject",mixed[2]);set("visit",mixed[3]);set("varNames",mixed[4]);set("covariance","CS");set("fitMethod","REML");set("inference","KR");set("maxIter",200);}
   else{set("y",mixed[0]);set("x",mixed[1]);set("z",mixed[2]);set("subject",mixed[3]);set("visit",mixed[4]);set("xVarNames",mixed[5]);set("zVarNames",mixed[6]);set("residualCovariance","ID");set("randomCovariance","RI+S");set("inference",activeCase!=null&&T(activeCase,"model_profile")=="kr_inference"?"KR":"ResidualDF");set("maxIter",200);}
  }
  if(name.EndsWith(".MCA_FIT"))set("x",Categories(n,3));
  if(name.Contains(".WILCOX_")){var x=Vector(12,"continuous");var y=Vector(12,"continuous");for(int i=0;i<12;i++)y[i,0]=(double)y[i,0]+0.3*Math.Sin((i+1)*1.1)+0.07;set("x",x);set("y",y);}
  if(name=="BESH.PAR.TTEST_UNPAIRED_EQUIV")set("experimental",Vector(12,"continuous"));
  if(name=="BESH.CT.TREND")set("table",new object[,]{{8.0,5.0,4.0},{4.0,9.0,8.0}});
  if(name=="BESH.PAR.ANOVA2_NESTED"){var data=new object[24,3];for(int i=0;i<24;i++){data[i,0]="A"+(i/12);data[i,1]="B"+(i/4);data[i,2]=5.0+i*0.3+Math.Sin(i);}set("data",data);}
  if(name=="BESH.REGR.MMRM_CONTRASTS")set("group","treatment_active");
  if(name=="BESH.REGR.MMRM_LSMESTIMATE")set("spec",new object[,]{{"Label","Weight","visit","treatment_active"},{"L",1.0,1.0,1.0}});
  if(name=="BESH.REGR.GEE_LSMESTIMATE")set("spec",new object[,]{{"Label","Weight","X1"},{"L",1.0,Matrix(n,2)[0,0]}});
  if(name.EndsWith(".CA_FIT"))set("table",new object[,]{{8.0,5.0,4.0},{4.0,9.0,3.0},{5.0,2.0,7.0}});
  if(name.EndsWith(".PCA_FIT")||name.EndsWith(".FA_FIT")){set("retentionMethod","fixed");set("retentionValue",2.0);}
  if(name.EndsWith(".FA_FIT")){set("extractionMethod","principalcomponents");set("rotationMethod","none");}
  if(prefix=="PSM"&&name=="BESH.PS.FIT"){set("id",Vector(n,"id"));set("treatment",Vector(n,"binary"));set("outcome",Vector(n,"continuous"));set("covariates",Matrix(n,2));set("existingScore",Vector(n,"probability"));set("scoreMethod","existing");}
  if(seed&&(prefix=="GLM"||prefix=="GEE")&&bin){set("family","binomial");set("link","logit");set("y",Vector(n,"binary"));}
  if(name.EndsWith(".ORDLOGIT_FIT")||name.EndsWith(".MNLOGIT_FIT"))set("y",Col(Enumerable.Range(0,n).Select(i=>(object)(double)(i%3+1)).ToArray()));
  if(seed){set("randomSeed",1337.0);set("maxIter",200);}
  string profile=activeCase==null?"ordinary":T(activeCase,"model_profile","ordinary");
  if(profile!="ordinary"&&name.EndsWith("_FIT")){
   if(profile=="duplicate_labels"){
    int xslot=Array.FindIndex(pars,a=>a.Name.Equals("x",StringComparison.OrdinalIgnoreCase));var xmatrix=xslot<0?null:args[xslot] as object[,];int columns=xmatrix==null?2:xmatrix.GetLength(1);
    string labels=String.Join(",",Enumerable.Repeat("dup",columns));set("varNames",labels);set("xVarNames",labels);
    if(prefix=="CA"){set("rowNames","dup,dup,dup");set("colNames","dup,dup,dup");}
   }
   for(int slot=0;slot<pars.Length;slot++){
    var a=args[slot] as object[,];if(a==null||a.GetLength(0)<(profile=="reverse_rows"?1:6))continue;
    if(!new[]{"x","z","y","time","status","subject","visit","clusterId","covariates","id","treatment","outcome","groups","table","existingScore"}.Contains(pars[slot].Name))continue;
    if(profile=="reverse_rows"){var reversed=new object[a.GetLength(0),a.GetLength(1)];for(int r=0;r<a.GetLength(0);r++)for(int j=0;j<a.GetLength(1);j++)reversed[r,j]=a[a.GetLength(0)-1-r,j];args[slot]=reversed;}
    if(profile=="excluded_rows"&&new[]{"x","y","covariates"}.Contains(pars[slot].Name))a[3,0]=null;
    if(profile=="header_rows"&&!new[]{"subject","groups","clusterId","id"}.Contains(pars[slot].Name)){
     var headed=new object[a.GetLength(0)+1,a.GetLength(1)];for(int j=0;j<a.GetLength(1);j++)headed[0,j]=pars[slot].Name+"_"+(j+1);
     for(int r=0;r<a.GetLength(0);r++)for(int j=0;j<a.GetLength(1);j++)headed[r+1,j]=a[r,j];args[slot]=headed;
    }
   }
   if(profile=="excluded_rows")set("missingValuePolicy","listwise");
   if(profile=="header_rows"&&name.EndsWith(".MCA_FIT"))set("hasHeader",true);
  }
  return args;
 }
 static string EnsureSeed(string prefix,bool binary){
  string key=prefix+(binary?"/binary":"/ordinary")+"/"+(activeCase==null?"ordinary":T(activeCase,"model_profile","ordinary"));string value;if(seeds.TryGetValue(key,out value))return value;
  string fit=SeedExport(prefix);var method=exports[fit];var args=PublicArgs(fit,method,true);
  if(binary){var pars=method.GetParameters();for(int i=0;i<pars.Length;i++){if(pars[i].Name=="family")args[i]="binomial";if(pars[i].Name=="link")args[i]="logit";if(pars[i].Name=="y")args[i]=Vector(36,"binary");}}
  phase="SETUP_FIT_SEED";var actual=method.Invoke(null,args) as string;
  if(actual==null||!Regex.IsMatch(actual,@"^[A-Za-z0-9]+:[0-9a-fA-F]{32}$"))throw new InvalidOperationException("SEED_NOT_A_HANDLE/"+prefix+"/"+(actual??"non-string result"));
  RegisterHandle(actual);seeds[key]=actual;return actual;
 }
 static void RegisterHandle(string value){if(!handleTokens.ContainsKey(value))handleTokens[value]="H"+value.Substring(0,value.IndexOf(':'))+":"+(nextHandle++).ToString(Inv)+":N:"+(value.Substring(value.IndexOf(':')+1).Any(Char.IsUpper)?"mixed-case":"lowerhex");}
 static object Decode(string state){
  if(state=="null")return null;if(state=="empty")return Sentinel("ExcelEmpty");if(state=="missing")return Sentinel("ExcelMissing");if(state=="error")return Enum.Parse(AppDomain.CurrentDomain.GetAssemblies().First(a=>a.GetName().Name=="ExcelDna.Integration").GetType("ExcelDna.Integration.ExcelError"),"ExcelErrorValue");
  if(state=="dbnull")return DBNull.Value;if(state=="blank")return "";if(state=="whitespace")return " \t ";if(state=="true")return true;if(state=="false")return false;
  if(state.StartsWith("error_")){string suffix=state.Substring(6);return Enum.Parse(AppDomain.CurrentDomain.GetAssemblies().First(a=>a.GetName().Name=="ExcelDna.Integration").GetType("ExcelDna.Integration.ExcelError"),"ExcelError"+(suffix=="na"?"NA":suffix=="num"?"Num":"Ref"));}
  if(state.StartsWith("single:")){string v=state.Substring(7);return v=="NaN"?BitConverter.ToSingle(BitConverter.GetBytes(0x7fc00038),0):(float)D(v);}
  if(state.StartsWith("date:"))return DateTime.ParseExact(state.Substring(5),"yyyy-MM-dd",Inv,DateTimeStyles.None);
  if(state.StartsWith("text:"))return state.Substring(5);if(state.StartsWith("number:"))return D(state.Substring(7));
  if(state=="at_gee_name_value")return new object[,]{{"X1",Matrix(36,2)[0,0]}};
  if(state=="at_gee_wide")return new object[,]{{"X1"},{Matrix(36,2)[0,0]}};
  if(state=="at_mmrm_name_value")return new object[,]{{"treatment_active",1.0},{"visit",1.0}};
  if(state=="at_mmrm_wide")return new object[,]{{"treatment_active","visit"},{1.0,1.0}};
  if(state=="new_psm_options")return Activator.CreateInstance(Core.GetType("BESHStatNG.CausalInference.PsmComprehensiveFitOptions"));
  if(state.StartsWith("int:"))return Int32.Parse(state.Substring(4),Inv);if(state.StartsWith("long:"))return Int64.Parse(state.Substring(5),Inv);if(state.StartsWith("decimal:"))return Decimal.Parse(state.Substring(8),Inv);
  if(state.StartsWith("strings:"))return state.Substring(8).Split(',');if(state.StartsWith("doubles:"))return state.Substring(8)==""?new double[0]:state.Substring(8).Split(',').Select(D).ToArray();if(state.StartsWith("ints:"))return state.Substring(5)==""?new int[0]:state.Substring(5).Split(',').Select(s=>Int32.Parse(s,Inv)).ToArray();
  if(state.StartsWith("double_matrix:")){var spec=state.Split(':');int rows=Int32.Parse(spec[1],Inv),cols=Int32.Parse(spec[2],Inv);var a=new double[rows,cols];for(int r=0;r<rows;r++)for(int c=0;c<cols;c++)a[r,c]=spec[3]=="zero"?0.0:spec[3]=="signed_zero"?D("-0"):spec[3]=="nonfinite"?D(new[]{"NaN","+Inf","-Inf"}[(r+c)%3]):1.0+0.3*r-Math.Cos((r+1)*(c+1)*0.5);return a;}
  if(state=="row")return new object[,]{{1.0,2.0,3.0}};if(state=="column")return Col(1.0,2.0,3.0);if(state=="matrix")return Matrix(3,2);if(state=="zero_rows")return new object[0,1];if(state=="zero_columns")return new object[3,0];if(state=="ragged")return new object[][]{new object[]{1.0,2.0},new object[]{3.0}};
  if(state.StartsWith("matrix_profile:")){var s=state.Split(':');int nr=Int32.Parse(s[1],Inv),nc=Int32.Parse(s[2],Inv);var a=Matrix(nr,nc);if(nr>0)for(int j=0;j<nc;j++){if(s[3]=="text")a[0,j]="dup";if(s[3]=="numeric_text")a[0,j]="1";if(s[3]=="blank")a[0,j]=null;}if(nr>1&&nc>0){if(s[4]=="blank")a[1,0]=null;if(s[4]=="error")a[1,0]=Decode("error");if(s[4]=="NaN")a[1,0]=D("NaN");if(s[4]=="second_gap"&&nr>2)a[2,0]=null;}return a;}
  if(state.StartsWith("vector:")){string[] items=state.Substring(7).Split(',');return Col(items.Select(s=>s=="null"?null:(object)D(s)).ToArray());}
  if(state.StartsWith("thresholds:")){var s=state.Split(':');var list=s[2].Split(',').Select(v=>v=="null"?null:(object)D(v)).ToArray();if(s[1]=="column")return Col(list);var a=new object[s[1]=="matrix"?2:1,list.Length];for(int i=0;i<a.GetLength(0);i++)for(int j=0;j<list.Length;j++)a[i,j]=list[j];return a;}
  if(state=="object_cells")return new object[,]{{"p-value",D("-0"),D("NaN"),Decode("error"),DBNull.Value},{"metric",0.125,1.2500m,true,null},{"P value",1e-10,Single.NegativeInfinity,new DateTime(2000,1,2),""}};
  throw new InvalidOperationException("Unknown input state: "+state);
 }
 static string Snap(object value){
  if(value==null)return "N";string known;if(value is string&&handleTokens.TryGetValue((string)value,out known))return known;
  if(value is string)return "S"+B64((string)value);if(value is double)return "D"+BitConverter.DoubleToInt64Bits((double)value).ToString("X16",Inv);
  if(value is float)return "F"+BitConverter.ToInt32(BitConverter.GetBytes((float)value),0).ToString("X8",Inv);
  if(value is decimal)return "M"+String.Join(",",Decimal.GetBits((decimal)value).Select(x=>x.ToString("X8",Inv)));
  if(value is bool)return (bool)value?"B1":"B0";if(value is DBNull)return "DBNULL";if(value is DateTime)return "DATE"+((DateTime)value).Ticks+":"+((DateTime)value).Kind;
  Type type=value.GetType();if(type.IsEnum)return "ENUM"+type.FullName+":"+Convert.ToInt64(value,Inv);
  if(type.IsGenericType&&type.GetGenericTypeDefinition()==typeof(KeyValuePair<,>))return SnapDto(value);
  var array=value as Array;if(array!=null)return type.GetElementType().FullName+"["+String.Join(",",Enumerable.Range(0,array.Rank).Select(i=>array.GetLowerBound(i)+":"+array.GetLength(i)))+"]{"+String.Join("|",array.Cast<object>().Select(Snap))+"}";
  if(type.IsPrimitive)return type.FullName+":"+Convert.ToString(value,Inv);
  var sequence=value as IEnumerable;if(sequence!=null)return "SEQ"+type.FullName+"{"+String.Join("|",sequence.Cast<object>().Select(Snap))+"}";
  if(type.FullName=="ExcelDna.Integration.ExcelEmpty"||type.FullName=="ExcelDna.Integration.ExcelMissing")return type.FullName;
  if(type.FullName=="BESHStatNG.ResultTable"){var projection=type.GetMethod("ToOutputModel").Invoke(value,null);return SnapDto(projection);}
  if(type.Namespace!=null&&type.Namespace.StartsWith("BESHStatNG")&&(type.Name.EndsWith("Data",StringComparison.OrdinalIgnoreCase)||type.Name=="ResultTableOutputModel"||type.Name.EndsWith("Options",StringComparison.Ordinal)||type.Name=="PsmCoarseningSpec"))return SnapDto(value);
  return "OPAQUE:"+type.FullName;
 }
 static string SnapDto(object value){var type=value.GetType();var list=new List<string>();foreach(var p in type.GetProperties(BindingFlags.Public|BindingFlags.Instance).Where(p=>p.GetIndexParameters().Length==0).OrderBy(p=>p.Name,StringComparer.Ordinal))list.Add(p.Name+"="+Snap(p.GetValue(value,null)));foreach(var f in type.GetFields(BindingFlags.Public|BindingFlags.Instance).OrderBy(f=>f.Name,StringComparer.Ordinal))list.Add(f.Name+"="+Snap(f.GetValue(value)));return type.FullName+"{"+String.Join("|",list)+"}";}
 static string Kind(object result){if(result==null)return "RETURN_NULL";if(result.GetType().FullName=="ExcelDna.Integration.ExcelError")return "RETURN_ERROR_CELL";if(result is string&&Regex.IsMatch((string)result,@"^[A-Za-z0-9]+:[0-9a-fA-F]{32}$")){RegisterHandle((string)result);return "HANDLE_CREATED";}var a=result as Array;if(a!=null)return a.Length==0?"RETURN_EMPTY_ARRAY":"RETURN_ARRAY";if(result is string)return "RETURN_TEXT";return "RETURN_SCALAR";}
 static int Execute(string[] argv){
  string directory=Path.GetFullPath(argv[0]),casePath=argv[1],output=argv[2],receipt=argv[3];if(File.Exists(output)||File.Exists(receipt))throw new Exception("Append-only capture exists");
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var path=Path.Combine(directory,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
  Core=Assembly.LoadFrom(Path.Combine(directory,"BESHStat.Core.dll"));Library=Assembly.LoadFrom(Path.Combine(directory,"BESHStat.Core.Tests.dll"));
  exports=Library.GetTypes().SelectMany(t=>t.GetMethods(BindingFlags.Public|BindingFlags.Static)).Where(m=>m.CustomAttributes.Any(a=>a.AttributeType.FullName=="ExcelDna.Integration.ExcelFunctionAttribute")).ToDictionary(m=>(string)m.CustomAttributes.Single(a=>a.AttributeType.FullName=="ExcelDna.Integration.ExcelFunctionAttribute").NamedArguments.Single(a=>a.MemberName=="Name").TypedValue.Value,m=>m,StringComparer.Ordinal);
  if(exports.Count!=268)throw new Exception("Expected268 authentic export bodies");
  var serializer=new JavaScriptSerializer();serializer.MaxJsonLength=int.MaxValue;var started=DateTime.UtcNow;var outcomes=new Dictionary<string,int>();int count=0;
  var logger=new CaptureLogger();BESHStatNG.AppInfrastructure.CoreServices.Logger=logger;
  var reflectedLogger=Core.GetType("BESHStatNG.AppInfrastructure.CoreServices").GetProperty("Logger").GetValue(null,null);
  if(!Object.ReferenceEquals(reflectedLogger,logger))throw new Exception("REFERENCE_CORE_LOGGER_BINDING_MISMATCH");
  Library.GetType("BESHStatNG.UDFhelpers").GetMethod("LoggedUdfExceptionText",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{"CAPTURE_BINDING_PROBE",new InvalidOperationException("synthetic binding probe")});
  if(logger.Errors.Count!=1)throw new Exception("REFERENCE_LOGGER_PROBE_DID_NOT_REACH_SINK");logger.Errors.Clear();
  var traceType=Library.GetType("BESHStatNG.UdfTrace38");StreamWriter traceWriter=traceType==null?null:new StreamWriter(output+".reachability.jsonl",false,new UTF8Encoding(false));
  using(var writer=new StreamWriter(output,false,new UTF8Encoding(false))){writer.NewLine="\n";writer.WriteLine("case_id\tinput_sha256\toutcome\tphase\tresult\tbyref_arguments\terror\tcaught_error_events");
   foreach(var line in File.ReadLines(casePath,Encoding.UTF8)){
    var c=serializer.Deserialize<Dictionary<string,object>>(line);activeCase=c;var culture=CultureInfo.GetCultureInfo(T(c,"culture"));System.Threading.Thread.CurrentThread.CurrentCulture=culture;System.Threading.Thread.CurrentThread.CurrentUICulture=culture;
    phase="BUILD_INPUT";logger.Errors.Clear();object result=null;object[] args=null;string input="N",error="None",kind="UNSET";
    if(traceType!=null)traceType.GetMethod("Clear").Invoke(null,null);
    try{
     MethodInfo target;
     if(T(c,"target_kind")=="export") {target=exports[T(c,"target")];args=PublicArgs(T(c,"target"),target,false);}
     else{var type=Library.GetType(T(c,"type"));var candidates=type.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).Where(m=>m.Name==T(c,"target")&&(c.ContainsKey("generic_type")?m.IsGenericMethod:!m.IsGenericMethod));if(c.ContainsKey("overload_first_type"))candidates=candidates.Where(m=>m.GetParameters().First().ParameterType.FullName==T(c,"overload_first_type"));target=candidates.Single();if(c.ContainsKey("generic_type"))target=target.MakeGenericMethod(typeof(string));args=target.GetParameters().Select(p=>p.HasDefaultValue&&p.DefaultValue!=DBNull.Value?p.DefaultValue:Default(p)).ToArray();}
     var parameters=target.GetParameters();
     if(T(c,"target_kind")=="export"&&c.ContainsKey("handle_variant")){
      int slot=Array.FindIndex(parameters,p=>p.Name=="handle");if(slot<0)throw new Exception("Handle variant requires a handle argument");string original=args[slot] as string??EnsureSeed(Prefix(T(c,"target")),false);
      if(T(c,"handle_variant")=="uppercase"){string alias=original.ToUpperInvariant();handleTokens[alias]=handleTokens[original]+":UPPERCASE";args[slot]=alias;}
      if(T(c,"handle_variant")=="other_model")args[slot]=EnsureSeed(Prefix(T(c,"target"))=="PCA"?"LM":"PCA",false);
     }
     if(c.ContainsKey("arguments"))foreach(var pair in (Dictionary<string,object>)c["arguments"]){int slot=Array.FindIndex(parameters,p=>p.Name.Equals(pair.Key,StringComparison.OrdinalIgnoreCase));if(slot<0)throw new Exception("Unknown parameter "+pair.Key);args[slot]=Decode(Convert.ToString(pair.Value,Inv));}
     if(c.ContainsKey("cache_comparer")){
      var cache=new System.Collections.Concurrent.ConcurrentDictionary<string,string>(T(c,"cache_comparer")=="ordinal_ignore_case"?StringComparer.OrdinalIgnoreCase:StringComparer.Ordinal);cache["LM:known"]="synthetic cached payload";
      args[Array.FindIndex(parameters,p=>p.Name=="cache")]=cache;
     }
     input=Snap(args);ControlCachedFixtureClocks();phase="INVOKE_ORIGINAL_SOURCE_BODY";result=target.Invoke(null,args);kind=Kind(result);
     if(T(c,"transaction")=="repeat_query"){
      phase="ORIGINAL_REPEAT_QUERY";ControlCachedFixtureClocks();result=new object[]{result,target.Invoke(null,args)};kind=Kind(result);
     }
     if(T(c,"transaction")=="drop_repeat_query_miss"){
      phase="ORIGINAL_DROP_REPEAT_THEN_QUERY_MISS";object repeated=target.Invoke(null,args);string prefix=Prefix(T(c,"target"));string query=prefix=="LMM"?"BESH.REGR.LMM_COEF":prefix=="MMRM"?"BESH.REGR.MMRM_COEF":prefix=="PSM"?"BESH.PS.SUMMARY":exports.Keys.Single(k=>k.EndsWith("."+prefix+"_SUMMARY"));
      var q=exports[query];var qa=q.GetParameters().Select(p=>p.HasDefaultValue&&p.DefaultValue!=DBNull.Value?p.DefaultValue:Default(p)).ToArray();qa[Array.FindIndex(q.GetParameters(),p=>p.Name=="handle")]=args[Array.FindIndex(parameters,p=>p.Name=="handle")];result=new object[]{result,repeated,q.Invoke(null,qa)};kind=Kind(result);
     }
     if(T(c,"target_kind")=="export"&&(T(c,"target").EndsWith("_DROP")||T(c,"target").EndsWith("_CLEAR_ALL")||T(c,"target")=="BESH.PS.CLEANUP"))seeds.Clear();
    }catch(Exception ex){error=Error(ex);kind="EXCEPTION";}
    int native=(int)Library.GetType("ExcelDna.Integration.XlCall").GetField("NativeAttempts").GetValue(null);if(native!=0)throw new Exception("NATIVE_TEST_DOUBLE_WAS_REACHED");
    if(traceWriter!=null)traceWriter.WriteLine(serializer.Serialize(new{case_id=T(c,"case_id"),reached=traceType.GetMethod("Snapshot").Invoke(null,null)}));
    writer.WriteLine(String.Join("\t",new[]{T(c,"case_id"),Hash(line+"\n"+input),kind,phase,Snap(result),Snap(args),error,Snap(logger.Errors.ToArray())}));
    if(!outcomes.ContainsKey(kind))outcomes[kind]=0;outcomes[kind]++;count++;
   }
  }
  if(traceWriter!=null)traceWriter.Dispose();
  File.WriteAllText(receipt,serializer.Serialize(new{started_utc=started.ToString("o",Inv),completed_utc=DateTime.UtcNow.ToString("o",Inv),rows=count,outcomes,core_mvid=Core.ManifestModule.ModuleVersionId.ToString(),library_mvid=Library.ManifestModule.ModuleVersionId.ToString(),native_attempts=0,wizard_false_stub_calls=Library.GetType("ExcelDna.Integration.ExcelDnaUtil").GetField("WizardFalseBranchCalls").GetValue(null),native="NATIVE_NOT_RUN; explicit native test double only",scientific="NOT_GOLDEN"}),new UTF8Encoding(false));
  Console.WriteLine("Captured "+count+" original-source cases; native attempts0; "+serializer.Serialize(outcomes));return 0;
 }
 public static int Main(string[] argv){try{return Execute(argv);}catch(Exception ex){Console.Error.WriteLine(Error(ex));return 1;}}
}
