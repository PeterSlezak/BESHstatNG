// Read-only VB declaration metadata. ParseText only: no VB compilation or execution.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
string Norm(string s)=>Regex.Replace(s,@"\s+"," ").Trim();
SyntaxNode Header(SyntaxNode n)=>n switch {MethodBlockBaseSyntax b=>b.BlockStatement,PropertyBlockSyntax b=>b.PropertyStatement,LambdaExpressionSyntax b=>b.SubOrFunctionHeader,_=>n};
string Owner(SyntaxNode n)=>("BESHStatNG."+string.Join(".",n.Ancestors().Reverse().Select(a=>a is NamespaceBlockSyntax ns?ns.NamespaceStatement.Name.ToString():a is TypeBlockSyntax t?t.BlockStatement.Identifier.ValueText:null).Where(s=>s!=null))).Replace("BESHStatNG.Global.","");
bool Member(SyntaxNode n)=>n is MethodBlockBaseSyntax or PropertyBlockSyntax or LambdaExpressionSyntax;
string root=Path.GetFullPath(args[0]),output=args[1];if(File.Exists(output))throw new Exception("Append-only output exists");var rows=new List<object>();var sources=new List<object>();
foreach(var path in args.Skip(2).Order()){
 string text=File.ReadAllText(Path.Combine(root,path));var tree=VisualBasicSyntaxTree.ParseText(text);if(tree.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error))throw new Exception("Syntax error "+path);
 sources.Add(new{path,bytes=new FileInfo(Path.Combine(root,path)).Length,sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root,path)))).ToLowerInvariant()});
 foreach(var n in tree.GetRoot().DescendantNodes().Where(Member)){
  var h=Header(n);ParameterListSyntax? ps=h switch{MethodStatementSyntax s=>s.ParameterList,SubNewStatementSyntax s=>s.ParameterList,LambdaHeaderSyntax s=>s.ParameterList,PropertyStatementSyntax s=>s.ParameterList,_=>null};
  string owner=n is AccessorBlockSyntax?Owner(n)+"."+((PropertyBlockSyntax)n.Parent!).PropertyStatement.Identifier.ValueText:Owner(n);
  string id=path+"|"+owner+"|"+n.Kind()+"|"+Hash(Norm(h.ToString()))[..16]+"@"+n.Span.Start+":"+n.Span.Length;
  rows.Add(new{symbol_id=id,owner,header=Norm(h.ToString()),parameters=ps?.Parameters.Select(p=>new{syntax=Norm(p.ToString()),identifier=p.Identifier.ToString(),type=p.AsClause?.Type.ToString(),byref=p.Modifiers.Any(SyntaxKind.ByRefKeyword),optional=p.Modifiers.Any(SyntaxKind.OptionalKeyword),paramarray=p.Modifiers.Any(SyntaxKind.ParamArrayKeyword)}).ToArray()});
 }
}
File.WriteAllText(output,JsonSerializer.Serialize(new{scope="SYNTAX_ONLY_NO_VB_COMPILATION_OR_EXECUTION",sources,rows},new JsonSerializerOptions{WriteIndented=true})+"\n",new UTF8Encoding(false));Console.WriteLine(rows.Count);
