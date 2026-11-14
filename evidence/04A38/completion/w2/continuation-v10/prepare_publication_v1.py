from common import *
source=(PREVIOUS/'verify_c9_publication_v1.py').read_text(encoding='utf-8').replace("HERE.parent/'continuation-v8'","HERE.parent/'continuation-v9'")
source=source.replace('import verify_c8_publication_v4 as prior_publication','import verify_c9_publication_v1 as prior_publication').replace('publication_privacy_c9_v1','publication_privacy_c10_v1').replace('verify_c9_checkpoint_v1','verify_c10_checkpoint_v1').replace('verify_c9_preservation_v1','verify_c10_preservation_v1').replace('C9_PARTIAL','C10_PARTIAL')
with (HERE/'verify_c10_publication_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
source=(PREVIOUS/'publish_v3.py').read_text(encoding='utf-8').replace('from common_v2 import *','from common import *').replace('verify_c9_publication_v1','verify_c10_publication_v1')
begin=source.index("    previous=json.loads");end=source.index('    ordered=sorted(paths)')
source=source[:begin]+"""    previous=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes())
    paths={row['path'] for row in previous['entries']}
    paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
    paths.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v9-*') if p.is_file())
    paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
    paths.update((HERE/name).relative_to(ROOT).as_posix() for name in ['file-inventory-v1.json','publication-manifest-v1.json'])
    giant='evidence/04A38/completion/w2/continuation-v9/python-first-failures-v1.json'
    assert giant not in paths
"""+source[end:]
source=source.replace('C9','C10').replace('c9','c10').replace('continuation_v9','continuation_v10').replace('continuation-v9','continuation-v10')
# Above mechanical checkpoint substitutions must NOT move actual predecessor
# external paths or the protected historical giant exclusion to C10.
source=source.replace("W2.glob('continuation-v10-*')","W2.glob('continuation-v9-*')").replace("giant='evidence/04A38/completion/w2/continuation-v10/python-first-failures-v1.json'","giant='evidence/04A38/completion/w2/continuation-v9/python-first-failures-v1.json'")
source=source.replace("'C10 interrupted 4213129746-byte python-first-failures-v1.json, retained unchanged and pinned by failed-aggregation-attempt-v1.json'","'Protected C9 4213129746-byte failed aggregation excluded from review derivatives only; exact SHA and authoritative preservation in review-exclusions-v1.json. Never deleted or changed.'")
with (HERE/'publish_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('review-exclusions-v1.json',dict(review_derivative_only=True,excluded=[json.loads((HERE/'v9-private-entry-v1.json').read_bytes())['partial']],law='Exactly the independently pinned historical 4GB failed aggregation remains in the authoritative checkout and outside practical review derivatives; C9 already excluded it from its bounded standalone. All current critical source/evidence and historical bounded v2 nonpass records remain included. Private raw/TRX/cache/DLLs, all16 owner documents and ignored retained history require separate explicit live preservation; selected ZIPs prove neither live Git nor exhaustive ignored preservation.',original_queries=0))
save('publication-reader-crosswalk-v1.json',dict(reader=dict(before=pin(PREVIOUS/'verify_c9_publication_v1.py'),after=pin(HERE/'verify_c10_publication_v1.py')),publisher=dict(before=pin(PREVIOUS/'publish_v3.py'),after=pin(HERE/'publish_v1.py')),scope='C10 reader and exact C9 inherited path inventory plus all C10 critical source/evidence and append-only external C9 final artifacts. One null self-entry, unchanged privacy policy, deterministic physical ZIP/rebuild, copied executable, fresh SHA/CRC and actual mutation ZIP remain mandatory.'))
