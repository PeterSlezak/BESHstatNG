"""Generate reproducible final captures; frozen pre-edit files are never rewritten."""
from work import *
prefix='''
 static object InvokeTarget(MethodInfo method,object[] args,out string error){
  error="None";try{return method.Invoke(null,args);}catch(TargetInvocationException e){var a=e.InnerException as ArgumentException;error=e.InnerException.GetType().FullName+":"+(a==null?"":a.ParamName);return null;}
 }
 static string ProjectSnapshot(object value,Type template){
  if(value==null)return "Nothing";
  if(value is double)return BitConverter.DoubleToInt64Bits((double)value).ToString("X16",CultureInfo.InvariantCulture);
  if(value is bool||value is int||value is string)return Convert.ToString(value,CultureInfo.InvariantCulture);
  if(value is Array)return "["+String.Join("|",((Array)value).Cast<object>().Select(v=>ProjectSnapshot(v,v==null?typeof(object):v.GetType())))+"]";
  return String.Join("|",template.GetProperties().OrderBy(p=>p.Name,StringComparer.Ordinal).Select(p=>p.Name+"="+ProjectSnapshot(value.GetType().GetProperty(p.Name).GetValue(value,null),p.PropertyType)));
 }
'''
files=[]
for section,name,output in [('Shapes','CaptureShapes','geometry'),('Extras','CaptureExtras','extra-geometry'),('Preparation','CapturePreparation','preparation'),('Series','CaptureSeries','series'),('Policies','CapturePolicies','policies')]:
    s=(HERE/(name+'.cs')).read_text(encoding='utf-8');s=s.replace('public static void Run(string root)','public static void Run(string root,string config)')
    s=s.replace('public static class Completion','public static class PostCompletion')
    s=s.replace('.test-rationalization-04A34.tmp/builds/final/Debug/legacy','.test-rationalization-04A34.tmp/builds/completed/CONFIG/legacy').replace('Path.Combine(root,".test-rationalization-04A34.tmp/builds/completed/CONFIG/legacy','Path.Combine(root,".test-rationalization-04A34.tmp/builds/completed/"+config+"/legacy')
    s=s.replace('"evidence/04A34/completion/pre-edit-'+output+'.csv"','".test-rationalization-04A34.tmp/completion-post-"+config+"-'+output+'.csv"')
    s=s.replace(' static string Q(',prefix+' static string Q(')
    # All three actual production assemblies are loaded from the isolated final build.
    insert='''  var finalFolder=Path.Combine(root,".test-rationalization-04A34.tmp/builds/completed/"+config+"/legacy");
  var finalCore=Assembly.LoadFrom(Path.Combine(finalFolder,"BESHStat.Core.dll"));
  var finalCommon=Assembly.LoadFrom(Path.Combine(finalFolder,"BESHStat.Excel.Common.dll"));
  var finalHost=Assembly.LoadFrom(Path.Combine(finalFolder,"BESHstatNG.dll"));
'''
    s=s.replace(' public static void Run(string root,string config) {',' public static void Run(string root,string config) {\n'+insert)
    if section in ['Shapes','Extras']:
        s=s.replace('object[] values={method,args,null};var result=invoke.Invoke(null,values);','var target=finalCore.GetType("BESHStatNG."+family+"MeasuredGeometryCore",true).GetMethod(family+method.Name); string error;var result=InvokeTarget(target,args,out error);')
        s=s.replace('snapshot.Invoke(null,new object[]{result}),values[2]','ProjectSnapshot(result,method.ReturnType),error')
    elif section=='Preparation':
        lookup='''var names=new Dictionary<string,string>{{"Bullet","BulletChartExcel"},{"Sankey","SankeyPlotExcel"},{"Cdf","CumulativeDistributionPlotExcel"},{"Dumbbell","DumbbellPlotExcel"},{"Ladder","LadderPlotExcel"},{"Hull","ConvexHullPlotExcel"},{"Polar","PolarPlotExcel"},{"Spc","graphics.SpcControlChartExcel"}};
     var owner=finalHost.GetType("BESHStatNG."+((family=="Spc"&&(method.Name=="NormalizeText"||method.Name=="NormalizeNumberFormat"))?"graphics.SpcControlChartAppearanceOptions":names[family]),true);
     var target=owner.GetMethod(method.Name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
'''
        s=s.replace('try{result=method.Invoke(null,args);}',lookup+'     try{result=target.Invoke(null,args);}')
    elif section=='Series':
        s=s.replace('try{result=method.Invoke(null,args);}','var target=finalCommon.GetType("BESHStatNG.ExcelCommon.Chart34."+family+"Series."+family+"SeriesPreparation34",true).GetMethod(method.Name);\n     try{result=target.Invoke(null,args);}')
    else:
        s=s.replace('try{result=method.Invoke(null,args);}','var target=(method.Name=="PolarSquare"?finalCore.GetType("BESHStatNG.PolarMeasuredLayoutCore",true):finalCommon.GetType("BESHStatNG.ExcelCommon.Chart34.SpecialtyLegendPolicy34",true)).GetMethod(method.Name);\n     try{result=target.Invoke(null,args);}')
    s=s.replace('builds/completed/','builds/review3/').replace('completion-post-','completion-review3-post-')
    path='Post'+name+'.cs';write('evidence/04A34/completion/'+path,s);files.append((path,'PostCompletion'+section+'34Capture'))
# Original 544-row corpus uses real final Windows Ladder/ECDF/GIF methods,
# with the independently frozen original source slices as immutable side rows.
s=(P:=HERE.parent/'CapturePost.cs').read_text(encoding='utf-8').replace('Specialty34PostCapture','Specialty34FinalCapture').replace('Run(string root)','Run(string root,string config)')
s=s.replace('builds/final/Debug/legacy','builds/completed/"+config+"/legacy').replace('evidence/04A34/post-edit-compatibility-capture.csv','.test-rationalization-04A34.tmp/completion-post-"+config+"-compatibility.csv')
s=s.replace('builds/completed/','builds/review3/').replace('completion-post-','completion-review3-post-')
write('evidence/04A34/completion/PostCaptureOriginal.cs',s);files.append(('PostCaptureOriginal.cs','Specialty34FinalCapture'))
ps='''param([ValidateSet('Debug','Release')][string]$Configuration34)
$ErrorActionPreference='Stop'
'''
for file,klass in files:ps+='Add-Type -Path evidence/04A34/completion/'+file+'\n['+klass+']::Run((Get-Location).Path,$Configuration34)\n'
write('evidence/04A34/completion/capture-final.ps1',ps)
spc=(HERE/'capture-spc-basis.ps1').read_text(encoding='utf-8-sig')
spc="param([ValidateSet('Debug','Release')][string]$Configuration34)\n"+spc[spc.index('$ErrorActionPreference'):]
spc=spc[0:spc.index('$compilerLocator34')]+spc[spc.index('$fixture34='):]
spc=spc.replace("'.test-rationalization-04A34.tmp/builds/final/Debug/legacy/BESHStat.Core.dll'","('.test-rationalization-04A34.tmp/builds/completed/'+$Configuration34+'/legacy/BESHStat.Core.dll')")
spc=spc.replace("$module34=$fixture34.GetType('Batch34CompletionFixtures.LegacySpcBasis34')","$module34=([Reflection.Assembly]::LoadFrom((Join-Path (Get-Location) ('.test-rationalization-04A34.tmp/builds/completed/'+$Configuration34+'/legacy/BESHStat.Core.dll')))).GetType('BESHStatNG.SpcAxisBoundsCore')")
spc=spc.replace("$module34.GetMethods([Reflection.BindingFlags]'Public,Static')","($module34.GetMethods([Reflection.BindingFlags]'Public,Static') | Where-Object { $_.Name -in @('SpcOptionalExtent','SpcConstantReference','SpcPanelTop') })")
spc=spc.replace("'evidence/04A34/completion/pre-edit-spc-basis.csv'","('.test-rationalization-04A34.tmp/completion-post-'+$Configuration34+'-spc-basis.csv')")
spc=spc.replace('builds/completed/','builds/review3/').replace('completion-post-','completion-review3-post-')
write('evidence/04A34/completion/capture-final-spc.ps1',spc)
# Public script paths use installed framework locations supplied by the host.
for p in HERE.glob('capture-*.ps1'):
    s=p.read_text(encoding='utf-8-sig')
    s=re.sub(r"/reference:'[^']*System.Core.dll'",lambda m:"/reference:$([System.Linq.Enumerable].Assembly.Location)",s)
    s=re.sub(r"/reference:'[^']*Facades[\\/]netstandard.dll'",lambda m:"/reference:$(Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')",s)
    write(p.relative_to(ROOT),s)
print('Generated final capture consumers; oracle CSVs untouched')
