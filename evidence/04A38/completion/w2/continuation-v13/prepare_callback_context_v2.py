from common import *
import ast
source=HERE/'callback_context_v1.py';value=source.read_text(encoding='utf-8')
try:ast.parse(value)
except SyntaxError as error:diagnostic=dict(type=type(error).__name__,message=error.msg,line=error.lineno,text=error.text)
else:raise AssertionError('Expected preserved unmatched-parenthesis source')
old='enumerate(zip(oldchildren,newchildren))):';assert value.count(old)==1
target=HERE/'callback_context_v2.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value.replace(old,'enumerate(zip(oldchildren,newchildren)):'))
save('callback-proof-source-correction-v1.json',dict(before=pin(source),after=pin(target),first_attempt=diagnostic,scope='Bookkeeping Python syntax only; rejected before execution. No production/test or original reference source changed.',original_queries=0))
result=command('callback-context-v2',[sys.executable,'-B',str(target)])
save('callback-context-execution-v2.json',clean(result));assert result['exit_code']==0
