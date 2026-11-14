using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
string Norm(string s)=>Regex.Replace(s,@"\s+"," ").Trim();
string Tokens(SyntaxNode n)=>string.Join(" ",n.DescendantTokens().Select(t=>t.Text));
SyntaxNode Header(SyntaxNode n)=>n switch{MethodBlockBaseSyntax b=>b.BlockStatement,PropertyBlockSyntax b=>b.PropertyStatement,LambdaExpressionSyntax b=>b.SubOrFunctionHeader,_=>n};
string Name(SyntaxNode n)=>Header(n) switch{MethodStatementSyntax s=>s.Identifier.ValueText,SubNewStatementSyntax=>".ctor",AccessorStatementSyntax s=>s.AccessorKeyword.Text,PropertyStatementSyntax s=>s.Identifier.ValueText,_=>n is LambdaExpressionSyntax?"lambda":"initializer"};
string Owner(SyntaxNode n){var type="BESHStatNG."+string.Join(".",n.Ancestors().Reverse().Select(a=>a is NamespaceBlockSyntax ns?ns.NamespaceStatement.Name.ToString():a is TypeBlockSyntax t?t.BlockStatement.Identifier.ValueText:null).Where(s=>s!=null));return type.Replace("BESHStatNG.Global.","");}
bool Initializer(SyntaxNode n)=>n is VariableDeclaratorSyntax v&&(v.Initializer!=null||v.AsClause is AsNewClauseSyntax)&&(v.Parent is FieldDeclarationSyntax||v.Parent is LocalDeclarationStatementSyntax l&&l.Modifiers.Any(SyntaxKind.StaticKeyword))||n is PropertyStatementSyntax p&&(p.Initializer!=null||p.AsClause is AsNewClauseSyntax);
bool Member(SyntaxNode n)=>n is MethodBlockBaseSyntax or PropertyBlockSyntax or LambdaExpressionSyntax||Initializer(n);
var members=new List<object>();var exports=new List<object>();var sources=new List<object>();
foreach(var file in Directory.GetFiles("BESHstatNG/src/ExcelUDFs","*.vb",SearchOption.AllDirectories).Order()){
 string path=file.Replace('\\','/'),source=File.ReadAllText(file);var seen=new HashSet<string>();int count=0;
 foreach(bool win64 in new[]{false,true}){
  var options=VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithPreprocessorSymbols(new Dictionary<string,object>{{"DEBUG",true},{"TRACE",true},{"Win64",win64},{"VBA7",true},{"CONFIG","Debug"},{"_MYTYPE",win64?"WindowsForms":"Windows"},{"BESH_HELP_RESOLVER_PUBLISHED",true}});
  var tree=VisualBasicSyntaxTree.ParseText(source,options,path:path);
  if(tree.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error))throw new Exception("Syntax error: "+path);
  foreach(var n in tree.GetRoot().DescendantNodes().Where(Member)){
   if(!seen.Add(n.Kind()+":"+n.Span.Start+":"+n.Span.Length))continue;
   string owner=Owner(n);if(n is AccessorBlockSyntax)owner+="."+Name(n.Parent!);
   var header=Header(n);string signature=Norm(header.ToString()),key=path+"|"+owner+"|"+n.Kind()+"|"+Hash(signature)[..16];
   string id=key+"@"+n.Span.Start+":"+n.Span.Length;
   var method=header as MethodStatementSyntax;
   var attrs=method?.AttributeLists.SelectMany(a=>a.Attributes).Where(a=>Regex.IsMatch(a.Name.ToString(),@"(?i)(^|\.)ExcelFunction(Attribute)?$")).ToArray()??[];
   var calls=n.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(a=>new{expression=Norm(a.Expression.ToString()),text=Norm(a.ToString()),start=a.Span.Start,length=a.Span.Length,line=a.GetLocation().GetLineSpan().StartLinePosition.Line+1}).ToArray();
   members.Add(new{path,symbol_id=id,signature_key=key,name=Name(n),kind=n.Kind().ToString(),containing_type=owner,signature,header_tokens=Tokens(header),body_tokens=Tokens(n),body_token_sha256=Hash(Tokens(n)),body_sha256=Hash(n.ToString()),span_start=n.Span.Start,span_length=n.Span.Length,start_line=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,end_line=n.GetLocation().GetLineSpan().EndLinePosition.Line+1,exported=attrs.Length>0,calls,
      direct_calls=n.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(a=>a.Ancestors().FirstOrDefault(Member)==n).Select(a=>Norm(a.Expression.ToString())).ToArray(),
      creation=n.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Select(a=>Norm(a.ToString())).ToArray(),
      assignments=n.DescendantNodes().OfType<AssignmentStatementSyntax>().Select(a=>new{left=Tokens(a.Left),right=Tokens(a.Right)}).ToArray(),
      catches=n.DescendantNodes().OfType<CatchStatementSyntax>().Select(a=>Tokens(a)).ToArray(),
      enclosing=n.Ancestors().Where(Member).Select(a=>Name(a)).ToArray(),source_sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant()});count++;
   foreach(var attr in attrs){
    var nameArg=attr.ArgumentList?.Arguments.OfType<SimpleArgumentSyntax>().FirstOrDefault(a=>a.NameColonEquals?.Name.Identifier.ValueText.Equals("Name",StringComparison.OrdinalIgnoreCase)==true);
    string effective=nameArg?.Expression is LiteralExpressionSyntax lit?lit.Token.ValueText:nameArg==null?method!.Identifier.ValueText:"UNRESOLVED";
    exports.Add(new{export_id=path+"@"+attr.Span.Start,path,source_symbol_id=id,effective_excel_name=effective,implementing_method=Name(n),containing_type=owner,full_public_signature=header.ToString(),full_attribute_text=attr.ToString(),header_tokens=Tokens(header),return_type=method!.AsClause?.ToString(),arguments=method.ParameterList!.Parameters.Select(a=>new{name=a.Identifier.Identifier.ValueText,syntax=a.ToString(),tokens=Tokens(a),default_value=a.Default?.ToString(),type=a.AsClause?.ToString(),attributes=a.AttributeLists.ToString()}).ToArray(),attribute_arguments=attr.ArgumentList?.Arguments.Select(a=>a.ToString()).ToArray(),original_source_hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant()});
   }
  }
 }
 sources.Add(new{path,bytes=new FileInfo(file).Length,sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant(),members=count});
}
if(exports.Count!=268||sources.Count!=41)throw new Exception("Scope count mismatch");
string dest=args[0];if(File.Exists(dest))throw new Exception("Append-only inventory exists");
File.WriteAllText(dest,JsonSerializer.Serialize(new{provenance="EXECUTED_ROSLYN_SYNTAX_CENSUS; NOT_NATIVE_REGISTRATION",sources,members,exports},new JsonSerializerOptions{WriteIndented=true})+"\n",new UTF8Encoding(false));
Console.WriteLine($"Census: {sources.Count} sources; {members.Count} members; {exports.Count} exports; {exports.Select(x=>JsonSerializer.SerializeToElement(x).GetProperty("path").GetString()).Distinct().Count()} export-bearing sources");
