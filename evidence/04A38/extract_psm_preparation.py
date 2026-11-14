"""Extract PSM option composition without changing cache/native argument order."""
import re
from preflight import ROOT,save,pin

path='BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb'
p=ROOT/path;raw=p.read_bytes();text=raw.decode('utf-8-sig');edits=[];methods=[]
names=['NormalizeFitOptions','ApplyOptionText','ApplyOption','EnsureCoarseningSpec']
for name in names:
    match=re.search(r'^        Private Sub '+name+r'\b.*?^        End Sub',text,re.M|re.S);assert match,name
    code=match.group().replace('Private Sub ','Public Shared Sub ',1)
    code=code.replace('ChrW(10)','Convert.ToChar(10)').replace('ChrW(13)','Convert.ToChar(13)')
    if name=='ApplyOption':
        tokens=re.findall(r'^                Case (.*)$',code,re.M)
        literals=[token.strip() for line in tokens for token in line.split(',')]
        assert all(t.startswith('"') and t.endswith('"') for t in literals)
        code=code.replace('Select Case key','Select Case LegacyUdfText38.CaseToken(key, New String() {'+', '.join(literals)+'})',1)
    methods.append(code)
    args={'NormalizeFitOptions':'fitOptions','ApplyOptionText':'fitOptions, optionText','ApplyOption':'fitOptions, key, value','EnsureCoarseningSpec':'fitOptions'}[name]
    newline='\r\n' if '\r\n' in text else '\n'
    first=match.group().splitlines()[0]
    edits.append((match.start(),match.end(),first+newline+'            Global.BESHStatNG.ExcelCommon.UdfPsmPreparation38.'+name+'('+args+')'+newline+'        End Sub'))
for start,end,new in reversed(edits):text=text[:start]+new+text[end:]
# Typed enum selection is reusable. The old private dynamic reflection helper is
# retained as a compatibility seam until the complete member ledger resolves it.
match=re.search(r'^        Private Function ParseEstimand\b.*?^        End Function',text,re.M|re.S);assert match
replacement=match.group().splitlines()[0]+'\r\n            Return Global.BESHStatNG.ExcelCommon.UdfPsmPreparation38.ParseEstimand(value, defaultValue)\r\n        End Function'
text=text[:match.start()]+replacement+text[match.end():]
methods.append('''        Public Shared Function ParseEstimand(value As String, defaultValue As PsmEstimand) As PsmEstimand
            If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Try
                Return CType([Enum].Parse(GetType(PsmEstimand), value.Trim(), True), PsmEstimand)
            Catch
                Return defaultValue
            End Try
        End Function''')
target=ROOT/'BESHStat.Excel.Common/src/Analysis/UdfPsmPreparation38.vb';assert not target.exists()
header='Option Explicit On\nOption Strict On\n\nImports System\nImports BESHStatNG.CausalInference\nImports BESHStatNG.ExcelCommon.PropensityScoreUDFsOptions38\n\nNamespace ExcelCommon\n    Public NotInheritable Class UdfPsmPreparation38\n        Private Sub New()\n        End Sub\n'
target.write_text(header+'\n\n'.join(m.replace('\r\n','\n') for m in methods)+'\n    End Class\nEnd Namespace\n',encoding='utf-8',newline='\n')
p.write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8'))
save('psm-preparation-transition-v1.json',dict(status='IMPLEMENTED_NOT_YET_VALIDATED',members=names+['ParseEstimand'],common=pin(target.relative_to(ROOT).as_posix()),windows=pin(path),boundary='Normalized options only; old Windows cache dictionaries, lifecycle and native AsString evaluation order retained.'))
print('Extracted five PSM option composition/selection members')
