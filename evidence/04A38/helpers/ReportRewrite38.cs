using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

try {
 var text=File.ReadAllText(args[0]);var tree=VisualBasicSyntaxTree.ParseText(text);
 var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p)).ToList();
 references.Add(MetadataReference.CreateFromFile(args[2]));
 var compilation=VisualBasicCompilation.Create("ReportBinding38",new[]{tree},references,new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary,rootNamespace:"BESHStatNG",optionStrict:OptionStrict.On));
 var model=compilation.GetSemanticModel(tree);var rewriter=new ReportRewriter38(model);var result=rewriter.Visit(tree.GetRoot())!;
 if(File.Exists(args[1]))throw new Exception("Append-only report rewrite exists");
 File.WriteAllText(args[1],result.ToFullString(),new UTF8Encoding(false));
 File.WriteAllText(args[1]+".json",JsonSerializer.Serialize(new{rewritten_returns=rewriter.Returns,unsupported=rewriter.Unsupported},new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));
 if(rewriter.Unsupported.Count>0)Environment.ExitCode=1;
 Console.WriteLine("Prepared "+rewriter.Returns+" typed report returns; unsupported "+rewriter.Unsupported.Count);
} catch(Exception error){Console.Error.WriteLine(error.GetType().FullName+": "+error.Message);Environment.ExitCode=1;}

sealed class ReportRewriter38(SemanticModel model):VisualBasicSyntaxRewriter {
 public int Returns;public readonly List<string> Unsupported=[];
 ExpressionSyntax Wrap(ExpressionSyntax value,string preparation){
  var type=model.GetTypeInfo(value).Type;
  string body=type?.ToDisplayString()=="BESHStatNG.ResultTable"?value+".ToOutputModel()":"New Global.BESHStatNG.ResultTableOutputModel("+value+")";
  return SyntaxFactory.ParseExpression("New UdfReportProjection38("+body+", UdfReportPreparation38."+preparation+")");
 }
 public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node){
  var method=node.Ancestors().OfType<MethodBlockSyntax>().FirstOrDefault();
  if(method?.SubOrFunctionStatement.AsClause?.ToString()!="As UdfReportProjection38")return node;
  var expr=node.Expression;if(expr==null)return node;
  if(expr.IsKind(SyntaxKind.NothingLiteralExpression))return node;
  if(expr.ToString().StartsWith("ExcelError.")){
   string fault=expr.ToString().EndsWith("ExcelErrorNA")?"NotAvailable":expr.ToString().EndsWith("ExcelErrorNum")?"InvalidNumber":"InvalidValue";
   Returns++;return node.WithExpression(SyntaxFactory.ParseExpression("UdfReportProjection38.FromFault(UdfReportFault38."+fault+")").WithTriviaFrom(expr));
  }
  if(expr is InvocationExpressionSyntax call){
   string name=call.Expression.ToString();
   if(name=="PrepareResultTableForUdf"||name=="PreparePValueRowsForUdf"){
    var value=((SimpleArgumentSyntax)call.ArgumentList!.Arguments[0]).Expression;
    Returns++;return node.WithExpression(Wrap(value,name=="PreparePValueRowsForUdf"?"PvalueRows":"NormalizeBlanks").WithTriviaFrom(expr));
   }
  }
  var type=model.GetTypeInfo(expr).Type;
  if(type is IArrayTypeSymbol array&&array.Rank==2){Returns++;return node.WithExpression(Wrap(expr,"Raw").WithTriviaFrom(expr));}
  if(type?.ToDisplayString()=="BESHStatNG.ResultTable"){Returns++;return node.WithExpression(Wrap(expr,"NormalizeBlanks").WithTriviaFrom(expr));}
  // Calls between already typed report builders are preserved as delegation.
  if(expr is InvocationExpressionSyntax)return node;
  Unsupported.Add(node.GetLocation().GetLineSpan().StartLinePosition.Line+1+": "+node);return node;
 }
}
