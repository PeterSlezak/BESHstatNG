"""Classify every actual comparison site, without closing unrelated parent bodies."""
from common import *
t=json.loads((HERE/'text-call-sites.json').read_bytes());rows=[]
for r in t['calls']:
 parent=r['parent_name'];owner,name=parent.rsplit('.',1);owner=owner.rsplit('.',1)[1]
 if owner=='AssumptionsUDFsOptions38':normal='NormalizeText(arg): null→empty; trim/case/separators; ignorable characters may remain'
 elif owner=='MultivariateAnalysisUDFsOptions38' or owner=='ParametricUDFsOptions38':normal='NormalizeToken(input): null→empty; trim/case/separators; ignorable characters may remain'
 elif owner=='PropensityScoreUDFsOptions38':normal='blank guard/trim/lower' if name=='ParseBoolean' else 'NormalizeOptionKey(input): retain only letters/digits, invariant case'
 elif owner in ['CoxUDFsOptions38','LinearModelUDFsOptions38','MultinomialLogitUDFsOptions38','OrdinalLogitUDFsOptions38','SampleSizeUDFsOptions38']:normal='blank guard then Trim/case; nullable input returns through guard'
 elif owner in ['GEEUDFsOptions38','GLMUDFsOptions38','ZIPUDFsOptions38']:normal='blank guard then NormalizeKey; uses NormalizeMatchToken null→empty'
 elif owner=='UdfDataImportOptions38':
  if name.startswith('Parse') and name!='ParseRocDirection':normal='NormalizeToken(input): null→empty; ignorable characters may remain'
  elif name in ['GetCoxTieMethod','GetFormulaAddressingMode']:normal='blank guard then Trim/lower'
  elif name in ['GetHistogramBinRule','ParseRocDirection']:normal='UdfDataImport NormalizeOptionKey: Trim/lower/space-hyphen-underscore removal; ignorables may remain'
  else:normal='NormalizeRegressionOptionKey; blank/default guard; fixed literal alternatives'
 elif owner=='UdfScalarUtilitiesOptions38':normal='NormalizeMixedModelOptionToken: null→empty, letter/digit filtering removes NUL/soft-hyphen'
 elif owner in ['LmmUDFsMetadata38','MixedModelUDFsMetadata38','UdfDataImportMetadata38']:normal='nm = If(nm,String.Empty).Trim(); no null left at comparison'
 elif owner=='RegressionFormulaUDFsMetadata38':normal='Coalesced formula/error/name text; regex Group.Value and extraction return non-null; summary names coalesced/trimmed before Where. addressingMode may be null but choices fixed ASCII.'
 elif owner=='UdfNumericImportPreparation38':normal='g/sg = host labelText(cell), actual Windows callback Convert.ToString(cell).Trim(); null cell→empty; custom null ToString throws before comparison'
 elif owner=='UdfClassLabelPreparation38':normal='norms(i) comes from UdfDataImportOptions38.NormalizeOptionKey; ignorable characters may remain'
 elif owner=='UdfPsmPreparation38':normal='key may be null; comparison choices are fixed non-null printable ASCII'
 elif owner=='UdfScalarChoice38':normal='blank guard then Trim/case; return before comparisons on null/whitespace'
 else:raise ValueError('Unclassified parent '+parent)
 rows.append(dict(r,normalization_to_be_independently_classified=False,normalization_review=normal,null_rhs_reachable=False,ignorable_rhs_reachable=False,left_null_allowed_by_proof=True,proof_scope='Fixed non-null literal RHS/choice excludes demonstrated null-versus-ignorable mismatch regardless of left normalization; does NOT close whole parent exception/culture behavior'))
save('text-caller-domains.json',dict(status='ALL59_DIRECT_LINES_AND63_CASETOKEN_SITES_DOMAIN_REVIEWED',syntax_table=pin(HERE/'text-call-sites.json'),policy=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb'),source_line_groups=sorted({r['line_site_id'] for r in rows if r['kind']=='EqualText'}),calls=rows,parents=sorted({r['parent_name'] for r in rows}),policy_change='NONE',synthetic_differences_per_configuration=8,source_domain_differences_per_configuration=0,scope='Standard Invariant/sk-SK Framework host cultures; neither scientific reference nor new archived original BESH query',whole_parent_closure=False))
save('attempts/inherited-text-table-recorder-scope.json',dict(status='FAILED_SETUP',script=pin(PREVIOUS/'record_text_domains.py'),error="KeyError:sites; AST schema uses calls. No statistical case or production change."))
