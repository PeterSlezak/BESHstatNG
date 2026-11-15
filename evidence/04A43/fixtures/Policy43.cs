using System;using System.Collections.Generic;using System.IO;using System.Web.Script.Serialization;using BESHStatNG.ExcelCommon;
class Policy43 {
 static void Main(string[] a){try{Run(a);}catch(Exception e){Console.Error.WriteLine(e);Environment.ExitCode=1;}}
 static void Run(string[] args){var rows=new List<object>();var d=new UdfModelHandleDescriptor("LM:simulation-A",UdfHandleFamily.LM,UdfHandleState.Unobserved,1,UdfHandleScopeEvidence.VerifiedByHost,10,UdfHandleScopeEvidence.VerifiedByHost);
  Action<string,UdfHandleValidity> v=(id,value)=>rows.Add(new{id,kind="VALIDITY",actual=value.ToString()});
  v("blank",UdfModelHandlePolicy.Validity(null,UdfHandleFamily.LM));v("same-scope",UdfModelHandlePolicy.Validity(d,UdfHandleFamily.LM,true,1,10));v("wrong-session",UdfModelHandlePolicy.Validity(d,UdfHandleFamily.LM,true,2,10));v("wrong-workbook",UdfModelHandlePolicy.Validity(d,UdfHandleFamily.LM,true,1,11));v("not-found",UdfModelHandlePolicy.Validity(d,UdfHandleFamily.LM,false));v("removed",UdfModelHandlePolicy.Validity(d.WithState(UdfHandleState.Removed),UdfHandleFamily.LM,false));
  var unknown=new UdfModelHandleDescriptor("arbitrary stored key",UdfHandleFamily.LM,UdfHandleState.Unobserved,1,UdfHandleScopeEvidence.NotEstablished,10,UdfHandleScopeEvidence.NotEstablished);
  v("unestablished-scope",UdfModelHandlePolicy.Validity(unknown,UdfHandleFamily.LM,true,2,11));
  Action<string,UdfHandleLifecycleDecision> p=(id,value)=>rows.Add(new{id,kind="POLICY_ONLY_PLAN",actual=value.Action.ToString(),nextState=value.NextState.ToString(),performsStorage=value.PerformsStorage});
  p("unknown-workbook-close",UdfModelHandlePolicy.Plan(unknown,UdfHandleEvent.WorkbookClose,10,true));p("verified-simulated-close",UdfModelHandlePolicy.Plan(d,UdfHandleEvent.WorkbookClose,10,true));p("workbook-switch",UdfModelHandlePolicy.Plan(d,UdfHandleEvent.WorkbookSwitch,11,true));p("repeat-fit",UdfModelHandlePolicy.Plan(d,UdfHandleEvent.RepeatedFit));
  rows.Add(new{id="reference-good",kind="RELATIONAL_REFERENCE",actual=UdfModelHandlePolicy.ReferenceMatches(d,"lm:SIMULATION-a",UdfHandleFamily.LM,true)});
  rows.Add(new{id="reference-alias-substitution",kind="RELATIONAL_REFERENCE",actual=UdfModelHandlePolicy.ReferenceMatches(d,"LM:simulation-B",UdfHandleFamily.LM,true)});
  rows.Add(new{id="reference-wrong-cache-family",kind="RELATIONAL_REFERENCE",actual=UdfModelHandlePolicy.ReferenceMatches(d,"LM:simulation-A",UdfHandleFamily.PCA,true)});
  rows.Add(new{id="reference-wrong-object",kind="RELATIONAL_REFERENCE",actual=UdfModelHandlePolicy.ReferenceMatches(d,"LM:simulation-A",UdfHandleFamily.LM,false)});
  var s=new JavaScriptSerializer{MaxJsonLength=int.MaxValue};File.WriteAllText(args[0],s.Serialize(new{kind="DEFINED_POLICY_ONLY_COMPILED_COMMON_SIMULATION",rows,actualWindowsCallbacksInstalled=0,actualWorkbookAttribution="NOT_ESTABLISHED",liveExcel="NOT_RUN"}));Console.WriteLine("Actual compiled Common simulated policy cases "+rows.Count);
 }
}
