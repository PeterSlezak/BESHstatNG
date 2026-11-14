from common import *
import re
# Execute only the definition/build portion; preserve every previous dossier.
source=(HERE/'build_review_v2.py').read_text(encoding='utf-8')
source=source.replace("save('body-contracts-v2.json'","save('body-contracts-v3.json'")
source=source.replace("('TryGetBinary01','TryBinary',","('TryGetBinary01','TryBinaryPrepared',")
source=source.replace("('TryGetWholeNumber','TryWholeNumber',","('TryGetWholeNumber','TryWholeNumberPrepared',")
source=source.replace("('TryGetOptionalPositiveInteger','TryPositiveInteger',","('TryGetOptionalPositiveInteger','TryPositiveIntegerPrepared',")
source=source.replace("[(SCALAR, portable)]", "[(SCALAR, portable)] + ([(SCALAR, portable.removesuffix('Prepared'))] if portable.endswith('Prepared') else [])")
source=source.replace("[(NUMERIC,name)]", "[(NUMERIC,name+'Prepared'),(NUMERIC,name)]")
source=source.replace('Write zero before parsing and again at Common entry, with no intervening externally visible operation.','Write zero only at the native entry before parsing. A custom converter can mutate the aliased caller output; prepared validation does not reset it after conversion. The existing public Common API retains its reset wrapper.')
source=source.replace('Assign supplied default before missing check; finite-double conversion before rounding validation. Common repeats default assignment without an intervening callback, receives a present finite nullable and missing=False, so added nullable/finite guards cannot alter the original path.', 'Assign supplied default before missing check and native finite-double conversion. Conversion callbacks can mutate caller output; the prepared finite-number phase never repeats that reset. The existing public Common API keeps its original missing/nullable/finite guards and entry reset.')
source=source.replace('Write -1 before missing guard; numeric parser rejects Boolean without conversion,', 'Write -1 before missing guard and conversion. The prepared phase never resets an aliased output after a conversion callback; failures and overflow preserve its writes. Existing public Common API keeps its entry reset. Numeric parser rejects Boolean without conversion,')
source=source.replace('Common receives original typed arrays from native Get2D and native primitive callbacks;', 'The native adapter calls a no-reset prepared phase after acquisition; its original output resets remain before Get2D. The existing public Common API wraps the identical prepared body with its original entry resets. Common receives original typed arrays from native Get2D and native primitive callbacks;')
with (HERE/'build_review_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
exec(compile(source,str(HERE/'build_review_v3.py'),'exec'))

def member(text,name):
    m=re.search(r'^([ \t]*)Public Shared Function '+name+r'\b',text,re.M);assert m
    end=text.index('\n'+m[1]+'End Function',m.end())+len('\n'+m[1]+'End Function')
    return text[m.start():end]
rows=[]
for path,names in [('BESHStat.Excel.Common/src/Analysis/UdfScalarChoice38.vb',['TryBinary','TryWholeNumber','TryPositiveInteger']),
                   ('BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb',['TryGetGroupedCompleteMatrix','TryGetNestedThreeColumnData'])]:
    before=next(r for r in json.loads((HERE/'entry-v2.json').read_bytes())['source_before'] if r['live']['path']==path)
    old=(ROOT/before['preimage']['path']).read_text(encoding='utf-8-sig');new=(ROOT/path).read_text(encoding='utf-8-sig')
    for name in names:
        rows.append(dict(path=path,name=name,old_public_body=member(old,name),new_public_body=member(new,name),prepared_body=member(new,name+'Prepared'),
                         preimage=before['preimage'],current_source=pin(ROOT/path),contract='Public entry reset unchanged; Native consumers call no-reset prepared phase after original reset/conversion/acquisition'))
save('prepared-phase-source-proof-inputs.json',dict(rows=rows,original_queries=0))
save('compiler-generated-aliases-final.json',dict(aliases={'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__6-0':'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__8-0'},
    reason='Two public reset wrappers precede an otherwise unchanged portable matrix-header callback; compiler enclosing-method ordinal changed. Every field, constructor, lambda instruction and consumer must compare exactly after this one explicit type-name substitution.',
    historical_ids='UNCHANGED',new_native_callbacks=0,source_file='BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'))
