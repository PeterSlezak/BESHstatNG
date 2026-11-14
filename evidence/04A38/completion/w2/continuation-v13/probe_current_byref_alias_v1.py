"""Current-only managed probe; never runs or recompiles archived original sources."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path.cwd()/'evidence/04A38/completion/w2/continuation-v13'))
from common import *
folder=PRIVATE/'current-byref-alias-probe-v1';folder.mkdir(exist_ok=False)
source=folder/'CurrentGuessAlias.cs';exe=folder/'CurrentGuessAlias.exe'
code=r'''using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Script.Serialization;
public static class CurrentGuessAlias {
    public static string Label;
    public sealed class Cell {
        public override string ToString() { Label="converted"; return ""; }
    }
    private delegate bool Guess(object[,] cells,object explicitPositive,ref string label);
    public static int Main(string[] args) {
        string lib=Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve+=(sender,eventArgs)=>{
            string p=Path.Combine(lib,new AssemblyName(eventArgs.Name).Name+".dll");
            return File.Exists(p)?Assembly.LoadFrom(p):null;
        };
        Assembly a=Assembly.LoadFrom(Path.Combine(lib,"BESHStatNG.dll"));
        Type owner=a.GetType("BESHStatNG.UdfDataImport",true);
        MethodInfo method=owner.GetMethod("GuessPositiveClass",BindingFlags.NonPublic|BindingFlags.Static);
        var call=new DynamicMethod("CurrentGuessAliasOnly",typeof(bool),new[]{typeof(object[,]),typeof(object),typeof(string).MakeByRefType()},typeof(CurrentGuessAlias).Module,true);
        ILGenerator il=call.GetILGenerator();il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Ldarg_2);il.Emit(OpCodes.Call,method);il.Emit(OpCodes.Ret);
        Guess guess=(Guess)call.CreateDelegate(typeof(Guess));
        Label="before"; bool result=guess(new object[,]{{"red"},{"green"},{"blue"}},new Cell(),ref Label);
        Console.WriteLine(new JavaScriptSerializer().Serialize(new {
            classification="SYNTHETIC_CURRENT_ONLY_MANAGED_BYREF_ALIAS_PROBE_NOT_ORIGINAL_REFERENCE",
            result=result, output=Label, conversionSideEffect="converted",
            originalExecution=false, statisticalEngineExecution=false, nativeOfficeExecution=false,
            currentMethod=method.DeclaringType.FullName+"."+method.Name
        }));
        return (!result && Label==null)?0:2;
    }
}'''
with source.open('x',encoding='utf-8',newline='\n') as stream:stream.write(code)
compiled=command('current-byref-alias-probe-compile-v1',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(source)])
assert compiled['exit_code']==0
executed=command('current-byref-alias-probe-run-v1',[str(exe),str(PRIVATE/'packet-v1/Debug/lib')])
result=dict(source=pin(source),executable=pin(exe),compiled=clean(compiled),executed=clean(executed),
    runtime_output=pin(RUNROOT/'commands/w2c13-current-byref-alias-probe-run-v1/stdout.log'),
    qualification='New synthetic current-only managed ownership probe, separately counted from the40 new managed unit identities and956 Python parents. No archived source/DLL or original recorder was executed or recompiled. Source-only original prediction is separate from the measured current result.',original_queries=0)
with (folder/'execution.json').open('x',encoding='utf-8',newline='\n') as stream:json.dump(result,stream,indent=2)
assert executed['exit_code']==0
print('Current managed ByRef alias output confirms the additional Common reset; keep complete-parity claims fail-closed')
