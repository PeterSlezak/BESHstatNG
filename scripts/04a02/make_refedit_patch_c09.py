"""Render an unapplied, source-pinned product patch; never edit shipping files."""
import argparse
import difflib
import hashlib
import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
PIN='4ff232377a717e86fea412eb438a95db54969fac52b91224c2d93d3aab1aba84'


def proposed(source):
    def member(text,name,new):
        start=text.index('    Private Sub '+name+'(')
        end=text.index('    End Sub',start)+len('    End Sub')
        return text[:start]+new+text[end:]
    source=source.replace('    Private DisplayState As RefEditState',
        '    Private ReadOnly selectionLease As New RefEditEventLease(Of Excel.Range)\n    Private DisplayState As RefEditState',1)
    source=member(source,'xlSheetChangeHandle','''    Private Sub xlSheetChangeHandle(ByVal target As Excel.Worksheet) Handles xlBook.SheetActivate
        If Me.IsDisposed OrElse selectionLease.IsClosed OrElse Not pbInTextAdressHandler Then Return
        Try
            selectionLease.Clear()
            If target Is Nothing Then Return
            xlSheet = target
            BindSelection(target)
        Catch ex As Exception
            pbInTextAdressHandler = False
            LogRefEdit($"SheetActivate lifetime failure: {ex}")
        End Try
    End Sub

    Private Sub BindSelection(target As Excel.Worksheet)
        selectionLease.Rebind(Function(callback)
                                  Dim handler As Excel.DocEvents_SelectionChangeEventHandler = Sub(value) callback(value)
                                  AddHandler target.SelectionChange, handler
                                  Return Sub() RemoveHandler target.SelectionChange, handler
                              End Function, AddressOf SelectionChange)
    End Sub''')
    anchor='        Set(ByVal value As Excel.Application)\n            _ExcelConnector = value'
    assert source.count(anchor)==1
    source=source.replace(anchor,'''        Set(ByVal value As Excel.Application)
            selectionLease.Clear() ' Invalidate before WithEvents workbook/application rebinding.
            pbInTextAdressHandler = False
            xlBook = Nothing
            xlSheet = Nothing
            _ExcelConnector = value''',1)
    # All active selection add/remove sites now route through the same tested lease.
    lines=source.splitlines()
    for i,line in enumerate(lines):
        if line.lstrip().startswith("'"):continue
        if line.strip()=='AddHandler xlSheet.SelectionChange, AddressOf SelectionChange':
            lines[i]=line[:len(line)-len(line.lstrip())]+'BindSelection(xlSheet)'
        elif line.strip()=='RemoveHandler xlSheet.SelectionChange, AddressOf SelectionChange':
            lines[i]=line[:len(line)-len(line.lstrip())]+'selectionLease.Clear()'
    source='\n'.join(lines)+'\n'
    source=source.replace('        If ExcelConnector Is Nothing Then Return\n\n        Try\n            If xlBook Is Nothing',
        '        If ExcelConnector Is Nothing OrElse selectionLease.IsClosed Then Return\n        selectionLease.Clear()\n\n        Try\n            If xlBook Is Nothing',1)
    source=source.replace('    Private Sub SelectionChange(ByVal target As Excel.Range)\n        Try',
        '    Private Sub SelectionChange(ByVal target As Excel.Range)\n        If Me.IsDisposed OrElse selectionLease.IsClosed Then Return\n        Try',1)
    source=source.replace('                Call _NAR(rng)','                \' Borrowed event/Areas RCW: never ReleaseComObject here.',1)
    source=member(source,'Excel2007RefEdit_Disposed','''    Private Sub Excel2007RefEdit_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        pbInTextAdressHandler = False
        Try
            selectionLease.Dispose() ' Invalidates before detach, including a disconnected sheet.
        Catch ex As Exception
            LogRefEdit($"Disposed selection detach failure: {ex}")
        End Try
        Try
            xlBook = Nothing ' VB WithEvents detaches SheetActivate; callback guard is already closed.
        Catch ex As Exception
            LogRefEdit($"Disposed workbook detach failure: {ex}")
        Finally
            xlSheet = Nothing
            xlApp = Nothing ' No active xlApp Handles clause exists; retain no borrowed application.
            _ExcelConnector = Nothing
            DisplayState = Nothing
        End Try
    End Sub''')
    return source


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--evidence',type=Path,required=True);a=p.parse_args()
    source=ROOT/'BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb'
    assert hashlib.sha256(source.read_bytes()).hexdigest()==PIN,'Source drift'
    before=source.read_text(encoding='utf-8-sig');after=proposed(before)
    records=[('BESHstatNG/src/UI/Refedit/Excel2007RefEdit.vb',before,after),
             ('BESHstatNG/src/UI/Refedit/RefEditEventLease.vb','',(ROOT/'scripts/04a02/review/RefEditEventLease.vb').read_text()),
             ('BESHStatNG.Test/src/MigrationRefEditBoundary09Tests.vb','',(ROOT/'scripts/04a02/review/MigrationRefEditBoundary09Tests.vb').read_text())]
    for path,newline,anchor in [('BESHstatNG/BESHStatNG.vbproj','    <Compile Include="src\\UI\\Refedit\\RefEditEventLease.vb" />',
                               '    <Compile Include="src\\UI\\Refedit\\Excel2007RefEdit.vb">'),
                              ('BESHStatNG.Test/BESHStatNG.Tests.vbproj','    <Compile Include="src\\MigrationRefEditBoundary09Tests.vb" />',
                               '    <Compile Include="src\\')]:
        text=(ROOT/path).read_text(encoding='utf-8-sig');index=text.index(anchor)
        records.append((path,text,text[:index]+newline+'\n'+text[index:]))
    target=a.evidence/'refedit-proposal';target.mkdir()
    patch=''.join(''.join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),
        fromfile='a/'+path if old else '/dev/null',tofile='b/'+path)) for path,old,new in records)
    with (target/'RefEdit-C09-unapplied.patch').open('x',encoding='utf-8',newline='\n') as f:f.write(patch)
    with (target/'Excel2007RefEdit.proposed.vb').open('x',encoding='utf-8') as f:f.write(after)
    with (target/'proposal.json').open('x',encoding='utf-8') as f:json.dump(dict(status='PROPOSED_NOT_APPLIED',source_sha256=PIN,
        paths=[path for path,_,_ in records],limitation='Coordinator tests execute proposed shared orchestration. Private WinForms/COM handlers and WithEvents RCW detachment still require product authorization and native regression; no independent risk signoff.'),f,indent=2)
    print(target)


if __name__=='__main__':main()
