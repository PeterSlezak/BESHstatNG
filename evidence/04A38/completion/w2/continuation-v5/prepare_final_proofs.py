from common import *
def emit(name,text):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
# Capture the final test correction as well as the final production image.
trans=json.loads((HERE/'source-transitions-v2.json').read_bytes())
for r in trans['files']:
    dst=HERE/'source-after/final-v3'/(r['path']+'.txt');dst.parent.mkdir(parents=True,exist_ok=True)
    dst.write_bytes((ROOT/r['path']).read_bytes());r['after']=pin(ROOT/r['path']);r['snapshot']=pin(dst)
trans['intermediate_refinement']=json.loads((HERE/'attempts/conversion-il-refinement.json').read_bytes())
save('source-transitions-v3.json',trans)
# build_review_v2.py is run before this script; bind its complete new dossier.
review_hash=sha((HERE/'body-contracts-v2.json').read_bytes())
text=(HERE/'verify_slice.py').read_text(encoding='utf-8')
import re
text=re.sub(r"REVIEW_SHA256 = '[0-9a-f]+'","REVIEW_SHA256 = '"+review_hash+"'",text)
text=text.replace('body-contracts.json','body-contracts-v2.json').replace('inspection/final-v2/','inspection/final-v3/')
text=text.replace('build-cells-v3-','build-cells-v4-').replace('completion-w2-cont5/cells-v3','completion-w2-cont5/cells-v4')
text=text.replace('compatibility-cells-v3.json','compatibility-cells-v4.json').replace('source-transitions.json','source-transitions-v3.json')
text=text.replace("'dependency-metadata-v7/current-'","'dependency-metadata-v8/current-'")
text=text.replace('cell-probe-after-','cell-probe-v2-after-').replace('cell-probe-before-','cell-probe-v2-before-').replace("len(probe['cases']) == 192","len(probe['cases']) == 198")
text=text.replace("tokens(original)[len(tokens(prefix)):]==", "tokens(original)[len(tokens(prefix)):]==")
text=text.replace("tokens(original)[len(tokens(prefix)):] == tokens(body(cell['portable_bodies'][0]['whole_body']))", "tokens(original.replace('CStr(v)', 'DirectCast(v, String)'))[len(tokens(prefix)):] == tokens(body(cell['portable_bodies'][0]['whole_body']))")
needle="        expanded_old = load(PREFIX + 'dependency-metadata-v7/original-' + config + '.json')"
insert="""        # Preserve the actual original boxed-value copy count, not merely outputs.
        copy_calls = lambda m: sum('.GetObjectValue(' in str(i['operand']) for i in m['instructions'])
        portable_number = lookup('BESHStatNG.ExcelCommon.UdfCellConversion38','ConvertFiniteNumber')
        need(copy_calls(original_method) == 1 and copy_calls(method) == 0 and copy_calls(portable_number) == 1, 'Unintended boxed-value copy introduced')
        for pure in portable:
            need(not any('Microsoft.VisualBasic.' in str(i['operand']) for i in pure['instructions']), 'New portable VB conversion helper introduced')
            if pure['member_name'] in ['ConvertFiniteNumber','CellToTrimmedText']:
                need(pure['parameter_types'] == ['System.Object&'] and not any(i['op'] in ['stind.ref','starg','starg.s'] for i in pure['instructions']), 'Portable input alias mutated')
"""
assert needle in text
text=text.replace(needle,insert+needle)
emit('verify_slice_v2.py',text)
text=(HERE/'reconcile_v2.py').read_text(encoding='utf-8').replace('from verify_slice import inspect','from verify_slice_v2 import inspect')
text=text.replace("assert json.loads((HERE / 'independent-slice-proof.json').read_bytes()) == proof","save('independent-slice-proof-v2.json',proof)")
text=text.replace('current-universe-final-v2.json','current-universe-final-v3.json').replace("HERE / 'body-contracts.json'","HERE / 'body-contracts-v2.json'")
for name in ['historical-ownership','current-ownership','ownership-summary','original-fragment-parent-crosswalk','fragment-coverage','remaining-work-queue','per-family-ownership']:
    text=text.replace("save('"+name+".json'","save('"+name+"-v2.json'")
emit('reconcile_final.py',text)
text=(HERE/'verify_ownership.py').read_text(encoding='utf-8').replace('from verify_slice import','from verify_slice_v2 import').replace('current-universe-final-v2.json','current-universe-final-v3.json')
for name in ['historical-ownership','current-ownership','ownership-summary','original-fragment-parent-crosswalk','fragment-coverage','remaining-work-queue']:
    text=text.replace("PREFIX+'"+name+".json'","PREFIX+'"+name+"-v2.json'")
emit('verify_ownership_v2.py',text)
text=(HERE/'verify_preservation.py').read_text(encoding='utf-8').replace("C5+'source-transitions.json'","C5+'source-transitions-v3.json'")
emit('verify_preservation_v2.py',text)
text=(HERE/'record_edges.py').read_text(encoding='utf-8').replace('body-contracts.json','body-contracts-v2.json').replace("HERE / 'dependency-metadata-v7' / ('current-'+c+'.json')","HERE / 'dependency-metadata-v8' / ('current-'+c+'.json')")
text=text.replace("save('production-consumer-edges.json'","save('production-consumer-edges-v2.json'").replace("save('entry-discrepancies.json'","save('entry-discrepancies-v2.json'")
emit('record_edges_v2.py',text)
text=(HERE/'test_checkpoint.py').read_text(encoding='utf-8').replace('from verify_slice import','from verify_slice_v2 import').replace('from verify_ownership import','from verify_ownership_v2 import').replace('body-contracts.json','body-contracts-v2.json').replace('inspection/final-v2/','inspection/final-v3/').replace('dependency-metadata-v7/current-','dependency-metadata-v8/current-').replace('cell-probe-after-','cell-probe-v2-after-')
text=text.replace('compatibility-cells-v3.json','compatibility-cells-v4.json')
for name in ['historical-ownership','current-ownership','ownership-summary','original-fragment-parent-crosswalk','remaining-work-queue']:
    text=text.replace("'"+name+".json'","'"+name+"-v2.json'")
emit('test_checkpoint_v2.py',text)
text=(HERE/'report_managed_final.py').read_text(encoding='utf-8').replace('w2c5-validation-v2','w2c5-validation-v3').replace('focused-cells-v4-','focused-cells-v4-v2-')
emit('report_managed_final_v2.py',text)
text=(HERE/'report_validation_final.py').read_text(encoding='utf-8')
text=text.replace("pys=[python_receipt('w2c5-validation-v2-'+s) for s in ['python-tools','python-help','python-scripts','common-python']]", "pys=[python_receipt('w2c5-validation-v2-'+s) for s in ['python-tools','python-help','python-scripts']]+[python_receipt('w2c5-validation-v3-common-python')]")
emit('report_validation_final_v2.py',text)
text=(HERE/'prepare_diagnostic.py').read_text(encoding='utf-8').replace('full-managed-Debug-v1.json','full-managed-Debug-v3.json').replace('full-managed-Debug-public-v1.json','full-managed-Debug-public-v2.json').replace('diagnostic-public-derivative.json','diagnostic-public-derivative-v2.json').replace('verify_derivative.py','verify_derivative_v2.py').replace('diagnostic-derivative-proof.json','diagnostic-derivative-proof-v2.json')
text=text.replace("source=(PREVIOUS/'verify_derivative_v2.py').read_text(encoding='utf-8')", "source=(PREVIOUS/'verify_derivative.py').read_text(encoding='utf-8').replace('diagnostic-public-derivative.json','diagnostic-public-derivative-v2.json')")
emit('prepare_diagnostic_final.py',text)
text=(HERE/'record_checkpoint.py').read_text(encoding='utf-8').replace('from verify_ownership import','from verify_ownership_v2 import').replace('from verify_preservation import','from verify_preservation_v2 import').replace('from verify_derivative import','from verify_derivative_v2 import')
for name in ['independent-ownership-proof','preservation-final','ownership-summary','validation','fragment-coverage','readiness','git-closeout','command-receipts']:
    text=text.replace("'"+name+".json'","'"+name+"-v2.json'")
text=text.replace('source-transitions.json','source-transitions-v3.json')
emit('record_checkpoint_final.py',text)
