using System;using System.Collections;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;using System.Reflection;using System.Threading;using System.Threading.Tasks;using System.Web.Script.Serialization;
class Race43 {
 static Type O=typeof(Observe43V3);static Assembly Host;static BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Static;static List<object> Rows=new List<object>();
 static object[] Args(MethodInfo m,string f){return (object[])O.GetMethod("Args",Flags).Invoke(null,new object[]{m,f});}
 static object Lookup(Type owner,IDictionary cache,Type t,object key){return O.GetMethod("Lookup",Flags).Invoke(null,new object[]{owner,cache,t,key});}
 static void Pair(Action a,Action b){using(var gate=new Barrier(2)){var tasks=new[]{Task.Run(()=>{if(!gate.SignalAndWait(5000))throw new Exception("barrier timeout");a();}),Task.Run(()=>{if(!gate.SignalAndWait(5000))throw new Exception("barrier timeout");b();})};if(!Task.WaitAll(tasks,10000))throw new Exception("bounded task timeout");}}
 static void Main(string[] a){try{Run(a);}catch(Exception e){Console.Error.WriteLine(e);Environment.ExitCode=1;}}
 static void Run(string[] argv){Host=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"BESHStatNG.dll"));O.GetField("Host",Flags).SetValue(null,Host);typeof(Observe42).GetField("Host",Flags).SetValue(null,Host);typeof(Observe42).GetField("Profile",Flags).SetValue(null,"ordinary");
  var serializer=new JavaScriptSerializer{MaxJsonLength=int.MaxValue,RecursionLimit=200};var cfg=serializer.Deserialize<Dictionary<string,object>>(File.ReadAllText(argv[1]));
  foreach(string culture in new[]{"","sk-SK"}){CultureInfo.CurrentCulture=CultureInfo.CurrentUICulture=CultureInfo.GetCultureInfo(culture);
   foreach(Dictionary<string,object> f in (IEnumerable)cfg["families"]){string family=(string)f["family"];var owner=Host.GetType("BESHStatNG.WorksheetFunctions."+(string)f["module"],true,true);var cache=(IDictionary)owner.GetField((string)f["dictionary"],Flags).GetValue(null);cache.Clear();var fit=owner.GetMethod((string)f["FIT"]);var keyA=(string)fit.Invoke(null,Args(fit,family));var keyB=(string)fit.Invoke(null,Args(fit,family));var valueA=cache[keyA];var valueB=cache[keyB];if(valueA==null||valueB==null||Object.ReferenceEquals(valueA,valueB))throw new Exception("real race fit failure "+family);
    var drops=((IEnumerable)f["drop"]).Cast<string>().ToArray();var drop=owner.GetMethod(drops.Length>0?drops[0]:"PS_CLEANUP");var clears=((IEnumerable)f["clear"]).Cast<string>().ToArray();MethodInfo clear=clears.Length==0?null:owner.GetMethod(clears[0]);
    Func<object> dropA=()=>drop.Invoke(null,new object[]{keyA});Func<int> clearAll=()=>{if(clear!=null)return Convert.ToInt32(clear.Invoke(null,clear.GetParameters().Length==0?new object[0]:new object[]{"ALL"}));int n=cache.Count;cache.Clear();return n;};
    foreach(string mode in new[]{"GET_DROP","GET_CLEAR","CLEAR_CLEAR"})for(int n=0;n<8;n++){
     cache.Clear();cache.Add(keyA,valueA);cache.Add(keyB,valueB);bool found=false;object read=null;int c1=-1,c2=-1;bool removed=false;
     if(mode=="GET_DROP")Pair(()=>{read=Lookup(owner,cache,valueA.GetType(),keyA);found=read!=null;},()=>removed=Equals(dropA(),true));
     else if(mode=="GET_CLEAR")Pair(()=>{read=Lookup(owner,cache,valueA.GetType(),keyA);found=read!=null;},()=>c1=clearAll());
     else Pair(()=>c1=clearAll(),()=>c2=clearAll());
     bool correctRef=!found||Object.ReferenceEquals(read,valueA);if(!correctRef||cache.Contains(keyA)||(mode=="GET_DROP"&&!removed)||(mode!="GET_DROP"&&cache.Count!=0)||c1>2||c2>2)throw new Exception("invalid bounded race result "+family+"/"+mode);
     Rows.Add(new{id=culture+"/"+family+"/"+mode+"/"+n,family,mode,found,correctReference=correctRef,removed,c1,c2,remaining=cache.Count,clearKind=clear==null?"CONTROLLED_HOST_PRIMITIVE_SIMULATION;NO_PUBLIC_CLEAR_EXPORT":"ACTUAL_PUBLIC_CLEAR_OR_CLEANUP",completedWithinBound=true});
    }
    cache.Clear();
   }
  }
  var pca=Host.GetType("BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs");var pf=pca.GetMethod("PCA_FIT");string one=null,two=null;Pair(()=>one=(string)pf.Invoke(null,Args(pf,"PCA")),()=>two=(string)pf.Invoke(null,Args(pf,"PCA")));if(one==two||!one.StartsWith("PCA:")||!two.StartsWith("PCA:"))throw new Exception("parallel FIT identity failure");
  var pq=pca.GetMethod("PCA_SCORES");object[,] q1=null,q2=null;Pair(()=>q1=(object[,])pq.Invoke(null,new object[]{one,true}),()=>q2=(object[,])pq.Invoke(null,new object[]{one,true}));if(Object.ReferenceEquals(q1,q2)||q1.GetLength(0)!=q2.GetLength(0))throw new Exception("parallel read alias");q1[0,0]="caller mutation";if(Equals(q2[0,0],"caller mutation"))throw new Exception("parallel output leakage");
  File.WriteAllText(argv[0],serializer.Serialize(new{kind="CURRENT_MAIN_PRE_EDIT_04A43_BOUNDED_RACES",rows=Rows,parallelFitDistinctTokens=one!=two,parallelReadDistinctArrays=!Object.ReferenceEquals(q1,q2),noOutputLeakage=!Equals(q2[0,0],"caller mutation"),liveExcel="NOT_RUN",countClearPolicy="Counts are observed snapshots,not a serializable removal count;race ordering not normalized to one fixed result."}));Console.WriteLine("Bounded actual-cache race rows "+Rows.Count+";parallel FIT/read assertions PASS");
 }
}
