from common import *
p='BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb';b=(ROOT/p).read_bytes();dst=HERE/'attempts/names-test-compiler-v1.vb.txt';dst.parent.mkdir(parents=True,exist_ok=True);dst.write_bytes(b)
s=b.decode('utf-8-sig').replace('Dim column As New UdfVariableNameRange38','Dim columnRange As New UdfVariableNameRange38').replace('Function() column))','Function() columnRange))');write_source(p,s)
save('attempts/names-test-compiler-v1.json',dict(status='TEST_COMPILE_FAILED',error='BC36641: lambda column parameter shadows later local column variable; rename only local fixture variable to columnRange. Original failed test source/command preserved; production unchanged.',source=pin(dst)))
