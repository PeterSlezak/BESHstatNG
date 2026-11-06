using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
static partial class Program
{
    static string Family(string path) => Path.GetFileNameWithoutExtension(path).Replace("Excel", "") switch {
        "3Dobjects"=>"3D wire objects", "ChartingFunc"=>"Shared axes and histogram bridges", "SymetryPlot"=>"Symmetry", var s=>s };
    static string[] Batches(string path,string name) => Path.GetFileNameWithoutExtension(path).Replace("Excel", "") switch {
        "ROC" or "NormalPlot"=>["04A.30","04A.31"],
        "BoxPlot" or "Histogram" or "ViolinPlot" or "CategoricalHistogram" or "ChartingFunc"=>["04A.30","04A.32"],
        "XYZscatter" or "ScatterPlotMatrix"=>["04A.30","04A.33"],
        "3Dobjects"=>["04A.33","04A.34"],
        "AgreementPlot" or "PsmLovePlot" or "MultipleComparisonPlot"=>["04A.34","04A.35"],
        "KaplanMeierPlot" or "CoxSurvivalPlot"=>["04A.36"],
        "SpcControlChart" when name.Contains("Stage")||name.Contains("Label")=>["04A.34","04A.36"],
        "SpcControlChart"=>["04A.34"],
        "PcaPlot" or "FactorAnalysisPlot" or "ClusterAnalysisPlot" or "CorrespondenceAnalysisPlot"=>name.Contains("Label")||name.Contains("Group")?["04A.30","04A.37"]:["04A.37"],
        "ChartExport"=>[], "GifAnimator" when name!="MsToCentiseconds"=>[],
        _=>name.Contains("Label")||name.Contains("Group")||name.Contains("Name")||name.Contains("Override")?["04A.30","04A.34"]:["04A.34"] };
    static readonly string[] HostTokens=["Worksheet","Workbook","As Chart","As Series","As Shape","As Axis","As Range",".Chart","As XYZscatter","ChartObjects","ChartObject","SeriesCollection","Shapes","PlotArea","ChartArea","XlChart","XlAxis","XlMarker","XlData","XlLegend","XlLine","Mso","ExcelDna","Application","Clipboard","Bitmap","Metafile","Graphics.FromImage","Image.","ImageCodec","EncoderParameter","PropertyItem","FileStream","BinaryWriter","File.","DllImport","IntPtr","Marshal.","WithEvents","Handles ","Thread.Sleep","ProgressBar"];
    static bool HasHost(string s)=>HostTokens.Any(t=>s.Contains(t,StringComparison.OrdinalIgnoreCase));
    static string[] Dependencies(string text)=>HostTokens.Where(t=>text.Contains(t,StringComparison.OrdinalIgnoreCase)).Concat(new[]{"CultureInfo","CurrentCulture","IsNumeric","CStr(","CDbl(","Timer","DoEvents","CoreServices.Errors"}.Where(t=>text.Contains(t,StringComparison.OrdinalIgnoreCase))).Distinct().ToArray();
    static string CoreName(string family)=>family switch {"Shared axes and histogram bridges"=>"ChartAxisScalingCore;HistogramBinningCore", "Histogram"=>"HistogramBinningCore", "BulletChart"=>"BulletChartCore", "CategoricalHistogram"=>"CategoricalHistogramCore", "ConvexHullPlot"=>"ConvexHullPlotCore", "CumulativeDistributionPlot"=>"CumulativeDistributionPlotCore", "DumbbellPlot"=>"DumbbellPlotCore", "KiteChart"=>"KiteChartCore", "LadderPlot"=>"LadderPlotCore", "PolarPlot"=>"PolarPlotCore", "SankeyPlot"=>"SankeyPlotCore", "ViolinPlot"=>"ViolinPlotCore", _=>""};
    static string CommonName(string family)=>family switch {"PsmLovePlot"=>"PsmLovePlotProjection", "SpcControlChart"=>"SpcUnivariateChartProjectionPolicy;SpcMultivariateChartProjectionPolicy", "KaplanMeierPlot" or "CoxSurvivalPlot"=>"SurvivalReportPolicy;SurvivalWorkflowPolicy", "AgreementPlot"=>"AgreementInputAxis;AgreementReportPlan", "PcaPlot" or "CorrespondenceAnalysisPlot"=>"MultivariateInput/Correspondence policies (input/report only)", "FactorAnalysisPlot"=>"MultivariateFactorProjection (report only)", "ClusterAnalysisPlot"=>"MultivariateClusteringProjection (report only)", _=>""};
    static string NeutralPlan(string family)=>family switch {
        "ROC"=>"Core cutoff/sensitivity/specificity/AUC/SE/CI with tie-safe original-order ranks; Common ordered series and diagonal/reference/annotation intent; Windows chart and text.",
        "NormalPlot"=>"Core normal-score probabilities and SPSS/OLS/R reference endpoints; Common labels and X=data,Y=normal-score orientation; native Excel scatter.",
        "BoxPlot"=>"Reuse Core DescriptiveStat/QuartilesComp; later extract sign-split stacks, whisker lookup and deterministic jitter without changing legacy method. Common group/spec provenance; native error bars/shapes.",
        "Histogram"=>"Existing Core bins/PrettyBreaks/Gaussian plus existing quartiles/Tukey helpers; remaining Core bounds/box segment coordinates, Common overlay/options/spec. Native primary/secondary axes and measured shape projection.",
        "Shared axes and histogram bridges"=>"ChartScaling and histogram/overlay are existing Core forwards. CHARTscale is Windows compatibility shape; Common axes/spec and palette intent; native scatter and timer fallback retained.",
        "3D wire objects"=>"Core raw sphere/ellipsoid vertices and bounds with explicit full diameter/radius convention and segment breaks; Common wire request; Windows owner projection/draw/series lifetime.",
        "XYZscatter"=>"Core normalized cube, rotation rows, shift/zoom, tick/grid/cage/cut-plane/object bounds; Common aligned labels/groups/segments; Windows Chart and interface/object registry lifetime.",
        "ScatterPlotMatrix"=>"Core cell-coordinate rescaling and regression endpoints reusing Core correlation/slope/intercept; Common matrix cells/row/variable labels and segment masks; native grid/series.",
        "PcaPlot"=>"Existing Core eigen/loadings/scores; remaining biplot scaling/union bounds Core, Common dimension/percent/group/retained-RowId label projection; native arrows, labels and XYZ renderer.",
        "FactorAnalysisPlot"=>"Reuse existing private Core trace/percentage/pattern-structure helpers via approved contract; Common loading/scree axis/labels; no eigen/sign recomputation. Windows charts/XYZ.",
        "CorrespondenceAnalysisPlot"=>"Core coordinates/contributions/PlotLabels already authoritative; Common CA/MCA row-column axis/series/percentage caption intent; Windows scatter/column and zero-reference rendering.",
        "ClusterAnalysisPlot"=>"Core CreateDendrogramLayout already authoritative; Common orientation/leaf labels/cut/axis payload; residual extents separate from native chart target/writes.",
        "KaplanMeierPlot"=>"Core KaplanMeierPlotData already supplies step/censor/CI arrays; Common immutable group series projection with origin prepend and per-group truncation; Windows auxiliary series/legend/native chart.",
        "CoxSurvivalPlot"=>"Core baseline time/survival fit output; Common ordered baseline curve/time-axis/title spec without inventing step/censor/CI behavior; Windows renderer. Residual UI remains dormant.",
        "SpcControlChart"=>"Accepted Common shared axis, ten channels, masks, stage fractions and label intents already authoritative. Remaining Core bounds/zone rectangles; Common reference/annotation intent; Windows blank VARIANT marshalling, measured plot area, series and cleanup.",
        "PsmLovePlot"=>"Accepted Common PsmLovePlotProjection already owns ordering/absolute SMD/masks/threshold/maxX/table. Remaining neutral annotation/reference spec only; Windows finite-series admission, label/threshold series and reversed Y axis.",
        "AgreementPlot"=>"Core GetPlotData/fit and paired provenance retained; Core affine/reference endpoints and equal-range padding candidate; Common bias/LoA/unity/subject-mean series spec; Windows chart/style.",
        "MultipleComparisonPlot"=>"Core estimates/CIs unchanged; Core horizontal error lengths/bounds, Common reverse comparison axis/title/suggested height/zero line; Windows custom errors and labels.",
        "CalibrationPlot"=>"Existing Core CalibrationBinSummary consumed unchanged; Common ordered observed-vs-predicted points and identity spec; native fixed [0,1] axes. No local bin/CI fit migration.",
        "Symmetry"=>"Core median/tail deviation pairing candidate preserving actual upper-X/lower-Y convention; Common axis caption decision remains deferred; Windows active sheet scatter.",
        "TheilSenPlot"=>"Core GetPlotData already provides fitted endpoints and cloned observations; Common scatter/fit-line specification; native GeneralScatterPlot and styling.",
        "ChartExport"=>"Windows-only clipboard/EMF/GDI+/encoders/files/budget pipeline, including narrow stride/header math; no statistical geometry extraction or portable export claim.",
        "GifAnimator"=>"Windows GDI+ frames, metadata, file and WinForms progress lifetime. Millisecond-to-centisecond request arithmetic is a narrow Common candidate only; no portable GIF engine.",
        "SankeyPlot"=>"Existing Core staged nodes/links/layout/order; Common culture-aware label/style/order and explicit measured-size layout; Core affine/cubic control points candidate. Windows freeforms/text, resize sink registry and cleanup.",
        "ViolinPlot"=>"Existing Core KDE/grid/halfwidths/summary/observations; Core jitter and affine/clamped polygon coordinates candidate; Common group/style/axis/summary intent; Windows measured freeform/point shapes.",
        "CategoricalHistogram"=>"Existing Core bins and per-group values; remaining comparative sign/axis nice-ceiling Core, Common mode/units/category style intent; Windows column/stacked/native enum application.",
        _=>"Existing "+CoreName(family)+" remains numerical authority. Common labels/category/group/source-index/axis/annotation/style spec; residual explicit-size coordinate projections may be Core candidates. Windows actual chart/series/Shapes, native enums, measured text and cleanup." };
    static string FamilyRisk(string family)=>family switch {
        "ROC"=>"Ties credit 0.5; terminal cutoff=max+1; explicit (1,1)/(0,0) endpoints; alpha default .05; unbounded CI; compute accumulates pAUC on reused instance. Preserve and defer correction.",
        "NormalPlot"=>"Constructor sorts/aliases caller array. Blom/Rankit/Van Der Waerden and SPSS/OLS/R are distinct; no blanket missing/finite guard; preserve orientation/reference output.",
        "Histogram" or "Shared axes and histogram bridges"=>"PrettyBreaks always active in current Core; FD/overlay quartiles may sort input. Bin result Nx2 midpoint/count, last edge inclusive. 04A.75 alone owns future raw-bin toggle. Legacy axes asymmetric padding/floor.",
        "BoxPlot"=>"Legacy SAS-CDF quartiles; bounded whisker scans differ from full Core ResolveTukeyWhiskers; repeated Calculate appends collection. Jagged aliases vs 2D column copies; no violin-style finite filtering.",
        "PcaPlot"=>"Biplot c=0/.5/1 with sqrt(eigen)*sqrt(n) scaling, zero eigen/NaN c risk; retained RowIds and exact-length supplied labels, missing-group literal collision; no sign/eigen correction.",
        "FactorAnalysisPlot"=>"Pattern endpoints vs pattern*structure percentages; Core private duplicates need contract exposure, not new algorithms. No FA sign/orientation or row-ID correction.",
        "CorrespondenceAnalysisPlot"=>"Current pct(2,1)/pct(1,1) caption indexing uses cumulative column and can fail for 2 axes; candidate-only. C17/C18 unlabelled input correction stays dormant.",
        "ClusterAnalysisPlot"=>"Core dendrogram leaf/cut/orientation authoritative; local ordinal/physical RowId ambiguity C19 retained; inverted cut/science review deferred.",
        "KaplanMeierPlot"=>"Prepends (0,1), truncates per group maximum, always allocates censor series; CI alpha .05 and auxiliary legend indices retained. C13 log-rank/science/native restrictions unchanged.",
        "CoxSurvivalPlot"=>"Input baseline arrays rendered as supplied line; no local step expansion/censor/CI. C13 native scope and original Cox residual checkbox disposition remain open.",
        "SpcControlChart"=>"Masks/point indices/stage order from accepted Common. Blank Object Nothing remains native. C21 stacked row-count and C24 historical diagnostic-scope candidates stay unapplied; annotation layout needs native capture.",
        "PsmLovePlot"=>"Plot ordinals are not physical RowIds. NaN chart/Nothing table retained; fallback threshold .1 and maxX floor .25. C25 missing-threshold/no-finite-axis and reentrant options candidates unchanged.",
        "SankeyPlot"=>"Core typed/Ordinal node keys and first appearance differ from CurrentCultureIgnoreCase visual overrides; normalized Y is downward. Late resize/Refresh failure can leave sink/partial layer.",
        "XYZscatter" or "3D wire objects"=>"Cube [-.5,.5], Y chart [0,2], fixed cage/cut faces, asymmetric shifts, degree/zoom mapping, break sentinel, scale ratios and aliasing retained. No perspective or new finite guards.",
        "ScatterPlotMatrix"=>"Constructor overwrites supplied margin with .1; constant-column division by zero, correlation triangle/p-value orientation and sentinel -100 retained; labels/source row alignment explicit.",
        "Symmetry"=>"Input sorted in place; actual X upper-tail/Y lower-tail distances disagree with captions. Candidate-only scientific/native decision before correction.",
        "CalibrationPlot"=>"Only NaN skipped; Infinity may reach Excel. No CI-errorbars although separate reporting/UDF payload can expose CIs. Preserve current point/identity behavior.",
        "ChartExport"=>"Borrowed clipboard EMF handle, retry/sleep state, partial files, Int32 stride/header overflow and GDI+ memory heuristic; Windows only.",
        "GifAnimator"=>"Stretch aspect policy, banker-rounded minimum1cs, decoder-vs-encoder lookup and DoEvents reentrancy; no cancellation or native parity established.",
        _=>"Keep current defaults, cardinality, category/first-occurrence order, clone/alias semantics, finite/tie/missing handling and culture exactly as the pinned member declaration/review describes; native appearance and series indexing remain unverified." };

    static void Report()
    {
        Inventory(); // Reconstruct AST, never execute product code.
        var files=CsvRead(Path.Combine(Out,"graphics-files-v1.csv")); var current=CsvRead(Path.Combine(Out,"graphics-members-raw-v1.csv"));
        var old=CsvRead("docs/architecture/WindowsMigrationLedger/symbols.csv").Where(r=>r["path"].StartsWith("BESHstatNG/src/Graphics/")).ToList();
        using(var decisions=System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText("scripts/windows-migration-audit/review-decisions.json")))
        {
            var ordered=decisions.RootElement.EnumerateArray().Select(d=>d.GetProperty("symbol_id").GetString()!).Where(id=>id.StartsWith("BESHstatNG/src/Graphics/")).ToArray();
            if(ordered.Length!=old.Count||!ordered.ToHashSet().SetEquals(old.Select(r=>r["symbol_id"])))throw new System.Exception("Original decision objects and accepted symbols Graphics population differ");
            var by=old.ToDictionary(r=>r["symbol_id"]);old=ordered.Select(id=>by[id]).ToList();
        }
        var oldById=old.ToDictionary(r=>r["symbol_id"]); var historicalByKey=old.GroupBy(r=>r["signature_key"]).ToDictionary(g=>g.Key,g=>g.ToList());
        var nodes=Trees.SelectMany(t=>t.members).ToDictionary(x=>x.Value["member_id"],x=>x.Key);
        var edges=CsvRead(Path.Combine(Out,"graphics-call-edges-v1.csv")); var portable=CsvRead(Path.Combine(Out,"portable-members-v1.csv"));
        var mappedOld=new Dictionary<string,string>(); var operationRows=new List<Dictionary<string,string>>();
        var suboperations=new List<Dictionary<string,string>>();
        foreach(var r in current)
        {
            var id=r["member_id"]; var n=nodes[id]; var body=n.ToString(); var path=r["path"]; var family=Family(path); var name=r["name"]; var sig=r["signature"];
            Dictionary<string,string>? h=oldById.GetValueOrDefault(id);
            if(h==null && historicalByKey.TryGetValue(r["signature_key"],out var options))
            {
                var matching=options.Where(o=>BodyCorresponds(body,o["body_sha256"]) && !mappedOld.ContainsKey(o["symbol_id"])).ToArray();
                if(matching.Length==1)h=matching[0]; else if(options.Count==1)h=options[0];
            }
            if(h!=null && mappedOld.ContainsKey(h["symbol_id"]))h=null;
            var identical=h!=null && BodyCorresponds(body,h["body_sha256"]);
            if(h!=null)mappedOld[h["symbol_id"]]=id;
            var state=n is VariableDeclaratorSyntax or EnumMemberDeclarationSyntax || n is PropertyStatementSyntax;
            var declarationBody=state&&n.Parent is FieldDeclarationSyntax?n.Parent.ToString():body;
            var host=HasHost(declarationBody); var deps=Dependencies(declarationBody); var oldDisposition=h?.GetValueOrDefault("disposition")??"";
            var coreCalls=Regex.Matches(body,@"\b(?:[A-Za-z]+Core)\.[A-Za-z][A-Za-z0-9_]*").Select(m=>m.Value).Distinct().ToArray();
            var commonCalls=Regex.Matches(body,@"\b(?:PsmLovePlotProjection|SpcUnivariateChartProjectionPolicy|SpcMultivariateChartProjectionPolicy|projection\.[A-Za-z][A-Za-z0-9_]*)\b").Select(m=>m.Value).Distinct().ToArray();
            if(family=="PolarPlot" && r["qualified_type"]=="BESHStatNG.PolarPlot")coreCalls=name=="Compute"?["PolarPlotCore.Compute"]:name==".ctor"?["PolarPlotCore..ctor"]:coreCalls;
            var math=body.Contains("Math.")||body.Contains("Array.Sort")||body.Contains("ReDim")||body.Contains(" / ")||body.Contains(" * ")||body.Contains("QuartilesComp")||body.Contains("ResolveTukeyWhiskers");
            string ownership;
            if(state)ownership=host?"WINDOWS_BRIDGE_REQUIRED":"COMMON_CANDIDATE";
            else if(coreCalls.Length>0 && !host && !math)ownership="CORE_EXISTING";
            else if(family=="PsmLovePlot" && name is "BuildPlotDataTable" or "BuildProjectedDataTable")ownership="COMMON_EXISTING";
            else if(family=="ChartExport")ownership="WINDOWS_NATIVE_ONLY";
            else if(family=="GifAnimator" && name!="MsToCentiseconds")ownership="WINDOWS_NATIVE_ONLY";
            else if(host && (math||oldDisposition.StartsWith("extract-")||coreCalls.Length>0||commonCalls.Length>0))ownership="SHARED_MIXED_SPLIT";
            else if(host)ownership="WINDOWS_NATIVE_ONLY";
            else if(oldDisposition=="extract-to-Core")ownership="CORE_CANDIDATE";
            else if(oldDisposition=="retain-Windows-host-only")ownership="WINDOWS_NATIVE_ONLY";
            else if(oldDisposition=="compatibility-facade-to-preserve")ownership="WINDOWS_BRIDGE_REQUIRED";
            else ownership="COMMON_CANDIDATE";
            if(name is "ChartScaling" or "HistogramBinsComputation" or "GaussOverlayComputation")ownership="CORE_EXISTING";
            if(family=="XYZscatter" && name is "get_gridlines" or "cut_planes" or "normalizedData" or "getnorm" or "tick_marks_data" or "tick_marks_prerequisites" or "get_rotations" or "cage_data" or "comp_zoom_inter" or "comp_x_shift_inter" or "comp_y_shift_inter" or "ProjectRawPoint" or "EnsureNonZeroRanges" or "RefreshScaleRatios" or "ValidateAxisMinMax")ownership="CORE_CANDIDATE";
            if(name is "DeterministicJitter" or "NormalizedXToChartX" or "NormalizedYToChartY" or "BuildStageCentres" or "NiceCeiling" or "ResolveComparativeAxisLimit" or "ResolveComparativeMajorUnit" or "GetRawBounds" or "transformData")ownership="CORE_CANDIDATE";
            if(family=="NormalPlot" && name is "compute" or "computeRefLIne")ownership="CORE_CANDIDATE";
            if(family=="ROC" && name is "compute" or "DeLongSE" or "MidRanks" or "lambda")ownership="CORE_CANDIDATE";
            if(family=="FactorAnalysisPlot" && name is "ColumnSumsOfSquares" or "PercentOfTotal" or "TotalVariance")ownership="CORE_EXISTING"; // private duplicate, not a second algorithm
            if(family=="SpcControlChart" && name.StartsWith("Marshal"))ownership="WINDOWS_BRIDGE_REQUIRED";
            if(n is ConstructorBlockSyntax emptyConstructor && emptyConstructor.Statements.Count==0)ownership="WINDOWS_NATIVE_ONLY";
            if(family=="PolarPlot" && name is "Compute" or ".ctor" && r["qualified_type"]=="BESHStatNG.PolarPlot")ownership="CORE_EXISTING";
            if(family=="PolarPlot" && state && sig.Contains("PolarPlotCore"))ownership="WINDOWS_BRIDGE_REQUIRED";
            if(family=="ChartExport" || family=="GifAnimator"&&name!="MsToCentiseconds")ownership="WINDOWS_NATIVE_ONLY";
            if(state&&(r["qualified_type"].Contains("Overlay")||sig.Contains("Overlay")||sig.Contains("EventSink")))ownership="WINDOWS_BRIDGE_REQUIRED";
            var batches=Batches(path,name); if(ownership is "CORE_EXISTING" or "COMMON_EXISTING" or "WINDOWS_NATIVE_ONLY")batches=[];
            var op=id+"#ownership";
            r["family"]=family; r["ownership"]=ownership; r["historical_member_id"]=h?.GetValueOrDefault("symbol_id")??"";
            r["review_basis"]=identical?"EXACT_BODY_REUSE_OF_ACCEPTED_MEMBER_REVIEW":h!=null?"CHANGED_BODY_ACCEPTED_LATER_EXTRACTION_AND_LIVE_SOURCE":state?"LIVE_AST_DECLARATION_STATE_REVIEW":"LIVE_BODY_ACCEPTED_LATER_EXTRACTION";
            r["historical_body_match"]=h==null?"NO_ORIGINAL_EXECUTABLE_ROW":h["body_sha256"]==r["body_sha256"]?"EXACT_BODY_BYTES":identical?"LF_CRLF_TRANSFORM_VERIFIED_AGAINST_ORIGINAL_BODY_SHA":"CHANGED_BODY";
            r["algorithm_or_geometry"]=identical?h!["rationale"]:state?"State declaration only: "+sig+". Preserve default, native types, alias/copy contract and its enclosing consumer; not a new statistical algorithm.":ChangedDescription(family,name,sig);
            r["dependencies"]=string.Join(';',deps); r["existing_contract"]=string.Join(';',coreCalls.Concat(commonCalls));
            if(family=="FactorAnalysisPlot"&&name is "ColumnSumsOfSquares" or "PercentOfTotal" or "TotalVariance")r["existing_contract"]="BESHStat.Core/src/RegModels/Multivariate/FactorAnalysis.vb::"+name+" (private existing numerical operation; public projection/exposure not yet approved)";
            r["contract_gap"]=NeutralPlan(family); r["behavior_to_preserve"]=identical?h!["risk"]:FamilyRisk(family);
            r["old_operation_id"]=h?.GetValueOrDefault("candidate_operation_id")??"";
            r["operation_id"]=op; r["proposed_batches"]=string.Join(';',batches); r["host_owner"]="Windows Excel renderer/lifetime; native capture 04A.45-04A.48";
            r["responsible_owner"]=ownership.StartsWith("CORE")?"Core graphics owner":ownership.StartsWith("COMMON")?"Common plot/spec owner":"Windows graphics owner with Core/Common split review";
            r["test_reference"]=h?.GetValueOrDefault("proposed_test")??"casebook family "+family+"; source-backed proposal only";
            r["risk"]=FamilyRisk(family); r["side_effects"]=state?"Declaration/initializer: see signature; mutable storage remains with containing type until approved split.":identical?h!["runtime_boundary"]:"Current source and accepted later batch preserve calls, native state and ordering; NOT_RUN native.";
            r["suboperation_classifications"]=ownership=="SHARED_MIXED_SPLIT"?"CORE_EXISTING: pinned calculator calls if present; CORE_CANDIDATE: explicit arithmetic/coordinates detached from native measures; COMMON_CANDIDATE: labels/order/options/spec; WINDOWS_NATIVE_ONLY: COM/chart/series/Shapes/native measurement/cleanup. See operation_id statement ranges.":ownership+": "+(state?"typed state/default, no independent numerical execution":"whole described operation; wrapper API ownership remains Windows");
            r["shipping_status"]="COMPILED; STATIC_REFERENCE_ONLY; not executed; absent callers are not proof of dead code";
            var rangeRows=n.DescendantNodes().OfType<Microsoft.CodeAnalysis.VisualBasic.Syntax.StatementSyntax>().Where(s=>s.Parent==n || s.Parent is Microsoft.CodeAnalysis.VisualBasic.Syntax.MultiLineIfBlockSyntax || s.Parent is Microsoft.CodeAnalysis.VisualBasic.Syntax.ForBlockSyntax).Select(s=>new { start=Start(s),end=End(s),text=Norm(s.ToString()) }).Where(s=>s.text.Length<450).ToArray();
            // Evidence is per body, not a file-level guess. Exact AST source spans resolve operations without duplicating the corpus.
            operationRows.Add(Row("operation_id",op,"member_id",id,"source_start",r["start_line"],"source_end",r["end_line"],"ownership",ownership,"split_boundary",r["suboperation_classifications"],"core_call_evidence",string.Join(';',coreCalls),"common_call_evidence",string.Join(';',commonCalls),"host_evidence",string.Join(';',deps),"geometry_statement_lines",string.Join(';',rangeRows.Where(s=>s.text.Contains("Math.")||s.text.Contains(" / ")||s.text.Contains(" * ")||s.text.Contains("Array.Sort")).Select(s=>s.start+":"+s.end)),"proposed_batches",string.Join(';',batches),"source_sha256",r["source_sha256"]));
            if(ownership=="SHARED_MIXED_SPLIT")foreach(var statement in n.DescendantNodes().Where(s=>s is AssignmentStatementSyntax or LocalDeclarationStatementSyntax or ReturnStatementSyntax or ExpressionStatementSyntax))
            {
                var text=Norm(statement.ToString());var context=string.Join(" > ",statement.Ancestors().TakeWhile(a=>a!=n).OfType<WithBlockSyntax>().Reverse().Select(w=>Norm(w.WithStatement.Expression.ToString())));var native=HasHost(text+" "+context);var numeric=text.Contains("Math.")||text.Contains(" / ")||text.Contains(" * ")||text.Contains("Array.Sort")||text.Contains(" ^ ");
                var subowner=native?(numeric?"SHARED_MIXED_SPLIT":"WINDOWS_NATIVE_ONLY"):numeric?"CORE_CANDIDATE":"COMMON_CANDIDATE";
                suboperations.Add(Row("suboperation_id",id+"#statement@"+statement.Span.Start,"member_id",id,"start_line",Start(statement).ToString(),"end_line",End(statement).ToString(),"ownership",subowner,"source_evidence",text.Length>400?text[..400]+" [source range continues]":text,"with_context",context,"boundary",subowner=="SHARED_MIXED_SPLIT"?"Detach scalar coordinate arithmetic with explicit measured viewport inputs; retain native assignment/call in Windows.":subowner=="CORE_CANDIDATE"?"Coordinate/interval/axis arithmetic only; exact statement order and input/output dimensions preserved.":subowner=="WINDOWS_NATIVE_ONLY"?"Native object measurement/mutation/lifetime remains Windows; common label/series intent can be supplied as detached values.":"Local spec/label/order/option preparation candidate within mixed method; context includes native consumer, not an executable independence claim.","confidence","SOURCE_INFERRED; inspected accepted per-member purpose refines syntax evidence","source_sha256",r["source_sha256"]));
            }
        }
        Csv("graphics-members-v1.csv",current); Csv("graphics-operations-v1.csv",operationRows);Csv("graphics-suboperations-v1.csv",suboperations);
        Reconcile(old,current,mappedOld,portable);
        var primIds=current.Select(r=>r["member_id"]).ToHashSet(); var all=current.Concat(portable).ToArray();
        foreach(var e in edges)
        {
            var expr=e["target_expression"].Replace("Global.","");
            var simple=expr.StartsWith("New ")?".ctor":expr.Split('.').Last();
            var receiver=expr.StartsWith("New ")?expr[4..].Split('.').Last():expr.Contains('.')?expr.Split('.')[^2]:"";
            var targets=all.Where(r=>r["name"].Equals(simple,StringComparison.OrdinalIgnoreCase)&&(receiver==""?r["path"]==e["caller_path"]:r["qualified_type"].Split('.').Last().Equals(receiver,StringComparison.OrdinalIgnoreCase))).ToArray();
            e["callee_id"]=targets.Length==1?targets[0]["member_id"]:targets.Length>1?"OVERLOAD_OR_RECEIVER_UNRESOLVED":"EXTERNAL_OR_UNRESOLVED";
            e["candidate_callee_ids"]=string.Join(';',targets.Select(r=>r["member_id"])); e["resolution"]=targets.Length==1?"UNIQUE_SYNTAX_NAME_TARGET_NOT_SEMANTIC_BINDING":"STATIC_EXPRESSION_ONLY";
            e["host_crossing"]=HasHost(expr)?"NATIVE_EXPRESSION":targets.Any(r=>r["path"].StartsWith("BESHStat.Core/"))?"WINDOWS_TO_CORE_OR_CORE_INTERNAL":targets.Any(r=>r["path"].StartsWith("BESHStat.Excel.Common/"))?"COMMON_PROJECTION":"NO_PROVEN_HOST_CROSSING";
        }
        Csv("graphics-call-edges-v1.csv",edges); ExternalReferences(current);
        foreach(var r in current)
        {
            var incoming=edges.Where(e=>e["candidate_callee_ids"].Split(';').Contains(r["member_id"])).ToArray();
            r["caller_edge_ids"]=string.Join(';',incoming.Select(e=>e["edge_id"]));r["caller_confidence"]="SOURCE_INFERRED; syntax-name candidates only, not semantic binding or execution";
            r["reachability"]=incoming.Length>0||r["signature"].Contains("Handles ")?"STATIC_REFERENCE_ONLY":"UNRESOLVED_STATIC_REACHABILITY";
            r["external_callers"]="graphics-external-references-v1.csv candidate_graphics_paths includes this path; constructor/type/extension receiver references do not bind every member in file.";
        }
        Csv("graphics-members-v1.csv",current);
        foreach(var f in files) { f["family"]=Family(f["path"]);f["host_dependencies"]=string.Join(';',Dependencies(File.ReadAllText(f["path"])));f["neutral_plan"]=NeutralPlan(f["family"]); }
        Csv("graphics-files-v1.csv",files);
        ExistingCrosswalk(files,portable,current); Casebook(files,current); Roadmap(current); OriginalControls(); AuthoritiesAndRisks(files,current);
        Write("audit-summary.json",new { files=files.Count, text_lines=files.Sum(r=>int.Parse(r["text_lines"])), members=current.Count, census_policy=current.GroupBy(r=>r["census_policy"]).ToDictionary(g=>g.Key,g=>g.Count()), families=files.Select(f=>f["family"]).Distinct().Count(), ownership=current.GroupBy(r=>r["ownership"]).ToDictionary(g=>g.Key,g=>g.Count()), unknown_blocked=current.Where(r=>r["ownership"]=="UNKNOWN_BLOCKED").Select(r=>r["member_id"]).ToArray(), classification_scope="Static ownership proposal, exact-body accepted review reuse plus current AST state/body review. No semantic call binding, execution or equivalence inferred from counts.", source_profile="38 unconditional Compile Include items; no primary #If/Partial; no direct noncompiled .vb or generated primary source", index_empty=Git("diff","--cached","--name-only")=="" });
        ReconciliationReadme(current,edges);
        Console.WriteLine("Reports assembled from frozen source and immutable 04A01 decisions.");
    }
    static bool BodyCorresponds(string current,string historicalSha)=>ShaText(current)==historicalSha||ShaText(current.Replace("\r\n","\n").Replace("\n","\r\n"))==historicalSha||ShaText(current.Replace("\r\n","\n"))==historicalSha;
    static string ChangedDescription(string family,string name,string signature)=>family switch {
        "KaplanMeierPlot"=>"04A.13 accepted payload-overload seam: "+signature+". Analysis overload obtains existing Core GetKaplanMeierPlotData, payload overload retains step/origin/truncation/censor/CI/native series body; compatibility extension stays Windows.",
        "PsmLovePlot"=>"04A.25 accepted projection seam: "+signature+". Build methods consume PsmLovePlotProjection.BuildDataTable; chart methods consume ordered copied Y/absolute-SMD/labels/threshold/MaximumX. Native tail and finite-series helper unchanged; no new SMD algorithm.",
        "SpcControlChart"=>"04A.22 accepted projection seam: "+signature+". AddCharts delegates/project-validates result identity and axis options; MarshalAxis/Panel/Categories/Values map accepted Common axis and ten nullable channels to native arrays/Nothing. Labels/stage annotations consume Common intents; Windows rendering/lifetime retained.",
        _=>"Live member "+signature+"; per-body AST dependencies/operation rows and original source-linked family contract reviewed; no runtime equivalence assertion." };
}
