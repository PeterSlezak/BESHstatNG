using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

const string Base = "45466a88e7567c9f4711ba4fbbe295a7a2e69bb4";
var root = Directory.GetCurrentDirectory();
string Git(params string[] args) {
    var info = new ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true };
    info.ArgumentList.Add("-c"); info.ArgumentList.Add("safe.directory=" + root.Replace('\\', '/'));
    foreach (var arg in args) info.ArgumentList.Add(arg);
    using var process = Process.Start(info)!;
    var text = process.StandardOutput.ReadToEnd(); var error = process.StandardError.ReadToEnd(); process.WaitForExit();
    if (process.ExitCode != 0) throw new Exception(error.Replace(root, "<REPO_ROOT>"));
    return text;
}
string Sha(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
string FileSha(string path) { using var file = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(file)).ToLowerInvariant(); }
SyntaxNode Parse(string path, string text) {
    var tree = VisualBasicSyntaxTree.ParseText(text, VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest)
        .WithPreprocessorSymbols(new Dictionary<string, object> { ["DEBUG"] = true, ["TRACE"] = true, ["CONFIG"] = "Debug", ["_MYTYPE"] = "Windows" }), path);
    var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length != 0) throw new Exception(path + ": " + string.Join(";", errors.Select(d => d.GetMessage())));
    return tree.GetRoot();
}
string Tokens(SyntaxNode node) => string.Join(" ", node.DescendantTokens().Select(t => t.Text));
SyntaxNode Header(SyntaxNode n) => n switch {
    MethodBlockSyntax m => m.SubOrFunctionStatement, ConstructorBlockSyntax c => c.SubNewStatement,
    OperatorBlockSyntax o => o.OperatorStatement, AccessorBlockSyntax a => a.AccessorStatement,
    PropertyBlockSyntax p => p.PropertyStatement, _ => n
};
IEnumerable<SyntaxNode> Members(SyntaxNode n) => n.DescendantNodes().Where(n => n is MethodBlockBaseSyntax or PropertyBlockSyntax);
string TypeName(SyntaxNode n) { var name = "BESHStatNG." + string.Join(".", n.Ancestors().Reverse().Select(a => a switch {
    NamespaceBlockSyntax ns => ns.NamespaceStatement.Name.ToString(), ClassBlockSyntax c => c.ClassStatement.Identifier.Text,
    ModuleBlockSyntax m => m.ModuleStatement.Identifier.Text, StructureBlockSyntax s => s.StructureStatement.Identifier.Text,
    _ => ""
}).Where(s => s != ""));
    if (n is AccessorBlockSyntax && n.Parent is PropertyBlockSyntax p) name += "." + p.PropertyStatement.Identifier.Text;
    return name;
}
string Name(SyntaxNode n) => Header(n) switch {
    MethodStatementSyntax m => m.Identifier.Text, SubNewStatementSyntax => ".ctor",
    PropertyStatementSyntax p => p.Identifier.Text, AccessorStatementSyntax a => a.AccessorKeyword.Text,
    OperatorStatementSyntax o => o.OperatorToken.Text, _ => Header(n).Kind().ToString()
};
string Key(SyntaxNode n) => TypeName(n) + "|" + n.Kind() + "|" + Tokens(Header(n));
string Id(string path, SyntaxNode n) => path + "|" + TypeName(n) + "|" + n.Kind() + "|" + Sha(Regex.Replace(Header(n).ToString(), @"\s+", " ").Trim())[..16] + "@" + n.Span.Start + ":" + n.Span.Length;
object Record(string path, SyntaxNode n) => new { member_id = Id(path, n), path, qualified_type = TypeName(n), name = Name(n),
    signature = Regex.Replace(Header(n).ToString(), @"\s+", " ").Trim(), start_line = n.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
    end_line = n.GetLocation().GetLineSpan().EndLinePosition.Line + 1, source_sha256 = FileSha(path), body_sha256 = Sha(n.ToString()) };

var portable = new[] { "RocDataCore", "NormalPlotDataCore", "BoxSegmentsCore", "HistogramDataCore", "ScatterMatrixDataCore", "XyzDataCore" }
    .Select(s => "BESHStat.Core/src/Graphics/" + s + ".vb")
    .Append("BESHStat.Excel.Common/src/Analysis/OrderedPlotGroups.vb").ToArray();
if (args.Contains("results-snapshot")) {
    var clock = Stopwatch.StartNew(); var metadata = new List<string>(); long bytes = 0;
    void Add(string directory) {
        foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)) {
            var file = new FileInfo(path); var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            metadata.Add(relative + "|" + file.Length + "|" + file.LastWriteTimeUtc.Ticks); bytes += file.Length;
        }
    }
    foreach (var directory in Directory.EnumerateDirectories(root).Where(d => !Path.GetFileName(d).EndsWith(".tmp") && Path.GetFileName(d) != ".git")) {
        if (Path.GetFileName(directory).Equals("TestResults", StringComparison.OrdinalIgnoreCase)) Add(directory);
        else foreach (var nested in Directory.EnumerateDirectories(directory, "TestResults", SearchOption.AllDirectories)) Add(nested);
    }
    metadata.Sort(StringComparer.Ordinal);
    var digest = Sha(string.Join('\n', metadata));
    using var prior = JsonDocument.Parse(File.ReadAllText("evidence/04A29/historical-results-entry.json"));
    var reference = prior.RootElement;
    var equal = reference.GetProperty("count").GetInt32() == metadata.Count && reference.GetProperty("bytes").GetInt64() == bytes && reference.GetProperty("membership_size_mtime_sha256").GetString() == digest;
    var result = new { status = equal ? "MATCH_ACCEPTED_METADATA_RECEIPT" : "METADATA_DELTA_REQUIRES_QUALIFICATION", count = metadata.Count, bytes,
        membership_size_mtime_sha256 = digest, reference_path = "evidence/04A29/historical-results-entry.json", reference_file_sha256 = FileSha("evidence/04A29/historical-results-entry.json"),
        qualification = "One scoped stat/membership traversal; historical payload NOT_REHASHED. No isolated *.tmp outputs counted; no historical writes authorized.", seconds = clock.Elapsed.TotalSeconds };
    File.WriteAllText("evidence/04A30/historical-results-preservation.json", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    Console.WriteLine($"Historical metadata:{metadata.Count} files;{bytes} bytes;match={equal};{clock.Elapsed.TotalSeconds:F2}s"); return;
}
if (args.Contains("format-new")) {
    foreach (var path in portable) File.WriteAllText(path, Parse(path, File.ReadAllText(path)).NormalizeWhitespace(indentation: "    ", eol: "\n").ToFullString() + "\n", new UTF8Encoding(false));
    Console.WriteLine("Formatted seven new portable files only."); return;
}
var compile = XDocument.Load("BESHstatNG/BESHstatNG.vbproj").Descendants().Where(e => e.Name.LocalName == "Compile")
    .Select(e => "BESHstatNG/" + e.Attribute("Include")!.Value.Replace('\\', '/')).Where(p => p.Contains("/Graphics/")).Order().ToArray();
var disk = Directory.GetFiles("BESHstatNG/src/Graphics", "*.vb").Select(p => p.Replace('\\', '/')).Order().ToArray();
if (compile.Length != 38 || !compile.SequenceEqual(disk)) throw new Exception("Compiled primary population changed.");
var allowed = new Dictionary<string, string[]> {
    ["ROC.vb"] = ["compute", "DeLongSE", "MidRanks"], ["NormalPlot.vb"] = [".ctor", "compute", "computeRefLIne"],
    ["BoxPlot.vb"] = ["AddBoxPlot", "wrapResults", "CalcForPlotting"], ["Histogram.vb"] = ["compute", "addChart"],
    ["ScatterPlotMatrix.vb"] = ["transformData"], ["XYZscatter.vb"] = ["getnorm"]
};
var changed = new List<object>(); var rows = new List<object>();
foreach (var path in compile) {
    var before = Members(Parse(path, Git("show", Base + ":" + path))).ToDictionary(Key);
    var current = Members(Parse(path, File.ReadAllText(path))).ToDictionary(Key);
    if (before.Keys.Except(current.Keys).Any()) throw new Exception("Windows declaration/signature/default changed: " + path);
    foreach (var extra in current.Keys.Except(before.Keys)) {
        var method = current[extra];
        if (Path.GetFileName(path) != "BoxPlot.vb" || Name(method) != "SnapshotGroupNames" || !Tokens(Header(method)).StartsWith("Private "))
            throw new Exception("Unexpected added Windows member: " + path);
        rows.Add(Record(path, method));
    }
    foreach (var (key, old) in before) {
        var now = current[key]; rows.Add(Record(path, now));
        if (Tokens(old) == Tokens(now)) continue;
        if (!allowed.GetValueOrDefault(Path.GetFileName(path), []).Contains(Name(now))) throw new Exception("Unexpected Windows member-body delta: " + path + " " + Name(now));
        changed.Add(new { old_git_blob_member_id = Id(path, old),
            identity_basis = "Git-blob AST surrogate; canonical accepted04A29 live-source IDs and exact byte/body pins are in migration-operation-map-v1.csv. Newline/offset bases are not silently equated.",
            current = Record(path, now), native_status = "NATIVE_NOT_RUN" });
        if (Path.GetFileName(path) == "Histogram.vb" && Name(now) == "addChart") {
            string Tail(SyntaxNode n) { var s = n.ToString(); return s[s.IndexOf("Dim anchor As Range", StringComparison.Ordinal)..]; }
            if (Tokens(Parse("native-tail.vb", "Module T\nSub S()\n" + Tail(old) + "\nEnd Module")) != Tokens(Parse("native-tail.vb", "Module T\nSub S()\n" + Tail(now) + "\nEnd Module"))) throw new Exception("Histogram chart/native tail changed.");
        }
    }
}
foreach (var path in portable) {
    var source = File.ReadAllText(path);
    if (!source.Contains("Option Strict On")) throw new Exception("Strict missing: " + path);
    if (Regex.IsMatch(source, @"ExcelDna|Office\.Interop|System\.Drawing|System\.Windows\.Forms|System\.IO|Microsoft\.VisualBasic|\bTimer\b", RegexOptions.IgnoreCase)) throw new Exception("Forbidden new portable dependency: " + path);
    rows.AddRange(Members(Parse(path, source)).Select(n => Record(path, n)));
}
// Existing engine identity for the unchanged ChartingFunc compatibility adapter.
var existingAxis = "BESHStat.Core/src/Graphics/ChartAxisScalingCore.vb";
rows.AddRange(Members(Parse(existingAxis, File.ReadAllText(existingAxis))).Select(n => Record(existingAxis, n)));
// These are the actual frozen member bodies, with access changes only.
var fixtureRoot = Parse("LegacyGraphics30.vb", File.ReadAllText("BESHStat.Core.Tests/src/Graphics/LegacyGraphics30.vb"));
var boxFixtureRoot = Parse("LegacyBoxBounds30.vb", File.ReadAllText("BESHStat.Core.Tests/src/Graphics/LegacyBoxBounds30.vb"));
var fixtureProofs = new List<object>();
foreach (var (file, type, names) in new[] {
    ("ROC.vb", "LegacyRoc30", new[]{"compute", "DeLongSE", "MidRanks"}),
    ("NormalPlot.vb", "LegacyNormal30", new[]{".ctor", "compute", "computeRefLIne"}),
    ("ScatterPlotMatrix.vb", "LegacyScatter30", new[]{"transformData"}),
    ("XYZscatter.vb", "LegacyXyz30", new[]{"getnorm"}), ("BoxPlot.vb", "LegacyBox30", new[]{"CalcForPlotting"}) }) {
    var oldRoot = Parse(file, Git("show", Base + ":BESHstatNG/src/Graphics/" + file));
    foreach (var name in names) {
        var old = Members(oldRoot).Single(n => Name(n) == name);
        var fixture = Members(file == "BoxPlot.vb" ? boxFixtureRoot : fixtureRoot).Single(n => Name(n) == name && TypeName(n).EndsWith("." + type));
        string Body(SyntaxNode n) => string.Join(" ", n.ChildNodes().Skip(1).Select(Tokens));
        if (Body(old) != Body(fixture)) throw new Exception("Frozen fixture body changed: " + file + " " + name);
        fixtureProofs.Add(new { source = file, member = name, original_statement_tokens_sha256 = Sha(Body(old)), fixture_statement_tokens_sha256 = Sha(Body(fixture)), status = "IDENTICAL_STATEMENTS", scientific = "NOT_GOLDEN" });
    }
}
var output = new { status = "PASS", @base = Base, compiled_windows_graphics = compile.Length, unchanged_windows_declarations = true,
    changed_member_count = changed.Count, changed, current_members = rows, frozen_fixture_proofs = fixtureProofs,
    portable_dependency_guard = "PASS", native_chart_appearance = "NATIVE_NOT_RUN", semantic_limit = "AST source/contract proof and managed tests; not native execution or independent science" };
File.WriteAllText("evidence/04A30/source-ownership-guard.json", JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"PASS:38 compiled modules; all Windows declarations preserved;{changed.Count} bounded member deltas;seven portable files;{fixtureProofs.Count} exact frozen method bodies.");
