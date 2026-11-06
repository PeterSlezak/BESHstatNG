using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

// Read-only source analysis. No product assembly is loaded or executed.
static partial class Program
{
    static string Root = Directory.GetCurrentDirectory();
    static string Out = Path.Combine(Root, "evidence/04A29");
    static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    static string Rel(string p) => Path.GetRelativePath(Root, p).Replace('\\', '/');
    static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    static string ShaText(string text) => Sha(Encoding.UTF8.GetBytes(text));
    static string FileSha(string p) { using var f = File.OpenRead(p); return Convert.ToHexString(SHA256.HashData(f)).ToLowerInvariant(); }
    static string Norm(string s) => Regex.Replace(s, @"\s+", " ").Trim();
    static string Git(params string[] args)
    {
        var i = new ProcessStartInfo("git") { WorkingDirectory = Root, RedirectStandardOutput = true, RedirectStandardError = true };
        i.ArgumentList.Add("-c"); i.ArgumentList.Add("safe.directory=" + Root.Replace('\\', '/'));
        foreach (var a in args) i.ArgumentList.Add(a);
        using var p = Process.Start(i)!; var s = p.StandardOutput.ReadToEnd(); var err = p.StandardError.ReadToEnd(); p.WaitForExit();
        if (p.ExitCode != 0) throw new Exception("git " + string.Join(' ', args) + ": " + err.Replace(Root, "<REPO_ROOT>"));
        return s.TrimEnd();
    }
    static void Write(string name, object data) => File.WriteAllText(Path.Combine(Out, name), JsonSerializer.Serialize(data, Json) + "\n", new UTF8Encoding(false));
    static Dictionary<string,string> Row(params string[] pairs) { var d = new Dictionary<string,string>(); for (int i=0;i<pairs.Length;i+=2)d.Add(pairs[i],pairs[i+1]); return d; }
    static List<Dictionary<string,string>> CsvRead(string p)
    {
        using var r = new TextFieldParser(p); r.SetDelimiters(","); r.HasFieldsEnclosedInQuotes=true;
        var headers=r.ReadFields()!; var result=new List<Dictionary<string,string>>();
        while (!r.EndOfData) { var fields=r.ReadFields()!; result.Add(headers.Select((h,i)=>(h,fields[i])).ToDictionary(x=>x.h,x=>x.Item2)); }
        return result;
    }
    static void Csv(string name, List<Dictionary<string,string>> rows)
    {
        var keys=rows.SelectMany(x=>x.Keys).Distinct().ToArray();
        string Q(string s) => "\""+s.Replace("\"","\"\"")+"\"";
        using var w=new StreamWriter(Path.Combine(Out,name),false,new UTF8Encoding(false));
        w.WriteLine(string.Join(",",keys.Select(Q)));
        foreach(var r in rows)w.WriteLine(string.Join(",",keys.Select(k=>Q(r.GetValueOrDefault(k,"")))));
    }
    static object Pin(string p) => new { path=Rel(p), bytes=new FileInfo(p).Length, sha256=FileSha(p) };
    static string[] ProtectedPaths() => Git("ls-files").Split('\n').Where(p=>p!="" && !p.StartsWith("evidence/04A29/") && p!="docs/architecture/batch-04A-29-owner-review.md").Order().ToArray();
    static string[] HistoricalPaths() => new[]{"docs/architecture/WindowsMigrationLedger","docs/architecture/04A02","evidence"}.SelectMany(d=>Directory.EnumerateFiles(d,"*",SearchOption.AllDirectories)).Select(Path.GetFullPath).Where(p=>!Rel(p).StartsWith("evidence/04A29/")).Order().ToArray();
    static object ResultsSnapshot()
    {
        // One scoped stat traversal; no historical TestResults payload read/rehash.
        var rows=new List<string>(); long bytes=0;
        foreach(var dir in Directory.EnumerateDirectories(Root).Where(d=>!Path.GetFileName(d).EndsWith(".tmp") && Path.GetFileName(d)!=".git"))
        {
            if(Path.GetFileName(dir).Equals("TestResults",StringComparison.OrdinalIgnoreCase))Add(dir);
            else foreach(var d in Directory.EnumerateDirectories(dir,"TestResults",SearchOption.AllDirectories))Add(d);
        }
        void Add(string d) { foreach(var f in Directory.EnumerateFiles(d,"*",SearchOption.AllDirectories)) { var i=new FileInfo(f); rows.Add(Rel(f)+"|"+i.Length+"|"+i.LastWriteTimeUtc.Ticks); bytes+=i.Length; } }
        rows.Sort(StringComparer.Ordinal); return new { count=rows.Count, bytes, membership_size_mtime_sha256=ShaText(string.Join('\n',rows)), qualification="metadata/membership only; payload bytes NOT_REHASHED; no historical writes" };
    }
    static void Freeze()
    {
        if(File.Exists(Path.Combine(Out,"entry-state.json")))throw new Exception("Refuse to replace entry freeze");
        if(Git("branch","--show-current")!="codex/04A-29" || Git("rev-parse","HEAD")!=Git("rev-parse","main") || Git("diff","--name-only")!="" || Git("diff","--cached","--name-only")!="")throw new Exception("Entry safety failure");
        var sw=Stopwatch.StartNew();
        Write("protected-files-entry.json",ProtectedPaths().Select(p=>Pin(p)).ToArray());
        Write("historical-files-entry.json",HistoricalPaths().Select(p=>Pin(p)).ToArray());
        Write("historical-results-entry.json",ResultsSnapshot());
        Write("entry-state.json",new { branch=Git("branch","--show-current"), @base=Git("rev-parse","main"), head=Git("rev-parse","HEAD"), parent=Git("rev-parse","HEAD^"), accepted_04A28=Git("rev-parse","ed94086"), accepted_test_rationalization=Git("rev-parse","033cbe9"), ancestry="both verified ancestors of main; linear/fast-forward integration; owner acceptance supplied in request", owner_edits=Array.Empty<string>(), index_empty=true, repo="<REPO_ROOT>", plan=Pin("planning/BESHStatNG_CrossPlatform_MasterPlan.md"), plan_revision="2.11", authorities="protected-files-entry.json and historical-files-entry.json", writer_observation="No dotnet/MSBuild/VBCSCompiler/python/Excel writer at entry; existing Node service processes not used. Single new audit writer, no product build/shared output. Not a permanent lease.", ignored_outputs="Existing bin/obj/TestResults/packages/.vs and prior .test-rationalization roots retained; original ledger is ignored local authority and must accompany owner's complete ZIP.", initial_git_failure="Git dubious ownership; per-command safe.directory only; branch index.lock denied in sandbox then separately approved", started_utc=DateTime.UtcNow.ToString("O"), freeze_seconds=sw.Elapsed.TotalSeconds, gates=new { P="NO",N1="NO GO",N2="NO",C2="UNMET" }, native_releases=0, obligation_discharges=0 });
        Console.WriteLine("Entry frozen; seconds="+sw.Elapsed.TotalSeconds.ToString("F2",CultureInfo.InvariantCulture));
    }
    static SyntaxNode Header(SyntaxNode n) => n switch { MethodBlockBaseSyntax b=>b.BlockStatement, PropertyBlockSyntax b=>b.PropertyStatement, EventBlockSyntax b=>b.EventStatement, LambdaExpressionSyntax b=>b.SubOrFunctionHeader, _=>n };
    static string Name(SyntaxNode n) => Header(n) switch { MethodStatementSyntax s=>s.Identifier.ValueText, SubNewStatementSyntax=>".ctor", OperatorStatementSyntax s=>"Operator "+s.OperatorToken.Text, AccessorStatementSyntax s=>s.AccessorKeyword.Text, PropertyStatementSyntax s=>s.Identifier.ValueText, EventStatementSyntax s=>s.Identifier.ValueText, DeclareStatementSyntax s=>s.Identifier.ValueText, VariableDeclaratorSyntax s=>string.Join(",",s.Names.Select(x=>x.Identifier.ValueText)), EnumMemberDeclarationSyntax s=>s.Identifier.ValueText, _=>n is LambdaExpressionSyntax?"lambda":"initializer" };
    static bool IsMember(SyntaxNode n) => n is MethodBlockBaseSyntax or PropertyBlockSyntax or EventBlockSyntax or LambdaExpressionSyntax or DeclareStatementSyntax || n is MethodStatementSyntax && n.Parent is not MethodBlockBaseSyntax || n is EventStatementSyntax && n.Parent is not EventBlockSyntax;
    static bool IsInitializer(SyntaxNode n) => n is VariableDeclaratorSyntax v && (v.Initializer!=null || v.AsClause is AsNewClauseSyntax) && (v.Parent is FieldDeclarationSyntax || v.Parent is LocalDeclarationStatementSyntax l && l.Modifiers.Any(SyntaxKind.StaticKeyword)) || n is PropertyStatementSyntax p && (p.Initializer!=null || p.AsClause is AsNewClauseSyntax);
    static string TypeName(SyntaxNode n,string root)
    {
        var parts=new List<string>{root}; foreach(var a in n.Ancestors().Reverse()) { if(a is NamespaceBlockSyntax ns)parts.Add(ns.NamespaceStatement.Name.ToString()); if(a is TypeBlockSyntax t)parts.Add(t.BlockStatement.Identifier.ValueText); if(a is EnumBlockSyntax e)parts.Add(e.EnumStatement.Identifier.ValueText); }
        var s=string.Join('.',parts.Where(x=>x!="")); return s.Replace(root+".Global.","");
    }
    static int Start(SyntaxNode n)=>n.GetLocation().GetLineSpan().StartLinePosition.Line+1;
    static int End(SyntaxNode n)=>n.GetLocation().GetLineSpan().EndLinePosition.Line+1;
    static int DeclarationLine(SyntaxNode n) {var h=Header(n);var tokens=h.ChildTokens().ToArray();var token=tokens.Length>0?tokens[0]:h.GetFirstToken();return token.GetLocation().GetLineSpan().StartLinePosition.Line+1;}
    static List<(string path, SyntaxNode root, Dictionary<SyntaxNode,Dictionary<string,string>> members)> Trees=[];
    static List<Dictionary<string,string>> Parse(string path,string ns,bool primary)
    {
        var source=File.ReadAllText(path); var tree=VisualBasicSyntaxTree.ParseText(source,VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithPreprocessorSymbols(new Dictionary<string,object>{{"DEBUG",true},{"TRACE",true},{"CONFIG","Debug"},{"_MYTYPE","Windows"}}),path:path);
        var errors=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray(); if(errors.Length>0)throw new Exception("Syntax errors: "+path+" "+string.Join(";",errors.Select(d=>d.ToString())));
        var root=tree.GetRoot(); var map=new Dictionary<SyntaxNode,Dictionary<string,string>>(); var result=new List<Dictionary<string,string>>();
        foreach(var n in root.DescendantNodes().Where(n=>IsMember(n)||IsInitializer(n)||n is PropertyStatementSyntax && n.Parent is not PropertyBlockSyntax||n is VariableDeclaratorSyntax && n.Parent is FieldDeclarationSyntax||n is EnumMemberDeclarationSyntax))
        {
            var sig=Norm(Header(n).ToString()); var type=TypeName(n,ns); if(n is AccessorBlockSyntax)type+="."+Name(n.Parent!);
            var key=path+"|"+type+"|"+n.Kind()+"|"+ShaText(sig)[..16]; var id=key+"@"+n.Span.Start+":"+n.Span.Length;
            var r=Row("member_id",id,"signature_key",key,"path",path,"qualified_type",type,"name",Name(n),"kind",n.Kind().ToString(),"signature",sig,"start_line",Start(n).ToString(),"end_line",End(n).ToString(),"span_start",n.Span.Start.ToString(),"span_length",n.Span.Length.ToString(),"visibility",string.Join(' ',Header(n).ChildTokens().Where(t=>t.IsKind(SyntaxKind.PublicKeyword)||t.IsKind(SyntaxKind.PrivateKeyword)||t.IsKind(SyntaxKind.FriendKeyword)||t.IsKind(SyntaxKind.ProtectedKeyword)).Select(t=>t.Text)),"source_sha256",FileSha(path),"body_sha256",ShaText(n.ToString()),"body_lf_sha256",ShaText(n.ToString().Replace("\r\n","\n")),"compiled","YES","reachability","STATIC_REFERENCE_ONLY","confidence","SOURCE_INFERRED","native_status","NOT_RUN","scientific_status","NOT_GOLDEN");
            if(r["visibility"]=="")r["visibility"]="implicit VB; declaration/context applies";
            if(n.Parent is FieldDeclarationSyntax field){var mods=string.Join(' ',field.Modifiers.Select(t=>t.Text));r["field_modifiers"]=mods;r["visibility"]=string.Join(' ',field.Modifiers.Where(t=>t.IsKind(SyntaxKind.PublicKeyword)||t.IsKind(SyntaxKind.PrivateKeyword)||t.IsKind(SyntaxKind.FriendKeyword)||t.IsKind(SyntaxKind.ProtectedKeyword)).Select(t=>t.Text));if(r["visibility"]=="")r["visibility"]="implicit VB field visibility; exact modifiers/context retained";}
            r["declared_name_count"]=n is VariableDeclaratorSyntax declarator?declarator.Names.Count.ToString():"1";
            r["declaration_line"]=DeclarationLine(n).ToString();
            r["census_policy"]=IsMember(n)||IsInitializer(n)?"ORIGINAL_EXECUTABLE_INITIALIZER_LAMBDA_POLICY":"EXTENDED_STATE_DECLARATION";
            map[n]=r; result.Add(r);
        }
        foreach(var (n,r) in map)r["enclosing_member_id"]=n.Ancestors().Select(a=>map.GetValueOrDefault(a)).FirstOrDefault(x=>x!=null)?.GetValueOrDefault("member_id")??"type-scope";
        Trees.Add((path,root,map)); return result;
    }
    static void Inventory()
    {
        var sw=Stopwatch.StartNew();
        var project=XDocument.Load("BESHstatNG/BESHstatNG.vbproj"); var comp=project.Descendants().Where(x=>x.Name.LocalName=="Compile").Select(x=>new { path="BESHstatNG/"+x.Attribute("Include")!.Value.Replace('\\','/'), condition=x.Attribute("Condition")?.Value??x.Parent?.Attribute("Condition")?.Value??"", link=x.Elements().FirstOrDefault(e=>e.Name.LocalName=="Link")?.Value??"" }).Where(x=>x.path.Contains("/Graphics/")).OrderBy(x=>x.path,StringComparer.Ordinal).ToArray();
        var disk=Directory.GetFiles("BESHstatNG/src/Graphics","*.vb").Select(x=>x.Replace('\\','/')).Order(StringComparer.Ordinal).ToArray();
        if(!disk.SequenceEqual(comp.Select(x=>x.path)))throw new Exception("Compilation/disk discrepancy");
        var files=new List<Dictionary<string,string>>(); var members=new List<Dictionary<string,string>>();
        foreach(var c in comp)
        {
            var rs=Parse(c.path,"BESHStatNG",true); members.AddRange(rs); var tree=Trees.Last().root;
            files.Add(Row("path",c.path,"sha256",FileSha(c.path),"bytes",new FileInfo(c.path).Length.ToString(),"text_lines",File.ReadAllLines(c.path).Length.ToString(),"compiled","YES","compile_condition",c.condition,"linked_path",c.link,"type_count",tree.DescendantNodes().Count(n=>n is TypeBlockSyntax or EnumBlockSyntax).ToString(),"member_count",rs.Count.ToString(),"conditional_directives",tree.DescendantTrivia(descendIntoTrivia:true).Count(t=>t.IsDirective).ToString(),"generated","NO","partial",tree.DescendantTokens().Any(t=>t.IsKind(SyntaxKind.PartialKeyword)).ToString()));
        }
        var portable=new List<Dictionary<string,string>>();
        foreach(var p in Directory.GetFiles("BESHStat.Core/src/Graphics","*.vb").Order())portable.AddRange(Parse(p.Replace('\\','/'),"BESHStatNG",false));
        foreach(var p in new[]{"BESHStat.Excel.Common/src/Analysis/SpcUnivariateChartProjectionPolicy.vb","BESHStat.Excel.Common/src/Analysis/SpcMultivariateChartProjectionPolicy.vb","BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb"}.Where(File.Exists))portable.AddRange(Parse(p,"BESHStatNG",false));
        Csv("graphics-files-v1.csv",files); Csv("graphics-members-raw-v1.csv",members); Csv("portable-members-v1.csv",portable);
        // AST invocation/object-creation occurrences. No binding/execution assertion.
        var edges=new List<Dictionary<string,string>>();
        foreach(var (path,root,map) in Trees)foreach(var n in root.DescendantNodes().Where(n=>n is InvocationExpressionSyntax or ObjectCreationExpressionSyntax or AddRemoveHandlerStatementSyntax))
        {
            var owner=n.Ancestors().Select(a=>map.GetValueOrDefault(a)).FirstOrDefault(x=>x!=null);
            var target=n switch { InvocationExpressionSyntax i=>i.Expression.ToString(), ObjectCreationExpressionSyntax o=>"New "+o.Type.ToString(), _=>n.ToString() };
            edges.Add(Row("edge_id",path+"@"+n.Span.Start+":"+n.Span.Length,"caller_id",owner?.GetValueOrDefault("member_id")??"type-scope","caller_path",path,"line",Start(n).ToString(),"target_expression",Norm(target),"callee_id","UNRESOLVED_STATIC_EXPRESSION","confidence","SOURCE_INFERRED","reachability","STATIC_REFERENCE_ONLY","source_sha256",FileSha(path),"host_crossing","review expression; no semantic binding"));
        }
        Csv("graphics-call-edges-v1.csv",edges);
        // Independent count: each declaration header is paired to AST owner, distinct from historical state/lambda-inclusive policy.
        var declarations=Trees.Where(t=>disk.Contains(t.path)).SelectMany(t=>t.root.DescendantNodes().Where(n=>n is MethodStatementSyntax or SubNewStatementSyntax or PropertyStatementSyntax or OperatorStatementSyntax or EventStatementSyntax or DeclareStatementSyntax or AccessorStatementSyntax).Select(n=>new{path=t.path,line=Start(n),declaration_line=n.ChildTokens().First().GetLocation().GetLineSpan().StartLinePosition.Line+1,kind=n.Kind().ToString(),signature=Norm(n.ToString()),owner=t.members.ContainsKey(n)?t.members[n]["member_id"]:n.Ancestors().Where(t.members.ContainsKey).Select(a=>t.members[a]["member_id"]).FirstOrDefault()??"UNMAPPED"})).ToArray();
        var lexicalRows=new List<Dictionary<string,string>>();
        foreach(var path in disk)
        {
            var expected=declarations.Where(d=>d.path==path).Select(d=>d.declaration_line).ToHashSet();var lexical=new HashSet<int>();var lines=File.ReadAllLines(path);
            for(int i=0;i<lines.Length;i++)if(Regex.IsMatch(lines[i],@"^\s*(?:(?:Public|Private|Friend|Protected|Shared|ReadOnly|WriteOnly|Default|Overloads|Overrides|Overridable|MustOverride|NotOverridable|Shadows|Widening|Narrowing|Static|Async|Iterator|Custom)\s+)*(?:(?:Sub|Function|Property|Operator|Event|Declare)\b|Get\s*$|Set\s*\()",RegexOptions.IgnoreCase))lexical.Add(i+1);
            lexicalRows.Add(Row("path",path,"ast_header_count",expected.Count.ToString(),"independent_anchored_declaration_count",lexical.Count.ToString(),"ast_only_lines",string.Join(';',expected.Except(lexical).Order()),"lexical_only_lines",string.Join(';',lexical.Except(expected).Order()),"attribute_start_to_declaration",string.Join(';',declarations.Where(d=>d.path==path&&d.line!=d.declaration_line).Select(d=>d.line+"->"+d.declaration_line)),"authority","Roslyn defines semantic census; anchored scan is an independent sampling/reconciliation check, not a parser substitute. Attribute start and declaration-token line are separately reconciled."));
        }
        Csv("independent-declaration-reconciliation-v1.csv",lexicalRows);
        Write("declaration-reconciliation.json",new { parser="Installed SDK Roslyn VisualBasicSyntaxTree; syntax only, not semantic compilation or native runtime", source_profiles="No #If or Partial tokens in primary files; project Graphics Include items unconditional. Debug/Release and x86/x64 population identical by source inspection.", definition="Extended census: 04A.01 executable/state-initializer/lambda population PLUS every uninitialized auto-property, field declarator and enum constant. These additional declarations are not presumed new source. Independent header pass reconciles methods/constructors/properties/operators/events/accessors, including headers nested in blocks.", files=disk.Length, members=members.Count, declarations=declarations.Length, unmapped=declarations.Where(d=>d.owner=="UNMAPPED").ToArray(), rows=declarations, duration_seconds=sw.Elapsed.TotalSeconds });
        Console.WriteLine($"Inventory: {disk.Length} files, {members.Count} extended members, {declarations.Length} declaration headers; {sw.Elapsed.TotalSeconds:F2}s");
    }
    static int Main(string[] args)
    {
        try { if(args.Length!=1)throw new Exception("freeze | inventory | report | verify | self-test | qualify-entry-privacy | close | publish-audit | correct-lineage | finalize-lineage | publish-lineage"); if(args[0]=="freeze")Freeze(); else if(args[0]=="inventory")Inventory(); else if(args[0]=="report")Report(); else if(args[0]=="verify")Verify(); else if(args[0]=="self-test")SelfTest(); else if(args[0]=="qualify-entry-privacy")QualifyEntryPrivacy(); else if(args[0]=="close")Close(); else if(args[0]=="publish-audit")PublishAudit(); else if(args[0]=="correct-lineage")WriteLineageV2(); else if(args[0]=="finalize-lineage")FinalizeLineageCorrection(); else if(args[0]=="publish-lineage")FinalizeLineageCorrection(true); else throw new Exception("Unknown command"); return 0; }
        catch(Exception e) { Console.Error.WriteLine(e.Message.Replace(Root,"<REPO_ROOT>")); return 1; }
    }
}
