from common import *
path='BESHStat.Excel.Common/src/Analysis/UdfCellConversion38.vb'
src=ROOT/path
dst=HERE/'attempts/cells-v3-UdfCellConversion38.vb.txt'
dst.write_bytes(src.read_bytes())
text=src.read_text(encoding='utf-8')
text=text.replace('CellToTrimmedText(v As Object)','CellToTrimmedText(ByRef v As Object)').replace('ConvertFiniteNumber(v As Object)','ConvertFiniteNumber(ByRef v As Object)')
text=text.replace('Return CStr(v).Trim()','Return DirectCast(v, String).Trim()')
text=text.replace('Callers own exception handling.', 'Callers own exception handling. ByRef avoids an extra boxed-value copy; these methods never assign the input.')
src.write_text(text,encoding='utf-8',newline='\n')
save('attempts/conversion-il-refinement.json',dict(status='INTERMEDIATE_IMPLEMENTATION_RETAINED',before=pin(dst),after=pin(src),
    reason='Original native Convert calls clone boxed mutable values once with CLR GetObjectValue. A ByVal forwarding call plus portable Convert cloned twice. Passing the Windows local Object ByRef avoids the added copy; portable functions never assign the argument. DirectCast on an already verified string avoids the embedded VB Conversions helper.',
    original_queries=0,old_public_Windows_signatures='UNCHANGED',new_portable_API='New C5 methods only; no prior accepted API changed',
    intermediate_receipts='cells-v3 builds/tests/probes/ledgers remain immutable; final suffix-v2 receipts supersede them'))
