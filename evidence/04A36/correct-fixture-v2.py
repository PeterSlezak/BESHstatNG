"""Versioned fixture correction. Archived original algorithm; entry assemblies only."""
from work import *
if __name__=='__main__':
    assert not (HERE/'LegacyPresentation36V2.vb').exists()
    old=(HERE/'LegacyPresentation36.vb').read_text()
    new=old.replace('OriginalPresentation36','OriginalPresentation36V2')
    needle='Dim panelValue As New SpcPanelResult(kinds(index Mod kinds.Length),"Statistic",points)'
    assert needle in new
    new=new.replace(needle,'''Dim signalValues As New List(Of SpcSignalResult)
For Each point As SpcPointResult In points
For Each number As Integer In point.SignalRuleNumbers
Dim rule=SpcRuleCatalog.GetRules(SpcRulePreset.Nelson).Single(Function(r) r.RuleNumber=number)
signalValues.Add(New SpcSignalResult(kinds(index Mod kinds.Length),point.StageId,rule,point.PointIndex,point.PointIndex,point.PointIndex))
Next:Next
Dim panelValue As New SpcPanelResult(kinds(index Mod kinds.Length),"Statistic",points,signals:=signalValues.ToArray())''')
    write('evidence/04A36/LegacyPresentation36V2.vb',new)
    c=(HERE/'CapturePresentation36.cs').read_text().replace('CapturePresentation36','CapturePresentation36V2').replace('LegacyPresentation36.dll','LegacyPresentation36V2.dll').replace('OriginalPresentation36','OriginalPresentation36V2')
    write('evidence/04A36/CapturePresentation36V2.cs',c)
    ps=(HERE/'capture-presentation.ps1').read_text().replace('LegacyPresentation36.dll','LegacyPresentation36V2.dll').replace('LegacyPresentation36.vb','LegacyPresentation36V2.vb').replace('CapturePresentation36','CapturePresentation36V2').replace('pre-edit-presentation.csv','pre-edit-presentation-v2.csv')
    write('evidence/04A36/capture-presentation-v2.ps1',ps)
    c=(HERE/'CaptureAxis36.cs').read_text().replace('CaptureAxis36','CaptureAxis36V2').replace('LegacyPresentation36.dll','LegacyPresentation36V2.dll').replace('OriginalPresentation36','OriginalPresentation36V2')
    write('evidence/04A36/CaptureAxis36V2.cs',c)
    ps=(HERE/'capture-axis.ps1').read_text().replace('CaptureAxis36','CaptureAxis36V2').replace('pre-edit-axis.csv','pre-edit-axis-v2.csv')
    write('evidence/04A36/capture-axis-v2.ps1',ps)
    save('fixture-correction-v2.json',dict(status='VERSIONED_CORRECTION',original_source=pin('evidence/04A36/LegacyPresentation36.vb'),corrected_source=pin('evidence/04A36/LegacyPresentation36V2.vb'),original_capture=pin('evidence/04A36/pre-edit-presentation.csv'),reason='Most V1 synthetic panels carried unmatched point rule numbers or were empty:378 construction rejections and6 valid outputs per384 rows. All rows remain immutable; V1 provides insufficient valid-series coverage. V2 supplies corresponding immutable panel signal metadata; it changes no original preparation algorithm or scientific calculation.',timing='Correction after initial production extraction, explicitly versioned. Compiles only frozen legacy functions and links actual pre-edit entry assemblies. No new36 implementation is an oracle. Original successful5472 KM/Cox/band corpus unchanged.',scientific='NOT_GOLDEN'))
    print('Retained v1; generated independent archived-source v2 with valid panel signal metadata')
