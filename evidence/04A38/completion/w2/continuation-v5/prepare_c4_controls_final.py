from common import *
text=(HERE/'prepare_c4_controls.py').read_text(encoding='utf-8').replace("C5/'entry.json'","C5/'entry-v2.json'").replace('C4_controls_projected.py','C4_controls_projected_v2.py').replace("'retained-c4-controls'","'retained-c4-controls-v2'").replace("'retained-C4-controls.json'","'retained-C4-controls-v2.json'").replace('four C5 modified','seven C5 modified')
with (HERE/'prepare_c4_controls_final_run.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
exec(compile(text,str(HERE/'prepare_c4_controls_final_run.py'),'exec'))
