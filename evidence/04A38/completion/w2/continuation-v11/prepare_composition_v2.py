from common import *
source=(HERE/'verify_composition_v1.py').read_text(encoding='utf-8')
before="    header=re.search(r'\\)\\s+As [^\\n]+\\n',text)\n    need(header is not None,'Missing actual typed source header')\n    return text[header.end():text.rfind('End Function')].strip()"
after="""    begin=text.index('(');depth=0;end=None
    for index in range(begin,len(text)):
        if text[index]=='(':depth+=1
        elif text[index]==')':
            depth-=1
            if depth==0:end=index;break
    need(end is not None,'Missing actual typed source header')
    end=text.index('\\n',end)+1
    return text[end:text.rfind('End Function')].strip()"""
assert before in source;source=source.replace(before,after)
with (HERE/'verify_composition_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
fragment=(HERE/'fragment_correspondence_v1.py').read_text(encoding='utf-8').replace('verify_composition_v1','verify_composition_v2')
with (HERE/'fragment_correspondence_v2.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(fragment)
save('composition-parser-first-failure-v1.json',dict(before=pin(HERE/'verify_composition_v1.py'),after=pin(HERE/'verify_composition_v2.py'),
    observed_exit_code=1,observed_error='ValueError: Original full helper does not reconstruct BuildClusterPreprocessingTable',
    cause='The new proof parser matched the first array-parameter closing parenthesis followed by As instead of the full multiline method parameter list.',
    correction='Parse balanced method-parameter parentheses before taking the body; retain the exact full-body/branch comparison. No source or test assertion changed.',original_queries=0))
result=command('composition-v2',[sys.executable,'-B',str(HERE/'fragment_correspondence_v2.py')])
save('composition-execution-v2.json',clean(result));assert result['exit_code']==0
