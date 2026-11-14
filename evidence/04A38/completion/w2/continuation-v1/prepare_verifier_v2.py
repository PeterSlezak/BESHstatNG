from common import *
s=(HERE/'verify_p1.py').read_text()
s=s.replace("native_value=member if False else re.search(r'Friend Class '+family+'LsmEstimateProfileValue\\b([\\s\\S]*?)End Class',now)","native_value=re.search(r'(?:Private|Friend) Class '+family+r'LsmEstimateProfileValue\\b([\\s\\S]*?)End Class',now)")
s=s.replace("[r['owner'] for r in matrix if r['canonical_original_id']==p['original_id']]","[p['original_id'].split('|')[1]]")
with (HERE/'verify_p1_v2.py').open('x',encoding='utf-8') as f:f.write(s)
save('attempts/p1-independent-reader-v1.json',dict(exit_code=1,reader=pin(HERE/'verify_p1.py'),error='Native TValue initializer/constructor changed',diagnosis='Reader incorrectly required Friend on both DTO classes. Authenticated GEE is Private; MMRM is Friend. Neither class has a body other than inherited DTO. Corrected visibility regex retains exact constructor/initializer restriction. Canonical identity supplies the declaring type rather than a nonexistent owner field.',product_changed=False))
