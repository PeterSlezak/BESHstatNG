using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
const string Base="43d20f86b80934c49b2f3dec90824aa62af5aaee";
var root=Directory.GetCurrentDirectory();
string Git(params string[] arguments) {
 var info=new ProcessStartInfo("git"){RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
 info.ArgumentList.Add("-c");info.ArgumentList.Add("safe.directory="+root.Replace('\\','/'));
 foreach(var a in arguments)info.ArgumentList.Add(a);
 using var p=Process.Start(info)!;var output=p.StandardOutput.ReadToEnd();var error=p.StandardError.ReadToEnd();p.WaitForExit();
 if(p.ExitCode!=0)throw new Exception(error.Replace(root,"<REPO_ROOT>"));return output;
}
var rows=new List<object>();
var allowed=new Dictionary<string,string[]> {
 ["XYZscatter.vb"]=["normalizedData","cut_planes","cage_data","tick_marks_data","get_gridlines","tick_marks_prerequisites","get_rotations","comp_zoom_inter","comp_x_shift_inter","comp_y_shift_inter","ProjectRawPoint"],
 ["ScatterPlotMatrix.vb"]=["transformData","compute"],
 ["3Dobjects.vb"]=["Draw","GetRawBounds"]
};
foreach(var file in allowed.Keys) {
 string path="BESHstatNG/src/Graphics/"+file;
 var old=Parse(path,Git("show",Base+":"+path));var newer=Parse(path,File.ReadAllText(path));var by=Members(newer).ToDictionary(Key);
 foreach(var m in Members(old)) {
  Require(by.TryGetValue(Key(m),out var n),"Existing declaration changed:"+Key(m));
  var name=m is MethodBlockSyntax method?method.SubOrFunctionStatement.Identifier.Text:m is ConstructorBlockSyntax?"New":"property";
  bool same=Tokens(m)==Tokens(n!);Require(same||allowed[file].Contains(name),"Unapproved member delta:"+Key(m));
  rows.Add(new {path,member=Key(m),name,entry_start=m.GetLocation().GetLineSpan().StartLinePosition.Line+1,entry_end=m.GetLocation().GetLineSpan().EndLinePosition.Line+1,final_start=n!.GetLocation().GetLineSpan().StartLinePosition.Line+1,final_end=n.GetLocation().GetLineSpan().EndLinePosition.Line+1,entry_token_sha256=Sha(Tokens(m)),final_token_sha256=Sha(Tokens(n)),header_preserved=true,body_preserved=same});
 }
 if(file=="XYZscatter.vb") {
  string text=File.ReadAllText(path);
  Require(!text.Contains("Math.Sin")&&!text.Contains("Math.Cos")&&!text.Contains("Radians("),"Parallel rotation engine remains");
  foreach(var call in new[]{"ProjectNormalized","CutPlanes","Cage","Ticks","GridLines","TickAxis"})Require(text.Contains("XyzGeometryCore."+call+"("),"Missing delegate:"+call);
 }
 if(file=="3Dobjects.vb")foreach(var call in new[]{"Sphere","Ellipsoid","SphereBounds","EllipsoidBounds"})Require(File.ReadAllText(path).Contains("WireObjectGeometryCore."+call+"("),"Missing wire delegate:"+call);
 if(file=="ScatterPlotMatrix.vb")foreach(var call in new[]{"PanelCoordinates","Correlations","GridPosition","PanelValues"})Require(File.ReadAllText(path).Contains("ScatterMatrixSeriesCore."+call+"("),"Missing panel delegate:"+call);
}
foreach(var name in new[]{"XyzGeometryCore","XyzProjectionContext","WireObjectGeometryCore","ScatterMatrixSeriesCore","ScatterMatrixPanelData"}) {
 string path="BESHStat.Core/src/Graphics/"+name+".vb";var text=File.ReadAllText(path);var tokens=Tokens(Parse(path,text));Require(text.Contains("Option Strict On"),"Strict missing");
 foreach(var forbidden in new[]{"ExcelDna","Interop","WinForms","Office.js","Microsoft.VisualBasic","System.Drawing","System.IO","Clipboard","Worksheet","Workbook","COMObject"})Require(!tokens.Contains(forbidden,StringComparison.OrdinalIgnoreCase),"Portable dependency:"+forbidden);
}
foreach(var path in new[]{"BESHStat.Core/BESHStat.Core.vbproj","BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"})Require(XDocument.Load(path).Descendants("TargetFramework").Single().Value=="netstandard2.0","Portable target changed");
var compiled=XDocument.Load("BESHstatNG/BESHstatNG.vbproj").Descendants().Where(n=>n.Name.LocalName=="Compile").Select(n=>(string?)n.Attribute("Include")).Where(s=>s!=null&&s.StartsWith("src\\Graphics\\")).ToArray();Require(compiled.Length==38,"Graphics membership changed");
var result=new {schema="04A33-source-ownership-v1",status="PASS",comparison="Roslyn declarations/tokens versus merged-main;not native execution",existing_headers_preserved=true,compiled_graphics=38,portable_dependency_guard="PASS",members=rows};
File.WriteAllText("evidence/04A33/source-ownership.json",JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+"\n");Console.WriteLine("PASS member/API/delegation/native boundary and portable dependency guards");
string Sha(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
string Tokens(SyntaxNode node)=>string.Join(" ",node.DescendantTokens().Select(t=>t.Text));
void Require(bool b,string message){if(!b)throw new Exception(message);}
SyntaxNode Parse(string path,string text){var t=VisualBasicSyntaxTree.ParseText(text,VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest),path);Require(!t.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error),"Parse error:"+path+" "+string.Join(";",t.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).Select(d=>d.ToString())));return t.GetRoot();}
string TypeName(SyntaxNode node)=>string.Join(".",node.Ancestors().Reverse().Select(n=>n switch {NamespaceBlockSyntax a=>a.NamespaceStatement.Name.ToString(),ClassBlockSyntax a=>a.ClassStatement.Identifier.Text,ModuleBlockSyntax a=>a.ModuleStatement.Identifier.Text,_=>""}).Where(x=>x.Length>0));
SyntaxNode Header(SyntaxNode n)=>n switch {MethodBlockSyntax m=>m.SubOrFunctionStatement,ConstructorBlockSyntax c=>c.SubNewStatement,PropertyBlockSyntax p=>p.PropertyStatement,_=>n};
IEnumerable<SyntaxNode> Members(SyntaxNode n)=>n.DescendantNodes().Where(n=>n is MethodBlockSyntax or ConstructorBlockSyntax or PropertyBlockSyntax);
string Key(SyntaxNode n)=>TypeName(n)+"|"+Tokens(Header(n));
if(args.Contains("--results-snapshot")) {
 var clock=Stopwatch.StartNew();var metadata=new List<string>();long bytes=0;
 void Add(string d){foreach(var path in Directory.EnumerateFiles(d,"*",SearchOption.AllDirectories)){var f=new FileInfo(path);metadata.Add(Path.GetRelativePath(root,path).Replace('\\','/')+"|"+f.Length+"|"+f.LastWriteTimeUtc.Ticks);bytes+=f.Length;}}
 foreach(var d in Directory.EnumerateDirectories(root).Where(d=>!Path.GetFileName(d).EndsWith(".tmp")&&Path.GetFileName(d)!=".git")){
  if(Path.GetFileName(d).Equals("TestResults",StringComparison.OrdinalIgnoreCase))Add(d);else foreach(var nested in Directory.EnumerateDirectories(d,"TestResults",SearchOption.AllDirectories))Add(nested);
 }
 metadata.Sort(StringComparer.Ordinal);var digest=Sha(string.Join('\n',metadata));using var prior=JsonDocument.Parse(File.ReadAllText("evidence/04A32/historical-results-preservation.json"));var p=prior.RootElement;
 bool equal=p.GetProperty("count").GetInt32()==metadata.Count&&p.GetProperty("bytes").GetInt64()==bytes&&p.GetProperty("membership_size_mtime_sha256").GetString()==digest;
 var receipt=new {schema="04A33-historical-stat-v1",status=equal?"MATCH_ACCEPTED_METADATA_RECEIPT":"METADATA_DELTA_REQUIRES_QUALIFICATION",count=metadata.Count,bytes,membership_size_mtime_sha256=digest,reference="evidence/04A32/historical-results-preservation.json",qualification="Scoped membership/size/mtime only;138GB payload NOT_REHASHED;ignored *.tmp outputs excluded",seconds=clock.Elapsed.TotalSeconds};
 File.WriteAllText("evidence/04A33/historical-results-preservation.json",JsonSerializer.Serialize(receipt,new JsonSerializerOptions{WriteIndented=true})+"\n");Require(equal,"Historical metadata drift");Console.WriteLine("PASS historical metadata;payload NOT_REHASHED");return;
}
