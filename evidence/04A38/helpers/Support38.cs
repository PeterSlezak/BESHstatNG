using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
string Owner(SyntaxNode n)=>"BESHStatNG."+string.Join(".",n.Ancestors().Reverse().Select(a=>a is NamespaceBlockSyntax ns?ns.NamespaceStatement.Name.ToString():a is TypeBlockSyntax t?t.BlockStatement.Identifier.ValueText:null).Where(v=>v!=null));
var rows=new List<object>();
foreach(var p in Directory.GetFiles("BESHstatNG/src/ExcelUDFs","*.vb",SearchOption.AllDirectories).Order()){
 var tree=VisualBasicSyntaxTree.ParseText(File.ReadAllText(p));string path=p.Replace('\\','/');
 foreach(var n in tree.GetRoot().DescendantNodes().Where(n=>n is TypeBlockSyntax or FieldDeclarationSyntax||n is PropertyStatementSyntax&&n.Parent is not PropertyBlockSyntax)){
  var declaration=n is TypeBlockSyntax type?(SyntaxNode)type.BlockStatement:n;
  bool primary=n is FieldDeclarationSyntax field&&field.Declarators.Any(d=>d.Initializer!=null||d.AsClause is AsNewClauseSyntax)||n is PropertyStatementSyntax property&&(property.Initializer!=null||property.AsClause is AsNewClauseSyntax);
  rows.Add(new{path,owner=Owner(n),kind=n.Kind().ToString(),start_line=n.GetLocation().GetLineSpan().StartLinePosition.Line+1,span_start=n.Span.Start,span_length=n.Span.Length,declaration=declaration.ToString(),declaration_sha256=Hash(declaration.ToString()),already_in_primary_population=primary,
    ownership="WINDOWS_DECLARATION_AND_MODEL_CACHE_SHAPE; source-level native/session boundary, not a portable algorithm or discharged lifecycle obligation"});
 }
}
string output=args[0];if(File.Exists(output))throw new Exception("Append-only census exists");
File.WriteAllText(output,JsonSerializer.Serialize(new{scope="Supplemental type/field/uninitialized auto-property population; distinct from704 behavioral nodes/703 historical reviews; explicit overlap flags",rows},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine("Supporting declarations: "+rows.Count+"; overlaps with primary: "+rows.Count(r=>JsonSerializer.SerializeToElement(r).GetProperty("already_in_primary_population").GetBoolean()));
