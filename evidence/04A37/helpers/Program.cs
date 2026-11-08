using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
var here="evidence/04A37/";
string Sha(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
string Tokens(SyntaxNode n)=>string.Join(" ",n.DescendantTokens().Select(t=>t.Text));
string Norm(string s)=>Regex.Replace(s,@"\s+"," ").Trim();
SyntaxNode Header(SyntaxNode n)=>n switch{MethodBlockBaseSyntax b=>b.BlockStatement,PropertyBlockSyntax b=>b.PropertyStatement,LambdaExpressionSyntax b=>b.SubOrFunctionHeader,_=>n};
string Name(SyntaxNode n)=>Header(n) switch{MethodStatementSyntax b=>b.Identifier.ValueText,SubNewStatementSyntax=>".ctor",PropertyStatementSyntax b=>b.Identifier.ValueText,AccessorStatementSyntax b=>b.AccessorKeyword.Text,_=>"lambda"};
string Owner(SyntaxNode n)=>"BESHStatNG."+string.Join(".",n.Ancestors().Reverse().Select(a=>a is NamespaceBlockSyntax ns?ns.NamespaceStatement.Name.ToString():a is TypeBlockSyntax t?t.BlockStatement.Identifier.ValueText:null).Where(s=>s!=null));
object[] Nodes(string path,string source){
 var tree=VisualBasicSyntaxTree.ParseText(source);if(tree.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error))throw new Exception("Syntax error: "+path);
 return tree.GetRoot().DescendantNodes().Where(n=>n is MethodBlockBaseSyntax or PropertyBlockSyntax or LambdaExpressionSyntax||n is PropertyStatementSyntax&&n.Parent is not PropertyBlockSyntax).Select(n=>new{path,key=path+"|"+Owner(n)+"|"+n.Kind()+"|"+Sha(Norm(Header(n).ToString()))[..16],name=Name(n),kind=n.Kind().ToString(),owner=Owner(n),signature=Norm(Header(n).ToString()),header_tokens=Tokens(Header(n)),tokens=Tokens(n),token_sha256=Sha(Tokens(n)),span_start=n.Span.Start,span_length=n.Span.Length,start=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,end=n.GetLocation().GetLineSpan().EndLinePosition.Line+1,calls=n.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(x=>Norm(x.ToString())).ToArray(),assignments=n.DescendantNodes().OfType<AssignmentStatementSyntax>().Select(x=>new{left=Tokens(x.Left),right=Tokens(x.Right)}).ToArray(),catches=n.DescendantNodes().OfType<CatchStatementSyntax>().Select(Tokens).ToArray()}).Cast<object>().ToArray();
}
if(args.Contains("--supporting")) {
 var paths37=Directory.GetFiles("BESHStat.Excel.Common/src","Multivariate*.vb",SearchOption.AllDirectories).Concat(new[]{"PCA","FactorAnalysis","CA","ClusterAnalysis"}.Select(n=>"BESHStat.Core/src/RegModels/Multivariate/"+n+".vb")).Concat(new[]{"BESHstatNG/src/DM/WindowsMultivariateWorkflowAdapter.vb","BESHstatNG/src/UI/Ui11PCA.vb","BESHStat.Core/src/Graphics/ChartAxisScalingCore.vb","BESHStat.Core/src/Graphics/ChartReferenceSeries.vb"});
 var data37=paths37.SelectMany(p=>Nodes(p.Replace('\\','/'),File.ReadAllText(p))).ToArray();
 var dest37=here+"supporting-entry-members.json";if(File.Exists(dest37))throw new Exception("Frozen supporting census exists");
 File.WriteAllText(dest37,JsonSerializer.Serialize(data37,new JsonSerializerOptions{WriteIndented=true})+"\n");Console.WriteLine("Supporting members: "+data37.Length);return;
}
var files=new[]{"PcaPlotExcel","FactorAnalysisPlotExcel","ClusterAnalysisPlotExcel","CorrespondenceAnalysisPlotExcel"};
var old=files.SelectMany(f=>Nodes("BESHstatNG/src/Graphics/"+f+".vb",File.ReadAllText(here+"preedit/"+f+".vb.txt"))).ToArray();
var supporting=Array.Empty<object>();
var paths=files.Select(f=>"BESHstatNG/src/Graphics/"+f+".vb").Concat(Directory.GetFiles("BESHStat.Core/src/Graphics","*37.vb")).Concat(Directory.GetFiles("BESHStat.Excel.Common/src/Analysis","*37.vb")).Concat(Directory.GetFiles("BESHStat.Excel.Common/src/Results","*37.vb")).Select(p=>p.Replace('\\','/')).ToArray();
var current=paths.SelectMany(p=>Nodes(p,File.ReadAllText(p))).ToArray();
var output=here+(args.Contains("--entry")?"entry-members.json":"final-members.json");
if(args.Contains("--entry")&&File.Exists(output))throw new Exception("Frozen member census exists");
File.WriteAllText(output,JsonSerializer.Serialize(new{basis="Roslyn token/spans/calls; syntax source authority, not native execution",entry=old,supporting_entry=supporting,current},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine("Source census: "+old.Length+" original, "+current.Length+" current members");

if(args.Contains("--history")){
 var root=Directory.GetCurrentDirectory();var metadata=new List<string>();long bytes=0;
 void Add(string dir){foreach(var path in Directory.EnumerateFiles(dir,"*",SearchOption.AllDirectories)){var f=new FileInfo(path);metadata.Add(Path.GetRelativePath(root,path).Replace('\\','/')+"|"+f.Length+"|"+f.LastWriteTimeUtc.Ticks);bytes+=f.Length;}}
 foreach(var dir in Directory.EnumerateDirectories(root).Where(d=>!Path.GetFileName(d).EndsWith(".tmp")&&Path.GetFileName(d)!=".git")){
  if(Path.GetFileName(dir).Equals("TestResults",StringComparison.OrdinalIgnoreCase))Add(dir);
  else foreach(var nested in Directory.EnumerateDirectories(dir,"TestResults",SearchOption.AllDirectories))Add(nested);
 }
 metadata.Sort(StringComparer.Ordinal);var digest=Sha(string.Join('\n',metadata));using var prior=JsonDocument.Parse(File.ReadAllText("evidence/04A36/historic-census-metadata.json"));var p=prior.RootElement;
 bool match=p.GetProperty("count").GetInt32()==metadata.Count&&p.GetProperty("bytes").GetInt64()==bytes&&p.GetProperty("membership_size_mtime_sha256").GetString()==digest;
 File.WriteAllText(here+"historic-census-metadata.json",JsonSerializer.Serialize(new{status=match?"MATCH_ACCEPTED_METADATA":"METADATA_DELTA",count=metadata.Count,bytes,membership_size_mtime_sha256=digest,basis="Metadata only; ignored *.tmp outputs excluded. ~138GB historical payload bytes NOT_REHASHED; no cleanup."},new JsonSerializerOptions{WriteIndented=true})+"\n");
 if(!match)throw new Exception("Historical metadata changed");
 Console.WriteLine("PASS retained historical membership/size/mtime metadata");
}
