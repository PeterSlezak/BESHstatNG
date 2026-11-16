from common45 import *
lock();source=load(HERE/'native-risk/build-attribution-v1.json');freeze=load(HERE/'matrix/source-freeze-v1.json')
source.update(current_compiler_source_manifest_sha256=sha(json.dumps(freeze['source_pins'],sort_keys=True).encode()),source_manifest_hash_definition='SHA256 over UTF-8 JSON sort_keys=True source-path-to-SHA256 mapping;not the Git tree object',main_git_tree_object=git('rev-parse',MAIN+'^{tree}').decode().strip(),reviewed_patch='evidence/04A45/source/review-v1.patch',entry_compiler_source_manifest_sha256=source.pop('source_tree_sha256'),production_source_exact_main=True,test_harness_is_uncommitted_source_delta=True,declared_build_date_source=pin('BESHstatNG/My Project/VersionAttributes.vb') if (ROOT/'BESHstatNG/My Project/VersionAttributes.vb').exists() else None)
for row in source['managed_builds']:
 c=row['configuration']
 for assembly in row['assemblies']:
  p=PRIVATE/'compiled'/c/'lib'/assembly['file_name'];assembly['observed_file_last_write_UTC']=datetime.datetime.fromtimestamp(p.stat().st_mtime,datetime.timezone.utc).isoformat()
 row['compiled_utc']='Not asserted from filesystem times;declared source version/build metadata and observed artifact times are distinct.'
save('native-risk/build-attribution-v2.json',source)
