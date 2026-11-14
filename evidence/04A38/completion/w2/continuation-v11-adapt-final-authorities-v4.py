"""Version successors for additive complete publication; immutable earlier sources retained."""
from pathlib import Path
base=Path(__file__).resolve().parent
s=(base/'continuation-v11-retained-private-reader-v2.py').read_text()
s=s.replace('continuation-v11-retained-private-preservation-v2.json','continuation-v11-retained-private-preservation-v3.json')
s=s.replace('continuation-v11-review-parts-index-v1.json','continuation-v11-review-parts-index-v2.json')
with (base/'continuation-v11-retained-private-reader-v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(s)
s=(base/'continuation-v11-finalize-live-v3.py').read_text()
for a,b in [('continuation-v11-retained-private-preservation-v2.json','continuation-v11-retained-private-preservation-v3.json'),
            ('continuation-v11-retained-private-reader-v2.py','continuation-v11-retained-private-reader-v3.py'),
            ('continuation-v11-review-parts-index-v1.json','continuation-v11-review-parts-index-v2.json'),
            ('continuation-v11-publication-receipt-v1.json','continuation-v11-publication-receipt-v2.json'),
            ('continuation-v11-copied-review-reader-v1.json','continuation-v11-copied-review-reader-v2.json')]:s=s.replace(a,b)
s=s.replace("publication_reader=pin(HERE/'verify_c11_publication_v2.py'),", "publication_reader=pin(W2/'continuation-v11-publication-reader-v3.py'),sealed_publication_reader=pin(HERE/'verify_c11_publication_v2.py'),complete_manifest=pin(W2/'continuation-v11-logical-publication-manifest-v2.json'),copy_failure_reconciliation=pin(W2/'continuation-v11-copy-failure-reconciliation-v1.json'),publication_supplement_addendum=pin(W2/'continuation-v11-publication-supplement-addendum-v1.md'),")
s=s.replace('Authoritative executable readers are verify_c11_checkpoint_v2.py and verify_c11_publication_v2.py, under review-seal-v2.json.', 'Authoritative complete publication reader is continuation-v11-publication-reader-v3.py; it verifies the additive union and replays unchanged verify_c11_publication_v2.py / verify_c11_checkpoint_v2.py under review-seal-v2.json. The first copied default reader failed on omitted entry context; its actual exit1 and unchanged five ZIPs / first copy remain. The versioned sixth ACTIVE supplement supplies exactly three pinned required entry documents plus executable union authorities; optional excluded owner README documents remain omitted. The failed first supplement preparer confused optional owner docs with required context; its source and exact failure reproduction remain. The initial shell correction failed to parse before writes; the additive Python correction is retained. No production/test source or historical assertion changed for these publication corrections.')
s=s.replace('The earlier private reader-v1 is retained as a draft source and has no claimed result.', 'The earlier private reader-v1/v2 sources are retained drafts with no claimed result; v3 selects the complete six-part index.')
with (base/'continuation-v11-finalize-live-v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(s)
