// Real shipping FIT/query/drop operations and symbolic-reference graph;raw tokens/results remain private.
using System;using System.Collections;using System.Collections.Generic;using System.Globalization;using System.IO;
using System.Linq;using System.Reflection;using System.Text.RegularExpressions;using System.Threading;using System.Threading.Tasks;using System.Web.Script.Serialization;
class Observe43V2 {
 static Assembly Host;static List<object> Ops=new List<object>(),Graphs=new List<object>();static string Context;
 static Type Previous=typeof(Observe42);static Dictionary<string,MethodInfo> Methods;
 sealed class BrokenText {public override string ToString(){throw new InvalidOperationException("controlled coercion failure");}}
 static object V(object x){if(x==null)return new{type="Nothing"};if(x is double)return new{type="Double",bits=BitConverter.DoubleToInt64Bits((double)x).ToString("x16")};var a=x as Array;if(a!=null)return new{type=x.GetType().FullName,bounds=Enumerable.Range(0,a.Rank).Select(i=>new[]{a.GetLowerBound(i),a.GetLength(i)}).ToArray(),values=a.Cast<object>().Select(V).ToArray()};if(x is BrokenText)return new{type="BrokenText",behavior="throws controlled coercion failure"};return new{type=x.GetType().FullName,value=Convert.ToString(x,CultureInfo.InvariantCulture)};}
 static object[,] Col(params object[] x){var a=new object[x.Length,1];for(int i=0;i<x.Length;i++)a[i,0]=x[i];return a;}
 static object[,] Vec(string kind){return Col(Enumerable.Range(0,36).Select(i=>kind=="subject"?(object)("S"+(i/4+1)):kind=="visit"?(object)(double)(i%4+1):kind=="status"?(object)(i%4==0?0.0:1.0):kind=="time"?(object)(1.0+(i*7)%31):kind=="count"?(object)(double)((i*5)%9):kind=="ordinal"?(object)(double)(i%3+1):(object)(3.0+i*.4+Math.Sin((i+1)*.7))).ToArray());}
 static object[,] Matrix(int n,int p){var a=new object[n,p];for(int i=0;i<n;i++)for(int j=0;j<p;j++)a[i,j]=1.0+.3*i+Math.Sin((i+1)*(j+1)*.7)+((i+2*j)%5)*.11;return a;}
 static void Set(MethodInfo m,object[] a,string name,object x){int k=Array.FindIndex(m.GetParameters(),p=>p.Name==name);if(k>=0)a[k]=x;}
 static object[] Args(MethodInfo m,string family){
  if(m.DeclaringType.Name=="MultivariateAnalysisUDFs"||m.DeclaringType.Name=="PropensityScoreUDFs")return (object[])Previous.GetMethod("Args",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{m});
  var a=m.GetParameters().Select(p=>p.IsOptional?null:p.Name=="y"?(object)Vec(family=="ORDLOGIT"||family=="MNLOGIT"?"ordinal":family=="GLMNB"||family=="ZIP"?"count":"continuous"):p.Name=="x"||p.Name=="xCount"||p.Name=="newX"?Matrix(36,2):p.Name=="time"?Vec("time"):p.Name=="status"?Vec("status"):p.Name=="clusterId"||p.Name=="subject"?Vec("subject"):p.Name=="visit"?Vec("visit"):null).ToArray();
  if(family=="GLM"||family=="GEE"){Set(m,a,"family","Gaussian");Set(m,a,"link","Identity");}
  if(family=="GEE")Set(m,a,"covariance","Independence");
  if(family=="LMM"||family=="MMRM"){
   int visits=family=="LMM"?5:3,n=18*visits;var y=new object[n,1];var x=new object[n,family=="LMM"?3:5];var z=new object[n,1];var subject=new object[n,1];var visit=new object[n,1];int r=0;
   for(int sid=1;sid<=18;sid++)for(int v=1;v<=visits;v++,r++){double trt=sid%2==0?1:0,days=v-3,diff=-.2+.15*(v-1)+.04*(sid%4);subject[r,0]="S"+sid.ToString("00",CultureInfo.InvariantCulture);visit[r,0]=(double)v;
    if(family=="LMM"){y[r,0]=12+.9*days+1.1*trt+.55*diff+.28*days*trt+.03*(sid-9)+.35*(sid%6-2)+.04*(sid%5-2)*days+.035*((sid+v-1)%5-2);x[r,0]=days;x[r,1]=trt;x[r,2]=diff;z[r,0]=days;}
    else{double v2=v==2?1:0,v3=v==3?1:0;y[r,0]=20+1.4*trt+.8*v+.35*trt*v+.15*(sid%5-2)+.03*((sid+v)%4-1);x[r,0]=trt;x[r,1]=v2;x[r,2]=v3;x[r,3]=trt*v2;x[r,4]=trt*v3;}
   }
   Set(m,a,"y",y);Set(m,a,"x",x);Set(m,a,"subject",subject);Set(m,a,"visit",visit);Set(m,a,"maxIter",200);
   if(family=="LMM"){Set(m,a,"z",z);Set(m,a,"xVarNames","Days,Treatment,Difficulty");Set(m,a,"zVarNames","Days");Set(m,a,"residualCovariance","ID");Set(m,a,"randomCovariance","RI+S");Set(m,a,"inference","ResidualDF");}
   else{Set(m,a,"varNames","treatment_active,visit2,visit3,treatment_active:visit2,treatment_active:visit3");Set(m,a,"covariance","CS");Set(m,a,"inference","KR");}
  }
  return a;
 }
 static object Add(MethodInfo m,string tag,object[] args){var input=V(args);try{var x=m.Invoke(null,args);Ops.Add(new{id=Context+"/"+tag,method=m.DeclaringType.FullName+"."+m.Name,input,outcome="VALUE",output=V(x),exception=(object)null});return x;}catch(Exception e){while(e is TargetInvocationException)e=e.InnerException;Ops.Add(new{id=Context+"/"+tag,method=m.DeclaringType.FullName+"."+m.Name,input,outcome="EXCEPTION",output=(object)null,exception=new{type=e.GetType().FullName,message=e.Message}});return null;}}
 static object Sentinel(string name){var t=Host.GetReferencedAssemblies().First(a=>a.Name=="ExcelDna.Integration");return Assembly.Load(t).GetType("ExcelDna.Integration."+name).GetField("Value").GetValue(null);}
 static int Count(IDictionary cache){return cache.Count;}
 static object Lookup(Type owner,IDictionary cache,Type valueType,object key){
  var direct=owner.GetMethods(BindingFlags.NonPublic|BindingFlags.Static).FirstOrDefault(m=>m.Name.StartsWith("TryGet")&&m.GetParameters().Length==2&&m.GetParameters()[1].ParameterType==valueType.MakeByRefType());
  if(direct!=null){var a=new object[]{key,null};bool ok=(bool)direct.Invoke(null,a);return ok?a[1]:null;}
  var generic=Host.GetType("BESHStatNG.WorksheetFunctions.UdfCacheHelpers").GetMethod("TryGetCachedHandle",BindingFlags.NonPublic|BindingFlags.Static).MakeGenericMethod(valueType);var b=new object[]{key,cache,null};return (bool)generic.Invoke(null,b)?b[2]:null;
 }
 static object[] QueryArgs(MethodInfo q,string family,object handle){var a=Args(q,family);Set(q,a,"handle",handle);return a;}
 static void Main(string[] argv){try{Run(argv);}catch(Exception e){Console.Error.WriteLine(e);Environment.ExitCode=1;}}
 static void Run(string[] argv){
  Host=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"BESHStatNG.dll"));Previous.GetField("Host",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,Host);Previous.GetField("Profile",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,"ordinary");
  Methods=Host.GetTypes().Where(t=>t.Namespace=="BESHStatNG.WorksheetFunctions").SelectMany(t=>t.GetMethods(BindingFlags.Static|BindingFlags.Public)).Where(m=>m.GetCustomAttributes(false).Any(a=>a.GetType().Name=="ExcelFunctionAttribute")).ToDictionary(m=>m.Name);
  var cfg=new JavaScriptSerializer{MaxJsonLength=int.MaxValue}.Deserialize<Dictionary<string,object>>(File.ReadAllText(argv[1]));var families=((IEnumerable)cfg["families"]).Cast<Dictionary<string,object>>().ToArray();
  foreach(string culture in new[]{"","sk-SK"}){CultureInfo.CurrentCulture=CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
   foreach(Dictionary<string,object> f in families){string family=(string)f["family"],module=(string)f["module"],cacheName=(string)f["dictionary"],fitName=(string)f["FIT"];Context=culture+"/"+family;Console.WriteLine("Lifecycle "+Context);
    var owner=Host.GetType("BESHStatNG.WorksheetFunctions."+module);var cache=(IDictionary)owner.GetField(cacheName,BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);cache.Clear();var fit=Methods[fitName];var fitArgs=Args(fit,family);
    var keyA=Add(fit,"FIT_A",fitArgs) as string;var keyB=Add(fit,"FIT_B_SAME_INPUT",Args(fit,family)) as string;
    if(keyA==null||keyB==null||!Regex.IsMatch(keyA,"^"+family+":[0-9a-f]{32}$")||!Regex.IsMatch(keyB,"^"+family+":[0-9a-f]{32}$")||keyA==keyB||!cache.Contains(keyA)||!cache.Contains(keyB))throw new Exception("REAL_FIT_GRAPH_FAILURE:"+family);
    var modelA=cache[keyA];var modelB=cache[keyB];if(Object.ReferenceEquals(modelA,modelB))throw new Exception("ALIASED_FIT_OBJECTS:"+family);
    var boundA=Lookup(owner,cache,modelA.GetType(),keyA);var boundB=Lookup(owner,cache,modelB.GetType(),keyB);
    if(!Object.ReferenceEquals(boundA,modelA)||!Object.ReferenceEquals(boundB,modelB))throw new Exception("WRONG_MODEL_REFERENCE:"+family);
    var tokens=new List<object>{new{symbol="A",token=keyA,valueOrdinal=1,boundValueOrdinal=Object.ReferenceEquals(boundA,modelA)?1:2},new{symbol="B",token=keyB,valueOrdinal=2,boundValueOrdinal=Object.ReferenceEquals(boundB,modelB)?2:1}};
    var qs=((IEnumerable)f["queries"]).Cast<string>().ToArray();string preferred=family=="PCA"?"PCA_SCORES":family=="FA"?"FA_SCORES":family=="KMEANS"?"KMEANS_ASSIGNMENTS":family=="HCLUST"?"HCLUST_MEMBERSHIP":family=="CA"?"CA_ROWS":family=="MCA"?"MCA_CATEGORIES":family=="DA"?"DA_CASEWISE":family=="PSM"?"PS_SCORES":family=="GEE"?"GEE_VCOV":family=="LMM"||family=="MMRM"?family+"_COEF":family+"_TESTS";
    var q=Methods[preferred];var a=QueryArgs(q,family,keyA);var first=Add(q,"QUERY_A",a);if(!(first is Array))throw new Exception("REAL_QUERY_NOT_ARRAY:"+family);
    Add(q,"QUERY_B",QueryArgs(q,family,keyB));Add(q,"QUERY_A_CASE_SPACE",QueryArgs(q,family,"  "+keyA.ToUpperInvariant()+"  "));
    var table=first as object[,];if(table!=null&&table.Length>0)table[0,0]="CALLER_MUTATION";Add(q,"QUERY_A_AFTER_MUTATION",QueryArgs(q,family,keyA));
    var invalid=new object[]{null,"","   ",Sentinel("ExcelMissing"),Sentinel("ExcelEmpty"),17,true,new object[0,0],new object[,]{{"bad"}},"malformed",family+":"+new string('f',32),"OTHER:"+keyA.Split(':')[1],new BrokenText()};
    for(int i=0;i<invalid.Length;i++)Add(q,"INVALID_"+i,QueryArgs(q,family,invalid[i]));
    foreach(string name in qs){var method=Methods[name];Add(method,"ALL_QUERY_"+name,QueryArgs(method,family,keyA));}
    var dropNames=((IEnumerable)f["drop"]).Cast<string>().ToArray();var drop=Methods[dropNames.Length==0?"PS_CLEANUP":dropNames[0]];int before=Count(cache);
    var dropped=Add(drop,"DROP_A",QueryArgs(drop,family," "+keyA.ToUpperInvariant()+" "));bool gone=!cache.Contains(keyA)&&cache.Contains(keyB)&&Object.ReferenceEquals(cache[keyB],modelB);if(!gone||!Equals(dropped,true))throw new Exception("DROP_ISOLATION_FAILURE:"+family);
    Add(q,"QUERY_A_AFTER_DROP",QueryArgs(q,family,keyA));Add(q,"QUERY_B_SURVIVES",QueryArgs(q,family,keyB));Add(drop,"DROP_A_AGAIN",QueryArgs(drop,family,keyA));
    foreach(string name in ((IEnumerable)f["clear"]).Cast<string>()){var clear=Methods[name];var ca=Args(clear,family);if(name=="PS_CLEANUP")Set(clear,ca,"handle","aLl");Add(clear,"CLEAR_REMAINDER",ca);Add(clear,"CLEAR_EMPTY",ca);}
    if(family=="PSM"){var again=Add(fit,"FIT_FOR_BLANK_CLEANUP",Args(fit,family)) as string;var value=cache[again];var bound=Lookup(owner,cache,value.GetType(),again);if(!Object.ReferenceEquals(value,bound))throw new Exception("BLANK_CLEANUP_REFERENCE_FAILURE");tokens.Add(new{symbol="C",token=again,valueOrdinal=3,boundValueOrdinal=3});Add(Methods["PS_CLEANUP"],"BLANK_CLEANUP",new object[]{null});Add(q,"QUERY_C_AFTER_BLANK_CLEANUP",QueryArgs(q,family,again));}
    Graphs.Add(new{context=Context,family,tokens=tokens,modelTypes=new[]{modelA.GetType().FullName,modelB.GetType().FullName},uniqueGeneratedTokens=keyA!=keyB,distinctCachedObjects=!Object.ReferenceEquals(modelA,modelB),queryABindsOwnObject=Object.ReferenceEquals(boundA,modelA),queryBBindsOwnObject=Object.ReferenceEquals(boundB,modelB),dropOnlyA=gone,preDropCount=before,postDropCount=before-1,workbookScope="NOT_ESTABLISHED",sessionEpoch="NOT_ESTABLISHED",liveExcel="NOT_RUN"});
    cache.Clear();
   }
  }
  var serializer=new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=200};File.WriteAllText(argv[0],serializer.Serialize(new{kind="CURRENT_MAIN_PRE_EDIT_04A43",seed=173,operations=Ops,graphs=Graphs}),new System.Text.UTF8Encoding(false));Console.WriteLine("Real lifecycle operations "+Ops.Count+" /graphs "+Graphs.Count);
 }
}
