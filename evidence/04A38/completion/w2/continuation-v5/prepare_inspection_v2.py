from common import *
text = (HERE / 'inspect_final.py').read_text(encoding='utf-8')
text = text.replace("'final-inspection'", "'final-inspection-v2'").replace("'inspection/final'", "'inspection/final-v2'")
text = text.replace("'final-'", "'final-v2-'").replace("'final-original-metadata-'", "'final-v2-original-metadata-'")
text = text.replace("'final-current-syntax'", "'final-v2-current-syntax'").replace("'current-universe-final.json'", "'current-universe-final-v2.json'")
old = "            assert json.loads(dst.read_bytes()) == json.loads((PREVIOUS / 'metadata-v6/parametric-v4' / ('original-' + config + '.json')).read_bytes())"
new = """            actual = json.loads(dst.read_bytes())
            prior = json.loads((PREVIOUS / 'metadata-v6/parametric-v4' / ('original-' + config + '.json')).read_bytes())
            # The newly compiled reader's own MVID/hash changes; original DLLs never may.
            for data in [actual, prior]:
                data['loaded_modules'] = [a for a in data['loaded_modules'] if a['file_name'] != 'MethodMetadata38.exe']
            assert actual == prior"""
assert old in text
text = text.replace(old, new).replace("save('inspection-final.json'", "save('inspection-final-v2.json'")
with (HERE / 'inspect_final_v2.py').open('x', encoding='utf-8', newline='\n') as stream:
    stream.write(text)
