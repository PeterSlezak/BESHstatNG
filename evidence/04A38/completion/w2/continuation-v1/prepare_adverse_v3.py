from common import *
s=(HERE/'test_p1_v2.py').read_text().replace('def test_changed_export_default(self):','def test_changed_export_name(self):')
extra=''' def test_changed_optional_default(self):
  def mutate(b):
   d=json.loads(b);a=next(a for e in d['exports'] for a in e['arguments'] if a['optional'] and a['has_default']);a['default_type']='System.Double';a['default_value']=0.05;return json.dumps(d).encode()
  self.rejects(self.here+'inspection/p1-v5/Inspect38-Release.json',mutate)
'''
s=s.replace("if __name__=='__main__':unittest.main()",extra+"if __name__=='__main__':unittest.main()")
with (HERE/'test_p1_v3.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/adverse-name-label-correction.json',dict(previous='test_p1_v2.test_changed_export_default actually mutated effective_excel_name. It is valid export-name corruption evidence but is not optional-default corruption evidence; no claim of that condition is made.',correction='Keep v2 source/results unchanged; v3 labels the name mutation correctly and adds a separate actual optional/default-value mutation. Run15 conditions. No original semantic query or test-project change.'))
