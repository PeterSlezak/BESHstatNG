from common import *
source=PREVIOUS/'build_contracts_v3.py';value=source.read_text(encoding='utf-8')
value=value.replace('from proof_kernel_v4 import','from proof_kernel_v5 import')
value=value.replace("mapping=map_current(old['members'],u['members'])","mapping=map_current(old['members'],u['members'],json.loads((HERE/'callback-identity-context-v1.json').read_bytes())['mapping'])")
needle="    if len(candidates)==1:\n        hmap[candidates[0]['symbol_id']].append(h)";assert needle in value
new="""    if len(candidates)>1:
        binding=[r for r in json.loads((HERE/'original-km-callback-bindings-v1.json').read_bytes())['rows'] if r['configuration']=='Debug' and r['original_record']['symbol_id']==o['symbol_id']]
        need(len(binding)==1,'Ambiguous historical callback without exact original/current CIL binding')
        candidates=[n for n in candidates if n['symbol_id']==binding[0]['current_id']]
    if len(candidates)==1:
        hmap[candidates[0]['symbol_id']].append(h)"""
value=value.replace(needle,new)
value=value.replace("    new=oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')", "    was_open=oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')\n    blocked=n['name'] in {'GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS'}\n    new=was_open and not blocked")
value=value.replace("        new_support=new,carried_proof=", "        proof_status='OPEN_BYREF_PARITY_DEPENDENCY' if blocked else 'COMPLETE_SOURCE_AND_COMPILED_CONTEXT',blocked_reason='Existing GuessPositiveClass double-reset alias difference; current-only measured, original prediction source-backed. No complete-parent closure from ordinary frozen compatibility.' if blocked else None,\n        new_support=new,carried_proof=")
value=value.replace('Retained complete v11 authority','Retained complete v12 authority').replace('../continuation-v11/','../continuation-v12/')
value=value.replace("portable,new_public_apis=5","portable,new_public_apis=8")
value=value.replace("new_historical_ids=[c['original_id'] for c in contracts+callbacks if c['new_support'] and c['original_id']],", "new_historical_ids=[c['original_id'] for c in contracts+callbacks if c['new_support'] and c['original_id']]+[json.loads((HERE/'portable-historical-callback-proof-v1.json').read_bytes())['original_id']],")
with (HERE/'build_contracts_v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
save('contract-tools-source-crosswalk-v1.json',dict(before=pin(source),after=pin(HERE/'build_contracts_v4.py'),
    scope='Exact existing source/parameter/return/actual ldftn proof writer, with proved KM mapping and explicit original-only portable callback. Four ByRef-dependent parents remain OPEN; never promote parent from a helper call or compatible ordinary inputs.',original_queries=0))
reviews={
 'DA_MEANS':'Handle lookup precedes scale/header conversion. Borrow original model; exact existing Common means report and native materializer; original/working scale labels, defaults, shape and error order unchanged.',
 'DA_PREPROCESS':'Handle, PreparedData and variable/location/scale reads in original order; existing Common preprocessing report; native header conversion and exception identity remain.',
 'DA_COVARIANCE':'Handle and native label coercion precede pooled/QDA selection. Original title/startsWith scan, ordinal first group match, missing-group native throw before header; borrowed covariance and cache identity retained.',
 'DA_FUNCTIONS':'Exact source retains linear-only note, wrapped title lookup, header ordering and native exception text; full scanner/report/Core dependencies measured.',
 'DA_CANONICAL':'Exact source retains canonical title/availability note and native materialization. No eigenvalue, orientation, inference or formatting correction.',
 'DA_CANONCOEF':'Exact source keeps canonical coefficient title, note branch, default/header and error order; Core coefficient engine unchanged.',
 'DA_CENTROIDS':'Exact source keeps centroid title/availability note, group/axis order and native header/materialization.',
 'DA_CONFUSION':'Exact source preserves training/validation aliases, missing confusion notes, native invalid-source throw, Core ToObjectTable and header evaluation order; cached result references unchanged.',
 'DA_CASEWISE':'Exact source preserves source selection, missing-result note, maxRows conversion before header, row indices/labels, posterior/distance columns and reference identity.',
 'DA_PREDICT':'Exact source keeps handle-first acquisition, predictor count, label/actual-group alignment and Object copy, current Core PredictDetailed, native cache ownership and result materialization.',
 'DA_REMOVED':'Exact source retains original removed indices/labels, empty-note and header conversion order; no renumbering or missing-value change.',
 'FindWrappedDiscriminantTable':'Native Nothing/model.wrapResults/None order remains. Common scans one real ResultTable at a time, preserving empty dimensions, host cell conversion, ordinal exact/prefix matching and early return before later bad tables. Actual enumerable/CIL exception boundary recorded.',
 'BuildDiscriminantMeansTable':'Original full means body reconstructs from existing Common Raw projection and native materializer; original/working branches, variable/group order and array bounds retained.',
 'GetDiscriminantPredictionResult':'Whole exact native source including Nothing-before-normalize and all training/validation aliases; invalid-source ArgumentException stays in Windows. Borrowed result identity unchanged.',
 'HIST_BINS':'Acquisition/default bin rule precede original grouped iteration. Empty groups/bins skip; actual named Windows histogram delegate invokes same Core engine; labels after computation, raw boxes, stacking and host error unchanged.',
 'HIST_NORMAL':'Preserves minimum two values, original histogram then invariant midpoint conversion then overlay, skip/error precedence, exact group labels/shape and Windows engine delegates.',
 'KM_CURVE':'Alpha before acquisition, count/empty branches, unchanged Core engine and alpha argument. Exact step preparation plus two separately indexed count/fill casts; native error/logger boundary retained.',
 'CALIB_POINTS':'Exact native source preserves y/p/weights acquisition and validation order, default bins/method, actual Core engine, existing typed report and numeric ExcelErrorNA cell behavior. No scientific repair.',
 'BuildKaplanMeierStepTable':'Entire original body explicitly reconstructed from Common loops and two native casts. Separate actual ldftn and parameter loads prove original count and fill identities, repeated enumeration, origin/plateau/drop rows, notes, labels, bounds and cast errors.',
 'StackHomogeneousTables':'Empty/None sentinel allocation stays native. Exact nonempty original body in Common preserves first header, empty body parts, widths, lower-bound faults and Object references without normalization.',
 'GetHistogramBinRule':'Original native AsString policy before exact Common normalization and legacy VB-text CaseToken cases; all rule/default strings and invalid-option throw behavior retained.',
 'ParseRocDirection':'Native AsString remains; exact Common direction grammar, aliases/default and failure semantics are fully source/compiled reconciled.',
 'TryGetSurvivalRecords':'Native resets/acquisition/dimensions/group presence remain. Exact acquired-row loop in Common keeps finite/time-negative-before-status precedence, group labels/row order, Core record creation, ByRef partial state and false/null results.',
 'IsStatusBinaryCell38':'Current-only full typed host status predicate; local Integer ByRef receives the original numeric policy result, only Boolean escapes. Source, actual body, exceptions and all callers independently measured.',
 'GuessPositiveClass':'OPEN: current Common reset occurs after native text conversion, unlike archived original. Current-only alias probe confirms lost callback side effect; parent and dependent ROC contracts are recorded but not supported.',
 'TryGetRocInput':'OPEN: extraction preserves current acquired-row behavior and ordinary frozen compatibility, but full original ByRef positiveLabel parity depends on unresolved GuessPositiveClass.',
 'ROC_POINTS':'OPEN dependency: full source/CIL retained and ordinary current-only comparisons exact, but GuessPositiveClass/TryGetRocInput full-body parity remains unclosed.',
 'ROC_STATS':'OPEN dependency: complete source/engine/report boundaries recorded, while unresolved ROC import ownership prevents a supported parent claim.'}
save('whole-parent-reviews-v1.json',reviews)
print('Whole-body proof recipes prepared with four explicit fail-closed ByRef dependency parents')
