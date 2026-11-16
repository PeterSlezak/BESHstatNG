from common45 import *
import re
lock();path='BESHstatNG/src/UI/UibyID.vb';source=(ROOT/path).read_text(encoding='utf-8-sig')
names=['optOutputRange_Click','optWorksheet_Click','optWorkbook_Click','optByColumn_Click','optByID_Click','ApplyUnpairedTtestInputLabels']
blocks=[];records=[]
for name in names:
 match=re.search(r'(?ms)^    Private Sub '+name+r'\(.*?^    End Sub',source);assert match
 blocks.append(match.group())
 records.append(dict(owner='BESHStatNG.UibyID',method=name,start_line=source[:match.start()].count('\n')+1,source_text_sha256=sha(match.group().encode()),binding=re.search(r'Handles (.*)',match.group()).group(1) if 'Handles ' in match.group() else 'helper-reached-from-optByColumn_Click'))
head='''Option Strict On
Option Explicit On
Imports System
Namespace Harness45
Friend Class FakeControl45
 Friend Enabled As Boolean = True
 Friend Visible As Boolean = True
 Friend Checked As Boolean
 Friend Text As String = ""
 Friend Selections As Integer
 Friend Event Click As EventHandler
 Friend Sub FireClick()
  RaiseEvent Click(Me, System.EventArgs.Empty)
 End Sub
 Friend Sub [Select]()
  Selections += 1
 End Sub
End Class
Friend Class FakeRefEdit45
 Friend Enabled As Boolean = True
 Friend txtAddress As New FakeControl45()
End Class
Friend Class OptionProjection45
 Friend Tag As HelpTopic
 Friend RefEdit1 As New FakeRefEdit45(), RefEdit2 As New FakeRefEdit45(), RefEditOutput As New FakeRefEdit45()
 Friend lblRefedit1 As New FakeControl45(), lblRefedit2 As New FakeControl45()
 Friend WithEvents optOutputRange As New FakeControl45(), optWorksheet As New FakeControl45(), optWorkbook As New FakeControl45(), optByColumn As New FakeControl45(), optByID As New FakeControl45()
'''
target=ROOT/'BESHStatNG.Test/src/OptionProjection45.vb'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write(head+'\n'.join(blocks)+'\nEnd Class\nEnd Namespace\n')
save('fixtures/option-projection-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A45_SOURCE_DERIVED_FAKE',source=pin(path),projection=pin(target),members=records,substitutions=[],binding='Original Handles clauses compile unchanged against fake WithEvents events;tests raise those events,not direct handler calls',unproved='Actual WinForms radio exclusivity,focus,selection,workbook capture and request projection remain native pending;family sweep46/47',native='NOT_RUN'))
