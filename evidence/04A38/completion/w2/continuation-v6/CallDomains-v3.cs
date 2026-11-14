// Syntax only: no original assembly, model, UDF or comparison is executed.
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
string Tokens(SyntaxNode n)=>string.Join(" ",n.DescendantTokens().Select(t=>t.Text));
string Owner(SyntaxNode n)=>"BESHStatNG."+string.Join(".",n.Ancestors().Reverse().Select(a=>a is NamespaceBlockSyntax ns?ns.NamespaceStatement.Name.ToString():a is TypeBlockSyntax t?t.BlockStatement.Identifier.ValueText:null).Where(s=>s!=null));
string MemberId(string path,MethodBlockBaseSyntax method)=>path+"|"+Owner(method)+"|"+method.Kind()+"|"+Hash(Tokens(method.BlockStatement))[..16]+"@"+method.Span.Start+":"+method.Span.Length;
string root=Path.GetFullPath(args[0]),output=args[1];if(File.Exists(output))throw new Exception("Append-only output exists");
var calls=new List<object>();var sources=new List<object>();var directLines=new HashSet<string>();int direct=0,indirect=0,choiceCount=0;
foreach(var file in Directory.GetFiles(Path.Combine(root,"BESHStat.Excel.Common/src"),"*.vb",SearchOption.AllDirectories).Order()){
 string path=Path.GetRelativePath(root,file).Replace('\\','/'),source=File.ReadAllText(file),sourceHash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();
 var tree=VisualBasicSyntaxTree.ParseText(source,VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest));if(tree.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error))throw new Exception("Syntax error "+path);
 var found=new List<object>();
 foreach(var invoke in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>()){
  string expression=invoke.Expression.ToString();if(!expression.EndsWith("LegacyUdfText38.EqualText")&&!expression.EndsWith("LegacyUdfText38.CaseToken"))continue;
  bool equal=expression.EndsWith(".EqualText");var arguments=invoke.ArgumentList.Arguments.OfType<SimpleArgumentSyntax>().Select(a=>a.Expression).ToArray();if(arguments.Length!=2)throw new Exception("Unexpected comparison arguments");
  var parent=invoke.Ancestors().OfType<MethodBlockBaseSyntax>().First();var lambda=invoke.Ancestors().OfType<LambdaExpressionSyntax>().FirstOrDefault();int line=invoke.GetLocation().GetLineSpan().StartLinePosition.Line+1;
  string? right=null;var choices=new List<string>();
  if(equal){
   direct++;directLines.Add(path+":"+line);
   if(arguments[1] is LiteralExpressionSyntax literal&&literal.IsKind(SyntaxKind.StringLiteralExpression))right=literal.Token.ValueText;
   else if(arguments[1].ToString()=="String.Empty")right="";
   else throw new Exception("Unclassified nullable/dynamic right operand "+arguments[1]);
   if(right.Any(c=>c>127||c=='\0'))throw new Exception("Unexpected non-ASCII/ignorable direct literal");
  }else{
   indirect++;
   var init=arguments[1] as CollectionInitializerSyntax??(arguments[1] as ArrayCreationExpressionSyntax)?.Initializer;
   if(init==null)throw new Exception("Dynamic CaseToken choices "+arguments[1]);
   foreach(var item in init.Initializers){if(item is not LiteralExpressionSyntax lit||!lit.IsKind(SyntaxKind.StringLiteralExpression))throw new Exception("Nullable/nonliteral choice");string value=lit.Token.ValueText;if(value.Any(c=>c>127||c=='\0'))throw new Exception("Unexpected non-ASCII choice");choices.Add(value);}
   choiceCount+=choices.Count;
  }
  var locals=parent.DescendantNodes().OfType<VariableDeclaratorSyntax>().Select(v=>new{names=v.Names.Select(n=>n.Identifier.ValueText).ToArray(),initializer=v.Initializer?.Value.ToString(),declaration=v.ToString(),line=v.GetLocation().GetLineSpan().StartLinePosition.Line+1}).ToArray();
  calls.Add(new{site_id=path+"|"+Owner(parent)+"."+((MethodStatementSyntax)parent.BlockStatement).Identifier.ValueText+"|"+(equal?"EqualText":"CaseToken")+"@"+invoke.Span.Start+":"+invoke.Span.Length,line_site_id=path+":"+line,path,line,span_start=invoke.Span.Start,span_length=invoke.Span.Length,source_sha256=sourceHash,parent_id=MemberId(path,parent),parent_name=Owner(parent)+"."+((MethodStatementSyntax)parent.BlockStatement).Identifier.ValueText,parent_signature=Tokens(parent.BlockStatement),parent_span_start=parent.Span.Start,parent_span_length=parent.Span.Length,parent_body_token_sha256=Hash(Tokens(parent)),lambda_span=lambda==null?null:(object)new{start=lambda.Span.Start,length=lambda.Span.Length,code=lambda.ToString()},kind=equal?"EqualText":"CaseToken",left_expression=arguments[0].ToString(),right_expression=arguments[1].ToString(),right_literal=right,choices=choices.ToArray(),locals,domain_exclusion="Comparison RHS is a non-null ASCII literal; None-vs-nonempty-ignorable cannot occur. Literal Empty remains non-null. Ignorables may survive normalization on left; this is not claimed absent.",normalization_to_be_independently_classified=true});
 }
 sources.Add(new{path,bytes=new FileInfo(file).Length,sha256=sourceHash});
}
if(direct!=153||directLines.Count!=62||indirect!=67||choiceCount!=788)throw new Exception("Unclassified comparison frontier changed: "+direct+"/"+directLines.Count+"/"+indirect+"/"+choiceCount);
File.WriteAllText(output,JsonSerializer.Serialize(new{scope="CURRENT_COMMON_CALLER_DOMAIN_SYNTAX;NOT_ORIGINAL_REFERENCE_CAPTURE",sources,direct_invocations=direct,direct_source_lines=directLines.Count,CaseToken_sites=indirect,CaseToken_choice_occurrences=choiceCount,calls},new JsonSerializerOptions{WriteIndented=true})+"\n",new UTF8Encoding(false));
Console.WriteLine("153 direct invocations/62 lines;67 CaseToken sites/788 nonnull ASCII choices; source/member/lambda spans recorded");
