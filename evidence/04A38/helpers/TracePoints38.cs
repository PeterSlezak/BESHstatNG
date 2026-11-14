using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

// Read-only syntax positions. No production rewrite; an independent reader can
// remove each declared insertion from the isolated coverage copy byte-for-byte.
var input = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var points = new List<object>();
foreach (var group in input.RootElement.GetProperty("members").EnumerateArray()
    .Where(r => new[] {"FunctionBlock", "SubBlock", "GetAccessorBlock", "SetAccessorBlock"}.Contains(r.GetProperty("kind").GetString()))
    .GroupBy(r => r.GetProperty("path").GetString()))
{
    var text = File.ReadAllText(group.Key!);
    var root = VisualBasicSyntaxTree.ParseText(text).GetRoot();
    foreach (var row in group)
    {
        var block = root.DescendantNodes().OfType<MethodBlockBaseSyntax>()
            .Single(n => n.Span.Start == row.GetProperty("span_start").GetInt32());
        if (block.Statements.Count == 0) continue;
        points.Add(new {path = group.Key, symbol_id = row.GetProperty("symbol_id").GetString(),
            insertion_utf16 = block.Statements[0].SpanStart});
    }
}
if (File.Exists(args[1])) throw new Exception("Append-only trace positions exist");
File.WriteAllText(args[1], JsonSerializer.Serialize(points, new JsonSerializerOptions {WriteIndented = true}) + "\n", new UTF8Encoding(false));
Console.WriteLine("Original-source coverage insertion positions: " + points.Count);
