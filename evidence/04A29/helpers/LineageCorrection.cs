using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

static partial class Program
{
    const string V2Crosswalk="04A01-graphics-member-crosswalk-v2.csv";
    const string V2Reconciliation="04A01-graphics-reconciliation-v2.json";
    const string HistoricalHead="11fa5d99a040de2e62c75ec201c49852c1d6b289";
    const string WinKm="BESHstatNG/src/Graphics/KaplanMeierPlotExcel.vb";
    const string WinPsm="BESHstatNG/src/Graphics/PsmLovePlotExcel.vb";
    const string WinSpc="BESHstatNG/src/Graphics/SpcControlChartExcel.vb";
    const string CommonPsm="BESHStat.Excel.Common/src/Analysis/PsmLovePlotProjection.vb";
    const string CommonSpc="BESHStat.Excel.Common/src/Analysis/SpcUnivariateChartProjectionPolicy.vb";
    const string CommonSurvival="BESHStat.Excel.Common/src/Results/SurvivalReportPolicy.vb";
    const string PsmType="BESHStatNG.ExcelCommon.PsmLovePlotProjection";
    const string SpcType="BESHStatNG.graphics.SpcControlChartExcel";
    const string ProjectionType="BESHStatNG.ExcelCommon.SpcUnivariateChartProjection";
    const string PanelType="BESHStatNG.ExcelCommon.SpcPanelSeriesProjection";
    static string ActiveCrosswalk()=>File.Exists(Path.Combine(Out,V2Crosswalk))?Path.Combine(Out,V2Crosswalk):throw new InvalidOperationException("Canonical v2 lineage is required; run correct-lineage. Superseded v1 is never a validation fallback.");
    sealed record TargetSpec(string Path,string Type,string Name,string Anchor,string Role="OPERATION",string Signature="",string Property="",string Invocation="");
    sealed record MigrationSpec(int Ordinal,string OldPath,string OldName,string Batch,string Commit,string Category,string OldAnchor,string Scope,string Caveat,TargetSpec[] Targets);
    sealed record SourceTarget(Dictionary<string,string> Row,SyntaxNode Node);
    static readonly Dictionary<string,List<SourceTarget>> TargetCache=[];
    static readonly Dictionary<string,SyntaxNode> VersionCache=[];
    static List<SourceTarget> SourceTargets(string path)
    {
        if(TargetCache.TryGetValue(path,out var cached))return cached;
        var before=Trees.Count;var rows=Parse(path,"BESHStatNG",false);var tree=Trees[before];
        return TargetCache[path]=tree.members.Select(kv=>new SourceTarget(kv.Value,kv.Key)).ToList();
    }
    static string PropertyScope(SyntaxNode n)=>n.Ancestors().OfType<PropertyBlockSyntax>().FirstOrDefault()?.PropertyStatement.Identifier.ValueText??"";
    static string InvokedOperator(SyntaxNode n)=>n.Ancestors().OfType<InvocationExpressionSyntax>().Select(i=>i.Expression is MemberAccessExpressionSyntax a?a.Name.Identifier.ValueText:i.Expression.ToString()).FirstOrDefault()??"";
    static SourceTarget SelectTarget(TargetSpec spec)
    {
        var candidates=SourceTargets(spec.Path).Where(t=>t.Row["qualified_type"]==spec.Type&&t.Row["name"]==spec.Name&&(spec.Signature==""||t.Row["signature"].Contains(spec.Signature))&&(spec.Property==""||PropertyScope(t.Node)==spec.Property)&&(spec.Invocation==""||InvokedOperator(t.Node)==spec.Invocation)&&Norm(t.Node.ToString()).Contains(Norm(spec.Anchor))).ToArray();
        if(candidates.Length!=1)throw new Exception($"Explicit lineage selector resolved {candidates.Length}, not one: {spec.Path}::{spec.Type}.{spec.Name}, property={spec.Property}, operator={spec.Invocation}, anchor={spec.Anchor}");
        return candidates[0];
    }
    static SyntaxNode VersionRoot(string commit,string path)
    {
        var key=commit+":"+path;if(!VersionCache.TryGetValue(key,out var root))VersionCache[key]=root=VisualBasicSyntaxTree.ParseText(Git("show",key)).GetRoot();return root;
    }
    static SyntaxNode HistoricalMember(Dictionary<string,string> h)
    {
        var nodes=VersionRoot(HistoricalHead,h["path"]).DescendantNodes().Where(n=>n.Kind().ToString()==h["kind"]&&Start(n)==int.Parse(h["start_line"])&&TypeName(n,"BESHStatNG")==h["containing_type"]&&BodyCorresponds(n.ToString(),h["body_sha256"])).ToArray();
        if(nodes.Length!=1)throw new Exception("Historical body/source correspondence not unique: "+h["symbol_id"]);return nodes[0];
    }
    static void RequireAcceptedBody(SourceTarget target,string commit)
    {
        var candidates=VersionRoot(commit,target.Row["path"]).DescendantNodes().Where(n=>n.Kind().ToString()==target.Row["kind"]&&Norm(Header(n).ToString())==target.Row["signature"]&&BodyCorresponds(target.Node.ToString(),ShaText(n.ToString()))).ToArray();
        if(candidates.Length==0)throw new Exception("Target body not found in accepted batch source: "+target.Row["member_id"]);
        // Repeated identical lambdas are permitted here only as accepted-body presence proof;
        // unique current selection additionally requires qualified type/property/LINQ operator.
    }
    static TargetSpec W(string path,string name,string anchor,string signature="",string type="",string role="OPERATION")=>new(path,type==""?path==WinSpc?SpcType:path==WinPsm?"BESHStatNG.graphics.PsmLovePlotExcel":"BESHStatNG.graphics.KaplanMeierPlotExcel":type,name,anchor,role,signature);
    static TargetSpec P(string name,string anchor,string property="",string invocation="",string role="OPERATION")=>new(CommonPsm,PsmType,name,anchor,role,"",property,invocation);
    static TargetSpec S(string type,string name,string anchor,string role="OPERATION")=>new(CommonSpc,"BESHStatNG.ExcelCommon."+type,name,anchor,role);
    static List<MigrationSpec> MigrationSpecs()
    {
        var specs=new List<MigrationSpec>();
        void Add(int ordinal,string path,string oldName,string batch,string commit,string category,string oldAnchor,string scope,string caveat,params TargetSpec[] targets)=>specs.Add(new(ordinal,path,oldName,batch,commit,category,oldAnchor,scope,caveat,targets));
        const string split="SPLIT_OPERATION", relocated="CURRENT_RELOCATED_OR_REFACTORED", replaced="CURRENT_REMOVED_OR_RENAMED", uncertain="UNCERTAIN_DIFFERENT_SOURCE";
        const string km="b02480a",spc="519e875",psm="97d5461";
        Add(123,WinKm,"Plot","04A.13",km,split,"analysis.GetKaplanMeierPlotData(alpha)",
            "Existing Core payload preparation did not move from this historical member: it was already called there. The analysis overload became a forwarding seam; the payload overload contains the retained native renderer tail. Common snapshot/ToCoreData is an additional GUI route, not a new KM estimator or a relocated SPC operation.",
            "Old null-worksheet check preceded Core preparation; current forwarding overload obtains Core payload before the payload overload checks worksheet. No whole-method equivalence or new correction is asserted.",
            W(WinKm,"Plot","Plot(analysis.GetKaplanMeierPlotData(alpha)","analysis As survival.Survival_KM_LR"),
            W(WinKm,"Plot","probability(0) = 1","plotData As survival.KaplanMeierPlotData"),
            new("BESHStat.Core/src/StatTests/Survival.vb","BESHStatNG.survival.Survival_KM_LR","GetKaplanMeierPlotData","Me.KaplanMeierPlotDataPrep","EXISTING_CONTEXT"),
            new(CommonSurvival,"BESHStatNG.ExcelCommon.SurvivalReportPolicy","Plot","model.GetKaplanMeierPlotData(alpha)","ADDITIONAL_GUI_CONTEXT"),
            new(CommonSurvival,"BESHStatNG.ExcelCommon.KaplanMeierCurvePayload","ToCoreData","Return Clone(_data)","ADDITIONAL_GUI_CONTEXT"));
        Add(151,WinPsm,"BuildPlotDataTable","04A.25",psm,split,"OrderByDescending",
            "Table facade delegates via BuildProjectedDataTable; Common constructor owns filtering/ordered snapshot and BuildDataTable owns the seven headings, ordinal cells, table missing-value conversion, threshold and flag.","Snapshot constructor also prepares chart-only bounds, which old table builder did not do; no whole-wrapper equivalence.",
            W(WinPsm,"BuildPlotDataTable","New PsmLovePlotProjection(rows)"),W(WinPsm,"BuildProjectedDataTable","projection.BuildDataTable()"),P(".ctor","ThenBy(Function(r) r.VariableName, StringComparer.OrdinalIgnoreCase)"),P("BuildDataTable","Dim table(Count, 6) As Object"));
        Add(152,WinPsm,"AddChart","04A.25",psm,split,"MaxFinite(xBefore.Concat(xAfterMatching).Concat(xAfterWeighting))",
            "Rows/sort, plot arrays, first-positive threshold and MaximumX moved to the exact Common projection; AddChart forwards and AddProjectedChart consumes it while retaining native chart/label/axis/threshold-series code.","Source responsibility correspondence, not native appearance or changed finite-data semantics.",
            W(WinPsm,"AddChart","AddProjectedChart(ws, New PsmLovePlotProjection(rows)"),W(WinPsm,"AddProjectedChart","Dim maxX = projection.MaximumX"),P(".ctor","value = Math.Max(Threshold * 1.25R, maxValue * 1.15R)"));
        foreach(var ordinal in new[]{153,156})Add(ordinal,WinPsm,"lambda","04A.25",psm,relocated,"r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)","Table/chart null-or-blank-name Where predicates consolidated into the Common constructor Where predicate; exact lambda text and LINQ operator match.","Two historical callbacks share one current predicate; no extra release/discharge.",P("lambda","r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)",invocation:"Where"));
        foreach(var ordinal in new[]{154,157})Add(ordinal,WinPsm,"lambda","04A.25",psm,relocated,"MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)","Descending sort key across before/matched/weighted absolute SMD is the same exact Common OrderByDescending lambda; input triple order retained.","Uses the non-null three-value call path; standalone helper Nothing caveat is ordinal171.",P("lambda","MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)",invocation:"OrderByDescending"));
        foreach(var ordinal in new[]{155,158})Add(ordinal,WinPsm,"lambda","04A.25",psm,relocated,"r.VariableName","Secondary VariableName key maps specifically to ThenBy with OrdinalIgnoreCase in its enclosing invocation, not to a similarly named label Select.","Stable ties retain incoming order; no culture normalization introduced.",P("lambda","r.VariableName",invocation:"ThenBy"));
        Add(159,WinPsm,"lambda","04A.25",psm,relocated,"CDbl(i)","One-based Enumerable.Range Y ordinal conversion maps to the exact YValues getter Select lambda.","Plot ordinals are not physical RowIds.",P("lambda","CDbl(i)",property:"YValues",invocation:"Select"));
        foreach(var entry in new[]{(160,"SmdBefore","Before"),(161,"SmdAfterMatching","AfterMatching"),(162,"SmdAfterWeighting","AfterWeighting")})Add(entry.Item1,WinPsm,"lambda","04A.25",psm,relocated,"AbsForChart(r."+entry.Item2+")","Specific "+entry.Item2+" X selector now calls ChartValue in "+entry.Item3+" getter; input column, absolute finite/NaN mask and row order preserved.","AbsForChart->ChartValue predicate/helper correspondence is separately anchored at ordinal169.",P("lambda","ChartValue(r."+entry.Item2+")",property:entry.Item3,invocation:"Select"));
        Add(163,WinPsm,"lambda","04A.25",psm,relocated,"r.VariableName","Label selector maps to Labels getter Select, distinct from the identically spelled ThenBy key.","Untrimmed valid names and sorted row association retained.",P("lambda","r.VariableName",property:"Labels",invocation:"Select"));
        Add(164,WinPsm,"lambda","04A.25",psm,relocated,"r.Threshold","Threshold field selector maps to constructor Select before FirstOrDefault; no row threshold aggregation.","The subsequent finite-positive predicate and fallback are separate operations.",P("lambda","Function(r) r.Threshold",invocation:"Select"));
        Add(165,WinPsm,"lambda","04A.25",psm,relocated,"AppInfrastructure.IsFinite(v) AndAlso v > 0","Constructor FirstOrDefault uses Finite(v) AndAlso v>0; finite predicate has the same NaN/infinity exclusions; first sorted positive value or default0 then0.1 remains.","No missing-threshold correction is authorized.",P("lambda","Finite(v) AndAlso v > 0",invocation:"FirstOrDefault"),P("Finite","Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)",role:"PREDICATE_CONTEXT"));
        Add(169,WinPsm,"AbsForChart","04A.25",psm,relocated,"Return Double.NaN","Common ChartValue returns finite absolute Double or NaN; exact Double finite predicate and input/output shape are pinned.","No SMD calculation moved here.",P("ChartValue","Return If(Finite(value), Math.Abs(value), Double.NaN)"),P("Finite","Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)",role:"PREDICATE_CONTEXT"));
        Add(170,WinPsm,"AbsOrMissing","04A.25",psm,relocated,"Return Nothing","Common TableValue returns Nothing Object for nonfinite and absolute finite Double for table cells.","Chart NaN and table Nothing remain distinct.",P("TableValue","If Not Finite(value) Then Return Nothing"),P("Finite","Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)",role:"PREDICATE_CONTEXT"));
        Add(171,WinPsm,"MaxFiniteAbs","04A.25",psm,uncertain,"If values Is Nothing Then Return maxValue","Finite absolute maximum loop is present in the specific Common MaxFiniteAbs helper, used by the same three-value OrderByDescending call.","Whole-helper correspondence is limited: historical explicit Nothing ParamArray returns NaN; new Common helper has no Nothing guard. Reachable non-null triple operation is traced; standalone Nothing equivalence remains unproved and is not discharged.",P("MaxFiniteAbs","For Each v In values"));
        Add(172,WinPsm,"MaxFinite","04A.25",psm,uncertain,"If values Is Nothing Then Return maxValue","Historical finite maximum over xBefore.Concat(xAfterMatching).Concat(xAfterWeighting) is now the exact constructor For Each over Before.Concat(AfterMatching).Concat(AfterWeighting), including NaN seed and finite max update before MaximumX.","No standalone MaxFinite API remains; old Nothing/enumerable contract and arbitrary signed input behavior are not established by a constructor operating on absolute projected vectors. Only this contextual loop correspondence is claimed.",P(".ctor","For Each v In Before.Concat(AfterMatching).Concat(AfterWeighting)"));
        Add(775,WinSpc,"AddCharts","04A.22",spc,split,"BuildSharedAxisContext(result, options)","Three-argument compatibility entry forwards to the four-argument AddCharts overload, which calls Common Project if absent and validates projection identity/options. Native chart creation/order/reverse cleanup stay in the four-argument body.","Projection validation is an accepted new seam, not whole-method runtime equivalence.",W(WinSpc,"AddCharts","Return AddCharts(worksheet, result, appearance, Nothing)","Optional appearance"),W(WinSpc,"AddCharts","Dim axisContext As SharedAxisContext = MarshalAxis(projection)","projection As SpcUnivariateChartProjection"),S("SpcUnivariateChartProjectionPolicy","Project","Return New SpcUnivariateChartProjection(result, useSequenceValues)","ENTRY_CONTEXT"));
        Add(776,WinSpc,"RenderPanel","04A.22",spc,replaced,"BuildPanelChartData(panel, axisContext)","Replacement RenderPanel accepts panelProjection, calls MarshalPanel and sends Common point/exclusion/signal intents to native label helpers. Chart type/series/bounds/native operations remain renderer-owned.","Signature replacement and data seam, not a new Core fit.",W(WinSpc,"RenderPanel","Dim panelData As PanelChartData = MarshalPanel(panelProjection)","panelProjection As SpcPanelSeriesProjection"),W(WinSpc,"MarshalPanel","Return New PanelChartData(points,"));
        Add(777,WinSpc,"BuildSharedAxisContext","04A.22",spc,relocated,"orderedIndices.Sort()","Union/first representative and sorted PointIndex order move to SpcUnivariateChartProjection.New; sequence-vs-label intent to SpcChartAxisPoint.New; native category boxing to MarshalChartCategories; MarshalAxis invokes only its exact SharedAxisContext holder.","Policy Project is an entry route, not evidence that every constructor in its file is related.",S("SpcUnivariateChartProjection",".ctor","indices.Sort()"),S("SpcChartAxisPoint",".ctor","Me.IsSequenceCategory = useSequence AndAlso point.SequenceValue.HasValue"),W(WinSpc,"MarshalChartCategories","categories(i) = category.Point.SequenceValue.Value"),W(WinSpc,"MarshalAxis","New SharedAxisContext(MarshalChartCategories(projection), projection)"),W(WinSpc,".ctor","Me.Projection = projection",type:SpcType+".SharedAxisContext"));
        Add(778,WinSpc,"BuildPanelChartData","04A.22",spc,relocated,"pointByIndex.TryGetValue(axisContext.PointIndices(i), point)","Dictionary alignment, eight base channels and signal/exclusion masks move to SpcPanelSeriesProjection.New. MarshalPanel copies aligned point references and ten channels; MarshalChartValues converts nullable samples to native Double/Nothing; the exact PanelChartData holder aliases those prepared arrays.","PanelChartData.New is linked only through the explicit Return New PanelChartData in MarshalPanel; unrelated constructors excluded.",S("SpcPanelSeriesProjection",".ctor","If Not byIndex.TryGetValue(axis(i).Point.PointIndex, point) Then Continue For"),W(WinSpc,"MarshalPanel","MarshalChartValues(projection.Channels(CInt(SpcChartChannel.Excluded)))"),W(WinSpc,"MarshalChartValues","If values(i).HasValue Then marshalled(i) = values(i).Value"),W(WinSpc,".ctor","Me.ExcludedValues = excludedValues",type:SpcType+".PanelChartData"));
        Add(788,WinSpc,"AddStageBoundaries","04A.22",spc,split,"StringComparison.OrdinalIgnoreCase","Adjacent stage comparison stays OrdinalIgnoreCase in the exact univariate projection constructor; SpcStageBoundaryIntent.New computes ordinal/count fraction and text; native AddStageBoundaries measures plot area and maps fractions to chart Shapes.","Measured geometry, catches and chart lifetime remain Windows.",S("SpcUnivariateChartProjection",".ctor","StringComparison.OrdinalIgnoreCase"),S("SpcStageBoundaryIntent",".ctor","Me.Fraction = CDbl(ordinal) / CDbl(count)"),W(WinSpc,"AddStageBoundaries","plotLeft + plotWidth * boundary.Fraction"));
        foreach(var entry in new[]{(789,"AddPointLabels","PointLabels","point.Label"),(790,"AddSignalLabels","SignalLabels","RuleText(point.SignalRuleNumbers)"),(791,"AddExclusionLabels","ExclusionLabels","If(point.ExclusionReason.Length = 0, \"Excluded\", point.ExclusionReason)")})
            Add(entry.Item1,WinSpc,entry.Item2,"04A.22",spc,replaced,"SetPointLabel(series,","Specific "+entry.Item3+" intents are built only for aligned finite points in SpcPanelSeriesProjection.New, with signal/exclusion guard as appropriate. AddLabel omits blank text; replacement native helper applies the same slot/text through SetPointLabel.","Whitespace omission was already performed by native SetPointLabel; no new finite mask or label text is inferred.",S("SpcPanelSeriesProjection",".ctor",entry.Item4),S("SpcPanelSeriesProjection","AddLabel","If Not String.IsNullOrWhiteSpace(text)"),W(WinSpc,entry.Item2,"For Each label In labels"));
        Add(804,WinSpc,"FormatRuleNumbers","04A.22",spc,relocated,"CultureInfo.InvariantCulture","Exact Common SpcPanelSeriesProjection.RuleText contains the same empty handling, invariant integer strings and R plus comma join.","No sorting or deduplication is introduced by this formatter.",S("SpcPanelSeriesProjection","RuleText","Return \"R\" & String.Join(\",\", parts)"));
        Add(805,WinSpc,"ChartValue","04A.22",spc,relocated,"If IsFinite(value) Then Return value","Common panel constructor stores only finite samples into initially Nothing nullable channels using its exact IsFinite predicate. MarshalChartValues boxes HasValue and leaves native Object slots Nothing otherwise, composing the former ChartValue behavior.","IsFinite is only the predicate; nullable preparation plus Windows marshalling is the full split. No unrelated holder constructor is a substitute.",S("SpcPanelSeriesProjection",".ctor","If IsFinite(values(c)) Then samples(c)(i) = values(c)"),S("SpcPanelSeriesProjection","IsFinite","Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)"),W(WinSpc,"MarshalChartValues","If values(i).HasValue Then marshalled(i) = values(i).Value"));
        Add(814,WinSpc,".ctor","04A.22",spc,replaced,"Me.StageIds = stageIds","Only SpcControlChartExcel.SharedAxisContext.New is the replacement holder: Categories is retained, PointIndices/StageIds arrays are replaced by a reference to the prepared projection containing ordered axis points/stages.","Different holder signature and fields; no array cloning/validation or whole-contract equivalence claimed. No PanelChartData, AxisBounds or Common policy constructors are targets.",W(WinSpc,".ctor","Me.Projection = projection",type:SpcType+".SharedAxisContext"));
        return specs.OrderBy(s=>s.Ordinal).ToList();
    }
}
