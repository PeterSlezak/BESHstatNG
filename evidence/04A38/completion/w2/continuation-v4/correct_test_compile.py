from common import *
import shutil
p='BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb';src=ROOT/p;dest=HERE/'attempts/common-test-compile-v2.vb.txt';shutil.copyfile(src,dest)
s=src.read_text(encoding='utf-8-sig').replace('model.HeaderRows','model.HeaderTopRows').replace('model.LeftHeaderColumns','model.HeaderLeftColumns').replace('Dim single =','Dim selected =').replace('single.Count','selected.Count');write_source(p,s)
save('attempts/test-compile-v2.json',dict(status='COMPILER_ERROR_RETAINED',reason='New test used VB reserved keyword Single and two mistaken existing DTO property names; only new fixture corrected',preimage=pin(dest)))
