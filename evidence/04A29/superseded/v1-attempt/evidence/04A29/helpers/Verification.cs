using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

static partial class Program
{
    static readonly string[] Ownership=["CORE_EXISTING","CORE_CANDIDATE","COMMON_EXISTING","COMMON_CANDIDATE","WINDOWS_NATIVE_ONLY","WINDOWS_BRIDGE_REQUIRED","SHARED_MIXED_SPLIT","UNKNOWN_BLOCKED"];
    static List<string> CheckRows(List<Dictionary<string,string>> rows,HashSet<string> paths)
    {
        var errors=new List<string>();
        if(rows.Select(r=>r["member_id"]).Distinct().Count()!=rows.Count)errors.Add("duplicate member ID");
        if(!rows.Select(r=>r["path"]).ToHashSet().SetEquals(paths))errors.Add("file coverage mismatch");
        foreach(var r in rows)
        {
            foreach(var k in new[]{"member_id","source_sha256","qualified_type","signature","start_line","end_line","ownership","algorithm_or_geometry","contract_gap","behavior_to_preserve","responsible_owner","suboperation_classifications","native_status","scientific_status"})if(string.IsNullOrWhiteSpace(r.GetValueOrDefault(k)))errors.Add("empty "+k);
            if(!Ownership.Contains(r["ownership"]))errors.Add("invalid ownership");
            if(r["native_status"]!="NOT_RUN"||r["scientific_status"]!="NOT_GOLDEN")errors.Add("unauthorized status");
            if(int.Parse(r["start_line"])>int.Parse(r["end_line"]))errors.Add("inverted span");
            if(r["ownership"]=="SHARED_MIXED_SPLIT"&&!r["suboperation_classifications"].Contains("WINDOWS_NATIVE_ONLY"))errors.Add("missing mixed native boundary");
        }
        return errors;
    }
    static List<string> CheckData()
    {
        var errors=new List<string>();
        var files=CsvRead(Path.Combine(Out,"graphics-files-v1.csv"));var members=CsvRead(Path.Combine(Out,"graphics-members-v1.csv"));
        var paths=files.Select(f=>f["path"]).ToHashSet();errors.AddRange(CheckRows(members,paths));
        var raw=CsvRead(Path.Combine(Out,"graphics-members-raw-v1.csv"));
        if(!raw.Select(r=>r["member_id"]).ToHashSet().SetEquals(members.Select(r=>r["member_id"])))errors.Add("raw/classified IDs mismatch");
        if(files.Count!=38||paths.Count!=files.Count)errors.Add("population drift");
        var privacyReceipt=Path.Combine(Out,"entry-privacy-qualification.json");
        if(File.Exists(privacyReceipt)){using var qualification=JsonDocument.Parse(File.ReadAllText(privacyReceipt));var derivative=qualification.RootElement.GetProperty("public_derivative");if(FileSha(derivative.GetProperty("path").GetString()!)!=derivative.GetProperty("sha256").GetString())errors.Add("entry privacy derivative pin drift");var local=Path.Combine(Root,".04a29-audit.tmp","protected-files-entry-raw.json");if(File.Exists(local)&&FileSha(local)!=qualification.RootElement.GetProperty("raw_local_receipt").GetProperty("sha256").GetString())errors.Add("private raw entry receipt drift");}
        foreach(var f in files)if(FileSha(f["path"])!=f["sha256"]||new FileInfo(f["path"]).Length!=long.Parse(f["bytes"]))errors.Add("source file drift "+f["path"]);
        var xp=XDocument.Load("BESHstatNG/BESHstatNG.vbproj");var compile=xp.Descendants().Where(x=>x.Name.LocalName=="Compile"&&x.Attribute("Include")!.Value.Contains("Graphics\\")).Select(x=>"BESHstatNG/"+x.Attribute("Include")!.Value.Replace('\\','/')).ToHashSet();
        if(!paths.SetEquals(compile))errors.Add("Compile membership drift");
        if(!paths.SetEquals(Directory.GetFiles("BESHstatNG/src/Graphics","*.vb").Select(p=>p.Replace('\\','/'))))errors.Add("disk population drift");
        var old=CsvRead("docs/architecture/WindowsMigrationLedger/symbols.csv").Where(r=>r["path"].StartsWith("BESHstatNG/src/Graphics/")).ToArray();
        var cross=CsvRead(Path.Combine(Out,"04A01-graphics-member-crosswalk-v1.csv"));
        using var originalDecisions=JsonDocument.Parse(File.ReadAllText("scripts/windows-migration-audit/review-decisions.json"));var orderedIds=originalDecisions.RootElement.EnumerateArray().Select(d=>d.GetProperty("symbol_id").GetString()!).Where(id=>id.StartsWith("BESHstatNG/src/Graphics/")).ToArray();
        if(cross.Count!=old.Length||!cross.Select(r=>r["historical_member_id"]).SequenceEqual(orderedIds))errors.Add("ordered historical completeness failure");
        var valid=members.Concat(CsvRead(Path.Combine(Out,"portable-members-v1.csv"))).Select(r=>r["member_id"]).ToHashSet();
        foreach(var r in cross)foreach(var id in (r["current_member_ids"]+";"+r["portable_split_ids"]).Split(';',StringSplitOptions.RemoveEmptyEntries))if(!valid.Contains(id))errors.Add("invalid crosswalk target");
        var edges=CsvRead(Path.Combine(Out,"graphics-call-edges-v1.csv"));
        if(edges.Select(e=>e["edge_id"]).Distinct().Count()!=edges.Count)errors.Add("duplicate edge");
        foreach(var e in edges)if(e["caller_id"]!="type-scope"&&!valid.Contains(e["caller_id"]))errors.Add("invalid edge caller");
        var ops=CsvRead(Path.Combine(Out,"graphics-operations-v1.csv"));if(!ops.Select(r=>r["member_id"]).ToHashSet().SetEquals(members.Select(r=>r["member_id"])))errors.Add("operation coverage");
        using var decl=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,"declaration-reconciliation.json")));if(decl.RootElement.GetProperty("unmapped").GetArrayLength()!=0)errors.Add("unmapped declaration");
        foreach(var row in CsvRead(Path.Combine(Out,"independent-declaration-reconciliation-v1.csv")))if(row["ast_only_lines"]!=""||row["lexical_only_lines"]!="")errors.Add("independent declaration mismatch "+row["path"]);
        foreach(var row in CsvRead(Path.Combine(Out,"graphics-suboperations-v1.csv")))if(!valid.Contains(row["member_id"])||!Ownership.Contains(row["ownership"]))errors.Add("invalid suboperation identity/ownership");
        foreach(var r in cross)if(!new[]{"UNCHANGED_MATCHED_SOURCE","CURRENT_RELOCATED_OR_REFACTORED","CURRENT_NEW_MEMBER","CURRENT_REMOVED_OR_RENAMED","HISTORICAL_UNRESOLVED","SPLIT_OPERATION","UNCERTAIN_DIFFERENT_SOURCE"}.Contains(r["category"]))errors.Add("crosswalk category");
        foreach(var file in Directory.GetFiles("BESHStat.Core/src/Graphics","*.vb"))if(!File.ReadAllText(file).Contains("Option Strict On")||Regex.IsMatch(File.ReadAllText(file),@"(?im)^\s*Imports\s+(Microsoft\.Office|ExcelDna|System\.Windows\.Forms|System\.Drawing|Microsoft\.VisualBasic|System\.IO)"))errors.Add("Core Graphics dependency/strict guard "+file);
        if(Directory.GetFiles("BESHStat.Core/src/Graphics","*.vb").Length!=12)errors.Add("Core population changed");
        foreach(var t in CsvRead(Path.Combine(Out,"existing-graphics-test-fixtures-v1.csv")))if(FileSha(t["path"])!=t["source_sha256"])errors.Add("fixture source drift");
        using var casebook=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,"graphics-family-casebook-v1.json")));
        if(!casebook.RootElement.GetProperty("cases").EnumerateArray().Select(c=>c.GetProperty("family").GetString()!).ToHashSet().SetEquals(files.Select(f=>f["family"])))errors.Add("casebook family coverage");
        foreach(var c in casebook.RootElement.GetProperty("cases").EnumerateArray())if(c.GetProperty("native_status").GetString()!="NATIVE_NOT_RUN"||c.GetProperty("scientific_status").GetString()!="NOT_GOLDEN")errors.Add("casebook status promotion");
        return errors;
    }
    static void Verify()
    {
        var sw=Stopwatch.StartNew(); var errors=CheckData();
        var manifest=Path.Combine(Out,"publication-manifest.json");
        if(File.Exists(manifest))
        {
            using var doc=JsonDocument.Parse(File.ReadAllText(manifest));
            var entries=doc.RootElement.GetProperty("files").EnumerateArray().ToArray();
            var actual=CandidateFiles();
            if(!actual.Select(Rel).ToHashSet().SetEquals(entries.Select(e=>e.GetProperty("path").GetString()!)))errors.Add("publication membership mismatch");
            foreach(var e in entries.Where(e=>e.GetProperty("path").GetString()!="evidence/04A29/publication-manifest.json"))
            {
                var path=e.GetProperty("path").GetString()!;if(!SafePublicationPath(path))errors.Add("path escape");
                else if(!File.Exists(path)||FileSha(path)!=e.GetProperty("sha256").GetString()||new FileInfo(path).Length!=e.GetProperty("bytes").GetInt64())errors.Add("publication hash/byte mismatch "+path);
            }
        }
        if(Git("diff","--cached","--name-only")!="")errors.Add("index not empty");
        if(Git("diff","--name-only")!="")errors.Add("tracked product/authority edit");
        foreach(var p in CandidateFiles())if(PrivateText(File.ReadAllText(p)))errors.Add("privacy pattern in "+Rel(p));
        Git("diff","--check");
        if(errors.Count>0)throw new Exception(string.Join('\n',errors.Distinct()));
        Console.WriteLine("PASS: audit schema, unique IDs,38-file compiled coverage, all-member classification, ordered1062 crosswalk, references, status enums, fixture/source hashes"+(File.Exists(manifest)?", publication SHA/bytes":"; publication manifest not yet present")+$"; {sw.Elapsed.TotalSeconds:F2}s");
    }
    static void SelfTest()
    {
        var results=new List<object>();
        void Test(string name,Action action) {action();results.Add(new {name,outcome="PASS"});}
        void Expect(bool yes) {if(!yes)throw new Exception("fault-focused self-test failed");}
        var members=CsvRead(Path.Combine(Out,"graphics-members-v1.csv"));var paths=members.Select(r=>r["path"]).ToHashSet();
        Test("reject duplicate member identity",()=>Expect(CheckRows(members.Append(members[0]).ToList(),paths).Contains("duplicate member ID")));
        Test("reject missing file population",()=>Expect(CheckRows(members.Where(r=>r["path"]!=members[0]["path"]).ToList(),paths).Contains("file coverage mismatch")));
        Test("reject invented ownership",()=>{var rows=members.Select(r=>new Dictionary<string,string>(r)).ToList();rows[0]["ownership"]="GOLDEN";Expect(CheckRows(rows,paths).Contains("invalid ownership"));});
        Test("reject native/scientific promotion",()=>{var rows=members.Select(r=>new Dictionary<string,string>(r)).ToList();rows[0]["native_status"]="PASS";rows[0]["scientific_status"]="GOLDEN";Expect(CheckRows(rows,paths).Contains("unauthorized status"));});
        Test("reject inverted source span",()=>{var rows=members.Select(r=>new Dictionary<string,string>(r)).ToList();rows[0]["end_line"]="0";Expect(CheckRows(rows,paths).Contains("inverted span"));});
        Test("reject mixed ownership without native boundary",()=>{var rows=members.Select(r=>new Dictionary<string,string>(r)).ToList();rows[0]["ownership"]="SHARED_MIXED_SPLIT";rows[0]["suboperation_classifications"]="math only";Expect(CheckRows(rows,paths).Contains("missing mixed native boundary"));});
        Test("publication path cannot escape audit allowlist",()=>Expect(!SafePublicationPath("../private")&&!SafePublicationPath("evidence/04A29/../../AGENTS.md")&&!SafePublicationPath("BESHStat.Core/src/changed.vb")&&SafePublicationPath("evidence/04A29/a.csv")));
        Test("private path aliases resolve deterministically from frozen Git tree",()=>{using var entry=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,"protected-files-entry.json")));var alias=entry.RootElement.EnumerateArray().Select(e=>e.GetProperty("path").GetString()!).First(p=>p.StartsWith("<PRIVATE_PROTECTED_PATH:"));var actual=ResolveProtectedPath(alias);Expect(File.Exists(actual)&&ProtectedAlias(actual)==alias);});
        Test("privacy rejects native machine identity without printing it",()=>Expect(PrivateText("private-"+Environment.MachineName)&&!PrivateText("relative/neutral/a.vb")));
        Test("Roslyn distinguishes overloaded constructors, accessors, multiline signatures and strings",()=>{
            var source="Class A\n Public Sub New()\n End Sub\n Public Sub New(x As Integer)\n End Sub\n Public Property P As Integer\n Get\n Return 1\n End Get\n Set(value As Integer)\n End Set\n End Property\n Public Function F(\n x As Integer) As String\n Return \"Sub Fake()\"\n End Function\nEnd Class";
            var tree=VisualBasicSyntaxTree.ParseText(source);Expect(!tree.GetDiagnostics().Any(d=>d.Severity==Microsoft.CodeAnalysis.DiagnosticSeverity.Error));var root=tree.GetRoot();Expect(root.DescendantNodes().OfType<ConstructorBlockSyntax>().Count()==2&&root.DescendantNodes().OfType<AccessorBlockSyntax>().Count()==2&&root.DescendantNodes().OfType<MethodBlockSyntax>().Count()==1);
        });
        Test("byte hash distinguishes line-ending and payload tampering",()=>Expect(ShaText("x\r\n")!=ShaText("x\n")&&ShaText("x")!=ShaText("y")));
        Test("historical body matching accepts verified newline transform but rejects changed operation",()=>Expect(BodyCorresponds("Return 1\n",ShaText("Return 1\r\n"))&&!BodyCorresponds("Return 2\n",ShaText("Return 1\r\n"))));
        Test("field declarator fallback and attributed declaration-token line",()=>{var root=VisualBasicSyntaxTree.ParseText("Class A\n Private count As Integer\n <Obsolete>\n Public Sub F()\n End Sub\nEnd Class").GetRoot();var field=root.DescendantNodes().OfType<VariableDeclaratorSyntax>().Single();var method=root.DescendantNodes().OfType<MethodBlockSyntax>().Single();Expect(DeclarationLine(field)==2&&Start(method)==3&&DeclarationLine(method)==4);});
        Test("CSV quoted multiline input and quoted signature roundtrip",()=>{
            var temp=Path.Combine(Root,".04a29-audit.tmp","self-test.csv");File.WriteAllText(temp,"\"id\",\"body\"\n\"one\",\"line1\n\"\"quoted\"\"\"\n");var rs=CsvRead(temp);Expect(rs.Count==1&&rs[0]["body"]=="line1\n\"quoted\"");
        });
        Write("audit-helper-tests.json",new {command="dotnet <AUDIT_HELPER>/GraphicsAudit.dll self-test",passed=results.Count,failed=0,tests=results,scope="Audit infrastructure only; no existing test changed; no product assembly executed"});Console.WriteLine("PASS: "+results.Count+" fault-focused audit-helper tests");
    }
    static bool SafePublicationPath(string path)
    {
        var full=Path.GetFullPath(path,Root); var rel=Rel(full);
        return path==rel&&(rel.StartsWith("evidence/04A29/",StringComparison.Ordinal)||rel=="docs/architecture/batch-04A-29-owner-review.md");
    }
    static string[] CandidateFiles()=>Directory.GetFiles(Out,"*",System.IO.SearchOption.AllDirectories).Concat(new[]{Path.Combine(Root,"docs/architecture/batch-04A-29-owner-review.md")}).Where(File.Exists).Order().ToArray();
    static void Close()
    {
        var sw=Stopwatch.StartNew(); var errors=CheckData();if(errors.Count>0)throw new Exception(string.Join('\n',errors));
        var protectedChanged=CheckPins(Path.Combine(Out,"protected-files-entry.json"));
        var historicalChanged=CheckPins(Path.Combine(Out,"historical-files-entry.json"));
        var captures=VerifyCaptureRegistry();
        var resultNow=ResultsSnapshot();using var entry=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,"historical-results-entry.json")));
        var resultsSame=JsonSerializer.Serialize(resultNow)==JsonSerializer.Serialize(entry.RootElement);
        // Compare canonical property values, independent of pretty-print/serializer whitespace.
        using var resultDoc=JsonDocument.Parse(JsonSerializer.Serialize(resultNow));resultsSame=entry.RootElement.GetProperty("membership_size_mtime_sha256").GetString()==resultDoc.RootElement.GetProperty("membership_size_mtime_sha256").GetString()&&entry.RootElement.GetProperty("count").GetInt32()==resultDoc.RootElement.GetProperty("count").GetInt32();
        Write("source-preservation.json",new { protected_entry_manifest=Pin(Path.Combine(Out,"protected-files-entry.json")), historical_entry_manifest=Pin(Path.Combine(Out,"historical-files-entry.json")), protected_changed=protectedChanged,historical_changed=historicalChanged,product_and_authority_bytes_unchanged=protectedChanged.Length==0&&historicalChanged.Length==0,scope="All3699 tracked entry files plus all local ledger/04A02/evidence authority; exact byte SHA and size, not Git newline normalization. UDF/Designer/Ribbon/projects included.",historical_TestResults_entry=entry.RootElement.Clone(),historical_TestResults_final=resultDoc.RootElement.Clone(), historical_TestResults_unchanged=resultsSame,historical_payload_qualification="No280953-result payload rehash. Entry/final membership,size,write-time digest checked; original prior hash/stat receipt pinned. NOT a fresh byte seal of138GB.", original_100_mixed_captures=captures,original_hash_receipt=Pin("docs/architecture/WindowsMigrationLedger/04A01-closeout-preservation.json"),final_seconds=sw.Elapsed.TotalSeconds,no_historical_writes=true });
        if(protectedChanged.Length>0||historicalChanged.Length>0||!resultsSame)throw new Exception("Preservation mismatch; see source-preservation.json");
        var inherited=CheckFrozenReference();
        Write("nonpasses.json",new { current_audit_nonpasses=Array.Empty<string>(),development_failures=new[]{"Initial audit-helper build: CS0104 SearchOption ambiguity,3 errors; fixed by System.IO alias; no product source change.","Reporting-helper extension build:3missing VB syntax-type import errors; fixed by explicit syntax namespace; subsequent build exit0.","Reporting run exit1 Sequence contains no elements on field declarator without immediate child token; fixed declaration-token fallback and added focused regression test. Partial audit outputs regenerated, historical/product files untouched.","Publication verification exit1: protected entry path manifest contained machine-name text in70relative tracked paths. Raw manifest retained locally; explicitly sealed public alias derivative qualified. No historical/product source changed, no failure suppressed.","Git ownership and index.lock sandbox failures; per-command trust and separately approved branch creation resolved.","python.exe app execution alias inaccessible; py absent. No install; existing SDK/Roslyn used.","Read-only discovery included nonexistent toolchain/final-source-qualification and guessed Core subfolders; corrected to actual paths, no missing product evidence inferred."},inherited_reference_hash_mismatches=inherited, historical_suite_reference="04A28 full Debug managed14raw failures;Python96 FAIL/10ERROR;strict/profile1/1UNMET. These are historical references, not04A29 executions.",native="NOT_RUN",scientific="NOT_GOLDEN" });
        Write("validation.json",new { audit_schema="PASS",current_files=38,classified_members=CsvRead(Path.Combine(Out,"graphics-members-v1.csv")).Count,helper_tests=Pin(Path.Combine(Out,"audit-helper-tests.json")),product_source_integrity="PASS",compiled_membership="PASS",historical_evidence_integrity="PASS_ENTRY_TO_FINAL",frozen_reference_qualification=inherited.Length==0?"PASS":"INHERITED_MISMATCHES_RETAINED_SEPARATELY",product_build=new {status="NOT_RUN",reason="Source-only audit; compiled product inputs unchanged. Only isolated offline audit helper compiled."},suites=new[]{"Node","full Debug managed","full Release managed","full Python","strict","profile","strict MkDocs","focused Core Graphics","verify-beshstatng.ps1","verify-beshstatng.ps1 -Legacy"}.Select(s=>new{name=s,status="NOT_RUN",reason=s.StartsWith("verify-")?"Official audit-only override; wrapper writes historical/shared TestResults. Source/schema/hash checks used; no product source changes.":"No product-source change or executable fixture claim; required source-only audit validation executed instead."}).ToArray(),historical_reference=new {status="REUSED_HISTORICAL_REFERENCE",source=Pin("evidence/04A28/debug-full-validation.json"),dossier=Pin("docs/architecture/batch-04A-28-owner-review.md"),counts="Debug12090/12103logical,19854/19868raw;Node916/916;Python850/956(96FAIL10ERROR);Release targeted1253/1253;fullReleaseNOT_RUN under batch-specific trial;strict/profile1/1;MkDocs0",meaning="Background only; exact current test fixture/source pins in protected manifest. Not an04A29 pass or permanent Debug-first waiver."},manual_checks=new[]{new{host="Excel",status="NOT_RUN",proposed="After separate native authorization, compare histogram bins/box/overlay and staged Sankey labels/resize; verify loaded build identity before capture."},new{host="Excel/WASM",status="NOT_RUN",proposed="After separate authorization, compare PCA retained labels/biplot and KM/censor/CI series. WASM is smoke only; audited renderers are not available there by this audit."}},duration_seconds=sw.Elapsed.TotalSeconds,performance="Measured freeze81.70s and final scoped preservation duration reported; no matched sequential control or speedup claim." });
        Dossier(); Privacy(); Commands(); Manifest(); Verify();
        Console.WriteLine("Closure PASS; preservation seconds="+sw.Elapsed.TotalSeconds.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
    }
    static string[] CheckPins(string manifest)
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(manifest));return doc.RootElement.EnumerateArray().Where(p=>{var actual=ResolveProtectedPath(p.GetProperty("path").GetString()!);return !File.Exists(actual)||new FileInfo(actual).Length!=p.GetProperty("bytes").GetInt64()||FileSha(actual)!=p.GetProperty("sha256").GetString();}).Select(p=>p.GetProperty("path").GetString()!).ToArray();
    }
    static Dictionary<string,string>? ProtectedAliases;
    static string ProtectedAlias(string path)=>"<PRIVATE_PROTECTED_PATH:"+ShaText(path)[..24]+">";
    static string ResolveProtectedPath(string path)
    {
        if(!path.StartsWith("<PRIVATE_PROTECTED_PATH:"))return path;
        ProtectedAliases??=Git("ls-tree","-r","--name-only","02a6d01cd3520116539fcefdd77612bbd9550129").Split('\n').ToDictionary(ProtectedAlias,p=>p);
        return ProtectedAliases.GetValueOrDefault(path)??throw new Exception("Protected alias absent from frozen Git tree");
    }
    static void QualifyEntryPrivacy()
    {
        var path=Path.Combine(Out,"protected-files-entry.json");var raw=File.ReadAllBytes(path);
        var privatePath=Path.Combine(Root,".04a29-audit.tmp","protected-files-entry-raw.json");
        if(File.Exists(privatePath))throw new Exception("Private original receipt already exists; refuse overwrite");
        using var doc=JsonDocument.Parse(raw);var redacted=0;
        var rows=doc.RootElement.EnumerateArray().Select(p=>{var name=p.GetProperty("path").GetString()!;if(PrivateText(name)){name=ProtectedAlias(name);redacted++;}return new{path=name,bytes=p.GetProperty("bytes").GetInt64(),sha256=p.GetProperty("sha256").GetString()!};}).ToArray();
        if(redacted==0)throw new Exception("No entry path privacy qualification required");
        File.WriteAllBytes(privatePath,raw); // New local raw receipt only; no historical/product file moved or deleted.
        Write("protected-files-entry.json",rows);
        Write("entry-privacy-qualification.json",new {status="PASS",reason="Initial publication check caught machine-name text in a tracked relative path. The raw entry manifest is retained locally byte-for-byte; only a public path-alias derivative replaces its candidate presentation.",redacted_paths=redacted,raw_local_receipt=new{path="<AUDIT_PRIVATE>/protected-files-entry-raw.json",bytes=raw.Length,sha256=Sha(raw)},public_derivative=Pin(path),alias_rule="<PRIVATE_PROTECTED_PATH:first24SHA256(original Git-relative path)>; resolve against ls-tree of frozen02a6d01base, never a guessed path. File bytes/SHA stay exact.",product_or_historical_changes=0});
        Console.WriteLine("PASS: entry privacy derivative; "+redacted+" path aliases, raw local receipt retained");
    }
    static object VerifyCaptureRegistry()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText("evidence/04A14/baseline-registry.json"));var entries=doc.RootElement.GetProperty("files").EnumerateObject().ToArray();
        var changed=entries.Where(p=>!File.Exists(p.Name)||FileSha(p.Name)!=p.Value.GetProperty("sha256").GetString()||new FileInfo(p.Name).Length!=p.Value.GetProperty("bytes").GetInt64()).Select(p=>p.Name).ToArray();
        if(entries.Length!=100||changed.Length>0)throw new Exception("Original100capture registry failure");return new {registry=Pin("evidence/04A14/baseline-registry.json"),count=entries.Length,changed,status="NOT_GOLDEN; exact byte/hash check PASS"};
    }
    static string[] CheckFrozenReference()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText("evidence/04A28/frozen-capture-hashes.json"));
        var pins=new Dictionary<string,(long bytes,string sha)>();
        foreach(var name in new[]{"protected-files-entry.json","historical-files-entry.json"}){using var cache=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,name)));foreach(var p in cache.RootElement.EnumerateArray())pins[ResolveProtectedPath(p.GetProperty("path").GetString()!)]=(p.GetProperty("bytes").GetInt64(),p.GetProperty("sha256").GetString()!);}
        var changed=new List<string>();foreach(var p in doc.RootElement.GetProperty("files").EnumerateObject())
        {
            // Entry cache is qualified by the just-completed final preservation check; no third payload rehash.
            if(pins.TryGetValue(p.Name,out var pin)){if(pin.sha!=p.Value.GetProperty("sha256").GetString()||pin.bytes!=p.Value.GetProperty("bytes").GetInt64())changed.Add(p.Name);}
            else if(!File.Exists(p.Name)||FileSha(p.Name)!=p.Value.GetProperty("sha256").GetString()||new FileInfo(p.Name).Length!=p.Value.GetProperty("bytes").GetInt64())changed.Add(p.Name);
        }
        return changed.ToArray();
    }
    static bool PrivateText(string text)=>Regex.IsMatch(text,@"(?i)[A-Z]:[/\\]Users[/\\]|S-1-5-21-\d")||text.Contains(Environment.MachineName,StringComparison.OrdinalIgnoreCase);
    static void Privacy()
    {
        var findings=new List<string>();
        foreach(var p in CandidateFiles().Where(p=>!p.EndsWith("privacy.json")&&!p.EndsWith("publication-manifest.json")))
        {
            var t=File.ReadAllText(p);if(PrivateText(t))findings.Add(Rel(p));
        }
        Write("privacy.json",new {status=findings.Count==0?"PASS":"FAIL",findings,scope="All candidate text including helper sources/dossier; no workbook contents, secrets, private home paths, SIDs, hostnames or private tool paths. Source relative identity and public synthetic fixture names retained.",excluded_owner_artifact="Untracked session transcript created after entry is untouched and excluded; no contents read/copied.",repo_placeholder="<REPO_ROOT>"});if(findings.Count>0)throw new Exception("Privacy scan findings: "+string.Join(';',findings));
    }
    static void Manifest()
    {
        var paths=CandidateFiles().Select(Rel).Append("evidence/04A29/publication-manifest.json").Distinct().Order().ToArray();
        foreach(var path in paths)if(!SafePublicationPath(path))throw new Exception("Unsafe publication path");
        var info=new ProcessStartInfo("git"){WorkingDirectory=Root,RedirectStandardOutput=true,RedirectStandardError=true};foreach(var arg in new[]{"-c","safe.directory="+Root.Replace('\\','/'),"check-ignore","--no-index"}.Concat(paths))info.ArgumentList.Add(arg);
        using(var p=Process.Start(info)!){var ignored=p.StandardOutput.ReadToEnd();var err=p.StandardError.ReadToEnd();p.WaitForExit();if(p.ExitCode!=1)throw new Exception("Publication ignored/error: "+ignored+err.Replace(Root,"<REPO_ROOT>"));}
        Write("publication-manifest.json",new {schema="04A29-publication-v1",self_rule="Self included with bytes/sha256 null; no recursive/unstable self-hash",files=paths.Select(p=>p=="evidence/04A29/publication-manifest.json"?new{path=p,bytes=(long?)null,sha256=(string?)null}:new{path=p,bytes=(long?)new FileInfo(p).Length,sha256=(string?)FileSha(p)}).ToArray(),trackability="PASS: git check-ignore --no-index over complete candidate allowlist exited1 with no ignored candidates",branch=Git("branch","--show-current"),head=Git("rev-parse","HEAD"),index_empty=Git("diff","--cached","--name-only")=="",excluded_owner_files="Untracked session transcript outside authorized publication roots",publication="CANDIDATES_ONLY; no staging,commit,push,merge or04A30"});
    }
}
