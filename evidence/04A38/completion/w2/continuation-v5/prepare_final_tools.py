from common import *
def emit(name,text):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE/'inspect_final_v2.py').read_text(encoding='utf-8').replace('cells-v3','cells-v4').replace('final-v2','final-v3').replace('final-inspection-v2','final-inspection-v3')
text=text.replace("save('source-transitions.json'","save('source-transitions-v2.json'").replace("'source-after/final'","'source-after/final-v2'").replace("save('attempts/inspection-race.json'","save('attempts/final-inspection-v3.json'")
emit('inspect_final_v3.py',text)
text=(HERE/'run_full_validation.py').read_text(encoding='utf-8').replace('w2c5-validation-v1','w2c5-validation-v2').replace('full-managed-Debug-v1.json','full-managed-Debug-v2.json').replace('source-validation-v1.json','source-validation-v2.json').replace('common-python-validation-v1.json','common-python-validation-v2.json').replace('serial-validation-v1.json','serial-validation-v2.json')
emit('run_full_validation_v2.py',text)
text=(HERE/'build_review.py').read_text(encoding='utf-8').replace("save('body-contracts.json'","save('body-contracts-v2.json'")
text=text.replace('No array indexing/copy/mutation. Current-only probe','The portable ByRef argument aliases only the host local Object and is never assigned, preserving the original single CLR boxed-value copy before conversion. DirectCast on the TypeOf String branch replaces the VB conversion helper on its proven domain. No array indexing/copy/mutation. Current-only probe')
emit('build_review_v2.py',text)
text=(HERE/'CellProbe.cs').read_text(encoding='utf-8')
mutable='''    struct Mutable : IConvertible {
        public List<string> Calls; public int Writes;
        public TypeCode GetTypeCode(){Calls.Add("type");return TypeCode.Double;}
        public double ToDouble(IFormatProvider p){Writes++;Calls.Add("double:"+Writes+":"+(p==null?"null":((CultureInfo)p).Name));return 1.25;}
        public string ToString(IFormatProvider p){Writes++;Calls.Add("text:"+Writes+":"+(p==null?"null":((CultureInfo)p).Name));return "  mutable  ";}
        public bool ToBoolean(IFormatProvider p){throw new InvalidCastException();}
        public byte ToByte(IFormatProvider p){throw new InvalidCastException();}
        public char ToChar(IFormatProvider p){throw new InvalidCastException();}
        public DateTime ToDateTime(IFormatProvider p){throw new InvalidCastException();}
        public decimal ToDecimal(IFormatProvider p){throw new InvalidCastException();}
        public short ToInt16(IFormatProvider p){throw new InvalidCastException();}
        public int ToInt32(IFormatProvider p){throw new InvalidCastException();}
        public long ToInt64(IFormatProvider p){throw new InvalidCastException();}
        public sbyte ToSByte(IFormatProvider p){throw new InvalidCastException();}
        public float ToSingle(IFormatProvider p){throw new InvalidCastException();}
        public object ToType(Type t,IFormatProvider p){throw new InvalidCastException();}
        public ushort ToUInt16(IFormatProvider p){throw new InvalidCastException();}
        public uint ToUInt32(IFormatProvider p){throw new InvalidCastException();}
        public ulong ToUInt64(IFormatProvider p){throw new InvalidCastException();}
    }
'''
text=text.replace('    static object Value(object v){',mutable+'    static object Value(object v){')
text=text.replace('new Convertible{ThrowText=true}}','new Convertible{ThrowText=true},new Mutable{Calls=new List<string>()}}')
text=text.replace('var probe=input as Convertible;if(probe!=null)probe.Calls.Clear();','var probe=input as Convertible;var trace=probe!=null?probe.Calls:input is Mutable?((Mutable)input).Calls:null;if(trace!=null)trace.Clear();')
text=text.replace('var calls=probe==null?new string[0]:probe.Calls.ToArray();','var calls=trace==null?new string[0]:trace.ToArray();')
text=text.replace('array=input is Array?Value(input):null','mutable_writes=input is Mutable?(int?)((Mutable)input).Writes:null,array=input is Array?Value(input):null')
emit('CellProbe-v2.cs',text)
text=(HERE/'run_cell_probe.py').read_text(encoding='utf-8').replace('CellProbe.cs','CellProbe-v2.cs').replace('cell-probe-before','cell-probe-v2-before').replace('cell-probe-after','cell-probe-v2-after')
text=text.replace("'cell-probe-' + role", "'cell-probe-v2-' + role").replace("'cell-probe-comparison.json'","'cell-probe-comparison-v2.json'")
emit('run_cell_probe_v2.py',text)
text=(HERE/'report_managed_v3.py').read_text(encoding='utf-8').replace('cells-v3','cells-v4').replace('w2c5-validation-v1','w2c5-validation-v2').replace('managed-Debug-identities.json','managed-Debug-identities-v2.json').replace('managed-validation.json','managed-validation-v2.json')
# The prior C4 identity file retains its original filename.
text=text.replace("PREVIOUS/'managed-Debug-identities-v2.json'","PREVIOUS/'managed-Debug-identities.json'")
emit('report_managed_final.py',text)
text=(HERE/'report_validation_v2.py').read_text(encoding='utf-8').replace('w2c5-validation-v1','w2c5-validation-v2').replace("HERE/'managed-validation.json'","HERE/'managed-validation-v2.json'").replace("HERE/'managed-Debug-identities.json'","HERE/'managed-Debug-identities-v2.json'").replace("save('nonpasses.json'","save('nonpasses-v2.json'").replace("save('validation.json'","save('validation-v2.json'")
emit('report_validation_final.py',text)
