using System;using System.Collections.Generic;using System.IO;using System.Reflection;using System.Web.Script.Serialization;
static class Observe46 {
 static Assembly tests;static List<object> rows=new List<object>();
 static object Form(string owner){return Activator.CreateInstance(tests.GetType("BESHStatNG.Test.Harness46."+owner+"Projection46",true),true);}
 static object Control(object form,string name){return form.GetType().GetProperty(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetValue(form,null);}
 static void Main(string[] args){var lib=Path.GetFullPath(args[0]);AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var p=Path.Combine(lib,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};tests=Assembly.LoadFrom(Path.Combine(lib,"BESHStatNG.Test.dll"));
  var form=Form("Ui01SankeyPlot");
  foreach(var index in new[]{-1,0,1,2,3,99}){
   foreach(var method in new[]{"GetSelectedRibbonColorMode","GetSelectedNodeLabelMode","GetSelectedPercentageBasis"}){
    var name=method=="GetSelectedRibbonColorMode"?"cmbRibbonColorMode":method=="GetSelectedNodeLabelMode"?"cmbNodeLabelMode":"cmbPercentageBasis";
    var ctl=Control(form,name);ctl.GetType().GetProperty("SelectedIndex",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ctl,index,null);
    var actual=form.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null).ToString();
    var expected=method=="GetSelectedRibbonColorMode"?(index==1?"TargetNode":index==2?"Neutral":"SourceNode"):method=="GetSelectedNodeLabelMode"?(index==1?"NameAndValue":index==2?"NameAndPercentage":index==3?"NamePercentageAndValue":"NameOnly"):(index==1?"StageTotal":"ReferenceTotal");
    if(actual!=expected)throw new InvalidOperationException("source option mapping mismatch");rows.Add(new{method=method,index=index,actual=actual,expected=expected,outcome="MATCHED_SOURCE_SOFTWARE_ONLY"});
   }
  }
  var bullet=Form("Ui01BulletChart");var validate=bullet.GetType().GetMethod("ValidateOptionalNumericFormat",BindingFlags.NonPublic|BindingFlags.Static);
  foreach(var format in new[]{""," ","F2","Q2"}){string failure=null;try{validate.Invoke(null,new object[]{format,"synthetic format"});}catch(TargetInvocationException e){failure=e.InnerException.GetType().Name;}if((format=="Q2")!=(failure=="ArgumentException"))throw new InvalidOperationException("format rejection mismatch");rows.Add(new{method="ValidateOptionalNumericFormat",input=format,failure=failure,outcome="MATCHED_SOURCE_SOFTWARE_ONLY"});}
  File.WriteAllText(args[1],new JavaScriptSerializer().Serialize(new{kind="CURRENT_MAIN_PRE_EDIT_04A46_SOURCE_DERIVED_FAKE_CURRENT_OBSERVER_NOT_NATIVE",rows=rows}));
 }
}
