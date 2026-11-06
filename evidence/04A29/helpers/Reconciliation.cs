using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

static partial class Program
{
    // The original v1 mapper is retained only in superseded/v1-attempt. No family/name fallback.
    static void Reconcile(List<Dictionary<string,string>> old, List<Dictionary<string,string>> current,
                          Dictionary<string,string> mapped, List<Dictionary<string,string>> portable)
    {
        WriteLineageV2();
    }
    static void ExternalReferences(List<Dictionary<string,string>> members)
    {
        var types=Trees.Where(t=>t.path.StartsWith("BESHstatNG/src/Graphics/")).SelectMany(t=>t.root.DescendantNodes().OfType<TypeBlockSyntax>()).Select(t=>t.BlockStatement.Identifier.ValueText).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var rows=new List<Dictionary<string,string>>();
        foreach(var path in Git("ls-files","BESHstatNG","BESHStat.Excel.Common","BESHStat.Core","BESHStatNG.Test","BESHStat.Core.Tests","BESHStat.Excel.Common.Tests").Split('\n').Where(p=>p.EndsWith(".vb")&&!p.Contains("/src/Graphics/")))
        {
            var text=File.ReadAllText(path); var tree=VisualBasicSyntaxTree.ParseText(text); var root=tree.GetRoot();
            foreach(var node in root.DescendantNodes().OfType<IdentifierNameSyntax>().Where(n=>types.Contains(n.Identifier.ValueText)))
            {
                var parent=node.Ancestors().FirstOrDefault(n=>n is StatementSyntax)??node;
                var owner=node.Ancestors().OfType<MethodBlockBaseSyntax>().FirstOrDefault();
                var type=node.Ancestors().OfType<TypeBlockSyntax>().FirstOrDefault()?.BlockStatement.Identifier.ValueText??"module-scope";
                var targets=members.Where(m=>m["qualified_type"].Split('.').Last().Equals(node.Identifier.ValueText,StringComparison.OrdinalIgnoreCase)).Select(m=>m["path"]).Distinct().ToArray();
                rows.Add(Row("reference_id",path+"@"+node.Span.Start,"path",path,"line",Start(node).ToString(),"caller_type",type,"caller_member",owner==null?"state/declaration":Norm(owner.BlockStatement.ToString()),"referenced_type",node.Identifier.ValueText,"candidate_graphics_paths",string.Join(';',targets),"evidence",Norm(parent.ToString()),"source_sha256",FileSha(path),"reachability","STATIC_REFERENCE_ONLY","confidence","SOURCE_INFERRED","role",path.Contains("Tests")||path.Contains(".Test/")?"TEST_REFERENCE":"SHIPPING_SOURCE_REFERENCE"));
            }
            foreach(var call in root.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(i=>i.Expression is MemberAccessExpressionSyntax access&&access.Name.Identifier.ValueText is "AddPlot" or "AddKMplot"))
            {
                var access=(MemberAccessExpressionSyntax)call.Expression;var owner=call.Ancestors().OfType<MethodBlockBaseSyntax>().FirstOrDefault();var type=call.Ancestors().OfType<TypeBlockSyntax>().FirstOrDefault()?.BlockStatement.Identifier.ValueText??"module-scope";
                rows.Add(Row("reference_id",path+"@extension:"+call.Span.Start,"path",path,"line",Start(call).ToString(),"caller_type",type,"caller_member",owner==null?"state/declaration":Norm(owner.BlockStatement.ToString()),"referenced_type","EXTENSION_RECEIVER_UNBOUND:"+access.Expression,"candidate_graphics_paths",string.Join(';',members.Where(m=>m["name"]==access.Name.Identifier.ValueText).Select(m=>m["path"]).Distinct()),"evidence",Norm(call.ToString()),"source_sha256",FileSha(path),"reachability","STATIC_REFERENCE_ONLY","confidence","SOURCE_INFERRED","role",path.Contains("Tests")||path.Contains(".Test/")?"TEST_REFERENCE":"SHIPPING_SOURCE_REFERENCE"));
            }
        }
        Csv("graphics-external-references-v1.csv",rows);
        // Exact UI/callback/method metadata remains authored authority; lexical rows are not execution records.
        var methodMeta="BESHStatNG_Help_MkDocs/docs/_meta/methods.yml";
        var ribbon="BESHstatNG/Resources/RibbonXML.xml";
        Write("host-reachability-v1.json",new { external_references=rows.Count, caller_files=rows.Select(r=>r["path"]).Distinct().Count(), confidence="STATIC_REFERENCE_ONLY; no native execution, no semantic binding or dynamic callback registration proof", reference_file="graphics-external-references-v1.csv", authored_help_metadata=Pin(methodMeta), windows_callback_authority=Pin(ribbon), original_callback_index=Pin("docs/architecture/WindowsMigrationLedger/callback-index.csv"), original_catalog_crosswalk=Pin("docs/architecture/WindowsMigrationLedger/catalog-crosswalk.csv"), required_UI_consumers=new[]{"UibyID","Ui11PCA","Ui21ControlCharts","Ui22MultivariateControlCharts","Ui20PropensityScoreMatching","Ui4Cox","Ui0OneRefeditMulticol"}.Select(t=>new { type=t, static_graphics_references=rows.Count(r=>r["caller_type"].Equals(t,StringComparison.OrdinalIgnoreCase)), references=rows.Where(r=>r["caller_type"].Equals(t,StringComparison.OrdinalIgnoreCase)).Select(r=>r["reference_id"]).ToArray() }).ToArray(), lifetime="WithEvents chart overlay sinks and registries remain Windows; source callbacks do not establish event delivery/cleanup. Export/animation reachable source references are catalogued without execution.", limitation="Identifier-name references include declarations/options and may be unused. Extension-call instance receiver dispatch is catalogued separately in original catalog and source-anchored family casebook; zero lexical hits is not dead-code proof." });
    }
    static void ExistingCrosswalk(List<Dictionary<string,string>> files,List<Dictionary<string,string>> portable,List<Dictionary<string,string>> current)
    {
        var core=Directory.GetFiles("BESHStat.Core/src/Graphics","*.vb").Order().Select(p=>new { source=Pin(p), public_contract_ids=portable.Where(r=>r["path"]==p.Replace('\\','/')&&r["visibility"].Contains("Public")).Select(r=>r["member_id"]).ToArray(), windows_direct_callers=current.Where(r=>r["existing_contract"].Contains(Path.GetFileNameWithoutExtension(p)+".")).Select(r=>r["member_id"]).ToArray(), authoritative_tests=Directory.GetFiles("BESHStat.Core.Tests/src/Graphics",Path.GetFileNameWithoutExtension(p)+"Tests.vb").Select(Pin).ToArray(), status="EXISTING_CORE; no new migration of this algorithm; numeric API shapes in portable-members-v1.csv" }).ToArray();
        Write("graphics-existing-core-crosswalk-v1.json",new { core_source_count=core.Length, core, common_sources=new[]{"SpcUnivariateChartProjectionPolicy","SpcMultivariateChartProjectionPolicy","PsmLovePlotProjection"}.Select(n=>Pin("BESHStat.Excel.Common/src/Analysis/"+n+".vb")).ToArray(), families=files.GroupBy(f=>f["family"]).Select(g=>new { family=g.Key, windows_paths=g.Select(f=>f["path"]).ToArray(), existing_core=CoreName(g.Key), existing_common=CommonName(g.Key), neutral_ownership=NeutralPlan(g.Key), preserve=FamilyRisk(g.Key), actual_native_status="NOT_RUN", availability="Windows source only; future Office/Mac/LibreOffice reuse not availability" }).ToArray(), private_duplicates="FactorAnalysisPlotExcel ColumnSumsOfSquares/PercentOfTotal/TotalVariance duplicate private Core FA operations; contract exposure is a 04A.37 design question, not a new numerical algorithm.", defaults="Exact wrapper signatures and Core signatures recorded independently; direct forwards preserve optional defaults. Renderer fallbacks/culture/style ordering are separate operations in member inventory, not presumed covered by matching file name.", residual_native_dependency="Core Graphics uses strict portable code; source dependency guard in validation. Existing CoreInfrastructure logger/error policy remains accepted GC8 restriction; no new dependency introduced." });
    }
}
