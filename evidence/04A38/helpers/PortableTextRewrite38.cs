using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

// The Windows project uses Option Compare Text. Portable code makes that exact
// comparison intent explicit instead of inheriting a project/compiler option.
try {
var text=File.ReadAllText(args[0]);var tree=VisualBasicSyntaxTree.ParseText(text);
var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
    .Select(p=>MetadataReference.CreateFromFile(p)).ToList();
references.Add(MetadataReference.CreateFromFile(args[2]));
var compilation=VisualBasicCompilation.Create("TextBinding38",new[]{tree},references,
    new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary,rootNamespace:"BESHStatNG",optionStrict:OptionStrict.On));
var model=compilation.GetSemanticModel(tree);
var rewriter=new TextRewriter38(model);var result=rewriter.Visit(tree.GetRoot())!;
if(File.Exists(args[1]))throw new Exception("Append-only rewrite output exists");
File.WriteAllText(args[1],result.ToFullString(),new UTF8Encoding(false));
File.WriteAllText(args[1]+".json",JsonSerializer.Serialize(new{string_comparisons=rewriter.Comparisons,string_selects=rewriter.Selects,
    semantic_errors=compilation.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).Select(d=>new{code=d.Id,message=d.GetMessage(),line=d.Location.GetLineSpan().StartLinePosition.Line+1}).ToArray()},new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));
Console.WriteLine($"Made {rewriter.Comparisons} string comparisons and {rewriter.Selects} string selectors explicit");

}
catch(Exception error) { Console.Error.WriteLine(error.GetType().FullName+": "+error.Message); Environment.ExitCode=1; }

sealed class TextRewriter38(SemanticModel model):VisualBasicSyntaxRewriter {
 public int Comparisons,Selects;
 public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node){
  var rewritten=(BinaryExpressionSyntax)base.VisitBinaryExpression(node)!;
  if(!node.IsKind(SyntaxKind.EqualsExpression)&&!node.IsKind(SyntaxKind.NotEqualsExpression))return rewritten;
  bool text=model.GetTypeInfo(node.Left).Type?.SpecialType==SpecialType.System_String||model.GetTypeInfo(node.Right).Type?.SpecialType==SpecialType.System_String;
  if(!text)return rewritten;
  Comparisons++;string expression="LegacyUdfText38.EqualText("+rewritten.Left.ToString()+", "+rewritten.Right.ToString()+")";
  if(node.IsKind(SyntaxKind.NotEqualsExpression))expression="Not "+expression;
  return SyntaxFactory.ParseExpression(expression).WithTriviaFrom(node);
 }
 public override SyntaxNode? VisitSelectBlock(SelectBlockSyntax node){
  var rewritten=(SelectBlockSyntax)base.VisitSelectBlock(node)!;
  if(model.GetTypeInfo(node.SelectStatement.Expression).Type?.SpecialType!=SpecialType.System_String)return rewritten;
  var clauses=node.CaseBlocks.SelectMany(b=>b.CaseStatement.Cases).OfType<SimpleCaseClauseSyntax>().ToArray();
  if(clauses.Length==0)return rewritten;
  if(clauses.Any(c=>c.Value is not LiteralExpressionSyntax literal||literal.Token.Value is not string))throw new Exception("Nonliteral legacy String selector requires explicit review");
  Selects++;string expression="LegacyUdfText38.CaseToken("+rewritten.SelectStatement.Expression.ToString()+", New String() {"+string.Join(", ",clauses.Select(c=>c.Value.ToString()))+"})";
  return rewritten.WithSelectStatement(rewritten.SelectStatement.WithExpression(SyntaxFactory.ParseExpression(expression).WithTriviaFrom(rewritten.SelectStatement.Expression)));
 }
}
