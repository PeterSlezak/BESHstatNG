using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using BESHStatNG;
using BESHStatNG.Multivariate;
#if CURRENT
using BESHStatNG.ExcelCommon;
using Prefix=Frozen37.Current37;
#else
using Prefix=Frozen37.Original37;
using Text=Frozen37.ArchivedLateExpressions37;
#endif

public static class Harness37 {
 static string phase; static bool prefixComplete;static List<object> output;
 static readonly CultureInfo Invariant=CultureInfo.InvariantCulture;
 static int I(Dictionary<string,object>d,string k,int fallback=0){return d.ContainsKey(k)?Convert.ToInt32(d[k],Invariant):fallback;}
 static string T(Dictionary<string,object>d,string k,string fallback=""){return d.ContainsKey(k)?Convert.ToString(d[k],Invariant):fallback;}
 static bool B(Dictionary<string,object>d,string k){return d.ContainsKey(k)&&Convert.ToBoolean(d[k],Invariant);}
 static void Put(object model,string field,object value){model.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(model,value);}
 static double Number(string s){return s=="NaN"?Double.NaN:s=="+Inf"?Double.PositiveInfinity:s=="-Inf"?Double.NegativeInfinity:Double.Parse(s,Invariant);}
 static double[,] Matrix(int rows,int cols,double multiplier=1){var a=new double[rows,cols];for(int i=0;i<rows;i++)for(int j=0;j<cols;j++)a[i,j]=multiplier*((i+1)*0.375-(j+1)*0.17);return a;}
 static string[] Labels(int n,string state,string prefix){if(state=="none"||state=="missing")return null;int count=state=="short"?Math.Max(0,n-1):state=="long"?n+1:n;if(state=="custom_duplicates")return Enumerable.Range(0,count).Select(i=>i%2==0?"duplicate":" custom ").ToArray();return Enumerable.Range(0,count).Select(i=>state=="blank"?(i%2==0?null:" "):new[]{" dup ","dup"," \u03bc ","",prefix+":\u017e\nlabel"}[i%5]).ToArray();}
 static string[] Groups(int n,string state){if(state=="none")return null;if(state=="case_width")return Enumerable.Range(0,n).Select(i=>new[]{"B","\uff22","b","A","a","",null}[i%7]).ToArray();int count=state=="short"?Math.Max(0,n-1):state=="long"?n+1:n;return Enumerable.Range(0,count).Select(i=>new[]{" B ","a","A",null," ","\uff22","b"}[i%7]).ToArray();}
 static PCA Pca(Dictionary<string,object>d){
  int n=I(d,"observations"),p=I(d,"variables"),k=I(d,"components"),cols=Math.Max(3,k);var m=new PCA();var scores=Matrix(n,cols);var loads=Matrix(p,cols,-0.75);string state=T(d,"matrix");
  if(state=="missing_scores")scores=null;if(state=="missing_loadings")loads=null;if(state=="short_columns"){scores=Matrix(n,1);loads=Matrix(p,1);}if(state=="short_rows")scores=Matrix(Math.Max(0,n-1),cols);
  if(state=="constant"){scores=new double[n,cols];loads=new double[p,cols];}
  if(state=="signed_zero"){for(int i=0;i<n;i++)for(int j=0;j<cols;j++)scores[i,j]=BitConverter.Int64BitsToDouble(long.MinValue);}
  var eigen=new double[]{5,2,0.75,0.125};switch(T(d,"eigen")){case "zero":eigen=new double[4];break;case "negative":eigen[0]=-5;eigen[1]=-2;break;case "NaN":eigen[0]=BitConverter.Int64BitsToDouble(unchecked((long)0x7FF8000000000037));break;case "+Inf":eigen[0]=Double.PositiveInfinity;break;case "missing":eigen=null;break;case "too_short":eigen=new[]{5.0};break;}
  string row=T(d,"row_ids");int[] ids=row=="missing"?null:Enumerable.Range(0,row=="short"?Math.Max(0,n-1):n).Select(i=>row=="duplicate"?501:501-i*17).ToArray();
  Put(m,"n",n);Put(m,"p",p);Put(m,"pNoExtractComponents",k);Put(m,"pEigenval",eigen);Put(m,"pFinalDataset",scores);Put(m,"pLoadings",loads);Put(m,"pRowNums",ids);Put(m,"pVarNames",Labels(p,T(d,"variable_labels"),"var"));return m;
 }
 static FactorAnalysis Factor(Dictionary<string,object>d){
  int k=I(d,"factors"),p=4,cols=Math.Max(4,k);string profile=T(d,"profile");
  if(profile.StartsWith("fitted_")){
   p=6;var data=new double[36,p];for(int i=0;i<36;i++){double f1=(i-17.5)/6.0+0.15*Math.Cos(i*0.3),f2=0.35*f1+0.9*(Math.Sin((i+1)*0.55)+((i%5)-2)/3.0);data[i,0]=0.88*f1+0.1*f2+(((i*5)%7)-3)/40.0;data[i,1]=0.78*f1-0.12*f2+(((i*3)%11)-5)/45.0;data[i,2]=0.18*f1+0.86*f2+(((i*4)%9)-4)/42.0;data[i,3]=-0.08*f1+0.76*f2+(((i*6)%10)-5)/38.0;data[i,4]=0.58*f1+0.46*f2+(((i*2)%8)-4)/44.0;data[i,5]=0.52*f1-0.4*f2+(((i*7)%12)-6)/48.0;}
   var fitted=new FactorAnalysis();fitted.dataInputs(data,Enumerable.Range(0,36).Select(i=>501+i*7).ToArray(),Labels(p,"mixed","var"));fitted.settingsInputs(maximumIteration:120,dEps:0.0000001,extractionMethod:FactorAnalysisExtractionMethod.PrincipalComponents,retentionMethod:FactorAnalysisRetentionMethod.Fixed,retentionValue:k,rotationMethod:(FactorAnalysisRotationMethod)Enum.Parse(typeof(FactorAnalysisRotationMethod),T(d,"rotation")));fitted.Calculate();return fitted;
  }
  var m=new FactorAnalysis();var pattern=Matrix(p,cols);var structure=new double[p,cols];for(int i=0;i<p;i++)for(int j=0;j<cols;j++){pattern[i,j]*=j%2==0?1:-1;structure[i,j]=profile=="orthogonal_signs"?pattern[i,j]:0.7*pattern[i,j]+0.2*pattern[i,(j+1)%cols];}
  var working=new double[p,p];for(int i=0;i<p;i++)working[i,i]=T(d,"trace")=="zero"?0:T(d,"trace")=="negative"?-1:1;
  if(T(d,"trace")=="NaN")working[0,0]=Double.NaN;if(T(d,"trace")=="+Inf")working[0,0]=Double.PositiveInfinity;if(T(d,"trace")=="missing")working=null;
  double[] eigen=new[]{3.5,1.5,0.75,0.25};if(T(d,"initial_eigen")=="missing")eigen=null;if(T(d,"initial_eigen")=="empty")eigen=new double[0];if(T(d,"initial_eigen")=="nonfinite")eigen=new[]{Double.NaN,Double.PositiveInfinity,-0.0,-1.0};
  switch(T(d,"matrices")){case "pattern_missing":pattern=null;break;case "structure_missing":structure=null;break;case "pattern_short_columns":pattern=Matrix(p,1);break;case "structure_short_rows":structure=Matrix(1,cols);break;case "structure_short_columns":structure=Matrix(p,1);break;}
  Put(m,"pNoFactors",k);Put(m,"pp",p);Put(m,"pPatternMatrix",pattern);Put(m,"pStructureMatrix",structure);Put(m,"pWorkingMatrix",working);Put(m,"pInitialEigenvalues",eigen);Put(m,"pVarNames",Labels(p,T(d,"variable_labels"),"var"));return m;
 }
 static CA Ca(Dictionary<string,object>d){
  var m=new CA();int k=I(d,"factors"),rows=I(d,"rows",B(d,"multiple")?4:3),cols=I(d,"columns",4);string state=T(d,"structures");
  var r=Matrix(rows,k,T(d,"bounds_profile")=="hidden_row_extremes"?100:1);var c=Matrix(cols,k,0.25);var rc=Matrix(rows,k);var cc=Matrix(cols,k);var rn=Labels(rows,"mixed","row");var cn=Labels(cols,"mixed","col");var vars=Labels(cols,"mixed","variable");
  switch(state){case "row_factors_missing":r=null;break;case "column_factors_missing":c=null;break;case "row_short_columns":r=Matrix(rows,Math.Min(1,k));break;case "column_short_columns":c=Matrix(cols,Math.Min(1,k));break;case "row_labels_missing":rn=null;break;case "column_labels_missing":cn=null;break;case "row_labels_short":rn=Labels(rows,"short","row");break;case "column_labels_short":cn=Labels(cols,"short","col");break;case "variable_labels_short":vars=Labels(cols,"short","var");break;case "row_contribution_missing":rc=null;break;case "column_contribution_missing":cc=null;break;case "contribution_short_columns":rc=Matrix(rows,0);cc=Matrix(cols,0);break;case "zero_contributors":rc=new double[rows,k];cc=new double[cols,k];break;}
  double[] eigen=new[]{5.0,2.0,0.75,0.125};if(state=="percent_missing")eigen=null;
  Put(m,"pRowFactors",r);Put(m,"pColFactors",c);Put(m,"pRowContribution",rc);Put(m,"pColContribution",cc);Put(m,"pRowNames",rn);Put(m,"pColNames",cn);Put(m,"pVarNamesToPresent",vars);Put(m,"pbMultiple",B(d,"multiple"));Put(m,"pDim",state=="percent_short"?Math.Min(1,k-1):k-1);Put(m,"pEigenvalues",eigen);Put(m,"pR",rows);Put(m,"pC",cols);return m;
 }
 static string Snapshot(object value){
  if(value==null)return "N";if(value is double)return "D"+BitConverter.DoubleToInt64Bits((double)value).ToString("X16",Invariant);if(value is string)return "S"+Convert.ToBase64String(Encoding.UTF8.GetBytes((string)value));
  var a=value as Array;if(a!=null)return a.GetType().GetElementType().FullName+"["+String.Join(",",Enumerable.Range(0,a.Rank).Select(i=>a.GetLowerBound(i).ToString(Invariant)+":"+a.GetLength(i).ToString(Invariant)))+"]{"+String.Join("|",a.Cast<object>().Select(Snapshot))+"}";
  return value.GetType().FullName+":"+Convert.ToString(value,Invariant);
 }
 static string Hash(string s){using(var h=System.Security.Cryptography.SHA256.Create())return String.Concat(h.ComputeHash(Encoding.UTF8.GetBytes(s)).Select(b=>b.ToString("x2")));}
 static string Error(Exception e){while(e is TargetInvocationException)e=e.InnerException;var a=e as ArgumentException;return e.GetType().FullName+":"+(a==null?"":a.ParamName)+":"+Convert.ToBase64String(Encoding.UTF8.GetBytes(e.Message));}
 static object Input(object model){if(model==null)return null;return model.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic).Where(f=>f.FieldType.IsArray||f.Name=="n"||f.Name=="p"||f.Name=="pp"||f.Name=="pNoExtractComponents"||f.Name=="pNoFactors"||f.Name=="pDim"||f.Name=="pbMultiple").OrderBy(f=>f.Name,StringComparer.Ordinal).Select(f=>(object)new object[]{f.Name,f.GetValue(model)}).ToArray();}
 static string Component(double[] pct,int i,bool score){
#if CURRENT
 return MultivariateChartPresentation37.ComponentCaption(pct[i],i,score);
#else
 return Text.Component(pct,i,score);
#endif
 }
 static string FactorCaption(string[] names,double[] pct,int i){
#if CURRENT
 return MultivariateChartPresentation37.FactorCaption(names[i],pct[i]);
#else
 return Text.Factor(names,pct,i);
#endif
 }
 static string Percent(double[] pct,int i){
#if CURRENT
 return MultivariateChartPresentation37.PercentLabel(pct[i]);
#else
 return Text.Percent(pct,i);
#endif
 }
 static string Variable(string[] names,int i){
#if CURRENT
 return names[i];
#else
 return Text.VariableLabel(names,i);
#endif
 }
 static object[] GroupSeries(double[] x,double[] y,string[] labels,string[] groups,string name,int[] rowIds){
#if CURRENT
 return MultivariateChartPresentation37.ScoreSeries(x,y,labels,groups,name,rowIds).Select(s=>(object)new object[]{s.Name,s.XValues,s.YValues,s.Labels,s.SourceOrdinals,s.ModelRowIds}).ToArray();
#else
 var shape=(object[])Prefix.Groups(x,y,labels,groups,name);var slots=Text.GroupIndices(groups,x.Length);return shape.Select((v,i)=>{var a=(object[])v;var ids=(int[])slots[i];int?[] modelIds=rowIds==null?null:ids.Select(j=>j>=0&&j<rowIds.Length?(int?)rowIds[j]:null).ToArray();return (object)new object[]{a[0],a[1],a[2],a[3],ids,modelIds};}).ToArray();
#endif
 }
 static void Arrows(double[] x,double[] y,string[] names,int count){phase="LOADING_SERIES_AND_POINT2_LABEL_AFTER_CHART";for(int i=0;i<count;i++){
#if CURRENT
  var a=MultivariatePlotGeometry37.LoadingVector(x[i]);var b=MultivariatePlotGeometry37.LoadingVector(y[i]);string name=MultivariateChartPresentation37.LoadingSeriesName(i);
#else
  var a=new[]{0.0,x[i]};var b=new[]{0.0,y[i]};string name=Text.LoadingName(i);
#endif
  output.Add(new object[]{name,a,b,2});output.Add(Variable(names,i));}}
 static void Zeros(object[] p,int start){phase="ZERO_REFERENCE_SERIES_AFTER_CHART";double xmin=(double)p[start],xmax=(double)p[start+1],ymin=(double)p[start+3],ymax=(double)p[start+4];
#if CURRENT
 output.Add(new object[]{MultivariateChartPresentation37.YZeroName,ChartReferenceSeries.Horizontal(xmin,xmax,0).XValues,MultivariatePlotGeometry37.ZeroPair()});output.Add(new object[]{MultivariateChartPresentation37.XZeroName,MultivariatePlotGeometry37.ZeroPair(),ChartReferenceSeries.Vertical(0,ymin,ymax).YValues});
#else
 output.Add(new object[]{"Y Zero Line",new[]{xmin,xmax},new[]{0,0}});output.Add(new object[]{"X Zero Line",new[]{0,0},new[]{ymin,ymax}});
#endif
 }
 static object[] Call(string name,params object[] a){try{return (object[])typeof(Prefix).GetMethod(name).Invoke(null,a);}catch(TargetInvocationException e){throw e.InnerException;}}
 static bool PcaPlan(PCA model,Dictionary<string,object>d){
  string op=T(d,"operation");int n=model==null?0:model.ObservationCount;var labels=Labels(n,T(d,"row_labels"),"row");var groups=Groups(n,T(d,"groups"));double c=Number(T(d,"scale","1"));
  phase="PRE_CHART_PUBLIC_GUARD_OR_NUMERIC";object[] p=op=="Biplot"?Call("PcaBiplot",model,c,labels,groups):op.StartsWith("Score")?Call("Pca"+op,model,labels,groups):Call("Pca"+op,model);
  if(p==null)return false;prefixComplete=true;output.Add(p);
  bool score=op.StartsWith("Score"),three=op.EndsWith("3D");
  if(op=="ScreePlot"){phase="SCREE_POINT_LABELS_AFTER_CHART";for(int i=0;i<model.VariableCount;i++)output.Add(new object[]{i+1,Percent((double[])p[1],i)});output.Add(new[]{"Scree Plot","Principal Component","Variance explained [%]","Percent Explained"});return true;}
  if(three){phase="XYZ_AXIS_CAPTIONS_AFTER_DATA_INPUT_BOUNDARY";var pct=(double[])p[score?5:4];for(int i=0;i<3;i++)output.Add(Component(pct,i,score));phase="XYZ_LABEL_GROUP_ARGUMENT_BOUNDARY";output.Add(p[3]);if(score)output.Add(p[4]);return true;}
  if(op=="Biplot"){
   phase="SCORE_SERIES_AFTER_CHART_AND_AXES";var series=GroupSeries((double[])p[0],(double[])p[1],(string[])p[4],(string[])p[5],"Biplot: "+(string)p[7],model.RowIds);output.Add(series);
   Arrows((double[])p[2],(double[])p[3],model.VariableNames,model.VariableCount);Zeros(p,8);
#if CURRENT
   output.Add(MultivariateChartPresentation37.BiplotName(c));output.Add(MultivariateChartPresentation37.ShowScoreLegend(p[5]==null?0:series.Length));
#else
   output.Add(Text.BiplotName(c));output.Add(p[5]!=null&&series.Length>1);
#endif
   phase="AXIS_CAPTIONS_AFTER_SERIES";output.Add(Component((double[])p[6],0,false));output.Add(Component((double[])p[6],1,false));output.Add("Biplot: "+(string)p[7]);return true;
  }
  if(score){phase="SCORE_SERIES_AFTER_CHART_AND_AXES";var series=GroupSeries((double[])p[0],(double[])p[1],(string[])p[2],(string[])p[3],"Score plot",model.RowIds);output.Add(series);Zeros(p,5);output.Add(p[3]!=null&&series.Length>1);}
  else{Arrows((double[])p[0],(double[])p[1],(string[])p[2],model.VariableCount);Zeros(p,4);}
  phase="AXIS_CAPTIONS_AFTER_SERIES";var percentages=(double[])p[score?4:3];output.Add(Component(percentages,0,score));output.Add(Component(percentages,1,false));return true;
 }
 static bool FactorPlan(FactorAnalysis model,Dictionary<string,object>d){
  string op=T(d,"operation");phase="PRE_CHART_PUBLIC_GUARD_OR_NUMERIC";var p=Call("Factor"+op,model);if(p==null)return false;prefixComplete=true;output.Add(p);
  if(op=="ScreePlot"){phase="SCREE_POINT_LABELS_AFTER_CHART";var pct=(double[])p[1];for(int i=0;i<model.InitialEigenvalues.Length;i++)output.Add(new object[]{i+1,Percent(pct,i)});output.Add(new[]{"Scree Plot","Factor","Variance explained [%]","Initial Variance Explained"});return true;}
  if(op.EndsWith("3D")){phase="XYZ_AXIS_CAPTIONS_AFTER_DATA_INPUT_BOUNDARY";for(int i=0;i<3;i++)output.Add(FactorCaption((string[])p[5],(double[])p[4],i));phase="XYZ_LABEL_ARGUMENT_BOUNDARY";output.Add(p[3]);return true;}
  Arrows((double[])p[0],(double[])p[1],(string[])p[2],model.VariableCount);Zeros(p,5);phase="AXIS_CAPTIONS_AFTER_SERIES";output.Add(FactorCaption((string[])p[4],(double[])p[3],1));output.Add(FactorCaption((string[])p[4],(double[])p[3],0));return true;
 }
 static void CaPoint(CA m,bool row){phase=row?"ROW_SERIES_AFTER_CHART":"COLUMN_SERIES_AFTER_CHART";
#if CURRENT
  var s=MultivariateChartPresentation37.CorrespondenceSeries(m,row);output.Add(new object[]{s.Name,s.XValues,s.YValues});
#else
  output.Add(new object[]{row?"Rows":"Columns",row?m.get_RowFactors(0):m.get_ColFactors(0),row?m.get_RowFactors(1):m.get_ColFactors(1)});
#endif
  phase=row?"ROW_POINT_LABEL_AFTER_SERIES":"COLUMN_POINT_LABEL_AFTER_SERIES";int count=row?m.rowNames.Length:m.ColumNames.Length;for(int i=0;i<count;i++){
#if CURRENT
  output.Add(new object[]{i+1,MultivariateChartPresentation37.CorrespondenceLabel(m,row,i)});
#else
  output.Add(new object[]{i+1,m.PlotLabels(row,null)[i]});
#endif
  }}
 static bool CaPlan(CA model,Dictionary<string,object>d){
  bool row=B(d,"row_side");int axis=I(d,"axis");phase="PRE_CHART_PUBLIC_GUARD_OR_NUMERIC";
  if(T(d,"operation")=="ContributionPlot"){
   object values;
#if CURRENT
   values=MultivariatePlotGeometry37.Contribution(model,axis,row);
#else
   if(model==null)throw new ArgumentNullException("model");values=row?model.get_RowContribution(axis):model.get_ColContribution(axis);
#endif
   prefixComplete=true;output.Add(values);phase="CONTRIBUTION_TITLE_AFTER_CHART";
#if CURRENT
   output.Add(MultivariateChartPresentation37.ContributionTitle(axis));if(model.IsMultiple)output.Add(MultivariateChartPresentation37.ContributionName(axis));
#else
   output.Add(Text.ContributionTitle(axis));if(model.IsMultiple)output.Add(Text.ContributionName(axis));
#endif
   phase="CONTRIBUTION_CATEGORIES_AFTER_CHART";
#if CURRENT
   output.Add(MultivariateChartPresentation37.ContributionLabels(model,row));
#else
   output.Add(model.PlotLabels(row,"\r\n"));
#endif
   output.Add(new object[]{"Contribution",false,"column-chart-source-intent"});return true;
  }
  var p=Call("CaBounds",model);if(p==null)return false;prefixComplete=true;output.Add(p);CaPoint(model,false);if(!model.IsMultiple)CaPoint(model,true);Zeros(p,0);output.Add(!model.IsMultiple);
  phase="CA_LATE_PERCENT_ACQUISITION_CAUGHT";
  try{var pct=model.Percents;phase="CA_Y_CAPTION_CAUGHT";
#if CURRENT
   output.Add(MultivariateChartPresentation37.CorrespondenceCaption(pct[2,1],2));phase="CA_X_CAPTION_CAUGHT";output.Add(MultivariateChartPresentation37.CorrespondenceCaption(pct[1,1],1));
#else
   output.Add(Text.Correspondence(pct,2));phase="CA_X_CAPTION_CAUGHT";output.Add(Text.Correspondence(pct,1));
#endif
   output.Add("Correspondence Plot");}catch(Exception e){output.Add(new object[]{"CAUGHT_ORIGINAL_LATE_TRY",phase,Error(e)});}
  return true;
 }
 static bool Finite(object a){if(a is double)return !Double.IsNaN((double)a)&&!Double.IsInfinity((double)a);var ar=a as Array;return ar==null||ar.Cast<object>().All(Finite);}
 public static int Main(string[] args){
  string cases=args[0],dest=args[1],receipt=args[2];if(File.Exists(dest)||File.Exists(receipt))throw new InvalidOperationException("Append-only capture exists");
  var core=typeof(PCA).Assembly;
#if CURRENT
  if(core.GetType("BESHStatNG.MultivariatePlotGeometry37")==null)throw new Exception("Missing current preparation");
#else
  if(core.GetType("BESHStatNG.MultivariatePlotGeometry37")!=null)throw new Exception("Original oracle loaded current Core");
  if(AppDomain.CurrentDomain.GetAssemblies().Any(a=>a.GetName().Name=="BESHStat.Excel.Common"))throw new Exception("Original oracle may not load Common");
#endif
  var serializer=new JavaScriptSerializer();serializer.MaxJsonLength=int.MaxValue;int count=0,guards=0,geometries=0,exceptions=0;var started=DateTime.UtcNow;
  using(var writer=new StreamWriter(dest,false,new UTF8Encoding(false))){writer.NewLine="\n";writer.WriteLine("case_id\tinput_sha256\tdispatch\tphase\tprefix_completed\tusable_numeric_prefix\tresult\terror");
   foreach(string line in File.ReadLines(cases,Encoding.UTF8)){
    var d=serializer.Deserialize<Dictionary<string,object>>(line);CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(T(d,"culture"));phase="INPUT_CONSTRUCTION";prefixComplete=false;output=new List<object>();string error="None",dispatch="EXCEPTION",input="";bool usable=false;
    try{object model=null;if(T(d,"operation")!="null_model")model=T(d,"family")=="PCA"?(object)Pca(d):T(d,"family")=="FACTOR"?Factor(d):(object)Ca(d);input=Snapshot(Input(model));
     bool rendered;
     if(T(d,"operation")=="null_model"){phase="PRE_CHART_PUBLIC_GUARD_OR_NUMERIC";if(T(d,"family")=="PCA")Call("PcaScreePlot",new object[]{null});else if(T(d,"family")=="FACTOR")Call("FactorScreePlot",new object[]{null});else Call("CaBounds",new object[]{null});rendered=false;}
     else rendered=T(d,"family")=="PCA"?PcaPlan((PCA)model,d):T(d,"family")=="FACTOR"?FactorPlan((FactorAnalysis)model,d):CaPlan((CA)model,d);
     dispatch=rendered?"GEOMETRY":"GUARD";usable=prefixComplete&&output.Count>0&&Finite(output[0]);phase=rendered?phase:"PUBLIC_DIMENSION_OR_EMPTY_PROFILE_GUARD";
    }catch(Exception e){error=Error(e);dispatch="EXCEPTION";}
    if(dispatch=="GUARD")guards++;else if(dispatch=="GEOMETRY")geometries++;else exceptions++;
    writer.WriteLine(String.Join("\t",new[]{T(d,"case_id"),Hash(line+"\n"+input),dispatch,phase,prefixComplete?"true":"false",usable?"true":"false",Snapshot(output.ToArray()),error}));count++;
   }
  }
  using(var w=new StreamWriter(receipt,false,new UTF8Encoding(false)))w.Write(serializer.Serialize(new{started_utc=started.ToString("o",Invariant),completed_utc=DateTime.UtcNow.ToString("o",Invariant),count,guards,geometries,exceptions,core_name=core.GetName().Name,core_mvid=core.ManifestModule.ModuleVersionId.ToString(),provenance="CAPTURED_AFTER_INITIAL_IMPLEMENTATION_FROM_PINNED_ARCHIVED_ENTRY",native="NOT_RUN",scientific="NOT_GOLDEN"}));
  Console.WriteLine("Captured "+count+" cases; GUARD="+guards+" GEOMETRY="+geometries+" EXCEPTION="+exceptions+"; no native execution");return 0;
 }
}
