from common import *
text=(HERE/'test_checkpoint_v2.py').read_text(encoding='utf-8')
extra='''    def test_added_boxed_value_copy(self):
        def change(d):
            number=next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.ExcelArgNumeric' and m['member_name']=='TryGetDouble')
            portable=next(m for m in d['methods'] if m['member_name']=='ConvertFiniteNumber')
            copy=next(i for i in portable['instructions'] if '.GetObjectValue(' in str(i['operand']))
            number['instructions'].insert(0,dict(copy))
        self.current(change,'boxed-value copy')

    def test_portable_input_write(self):
        self.current(lambda d:next(m for m in d['methods'] if m['member_name']=='ConvertFiniteNumber')['instructions'].insert(0,dict(op='stind.ref',operand=None)),'input alias mutated')

    def test_portable_VB_conversion_helper(self):
        self.current(lambda d:next(m for m in d['methods'] if m['member_name']=='CellToTrimmedText' and m['declaring_type']=='BESHStatNG.ExcelCommon.UdfCellConversion38')['instructions'].insert(0,dict(op='call',operand='METHOD:Microsoft.VisualBasic.CompilerServices.Conversions.ToString')),'portable VB conversion helper')

'''
text=text.replace("if __name__=='__main__':",extra+"\nif __name__=='__main__':")
with (HERE/'test_checkpoint_final.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
r=command('independent-c5-final-controls',[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p','test_checkpoint_final.py','-v'])
save('independent-controls-final.json',dict(command=clean(r),passed=r['exit_code']==0,expected_test_count=27,
    scope='Original24 C5 controls plus boxed-copy count, ByRef nonmutation and portable VB-helper rejection; only final cells-v4 code/proofs'))
assert r['exit_code']==0
