using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

// Syntax inventory, deliberately not a VB semantic compiler or runtime registration test.
// No package references: Roslyn ships with the installed SDK. Review decisions are separate.
static class Audit
{
    const string Version = "04A.01-syntax-1";
    static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    static string Root = "", Output = "";
    static string? ReviewOverride;
    static string? WiringReviewOverride;
    static int ReviewedControls;
    static int QualifiedControlContracts;
    static readonly string[] SemanticFields = {"responsibility", "source_purpose", "separable_responsibilities", "equivalence_scope", "equivalent_evidence", "reusable_operation", "destination", "future_batch", "risk", "exception_tags", "proposed_test", "rationale", "owner_role", "runtime_boundary"};
    static bool SemanticReviewed(Dictionary<string,string> row) =>
        row.GetValueOrDefault("review_contract") == "04A.01-member-v1"
        && row.GetValueOrDefault("review_status") == "manually-reviewed-static"
        && SemanticFields.All(k => !string.IsNullOrWhiteSpace(row.GetValueOrDefault(k)))
        && row.GetValueOrDefault("equivalence_scope") is "none" or "helper-only" or "whole-method";
    static readonly List<Dictionary<string,string>> Symbols = [], Types = [], State = [], Edges = [], Refs = [], Exports = [], Files = [], Diagnostics = [];
    static readonly List<(string Path, SyntaxNode Node, string RootNamespace)> Trees = [];
    static readonly Dictionary<SyntaxNode, Dictionary<string,string>> MemberRows = [];
    static readonly Dictionary<SyntaxNode, Dictionary<string,string>> Aliases = [];
    static string Norm(string s) => Regex.Replace(s, @"\s+", " ").Trim();
    static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
    static string Sha(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
    static string Rel(string p) => Path.GetRelativePath(Root,p).Replace('\\','/');
    static string Git(params string[] args)
    {
        var info = new ProcessStartInfo("git") { RedirectStandardOutput=true, RedirectStandardError=true, WorkingDirectory=Root };
        info.ArgumentList.Add("-c"); info.ArgumentList.Add("safe.directory=" + Root.Replace('\\','/'));
        foreach(var a in args) info.ArgumentList.Add(a);
        using var p=Process.Start(info)!; var result=p.StandardOutput.ReadToEnd(); var err=p.StandardError.ReadToEnd(); p.WaitForExit();
        if(p.ExitCode!=0) throw new Exception(err); return result.TrimEnd();
    }
    static Dictionary<string,string> Row(params string[] pairs)
    {
        var d = new Dictionary<string,string>(); for(int i=0;i<pairs.Length;i+=2)d.Add(pairs[i],pairs[i+1]); return d;
    }
    static int Start(SyntaxNode n) => n.GetLocation().GetLineSpan().StartLinePosition.Line+1;
    static int End(SyntaxNode n) => n.GetLocation().GetLineSpan().EndLinePosition.Line+1;
    static string TypeName(SyntaxNode n, string root)
    {
        var parts=new List<string>{root};
        foreach(var a in n.Ancestors().Reverse())
        {
            if(a is NamespaceBlockSyntax ns) parts.Add(ns.NamespaceStatement.Name.ToString());
            if(a is TypeBlockSyntax tb) parts.Add(tb.BlockStatement.Identifier.ValueText);
            if(a is EnumBlockSyntax eb) parts.Add(eb.EnumStatement.Identifier.ValueText);
        }
        var result=string.Join(".",parts.Where(x=>x!=""));
        return result.Replace(root+".Global.", "");
    }
    static SyntaxNode Header(SyntaxNode n) => n switch {
        MethodBlockBaseSyntax b => b.BlockStatement,
        PropertyBlockSyntax b => b.PropertyStatement,
        EventBlockSyntax b => b.EventStatement,
        LambdaExpressionSyntax b => b.SubOrFunctionHeader,
        _ => n
    };
    static string Name(SyntaxNode n) => Header(n) switch {
        MethodStatementSyntax s => s.Identifier.ValueText,
        SubNewStatementSyntax => ".ctor",
        OperatorStatementSyntax s => "Operator " + s.OperatorToken.Text,
        AccessorStatementSyntax s => s.AccessorKeyword.Text,
        PropertyStatementSyntax s => s.Identifier.ValueText,
        EventStatementSyntax s => s.Identifier.ValueText,
        DeclareStatementSyntax s => s.Identifier.ValueText,
        _ => n is LambdaExpressionSyntax ? "lambda" : "initializer"
    };
    static string Family(string path) => path.Contains("/src/") ? path.Split("/src/")[1].Split('/')[0] : "project-support";
    static bool IsMember(SyntaxNode n) => n is MethodBlockBaseSyntax or PropertyBlockSyntax or EventBlockSyntax or LambdaExpressionSyntax or DeclareStatementSyntax
        || n is MethodStatementSyntax && n.Parent is not MethodBlockBaseSyntax
        || n is EventStatementSyntax && n.Parent is not EventBlockSyntax;
    static bool IsInitializer(SyntaxNode n) => n is VariableDeclaratorSyntax v && (v.Initializer!=null || v.AsClause is AsNewClauseSyntax)
        && (v.Parent is FieldDeclarationSyntax || v.Parent is LocalDeclarationStatementSyntax l && l.Modifiers.Any(SyntaxKind.StaticKeyword))
        || n is PropertyStatementSyntax p && (p.Initializer!=null || p.AsClause is AsNewClauseSyntax);
    static Dictionary<string,string>? Owner(SyntaxNode n) => n.AncestorsAndSelf().Select(a=>MemberRows.GetValueOrDefault(a)??Aliases.GetValueOrDefault(a)).FirstOrDefault(x=>x!=null);
    static string OwnerId(SyntaxNode n) => Owner(n)?.GetValueOrDefault("symbol_id") ?? "type-scope";
    static void Parse(string path, string rootNamespace, bool windows)
    {
        var source=File.ReadAllText(Path.Combine(Root,path));
        // Parse both Win32/Win64 preprocessor profiles. Active-node union is location keyed.
        var roots=new List<SyntaxNode>();
        foreach(bool win64 in new[]{false,true}) {
            var options=VisualBasicParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest)
                .WithPreprocessorSymbols(new Dictionary<string,object>{{"DEBUG",true},{"TRACE",true},{"Win64",win64},{"VBA7",true},{"CONFIG", "Debug"},{"_MYTYPE",win64?"WindowsForms":"Windows"},{"BESH_HELP_RESOLVER_PUBLISHED",true}});
            var tree=VisualBasicSyntaxTree.ParseText(source, options, path: path);
            roots.Add(tree.GetRoot());
            if(windows) foreach(var d in tree.GetDiagnostics().Where(x=>x.Severity==DiagnosticSeverity.Error))
                Diagnostics.Add(Row("path",path,"profile",win64?"Win64":"Win32","line",(d.Location.GetLineSpan().StartLinePosition.Line+1).ToString(),"diagnostic",d.ToString()));
        }
        var seen=new Dictionary<string,Dictionary<string,string>>();
        foreach(var root in roots) {
            Trees.Add((path,root,rootNamespace));
            foreach(var n in root.DescendantNodes().Where(n=>IsMember(n)||IsInitializer(n))) {
                var identity=n.Kind()+":"+n.Span.Start+":"+n.Span.Length;
                if(seen.TryGetValue(identity,out var existing)) {Aliases[n]=existing;continue;}
                var sig=Norm(Header(n).ToString()); var type=TypeName(n,rootNamespace);
                if(n is AccessorBlockSyntax) type += "." + Name(n.Parent!);
                var key=path+"|"+type+"|"+n.Kind()+"|"+Hash(sig)[..16];
                var id=key+"@"+n.Span.Start+":"+n.Span.Length;
                var row=Row("symbol_id",id,"signature_key",key,"path",path,"family",Family(path),"containing_type",type,"name",Name(n),"kind",n.Kind().ToString(),
                    "signature",sig,"start_line",Start(n).ToString(),"end_line",End(n).ToString(),"span_start",n.Span.Start.ToString(),"span_length",n.Span.Length.ToString(),
                    "visibility",string.Join(" ",Header(n).ChildTokens().Where(t=>t.IsKind(SyntaxKind.PublicKeyword)||t.IsKind(SyntaxKind.PrivateKeyword)||t.IsKind(SyntaxKind.FriendKeyword)||t.IsKind(SyntaxKind.ProtectedKeyword)).Select(t=>t.Text)),
                    "body_sha256",Hash(n.ToString()),"evidence_status","static","profile",ReferenceEquals(root,roots[0])?"project MyType=Windows":"alternative generated MyType=WindowsForms branch","source_sha256",Sha(Path.Combine(Root,path)));
                if(row["visibility"]=="") row["visibility"]="implicit VB visibility; see declaration/context";
                MemberRows[n]=row;
                seen[identity]=row;
                if(windows) Symbols.Add(row);
            }
        }
        if(!windows) return;
        var unique=new HashSet<string>();
        foreach(var root in roots) foreach(var n in root.DescendantNodes()) {
            if(!unique.Add(n.Kind()+":"+n.Span.Start+":"+n.Span.Length)) continue;
            if(n is TypeBlockSyntax or EnumBlockSyntax or DelegateStatementSyntax) Types.Add(Row("path",path,"containing_type",TypeName(n,rootNamespace),"qualified_type",TypeName(n,rootNamespace)+"."+(n is TypeBlockSyntax nt?nt.BlockStatement.Identifier.ValueText:n is EnumBlockSyntax ne?ne.EnumStatement.Identifier.ValueText:((DelegateStatementSyntax)n).Identifier.ValueText),"kind",n.Kind().ToString(),"declaration",Norm(Header(n is TypeBlockSyntax t?t.BlockStatement:n is EnumBlockSyntax e?e.EnumStatement:n).ToString()),"start_line",Start(n).ToString(),"end_line",End(n).ToString()));
            if(n is FieldDeclarationSyntax f) foreach(var v in f.Declarators) foreach(var name in v.Names) State.Add(Row("path",path,"containing_type",TypeName(n,rootNamespace),"name",name.Identifier.ValueText,"kind","field","declaration",Norm(f.ToString()),"declared_type",Norm(v.AsClause?.ToString()??"inferred"),"start_line",Start(n).ToString(),"end_line",End(n).ToString(),"with_events",f.Modifiers.Any(SyntaxKind.WithEventsKeyword).ToString(),"initializer",Norm(v.Initializer?.ToString()??(v.AsClause is AsNewClauseSyntax? v.AsClause.ToString():""))));
            if(n is PropertyStatementSyntax ps && n.Parent is not PropertyBlockSyntax) State.Add(Row("path",path,"containing_type",TypeName(n,rootNamespace),"name",ps.Identifier.ValueText,"kind","auto-or-abstract-property","declaration",Norm(ps.ToString()),"start_line",Start(n).ToString(),"end_line",End(n).ToString(),"with_events","False","initializer",Norm(ps.Initializer?.ToString()??"")));
            if(n is EnumMemberDeclarationSyntax em) State.Add(Row("path",path,"containing_type",TypeName(n,rootNamespace),"name",em.Identifier.ValueText,"kind","enum-member","declaration",Norm(em.ToString()),"start_line",Start(n).ToString(),"end_line",End(n).ToString(),"with_events","False","initializer",Norm(em.Initializer?.ToString()??"implicit ordinal")));
            if(n is InvocationExpressionSyntax inv) Refs.Add(Row("path",path,"line",Start(n).ToString(),"owner_id",OwnerId(n),"kind","invocation","reference",Norm(inv.Expression.ToString()),"arguments",Norm(inv.ArgumentList?.ToString()??""),"target_status","syntax-only; overload/late-binding unresolved"));
            if(n is ObjectCreationExpressionSyntax obj) Refs.Add(Row("path",path,"line",Start(n).ToString(),"owner_id",OwnerId(n),"kind","creation","reference",Norm(obj.Type.ToString()),"arguments",Norm(obj.ArgumentList?.ToString()??""),"target_status","syntax-only; constructor binding unresolved"));
            if(n is MethodStatementSyntax ms && ms.HandlesClause!=null) foreach(var ev in ms.HandlesClause.Events) Edge(path,rootNamespace,n,"Handles",ev.ToString(),ms.Identifier.ValueText);
            if(n is AddRemoveHandlerStatementSyntax ah) Edge(path,rootNamespace,n,ah.AddHandlerOrRemoveHandlerKeyword.Text,ah.EventExpression.ToString(),ah.DelegateExpression.ToString());
            if(n is EventStatementSyntax es) Edge(path,rootNamespace,n,"event-declaration",es.Identifier.ValueText,"declared event contract");
            if(n is RaiseEventStatementSyntax re) Edge(path,rootNamespace,n,"RaiseEvent",re.Name.ToString(),re.Name.ToString());
            if(n.IsKind(SyntaxKind.AddressOfExpression)) Edge(path,rootNamespace,n,"AddressOf","delegate reference",((UnaryExpressionSyntax)n).Operand.ToString());
            if(n is LambdaExpressionSyntax) Edge(path,rootNamespace,n,"lambda","inline callback",OwnerId(n));
            if(n is AssignmentStatementSyntax ass && path.EndsWith(".Designer.vb",StringComparison.OrdinalIgnoreCase)) Edge(path,rootNamespace,n,"Designer-configuration",ass.Left.ToString(),Norm(ass.Right.ToString()));
            if(n is InvocationExpressionSyntax call && Regex.IsMatch(call.Expression.ToString(), @"(?i)(GetMethod|InvokeMember|CallByName|ExcelRegistration|RegisterFunction|RegisterDelegates|WireHelp|BeginInvoke|\.Invoke$)")) Edge(path,rootNamespace,n,"indirect-dispatch",call.Expression.ToString(),Norm(call.ArgumentList?.ToString()??""));
            if(n is AttributeSyntax a && Regex.IsMatch(a.Name.ToString(), @"(?i)(^|\.)ExcelFunction(Attribute)?$")) {
                var stmt=a.Ancestors().OfType<MethodStatementSyntax>().FirstOrDefault();
                var member=stmt?.Parent is MethodBlockBaseSyntax mb? (SyntaxNode)mb : stmt;
                var row=member!=null?MemberRows.GetValueOrDefault(member):null;
                var nameArg=a.ArgumentList?.Arguments.OfType<SimpleArgumentSyntax>().FirstOrDefault(x=>x.NameColonEquals?.Name.Identifier.ValueText.Equals("Name",StringComparison.OrdinalIgnoreCase)==true);
                string effective=nameArg?.Expression is LiteralExpressionSyntax lit?lit.Token.ValueText:nameArg==null?stmt?.Identifier.ValueText??"UNRESOLVED":"UNRESOLVED: "+nameArg.Expression.ToString();
                Exports.Add(Row("export_id",path+"@"+a.Span.Start,"path",path,"attribute_line",Start(a).ToString(),"attribute_end_line",End(a).ToString(),"attribute",Norm(a.ToString()),"effective_name",effective,"symbol_id",row?.GetValueOrDefault("symbol_id")??"UNRESOLVED","implementing_name",stmt?.Identifier.ValueText??"UNRESOLVED","signature",row?.GetValueOrDefault("signature")??"UNRESOLVED","start_line",row?.GetValueOrDefault("start_line")??Start(a).ToString(),"end_line",row?.GetValueOrDefault("end_line")??End(a).ToString(),"registration_status","static attribute only; Excel runtime unproven"));
            }
        }
        // Disabled text under these two profiles is explicit residual syntax, never silently counted as parsed.
        foreach(var tr in roots[0].DescendantTrivia(descendIntoTrivia:true).Where(x=>x.IsKind(SyntaxKind.DisabledTextTrivia)))
            if(!roots.Skip(1).Any(r=>r.DescendantTokens().Any(t=>tr.Span.Contains(t.Span))))
                Diagnostics.Add(Row("path",path,"profile","conditional-residual","line",(tr.GetLocation().GetLineSpan().StartLinePosition.Line+1).ToString(),"diagnostic","Disabled source requires other preprocessor profile: "+Norm(tr.ToString())));
    }
    static void Edge(string path,string rn,SyntaxNode n,string kind,string ev,string target)
    {
        Edges.Add(Row("edge_id",path+"|"+kind+"@"+n.Span.Start+"|"+ev,"path",path,"containing_type",TypeName(n,rn),"owner_id",OwnerId(n),"kind",kind,"event_or_control",Norm(ev),"target",Norm(target),"start_line",Start(n).ToString(),"end_line",End(n).ToString(),"span_start",n.Span.Start.ToString(),"target_status","pending","target_ids","","evidence_status","static"));
    }
    static (string Batch,string Role,string Test) Followup(Dictionary<string,string> r)
    {
        string p=r["path"],name=r["name"], fam=r["family"];
        if(p.Contains("/UI/")) {
            if(p.EndsWith(".Designer.vb")) return("04A.45", "Windows UI maintainer", "04A.02 option/default capture; 04A.45 fake-host event and reset/focus tests");
            if(p.Contains("Refedit/")) return("04A.27", "Core/Common input and Windows RefEdit maintainers", "Workbook/sheet switch, frozen-range invalidation, event detach and Dispose fixture; E3");
            if(p.Contains("RegressionEffectsController")) return("04A.08", "Common model-spec / Windows UI maintainers", "Effect order, duplicate/hierarchy, categorical contrast and canonical formula parity without ListBox");
            if(p.Contains("UibyID")) return("04A.03", "Common grouped-analysis / Windows UI maintainers", "04A.02 per-tag grouping/normality/ANOVA/KW/plot option, table/p-value and source-row goldens; 04A.06/07/30 followups");
            if(p.Contains("Ui18MMRM")||p.Contains("Ui19LMM")) return("04A.14", "Common mixed-model / Windows UI maintainers", "Covariance ML/REML, KR/Satt, LS-means/reference-grid contrasts, diagnostics and invalid-choice goldens; 04A.15/16");
            if(p.Contains("Ui13GEE")) return("04A.12", "Common GEE / Windows UI maintainers", "Cluster/time order, correlation, offset/weights, robust SE, LS-estimate and report fixtures");
            if(p.Contains("UiGLM")) return("04A.09", "Common regression / Windows UI maintainers", "Design/family/link/offset/weight and coefficient/residual report fixtures; 04A.10/11");
            if(p.Contains("Ui4Cox")||p.Contains("Ui4KM")) return("04A.13", "Common survival / Windows UI maintainers", "Event/censor/strata, ties, residuals, time origin and survival plot fixtures");
            if(p.Contains("Ui22")) return("04A.23", "Common multivariate SPC / Windows UI maintainers", "T2/PCA-Q/MEWMA/MCUSUM covariance, exclusions, phase masks, limits and warning fixtures; 04A.24");
            if(p.Contains("Ui21")) return("04A.21", "Common SPC / Windows UI maintainers", "Chart choice, phases, historical limits, exclusions, run rules and signal/geometry fixtures; 04A.22");
            if(p.Contains("Ui20")) return("04A.25", "Common PSM / Windows UI maintainers", "Seeded match IDs, caliper/trim/weight/strata, balance/Love plot and row-provenance fixtures");
            if(p.Contains("Ui11PCA")) return("04A.17", "Common multivariate / Windows UI maintainers", "Scaling, priors, missing rows, seeded clustering, score labels and plot/table fixtures; 04A.18-20");
            if(p.Contains("Ui82x2")||p.Contains("Ui8Proportions")) return("04A.05", "Common contingency / Windows UI maintainers", "Count orientation, zero cells, exact/mid-p/alternative CI, worksheet/scalar and output-target goldens");
            if(p.Contains("Ui0OneRefedit")||p.Contains("UiTwoInput")||p.Contains("Ui9ANOVA")) return("04A.04", "Common paired/blocked / Windows UI maintainers", "Physical row alignment, missing pairs/blocks, alternative/margins, exact CI and report ordering; 04A.05/26");
            if(p.Contains("Ui12Sample")) return("04A.26", "Common sample-size / Windows UI maintainers", "Scalar rounding, effect/CI and paired planning goldens");
            return("04A.45", "Windows UI / Common application maintainers", "04A.02 option/default capture; open/toggle/reset/output/invalid/cancel/help/focus fake-host fixture");
        }
        if(fam=="Graphics") return(p.EndsWith("NormalPlot.vb")||p.EndsWith("ROC.vb")?"04A.31":p.Contains("Histogram")||p.Contains("BoxPlot")||p.Contains("Violin")||p.Contains("ChartingFunc")?"04A.32":p.Contains("XYZ")||p.Contains("3D")||p.Contains("ScatterPlotMatrix")?"04A.33":"04A.34", "Core graphics / Common chart-spec / Windows renderer maintainers", "04A.02 numeric/geometry/label/CI/axis/series golden; immutable inputs, ties/missing/constant data; COM placement independently mocked; E7");
        if(fam=="ExcelUDFs") return(p.Contains("UDFdataImport")||p.Contains("ExcelArg")?"04A.39":p.Contains("/Regression/")?"04A.40":p.Contains("Multivariate")||p.Contains("Classification")||p.Contains("Propensity")||p.Contains("PlotData")?"04A.42":"04A.41", "Core numerical / Common UDF policy / Windows UDF maintainers", "Scalar/array/error/missing/finite/marshaling parity; exported signature compatibility; 04A.43 cache/thread/handle-lifetime fixtures");
        if(fam=="DM") return(p.Contains("ResultWriter")||p.Contains("SheetManagement")?"04A.28":p.Contains("PSM")?"04A.25":p.Contains("UIprocedures")?"04A.44":"04A.27", "Core/Common input/output / Windows adapter maintainers", "Typed date/error/coercion/row-provenance and table/p-value/alpha/placement mock fixtures; E1/E3/E5/E7");
        if(fam=="RegModels") return(p.Contains("Matrix")?"04A.48":p.Contains("MixedModel")?"04A.14":"04A.09", "Core model / Common report / Windows compatibility maintainers", "Legacy signature and numeric/result-table goldens; compare exact Core overload and call semantics");
        return("04A.44", "Core/Common infrastructure / Windows host maintainers", "Settings/alpha/help/version/fault/progress/document retirement and disposal fixtures; GC8; public lifecycle compatibility");
    }
    static void Classify()
    {
        var nodesById=MemberRows.ToDictionary(x=>x.Value["symbol_id"],x=>x.Key);
        var callsByOwner=Refs.ToLookup(x=>x["owner_id"]);
        var edgesByTarget=Edges.ToLookup(x=>x["target"],StringComparer.OrdinalIgnoreCase);
        var portableMembers=MemberRows.Where(k=>k.Value["path"].StartsWith("BESHStat.Core/")||k.Value["path"].StartsWith("BESHStat.Excel.Common/")).ToArray();
        foreach(var r in Symbols) {
            var node=nodesById[r["symbol_id"]];
            var text=node.ToString(); var f=Followup(r);
            var calls=callsByOwner[r["symbol_id"]].ToArray();
            var deps=string.Join("; ", calls.Select(x=>x["reference"]).Distinct());
            bool designer=r["path"].EndsWith(".Designer.vb");
            bool viewOnly = r["family"]=="UI" && !Regex.IsMatch(text,@"(?i)(ResultTable|\.Compute\(|\.Fit\(|wrapResults|\.Run\(|Matrix\.|Math\.|getData|Parse|TermSpec|Formula|arData|New .*Options|New .*Request)")
                && Regex.IsMatch(text,@"(?i)(\.Checked|\.Enabled|\.Visible|\.Focus\(|\.Text|\.Selected|\.Close\(|\.Hide\(|InitializeComponent|ShowDialog|WndProc|IWin32Window)");
            bool generated=r["path"].Contains("/My Project/")||r["path"].Contains("/Generated/");
            string disposition = designer||generated||viewOnly ? "retain-Windows-host-only" : "needs-investigation";
            string reason=designer?"WinForms Designer initialization/disposal configures native controls; preserve exact defaults and partial event wiring.":generated?"Generated Windows assembly/resource/application boundary; audit only, preserve bytes.":viewOnly?"Member body performs WinForms state/focus/dialog operations; host dependency visible in source. Policy parity still needs owner fixture.":"Member mixes or may hide reusable policy/calculation with host/session behavior; syntax and call references do not establish semantic equivalence. Review the located body and listed calls before deciding extraction.";
            if(r["kind"]=="EventStatement") { disposition="retain-Windows-host-only"; reason="Event declaration is a Windows dispatch contract, not an implemented statistical routine; preserve callers and detach behavior."; }
            // Recognize only structurally transparent forwarding wrappers. Target declarations/signatures are retained for human verification.
            string equivalent="none verified";
            if(node is MethodBlockSyntax mb && mb.Statements.Count==1 && mb.Statements[0] is ReturnStatementSyntax ret && ret.Expression is InvocationExpressionSyntax inv) {
                var expr=inv.Expression.ToString().Replace("Global.","");
                var split=expr.Split('.');
                if(split.Length>=2) {
                    var member=split[^1].Split('(')[0]; var type=split[^2];
                    var targets=portableMembers
                        .Where(k=>k.Value["containing_type"].EndsWith("."+type,StringComparison.OrdinalIgnoreCase)&&k.Value["name"].Equals(member,StringComparison.OrdinalIgnoreCase))
                        .Where(k=>Header(k.Key) is MethodStatementSyntax ts && ts.ParameterList?.Parameters.Count==inv.ArgumentList?.Arguments.Count).ToArray();
                    if(targets.Length==1) {
                        var t=targets[0].Value;
                        disposition="compatibility-facade-to-preserve";
                        equivalent=t["path"]+":"+t["start_line"]+" | "+t["containing_type"]+" | "+t["signature"];
                        reason="Single Return invokes the exact located portable type/member with matching argument count; preserve legacy signature. Syntax-level forwarding confirmed; overload/type-conversion and numerical parity require fixture review.";
                    }
                }
            }
            r["disposition"]=disposition; r["core_common_equivalent"]=equivalent; r["rationale"]=reason+" Located member: "+r["containing_type"]+"."+r["name"]+"; observed references: "+string.Join("; ",calls.Take(8).Select(x=>x["reference"]))+".";
            r["responsibility"]=r["family"] switch {"Graphics"=>"chart geometry/series/axes/CI and/or Excel COM rendering","UI"=>designer?"host UX; control initialization/disposal":"host UX; option/input/model/report policy (mixed review)","ExcelUDFs"=>"Excel-DNA registration/marshaling; validation/calculation/result/cache policy","DM"=>"typed input/coercion/row provenance; semantic output and host write orchestration","RegModels"=>"model/design/numeric or report preparation","Help"=>"Help identity/resolution/host launch","Update"=>"Windows update/persistence/deployment",_=>"host lifecycle/settings/diagnostics/registration/packaging"};
            r["dependencies"]=deps;
            r["owner_role"]=f.Role; r["future_batch"]=f.Batch; r["proposed_test"]=f.Test;
            r["risk"]=r["family"]=="UI"||r["family"]=="DM"?"UX option/default parity; source-row identity; E1/E3/E5/E7 as applicable":r["family"]=="ExcelUDFs"?"Public XLL name/signature, scalar/array/native errors, calc concurrency/cache lifetime":"Numeric/geometry/CI and caller-input mutation parity; host lifecycle/public compatibility";
            r["confidence"]=disposition=="needs-investigation"?"unresolved semantic ownership":disposition=="compatibility-facade-to-preserve"?"static forwarding verified; semantic parity pending":"static host dependency";
            r["review_status"]="static; semantic owner review pending";
            r["review_contract"]="unreviewed";
            r["visible_entry_references"]=string.Join("; ",edgesByTarget[r["name"]].Select(e=>e["edge_id"]));
            r["existing_tests"]="not individually verified; see test-index.csv candidate references (lexical only)";
            r["exception_tags"]=Regex.IsMatch(text,@"(?i)(ResultTable|pvalue|Output)")?"E1; E5; E7":Regex.IsMatch(text,@"(?i)(group|sourceRow|RangeData)")?"E3":r["family"]=="AppInfrastructure"?"E7; GC8":"E7 where family/chart contract is promoted";
            r["enclosing_symbol_id"]=node.Ancestors().Select(a=>MemberRows.GetValueOrDefault(a)??Aliases.GetValueOrDefault(a)).FirstOrDefault(x=>x!=null)?.GetValueOrDefault("symbol_id")??"type-scope";
            r["body_start_line"]=node is MethodBlockBaseSyntax body&&body.Statements.Count>0?Start(body.Statements[0]).ToString():"see declaration/accessor/initializer span";
            r["body_end_line"]=node is MethodBlockBaseSyntax bodyEnd&&bodyEnd.Statements.Count>0?End(bodyEnd.Statements[^1]).ToString():"see declaration/accessor/initializer span";
        }
        var reviewPath=ReviewOverride??Path.Combine(Root,"scripts/windows-migration-audit/review-decisions.json");
        if(File.Exists(reviewPath)) {
            var reviews=JsonSerializer.Deserialize<List<Dictionary<string,string>>>(File.ReadAllText(reviewPath))!;
            var reviewedIds = new HashSet<string>();
            foreach(var review in reviews) {
                var matches=Symbols.Where(s=>s["signature_key"]==review["signature_key"]&&s["body_sha256"]==review["body_sha256"]
                    && (!review.ContainsKey("symbol_id") || s["symbol_id"]==review["symbol_id"])).ToArray();
                if(matches.Length!=1)throw new Exception("Stale or ambiguous review decision: "+review["signature_key"]);
                if(!reviewedIds.Add(matches[0]["symbol_id"]))throw new Exception("Duplicate pinned review: "+matches[0]["symbol_id"]);
                if(review.GetValueOrDefault("review_revision") is "04A.01-continuation-02" or "04A.01-continuation-03") {
                    var identity=JsonSerializer.Deserialize<Dictionary<string,string>>(review.GetValueOrDefault("source_identity")??"{}")!;
                    var identityKeys=new[]{"symbol_id","path","start_line","end_line","signature","containing_type","body_sha256"};
                    if(identity.Count!=identityKeys.Length || identityKeys.Any(k=>identity.GetValueOrDefault(k)!=matches[0][k]))
                        throw new Exception("Incorrect exact source identity: "+matches[0]["symbol_id"]);
                    var required=JsonSerializer.Deserialize<string[]>(review.GetValueOrDefault("required_dependency_paths")??"[]")!;
                    var requiredPins=JsonSerializer.Deserialize<Dictionary<string,string>>(review.GetValueOrDefault("dependency_pins")??"{}")!;
                    if(required.Length==0 || required.Distinct().Count()!=required.Length || !required.Contains(matches[0]["path"]) || !required.ToHashSet().SetEquals(requiredPins.Keys))
                        throw new Exception("Incomplete required dependency manifest: "+matches[0]["symbol_id"]);
                    if(string.IsNullOrWhiteSpace(review.GetValueOrDefault("exception_applicability")))
                        throw new Exception("Missing exception applicability: "+matches[0]["symbol_id"]);
                }
                if(review.GetValueOrDefault("review_revision")=="04A.01-continuation-03") {
                    if(new[]{"profile","span_start","span_length"}.Any(k=>review.GetValueOrDefault("source_"+k)!=matches[0][k]))
                        throw new Exception("Incorrect source profile/span: "+matches[0]["symbol_id"]);
                    using var assessment=JsonDocument.Parse(review.GetValueOrDefault("equivalence_assessment")??"{}");
                    var scope=review.GetValueOrDefault("equivalence_scope");
                    var targets=JsonSerializer.Deserialize<List<Dictionary<string,string>>>(review.GetValueOrDefault("portable_equivalent_pins")??"[]")!;
                    if(!review.ContainsKey("compared_portable_declarations"))throw new Exception("Missing comparison-only declaration field: "+matches[0]["symbol_id"]);
                    var comparisons=JsonSerializer.Deserialize<List<Dictionary<string,string>>>(review["compared_portable_declarations"])!;
                    var targetIds=targets.Select(t=>t["symbol_id"]).ToArray();
                    if(assessment.RootElement.GetProperty("scope").GetString()!=scope ||
                       !assessment.RootElement.GetProperty("target_ids").EnumerateArray().Select(t=>t.GetString()).ToHashSet().SetEquals(targetIds) ||
                       targetIds.Distinct().Count()!=targetIds.Length || (scope!="none" && targetIds.Length==0) ||
                       (scope=="whole-method" && targetIds.Length!=1) || string.IsNullOrWhiteSpace(assessment.RootElement.GetProperty("proof").GetString()))
                        throw new Exception("Unsupported whole/helper equivalence assertion: "+matches[0]["symbol_id"]);
                    foreach(var target in targets.Concat(comparisons)) {
                        var actual=portableMembers.SingleOrDefault(p=>p.Value["symbol_id"]==target["symbol_id"]).Value;
                        if(actual==null || new[]{"symbol_id","path","start_line","end_line","signature","containing_type","body_sha256"}.Any(k=>target.GetValueOrDefault(k)!=actual[k]))
                            throw new Exception("Stale portable equivalent declaration: "+matches[0]["symbol_id"]);
                    }
                }
                if(review.TryGetValue("dependency_pins",out var pins)) {
                    foreach(var pin in JsonSerializer.Deserialize<Dictionary<string,string>>(pins)!)
                        if(Sha(Path.Combine(Root,pin.Key))!=pin.Value)throw new Exception("Stale review dependency: "+pin.Key+" for "+review["signature_key"]);
                }
                foreach(var pair in review.Where(p=>!new[]{"signature_key","body_sha256","symbol_id"}.Contains(p.Key))) {
                    if(new[]{"path","start_line","end_line","signature","containing_type","family","name","kind","profile"}.Contains(pair.Key))throw new Exception("Review cannot override source identity: "+pair.Key);
                    matches[0][pair.Key]=pair.Value;
                }
                if(review.GetValueOrDefault("review_contract")=="04A.01-member-v1" && !SemanticReviewed(matches[0]))throw new Exception("Incomplete semantic review: "+matches[0]["symbol_id"]);
            }
        }
        foreach(var edge in Edges) {
            if(edge["kind"]=="Designer-configuration") {edge["target_status"]="configuration-expression; runtime state unproven";continue;}
            if(edge["kind"]=="event-declaration") {edge["target_status"]="event-contract-declaration; dispatch is separately inventoried";continue;}
            if(edge["kind"]=="lambda") {edge["target_status"]="resolved-inline-syntax";edge["target_ids"]=edge["target"];continue;}
            var target=Regex.Replace(edge["target"],@"^(?i:AddressOf\s+)?(?i:Me\.|MyBase\.|MyClass\.)?","");
            var sameType=Symbols.Where(s=>s["containing_type"]==edge["containing_type"] && s["name"].Equals(target,StringComparison.OrdinalIgnoreCase)).ToArray();
            if(sameType.Length==1) {edge["target_status"]="resolved-syntax-declaration; runtime unproven";edge["target_ids"]=sameType[0]["symbol_id"];}
            else {edge["target_status"]="unresolved-dynamic-or-external";edge["target_ids"]=string.Join("; ",sameType.Select(s=>s["symbol_id"]));}
            edge["owner_role"]="Windows UI/host wiring maintainer";edge["future_batch"]="04A.45";edge["verification"]="Bind actual delegate/event type, overload and instance; fake-host attach/detach plus owner Excel smoke";
        }
        foreach(var state in State.Where(s=>s["with_events"]=="True")) {
            var name=state["name"];
            var wired=Edges.Where(e=>e["containing_type"]==state["containing_type"]&&e["event_or_control"].StartsWith(name+".",StringComparison.OrdinalIgnoreCase)).ToArray();
            Edges.Add(Row("edge_id",state["path"]+"|WithEvents|"+name+"@"+state["start_line"],"path",state["path"],"containing_type",state["containing_type"],"owner_id","type-scope","kind","WithEvents-control-or-source","event_or_control",name,"target",state["declaration"],"start_line",state["start_line"],"end_line",state["end_line"],"target_status",wired.Length>0?"has-static-Handles; inspect dynamic edges":"no-Handles-found; decorative/default/indirect intent unresolved","target_ids",string.Join("; ",wired.Select(x=>x["edge_id"])),"evidence_status","static","owner_role","Windows UI maintainer","future_batch","04A.45","verification","Review default/enable/reset/validation behavior from Designer configuration and owning handlers; label/decorative intent requires review"));
        }
        var wiringReviewPath=WiringReviewOverride??Path.Combine(Root,"scripts/windows-migration-audit/wiring-review-decisions.json");
        if(File.Exists(wiringReviewPath)) {
            var seenReviews = new HashSet<string>();
            foreach(var decision in JsonSerializer.Deserialize<List<Dictionary<string,string>>>(File.ReadAllText(wiringReviewPath))!) {
                var edge=Edges.SingleOrDefault(e=>e["edge_id"]==decision["edge_id"])??throw new Exception("Stale wiring review: "+decision["edge_id"]);
                if(!seenReviews.Add(edge["edge_id"]))throw new Exception("Duplicate wiring review: "+edge["edge_id"]);
                var identity=string.Join("|",new[]{"path","kind","event_or_control","target","start_line","end_line","owner_id"}.Select(k=>edge[k]));
                if(Hash(identity)!=decision["edge_sha256"])throw new Exception("Stale wiring edge hash: "+edge["edge_id"]);
                foreach(var pin in JsonSerializer.Deserialize<Dictionary<string,string>>(decision["dependency_pins"])!)
                    if(Sha(Path.Combine(Root,pin.Key))!=pin.Value)throw new Exception("Stale wiring dependency: "+pin.Key);
                foreach(var id in decision["target_ids"].Split("; ",StringSplitOptions.RemoveEmptyEntries))
                    if(!Symbols.Any(s=>s["symbol_id"]==id))throw new Exception("Unbound reviewed wiring target: "+id);
                if(new[]{"rationale","verification","instance_contract","lifetime_contract","static_boundary","review_status"}.Any(k=>string.IsNullOrWhiteSpace(decision.GetValueOrDefault(k))))throw new Exception("Incomplete wiring review: "+edge["edge_id"]);
                if(decision["target_status"] is not ("reviewed-static-target; runtime deferred" or "reviewed-runtime-supplied-delegate; runtime deferred"))throw new Exception("Invalid reviewed wiring status: "+edge["edge_id"]);
                foreach(var key in new[]{"target_status","target_ids","rationale","verification","instance_contract","lifetime_contract","static_boundary","review_status","future_batch","owner_role"})edge[key]=decision[key];
                edge["evidence_status"]="source-pinned-static-review; no native runtime proof";
            }
        }
        var controlReviewPath=Path.Combine(Root,"scripts/windows-migration-audit/control-review-decisions.json");
        if(File.Exists(controlReviewPath)) {
            var seenControls = new HashSet<string>();
            foreach(var decision in JsonSerializer.Deserialize<List<Dictionary<string,string>>>(File.ReadAllText(controlReviewPath))!) {
                if(!seenControls.Add(decision["control_id"]))throw new Exception("Duplicate control review: "+decision["control_id"]);
                var fields=State.Where(s=>s["with_events"]=="True" && s["path"].Contains("/UI/") && s["containing_type"]+"."+s["name"]==decision["control_id"]).ToArray();
                if(fields.Length!=1 || fields[0]["path"]!=decision["declaration_path"] || fields[0]["start_line"]!=decision["declaration_line"] || fields[0]["declaration"]!=decision["type_declaration"])throw new Exception("Stale control review: "+decision["control_id"]);
                foreach(var pin in JsonSerializer.Deserialize<Dictionary<string,string>>(decision["dependency_pins"])!)
                    if(Sha(Path.Combine(Root,pin.Key))!=pin.Value)throw new Exception("Stale control dependency: "+pin.Key);
                foreach(var id in decision["source_edges"].Split("; ",StringSplitOptions.RemoveEmptyEntries))
                    if(!Edges.Any(e=>e["edge_id"]==id && e.GetValueOrDefault("review_status")=="manually-reviewed-static-edge"))throw new Exception("Unbacked control binding: "+id);
                if(new[]{"reviewed_intent","rationale","option_contract","owner_role","future_batch","verification"}.Any(k=>string.IsNullOrWhiteSpace(decision.GetValueOrDefault(k))))throw new Exception("Incomplete control review: "+decision["control_id"]);
                ReviewedControls++;
                if(decision.GetValueOrDefault("review_contract")=="04A.01-control-v1") {
                    if(new[]{"defaults_contract","enabled_reset_contract","validation_output_contract","option_contract"}.Any(k=>
                        string.IsNullOrWhiteSpace(decision.GetValueOrDefault(k)) || decision[k].Contains("unreviewed",StringComparison.OrdinalIgnoreCase)))
                        throw new Exception("Incomplete full control contract: "+decision["control_id"]);
                    QualifiedControlContracts++;
                }
            }
        }
        foreach(var e in Exports) {
            var symbol=Symbols.SingleOrDefault(s=>s["symbol_id"]==e["symbol_id"]);
            foreach(var k in new[]{"disposition","core_common_equivalent","owner_role","future_batch","proposed_test","dependencies","review_status","review_contract","responsibility","rationale","equivalence_scope","separable_responsibilities","runtime_boundary"}) e[k]=symbol?.GetValueOrDefault(k)??"unreviewed; Windows UDF maintainer / 04A.38";
            e["marshaling_error_shape_cache"]=symbol!=null&&SemanticReviewed(symbol)?symbol["rationale"]+" Boundary split: "+symbol["separable_responsibilities"]+"; native runtime parity remains deferred.":"Inspect signature/attributes and member references; ExcelError/ExcelMissing/ExcelEmpty/native array/spill/cache conversion semantic parity not yet verified";
        }
    }
    static string CsvValue(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
    static string Csv(List<Dictionary<string,string>> rows)
    {
        if(rows.Count==0)return "";
        var keys=rows.SelectMany(r=>r.Keys).Distinct().ToArray();
        return string.Join(",",keys.Select(CsvValue))+"\n"+string.Join("\n",rows.Select(r=>string.Join(",",keys.Select(k=>CsvValue(r.GetValueOrDefault(k)??"")))))+"\n";
    }
    static void Write(string name,string content,bool check,List<string> failures)
    {
        var path=Path.Combine(Output,name);
        if(check) {if(!File.Exists(path)||File.ReadAllText(path)!=content)failures.Add(name+": source-derived data differs or file missing");}
        else File.WriteAllText(path,content,new UTF8Encoding(false));
    }
    static void SelfTest()
    {
        // Fixtures exercise multiline attributes, overloads, generics, partials, custom events,
        // constructors, property/accessors, operators, Declare, delegates, initializers and callbacks.
        var fixture="""
        Partial Public Class Fixture
          Private Shared seed As Integer = 1
          Public ReadOnly Property Buffer As New System.Collections.Generic.List(Of Integer)
          Public Delegate Sub Callback(x As Integer)
          Public Declare Function Native Lib "x" () As Integer
          Public Sub New()
          End Sub
          <ExcelFunction(Name:="BESH.FIXTURE")>
          Public Function F(Of T)(
            x As T) As T
            AddHandler Me.Changed, AddressOf Handler
            RemoveHandler Me.Changed, AddressOf Handler
            Dim f = Function(z As Integer) z + seed
            Static counter As Integer = 1
            Return x
          End Function
          Public Function F(x As Integer, y As Integer) As Integer
            Return x + y
          End Function
          Public Property P As Integer
            Get
              Return seed
            End Get
            Set(value As Integer)
              seed = value
            End Set
          End Property
          Public Custom Event Changed As System.EventHandler
            AddHandler(value As System.EventHandler)
            End AddHandler
            RemoveHandler(value As System.EventHandler)
            End RemoveHandler
            RaiseEvent(sender As Object, e As System.EventArgs)
            End RaiseEvent
          End Event
          Private Sub Handler(sender As Object, e As System.EventArgs) Handles Me.Changed
          End Sub
          Public Shared Operator +(a As Fixture, b As Fixture) As Fixture
            Return a
          End Operator
        End Class
        """;
        var root=VisualBasicSyntaxTree.ParseText(fixture).GetRoot();
        if(root.GetDiagnostics().Any(x=>x.Severity==DiagnosticSeverity.Error))throw new Exception("Fixture parser diagnostics");
        var members=root.DescendantNodes().Where(n=>IsMember(n)||IsInitializer(n)).Select(n=>n.Kind()+"@"+n.Span.Start).ToArray();
        if(members.Length!=17)throw new Exception("Fixture expected 17 code-bearing records, got "+members.Length);
        var events=root.DescendantNodes().Where(n=>n is AddRemoveHandlerStatementSyntax or HandlesClauseItemSyntax).Select(n=>n.Kind()+"@"+n.Span.Start).ToArray();
        if(events.Length!=3)throw new Exception("Fixture event census");
        var exports=root.DescendantNodes().OfType<AttributeSyntax>().Where(x=>x.Name.ToString()=="ExcelFunction").Select(n=>"export@"+n.Span.Start).ToArray();
        if(exports.Length!=1)throw new Exception("Fixture export census");
        foreach(var set in new[]{members,events,exports}) {
            var incomplete=set.Skip(1).ToArray();
            if(!set.Except(incomplete).Any())throw new Exception("Missing ledger entry was not detected");
            var stale=set.Append("stale-entry").ToArray();
            if(!stale.Except(set).Any())throw new Exception("Stale ledger entry was not detected");
        }
        var semanticFixture=SemanticFields.ToDictionary(k=>k,k=>"source-specific fixture evidence");
        semanticFixture["review_status"]="manually-reviewed-static";
        semanticFixture["review_contract"]="04A.01-member-v1";
        semanticFixture["equivalence_scope"]="none";
        if(!SemanticReviewed(semanticFixture))throw new Exception("Complete semantic fixture rejected");
        semanticFixture["review_contract"]="unreviewed";
        if(SemanticReviewed(semanticFixture))throw new Exception("Generic classification conferred semantic review");
        semanticFixture["review_contract"]="04A.01-member-v1";semanticFixture.Remove("source_purpose");
        if(SemanticReviewed(semanticFixture))throw new Exception("Incomplete review conferred semantic coverage");
        semanticFixture["source_purpose"]="specific caller";semanticFixture["review_status"]="static; semantic owner review pending";
        if(SemanticReviewed(semanticFixture))throw new Exception("Static rule conferred manual review");
        Console.WriteLine("PASS 14 fixture assertions: original 10 parser/census guards plus four independent semantic-review guards. No production files changed.");
    }
    public static int Main(string[] args)
    {
        try {return Run(args);} catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
    static int Run(string[] args)
    {
        if(args.Contains("--self-test")){SelfTest();return 0;}
        Root=Path.GetFullPath(args.FirstOrDefault(x=>!x.StartsWith("--"))??Directory.GetCurrentDirectory());
        ReviewOverride=args.FirstOrDefault(x=>x.StartsWith("--reviews="))?.Split('=',2)[1];
        WiringReviewOverride=args.FirstOrDefault(x=>x.StartsWith("--wiring-reviews="))?.Split('=',2)[1];
        Output=Path.GetFullPath(args.FirstOrDefault(x=>x.StartsWith("--ledger="))?.Split('=',2)[1]??Path.Combine(Root,"docs/architecture/WindowsMigrationLedger"));
        var check=args.Contains("--check"); if(!check&&!args.Contains("--write"))throw new Exception("Choose --write or --check (no default mutation)");
        if(!check)Directory.CreateDirectory(Output);
        var project=XDocument.Load(Path.Combine(Root,"BESHstatNG/BESHStatNG.vbproj"));
        var ns=project.Root!.Name.Namespace;
        var compile=project.Descendants(ns+"Compile").Select(x=>"BESHstatNG/"+x.Attribute("Include")!.Value.Replace('\\','/')).Order(StringComparer.Ordinal).ToArray();
        if(compile.Distinct().Count()!=compile.Length)throw new Exception("Duplicate explicit Compile items");
        foreach(var path in compile) {if(!File.Exists(Path.Combine(Root,path)))throw new Exception("Missing Compile source: "+path);Parse(path,project.Descendants(ns+"RootNamespace").First().Value,true);}
        foreach(var portable in new[]{"BESHStat.Core","BESHStat.Excel.Common"}) {
            var doc=XDocument.Load(Path.Combine(Root,portable,portable+".vbproj"));
            var rn=doc.Descendants("RootNamespace").FirstOrDefault()?.Value??portable.Replace(' ','_');
            foreach(var p in Directory.EnumerateFiles(Path.Combine(Root,portable,"src"),"*.vb",SearchOption.AllDirectories).Order(StringComparer.Ordinal))Parse(Rel(p),rn,false);
        }
        Classify();
        foreach(var path in compile) {
            var members=Symbols.Where(s=>s["path"]==path).ToArray();
            Files.Add(Row("path",path,"family",Family(path),"explicit_compile","true","generated",(path.Contains("/My Project/")||path.Contains("/Generated/")||path.EndsWith(".Designer.vb")||path.EndsWith("HelpLinks.vb")||path.EndsWith("HelpContext.vb")).ToString(),"ignored","false (tracked explicit source)","sha256",Sha(Path.Combine(Root,path)),"physical_loc",File.ReadAllLines(Path.Combine(Root,path)).Length.ToString(),"parsed_member_count",members.Length.ToString(),"ledger_member_count",members.Length.ToString(),"manually_reviewed_static_count",members.Count(s=>s["review_status"]=="manually-reviewed-static").ToString(),"semantic_parity_verified_count","0","historical_delta",Family(path)=="DM"&&(path.EndsWith("ExcelDnaSharedOutputPlanAdapter.vb")||path.EndsWith("ExcelDnaCompositeOutputAdapter.vb")||path.EndsWith("ExcelDnaOutputTargetAdapter.vb"))?"Absent from 18-Oct 9-file headline; accepted Common output integration, current DM=12":Family(path)=="AppInfrastructure"&&!path.EndsWith("AppGlobals.vb")&&!path.EndsWith("ExcelDnaHostInfrastructure.vb")?"Absent from 18-Oct 2-file headline; accepted infrastructure integration":"Compare historical provisional inventory; live HEAD authoritative"));
        }
        var controls=State.Count(x=>x["with_events"]=="True");
        var ribbon=XDocument.Load(Path.Combine(Root,"BESHstatNG/Resources/RibbonXML.xml"),LoadOptions.SetLineInfo);
        var ribbonRows=new List<Dictionary<string,string>>();
        var actions=ribbon.Descendants().Where(x=>x.Attribute("onAction")!=null).ToArray();
        foreach(var button in actions) {
            var callback=button.Attribute("onAction")!.Value;
            var handlers=Symbols.Where(s=>s["path"]=="BESHstatNG/BESHStatAddIn.vb"&&s["name"]==callback).ToArray();
            var h=handlers.Length==1?handlers[0]:null;
            ribbonRows.Add(Row("ribbon_id",button.Attribute("id")?.Value??"no-id","element",button.Name.LocalName,"label",button.Attribute("label")?.Value??"","onAction",callback,"xml_line",((System.Xml.IXmlLineInfo)button).LineNumber.ToString(),"handler_ids",string.Join("; ",handlers.Select(x=>x["symbol_id"])),"handler_path",h?.GetValueOrDefault("path")??"UNRESOLVED","handler_line",h?.GetValueOrDefault("start_line")??"UNRESOLVED","handler_references",h?.GetValueOrDefault("dependencies")??"UNRESOLVED","reuse_count",actions.Count(x=>x.Attribute("onAction")?.Value==callback).ToString(),"status",h==null?"unresolved":"resolved syntax declaration; runtime unproven","owner_role","Windows Ribbon maintainer","future_batch","04A.45","verification","Owner Excel callback activation and form/tag/Help/default parity; three callback reuse sites require catalog reconciliation"));
        }
        var tests=new List<Dictionary<string,string>>();
        foreach(var dir in new[]{"BESHStat.Core.Tests","BESHStat.Excel.Common.Tests","BESHStatNG.Test"})
        foreach(var p in Git("ls-files",dir+"/*.vb",dir+"/**/*.vb").Split('\n',StringSplitOptions.RemoveEmptyEntries)) {
            var source=File.ReadAllText(Path.Combine(Root,p));
            foreach(var line in source.Split('\n').Select((s,i)=>(s,i)).Where(x=>Regex.IsMatch(x.s,@"(?i)(<TestMethod|Public Sub |Public Function )")))
                tests.Add(Row("path",p,"line",(line.i+1).ToString(),"declaration",Norm(line.s),"status","test discovery aid; not per-member semantic coverage proof"));
        }
        var supplemental=new List<Dictionary<string,string>>();
        foreach(var p in Git("ls-files","BESHstatNG","BESHStatNG_Installer").Split('\n',StringSplitOptions.RemoveEmptyEntries).Where(p=>!compile.Contains(p))) {
            var ext=Path.GetExtension(p).ToLowerInvariant();
            supplemental.Add(Row("path",p,"extension",ext,"sha256",Sha(Path.Combine(Root,p)),"kind",p.EndsWith(".vb")?"excluded-vb-not-Compile":ext is ".dll" or ".exe" or ".xll" or ".png" or ".jpg" or ".ico" or ".gif"?"binary-asset-not-portable-source":"integration/asset/tool/installer boundary","disposition","retain-Windows-host-only","owner_role","Windows packaging/host maintainer","verification","Read-only boundary inventory; no installer/signing/release or runtime proof"));
        }
        var failures=new List<string>();
        var portableSymbols=MemberRows.Values.Where(x=>!compile.Contains(x["path"])).ToList();
        var registry=new List<Dictionary<string,string>>();
        foreach(var t in Trees.Where(t=>t.Path.EndsWith("/AnalysisHandlerRegistry.vb")).Take(1))
        foreach(var creation in t.Node.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Where(n=>n.Type.ToString().Contains("TypedAnalysisHandler")))
            registry.Add(Row("path",t.Path,"line",Start(creation).ToString(),"handler_type",Norm(creation.Type.ToString()),"registration",Norm(creation.ArgumentList?.ToString()??""),"status","static registry construction; host capability intersection separate"));
        var tables=new Dictionary<string,List<Dictionary<string,string>>>{{"files.csv",Files},{"symbols.csv",Symbols},{"types.csv",Types},{"state.csv",State},{"ui-wiring.csv",Edges},{"references.csv",Refs},{"udf-exports.csv",Exports},{"ribbon-crosswalk.csv",ribbonRows},{"supplemental-files.csv",supplemental},{"parser-diagnostics.csv",Diagnostics},{"test-index.csv",tests},{"portable-symbols.csv",portableSymbols},{"common-registry.csv",registry}};
        foreach(var table in tables)Write(table.Key,Csv(table.Value),check,failures);
        var coverage = new {
            analyzer=Version, parser=typeof(VisualBasicSyntaxTree).Assembly.FullName, source_head=Git("rev-parse","HEAD"), main_head=Git("rev-parse","main"), branch=Git("branch","--show-current"),
            command="dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check",
            baseline="Master plan Revision 2.1; 40/223 accepted; Gate C owner approved with exceptions; 04A.01 not owner accepted",
            status="NOT READY FOR ACCEPTANCE: semantic review, indirect event binding and high-risk family equivalence remain open",
            compile_files=compile.Length, src_files=compile.Count(x=>x.Contains("/src/")), file_families=Files.GroupBy(x=>x["family"]).ToDictionary(g=>g.Key,g=>g.Count()),
            source_member_denominator=MemberRows.Values.Count(x=>compile.Contains(x["path"])), ledger_member_numerator=Symbols.Count,
            actual_profile_records=Symbols.Count(x=>x["profile"]=="project MyType=Windows"), alternative_only_records=Symbols.Count(x=>x["profile"]!="project MyType=Windows"),
            member_kinds=Symbols.GroupBy(x=>x["kind"]).ToDictionary(g=>g.Key,g=>g.Count()), dispositions=Symbols.GroupBy(x=>x["disposition"]).ToDictionary(g=>g.Key,g=>g.Count()),
            inventoried_ids=Symbols.Select(x=>x["symbol_id"]).ToArray(), uninventoried_ids=MemberRows.Values.Where(x=>compile.Contains(x["path"])).Select(x=>x["symbol_id"]).Except(Symbols.Select(x=>x["symbol_id"])).ToArray(),
            invalid_classification_ids=Symbols.Where(x=>!new[]{"already-in-Core","already-in-Common","extract-to-Core","extract-to-Common","retain-Windows-host-only","compatibility-facade-to-preserve","needs-investigation"}.Contains(x["disposition"])||new[]{"owner_role","future_batch","proposed_test","rationale"}.Any(k=>string.IsNullOrWhiteSpace(x[k]))).Select(x=>x["symbol_id"]).ToArray(),
            parser_diagnostics=Diagnostics.Count, unparsed=Diagnostics, manually_reviewed_static_members=Symbols.Count(s=>s["review_status"]=="manually-reviewed-static"), semantic_parity_verified_members=0,
            semantic_review_denominator=Symbols.Count, semantic_review_qualified_members=Symbols.Count(SemanticReviewed), generated_rule_verified_members=0,
            semantic_review_unreviewed_ids=Symbols.Where(s=>!SemanticReviewed(s)).Select(s=>s["symbol_id"]).ToArray(),
            control_intent_review_denominator=State.Count(s=>s["with_events"]=="True"&&s["path"].Contains("/UI/")), source_reviewed_control_intents=ReviewedControls, qualified_full_control_contracts=QualifiedControlContracts,
            ui_files=compile.Count(x=>x.Contains("/UI/")), designer_files=compile.Count(x=>x.Contains("/UI/")&&x.EndsWith(".Designer.vb")), with_events_fields=controls,
            edges_by_kind=Edges.GroupBy(x=>x["kind"]).ToDictionary(g=>g.Key,g=>g.Count()), unresolved_edges=Edges.Where(e=>e["target_status"].Contains("unresolved")).Select(e=>e["edge_id"]).ToArray(),
            ribbon_action_references=actions.Length, ribbon_buttons=ribbon.Descendants().Count(x=>x.Name.LocalName=="button"), ribbon_distinct_actions=actions.Select(x=>x.Attribute("onAction")!.Value).Distinct().Count(), ribbon_unresolved=ribbonRows.Where(x=>x["status"]=="unresolved").ToArray(),
            udf_attribute_denominator=Trees.Where(t=>compile.Contains(t.Path)).SelectMany(t=>t.Node.DescendantNodes().OfType<AttributeSyntax>().Where(a=>Regex.IsMatch(a.Name.ToString(),@"(?i)(^|\.)ExcelFunction(Attribute)?$")).Select(a=>t.Path+"@"+a.Span.Start)).Distinct().Count(),
            udf_export_numerator=Exports.Count, udf_exporting_files=Exports.Select(x=>x["path"]).Distinct().Count(), udf_duplicate_effective_names=Exports.GroupBy(x=>x["effective_name"],StringComparer.OrdinalIgnoreCase).Where(g=>g.Count()>1).ToDictionary(g=>g.Key,g=>g.Select(x=>x["export_id"]).ToArray()),
            udf_unresolved=Exports.Where(e=>e["symbol_id"]=="UNRESOLVED"||e["effective_name"].StartsWith("UNRESOLVED")).ToArray(),
            limits=new[]{"Syntax-only call targets; no semantic model/late-bound COM/runtime proof", "MyType=Windows actual project and WindowsForms generated alternative; other disabled text reported separately", "Per-member manual equivalence and test coverage incomplete; no 100% semantic review claim", "Control defaults are source assignment edges, not runtime state; no-handler intent unresolved", "No Excel/Office/native execution; 04A.02 golden capture not performed"}
        };
        Write("coverage.json",JsonSerializer.Serialize(coverage,Json)+"\n",check,failures);
        Write("source-provenance.json",JsonSerializer.Serialize(new {source_head=Git("rev-parse","HEAD"), main_head=Git("rev-parse","main"), source_changes_at_head=Git("show","--stat","--oneline","HEAD"), submodules=Git("submodule","status"), profiles="DEBUG/TRACE=true, CONFIG=Debug, VBA7=true, Win64=false/true; MyType=Windows project plus WindowsForms generated branch; all checked-in #If directives are MyType comparisons", sdk=Environment.Version.ToString(), analyzer=Version},Json)+"\n",check,failures);
        if(Symbols.Select(s=>s["symbol_id"]).Distinct().Count()!=Symbols.Count)failures.Add("duplicate symbol IDs");
        if(Edges.Select(s=>s["edge_id"]).Distinct().Count()!=Edges.Count)failures.Add("duplicate edge IDs");
        if(Exports.Any(x=>x["symbol_id"]=="UNRESOLVED"))failures.Add("unbound UDF attribute");
        if(ribbonRows.Any(x=>x["status"]=="unresolved"))failures.Add("unknown Ribbon callback");
        Console.WriteLine(JsonSerializer.Serialize(new{check,files=Files.Count,symbols=Symbols.Count,types=Types.Count,state=State.Count,edges=Edges.Count,exports=Exports.Count,ribbon=actions.Length,diagnostics=Diagnostics.Count,failures},Json));
        // Data integrity can pass with explicit open semantic risks. Strict acceptance cannot.
        // Integrity errors must not be masked as the expected incomplete-review exit code.
        if(failures.Count>0)return 1;
        if(args.Contains("--strict")&&(Symbols.Any(x=>!SemanticReviewed(x))||QualifiedControlContracts!=State.Count(s=>s["with_events"]=="True"&&s["path"].Contains("/UI/"))||Symbols.Any(x=>x["disposition"]=="needs-investigation")||Diagnostics.Count>0||Edges.Any(x=>x["target_status"].Contains("unresolved")))) {
            Console.Error.WriteLine($"OPEN COVERAGE BLOCKERS: semantic review {Symbols.Count(SemanticReviewed)}/{Symbols.Count}; complete control contracts {QualifiedControlContracts}/{State.Count(s=>s["with_events"]=="True"&&s["path"].Contains("/UI/"))} (routing/intent pins {ReviewedControls}); see coverage.json; NOT READY FOR ACCEPTANCE");return 2;
        }
        return failures.Count==0?0:1;
    }
}
