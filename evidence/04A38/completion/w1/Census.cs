// Read-only syntax census. It invokes no UDF, numerical engine or reference case.
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
string Owner(SyntaxNode n)=>("BESHStatNG."+string.Join(".",n.Ancestors().Reverse().Select(a=>a is NamespaceBlockSyntax ns?ns.NamespaceStatement.Name.ToString():a is TypeBlockSyntax t?t.BlockStatement.Identifier.ValueText:null).Where(s=>s!=null))).Replace("BESHStatNG.Global.","");
bool Initializer(SyntaxNode n)=>n is VariableDeclaratorSyntax v&&(v.Initializer!=null||v.AsClause is AsNewClauseSyntax)&&(v.Parent is FieldDeclarationSyntax||v.Parent is LocalDeclarationStatementSyntax l&&l.Modifiers.Any(SyntaxKind.StaticKeyword))||n is PropertyStatementSyntax p&&(p.Initializer!=null||p.AsClause is AsNewClauseSyntax);
bool Member(SyntaxNode n)=>n is MethodBlockBaseSyntax or PropertyBlockSyntax or LambdaExpressionSyntax||Initializer(n);
string Id(string p,SyntaxNode n)=>p+"|"+(n is AccessorBlockSyntax?Owner(n)+"."+Name(n.Parent!):Owner(n))+"|"+n.Kind()+"|"+Hash(Norm(Header(n).ToString()))[..16]+"@"+n.Span.Start+":"+n.Span.Length;
var branchKinds=new HashSet<string>{"IfStatement","ElseIfStatement","ElseStatement","SingleLineIfStatement","SimpleCaseClause","RangeCaseClause","RelationalCaseClause","CaseElseStatement","ForStatement","ForEachStatement","WhileStatement","SimpleDoStatement","DoWhileStatement","DoUntilStatement","SimpleLoopStatement","LoopWhileStatement","LoopUntilStatement","CatchStatement","TernaryConditionalExpression","BinaryConditionalExpression","AndAlsoExpression","OrElseExpression"};
string root=Path.GetFullPath(args[0]),output=args[1];if(File.Exists(output))throw new Exception("Append-only census exists");
var members=new List<object>();var supports=new List<object>();var fragments=new List<object>();var sources=new List<object>();var exports=new List<object>();
foreach(var file in Directory.GetFiles(Path.Combine(root,"BESHstatNG/src/ExcelUDFs"),"*.vb",SearchOption.AllDirectories).Order()){
 string path=Path.GetRelativePath(root,file).Replace('\\','/'),source=File.ReadAllText(file);var seen=new HashSet<string>();
 var bytes=File.ReadAllBytes(file);string sourceHash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
 foreach(bool win64 in new[]{false,true}){
  var options=VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithPreprocessorSymbols(new Dictionary<string,object>{{"DEBUG",true},{"TRACE",true},{"Win64",win64},{"VBA7",true},{"CONFIG","Debug"},{"_MYTYPE",win64?"WindowsForms":"Windows"},{"BESH_HELP_RESOLVER_PUBLISHED",true}});
  var tree=VisualBasicSyntaxTree.ParseText(source,options,path:path);if(tree.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error))throw new Exception("Syntax error: "+path);
  foreach(var n in tree.GetRoot().DescendantNodes()){
   if(!seen.Add(n.Kind()+":"+n.Span.Start+":"+n.Span.Length))continue;
   int start=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,end=n.GetLocation().GetLineSpan().EndLinePosition.Line+1;
   if(Member(n)){
    string id=Id(path,n);var h=Header(n);var method=h as MethodStatementSyntax;
    var attrs=method?.AttributeLists.SelectMany(a=>a.Attributes).Where(a=>Regex.IsMatch(a.Name.ToString(),@"(?i)(^|\.)ExcelFunction(Attribute)?$")).ToArray()??[];
    members.Add(new{symbol_id=id,signature_key=id.Split('@')[0],path,name=Name(n),kind=n.Kind().ToString(),owner=n is AccessorBlockSyntax?Owner(n)+"."+Name(n.Parent!):Owner(n),signature=Norm(h.ToString()),span_start=n.Span.Start,span_length=n.Span.Length,start_line=start,end_line=end,body_sha256=Hash(n.ToString()),body_token_sha256=Hash(Tokens(n)),source_sha256=sourceHash,exported=attrs.Length>0,
     parent_member=n.Ancestors().FirstOrDefault(Member) is SyntaxNode parent?Id(path,parent):null,invocations=n.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(a=>a.Ancestors().FirstOrDefault(Member)==n).Select(a=>new{text=Norm(a.ToString()),line=a.GetLocation().GetLineSpan().StartLinePosition.Line+1}).ToArray()});
    foreach(var attr in attrs){var nameArg=attr.ArgumentList?.Arguments.OfType<SimpleArgumentSyntax>().FirstOrDefault(a=>a.NameColonEquals?.Name.Identifier.ValueText.Equals("Name",StringComparison.OrdinalIgnoreCase)==true);
     exports.Add(new{path,symbol_id=id,name=nameArg?.Expression is LiteralExpressionSyntax lit?lit.Token.ValueText:"UNRESOLVED",signature=Tokens(h),attribute=Tokens(attr),arguments=method!.ParameterList!.Parameters.Select(p=>Tokens(p)).ToArray()});}
   }
   if(n is TypeBlockSyntax or FieldDeclarationSyntax||n is PropertyStatementSyntax&&n.Parent is not PropertyBlockSyntax){
    var d=n is TypeBlockSyntax tb?(SyntaxNode)tb.BlockStatement:n;
    bool overlap=n is FieldDeclarationSyntax fd&&fd.Declarators.Any(v=>v.Initializer!=null||v.AsClause is AsNewClauseSyntax)||n is PropertyStatementSyntax p&&(p.Initializer!=null||p.AsClause is AsNewClauseSyntax);
    supports.Add(new{support_id=path+"|"+Owner(n)+"|SUPPORT/"+n.Kind()+"@"+n.Span.Start+":"+n.Span.Length,path,owner=Owner(n),kind=n.Kind().ToString(),span_start=n.Span.Start,span_length=n.Span.Length,start_line=start,end_line=end,declaration=d.ToString(),declaration_sha256=Hash(d.ToString()),already_in_primary_population=overlap});
   }
   bool branch=branchKinds.Contains(n.Kind().ToString()),inline=n is LambdaExpressionSyntax||n.Kind().ToString().Contains("ConditionalExpression")||n.Kind().ToString() is "AndAlsoExpression" or "OrElseExpression"||n is QueryExpressionSyntax;
   if(n is StatementSyntax||branch||inline){
    var owner=n.AncestorsAndSelf().FirstOrDefault(Member);string? ownerId=owner==null?null:Id(path,owner);
    var arm=n.Parent is MultiLineIfBlockSyntax ifb?ifb.Statements.Cast<SyntaxNode>().ToArray():n.Parent is ElseIfBlockSyntax eb?eb.Statements.Cast<SyntaxNode>().ToArray():n.Parent is ElseBlockSyntax el?el.Statements.Cast<SyntaxNode>().ToArray():new[]{n};
    fragments.Add(new{fragment_id=path+"|"+n.Kind()+"@"+n.Span.Start+":"+n.Span.Length,path,member_id=ownerId,kind=n.Kind().ToString(),span_start=n.Span.Start,span_length=n.Span.Length,start_line=start,end_line=end,is_branch=branch,is_inline=inline,is_statement=n is StatementSyntax,code=n.ToString(),token_sha256=Hash(Tokens(n)),
     arm_string_literals=arm.SelectMany(a=>a.DescendantTokens()).Where(t=>t.IsKind(SyntaxKind.StringLiteralToken)).Select(t=>t.ValueText).Distinct().ToArray()});
   }
  }
 }
 sources.Add(new{path,bytes=bytes.Length,sha256=sourceHash});
}
File.WriteAllText(output,JsonSerializer.Serialize(new{scope="READ_ONLY_FULL_SYNTAX_UNIVERSE;NO_REFERENCE_EXECUTION;NOT_OWNERSHIP_CLOSURE",sources,members,supports,fragments,exports},new JsonSerializerOptions{WriteIndented=true})+"\n",new UTF8Encoding(false));
Console.WriteLine(JsonSerializer.Serialize(new{sources=sources.Count,members=members.Count,supports=supports.Count,fragments=fragments.Count,exports=exports.Count}));
