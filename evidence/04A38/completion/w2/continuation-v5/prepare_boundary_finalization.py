from common import *
def emit(name,text):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
for old,new in [('reconcile_boundary.py','reconcile_boundary_final.py'),('verify_ownership_v3.py','verify_ownership_final.py')]:
    emit(new,(HERE/old).read_text(encoding='utf-8').replace('from verify_slice_v3 import','from verify_slice_v4 import'))
# Final text caller census/probes use new files; no earlier record is rewritten.
text=(HERE/'run_text_domains_v5.py').read_text(encoding='utf-8').replace('text-domains-v5','text-domains-v6').replace('text-domain-v5-','text-domain-v6-').replace("'-v5-'","'-v6-'").replace("'-v5.json'","'-v6.json'").replace('text-call-sites.json','text-call-sites-v2.json')
emit('run_text_domains_v6.py',text)
text=(HERE/'record_text_domains_v3.py').read_text(encoding='utf-8').replace('text-call-sites.json','text-call-sites-v2.json').replace("save('text-caller-domains.json'","save('text-caller-domains-v2.json'")
emit('record_text_domains_final.py',text)
text=(HERE/'verify_text_domains_v2.py').read_text(encoding='utf-8').replace('from verify_slice_v2 import','from verify_slice_v4 import').replace('text-call-sites.json','text-call-sites-v2.json').replace('text-caller-domains.json','text-caller-domains-v2.json').replace('cells-v4-v5-','cells-v5-v6-')
emit('verify_text_domains_final.py',text)
text=(HERE/'test_checkpoint_final.py').read_text(encoding='utf-8').replace('from verify_slice_v2 import','from verify_slice_v4 import').replace('from verify_ownership_v2 import','from verify_ownership_final import').replace('body-contracts-v2.json','body-contracts-v3.json').replace('inspection/final-v3/','inspection/final-v4/').replace('dependency-metadata-v8/current-','dependency-metadata-v9/current-').replace('cell-probe-v2-after-','cell-probe-v3-after-').replace('compatibility-cells-v4.json','compatibility-cells-v5.json')
for name in ['historical-ownership','current-ownership','ownership-summary','original-fragment-parent-crosswalk','remaining-work-queue']:
    text=text.replace(name+'-v2.json',name+'-v3.json')
text=text.replace('test_checkpoint_v2','test_checkpoint_final')
extra='''    def test_resetting_native_consumer_rejected(self):
        def change(d):
            m=next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.ExcelArgNumeric' and m['member_name']=='TryGetWholeNumber')
            op=next(i for i in m['instructions'] if '.TryWholeNumberPrepared(' in str(i['operand']))
            op['operand']=op['operand'].replace('.TryWholeNumberPrepared(','.TryWholeNumber(')
        self.current(change,'resetting API')

    def test_erased_alias_adverse_evidence(self):
        self.mutate('alias-probe-before-Debug.json',lambda d:d['cases'][0].update(output=17),'adverse evidence lost')

    def test_forged_alias_output(self):
        self.mutate('alias-probe-after-Debug.json',lambda d:d['cases'][0].update(output=0),'Aliased output side effects')

    def test_unsupported_generated_type_alias(self):
        self.mutate('compiler-generated-aliases-final.json',lambda d:d['aliases'].update({'Fake':'Other'}),'Unsupported generated-name alias')

    def test_authenticated_census_cannot_drop_open(self):
        self.mutate('current-universe-final-v4.json',lambda d:d['members'].pop(),'Authenticated current census',ownership)

'''
text=text.replace("if __name__=='__main__':",extra+"\nif __name__=='__main__':")
emit('test_checkpoint_boundary.py',text)
text=(HERE/'record_checkpoint_final.py').read_text(encoding='utf-8').replace('from verify_ownership_v2 import','from verify_ownership_final import').replace('from verify_preservation_v2 import','from verify_preservation_v3 import').replace('from verify_derivative_v2 import','from verify_derivative_v3 import')
for name in ['independent-ownership-proof','preservation-final','ownership-summary','validation','fragment-coverage','readiness','git-closeout','command-receipts']:
    text=text.replace(name+'-v2.json',name+'-v3.json')
text=text.replace('source-transitions-v3.json','source-transitions-v4.json')
emit('record_checkpoint_boundary.py',text)
text=(HERE/'prepare_diagnostic_final.py').read_text(encoding='utf-8').replace('full-managed-Debug-v3.json','full-managed-Debug-v4.json').replace('full-managed-Debug-public-v2.json','full-managed-Debug-public-v3.json').replace('diagnostic-public-derivative-v2.json','diagnostic-public-derivative-v3.json').replace('verify_derivative_v2.py','verify_derivative_v3.py').replace('diagnostic-derivative-proof-v2.json','diagnostic-derivative-proof-v3.json')
emit('prepare_diagnostic_boundary.py',text)
# Late full source/test receipts are still pending; exact corrected Python accounting
# is produced separately after the unchanged targeted privacy rerun completes.
text=(HERE/'verify_publication.py').read_text(encoding='utf-8').replace('from verify_ownership_v2 import','from verify_ownership_final import').replace('from verify_preservation_v2 import','from verify_preservation_v3 import').replace('readiness-v2.json','readiness-v3.json').replace('validation-v2.json','validation-v3.json')
emit('verify_publication_final.py',text)
text=(HERE/'publish.py').read_text(encoding='utf-8').replace('source-transitions-v3.json','source-transitions-v4.json').replace('suffix-v2 ledgers plus source-transitions-v3 and cells-v4','suffix-v3 ledgers plus source-transitions-v4 and cells-v5')
emit('publish_final.py',text)
text=(HERE/'export_and_verify.py').read_text(encoding='utf-8').replace('from verify_publication import','from verify_publication_final import').replace("/'verify_publication.py'","/'verify_publication_final.py'")
emit('export_and_verify_final.py',text)
