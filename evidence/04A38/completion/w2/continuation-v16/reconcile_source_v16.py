"""Independent complete original/entry/current source composites, no reference execution."""
from proof_support_v16 import *
PROOF=HERE
OUT=HERE
contracts=load(PROOF/'whole-body-contracts-v1.json')['contracts']
portable=load(PROOF/'portable-api-contracts-v1.json')['contracts']
full=load(HERE/'final-v4/dependency-full-common.json');core=load(HERE/'final-v4/dependency-full-core.json')
api={(n['owner'].split('.')[-1],n['name']):n for n in full['members'] if n['parent_member'] is None and n['kind'] in ['FunctionBlock','SubBlock']}
def code(owner,name):
 n=api[owner,name];return body((ROOT/n['path']).read_bytes(),n)[0]
def untext(value):
 value=unwrap_cases(value)
 value=re.sub(r'LegacyUdfText38\.EqualText\((\w+),\s*("(?:[^"\n]|"")*"|String\.Empty)\)',r'\1 = \2',value)
 return value.replace('Global.BESHStatNG.regression.','regression.')
def same(a,b,label):assert tokens(a)==tokens(b),'Complete source mismatch '+label
old=load(PREVIOUS/'current-ownership-v3.json');before={n['symbol_id']:n for n in old['members']}
mapping={r['after']:r['before'] for r in load(PROOF/'current-semantic-position-crosswalk-v1.json')['rows']}
preimages={r['path']:r for r in load(HERE/'source-transitions-freeze-v3.json')['files'] if r['before']}
moves=sum([load(HERE/name)['moves'] for name in ['packet-A-source-moves-v1.json','packet-B-source-moves-v1.json','adjacent-LMM-source-moves-v1.json']],[])
rows=[];unresolved=[]
for c in contracts:
 n=c['current_record'];oldn=before[mapping[c['current_id']]]
 raw=(ROOT/preimages[n['path']]['preimage']['path']).read_bytes() if n['path'] in preimages else (ROOT/n['path']).read_bytes()
 entry_text,entry_view=body(raw,oldn);original=c['original_whole_body']
 try:
  if not c['new_support']:
   assert n['body_sha256']==oldn['body_sha256'];mode='EXACT_RETAINED_C15_SOURCE_AND_COMPILED_AUTHORITY'
  elif tokens(original)==tokens(entry_text):mode='ENTIRE_ORIGINAL_EQUALS_ENTRY'
  elif n['name'] in ['GeeFindHeaderIndex','GeeFindDesignColumnIndex','GeeNormalizeDesignColumnName']:
   same(phase(original),phase(code('GEEUDFsMetadata38',n['name'])),n['name'])
   args='headers, acceptedNames' if n['name']=='GeeFindHeaderIndex' else 'names, requestedName' if n['name']=='GeeFindDesignColumnIndex' else 's'
   same(phase(entry_text),'Return Global.BESHStatNG.ExcelCommon.GEEUDFsMetadata38.'+n['name']+'('+args+')','Native '+n['name']);mode='EXACT_COMMON_METADATA_BODY_AND_NATIVE_FORWARDER'
  elif n['name'] in ['ParseGeeLsmEstimateScale','ParseGlmResidualType','ParseLmAnovaScope','ParseLmResidualType']:
   owner='GEEUDFsOptions38' if n['name'].startswith('ParseGee') else 'GLMUDFsOptions38' if n['name'].startswith('ParseGlm') else 'LinearModelUDFsOptions38'
   expected=phase(original).replace('Dim s As String = AsString(v)','Dim s As String = v')
   same(expected,untext(phase(code(owner,n['name']))),n['name'])
   same(phase(entry_text),'Return Global.BESHStatNG.ExcelCommon.'+owner+'.'+n['name']+'(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(v))','Native '+n['name']);mode='COMPLETE_CONVERSION_GRAMMAR_DEFAULT_CASE_AND_EXPLICIT_LEGACY_TEXT_POLICY'
  elif n['name']=='NormalizeNameList':
   same(phase(original),untext(phase(code('LmmUDFsMetadata38',n['name']))),n['name'])
   same(phase(entry_text),'Return Global.BESHStatNG.ExcelCommon.LmmUDFsMetadata38.NormalizeNameList(inputNames, expectedCount, fallbackPrefix)','Native normalize');mode='EXACT_ORIGINAL_NAME_FALLBACK_WITH_LEGACY_TEXT_POLICY'
  elif n['name']=='AverageGeeFittedDesignRowForLsmProfile':
   prefix='matchedCount = 0\nIf h Is Nothing OrElse h.FittedDesign Is Nothing OrElse component Is Nothing Then Return Nothing'
   original_phase=phase(original);guard='If h Is Nothing OrElse h.FittedDesign Is Nothing OrElse component Is Nothing Then Return Nothing'
   suffix=original_phase[original_phase.index(guard)+len(guard):]
   aliases=[('h.FittedDesign','design'),('h.FittedTimeValues','timeValues'),('component.TimeSpecified','timeSpecified'),('component.TimeValue','targetTime'),('component.ProfileValues','profiles'),('For Each profileValue As GeeLsmEstimateProfileValue','For Each profile As T'),('profileValue.ColumnIndex','column(profile)'),('profileValue.Value','value(profile)'),('NearlyEqual(','equal(')]
   for a,b in aliases:suffix=suffix.replace(a,b)
   target=next(n for n in core['members'] if n['owner']=='BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38' and n['name']=='AverageDesign')
   same('matchedCount = 0\n'+suffix,phase(body((ROOT/target['path']).read_bytes(),target)[0]),'Entire generic AverageDesign')
   expected=prefix+'''\nReturn Global.BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38.AverageDesign(
h.FittedDesign, h.FittedTimeValues, component.TimeSpecified, component.TimeValue,
component.ProfileValues, Function(profile) profile.ColumnIndex, Function(profile) profile.Value,
AddressOf NearlyEqual, matchedCount)'''
   same(phase(entry_text),expected,'Native guarded lazy averaging');mode='COMPLETE_GUARDED_GENERIC_AVERAGE_WITH_LAZY_PROFILE_ACCESSORS_AND_BYREF_RESET'
  elif n['name']=='NormalizeResultTableTitle':
   same(phase(original),phase(code('UdfOutputTablesOptions38',n['name'])),n['name'])
   same(phase(entry_text),'Return Global.BESHStatNG.ExcelCommon.UdfOutputTablesOptions38.NormalizeResultTableTitle(title)','Native title normalization');mode='EXACT_ORIGINAL_TITLE_GRAMMAR_AND_NATIVE_REPORT_BOUNDARY'
  elif n['name'] in ['ParseMixedModelFitMethodStrict','ParseMixedModelInferenceMethodStrict','ParseMixedModelCovarianceOptimizerModeStrict','ParseMixedModelCovarianceGradientModeStrict','ApplyMixedModelOptimizerShortcutToGradient','NormalizeMixedModelResidualCovarianceName','NormalizeMixedModelOptionToken']:
   expected=phase(original)
   if n['name']=='NormalizeMixedModelOptionToken':expected=expected.replace('Dim raw As String = ExcelArgReaders.AsString(arg)','Dim raw As String = arg')
   if n['name']=='NormalizeMixedModelResidualCovarianceName':raise AssertionError('UNPROVED_ORIGINAL_FALLBACK_RECONVERSION: original NormalizeMixedModelOptionToken invokes AsString(arg), then unknown nonempty token invokes AsString(arg) a second time; current wrapper converts once before Common. Arbitrary Object conversion may be stateful or throw on second invocation. No archived original execution permitted; this helper and transitive parents must remain OPEN pending bounded decision.')
   same(expected,untext(phase(code('UdfScalarUtilitiesOptions38',n['name']))),n['name'])
   arg='optimizerArg' if n['name'].startswith('Apply') else 'arg'
   extra=', gradientMode' if n['name'].startswith('Apply') else ', defaultValue' if n['name']=='NormalizeMixedModelResidualCovarianceName' else '' if n['name']=='NormalizeMixedModelOptionToken' else ', analysisLabel'
   call=('' if n['kind']=='SubBlock' else 'Return ')+'Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.'+n['name']+'(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+arg+')'+extra+')'
   same(phase(entry_text),call,'Native strict mixed-model '+n['name']);mode='COMPLETE_STRICT_ORIGINAL_OPTIONS_AND_NATIVE_ONE_TIME_CONVERSION_BYREF_EXCEPTION_CONTEXT'
  else:raise AssertionError('Unclassified original/current complete parent '+n['name'])
  expected=entry_text.replace('\r\n','\n')
  used=[]
  for move in moves:
   if move['path']==n['path'] and move['original_segment'] in expected:
    assert expected.count(move['original_segment'])==1;expected=expected.replace(move['original_segment'],move['native_call'],1);used.append(move['name'])
  same(expected,c['current_whole_body'],'Entire current native '+n['name'])
  rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],name=n['name'],mode=mode,moved_segments=used,entry_view=entry_view,complete_body_reconciled=True))
 except (AssertionError,ValueError) as e:unresolved.append(dict(original_id=c['original_id'],current_id=c['current_id'],name=n['name'],reason=str(e)))

move_rows=[]
for move in moves:
 owner=move['destination'];actual=phase(code(owner,move['name']))
 kind=move.get('kind','EXTRACTION')
 if kind=='TYPED_PREPARED_ROWS_AND_NATIVE_NAME_METADATA_COMPOSITE':
  expected=move['original_row_prefix']
  for a in move['aliases']:expected=expected.replace(a['before'],a['after'])
  expected=move['portable_prefix']+'\n'+expected+'\n'+move['portable_projection']
  same(expected,actual,'Entire typed LMM row phase')
  # All original native object-initializer aliases remain explicitly in the
  # native projection; only the six prepared data fields cross the DTO.
  original_init=move['original_segment'][move['original_segment'].index('            imported = New LmmImportedInputs With {'):]
  for old,new in [('.Y = yList.ToArray()','.Y = prepared.Y'),('.X = xValues','.X = prepared.X'),('.Z = zValues','.Z = prepared.Z'),('.Subject = subjectList.ToArray()','.Subject = prepared.Subject'),('.Visit = finalVisit','.Visit = prepared.Visit'),('.DroppedRows = dropped','.DroppedRows = prepared.DroppedRows')]:original_init=original_init.replace(old,new)
  current_init=move['native_call'][move['native_call'].index('            imported = New LmmImportedInputs With {'):]
  same(original_init,current_init,'Native LMM name/letter aliases and publication')
 elif kind=='PROPERTY_GETTER_VALUE_PROJECTION':
  same(actual,move['portable_segment'],'Entire ObservationCount')
  same(move['original_segment'].replace('Y','values'),actual,'Original N/Get count value macro')
 else:
  expected=move['original_segment']
  for alias in move['aliases']:expected=expected.replace(alias['before'],alias['after'])
  if move['name']=='TryBuildExpandedDesign':expected=expected.replace('regression.MixedModelFrontEndHelpers.DefaultNames','Global.BESHStatNG.ExcelCommon.MixedModelUDFsMetadata38.DefaultNames')
  if move['portable_suffix']:expected+='\n'+move['portable_suffix']
  same(expected,actual,'Entire moved portable '+move['name'])
 move_rows.append(dict(path=move['path'],host=move['host'],name=move['name'],destination=owner,source_identity=api[owner,move['name']]['symbol_id'],kind=kind,complete_portable_body_reconciled=True))

file_rows=[]
for path in sorted({r['path'] for r in moves}):
 expected=(ROOT/preimages[path]['preimage']['path']).read_text(encoding='utf-8-sig')
 for move in [r for r in moves if r['path']==path]:
  assert expected.count(move['original_segment'])==1;expected=expected.replace(move['original_segment'],move['native_call'],1)
 same(expected,(ROOT/path).read_text(encoding='utf-8-sig'),'Entire native file '+path)
 file_rows.append(dict(path=path,before=preimages[path]['before'],preimage=preimages[path]['preimage'],current=pin(ROOT/path),complete_source_reconstructed=True))

# The existing Common fallback is called only for a strictly positive design
# width. Bind the actual original helper and entire existing Common loop, rather
# than treating its prior MMRM owner as authorization for any MMRM parent work.
adj=load(PROOF/'adjacent-dependency-contracts-v1.json')['contracts']
default=next(c for c in adj if c['source_record']['name']=='DefaultNames')
old_phase=phase(default['before_whole_body'])
old_suffix=old_phase[old_phase.index('Dim names(count - 1) As String'):]
prepared=phase(code('MixedModelUDFsMetadata38','DefaultNames')).replace('Math.Max(0, count)','count').replace('names.Length - 1','count - 1')
same(old_suffix,prepared,'Positive width fallback loop/name/culture')
assert 'designBuild.ExpandedPredictorMatrix.GetLength(1) <= 0' in code('UdfLmmInputPreparation38','TryBuildExpandedDesign')
doc=dict(status='COMPLETE_ORIGINAL_ENTRY_CURRENT_AND_PORTABLE_COMPOSITES_PROVED' if not unresolved else 'PARTIAL_SOURCE_COMPOSITES_WITH_EXPLICIT_OPEN_OBLIGATION',rows=rows,unresolved=unresolved,moves=move_rows,whole_native_files=file_rows,positive_width_fallback=dict(native_dependency=default['source_record'],portable=api['MixedModelUDFsMetadata38','DefaultNames'],guard='Positive design width rejected before fallback; exact allocation/copy/name loop and invariant culture on reachable domain'),Release=DEFERRED,original_queries=0)
write_json(OUT,'source-composition-proof-v1.json',doc)
print('Whole source composites',len(rows),'proved;',len(unresolved),'unresolved;',len(move_rows),'portable moves',flush=True)
assert {r['name'] for r in unresolved}=={'NormalizeMixedModelResidualCovarianceName'}
if unresolved:print(json.dumps(unresolved,indent=2))
