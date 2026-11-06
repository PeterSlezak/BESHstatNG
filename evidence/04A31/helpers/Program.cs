using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

const string Base = "f840b1695fd95be25faba6fd5408c9498a31d804";
var root = Directory.GetCurrentDirectory();
string Git(params string[] arguments) {
    var info = new ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
    info.ArgumentList.Add("-c");info.ArgumentList.Add("safe.directory=" + root.Replace('\\','/'));
    foreach(var a in arguments)info.ArgumentList.Add(a);
    using var p=Process.Start(info)!;
    var text=p.StandardOutput.ReadToEnd();var error=p.StandardError.ReadToEnd();p.WaitForExit();
    if(p.ExitCode!=0)throw new Exception(error.Replace(root,"<REPO_ROOT>"));
    return text;
}
string Sha(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
string Tokens(SyntaxNode n)=>string.Join(" ",n.DescendantTokens().Select(t=>t.Text));
SyntaxNode Parse(string path,string text) {
    var t=VisualBasicSyntaxTree.ParseText(text,VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest),path);
    if(t.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error))throw new Exception("VB parse error: "+path);
    return t.GetRoot();
}
string TypeName(SyntaxNode n)=>string.Join(".",n.Ancestors().Reverse().Select(a=>a switch { NamespaceBlockSyntax b=>b.NamespaceStatement.Name.ToString(),ClassBlockSyntax c=>c.ClassStatement.Identifier.Text,ModuleBlockSyntax m=>m.ModuleStatement.Identifier.Text,_=>"" }).Where(x=>x.Length>0));
SyntaxNode Header(SyntaxNode n)=>n switch { MethodBlockSyntax b=>b.SubOrFunctionStatement,ConstructorBlockSyntax b=>b.SubNewStatement,PropertyBlockSyntax b=>b.PropertyStatement,_=>n };
string Key(SyntaxNode n)=>TypeName(n)+"|"+Tokens(Header(n));
IEnumerable<SyntaxNode> Members(SyntaxNode n)=>n.DescendantNodes().Where(x=>x is MethodBlockSyntax or ConstructorBlockSyntax or PropertyBlockSyntax);
void Require(bool condition,string message) {if(!condition)throw new Exception(message);}

if(args.Contains("--results-snapshot")) {
    var clock=Stopwatch.StartNew();var metadata=new List<string>();long bytes=0;
    void Add(string directory) {
        foreach(var path in Directory.EnumerateFiles(directory,"*",SearchOption.AllDirectories)) {
            var f=new FileInfo(path);var relative=Path.GetRelativePath(root,path).Replace('\\','/');
            metadata.Add(relative+"|"+f.Length+"|"+f.LastWriteTimeUtc.Ticks);bytes+=f.Length;
        }
    }
    foreach(var d in Directory.EnumerateDirectories(root).Where(d=>!Path.GetFileName(d).EndsWith(".tmp")&&Path.GetFileName(d)!=".git")) {
        if(Path.GetFileName(d).Equals("TestResults",StringComparison.OrdinalIgnoreCase))Add(d);
        else foreach(var nested in Directory.EnumerateDirectories(d,"TestResults",SearchOption.AllDirectories))Add(nested);
    }
    metadata.Sort(StringComparer.Ordinal);var digest=Sha(string.Join('\n',metadata));
    using var prior=JsonDocument.Parse(File.ReadAllText("evidence/04A29/historical-results-entry.json"));var p=prior.RootElement;
    var equal=p.GetProperty("count").GetInt32()==metadata.Count&&p.GetProperty("bytes").GetInt64()==bytes&&p.GetProperty("membership_size_mtime_sha256").GetString()==digest;
    var metadataReceipt=new {schema="04A31-historical-stat-v1",status=equal?"MATCH_ACCEPTED_METADATA_RECEIPT":"METADATA_DELTA_REQUIRES_QUALIFICATION",count=metadata.Count,bytes,membership_size_mtime_sha256=digest,reference="evidence/04A29/historical-results-entry.json",qualification="One scoped membership/size/mtime traversal; payload bytes NOT_REHASHED. Ignored isolated *.tmp outputs excluded.",seconds=clock.Elapsed.TotalSeconds};
    File.WriteAllText("evidence/04A31/historical-results-preservation.json",JsonSerializer.Serialize(metadataReceipt,new JsonSerializerOptions{WriteIndented=true})+"\n");
    Require(equal,"Historical metadata changed");Console.WriteLine("PASS historical membership/size/mtime; payload NOT_REHASHED");return;
}

var rows=new List<object>();
var allowed=new Dictionary<string,string[]> {
    ["ROC.vb"]=["compute","addROCplot"],
    ["NormalPlot.vb"]=["New","compute","computeRefLIne","addChart"]
};
foreach(var pair in allowed) {
    var path="BESHstatNG/src/Graphics/"+pair.Key;
    var oldText=Git("show",Base+":"+path);var newText=File.ReadAllText(path);
    var oldTree=Parse(path,oldText);var newTree=Parse(path,newText);
    var newer=Members(newTree).ToDictionary(Key);
    foreach(var old in Members(oldTree)) {
        Require(newer.TryGetValue(Key(old),out var current),"Existing declaration changed: "+Key(old));
        var name=old is ConstructorBlockSyntax ? "New" : old is MethodBlockSyntax b ? b.SubOrFunctionStatement.Identifier.Text : "property";
        var same=Tokens(old)==Tokens(current!);
        Require(same||pair.Value.Contains(name),"Unapproved member change: "+Key(old));
        rows.Add(new {path,member=Key(old),header_preserved=true,body_preserved=same,entry_token_sha256=Sha(Tokens(old)),final_token_sha256=Sha(Tokens(current!)),entry_start=old.GetLocation().GetLineSpan().StartLinePosition.Line+1,entry_end=old.GetLocation().GetLineSpan().EndLinePosition.Line+1,final_start=current!.GetLocation().GetLineSpan().StartLinePosition.Line+1,final_end=current.GetLocation().GetLineSpan().EndLinePosition.Line+1});
    }
    var renderer=pair.Key=="ROC.vb"?"addROCplot":"addChart";
    var oldRender=Members(oldTree).OfType<MethodBlockSyntax>().Single(m=>m.SubOrFunctionStatement.Identifier.Text==renderer).ToString();
    var currentRender=Members(newTree).OfType<MethodBlockSyntax>().Single(m=>m.SubOrFunctionStatement.Identifier.Text==renderer);
    if(pair.Key=="ROC.vb") {
        oldRender=oldRender.Replace("Public Sub addROCplot(ws As Worksheet)","Public Sub addROCplot(ws As Worksheet)\n            Dim series = PlotSeries()")
            .Replace(".XValues = par1minusSpec",".XValues = series.X").Replace(".Values = parSensitivity",".Values = series.Y")
            .Replace(".XValues = {0, 1}",".XValues = series.ReferenceX").Replace(".Values = {0, 1}",".Values = series.ReferenceY");
    } else {
        oldRender=oldRender.Replace("Public Sub addChart(ws As Worksheet)","Public Sub addChart(ws As Worksheet)\n            Dim series = PlotSeries()")
            .Replace("GeneralScatterPlot(pData, pZ,","GeneralScatterPlot(series.X, series.Y,")
            .Replace("Matrix.GetColumnFrom2Darray(pRefLine, 0)","series.ReferenceX").Replace("Matrix.GetColumnFrom2Darray(pRefLine, 1)","series.ReferenceY");
    }
    var expectedRender=Parse("expected.vb",oldRender).DescendantNodes().OfType<MethodBlockSyntax>().Single();
    Require(Tokens(expectedRender)==Tokens(currentRender),"Renderer has edits beyond coordinate substitutions: "+path);
}
var corePaths=new[]{"BESHStat.Core/src/Graphics/RocPlotSeriesData.vb","BESHStat.Core/src/Graphics/NormalPlotSeriesData.vb"};
foreach(var path in corePaths) {
    var text=File.ReadAllText(path);var tree=Parse(path,text);var tokens=Tokens(tree);
    Require(text.Contains("Option Strict On"),"Strict missing");
    foreach(var forbidden in new[]{"ExcelDna","Interop","WinForms","Office.js","Microsoft.VisualBasic","System.Drawing","System.IO","Clipboard","Worksheet","Workbook","COMObject"})
        Require(!tokens.Contains(forbidden,StringComparison.OrdinalIgnoreCase),"Portable dependency: "+forbidden);
}
foreach(var path in new[]{"BESHStat.Core/BESHStat.Core.vbproj","BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"})
    Require(XDocument.Load(path).Descendants("TargetFramework").Single().Value=="netstandard2.0","Framework changed");
var project=XDocument.Load("BESHstatNG/BESHstatNG.vbproj");
var compiled=project.Descendants().Where(n=>n.Name.LocalName=="Compile").Select(n=>(string?)n.Attribute("Include")).Where(s=>s!=null&&s.StartsWith("src\\Graphics\\")).ToArray();
Require(compiled.Length==38,"Windows graphics compilation membership changed");
Require(new[]{"ROC.vb","NormalPlot.vb"}.All(file=>compiled.Contains("src\\Graphics\\"+file)),"Adapter not compiled");
// Independently exercise both inverse getters in the actual compiled portable
// assembly. Expected identity is specified by hand;this is metadata,not an oracle.
var assemblyPath=Path.Combine(root,".test-rationalization-04A31.tmp/builds/verified/Debug/portable/BESHStat.Core.dll");
var assembly=System.Reflection.Assembly.LoadFrom(assemblyPath);
var orderType=assembly.GetType("BESHStatNG.NormalPlotObservationOrder",true)!;
var order=Activator.CreateInstance(orderType,[new double[]{4,1,4,2,1}])!;
var inverseProperty=orderType.GetProperty("SourceToSorted")!;
var inverse=(int[])inverseProperty.GetValue(order)!;inverse[0]=999;
Require(((int[])inverseProperty.GetValue(order)!).SequenceEqual(new[]{3,0,4,2,1}),"Order inverse getter alias");
var seriesType=assembly.GetType("BESHStatNG.NormalPlotSeriesData",true)!;
var series=seriesType.GetMethod("Compute")!.Invoke(null,[new double[]{4,1,4,2,1},"Blom","OLS"])!;
var seriesInverse=seriesType.GetProperty("SourceToSorted")!;
var changedInverse=(int[])seriesInverse.GetValue(series)!;changedInverse[0]=999;
Require(((int[])seriesInverse.GetValue(series)!).SequenceEqual(new[]{3,0,4,2,1}),"Series inverse getter alias");
using var assemblyFile=File.OpenRead(assemblyPath);
var assemblySha=Convert.ToHexString(SHA256.HashData(assemblyFile)).ToLowerInvariant();
var result=new {schema="04A31-source-ownership-v1",status="PASS",comparison="Roslyn tokens against actual merged main;source evidence,no native executed-call claim",existing_headers_preserved=true,renderers_only_coordinate_substitutions=true,compiled_graphics=compiled.Length,portable_targets="netstandard2.0",portable_dependency_guard="PASS",runtime_inverse_copy_checks=new {status="HOST_FREE_EXECUTED_PASS",checks=2,assembly_sha256=assemblySha,expected=new[]{3,0,4,2,1},authority="Hand-specified metadata expectation;compiled portable API;NOT_GOLDEN"},members=rows};
File.WriteAllText("evidence/04A31/source-ownership.json",JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine("PASS existing declarations, bounded renderer substitutions, compiled membership and portable dependencies");
