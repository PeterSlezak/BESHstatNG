from common import *
source=(HERE/'prepare_review_inputs.py').read_text(encoding='utf-8')
source=source.replace("'review-inputs'","'review-inputs-v2'").replace("'review-inputs.json'","'review-inputs-v2.json'")
source=source.replace(".read_text(encoding='utf-8-sig')",".read_bytes().decode('utf-8-sig')")
p=HERE/'prepare_review_inputs_v2.py'
with p.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('attempts/review-span-reader-v1.json',dict(status='REVIEW_INPUT_EXTRACTION_REJECTED_BEFORE_ANY_CLOSURE_OR_SOURCE_EDIT',
    failure='Path.read_text normalized CRLF before applying Roslyn UTF16 source spans, causing current body offsets to cut wrong text',
    retained=pin(HERE/'review-inputs.json'),correction='v2 decodes actual source bytes without newline translation, preserving original/current syntax span coordinates',closures_from_failed_reader=0))
exec(compile(source,str(p),'exec'))
