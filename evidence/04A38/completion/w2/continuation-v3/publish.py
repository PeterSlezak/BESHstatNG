"""Additive review-copy manifest. Never edits any previous manifest."""
from common import *
from verify_publication import forbidden
version=sys.argv[1];name='publication-manifest-'+version+'.json';inventory='file-inventory-'+version+'.json'
paths={r['path'] for r in json.loads((PREVIOUS/'publication-manifest-v3.json').read_bytes())['entries']}
paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and not forbidden(p.relative_to(ROOT).as_posix()))
paths.update(r['path'] for r in json.loads((HERE/'text-call-sites.json').read_bytes())['sources'])
paths.update(r['path'] for r in json.loads((HERE/'source-transitions-final.json').read_bytes())['files'])
paths.update(['BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb','BESHStat.Excel.Common.Tests/src/UserFaultTests.vb'])
paths.update([(HERE/name).relative_to(ROOT).as_posix(),(HERE/inventory).relative_to(ROOT).as_posix()]);paths=sorted(p for p in paths if not forbidden(p))
save(inventory,dict(paths=paths,included='All safe C3 payloads/source/test/source-before/after/current replay/passive original metadata/adverse attempts; protected C2 predecessor publication plus original source fixture provenance',excluded=['C2 and C3 raw full-managed-Debug-v1.json: retained locally, diagnostic-only sanitized derivatives included','Every __pycache__/.pyc/.pyo','Live Git machinery/index/private machine logs/toolchain/package/current DLLs','Ignored retained TestResults history: not traversed','Other historical tracked/help/owner-doc/accepted-history files outside explicit paths'],review_boundary='Included hashes and physical ZIP inventory only; omitted payloads/live Git/index cannot be independently attested from derivative',self='Exactly one nonrecursive null self-entry in new manifest; predecessor manifests are ordinary hashed bytes'))
selfpath=(HERE/name).relative_to(ROOT).as_posix();rows=[dict(path=p,bytes=None,sha256=None) if p==selfpath else pin(ROOT/p) for p in paths]
save(name,dict(status='NOT_READY_04A38',version=version,inventory=inventory,entries=rows,readiness='readiness.json',scope='PARTIAL_W2_CONTINUATION_V3;NO_W3_READY_OR_OWNER_ACCEPTANCE'))
print(name,len(rows)-1,'hashed payloads plus one null self')
