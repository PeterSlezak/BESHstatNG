from pathlib import Path
p=Path(__file__).with_name('owner-dossier-v2.py');s=p.read_text(encoding='utf-8')
old=r'(?:\d{2}|04A)\.\d{2})'
assert old in s;s=s.replace(old,r'(?:\d{2}|04A)\.\d{2}[A-Z]?)')
s=s.replace("official_batch_identities=226,identity_order_unchanged=True","official_batch_identities=226,source_row_mentions=len(new_ids),summary_repeated_mentions=len(new_ids)-226,identity_order_unchanged=True")
exec(compile(s,str(p),'exec'))
