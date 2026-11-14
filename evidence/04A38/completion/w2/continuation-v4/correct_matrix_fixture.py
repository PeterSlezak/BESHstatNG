from common import *
import shutil
p='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';src=ROOT/p;dest=HERE/'attempts/native-test-compile-v3.vb.txt';shutil.copyfile(src,dest)
s=src.read_text(encoding='utf-8-sig');assert s.count('New Double(,)() {')==2;write_source(p,s.replace('New Double(,)() {','New List(Of Double(,)) From {'))
save('attempts/test-compile-v3.json',dict(status='FAILED_NEW_FIXTURE_COMPILE_RETAINED',reason='VB jagged/multidimensional array initializer spelling; replaced fixture containers with List(Of Double(,)), unchanged matrix values or assertions',preimage=pin(dest)))
