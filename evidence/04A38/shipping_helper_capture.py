"""Execution cross-check of whitelisted actual pre-edit Windows private helpers.

No exported body, function wizard, ExcelReference, XlCall, UI or COM is invoked.
Only managed scalar/array helper cases from the frozen corpus are dispatched.
No new calculations/expected values are introduced into the recorder.
"""
import json
import os
import subprocess
import sys
from preflight import HERE,ROOT,BASE,git,pin,save
from run import PRIVATE,m


def main():
    config,attempt=sys.argv[1:3]
    assert not git('diff','--name-only') and git('rev-parse','HEAD').decode().strip()==BASE
    tag='shipping-helpers-'+config+'-'+attempt
    private=PRIVATE/tag;private.mkdir(exist_ok=False)
    folder=HERE/'preedit/attempts'/tag;folder.mkdir(exist_ok=False)
    lines=(HERE/'preedit/frozen-v2/cases.jsonl').read_text(encoding='utf-8').splitlines()
    selected=[line for line in lines if json.loads(line)['target_kind']=='helper']
    assert all('handle_variant' not in json.loads(line) and 'transaction' not in json.loads(line) for line in selected)
    cases=folder/'cases.jsonl';cases.write_text('\n'.join(selected)+'\n',encoding='utf-8',newline='\n')
    owners=sorted({r['containing_type'] for r in json.loads((HERE/'entry-members.json').read_bytes())['members']})
    text=(HERE/'preedit/frozen-v2/Capture38.cs').read_text(encoding='utf-8')
    text=text.replace('public static class Capture38 {','public static class Capture38 {\n static readonly HashSet<string> ShippingOwners=new HashSet<string>(new string[]{'+','.join(json.dumps(v) for v in owners)+'},StringComparer.Ordinal);')
    text=text.replace('foreach(var type in Library.GetTypes())foreach(var f',"foreach(var type in Library.GetTypes().Where(t=>ShippingOwners.Contains(t.FullName.Replace('+','.'))))foreach(var f")
    text=text.replace('Library=Assembly.LoadFrom(Path.Combine(directory,"BESHStat.Core.Tests.dll"))','Library=Assembly.LoadFrom(Path.Combine(directory,"BESHStatNG.dll"))')
    text=text.replace('var c=serializer.Deserialize<Dictionary<string,object>>(line);activeCase=c;', 'var c=serializer.Deserialize<Dictionary<string,object>>(line);if(T(c,"target_kind")!="helper"||!ShippingOwners.Contains(T(c,"type")))throw new Exception("SHIPPING_HELPER_WHITELIST_REJECTED");activeCase=c;')
    text=text.replace('var parameters=target.GetParameters();', 'if(target.CustomAttributes.Any(a=>a.AttributeType.FullName=="ExcelDna.Integration.ExcelFunctionAttribute"))throw new InvalidOperationException("EXPORTED_BODY_DISPATCH_FORBIDDEN_04A38");var parameters=target.GetParameters();')
    text=text.replace('int native=(int)Library.GetType("ExcelDna.Integration.XlCall").GetField("NativeAttempts").GetValue(null);if(native!=0)throw new Exception("NATIVE_TEST_DOUBLE_WAS_REACHED");', '// Actual shipping helper dispatch is scalar/array-only. No fake counter is claimed; reference acquisition/wizard/export paths are denied by the whitelist and input protocol.')
    text=text.replace('native_attempts=0,wizard_false_stub_calls=Library.GetType("ExcelDna.Integration.ExcelDnaUtil").GetField("WizardFalseBranchCalls").GetValue(null)', 'native_attempts=(int?)null,native_guard="ACTUAL_SHIPPING_PRIVATE_HELPERS_ONLY; whitelisted scalar/array inputs; no reference/native/export/wizard dispatch"')
    text=text.replace('native="NATIVE_NOT_RUN; explicit native test double only"','native="NATIVE_NOT_RUN; actual shipping private helpers under explicit no-reference/no-export whitelist"')
    text=text.replace('original-source cases; native attempts0;', 'actual shipping private-helper cases; native NOT_RUN (source/input whitelist);')
    recorder=folder/'ShippingHelperCapture38.cs';recorder.write_text(text,encoding='utf-8',newline='\n')
    reference=PRIVATE/'builds/entry'/config/'legacy'
    executable=reference/('ShippingHelperCapture38-'+attempt+'.exe')
    locator=os.path.join(os.environ['ProgramFiles(x86)'],'Microsoft Visual Studio/Installer/vswhere.exe')
    compiler=subprocess.check_output([locator,'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
    facade=os.path.join(os.environ['ProgramFiles(x86)'],'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
    runner=m.Runner(PRIVATE)
    result=runner.command(tag+'-compile',[compiler,'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(reference/'BESHStat.Core.dll'),'/reference:'+facade,'/out:'+str(executable),str(recorder)])
    assert result['exit_code']==0
    result=runner.command(tag+'-run',[str(executable),str(reference),str(cases),str(folder/'original.tsv'),str(folder/'execution.json')])
    assert result['exit_code']==0
    save('preedit/attempts/'+tag+'/bindings.json',dict(status='EXECUTED_ACTUAL_ENTRY_WINDOWS_PRIVATE_HELPERS',source_commit=BASE,
        original_shipping_library=dict(bytes=(reference/'BESHStatNG.dll').stat().st_size,sha256=__import__('hashlib').sha256((reference/'BESHStatNG.dll').read_bytes()).hexdigest()),
        cases=pin(cases.relative_to(ROOT).as_posix()),recorder=pin(recorder.relative_to(ROOT).as_posix()),
        changes='Binding to original shipping assembly; fixed source-owner and helper-only whitelist; cache-clock traversal limited to UDF owner types; native stub counters omitted honestly. No expected/calculation logic changed.',
        native='NOT_RUN; no ExcelReference construction, XlCall, wizard, exported UDF body, UI or COM invocation',scientific='NOT_GOLDEN'))


if __name__=='__main__':main()
