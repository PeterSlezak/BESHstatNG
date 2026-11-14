from common import *
s=(HERE/'verify_slice_v2.py').read_text(encoding='utf-8')
needle=" for c,(o,n) in records.items():\n  # Accepted engines"
assert needle in s
extra=''' def replace_tokens(source,old,new):
  source=tokens(source);old=tokens(old);new=tokens(new);positions=[i for i in range(len(source)-len(old)+1) if source[i:i+len(old)]==old];need(len(positions)==1,'Ambiguous/absent operation slice');i=positions[0];return source[:i]+new+source[i+len(old):]
 need(replace_tokens(body(member(before,'ANOVA1')),classical,'Return PrepareResultTableForUdf(Global.BESHStatNG.ExcelCommon.ParametricUdfReports38.BuildOneWayAnovaTable(data, names))')==tokens(body(member(now,'ANOVA1'))),'Classical native import/catch/body order changed')
 need(replace_tokens(oldWelch,fragment,'Return PrepareResultTableForUdf(Global.BESHStatNG.ExcelCommon.ParametricUdfReports38.BuildWelchAnovaTable(data, names))')==tokens(body(member(now,'ANOVA1_WELCH'))),'Welch native import/catch/body order changed')
 oldMcp=body(member(before,'ANOVA1_MCP'));start=oldMcp.index('Dim mdl As New parametric.OneWayANOVA');end=oldMcp.index('Catch ex As Exception',start);oldBlock=oldMcp[start:end]
 nativeBlock="""Dim stackAll As Boolean = False
 Dim tables = Global.BESHStatNG.ExcelCommon.ParametricUdfReports38.BuildOneWayMcpTables(data, names, which, alphaValue, stackAll)
 If tables Is Nothing Then Return ExcelError.ExcelErrorValue
 If Not stackAll Then Return PrepareResultTableForUdf(tables(1))
 Dim stacked As Object(,) = PrepareResultTableForUdf(tables(1))
 For i As Integer = 2 To tables.Count - 1
 stacked = StackWithBlankRow(stacked, PrepareResultTableForUdf(tables(i)))
 Next
 Return PrepareResultTableForUdf(stacked)"""
 need(replace_tokens(oldMcp,oldBlock,nativeBlock)==tokens(host),'MCP native whole body/import/error/render/catch changed')
 caseGroups=list(re.finditer(r'^\\s*Case (.*)$',oldBlock,re.M));keys=[];branches=[]
 for i,case in enumerate(caseGroups):
  label=case[1].strip()
  if label=='Else':branches.append('Case Else\\nReturn Nothing');continue
  keys.extend(re.findall(r'"(?:""|[^"])*"',label));section=oldBlock[case.end():caseGroups[i+1].start()];engine=section[:section.index('Dim tables = mdl.wrapResults()')].strip();branches.append('Case '+label+'\\n'+engine+('\\nstackAll = True' if label=='"ALL", "BOTH"' else ''))
 expectedMcp='Dim mdl As New Global.BESHStatNG.parametric.OneWayANOVA(data, names)\\nmdl.compute()\\nstackAll = False\\nSelect Case LegacyUdfText38.CaseToken(which, New String() {'+', '.join(keys)+'})\\n'+'\\n'.join(branches)+'\\nEnd Select\\nReturn mdl.wrapResults()'
 need(tokens(mcp)==tokens(expectedMcp),'MCP entire alias/action/default/wrap dispatch changed')
 portableWrapper=expected.replace('Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.','')
 need(tokens(body(member(text(core),'TryInvertMatrix')))==tokens(portableWrapper),'Existing portable inversion wrapper contract changed')
'''
s=s.replace(needle,extra+needle)
with (HERE/'verify_slice_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
