from common import *
h = json.loads((PREVIOUS / 'historical-ownership.json').read_bytes())['members']
row = next(r for r in h if '4a1806f9c5baa4a2@99887:2213' in r['canonical_original_id'])
save('attempts/relocated-mapping.json',dict(status='RETAINED_RECONCILIATION_GUARD_FAILURE',member=row,
    cause='A historical helper already relocated entirely to Common has no live Windows identity; the v4 proof and its portable consumer must be carried rather than require a new Windows body'))
print(json.dumps(dict(name=row['name'],current_ids=row['current_ids'],proof=row['proof'])))
text = (HERE / 'reconcile.py').read_text(encoding='utf-8')
text = text.replace("save('independent-slice-proof.json', proof)", "assert json.loads((HERE / 'independent-slice-proof.json').read_bytes()) == proof")
text = text.replace("        assert ids, h['canonical_original_id']", "        assert ids or (h['proof'] and not h['current_ids']), h['canonical_original_id']")
with (HERE / 'reconcile_v2.py').open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(text)
