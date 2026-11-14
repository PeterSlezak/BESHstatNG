from common import *
rows=[]
for path in ['BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']:
 p=ROOT/path;dst=HERE/'attempts/families-v1-fixtures'/(path+'.txt');dst.parent.mkdir(parents=True,exist_ok=True)
 with dst.open('xb') as f:f.write(p.read_bytes())
 text=p.read_text(encoding='utf-8-sig')
 if path.startswith('BESHStat.Excel.Common.Tests'):
  old='Assert.AreEqual(2, UdfInferencePreparation38.IndependentTTableSelection("ＷＥＬＣＨ"))'
  new='Assert.AreEqual(If(culture = "sk-SK", -1, 2), UdfInferencePreparation38.IndependentTTableSelection("ＷＥＬＣＨ"))'
  assert old in text;text=text.replace(old,new,1)
  old='Dim eq = Global.BESHStatNG.equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence(a, b, -2.0R, 2.0R, 0.025R, False)'
  new='''Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Global.BESHStatNG.equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence(a, b, -2.0R, 2.0R, 0.025R, False))
        Dim eq = Global.BESHStatNG.equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence(a, b, -4.0R, 4.0R, 0.025R, False)'''
  assert old in text;text=text.replace(old,new,1)
 else:
  old='Dim eq = DirectCast(ParametricUDFs.TTEST_UNPAIRED_EQUIV(x, y, 2.0R), Object(,))'
  new='''Assert.AreEqual(ExcelError.ExcelErrorValue, ParametricUDFs.TTEST_UNPAIRED_EQUIV(x, y, 2.0R))
            Dim eq = DirectCast(ParametricUDFs.TTEST_UNPAIRED_EQUIV(x, y, 4.0R), Object(,))'''
  assert old in text;text=text.replace(old,new,1)
 write_source(path,text);rows.append(dict(before=pin(dst),after=pin(p)))
save('attempts/inference-fixture-correction.json',dict(status='NEW_FIXTURE_ERRORS_RETAINED_AND_CORRECTED',
    failures=[dict(test='Inference38KeepsReportChoicesNamesAndPairOwnership/sk-SK',cause='New assertion incorrectly assumed fullwidth WELCH compares equal in sk-SK; actual established comparison policy returns no alias. Preserve the culture difference, do not change global policy.'),
              dict(test='Inference38MeanReportBodiesKeepLabelsRawNumbersAndCiHelpers',cause='New sample difference2.25 lies outside margins[-2,2]; original engine properly throws. Add explicit throw/error check and use[-4,4] for valid report testing.')],
    records=[pin(HERE/('focused-families-v1-'+c+'.json')) for c in ['Debug','Release']],preimages=rows,
    changed_old_assertions=0,production_changes=0,original_queries=0))
print('Incorrect new culture/valid-domain fixture assumptions corrected; adverse failures retained')
