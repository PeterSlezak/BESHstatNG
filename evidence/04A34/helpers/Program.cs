using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
const string Base="b162a6ac801d7784fc5e27974fbf91aa57a2aee1";
const string Here="evidence/04A34/";
var root=Directory.GetCurrentDirectory();
string Sha(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
string Tokens(SyntaxNode n)=>String.Join(" ",n.DescendantTokens().Select(t=>t.Text));
string Norm(string s)=>Regex.Replace(s,@"\s+"," ").Trim();
string Git(params string[] args){
 var info=new ProcessStartInfo("git"){RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
 info.ArgumentList.Add("-c");info.ArgumentList.Add("safe.directory="+root.Replace('\\','/'));
 foreach(var a in args)info.ArgumentList.Add(a);
 using var p=Process.Start(info)!;var output=p.StandardOutput.ReadToEnd();var error=p.StandardError.ReadToEnd();p.WaitForExit();
 if(p.ExitCode!=0)throw new Exception(error.Replace(root,"<REPO_ROOT>"));return output;
}
void Require(bool b,string message){if(!b)throw new Exception(message);}
SyntaxNode Parse(string p,string s){var t=VisualBasicSyntaxTree.ParseText(s,VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest),p);Require(!t.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error),"VB syntax: "+p);return t.GetRoot();}
SyntaxNode Header(SyntaxNode n)=>n switch{MethodBlockBaseSyntax b=>b.BlockStatement,PropertyBlockSyntax b=>b.PropertyStatement,EventBlockSyntax b=>b.EventStatement,LambdaExpressionSyntax b=>b.SubOrFunctionHeader,_=>n};
string Name(SyntaxNode n)=>Header(n) switch{MethodStatementSyntax s=>s.Identifier.ValueText,SubNewStatementSyntax=>".ctor",AccessorStatementSyntax s=>s.AccessorKeyword.Text,PropertyStatementSyntax s=>s.Identifier.ValueText,VariableDeclaratorSyntax s=>String.Join(",",s.Names.Select(x=>x.Identifier.ValueText)),EnumMemberDeclarationSyntax s=>s.Identifier.ValueText,_=>n is LambdaExpressionSyntax?"lambda":"initializer"};
string TypeName(SyntaxNode n){var parts=new List<string>{"BESHStatNG"};foreach(var a in n.Ancestors().Reverse()){if(a is NamespaceBlockSyntax ns)parts.Add(ns.NamespaceStatement.Name.ToString());if(a is TypeBlockSyntax t)parts.Add(t.BlockStatement.Identifier.ValueText);if(a is EnumBlockSyntax e)parts.Add(e.EnumStatement.Identifier.ValueText);}if(n is AccessorBlockSyntax)parts.Add(Name(n.Parent!));return String.Join(".",parts);}
bool IsMember(SyntaxNode n)=>n is MethodBlockBaseSyntax or PropertyBlockSyntax or EventBlockSyntax or LambdaExpressionSyntax or DeclareStatementSyntax || n is MethodStatementSyntax && n.Parent is not MethodBlockBaseSyntax;
bool Initializer(SyntaxNode n)=>n is VariableDeclaratorSyntax v && (v.Initializer!=null||v.AsClause is AsNewClauseSyntax)&&(v.Parent is FieldDeclarationSyntax||v.Parent is LocalDeclarationStatementSyntax l&&l.Modifiers.Any(SyntaxKind.StaticKeyword))||n is PropertyStatementSyntax p&&(p.Initializer!=null||p.AsClause is AsNewClauseSyntax);
IEnumerable<SyntaxNode> Members(SyntaxNode n)=>n.DescendantNodes().Where(n=>IsMember(n)||Initializer(n)||n is PropertyStatementSyntax&&n.Parent is not PropertyBlockSyntax||n is VariableDeclaratorSyntax&&n.Parent is FieldDeclarationSyntax||n is EnumMemberDeclarationSyntax);
string Key(string path,SyntaxNode n)=>path+"|"+TypeName(n)+"|"+n.Kind()+"|"+Sha(Norm(Header(n).ToString()))[..16];
var paths=new[]{"3Dobjects","AgreementPlotExcel","BulletChart","BulletChartExcel","CalibrationPlot","ConvexHullPlot","CumulativeDistributionPlot","CumulativeDistributionPlotExcel","DumbbellPlot","DumbbellPlotExcel","GifAnimator","KiteChart","LadderPlot","LadderPlotExcel","MultipleComparisonPlotExcel","PolarPlot","PsmLovePlotExcel","SankeyPlot","SankeyPlotExcel","SpcControlChartExcel","SymetryPlot","TheilSenPlotExcel"}.Select(n=>"BESHstatNG/src/Graphics/"+n+".vb").ToArray();
var rows=new List<object>();var nodes=new List<object>();
foreach(var path in paths){
 var before=Parse(path,Git("show",Base+":"+path));var after=Parse(path,File.ReadAllText(path));
 var final=Members(after).GroupBy(n=>Key(path,n)).ToDictionary(g=>g.Key,g=>g.ToArray());
 foreach(var n in Members(before)){
  string key=Key(path,n);var hits=final.GetValueOrDefault(key,[]);bool same=hits.Any(x=>Tokens(x)==Tokens(n));
  bool removed=n is MethodBlockSyntax&&((Name(n)=="HasNaturalZero"&&path.EndsWith("SpcControlChartExcel.vb"))||(Name(n)=="Clamp"&&path.EndsWith("LadderPlotExcel.vb")));
  bool privateNode=Header(n).ChildTokens().Any(t=>t.IsKind(SyntaxKind.PrivateKeyword)||t.IsKind(SyntaxKind.FriendKeyword));
  if(n is MethodBlockSyntax or ConstructorBlockSyntax or PropertyBlockSyntax||n is PropertyStatementSyntax)Require(hits.Length>0||removed&&privateNode,"Existing Windows API header lost: "+key);
  rows.Add(new{path,key,name=Name(n),kind=n.Kind().ToString(),entry_start=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,entry_end=n.GetLocation().GetLineSpan().EndLinePosition.Line+1,entry_header_sha256=Sha(Tokens(Header(n))),entry_body_sha256=Sha(Tokens(n)),header_present=hits.Length>0,body_preserved=same,private_removed=removed,final_matches=hits.Select(x=>new{start=x.GetLocation().GetLineSpan().StartLinePosition.Line+1,end=x.GetLocation().GetLineSpan().EndLinePosition.Line+1,token_sha256=Sha(Tokens(x))}).ToArray()});
 }
 foreach(var n in Members(after))nodes.Add(new{path,key=Key(path,n),name=Name(n),kind=n.Kind().ToString(),qualified_type=TypeName(n),signature=Norm(Header(n).ToString()),start=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,end=n.GetLocation().GetLineSpan().EndLinePosition.Line+1,source_span_start=n.Span.Start,source_span_length=n.Span.Length,enclosing_name=n.Ancestors().FirstOrDefault(a=>a is MethodBlockSyntax) is SyntaxNode owner?Name(owner):"type-scope",token_sha256=Sha(Tokens(n))});
}
var newPortable=Git("ls-files","--others","--exclude-standard").Split('\n').Where(p=>(p.StartsWith("BESHStat.Core/")||p.StartsWith("BESHStat.Excel.Common/"))&&p.Trim().EndsWith(".vb")).Select(p=>p.Trim()).ToArray();
foreach(var path in newPortable){var s=File.ReadAllText(path);Require(s.Contains("Option Strict On"),"Strict missing: "+path);var tokens=Tokens(Parse(path,s));foreach(var banned in new[]{"ExcelDna","Interop","System.Windows.Forms","System.Drawing","Microsoft.VisualBasic","System.IO","Clipboard","Worksheet","Workbook"})Require(!tokens.Contains(banned,StringComparison.OrdinalIgnoreCase),"Portable dependency: "+path+":"+banned);}
object Detailed(string path,SyntaxNode n)=>new{path,key=Key(path,n),name=Name(n),kind=n.Kind().ToString(),qualified_type=TypeName(n),signature=Norm(Header(n).ToString()),start=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,end=n.GetLocation().GetLineSpan().EndLinePosition.Line+1,source_span_start=n.Span.Start,source_span_length=n.Span.Length,enclosing_name=n.Ancestors().FirstOrDefault(a=>a is MethodBlockSyntax) is SyntaxNode owner?Name(owner):"type-scope",parent_tokens=n is LambdaExpressionSyntax?Norm(n.Parent?.Parent?.ToString()??""):"",tokens=Tokens(n),header_tokens=Tokens(Header(n)),token_sha256=Sha(Tokens(n)),exception_boundaries=n.DescendantNodes().Where(x=>x is TryStatementSyntax or CatchStatementSyntax or FinallyStatementSyntax).Select(Tokens).ToArray(),measurements=n.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Where(x=>new[]{"InsideWidth","InsideHeight","InsideLeft","InsideTop","BoundWidth","BoundHeight"}.Contains(x.Name.ToString())).Select(Tokens).ToArray(),assignments=n.DescendantNodes().OfType<AssignmentStatementSyntax>().Select(x=>new{left=Tokens(x.Left),right=Tokens(x.Right)}).ToArray(),creations=n.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Select(x=>new{expression=Norm(x.Type.ToString()),start=x.Span.Start,length=x.Span.Length}).ToArray(),calls=n.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(x=>new{expression=Norm(x.Expression.ToString()),arguments=x.ArgumentList?.Arguments.Select(a=>Norm(a.ToString())).ToArray(),start=x.Span.Start,length=x.Span.Length}).ToArray()};
var detailPaths=paths.Concat(newPortable).Distinct().ToArray();
var details=detailPaths.SelectMany(path=>Members(Parse(path,File.ReadAllText(path))).Select(n=>Detailed(path,n))).ToArray();
var checkpoint=Directory.EnumerateFiles(Here+"completion/entry-source","*.vb.txt").SelectMany(path=>{string real="BESHstatNG/src/Graphics/"+Path.GetFileName(path).Replace(".txt","");return Members(Parse(real,File.ReadAllText(path))).Select(n=>Detailed(real,n));}).ToArray();
File.WriteAllText(Here+"completion/member-source.json",JsonSerializer.Serialize(new{basis="Roslyn syntax, exact current source spans and lexical calls; no COM execution or semantic overload binding",current=details,checkpoint},new JsonSerializerOptions{WriteIndented=true})+"\n");
foreach(var path in new[]{"BESHStat.Core/BESHStat.Core.vbproj","BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj"})Require(XDocument.Load(path).Descendants("TargetFramework").Single().Value=="netstandard2.0","Target drift");
Require(XDocument.Load("BESHstatNG/BESHstatNG.vbproj").Descendants().Count(x=>x.Name.LocalName=="Compile"&&((string?)x.Attribute("Include"))?.StartsWith("src\\Graphics\\")==true)==38,"Windows Graphics compile population drift");
File.WriteAllText(Here+"source-ownership.json",JsonSerializer.Serialize(new{schema="04A34-source-ownership-v1",status="PASS",basis="Current and entry Roslyn tokens; syntax identity, not native execution or semantic call binding",windows_headers_preserved=true,portable_dependency_guard="PASS",compiled_graphics=38,members=rows,current_nodes=nodes},new JsonSerializerOptions{WriteIndented=true})+"\n");
if(args.Contains("--history")){
 var metadata=new List<string>();long bytes=0;
 void Add(string dir){foreach(var path in Directory.EnumerateFiles(dir,"*",SearchOption.AllDirectories)){var f=new FileInfo(path);metadata.Add(Path.GetRelativePath(root,path).Replace('\\','/')+"|"+f.Length+"|"+f.LastWriteTimeUtc.Ticks);bytes+=f.Length;}}
 foreach(var dir in Directory.EnumerateDirectories(root).Where(d=>!Path.GetFileName(d).EndsWith(".tmp")&&Path.GetFileName(d)!=".git")){if(Path.GetFileName(dir).Equals("TestResults",StringComparison.OrdinalIgnoreCase))Add(dir);else foreach(var nested in Directory.EnumerateDirectories(dir,"TestResults",SearchOption.AllDirectories))Add(nested);}
 metadata.Sort(StringComparer.Ordinal);string digest=Sha(String.Join('\n',metadata));using var prior=JsonDocument.Parse(File.ReadAllText("evidence/04A33/historical-results-preservation.json"));var p=prior.RootElement;
 bool equal=p.GetProperty("count").GetInt32()==metadata.Count&&p.GetProperty("bytes").GetInt64()==bytes&&p.GetProperty("membership_size_mtime_sha256").GetString()==digest;
 File.WriteAllText(Here+"historic-census-metadata.json",JsonSerializer.Serialize(new{status=equal?"MATCH_ACCEPTED_METADATA":"METADATA_DELTA",count=metadata.Count,bytes,membership_size_mtime_sha256=digest,basis="Metadata only; historical payload bytes NOT_REHASHED; *.tmp output excluded",reference="evidence/04A33/historical-results-preservation.json"},new JsonSerializerOptions{WriteIndented=true})+"\n");Require(equal,"Historical metadata drift");
}
Console.WriteLine("PASS current member/API/portable target/dependency guard; "+nodes.Count+" current declarations");
