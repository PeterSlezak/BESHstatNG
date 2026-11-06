using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

static partial class Program
{
    static void Casebook(List<Dictionary<string,string>> files,List<Dictionary<string,string>> members)
    {
        var tests=new List<Dictionary<string,string>>();
        var testPaths=Directory.GetFiles("BESHStat.Core.Tests/src/Graphics","*.vb").Concat(Git("ls-files","BESHStat.Excel.Common.Tests","BESHStatNG.Test","BESHStat.Core.Tests").Split('\n').Where(p=>p.EndsWith(".vb")&&Regex.IsMatch(p,@"(Spc22|Spc24|Psm25|Survival13|Multivariate17|Multivariate18|Agreement26|Pca).*Tests",RegexOptions.IgnoreCase))).Distinct().Order().ToArray();
        foreach(var p in testPaths)
        {
            var root=VisualBasicSyntaxTree.ParseText(File.ReadAllText(p)).GetRoot();
            foreach(var m in root.DescendantNodes().OfType<MethodBlockSyntax>().Where(m=>m.SubOrFunctionStatement.AttributeLists.ToString().Contains("TestMethod")||m.SubOrFunctionStatement.AttributeLists.ToString().Contains("DataTestMethod")))
            {
                tests.Add(Row("fixture_id",p.Replace('\\','/')+"|"+m.SubOrFunctionStatement.Identifier.ValueText,"path",p.Replace('\\','/'),"test_name",m.SubOrFunctionStatement.Identifier.ValueText,"start_line",Start(m).ToString(),"end_line",End(m).ToString(),"source_sha256",FileSha(p),"body_sha256",ShaText(m.ToString()),"status","REUSED_HISTORICAL_REFERENCE","execution_04A29","NOT_RUN","scientific_status","NOT_GOLDEN","culture_options","Exact literals/options and expected values remain in pinned fixture and file-local helpers; no new fixture or culture override executed"));
            }
        }
        Csv("existing-graphics-test-fixtures-v1.csv",tests);
        var cases=new List<object>();
        foreach(var group in files.GroupBy(f=>f["family"]))
        {
            var family=group.Key; var coreNames=CoreName(family).Split(';',StringSplitOptions.RemoveEmptyEntries);
            var related=tests.Where(t=>coreNames.Any(c=>Path.GetFileName(t["path"])==c+"Tests.vb")).ToArray();
            var representative=related.Where(t=>Regex.IsMatch(t["test_name"],"Tie|Missing|Constant|Singleton|Culture|Order|Stage|Cop|Edge|Zero|Bound|Basic|Coordinates|Exact",RegexOptions.IgnoreCase)).Take(6).ToArray();
            if(representative.Length==0)representative=related.Take(2).ToArray();
            var sources=members.Where(m=>m["family"]==family && m["kind"] is "FunctionBlock" or "SubBlock" or "ConstructorBlock").ToArray();
            cases.Add(new { case_id="G29-"+Regex.Replace(family,"[^A-Za-z0-9]",""), family, windows_source_members=sources.Select(m=>m["member_id"]).ToArray(), calculation_ownership=CoreName(family)==""?"Existing Core result APIs and remaining operations per member inventory":CoreName(family), common_ownership=CommonName(family), host_free_evidence=representative.Length>0?"HOST_FREE_SOURCE_REFERENCE":"PROPOSED_NOT_RUN", existing_fixture_ids=representative.Select(t=>t["fixture_id"]).ToArray(), fixture_inputs_options_culture="Use exact pinned fixture bodies/file-local helper inputs; literal/option/expected values not re-evaluated in 04A29. Invariant and sk-SK contrasts proposed where future case requires culture.", output_shape_and_order=Shape(family), proposed_additional_cases=ProposedCases(family), native_status="NATIVE_NOT_RUN", manual_status="NOT_RUN", scientific_status="NOT_GOLDEN", future_owners=Batches(group.First()["path"],"casebook"), native_owner_batches=new[]{"04A.45","04A.46","04A.47","04A.48"}, neutral_spec=NeutralPlan(family), source_behavior_risk=FamilyRisk(family), acceptance="Compare exact dimensions/order/labels/masks and source arrays before/after future extraction. Existing same-Core fixtures remain compatibility-only. Native series/coordinates/appearance require separate authorized capture; no golden promotion." });
        }
        Write("graphics-family-casebook-v1.json",new { version=1, scope="Small source-backed family plan, not executed geometry capture or independent oracle", families=cases.Count, cases, test_fixture_index="existing-graphics-test-fixtures-v1.csv", actual_product_test_executions_04A29=0, native_captures=0, scientific_promotions=0 });
    }
    static string Shape(string family)=>family switch {
        "Histogram"=>"N x 2 Object midpoint/frequency; Gaussian overlay K x 2; optional quartile/whisker scalars and ordered outlier vector. No raw-bin switch.",
        "BoxPlot"=>"Group-ordered quartile/median/Q3 and strict outlier counts; five signed stacked components, custom whisker and XY median/errorbar channels.",
        "ViolinPlot"=>"Per-group evaluation grid/density/scaled half-width vectors; source-ordered Observations; summary scalars. Native polygon right side forward/left reverse.",
        "SankeyPlot"=>"Ordered nodes/links with stage IDs, typed color keys, first appearance and normalized endpoints; explicit-size cubic control points/rectangles/text layout remain proposal.",
        "PcaPlot"=>"First 2/3 score/loading columns, retained RowIds, optional exact-length labels/groups; biplot score/loading vectors at c=0/.5/1, eigen-dependent scaling.",
        "KaplanMeierPlot"=>"Time by group survival/CI arrays and censor-marker arrays/counts; prepend(0,1), group-maximum truncation, separate censor/upper/lower series.",
        "CoxSurvivalPlot"=>"Equal-length supplied baseline time/survival vectors, empty exit, no local step/censor/CI expansion.",
        "SpcControlChart"=>"Sorted shared PointIndex axis; ten nullable channels per panel; aligned point/signal/exclusion labels; stage boundary Fraction and AlternativeText.",
        "PsmLovePlot"=>"Count rows in one ordinal axis; Before/AfterMatching/AfterWeighting NaN vectors, labels/YValues, threshold/maxX. Table (Count+1) x 7 with Nothing missing values.",
        "ROC"=>"Unique-cutoff arrays and (unique+1) sensitivity/1-specificity arrays; Hanley/McNeil and DeLong AUC/SE/CI; original-order tie midranks.",
        "NormalPlot"=>"n sorted data/normal-score/probability values; 2 x 2 reference endpoints; X=data, Y=normal score.",
        "XYZscatter" or "3D wire objects"=>"x/y/z aligned raw order; two projected coordinate vectors, cage/grid/ticks and sentinel segment breaks; wire mesh rings/meridians.",
        "ScatterPlotMatrix"=>"n rows x p normalized coordinates, p x p correlation/label cells and ordered regression segment arrays; missing policy unchanged.",
        "ChartExport" or "GifAnimator"=>"Native file/clipboard/encoded raster/frame state, not neutral scientific point output; actual encoder/bitmap details NOT_RUN.",
        _=>"Exact typed DTO arrays/properties and label/source order in portable-members-v1.csv plus per-member current declaration; host application/measurement is separate." };
    static string ProposedCases(string family)=>family switch {
        "Histogram" or "BoxPlot" or "ViolinPlot"=>"Unequal groups; exact edge and constant/singleton/empty/NaN/Infinity; FD/Scott/Doane/Sturges/default; mutation before/after; legacy quartiles/whiskers versus existing violin policy; invariant/sk-SK. 04A.32; PrettyBreaks option only04A.75.",
        "SankeyPlot"=>"Repeated typed categories across >=2 stages, missing middle category with connect-across option, unequal flows, first appearance, culture override collision, tiny chart/resize/failure. Core source fixtures existing; physical geometry04A.34; native04A.45-48.",
        "PcaPlot" or "FactorAnalysisPlot" or "CorrespondenceAnalysisPlot" or "ClusterAnalysisPlot"=>"Low rank, retained1/2/3 dimensions, duplicate/blank labels, physical RowIds vs local ordinals, biplot0/.5/1 and zero eigen, dendrogram cuts/orientation. 04A.37; no sign/row-ID/index correction.",
        "KaplanMeierPlot" or "CoxSurvivalPlot"=>"Events/censors at same time, no-censor/empty/edge strata, per-group maximum and nonzero origin, CI flags/alpha; distinguish Core step from supplied Cox line. 04A.36 with C13 unchanged; native04A.45-48.",
        "SpcControlChart"=>"Stage boundary first/last/singletons, sequence category vs label, excluded/missing/nonfinite points, all ten channels, signals/specifications/zones; accepted Common projections reference-only. 04A.34/.36.",
        "PsmLovePlot" or "AgreementPlot" or "MultipleComparisonPlot" or "CalibrationPlot"=>"All/some nonfinite, tied category names, missing threshold, zero reference, CI endpoints/order, equal ranges and repeated subject mode. 04A.34/.35; no new finite-mask semantics or candidate defect activation.",
        "XYZscatter" or "3D wire objects" or "ScatterPlotMatrix"=>"Zero spans, 1/2/3 dimensions, aspect/scales, rotation/inversion, label length mismatch, interpolation where actual source calls it, default cuts and sentinel segments. 04A.33; native draw lifetime04A.45-48.",
        "ChartExport" or "GifAnimator"=>"Source-supported PNG/JPEG/GIF/TIFF/BMP/EMF, clipboard retry/partial-read, overbudget BMP tiling, frame delays/loop/progress. Native04A.45-48; no file created or clipboard accessed.",
        _=>"Staged/repeated categories or unequal vector dimensions, missing/empty/tied data, endpoint/interval labels and source indices; defaults and copy semantics in exact Core fixtures; future04A.30/.34 explicit-size/spec seam." };
    static void Roadmap(List<Dictionary<string,string>> members)
    {
        var descriptions=new Dictionary<string,string>{
            ["04A.30"]="Opening multi-family extraction: plot-label/group normalization, portable ROC/Q-Q/box/histogram/scatter/XYZ/scaling seams. Reuse existing calculators; define immutable source/label axes before later specialization.",
            ["04A.31"]="ROC AUC/ties/SE/CI/coordinates and normal probabilities/reference lines. Preserve fitted orientation, defaults and original arrays; separate numerical defect work.",
            ["04A.32"]="Box/whisker/histogram/violin aggregates, jitter and chart-axis semantics. Preserve distinct legacy quartile/whisker policies; no raw-bin option activation.",
            ["04A.33"]="XYZ/scatter matrix/3D raw and projected transforms, ticks/cage/grid/object bounds, segment geometry and label axes; no perspective or new sign convention.",
            ["04A.34"]="Specialty chart geometry/labels: calibration, agreement, comparisons, SPC, PSM, wire and existing Core-backed families. Explicit measurement inputs; renderer-only exceptions retained.",
            ["04A.35"]="Comparison CI/error lengths, agreement reference/LoA/unity and Love plot/report spec reuse. Do not duplicate accepted Common Love projection.",
            ["04A.36"]="KM/Cox time/step/censor series projections and SPC annotations; keep existing C13 restrictions and dormant Cox residual UI.",
            ["04A.37"]="PCA/FA/cluster/CA/MCA score/loading/axis/label/geometry projections. Reuse Core result/dendrogram/private math; no new eigen/sign or row-ID correction."};
        // Official scope and dependencies are copied literally, never inferred from file families.
        var planLines=File.ReadAllLines("planning/BESHStatNG_CrossPlatform_MasterPlan.md");
        Write("graphics-roadmap-owner-map-v1.json",new { prospective_only=true, candidate_members="graphics-members-v1.csv proposed_batches; graphics-operations-v1.csv", packages=descriptions.Select(kv=> { var rs=members.Where(m=>m["proposed_batches"].Split(';').Contains(kv.Key)).ToArray(); return new { batch=kv.Key, official_row=planLines.Single(l=>l.StartsWith("| **"+kv.Key+"** |")), purpose=kv.Value, candidate_member_count=rs.Length, work_family_count=rs.Select(r=>r["family"]).Distinct().Count(), families=rs.Select(r=>r["family"]).Distinct().Order().ToArray(), operation_ids=rs.Select(r=>r["operation_id"]).ToArray(), acceptance="Source-preserving portable result/spec contract, exact cardinality/order/identity/default/copy/missing fixtures; unchanged native chart boundary and public compatibility APIs. Owner approval and verified merged main before each batch.", neutral_contract="Explicit immutable axes/series/labels/masks/annotation requests, stable source IDs separate from display ordinals, measured viewport scalars only; reuse existing Core/Common DTOs.", risk="Family/member casebook; native/scientific mismatches remain candidate-only. Count overlaps across batches; do not add as unique workload." }; }).ToArray(), windows_exceptions=members.Where(m=>m["ownership"] is "WINDOWS_NATIVE_ONLY" or "WINDOWS_BRIDGE_REQUIRED").Select(m=>m["member_id"]).ToArray(), planning_amendments_proposed=new[]{"04A.30 opening overlaps specialized04A.31-33: accept narrow contracts first and track completed operations by member ID, never duplicate existing Core calculators.","04A.34 umbrella overlaps04A.35-37: document prerequisite neutral types and native measurement seams before specialty extraction; retain literal official dependencies.","FA private Core duplicates require approved public/internal projection seam in04A.37; matching formulas are not new algorithm work.","Export/GDI/GIF and timer/native palette exceptions remain Windows; optional GIF timing policy is04A.34 only if scope owner accepts, not a portable image engine."}, defects=new[]{"C21-STACKED-ROWCOUNT-VALIDATION-01","C24-HISTORICAL-ORACLE-SCOPE-01","C25-LOVE-THRESHOLD-MISSING-01","C25-LOVE-AXIS-NOFINITE-01","C28 output/row-ID obligations","G29-NORMAL-INPUT-ALIAS","G29-ROC-REPEATED-COMPUTE","G29-CA-CAPTION-INDEX","G29-SYMMETRY-AXIS-CAPTION","G29-SCATTER-MARGIN-IGNORED"}.Select(id=>new { id,status="OPEN_CANDIDATE_ONLY_UNAPPLIED",owner="Scientific/input/output owner04A.39-04A.41; native graphics owner04A.45-04A.48",next_action="Reproduce exact pinned source behavior and independently decide correction under separate authorization; audit does not activate fix." }).ToArray(), histogram_enhancement=new { owner="04A.75",status="DEFERRED_NOT_ENABLED" }, original_controls="original-control-crosswalk.json; no release or discharge" });
    }
    static void OriginalControls()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText("docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"));
        var rs=doc.RootElement.EnumerateArray().ToArray();
        var relevant=rs.Where(r=>r.GetProperty("original_id").GetString() is string id && Regex.IsMatch(id,"UibyID|Ui11PCA|Ui21ControlCharts|Ui22MultivariateControlCharts|Ui20Propensity|Ui4Cox|Ui4KM|Ui0OneRefedit|Ui01|Ui3XYZ|Ui2Export",RegexOptions.IgnoreCase)).Select(r=>r.GetProperty("obligation_id").GetString()).ToArray();
        Write("original-control-crosswalk.json",new { source=Pin("docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json"), accepted_dispositions=Pin("docs/architecture/WindowsMigrationLedger/04A01-approved-dispositions.json"), source_reconciliation=Pin("docs/architecture/WindowsMigrationLedger/04A01-closeout-reconciliation.json"), original_control_population=2056, originally_qualified_full_contracts=1164, incomplete_controls=892, unresolved_wiring_edges=622, live_control_golden_blockers=802, approved_restriction_schedule=rs.Length, restriction_schedule_id_kinds=rs.GroupBy(r=>r.GetProperty("id_kind").GetString()!).ToDictionary(g=>g.Key,g=>g.Count()), graphics_relevant_obligation_ids=relevant, relevant_unique_count=relevant.Distinct().Count(), overlap="892 controls,622 edges and802 golden blockers overlap;802 is not added to1514. Relevant IDs are filtered lineage links, not a new obligation population or resolved call edges.", all_status="OPEN_RETAINED; zero native releases and zero original-control discharges", original_04A02_restrictions=Pin("docs/architecture/04A02/native-restriction-crosswalk-v1.json"), gates=new {P="NO",N1="NO GO",N2="NO",C2="UNMET"}, legacy_pending_forms="Original unsigned/pending forms are preserved byte-for-byte; subsequent owner acceptance in request and live plan supersedes progress only, never native/scientific restrictions." });
    }
}
