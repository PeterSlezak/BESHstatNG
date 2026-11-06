using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

static partial class Program
{
    static void Reconcile(List<Dictionary<string,string>> old,List<Dictionary<string,string>> current,Dictionary<string,string> mapped,List<Dictionary<string,string>> portable)
    {
        var cross=new List<Dictionary<string,string>>(); var byId=current.ToDictionary(r=>r["member_id"]);
        foreach(var h in old)
        {
            var id=h["symbol_id"]; var target=mapped.GetValueOrDefault(id,""); string category,reason; var extra="";
            if(target!="")
            {
                var r=byId[target];
                if(r["historical_body_match"] is "EXACT_BODY_BYTES" or "LF_CRLF_TRANSFORM_VERIFIED_AGAINST_ORIGINAL_BODY_SHA") { category="UNCHANGED_MATCHED_SOURCE"; reason="Member body SHA exact or exact LF/CRLF transform verified against original body SHA, plus normalized signature; whole-file byte equality recorded separately. Offset/newline changes are not semantic changes."; }
                else { category="SPLIT_OPERATION";reason="Changed accepted seam; original decision retained; live member plus accepted Common/Core consumers divide the former operation. No whole semantic equivalence claimed."; extra=string.Join(';',portable.Where(p=>p["path"].Contains(Family(h["path"])=="PsmLovePlot"?"PsmLovePlotProjection":"SpcUnivariateChartProjection")).Select(p=>p["member_id"])); }
            }
            else if(h["path"].EndsWith("PsmLovePlotExcel.vb"))
            {
                category="CURRENT_RELOCATED_OR_REFACTORED"; reason="04A.25 live projection constructor/ChartValue/TableValue/MaxFiniteAbs carries former filtering, ordering, absolute-value/finite and axes preparation. Removed local lambdas/helpers retained as historical lineage; not a newly invented geometry task.";
                var wanted=h["name"] switch {"AbsForChart"=>"ChartValue","AbsOrMissing"=>"TableValue","MaxFiniteAbs"=>"MaxFiniteAbs",_=>".ctor"};
                target=string.Join(';',portable.Where(p=>p["path"].EndsWith("PsmLovePlotProjection.vb")&&p["name"]==wanted).Select(p=>p["member_id"]));
            }
            else if(h["path"].EndsWith("SpcControlChartExcel.vb"))
            {
                category="CURRENT_RELOCATED_OR_REFACTORED"; reason="04A.22 changed signatures/marshalling or moved shared axes/channels/labels/rule text into accepted Common projection. Exact original removed name is preserved; no deletion of original obligation.";
                var names=h["name"] switch {"BuildSharedAxisContext"=>new[]{"Project",".ctor"},"BuildPanelChartData"=>new[]{".ctor"},"FormatRuleNumbers"=>new[]{"RuleText"},"ChartValue"=>new[]{"IsFinite",".ctor"},_=>new[]{h["name"]}};
                target=string.Join(';',current.Concat(portable).Where(p=>(p["path"].EndsWith("SpcControlChartExcel.vb")||p["path"].EndsWith("SpcUnivariateChartProjectionPolicy.vb"))&&names.Contains(p["name"])).Select(p=>p["member_id"]));
                if(target=="") { target=string.Join(';',portable.Where(p=>p["path"].EndsWith("SpcUnivariateChartProjectionPolicy.vb")&&p["name"]==".ctor").Select(p=>p["member_id"]));reason+=" Constructor is the operation container; exact name not retained."; }
                if(h["name"] is "RenderPanel" or "AddPointLabels" or "AddSignalLabels" or "AddExclusionLabels" or ".ctor")category="CURRENT_REMOVED_OR_RENAMED";
            }
            else { category="HISTORICAL_UNRESOLVED";reason="No unique current signature/body or accepted relocated operation; Windows graphics owner must trace historical body before extraction."; }
            cross.Add(Row("historical_ordinal",(cross.Count+1).ToString(),"historical_member_id",id,"historical_source_sha256",h["source_sha256"],"historical_body_sha256",h["body_sha256"],"historical_checkpoint",h.GetValueOrDefault("checkpoint_id",""),"category",category,"current_member_ids",target,"portable_split_ids",extra,"reason",reason,"old_operation_id",h.GetValueOrDefault("candidate_operation_id",""),"owner","Windows/Core/Common graphics owners; original obligations OPEN","next_action",category=="HISTORICAL_UNRESOLVED"?"Resolve before extracting affected operation":"Use pinned old/current source and downstream casebook; not permission to migrate"));
        }
        Csv("04A01-graphics-member-crosswalk-v1.csv",cross);
        var historicalDeclarations=new HashSet<string>();
        foreach(var path in old.Select(r=>r["path"]).Distinct())
        {
            var source=Git("show","11fa5d99a040de2e62c75ec201c49852c1d6b289:"+path);var root=VisualBasicSyntaxTree.ParseText(source).GetRoot();
            foreach(var n in root.DescendantNodes().Where(n=>n is VariableDeclaratorSyntax&&n.Parent is FieldDeclarationSyntax||n is PropertyStatementSyntax&&n.Parent is not PropertyBlockSyntax||n is EnumMemberDeclarationSyntax))historicalDeclarations.Add(path+"|"+TypeName(n,"BESHStatNG")+"|"+n.Kind()+"|"+Norm(n.ToString()));
        }
        var delta=new List<Dictionary<string,string>>();
        foreach(var r in current.Where(r=>!mapped.Values.Contains(r["member_id"])))
        {
            var state=r["kind"] is "PropertyStatement" or "VariableDeclarator" or "EnumMemberDeclaration";
            var preexisting=state&&historicalDeclarations.Contains(r["path"]+"|"+r["qualified_type"]+"|"+r["kind"]+"|"+r["signature"]);
            var category=preexisting?"NEWLY_ENUMERATED_PREEXISTING_DECLARATION":"CURRENT_NEW_MEMBER";
            delta.Add(Row("member_id",r["member_id"],"category",category,"introducing_evidence",preexisting?"Broader declaration census; exact declaration/type/kind matched to Git source11fa5d99a040de2e62c75ec201c49852c1d6b289, not a post-audit addition.":r["path"].Contains("Kaplan")?"04A.13 payload overload":r["path"].Contains("Psm")?"04A.25 projection seam":"04A.22 SPC projection seam/new projection state", "source_sha256",r["source_sha256"]));
        }
        Csv("current-member-lineage-delta-v1.csv",delta);
        var counts=new[]{"UNCHANGED_MATCHED_SOURCE","CURRENT_RELOCATED_OR_REFACTORED","CURRENT_NEW_MEMBER","CURRENT_REMOVED_OR_RENAMED","HISTORICAL_UNRESOLVED","SPLIT_OPERATION","UNCERTAIN_DIFFERENT_SOURCE"}.ToDictionary(c=>c,c=>cross.Count(r=>r["category"]==c));
        Write("04A01-graphics-reconciliation-v1.json",new { original_order="Immutable review-decisions.json object-array order, filtered to exact Graphics prefix; reconciled1:1to accepted symbols.csv. One row per original decision.", original_ordered_ids_sha256=ShaText(string.Join('\n',old.Select(r=>r["symbol_id"]))), original_total=old.Count, original_all_windows=3866, original_pin=Pin("docs/architecture/WindowsMigrationLedger/symbols.csv"), decision_objects_pin=Pin("scripts/windows-migration-audit/review-decisions.json"), original_provenance=Pin("docs/architecture/WindowsMigrationLedger/source-provenance.json"), original_review_csvs=Directory.GetFiles("docs/architecture/WindowsMigrationLedger","continuation*-member-reviews.csv").Where(p=>new[]{"14","15","16","17","18"}.Any(n=>Path.GetFileName(p)=="continuation"+n+"-member-reviews.csv")).Order().Select(Pin).ToArray(), original_source_head="11fa5d99a040de2e62c75ec201c49852c1d6b289", current_distinct_members=current.Count, ordered_original_decisions_disposed=cross.Count, categories=counts, unmapped_original_ids=cross.Where(r=>r["category"]=="HISTORICAL_UNRESOLVED").Select(r=>r["historical_member_id"]).ToArray(), current_delta=delta.GroupBy(r=>r["category"]).ToDictionary(g=>g.Key,g=>g.Count()), current_coverage_percent=100, historical_crosswalk="04A01-graphics-member-crosswalk-v1.csv", current_delta_file="current-member-lineage-delta-v1.csv", caveat="Newly enumerated state is not post-audit new code. SPLIT and relocation assert source responsibility lineage, not numeric equivalence. All native original controls remain open." });
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
