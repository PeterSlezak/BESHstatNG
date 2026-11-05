"""Freeze the test-fixture Core invocation with original application option branches.
Not a second statistical engine; same unchanged Core methods and explicit existing test seed.
"""
import importlib.util
from pathlib import Path
s=importlib.util.spec_from_file_location('c26_ref',Path(__file__).with_name('capture-04a26-contract.py'));c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def generate():
    path=c.ROOT/'BESHStat.Excel.Common.Tests/src/Agreement26Tests.vb';text=path.read_text(encoding='utf-8-sig')
    start=text.index('            Dim failure = Planning26Tests.Failure(Sub()')+len('            Dim failure = Planning26Tests.Failure(Sub()')
    end=text.index('            End Sub)',start);body=text[start:end]
    for name,legacy in [('DemingRequestSelection','Deming'),('LinRequestSelection','Lin'),('KappaRequestSelection','Kappa'),('BlandRequestSelection','Bland'),('IccRequestIntent','Icc')]:body=body.replace('New '+name+'(', 'New Agreement26Legacy.'+legacy+'(')
    body=body.replace('axis.First(), axis.Second()', 'x, y').replace('axis.FirstCategories(), axis.SecondCategories()', 'first, second')
    header='''Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.Agreement
Imports BESHStatNG.ExcelCommon
Public NotInheritable Class Agreement26Reference
    Public Shared Function Run(kind As Integer, profile As Integer, x As Double(), y As Double(), rows As Integer(), subjects As Object()) As (Result As Object, Tables As List(Of ResultTable), Options As Object)
        Dim axis As AgreementInputAxis = Nothing, tables As List(Of ResultTable) = Nothing, fitResult As Object = Nothing, options As Object = Nothing
        Dim effectiveInput As Object = Nothing, effectiveRows = rows
'''
    out=c.ROOT/'BESHStat.Excel.Common.Tests/src/Agreement26Reference.vb';out.write_text(header+body+'\n        Return (fitResult, tables, options)\n    End Function\nEnd Class\n',encoding='utf-8',newline='\n')
    c.emit('agreement-reference-provenance.json',dict(legacy_options=dict(path='BESHStat.Excel.Common.Tests/src/Agreement26Legacy.vb',sha256=c.sha(c.ROOT/'BESHStat.Excel.Common.Tests/src/Agreement26Legacy.vb')),fixture_invocations=dict(path=out.relative_to(c.ROOT).as_posix(),sha256=c.sha(out)),native_authority='prechange-method-provenance.json: original radio/variance/dispatch/label branches',status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',seed='12345 only in tests through existing Core Fit randomSeed parameters; native default remains Integer.MinValue.'))
if __name__=='__main__':generate()
