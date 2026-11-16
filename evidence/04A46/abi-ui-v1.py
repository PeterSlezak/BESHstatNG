from common46 import *
lock();folder=PRIVATE/'ui-final-v1';folder.mkdir();exe=ROOT/'.test-rationalization-04A45.tmp/wiring-inspector-v2/InspectWiring45.exe';rows=[]
for c in ['Debug','Release']:
 lib=PRIVATE/'compiled'/c/'lib';out=folder/(c+'.json');r=command('UI-final-'+c+'-v1',[str(exe),str(lib),str(out)]);assert not r['exit_code']
 current=load(out);before=load(PRIVATE/'entry-builds'/c/'wiring.json');assert current['types']==before['types']
 proof=load(HERE/('abi-cil/'+c+'-proof-v1.json'));assert not proof['changed_methods'] and proof['exports']==268
 metadata=load(ROOT/proof['current_metadata']['path']);exports=load(ROOT/proof['current_exports']['path'])['exports']
 rows.append(dict(configuration=c,assembly=pin(lib/'BESHStatNG.dll'),managed_assemblies=metadata['assemblies'],MVID=current['mvid'],UI_Ribbon_RefEdit_fields_methods_IL_exact_pre_edit=True,types=len(current['types']),exports=268,export_item_digests=[dict(ordinal=i,sha256=sha(json.dumps(e,sort_keys=True,separators=(',',':')).encode())) for i,e in enumerate(exports)],raw=pin(out),baseline=pin(PRIVATE/'entry-builds'/c/'wiring.json'),product_method_changes=[]))
save('abi-cil/UI-and-attribution-v1.json',dict(rows=rows,main=MAIN,main_tree=git('rev-parse',MAIN+'^{tree}').decode().strip(),source_manifest_sha256=sha(json.dumps(load(HERE/'matrix/source-freeze-v1.json')['source_pins'],sort_keys=True).encode()),manifest_hash_kind='SHA256 JSON sort_keys source pin mapping;not Git tree object',effective_flags=pin('evidence/04A44/abi-cil/effective-registration-flags-v1.json'),all_export_items_and_CIL_exact45=True,XLL='NOT_BUILT_NOT_LOADED_NOT_AUTHORIZED',live_session='NOT_OBSERVED',actual_loaded_code='NOT_VERIFIED',native='NOT_RUN',gates=GATES))
print('UI/Ribbon/RefEdit CIL exact pre-edit;268 exports/config exact45;managed SHA/MVID recorded;actual native attribution NOT_VERIFIED',flush=True)
