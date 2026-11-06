using System.Text.Json;
using Microsoft.CodeAnalysis;

static partial class Program
{
    sealed record LineageData(List<Dictionary<string,string>> Rows,List<Dictionary<string,string>> Targets,List<object> Proofs);
    static object AnchorEvidence(SyntaxNode member,string anchor)
    {
        var node=member.DescendantNodesAndSelf().Where(n=>Norm(n.ToString()).Contains(Norm(anchor))).OrderBy(n=>n.Span.Length).First();
        return new {start_line=Start(node),end_line=End(node),span_start=node.Span.Start,span_length=node.Span.Length,anchor,matched_text=Norm(node.ToString()),node_sha256=ShaText(node.ToString())};
    }
    static bool FamilyPermits(string historicalPath,string targetPath)=>historicalPath switch {
        WinKm=>targetPath==WinKm||targetPath=="BESHStat.Core/src/StatTests/Survival.vb"||targetPath==CommonSurvival,
        WinPsm=>targetPath==WinPsm||targetPath==CommonPsm,
        WinSpc=>targetPath==WinSpc||targetPath==CommonSpc,
        _=>false };
    static List<string> CheckExplicitSpec(MigrationSpec spec)
    {
        var errors=new List<string>();
        if(string.IsNullOrWhiteSpace(spec.Batch)||string.IsNullOrWhiteSpace(spec.Commit)||string.IsNullOrWhiteSpace(spec.Scope)||string.IsNullOrWhiteSpace(spec.OldAnchor)||spec.Targets.Length==0)errors.Add("non-exact lineage requires explicit source/batch/operation evidence");
        foreach(var target in spec.Targets)
        {
            if(!FamilyPermits(spec.OldPath,target.Path))errors.Add("unrelated family target");
            if(target.Type==""||target.Anchor=="")errors.Add("target needs qualified type and operation anchor");
            if(target.Name==".ctor"&&(target.Type==""||target.Type.EndsWith(".*")||target.Anchor==""))errors.Add("generic constructor selection forbidden");
        }
        return errors;
    }
    static LineageData BuildLineageV2()
    {
        var oldRows=CsvRead(Path.Combine(Out,"04A01-graphics-member-crosswalk-v1.csv"));
        var historical=CsvRead("docs/architecture/WindowsMigrationLedger/symbols.csv").Where(r=>r["path"].StartsWith("BESHstatNG/src/Graphics/")).ToDictionary(r=>r["symbol_id"]);
        var specs=MigrationSpecs().ToDictionary(s=>s.Ordinal);
        var rows=new List<Dictionary<string,string>>();var targets=new Dictionary<string,Dictionary<string,string>>();var proofs=new List<object>();
        foreach(var original in oldRows)
        {
            var row=new Dictionary<string,string>(original);var ordinal=int.Parse(row["historical_ordinal"]);var id=row["historical_member_id"];
            if(row["category"]=="UNCHANGED_MATCHED_SOURCE"){rows.Add(row);continue;}
            if(!specs.TryGetValue(ordinal,out var spec))
            {
                row["category"]="HISTORICAL_UNRESOLVED";row["current_member_ids"]="";row["portable_split_ids"]="";
                row["reason"]="No explicit reviewed historical-ID/operation recipe. No family/name/constructor fallback is permitted.";
                row["next_action"]="Windows graphics lineage owner must inspect and add source-backed recipe before extraction.";rows.Add(row);continue;
            }
            var h=historical[id];if(h["path"]!=spec.OldPath||h["name"]!=spec.OldName)throw new Exception("Historical ID/operation recipe mismatch at ordinal "+ordinal);
            var errors=CheckExplicitSpec(spec);if(errors.Count>0)throw new Exception(string.Join(';',errors));
            var oldNode=HistoricalMember(h);if(!Norm(oldNode.ToString()).Contains(Norm(spec.OldAnchor)))throw new Exception("Historical operation anchor absent at "+ordinal);
            var commit=Git("rev-parse",spec.Commit);Git("merge-base","--is-ancestor",commit,"HEAD");
            var operations=new List<string>();var portableOperations=new List<string>();var contexts=new List<string>();var links=new List<object>();
            foreach(var selector in spec.Targets)
            {
                var target=SelectTarget(selector);RequireAcceptedBody(target,commit);
                var targetId=target.Row["member_id"];targets[targetId]=target.Row;
                if(selector.Role=="OPERATION")
                {
                    if(selector.Path.StartsWith("BESHstatNG/"))operations.Add(targetId);else portableOperations.Add(targetId);
                }
                else contexts.Add(targetId);
                links.Add(new {target_member_id=targetId,selector.Path,qualified_type=selector.Type,member_name=selector.Name,selector.Role,property_scope=selector.Property,linq_operator=selector.Invocation,signature_constraint=selector.Signature,source_sha256=target.Row["source_sha256"],body_sha256=target.Row["body_sha256"],operation=AnchorEvidence(target.Node,selector.Anchor),accepted_body_commit=commit,accepted_body_present=true});
            }
            if(ordinal==123)
            {
                var oldText=Norm(oldNode.ToString());var payload=SelectTarget(spec.Targets[1]);var currentText=Norm(payload.Node.ToString());
                var marker="Dim noGroups As Integer = plotData.GroupLabels.Length";
                if(oldText[oldText.IndexOf(marker)..]!=currentText[currentText.IndexOf(marker)..])throw new Exception("KM retained native tail correspondence failed");
            }
            row["category"]=spec.Category;row["current_member_ids"]=string.Join(';',operations.Distinct());row["portable_split_ids"]=string.Join(';',portableOperations.Distinct());row["context_existing_contract_ids"]=string.Join(';',contexts.Distinct());
            row["reason"]=spec.Scope;row["equivalence_limit"]=spec.Caveat;row["accepted_batch"]=spec.Batch;row["accepted_commit"]=commit;
            row["confidence"]=spec.Category=="UNCERTAIN_DIFFERENT_SOURCE"?"SOURCE_INFERRED":"PROVEN";
            row["confidence_scope"]="Source operation correspondence only; no runtime, whole-method, numeric-oracle or native equivalence asserted.";
            row["lineage_evidence_id"]="G29-LIN-"+ordinal;row["next_action"]=spec.Category=="UNCERTAIN_DIFFERENT_SOURCE"?"Future plot/spec owner must explicitly preserve or decide the stated standalone-helper contract limit before extraction; no fix authorized.":"Use exact operation anchors for downstream extraction; native/scientific gates remain unchanged.";
            proofs.Add(new {evidence_id=row["lineage_evidence_id"],historical_ordinal=ordinal,historical_member_id=id,historical_source_commit=HistoricalHead,historical_source_sha256=h["source_sha256"],historical_body_sha256=h["body_sha256"],historical_body_matches_pinned_hash=true,historical_operation=AnchorEvidence(oldNode,spec.OldAnchor),accepted_batch=spec.Batch,accepted_commit=commit,disposition=spec.Category,operation_scope=spec.Scope,equivalence_limit=spec.Caveat,confidence=row["confidence"],native_status="NATIVE_NOT_RUN",scientific_status="NOT_GOLDEN",target_links=links});
            rows.Add(row);
        }
        if(specs.Keys.Except(oldRows.Where(r=>r["category"]!="UNCHANGED_MATCHED_SOURCE").Select(r=>int.Parse(r["historical_ordinal"]))).Any())throw new Exception("Recipe attempts to override preserved exact row");
        return new(rows,targets.Values.OrderBy(r=>r["member_id"],StringComparer.Ordinal).ToList(),proofs);
    }
    static string CsvText(List<Dictionary<string,string>> rows)
    {
        var keys=rows.SelectMany(r=>r.Keys).Distinct().ToArray();string Q(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
        return string.Join(Environment.NewLine,new[]{string.Join(',',keys.Select(Q))}.Concat(rows.Select(r=>string.Join(',',keys.Select(k=>Q(r.GetValueOrDefault(k,"")))))))+Environment.NewLine;
    }
    static void WriteLineageV2()
    {
        var data=BuildLineageV2();
        Csv(V2Crosswalk,data.Rows);Csv("graphics-lineage-targets-v2.csv",data.Targets);
        Write("graphics-lineage-operation-evidence-v2.json",new {schema="04A29-explicit-lineage-v2",scope="Exactly the31non-exact historical operations, with unique qualified selectors, actual anchors, pinned historical bodies and verified accepted-batch bodies. Unrelated constructors and blanket file lists forbidden.",proofs=data.Proofs,supporting_contracts=new[]{Pin("BESHStat.Core/src/Survival/SurvivalContracts.vb"),Pin("BESHStat.Core/src/DM/NumericGuards.vb"),Pin("docs/architecture/batch-04A-13-owner-review.md"),Pin("docs/architecture/batch-04A-22-owner-review.md"),Pin("docs/architecture/batch-04A-25-owner-review.md")},km_contract="SurvivalContracts.KaplanMeierPlotData class38-49is the existing DTO, not a relocated estimator. SurvivalReportPolicy.Plot and KaplanMeierCurvePayload.ToCoreData are contextual GUI routes, not new operations moved from the old Plot member.",psm_predicate="AppInfrastructure.NumericGuards.IsFinite and Common PsmLovePlotProjection.Finite both exclude Double.NaN and both infinities; source anchors/pins are context, not a scientific oracle."});
        var prior=JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(File.ReadAllText(Path.Combine(Out,"04A01-graphics-reconciliation-v1.json")))!;
        var categories=new[]{"UNCHANGED_MATCHED_SOURCE","CURRENT_RELOCATED_OR_REFACTORED","CURRENT_NEW_MEMBER","CURRENT_REMOVED_OR_RENAMED","HISTORICAL_UNRESOLVED","SPLIT_OPERATION","UNCERTAIN_DIFFERENT_SOURCE"}.ToDictionary(c=>c,c=>data.Rows.Count(r=>r["category"]==c));
        Write(V2Reconciliation,new {schema="04A29-historical-reconciliation-v2",authoritative_crosswalk=V2Crosswalk,supersedes="04A01-graphics-member-crosswalk-v1.csv /04A01-graphics-reconciliation-v1.json; retained unchanged and explicitly non-authoritative",original_order=prior["original_order"],original_ordered_ids_sha256=prior["original_ordered_ids_sha256"],original_total=data.Rows.Count,exact_rows_retained_unchanged=data.Rows.Count(r=>r["category"]=="UNCHANGED_MATCHED_SOURCE"),non_exact_operations_reviewed=data.Proofs.Count,categories,historical_unresolved_ordinals=data.Rows.Where(r=>r["category"]=="HISTORICAL_UNRESOLVED").Select(r=>int.Parse(r["historical_ordinal"])).ToArray(),uncertain_different_source_ordinals=data.Rows.Where(r=>r["category"]=="UNCERTAIN_DIFFERENT_SOURCE").Select(r=>int.Parse(r["historical_ordinal"])).ToArray(),uncertainty="Ordinals171/172retain documented standalone-helper contract limits. Actual source operations are traced; whole-helper equivalence is explicitly unproved. This is an audit disposition, not a discharge or product correction.",current_distinct_members=1442,current_lineage_delta="current-member-lineage-delta-v1.csv unchanged; 14accepted new declarations and392newly enumerated pre-existing declarations",roadmap="graphics-roadmap-owner-map-v1.json unchanged; no new family/member work inflated",target_registry="graphics-lineage-targets-v2.csv",operation_evidence="graphics-lineage-operation-evidence-v2.json",original_authority=prior["decision_objects_pin"],current_file_coverage_percent=100,native_status="NATIVE_NOT_RUN",scientific_status="NOT_GOLDEN",native_releases=0,original_control_discharges=0});
        Write("lineage-authority.json",new {status="CANONICAL_V2",crosswalk=V2Crosswalk,reconciliation=V2Reconciliation,explanation="04A29-lineage-correction.md",superseded_crosswalk="04A01-graphics-member-crosswalk-v1.csv",superseded_reconciliation="04A01-graphics-reconciliation-v1.json",superseded_attempt="superseded/v1-attempt/",entry_receipt="lineage-correction-entry.json",versioning="v1 original bytes retained, never rewritten; only v2 is authoritative for historical lineage"});
        Console.WriteLine("Lineage v2: "+data.Rows.Count+"ordered rows; "+data.Proofs.Count+"explicit operation recipes; "+categories["UNCERTAIN_DIFFERENT_SOURCE"]+"limited whole-helper correspondences");
    }
    static List<string> VerifyLineageV2()
    {
        var errors=new List<string>();var expected=BuildLineageV2();var actual=CsvRead(Path.Combine(Out,V2Crosswalk));
        if(File.ReadAllText(Path.Combine(Out,V2Crosswalk))!=CsvText(expected.Rows))errors.Add("canonical v2 crosswalk is not reproducible from explicit source recipes");
        if(File.ReadAllText(Path.Combine(Out,"graphics-lineage-targets-v2.csv"))!=CsvText(expected.Targets))errors.Add("target registry source identity mismatch");
        using(var proof=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,"graphics-lineage-operation-evidence-v2.json"))))if(JsonSerializer.Serialize(proof.RootElement.GetProperty("proofs"),Json)!=JsonSerializer.Serialize(expected.Proofs,Json))errors.Add("operation evidence not reproducible from actual source/batch anchors");
        var prior=CsvRead(Path.Combine(Out,"04A01-graphics-member-crosswalk-v1.csv"));
        using(var summary=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,V2Reconciliation))))
        {
            if(summary.RootElement.GetProperty("original_total").GetInt32()!=actual.Count)errors.Add("summary population drift");
            foreach(var category in summary.RootElement.GetProperty("categories").EnumerateObject())if(category.Value.GetInt32()!=actual.Count(r=>r["category"]==category.Name))errors.Add("summary disposition count drift");
            if(summary.RootElement.GetProperty("authoritative_crosswalk").GetString()!=V2Crosswalk)errors.Add("ambiguous canonical lineage authority");
        }
        if(actual.Count!=prior.Count||!actual.Select(r=>r["historical_member_id"]).SequenceEqual(prior.Select(r=>r["historical_member_id"])))errors.Add("historical ordered disposition population drift");
        foreach(var row in prior.Where(r=>r["category"]=="UNCHANGED_MATCHED_SOURCE"))
        {
            var v2=actual.Single(r=>r["historical_member_id"]==row["historical_member_id"]);if(row.Any(kv=>v2.GetValueOrDefault(kv.Key)!=kv.Value))errors.Add("preserved exact row changed");
        }
        using var entry=JsonDocument.Parse(File.ReadAllText(Path.Combine(Out,"lineage-correction-entry.json")));
        foreach(var pin in entry.RootElement.GetProperty("files").EnumerateArray())
        {
            var path=pin.GetProperty("retained_path").GetString()!;if(!File.Exists(path)||FileSha(path)!=pin.GetProperty("sha256").GetString()||new FileInfo(path).Length!=pin.GetProperty("bytes").GetInt64())errors.Add("superseded attempt byte drift");
            var original=pin.GetProperty("original_path").GetString()!;if(original is "evidence/04A29/04A01-graphics-member-crosswalk-v1.csv" or "evidence/04A29/04A01-graphics-reconciliation-v1.json")if(FileSha(original)!=pin.GetProperty("sha256").GetString())errors.Add("v1 lineage evidence rewritten");
        }
        return errors;
    }
    static void LineageRegressionTests(Action<string,Action> test,Action<bool> expect)
    {
        var specs=MigrationSpecs();
        test("KM cannot point to SPC even when target ID exists",()=>{var s=specs.Single(s=>s.Ordinal==123);var bad=s with{Targets=[S("SpcUnivariateChartProjectionPolicy","Project","Return New SpcUnivariateChartProjection")]};expect(CheckExplicitSpec(bad).Contains("unrelated family target"));});
        test("SPC constructor targets require exact qualified type and operation anchor",()=>{var s=specs.Single(s=>s.Ordinal==814);expect(CheckExplicitSpec(s with{Targets=[new(WinSpc,"",".ctor","")]}).Contains("generic constructor selection forbidden"));});
        test("SPC old holder cannot resolve to unrelated AxisBounds constructor",()=>{var wanted=specs.Single(s=>s.Ordinal==814).Targets.Single();var rejected=false;try{SelectTarget(wanted with{Type=SpcType+".AxisBounds"});}catch{rejected=true;}expect(rejected);});
        test("non-exact lineage requires explicit accepted batch and source evidence",()=>expect(CheckExplicitSpec(specs[0] with{Batch="",Commit="",OldAnchor="",Scope=""}).Count>0));
        test("PSM identical VariableName lambdas retain ThenBy versus Labels Select identity",()=>{var order=SelectTarget(specs.Single(s=>s.Ordinal==155).Targets.Single());var labels=SelectTarget(specs.Single(s=>s.Ordinal==163).Targets.Single());expect(order.Row["member_id"]!=labels.Row["member_id"]&&InvokedOperator(order.Node)=="ThenBy"&&PropertyScope(labels.Node)=="Labels");});
        test("PSM threshold selector rejects clone-lambda ambiguity before choosing exact operation",()=>{var rejected=false;try{SelectTarget(P("lambda","r.Threshold",invocation:"Select"));}catch{rejected=true;}expect(rejected);expect(SelectTarget(specs.Single(s=>s.Ordinal==164).Targets.Single()).Node.ToString().Contains("Function(r) r.Threshold"));});
        test("SPC shared-axis/panel/value recipes have no arbitrary constructor lists",()=>{expect(specs.Single(s=>s.Ordinal==777).Targets.Count(t=>t.Name==".ctor")==3);expect(specs.Single(s=>s.Ordinal==778).Targets.Count(t=>t.Name==".ctor")==2);expect(specs.Single(s=>s.Ordinal==805).Targets.Count(t=>t.Name==".ctor")==1);expect(specs.Single(s=>s.Ordinal==814).Targets.Length==1);});
        test("all31non-exact historical IDs have exactly one explicit reviewed recipe",()=>{var old=CsvRead(Path.Combine(Out,"04A01-graphics-member-crosswalk-v1.csv"));expect(specs.Select(s=>s.Ordinal).Distinct().Count()==specs.Count&&specs.Select(s=>s.Ordinal).ToHashSet().SetEquals(old.Where(r=>r["category"]!="UNCHANGED_MATCHED_SOURCE").Select(r=>int.Parse(r["historical_ordinal"]))));});
        test("known PSM Nothing/standalone helper limits remain visible rather than discharged",()=>expect(specs.Where(s=>s.Category=="UNCERTAIN_DIFFERENT_SOURCE").Select(s=>s.Ordinal).SequenceEqual(new[]{171,172})));
        test("v2 source regeneration and1031preserved exact rows are reproducible",()=>expect(VerifyLineageV2().Count==0));
    }
}
